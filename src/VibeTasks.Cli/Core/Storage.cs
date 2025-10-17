using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Globalization;

namespace VibeTasks.Core;

public class DataStore
{
    private readonly AppConfig _cfg;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SqliteIndex _index;

    public DataStore(AppConfig cfg)
    {
        _cfg = cfg;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
        _index = new SqliteIndex(cfg);
    }

    public string GetDayPath(DateTime date) => Path.Combine(_cfg.DataDir, date.ToString("yyyy-MM-dd") + ".json");
    public IEnumerable<string> EnumerateDayFiles()
    => Directory.EnumerateFiles(_cfg.DataDir, "*.json")
        .Where(f => Regex.IsMatch(Path.GetFileName(f), @"^\d{4}-\d{2}-\d{2}\.json$"))
        .OrderBy(x => x);

    public DayFile LoadDay(DateTime date)
    {
        var path = GetDayPath(date);
        if (!File.Exists(path))
            return new DayFile { Date = date.ToString("yyyy-MM-dd"), Timezone = _cfg.Timezone, Tasks = new() };
        var json = File.ReadAllText(path);
        return System.Text.Json.JsonSerializer.Deserialize<DayFile>(json, _jsonOptions) ?? new DayFile { Date = date.ToString("yyyy-MM-dd"), Timezone = _cfg.Timezone, Tasks = new() };
    }

    public void SaveDay(DayFile df, string message = "update")
    {
        var path = Path.Combine(_cfg.DataDir, df.Date + ".json");
        Directory.CreateDirectory(_cfg.DataDir);
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(df, _jsonOptions));

        if (_cfg.UseSqliteIndex)
        {
            try { _index.ReplaceDay(df); } catch { /* ignore indexing errors */ }
        }

        if (_cfg.GitAutoCommit)
        {
            var git = new GitIntegration(_cfg.DataDir);
            git.CommitAll($"chore: {message} ({df.Date})");
        }
    }

    public DayFile LoadOrCreateToday()
    {
        var today = DateTime.Now.Date;
        var df = LoadDay(today);
        df.Date = today.ToString("yyyy-MM-dd");
        df.Timezone = _cfg.Timezone;
        return df;
    }

    public (DateTime? date, DayFile? day) FindMostRecentPriorDay(DateTime beforeDate)
    {
        var files = EnumerateDayFiles().Where(f => Path.GetFileNameWithoutExtension(f).CompareTo(beforeDate.ToString("yyyy-MM-dd")) < 0).ToList();
        if (files.Count == 0) return (null, null);
        var last = files.Last();
        var date = DateTime.Parse(Path.GetFileNameWithoutExtension(last));
        var json = File.ReadAllText(last);
        var df = System.Text.Json.JsonSerializer.Deserialize<DayFile>(json, _jsonOptions);
        return (date, df);
    }

    public IEnumerable<DayFile> LoadAllDays()
{
    foreach (var file in EnumerateDayFiles())
    {
        var name = Path.GetFileNameWithoutExtension(file);
        if (DateTime.TryParseExact(name, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            yield return LoadDay(date);
        }
    }
}
}



public class GitIntegration
{
    private readonly string _dir;
    public GitIntegration(string dir) { _dir = dir; }

    public void CommitAll(string message)
    {
        try
        {
            if (!Directory.Exists(Path.Combine(_dir, ".git"))) RunGit("init");
            RunGit("add", "-A");
            RunGit("commit", "-m", message);
        }
        catch { /* ignore git failures */ }
    }

    private void RunGit(params string[] args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = _dir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        var p = System.Diagnostics.Process.Start(psi);
        p?.WaitForExit(3000);
    }
}
