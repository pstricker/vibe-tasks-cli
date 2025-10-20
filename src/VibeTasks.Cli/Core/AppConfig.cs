using System.Text.Json;

namespace VibeTasks.Core;

public class AppConfig
{
    public string DataDir { get; set; } = GetDefaultDataDir();
    public string Timezone { get; set; } = TimeZoneInfo.Local.Id;
    public bool Color { get; set; } = true;
    public bool GitAutoCommit { get; set; } = true;
    public bool UseSqliteIndex { get; set; } = true;

    public static string GetDefaultDataDir()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".vibe-tasks");
    }

    public static string GetConfigPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".vibe-tasks", "config.json");
    }

    public static AppConfig Load()
    {
        var path = GetConfigPath();
        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch { }
        var cfg = new AppConfig();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
        return cfg;
    }

    public void Save()
    {
        var path = GetConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
