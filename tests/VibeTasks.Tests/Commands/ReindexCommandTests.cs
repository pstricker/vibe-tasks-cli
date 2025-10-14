using Xunit;
using VibeTasks.Core;
using VibeTasks.Commands;
using Spectre.Console.Cli;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VibeTasks.Tests.Commands;

public class ReindexCommandTests
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
    public void ReindexCommand_ExecutesSuccessfully_WhenEnabled()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_reindexcmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false, UseSqliteIndex = true };
        var store = new DataStore(cfg);
        var cmd = new ReindexCommand(store, cfg);
        var settings = new ReindexCommand.Settings();
        var context = new CommandContext(new DummyArgs(), "reindex", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(0, result);
    }

    [Fact]
    public void ReindexCommand_Fails_WhenDisabled()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_reindexcmd_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false, UseSqliteIndex = false };
        var store = new DataStore(cfg);
        var cmd = new ReindexCommand(store, cfg);
        var settings = new ReindexCommand.Settings();
        var context = new CommandContext(new DummyArgs(), "reindex", null);
        var result = cmd.Execute(context, settings);
        Assert.Equal(1, result);
    }
}
