using Xunit;
using VibeTasks.Core;
using VibeTasks.Commands;
using Spectre.Console.Cli;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VibeTasks.Tests.Commands;

public class ListCommandTests
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
    public void ListCommand_ListsTasksSuccessfully()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_listcmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);
        var today = store.LoadOrCreateToday();
        today.Tasks.Add(new TaskItem { Id = "t1", Description = "List me", Tags = new() { "tag1" }, Status = VibeTasks.Core.VibeTaskStatus.todo, FirstDate = DateTime.Today, LastDate = DateTime.Today });
        store.SaveDay(today, "add for list");
        var cmd = new ListCommand(store);
        var settings = new ListCommand.Settings { Day = "today", Tag = "tag1" };
        var context = new CommandContext(new DummyArgs(), "list", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(0, result);
    }
}
