using Xunit;
using VibeTasks.Core;
using VibeTasks.Commands;
using Spectre.Console.Cli;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VibeTasks.Tests.Commands;

public class StandupCommandTests
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
    public void StandupCommand_ExecutesSuccessfully()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_standupcmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);
        var today = store.LoadOrCreateToday();
        today.Tasks.Add(new TaskItem { Id = "s1", Description = "Standup task", Status = VibeTasks.Core.VibeTaskStatus.todo, FirstDate = DateTime.Today, LastDate = DateTime.Today });
        store.SaveDay(today, "add for standup");
        var cmd = new StandupCommand(store);
        var settings = new StandupCommand.Settings { Date = DateTime.Today.ToString("yyyy-MM-dd") };
        var context = new CommandContext(new DummyArgs(), "standup", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(0, result);
    }
}
