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

    }
    public class StreamingSchedule
    {
        public StreamingProvider Provider { get; }
        private List<(TimeSlot time, TelevisedGame game)> schedule = new List<(TimeSlot time, TelevisedGame game)>();
        public void AssignGame(TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        public StreamingSchedule(StreamingProvider provider)
        {
            this.Provider = provider;
        }

        public void AssignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null) return;
            schedule.Add((timeslot, game.Assign(timeslot)));
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