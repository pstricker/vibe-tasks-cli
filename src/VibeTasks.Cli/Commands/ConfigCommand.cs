using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Commands;

public sealed class ConfigCommand : Command<ConfigCommand.Settings>
{
    private readonly AppConfig _cfg;
    public ConfigCommand(AppConfig cfg) { _cfg = cfg; }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--set")] public string? Set { get; set; }
        [CommandOption("--get")] public string? Get { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        if (!string.IsNullOrWhiteSpace(s.Set))
        {
            var kv = s.Set.Split('=', 2);
            if (kv.Length != 2) { AnsiConsole.MarkupLine("[red]Use --set key=value[/]"); return 1; }
            var key = kv[0].Trim(); var value = kv[1].Trim();
            switch (key)
            {
                case "dataDir": _cfg.DataDir = value; break;
                case "timezone": _cfg.Timezone = value; break;
                case "gitAutoCommit": _cfg.GitAutoCommit = bool.Parse(value); break;
                default: AnsiConsole.MarkupLine("[yellow]Unknown key[/]"); return 1;
            }
            _cfg.Save();
            AnsiConsole.MarkupLine("[green]Saved[/]");
            return 0;
        }
        if (!string.IsNullOrWhiteSpace(s.Get))
        {
            var key = s.Get.Trim();
            var val = key switch
            {
                "dataDir" => _cfg.DataDir,
                "timezone" => _cfg.Timezone,
                "gitAutoCommit" => _cfg.GitAutoCommit.ToString(),
                _ => "(unknown key)"
            };
            Console.WriteLine(val);
            return 0;
        }

        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(_cfg, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
}
