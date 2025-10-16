using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;
using VibeTaskStatus = VibeTasks.Core.VibeTaskStatus;

namespace VibeTasks.Commands;

public sealed class StatusCommand : Command<StatusCommand.Settings>
{
    private readonly DataStore _store;
    public StatusCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")] public string Id { get; set; } = "";
        [CommandArgument(1, "<status>")] public string Status { get; set; } = "";
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var df = _store.LoadOrCreateToday();
        var t = df.Tasks.FirstOrDefault(x => x.Id.Equals(s.Id, StringComparison.OrdinalIgnoreCase));
        if (t is null) { AnsiConsole.MarkupLine($"[yellow]Not found:[/] {s.Id}"); return 1; }
        if (!Enum.TryParse<VibeTaskStatus>(s.Status, true, out var newStatus))
        {
            AnsiConsole.MarkupLine("[red]Invalid status.[/] Use: todo, inprogress, blocked, skipped, complete");
            return 1;
        }
        var old = t.Status;
        t.Status = newStatus;
        t.UpdatedAt = DateTimeOffset.Now;
        if (newStatus == VibeTaskStatus.complete) t.CompletedDate = DateTime.Today;
        t.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "status", From = old.ToString(), To = newStatus.ToString() });
        _store.SaveDay(df, $"status {t.Id} {old}->{newStatus}");
        AnsiConsole.MarkupLine($"[green]Status[/] {t.Id}: {old} -> {newStatus}");
        return 0;
    }
}
