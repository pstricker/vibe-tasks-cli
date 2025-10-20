using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;
using VibeTaskStatus = VibeTasks.Core.VibeTaskStatus;

namespace VibeTasks.Commands;

public sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly DataStore _store;
    public ListCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--day")] public string? Day { get; set; }
        [CommandOption("--open")] public bool Open { get; set; }
        [CommandOption("--done")] public bool Done { get; set; }
        [CommandOption("--all")] public bool All { get; set; }
        [CommandOption("-t|--tag")] public string? Tag { get; set; }
        [CommandOption("--json")] public bool AsJson { get; set; }
        [CommandOption("--showdates")] public bool ShowDates { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var date = DateTime.Now.Date;
        if (!string.IsNullOrWhiteSpace(s.Day))
        {
            date = s.Day switch
            {
                "today" => DateTime.Now.Date,
                "yesterday" => DateTime.Now.Date.AddDays(-1),
                _ => DateTime.Parse(s.Day).Date
            };
        }

        var df = _store.LoadDay(date);
        IEnumerable<TaskItem> items = df.Tasks;
        if (!string.IsNullOrWhiteSpace(s.Tag))
        {
            var tag = s.Tag.Trim().ToLowerInvariant();
            items = items.Where(t => t.Tags.Contains(tag));
        }
        if (!s.All)
        {
            if (s.Open) items = items.Where(t => t.Status != VibeTaskStatus.complete);
            else if (s.Done) items = items.Where(t => t.Status == VibeTaskStatus.complete);
        }

        if (s.AsJson)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            return 0;
        }

        Table table;
        if (s.ShowDates)
        {
            table = new Table().Border(TableBorder.Rounded)
                .AddColumns("ID", "Status", "Description", "Tags", "Note", "Date", "UpdatedAt");
        }
        else
        {
            table = new Table().Border(TableBorder.Rounded)
                .AddColumns("ID", "Status", "Description", "Tags", "Note");
        }

        foreach (var t in items.OrderBy(x => x.CreatedAt))
        {
            var statusColor = t.Status switch
            {
                VibeTaskStatus.todo => "yellow",
                VibeTaskStatus.inprogress => "cyan",
                VibeTaskStatus.blocked => "red",
                VibeTaskStatus.skipped => "grey",
                VibeTaskStatus.complete => "green",
                _ => "white"
            };
            if (s.ShowDates)
            {
                table.AddRow(
                    $"[bold]{t.Id}[/]",
                    $"[{statusColor}]{t.Status}[/]",
                    t.Description,
                    string.Join(", ", t.Tags.Select(x => $"#{x}")),
                    string.IsNullOrWhiteSpace(t.Note) ? "-" : t.Note,
                    t.CreatedAt.ToString("yyyy-MM-dd"),
                    t.UpdatedAt.ToString("yyyy-MM-dd HH:mm")
                );
            }
            else
            {
                table.AddRow(
                    $"[bold]{t.Id}[/]",
                    $"[{statusColor}]{t.Status}[/]",
                    t.Description,
                    string.Join(", ", t.Tags.Select(x => $"#{x}")),
                    string.IsNullOrWhiteSpace(t.Note) ? "-" : t.Note
                );
            }
        }
        AnsiConsole.Write(table);
        return 0;
    }
}
