using EA_DB_Editor.Scheduling.TV;
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

    }

    public class ChannelSchedule
    {
        private Dictionary<TimeSlot, TelevisedGame> schedule = new Dictionary<TimeSlot, TelevisedGame>();

        public ChannelName Name { get; }
        public ChannelSchedule(ChannelName name)
        {
            Name = name;
        }

        public bool AssignGame(TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            return AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        /// <summary>
        /// will try to assign the game, if unable to , will return the game so it can be requeued
        /// </summary>
        /// <param name="game"></param>
        /// <param name="timeslot"></param>
        /// <returns></returns>
        public bool AssignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null || schedule.ContainsKey(timeslot))
            {
                return false;
            }

            schedule[timeslot] = game.Assign(timeslot);
            return true;
        }

        public void PreassignGame(TelevisedGame game, TimeSlot timeslot, bool setGameTime = false)
        {
            schedule[timeslot] = game.PreAssign(setGameTime ? timeslot : null);
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