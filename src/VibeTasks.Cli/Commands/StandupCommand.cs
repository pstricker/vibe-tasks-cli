using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;
using VibeTaskStatus = VibeTasks.Core.VibeTaskStatus;

namespace VibeTasks.Commands;

public sealed class StandupCommand : Command<StandupCommand.Settings>
{
    private readonly DataStore _store;
    public StandupCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--date")] public string? Date { get; set; }
        [CommandOption("--clipboard")] public bool Clipboard { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var today = string.IsNullOrWhiteSpace(s.Date) ? DateTime.Now.Date : DateTime.Parse(s.Date).Date;
        var yesterday = today.AddDays(-1);

        var todayDf = _store.LoadDay(today);
        var yestDf = _store.LoadDay(yesterday);

        var yesterdayItems = yestDf.Tasks.Where(t => t.History.Any(h =>
            h.Ts.Date == yesterday && (h.Op == "add" || h.Op == "status" || h.Op == "note" || h.Op == "edit" || h.Op == "archive"))).ToList();

        var todayOpen = todayDf.Tasks.Where(t => t.Status != VibeTaskStatus.complete).ToList();

        var lines = new List<string> { "Yesterday:" };
        lines.AddRange(!yesterdayItems.Any() ? new[] { "- (none)" } : yesterdayItems.Select(t => "- " + t.Description));
        lines.Add("Today:");
        lines.AddRange(todayOpen.Count == 0 ? new[] { "- (none)" } : todayOpen.Select(t => "- " + t.Description));
        var text = string.Join(Environment.NewLine, lines);
        Console.WriteLine(text);

        if (s.Clipboard)
        {
            try { TextCopy.ClipboardService.SetText(text); AnsiConsole.MarkupLine("[green]Copied to clipboard[/]"); }
            catch { AnsiConsole.MarkupLine("[yellow]Clipboard not available[/]"); }
        }
        return 0;
    }
}
