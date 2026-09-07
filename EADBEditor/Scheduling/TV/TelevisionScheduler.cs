using EA_DB_Editor.Scheduling.TV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EA_DB_Editor.Scheduling
{
    public static class TelevisionScheduler
    {
        public static SeasonCalendar CurrentSeason = new SeasonCalendar(Form1.DynastyYear);

        public static void FixTelevisionSchedule()
        {
            var team = TableUtility.FindTable("TEAM").lRecords.ToDictionary(mr => mr.TeamId());
            var games = TableUtility.FindTable("SCHD").lRecords
                .Select(mr => new TelevisedGame(mr, team))
                .Where(g => g.GameNeedsAssignment())
                .GroupBy(g => g.ConferenceOwner)
                .ToDictionary(g => g.Key, g => g.ToList());

            // select the games
            CWNetwork.Instance.SelectGames(games);
            ESPNNetworks.Instance.SelectGames(games);
            CBSNetwork.Instance.SelectGames(games);
            NBCNetwork.Instance.SelectGames(games);
            FoxNetworks.Instance.SelectGames(games);

            // assign the games
            CWNetwork.Instance.AssignGames().Report();
            ESPNNetworks.Instance.AssignGames().Report();
            CBSNetwork.Instance.AssignGames().Report();
            NBCNetwork.Instance.AssignGames().Report();
            FoxNetworks.Instance.AssignGames().Report();
        }

        public static bool GameNeedsAssignment(this TelevisedGame game)
        {
            // labor day monday does not get assigned
            if (game.Week <= 2 && game.Day == 0)
            {
                game.PreAssigned();
                return false;
            }

            // Sundays before labor day do not get assigned
            // same with thur/fri as those are hand crafted
            if (game.Week <= 1 && game.Day != 5)
            {
                game.PreAssigned();
                return false;
            }

            // rocky mountain showdown will get assigned manually
            if ((game.HomeTeam == 22 && game.AwayTeam == 23) || (game.HomeTeam == 23 && game.AwayTeam == 22))
            {
                game.PreAssigned();
                return false;
            }

            return !game.Assigned;
        }

        public static void AssignGame(this Dictionary<TimeSlot, TelevisedGame> schedule, TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            schedule.AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        public static void AssignGame(this List<(TimeSlot time, TelevisedGame game)> schedule, TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            schedule.AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        public static void AssignGame(this Dictionary<TimeSlot, TelevisedGame> schedule, TelevisedGame game, TimeSlot timeslot)
        {
            schedule[timeslot] = game.Assign(timeslot);
        }

        public static void AssignGame(this List<(TimeSlot time, TelevisedGame game)> schedule, TelevisedGame game, TimeSlot timeslot)
        {
            schedule.Add((timeslot, game.Assign(timeslot)));
        }


        public static bool IsOctober(this int week)
        {
            return CurrentSeason.IsOctober(week);
        }

        public static bool IsAugustSeptember(this int week)
        {
            return CurrentSeason.IsAugustSeptember(week);
        }

        public static bool IsNovember(this int week)
        {
            return CurrentSeason.IsNovember(week);
        }
    }
}