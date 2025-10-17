using VibeTasks.Core;

namespace VibeTasks.Cli.Services
{
    public class RollForwardService
    {
        private readonly DataStore _store;
        public RollForwardService(DataStore store) { _store = store; }

        public void PerformIfNeeded()
        {
            var today = DateTime.Now.Date;
            var path = _store.GetDayPath(today);
            if (File.Exists(path)) return;

            var (prevDate, prevDay) = _store.FindMostRecentPriorDay(today);
            var todays = new DayFile { Date = today.ToString("yyyy-MM-dd"), Timezone = TimeZoneInfo.Local.Id, Tasks = new() };

            if (prevDay is null)
            {
                _store.SaveDay(todays, "create new day (no previous)");
                return;
            }

            var carry = prevDay.Tasks.Where(t => !t.Archived && t.Status != VibeTaskStatus.complete).ToList();

            if (carry.Count == 0)
            {
                var yesterday = prevDate!.Value.Date;
                carry = prevDay.Tasks.Where(t => !t.Archived && t.History.Any(h => h.Ts.Date == yesterday)).ToList();
            }

            foreach (var t in carry)
            {
                var copy = new TaskItem
                {
                    Id = t.Id,
                    Description = t.Description,
                    Tags = new List<string>(t.Tags),
                    Status = t.Status,
                    Note = t.Note,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = DateTimeOffset.Now,
                    FirstDate = t.FirstDate,
                    LastDate = today,
                    Archived = t.Archived,
                    CompletedDate = t.CompletedDate,
                    CarriedOverFrom = prevDate!.Value.ToString("yyyy-MM-dd"),
                    History = new List<TaskHistoryEvent>(t.History)
                };
                copy.History.Add(new TaskHistoryEvent { Ts = DateTimeOffset.Now, Op = "rollforward", From = prevDate!.Value.ToString("yyyy-MM-dd"), To = today.ToString("yyyy-MM-dd") });
                todays.Tasks.Add(copy);
            }

            _store.SaveDay(todays, "roll-forward");
        }
    }
}
