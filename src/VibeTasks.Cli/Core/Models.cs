using System.Text.Json.Serialization;

namespace VibeTasks.Core;

public enum VibeTaskStatus { todo, inprogress, blocked, skipped, complete }

public class TaskHistoryEvent
{
    public DateTimeOffset Ts { get; set; }
    public string Op { get; set; } = "";
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Delta { get; set; }
}

public class TaskItem
{
    public string Id { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VibeTaskStatus Status { get; set; } = VibeTaskStatus.todo;
    public string Note { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    public DateTime FirstDate { get; set; } = DateTime.Today;
    public DateTime LastDate { get; set; } = DateTime.Today;
    public bool Archived { get; set; } = false;
    public DateTime? CompletedDate { get; set; }
    public string? CarriedOverFrom { get; set; }
    public List<TaskHistoryEvent> History { get; set; } = new();
}

public class DayFile
{
    public string Date { get; set; } = "";
    public string Timezone { get; set; } = "";
    public List<TaskItem> Tasks { get; set; } = new();
}
