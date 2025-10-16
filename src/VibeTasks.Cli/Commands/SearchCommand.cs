using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;
using VibeTasks.Core;
using VibeTaskStatus = VibeTasks.Core.VibeTaskStatus;

namespace VibeTasks.Commands;

public sealed class SearchCommand : Command<SearchCommand.Settings>
{
    private readonly SearchService _search;
    public SearchCommand(SearchService search) { _search = search; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[query]")] public string? Query { get; set; }
        [CommandOption("--regex")] public bool Regex { get; set; }
        [CommandOption("--fuzzy")] public bool Fuzzy { get; set; }
        [CommandOption("-t|--tag")] public string? Tags { get; set; }
        [CommandOption("--status")] public string? Status { get; set; }
        [CommandOption("--from")] public string? From { get; set; }
        [CommandOption("--to")] public string? To { get; set; }
        [CommandOption("--json")] public bool AsJson { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var tags = s.Tags is null ? Array.Empty<string>() : Utils.NormalizeTags(s.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries));
        VibeTaskStatus[]? statuses = null;
        if (!string.IsNullOrWhiteSpace(s.Status))
        {
            var parts = s.Status.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = new List<VibeTaskStatus>();
            foreach (var p in parts)
                if (Enum.TryParse<VibeTaskStatus>(p, true, out var st)) list.Add(st);
            statuses = list.ToArray();
        }
        DateTime? from = string.IsNullOrWhiteSpace(s.From) ? null : DateTime.Parse(s.From).Date;
        DateTime? to = string.IsNullOrWhiteSpace(s.To) ? null : DateTime.Parse(s.To).Date;

        var results = _search.Search(s.Query, s.Regex, s.Fuzzy, tags, statuses, from, to).ToList();

        if (s.AsJson)
        {
            var shaped = results.Select(r => new { date = r.date.ToString("yyyy-MM-dd"), task = r.task });
            var json = System.Text.Json.JsonSerializer.Serialize(shaped, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            return 0;
        }

        var table = new Table().Border(TableBorder.Rounded).AddColumns("Date", "ID", "Status", "Description", "Tags");
        foreach (var (date, task) in results)
        {
            var statusColor = task.Status == VibeTaskStatus.complete ? "green" :
                              task.Status == VibeTaskStatus.blocked ? "red" :
                              task.Status == VibeTaskStatus.inprogress ? "cyan" :
                              task.Status == VibeTaskStatus.skipped ? "grey" : "yellow";
            table.AddRow(date.ToString("yyyy-MM-dd"), $"[bold]{task.Id}[/]", $"[{statusColor}]{task.Status}[/]", task.Description, string.Join(", ", task.Tags.Select(x=>$"#{x}")));
        }
        AnsiConsole.Write(table);
        return 0;
    }
}
