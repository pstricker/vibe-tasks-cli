using Microsoft.Data.Sqlite;
using System.Text;

namespace VibeTasks.Core;

public class SqliteIndex
{
    private readonly AppConfig _cfg;
    public string DbPath => Path.Combine(_cfg.DataDir, "vibetasks-index.sqlite");

    public SqliteIndex(AppConfig cfg) { _cfg = cfg; }

    private SqliteConnection Open()
    {
        Directory.CreateDirectory(_cfg.DataDir);
        var conn = new SqliteConnection($"Data Source={DbPath}");
        conn.Open();
        return conn;
    }

    public void EnsureSchema()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        PRAGMA journal_mode=WAL;
        CREATE TABLE IF NOT EXISTS tasks (
            date TEXT NOT NULL,
            id TEXT NOT NULL,
            description TEXT NOT NULL,
            tags TEXT NOT NULL,
            status TEXT NOT NULL,
            archived INTEGER NOT NULL,
            updatedAt TEXT NOT NULL,
            PRIMARY KEY (date, id)
        );
        CREATE INDEX IF NOT EXISTS idx_tasks_status ON tasks(status);
        CREATE INDEX IF NOT EXISTS idx_tasks_archived ON tasks(archived);
        ";
        cmd.ExecuteNonQuery();

        try
        {
            using var cmdFts = conn.CreateCommand();
            cmdFts.CommandText = @"CREATE VIRTUAL TABLE IF NOT EXISTS tasks_fts USING fts5(description, tags, id UNINDEXED, date UNINDEXED, content='')";
            cmdFts.ExecuteNonQuery();
        }
        catch { /* ignore if FTS not available */ }
    }

    public void ReplaceDay(DayFile df)
    {
        EnsureSchema();
        using var conn = Open();
        using var tx = conn.BeginTransaction();

        using (var del = conn.CreateCommand())
        {
            del.Transaction = tx;
            del.CommandText = "DELETE FROM tasks WHERE date = $d";
            del.Parameters.AddWithValue("$d", df.Date);
            del.ExecuteNonQuery();
        }
        try
        {
            using var delF = conn.CreateCommand();
            delF.Transaction = tx;
            delF.CommandText = "DELETE FROM tasks_fts WHERE date = $d";
            delF.Parameters.AddWithValue("$d", df.Date);
            delF.ExecuteNonQuery();
        }
        catch { }

        foreach (var t in df.Tasks)
        {
            using var ins = conn.CreateCommand();
            ins.Transaction = tx;
            ins.CommandText = @"INSERT INTO tasks(date,id,description,tags,status,archived,updatedAt)
                                VALUES($date,$id,$desc,$tags,$status,$arch,$upd)";
            ins.Parameters.AddWithValue("$date", df.Date);
            ins.Parameters.AddWithValue("$id", t.Id);
            ins.Parameters.AddWithValue("$desc", t.Description);
            ins.Parameters.AddWithValue("$tags", string.Join(",", t.Tags));
            ins.Parameters.AddWithValue("$status", t.Status.ToString());
            ins.Parameters.AddWithValue("$arch", t.Archived ? 1 : 0);
            ins.Parameters.AddWithValue("$upd", t.UpdatedAt.ToString("o"));
            ins.ExecuteNonQuery();

            try
            {
                using var insF = conn.CreateCommand();
                insF.Transaction = tx;
                insF.CommandText = @"INSERT INTO tasks_fts(description,tags,id,date) VALUES($d,$t,$id,$date)";
                insF.Parameters.AddWithValue("$d", t.Description);
                insF.Parameters.AddWithValue("$t", string.Join(" ", t.Tags));
                insF.Parameters.AddWithValue("$id", t.Id);
                insF.Parameters.AddWithValue("$date", df.Date);
                insF.ExecuteNonQuery();
            }
            catch { }
        }

        tx.Commit();
    }

    public IEnumerable<(DateTime date, string id, string description, string tagsCsv, string status, int archived)> Query(
        string? query, string[] tags, string[] statuses, DateTime? from, DateTime? to)
    {
        EnsureSchema();
        using var conn = Open();

        // Build WHERE
        var clauses = new List<string>();
        var parms = new Dictionary<string, object?>();

        if (from.HasValue) { clauses.Add("date >= $from"); parms["$from"] = from.Value.ToString("yyyy-MM-dd"); }
        if (to.HasValue) { clauses.Add("date <= $to"); parms["$to"] = to.Value.ToString("yyyy-MM-dd"); }

        if (statuses.Length > 0)
        {
            var inlist = string.Join(",", statuses.Select((s, i) => $"$st{i}"));
            clauses.Add($"status IN ({inlist})");
            for (var i = 0; i < statuses.Length; i++) parms[$"$st{i}"] = statuses[i];
        }

        for (var i = 0; i < tags.Length; i++)
        {
            clauses.Add($"(','||lower(tags)||',') LIKE $tg{i}");
            parms[$"$tg{i}"] = $"%,{tags[i].ToLowerInvariant()}%,";
        }

        string where = clauses.Count > 0 ? ("WHERE " + string.Join(" AND ", clauses)) : "";

        // Prefer FTS when query provided and FTS table exists; else fallback to LIKE
        bool haveFts = false;
        using (var check = conn.CreateCommand())
        {
            check.CommandText = "SELECT 1 FROM sqlite_master WHERE type='table' AND name='tasks_fts'";
            using var r = check.ExecuteReader();
            haveFts = r.Read();
        }

        string sql;
        if (!string.IsNullOrWhiteSpace(query) && haveFts)
        {
            // Prefix search by default
            var q = query!.Replace("\"", "\"\"");
            parms["$q"] = q + "*";

            var extra = clauses.Count > 0 ? " AND " + string.Join(" AND ", clauses) : "";
            sql =
                "SELECT t.date,t.id,t.description,t.tags,t.status,t.archived " +
                "FROM tasks t JOIN tasks_fts f ON f.id=t.id AND f.date=t.date " +
                "WHERE f MATCH $q" + extra +
                " ORDER BY t.date ASC, t.id ASC";
        }
        else if (!string.IsNullOrWhiteSpace(query))
        {
            sql =
                $"SELECT date,id,description,tags,status,archived FROM tasks " +
                (clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) + " AND " : "WHERE ") +
                "description LIKE $qlike ORDER BY date ASC, id ASC";
            parms["$qlike"] = "%" + query + "%";
        }
        else
        {
            sql =
                $"SELECT date,id,description,tags,status,archived FROM tasks " +
                (clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) : "") +
                " ORDER BY date ASC, id ASC";
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var kv in parms) cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            yield return (
                DateTime.Parse(reader.GetString(0)),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetInt32(5)
            );
        }
    }
}
