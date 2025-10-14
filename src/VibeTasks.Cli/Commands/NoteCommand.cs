using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Commands;

public sealed class NoteCommand : Command<NoteCommand.Settings>
{
    private readonly DataStore _store;
    public NoteCommand(DataStore store) { _store = store; }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")] public string Id { get; set; } = "";
        [CommandOption("--set")] public string? Set { get; set; }
        [CommandOption("--append")] public string? Append { get; set; }
        [CommandOption("--edit")] public bool Edit { get; set; }
    }

    public override int Execute(CommandContext context, Settings s)
    {
        var df = _store.LoadOrCreateToday();
        var t = df.Tasks.FirstOrDefault(x => x.Id.Equals(s.Id, StringComparison.OrdinalIgnoreCase));
        if (t is null) { AnsiConsole.MarkupLine($"[yellow]Not found:[/] {s.Id}"); return 1; }

        if (s.Edit)
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, t.Note ?? "");
            Utils.OpenEditorForFile(tmp);
            t.Note = File.ReadAllText(tmp);
            File.Delete(tmp);
        }
        else if (s.Set is not null)
        {
            t.Note = s.Set;
        }
        else if (s.Append is not null)
        {
            t.Note += (t.Note.EndsWith(Environment.NewLine) || string.IsNullOrEmpty(t.Note) ? "" : Environment.NewLine) + s.Append;
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Provide one of --set, --append, or --edit[/]");
            return 1;
        }

        t.UpdatedAt = DateTimeOffset.Now;
        t.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "note" });
        _store.SaveDay(df, $"note {t.Id}");
        AnsiConsole.MarkupLine($"[green]Updated note[/] for {t.Id}");
        return 0;
    }
}
