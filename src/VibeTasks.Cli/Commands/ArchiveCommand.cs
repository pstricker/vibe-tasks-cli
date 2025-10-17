using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Commands;

public sealed class ArchiveCommand : Command<ArchiveCommand.Settings>
{
    private readonly DataStore _store;
    public ArchiveCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")] public string Id { get; set; } = "";
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var df = _store.LoadOrCreateToday();
        var t = df.Tasks.FirstOrDefault(x => x.Id.Equals(s.Id, StringComparison.OrdinalIgnoreCase));
        if (t is null) { AnsiConsole.MarkupLine($"[yellow]Not found:[/] {s.Id}"); return 1; }
        t.Archived = true;
        t.UpdatedAt = DateTimeOffset.Now;
        t.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "archive" });
        _store.SaveDay(df, $"archive {t.Id}");
        AnsiConsole.MarkupLine($"[blue]Archived[/] {t.Id}: {t.Description}");
        return 0;
    }
}
