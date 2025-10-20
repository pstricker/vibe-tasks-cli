using System.Text.RegularExpressions;
using VibeTasks.Core;

namespace VibeTasks.Services;

public class SearchService
{
    private readonly DataStore _store;
    private readonly SqliteIndex _index;
    private readonly AppConfig _cfg;

    public SearchService(DataStore store, AppConfig cfg)
    {
        _store = store;
        _cfg = cfg;
        _index = new SqliteIndex(cfg);
    }

    public IEnumerable<(DateTime date, TaskItem task)> Search(string? query, bool regex, bool fuzzy, string[] tags, VibeTaskStatus[]? statusFilter, DateTime? from, DateTime? to)
    {
        // If SQLite index enabled and not using regex/fuzzy, use SQL for speed.
        if (_cfg.UseSqliteIndex && !regex && !fuzzy)
        {
            var statuses = statusFilter?.Select(s => s.ToString()).ToArray() ?? Array.Empty<string>();
            foreach (var row in _index.Query(query, tags, statuses, from, to))
            {
                var task = new TaskItem
                {
                    Id = row.id,
                    Description = row.description,
                    Tags = row.tagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
                    Status = Enum.TryParse<VibeTaskStatus>(row.status, out var st) ? st : VibeTaskStatus.todo,
                    Archived = row.archived == 1
                };
                yield return (row.date, task);
            }
            yield break;
        }

        // Fallback to file scan
        var files = _store.EnumerateDayFiles();
        foreach (var file in files)
        {
            var date = DateTime.Parse(Path.GetFileNameWithoutExtension(file));
            if (from.HasValue && date < from.Value) continue;
            if (to.HasValue && date > to.Value) continue;

            var df = _store.LoadDay(date);
            foreach (var task in df.Tasks)
            {
                if (tags.Length > 0 && !tags.All(t => task.Tags.Contains(t))) continue;
                if (statusFilter is not null && statusFilter.Length > 0 && !statusFilter.Contains(task.Status)) continue;

                var ok = true;
                if (!string.IsNullOrWhiteSpace(query))
                {
                    if (regex) ok = Regex.IsMatch(task.Description, query!, RegexOptions.IgnoreCase);
                    else if (fuzzy) ok = Fuzzy.SimilarityScore(task.Description, query!) >= 70;
                    else ok = task.Description.Contains(query!, StringComparison.OrdinalIgnoreCase);
                }

                if (ok) yield return (date, task);
            }
        }
    }
}
