using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Commands;

public sealed class ReindexCommand : Command<ReindexCommand.Settings>
{
    private readonly DataStore _store;
    private readonly AppConfig _cfg;
    private readonly SqliteIndex _index;

    public ReindexCommand(DataStore store, AppConfig cfg)
    {
        _store = store;
        _cfg = cfg;
        _index = new SqliteIndex(cfg);
    }

    public sealed class Settings : CommandSettings { }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!_cfg.UseSqliteIndex)
        {
            AnsiConsole.MarkupLine("[yellow]UseSqliteIndex is disabled in config.[/] Enable it with: [grey]task config --set UseSqliteIndex=true[/]");
            return 1;
        }

        _index.EnsureSchema();
        var count = 0;
        foreach (var df in _store.LoadAllDays())
        {
            _index.ReplaceDay(df);
            count++;
        }
        AnsiConsole.MarkupLine($"[green]Reindexed[/] {count} day files into SQLite index.");
        return 0;
    }
}
