using Spectre.Console.Cli;
using System.Collections;
using VibeTasks.Commands;
using VibeTasks.Core;
using Xunit;

namespace VibeTasks.Tests.Commands;

public class NoteCommandTests
{
    private class DummyArgs : IRemainingArguments, IEnumerable<string>
    {
        public int Count => 0;
        public string this[int index] => string.Empty;
        ILookup<string, string?> IRemainingArguments.Parsed => new List<string>().ToLookup(x => x, x => (string?)x);
        IReadOnlyList<string> IRemainingArguments.Raw => new List<string>();
        public IEnumerator<string> GetEnumerator() => (new List<string>()).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void NoteCommand_SetsNoteSuccessfully()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_notecmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);
        var today = store.LoadOrCreateToday();
        today.Tasks.Add(new TaskItem { Id = "n1", Description = "Note me", Status = VibeTasks.Core.VibeTaskStatus.todo, FirstDate = DateTime.Today, LastDate = DateTime.Today });
        store.SaveDay(today, "add for note");
        var cmd = new NoteCommand(store);
        var settings = new NoteCommand.Settings { Id = "n1", Set = "This is a note" };
        var context = new CommandContext(new DummyArgs(), "note", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(0, result);
        var updated = store.LoadOrCreateToday().Tasks.Find(t => t.Id == "n1");
        Assert.Equal("This is a note", updated.Note);
    }
}
