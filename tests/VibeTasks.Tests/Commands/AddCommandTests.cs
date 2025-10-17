using Spectre.Console.Cli;
using System.Collections;
using VibeTasks.Commands;
using VibeTasks.Core;
using Xunit;

namespace VibeTasks.Tests.Commands;

public class AddCommandTests
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
    public void AddCommand_AddsTaskSuccessfully()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_addcmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);
        var cmd = new AddCommand(store);
        var settings = new AddCommand.Settings { Description = "Test add", Tags = "work,urgent", Note = "Important" };
        var context = new CommandContext(new DummyArgs(), "add", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(0, result);
        var today = store.LoadOrCreateToday();
        Assert.Contains(today.Tasks, t => t.Description == "Test add" && t.Note == "Important" && t.Tags.Contains("work") && t.Tags.Contains("urgent"));
    }
}
