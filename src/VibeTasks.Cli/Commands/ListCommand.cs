using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;
using VibeTasks.Core;
using TaskStatus = VibeTasks.Core.TaskStatus;

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
            if (s.Open) items = items.Where(t => t.Status != TaskStatus.complete);
            else if (s.Done) items = items.Where(t => t.Status == TaskStatus.complete);
        }

        if (s.AsJson)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(items, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            return 0;
        }

        var table = new Table().Border(TableBorder.Rounded).AddColumns("ID", "Status", "Description", "Tags", "Note");
        foreach (var t in items.OrderBy(x => x.CreatedAt))
        {
            var statusColor = t.Status switch
            {
                TaskStatus.todo => "yellow",
                TaskStatus.inprogress => "cyan",
                TaskStatus.blocked => "red",
                TaskStatus.skipped => "grey",
                TaskStatus.complete => "green",
                _ => "white"
            };
            table.AddRow($"[bold]{t.Id}[/]", $"[{statusColor}]{t.Status}[/]", t.Description, string.Join(", ", t.Tags.Select(x=>$"#{x}")), string.IsNullOrWhiteSpace(t.Note) ? "-" : (t.Note.Length>60 ? t.Note.Substring(0, 60)+"..." : t.Note));
        }
        AnsiConsole.Write(table);
        return 0;
    }
}
