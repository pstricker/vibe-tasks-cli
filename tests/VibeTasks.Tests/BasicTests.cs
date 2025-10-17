using VibeTasks.Cli.Services;
using VibeTasks.Core;
using Xunit;
using VibeTaskStatus = VibeTasks.Core.VibeTaskStatus;

namespace VibeTasks.Tests;

public class BasicTests
{
    [Fact]
    public void AddAndListRoundtrip()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);
        var today = store.LoadOrCreateToday();
        today.Tasks.Clear();
        store.SaveDay(today, "reset");

        var df = store.LoadOrCreateToday();
        df.Tasks.Add(new TaskItem { Id = "abcd", Description = "Test task", FirstDate = DateTime.Today, LastDate = DateTime.Today });
        store.SaveDay(df, "add test");

        var again = store.LoadOrCreateToday();
        Assert.Contains(again.Tasks, t => t.Description == "Test task");
    }

    [Fact]
    public void RollForwardCarriesOpen()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "vibe_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        var cfg = new AppConfig { DataDir = tmp, GitAutoCommit = false };
        var store = new DataStore(cfg);

        var yesterday = DateTime.Now.Date.AddDays(-1);
        var ydf = new DayFile { Date = yesterday.ToString("yyyy-MM-dd"), Timezone = cfg.Timezone, Tasks = new() };
        ydf.Tasks.Add(new TaskItem { Id = "x1", Description = "Carry me", Status = VibeTaskStatus.inprogress, FirstDate = yesterday, LastDate = yesterday });
        store.SaveDay(ydf, "prepare yesterday");

        var roller = new RollForwardService(store);
        roller.PerformIfNeeded();

        var today = store.LoadOrCreateToday();
        Assert.Contains(today.Tasks, t => t.Id == "x1");
    }
}
