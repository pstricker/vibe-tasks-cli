using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Commands;

public sealed class EditCommand : Command<EditCommand.Settings>
{
    private readonly DataStore _store;
    public EditCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")] public string Id { get; set; } = "";
        [CommandOption("--desc")] public string? Description { get; set; }
        [CommandOption("--add-tag")] public string? AddTag { get; set; }
        [CommandOption("--remove-tag")] public string? RemoveTag { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var df = _store.LoadOrCreateToday();
        var t = df.Tasks.FirstOrDefault(x => x.Id.Equals(s.Id, StringComparison.OrdinalIgnoreCase));
        if (t is null) { AnsiConsole.MarkupLine($"[yellow]Not found:[/] {s.Id}"); return 1; }
        if (!string.IsNullOrWhiteSpace(s.Description)) t.Description = s.Description;
        if (!string.IsNullOrWhiteSpace(s.AddTag))
        {
            var tags = Utils.NormalizeTags(new[] { s.AddTag });
            foreach (var tg in tags) if (!t.Tags.Contains(tg)) t.Tags.Add(tg);
        }
        if (!string.IsNullOrWhiteSpace(s.RemoveTag))
        {
            var tags = Utils.NormalizeTags(new[] { s.RemoveTag });
            t.Tags.RemoveAll(x => tags.Contains(x));
        }
        t.UpdatedAt = DateTimeOffset.Now;
        t.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "edit" });
        _store.SaveDay(df, $"edit {t.Id}");
        AnsiConsole.MarkupLine($"[green]Edited[/] {t.Id}");
        return 0;
    }
}
