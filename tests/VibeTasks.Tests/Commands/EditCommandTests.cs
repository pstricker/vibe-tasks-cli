using Spectre.Console.Cli;
using System.Collections;
using VibeTasks.Commands;
using VibeTasks.Core;
using Xunit;

namespace VibeTasks.Tests.Commands;

public class EditCommandTests
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
    public void EditCommand_EditsTaskSuccessfully()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_editcmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);
        var today = store.LoadOrCreateToday();
        today.Tasks.Add(new TaskItem { Id = "e1", Description = "Edit me", Tags = new() { "tag1" }, Status = VibeTasks.Core.VibeTaskStatus.todo, FirstDate = DateTime.Today, LastDate = DateTime.Today });
        store.SaveDay(today, "add for edit");
        var cmd = new EditCommand(store);
        var settings = new EditCommand.Settings { Id = "e1", Description = "Edited desc", AddTag = "tag2" };
        var context = new CommandContext(new DummyArgs(), "edit", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(0, result);
        var updated = store.LoadOrCreateToday().Tasks.Find(t => t.Id == "e1");
        Assert.Equal("Edited desc", updated.Description);
        Assert.Contains("tag2", updated.Tags);
    }
}
