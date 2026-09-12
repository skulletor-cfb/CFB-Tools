using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor.Scheduling
{
    public enum ChannelName
    {
        ABC,
        ACCNetwork,
        BigTenNetwork,
        CBSSportsNetwork,
        CBS,
        CW,
        ESPN2,
        ESPN,
        ESPNU,
        FOX,
        FoxSports1,
        NBC,
        SECNetwork,
        TheMW,
        Peacock,
    }

    public interface ITelelvisionProvider
    {
        bool AssignGame(TelevisedGame game, TimeSlot timeslot);
    }

    public class ChannelSchedule : ITelelvisionProvider
    {
        private Dictionary<TimeSlot, TelevisedGame> schedule = new Dictionary<TimeSlot, TelevisedGame>();

        public ChannelName Name { get; }
        private ChannelSchedule(ChannelName name)
        {
            Name = name;
        }

        public static ChannelSchedule Create(ChannelName name)
        {
            var schedule = new ChannelSchedule(name);
            return schedule.Register();
        }

        public bool IsTimeslotAvailable(TimeSlot ts) => schedule.ContainsKey(ts) == false;

        public IEnumerable<(TimeSlot time, TelevisedGame game)> Games => this.schedule.Select(kvp => (kvp.Key, kvp.Value));

        public bool AssignGame(TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            return AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        /// <summary>
        /// will try to assign the game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="timeslot"></param>
        /// <returns></returns>
        public bool AssignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null || schedule.ContainsKey(timeslot) || game.Assigned)
            {
                return false;
            }

            schedule[timeslot] = game.Assign(timeslot);
            return true;
        }

        public bool PreassignGame(TelevisedGame game, TimeSlot timeslot, bool setGameTime)
        {
            if (schedule.ContainsKey(timeslot))
            {
                return false;
            }

            schedule[timeslot] = game.PreAssign(setGameTime ? timeslot : null);
            return game.Assigned;
        }

        public string WriteReport()
        {
            var sb = new StringBuilder();
            foreach (var kvp in schedule.OrderBy(k => k.Key.Week).ThenBy(k => k.Key.Day).ThenBy(k => k.Key.GTOD))
            {
                sb.AppendLine($"{kvp.Key.ToString()} - {kvp.Value?.AwayTeam} at {kvp.Value?.HomeTeam}");
            }

            return sb.ToString();
        }
    }
}