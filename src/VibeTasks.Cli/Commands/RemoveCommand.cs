using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Commands;

public sealed class RemoveCommand : Command<RemoveCommand.Settings>
{
    private readonly DataStore _store;
    public RemoveCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")] public string Id { get; set; } = "";
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var df = _store.LoadOrCreateToday();
        var idx = df.Tasks.FindIndex(t => t.Id.Equals(s.Id, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) { AnsiConsole.MarkupLine($"[yellow]Not found:[/] {s.Id}"); return 1; }
        var t = df.Tasks[idx];
        df.Tasks.RemoveAt(idx);
        t.History.Add(new TaskHistoryEvent{ Ts = DateTimeOffset.Now, Op = "remove" });
        _store.SaveDay(df, $"remove {t.Id}");
        AnsiConsole.MarkupLine($"[red]Removed[/] {t.Id}: {t.Description} (from today only)");
        return 0;
    }
}
