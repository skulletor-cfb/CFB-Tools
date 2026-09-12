using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor.Scheduling
{
    public enum StreamingProvider
    {
        None,
        ESPNPlus,
        FoxOne,
        ParamountPlus,
        Peacock,
        MWPlus,
        ACCNX,

    }
    public class StreamingSchedule : ITelelvisionProvider
    {
        public StreamingProvider Provider { get; }
        private List<(TimeSlot time, TelevisedGame game)> schedule = new List<(TimeSlot time, TelevisedGame game)>();
        public void AssignGame(TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        private StreamingSchedule(StreamingProvider provider)
        {
            this.Provider = provider;
        }

        public IEnumerable<(TimeSlot time, TelevisedGame game)> Games => this.schedule;

        public static StreamingSchedule Create(StreamingProvider provider)
        {
            return new StreamingSchedule(provider).Register();
        }

        public bool AssignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null || game.Assigned) return false;
            schedule.Add((timeslot, game.Assign(timeslot)));
            return true;
        }

        public string WriteReport()
        {
            var sb = new StringBuilder();

            foreach (var (time, game) in schedule.OrderBy(g => g.time.Week).ThenBy(g => g.time.Day).ThenBy(g => g.time.GTOD))
            {
                sb.AppendLine($"{time} - {game.AwayTeam} at {game.HomeTeam}");
            }

            return sb.ToString();
        }
    }
}