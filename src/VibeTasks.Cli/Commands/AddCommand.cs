using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;
using TaskStatus = VibeTasks.Core.TaskStatus;

namespace VibeTasks.Commands;

public sealed class AddCommand : Command<AddCommand.Settings>
{
    private readonly DataStore _store;
    public AddCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<description>")] public string Description { get; set; } = "";
        [CommandOption("-t|--tag")] public string? Tags { get; set; }
        [CommandOption("--note")] public string? Note { get; set; }
        [CommandOption("--when")] public string? When { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var date = string.IsNullOrWhiteSpace(s.When) ? DateTime.Now.Date : DateTime.Parse(s.When).Date;
        var df = _store.LoadDay(date);
        df.Date = date.ToString("yyyy-MM-dd");

        var tags = s.Tags is null ? Array.Empty<string>() : Utils.NormalizeTags(s.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries));
        var item = new TaskItem
        {
            Id = Utils.ShortId(),
            Description = s.Description,
            Tags = tags.ToList(),
            Note = s.Note ?? "",
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now,
            FirstDate = date,
            LastDate = date,
            Status = TaskStatus.todo
        };
        item.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "add" });
        df.Tasks.Add(item);

        _store.SaveDay(df, $"add {item.Id}");
        AnsiConsole.MarkupLine($"[green]Added[/] {item.Id}: {item.Description}");
        return 0;
    }
}
