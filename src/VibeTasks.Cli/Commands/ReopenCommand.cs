using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;
using TaskStatus = VibeTasks.Core.TaskStatus;

namespace VibeTasks.Commands;

public sealed class ReopenCommand : Command<ReopenCommand.Settings>
{
    private readonly DataStore _store;
    public ReopenCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")] public string Id { get; set; } = "";
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var df = _store.LoadOrCreateToday();
        var t = df.Tasks.FirstOrDefault(x => x.Id.Equals(s.Id, StringComparison.OrdinalIgnoreCase));
        if (t is null) { AnsiConsole.MarkupLine($"[yellow]Not found:[/] {s.Id}"); return 1; }
        var old = t.Status;
        t.Status = TaskStatus.todo;
        t.UpdatedAt = DateTimeOffset.Now;
        t.CompletedDate = null;
        t.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "reopen", From = old.ToString(), To = t.Status.ToString() });
        _store.SaveDay(df, $"reopen {t.Id}");
        AnsiConsole.MarkupLine($"[green]Reopened[/] {t.Id}");
        return 0;
    }
}
