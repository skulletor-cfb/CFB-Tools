using EA_DB_Editor.Scheduling.TV;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EA_DB_Editor.Scheduling
{
    public static class TelevisionScheduler
    {
        private static SeasonCalendar currentSeason;
        public static SeasonCalendar CurrentSeason
        {
            get
            {
                if (currentSeason == null)
                {
                    currentSeason = new SeasonCalendar(Form1.DynastyYear);
                }

                return currentSeason;
            }
        }

        public static Dictionary<int, List<TelevisedGame>> AllGames = null;

        public static void FixTelevisionSchedule()
        {
            var team = TableUtility.FindTable("TEAM").lRecords.ToDictionary(mr => mr.TeamId());
            var games = AllGames = TableUtility.FindTable("SCHD").lRecords
                .Select(mr => new TelevisedGame(mr, team))
                .Where(g => g.GameNeedsAssignment())
                .GroupBy(g => g.ConferenceOwner)
                .ToDictionary(g => g.Key, g => g.ToList());

            // select the games
            CBSNetwork.Instance.SelectGames(games);
            NBCNetwork.Instance.SelectGames(games);
            CBSSportsNetwork.Instance.SelectGames(games);
            CWNetwork.Instance.SelectGames(games);
            ESPNNetworks.Instance.SelectGames(games);
            FoxNetworks.Instance.SelectGames(games);

            // assign the games
            CWNetwork.Instance.AssignGames();
            ESPNNetworks.Instance.AssignGames();
            CBSNetwork.Instance.AssignGames();
            NBCNetwork.Instance.AssignGames();
            FoxNetworks.Instance.AssignGames();
            CBSSportsNetwork.Instance.AssignGames();

            //report
            CWNetwork.Instance.Report();
            ESPNNetworks.Instance.Report();
            CBSNetwork.Instance.Report();
            NBCNetwork.Instance.Report();
            FoxNetworks.Instance.Report();
            CBSSportsNetwork.Instance.Report();

            var unassigned = games.Values.SelectMany(l => l).Where(g => g.Assigned == false).ToList();
            var json = JsonConvert.SerializeObject(
                new
                {
                    count = unassigned.Count,
                    unassigned,
                }, Formatting.Indented);
            File.WriteAllText("unassigned-games.txt", json);
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
            if (game.CheckMatchup(22, 23))
            {
                game.PreAssigned();
                return false;
            }

            // thanksgiving and black friday will be pre assigned
            if (game.Week == 13 && game.Day != 5)
            {
                game.PreAssigned();
                return false;
            }

            // Mayhem at MBS, Oyster Bowl, Johnny Majors Classic do not get reassigned
            if (game.GTOD == 733 || game.GTOD == 717 || game.GTOD == 737)
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
            if (game == null || schedule.ContainsKey(timeslot)) return;

            schedule[timeslot] = game.Assign(timeslot);
        }

        public static void AssignGame(this List<(TimeSlot time, TelevisedGame game)> schedule, TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null) return;
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

        public static void ReturnInventory(this List<TelevisedGame> games)
        {
            games.Where(g => !g.Assigned).ToList().ForEach(g => g.Deselect());
        }

        public static int LaborDayWeek()
        {
            return CurrentSeason.IsLaborDayWeekendFirstWeek ? 0 : 1;
        }

        public static DateTime GetDate(this TelevisedGame game)
        {
            return CurrentSeason.GetDate(game.Week, game.Day);
        }

        public static int LastWeekOfOctober()
        {
            for (int i = CurrentSeason.Weeks.Length - 1; i >= 0; i--)
            {
                if (CurrentSeason.IsOctober(i))
                {
                    return i;
                }
            }

            throw new Exception("Bad calendar");
        }

        public static int FirstWeekOfOctober()
        {
            for (int i = 0; i <= 13; i++)
            {
                if (CurrentSeason.IsOctober(i))
                {
                    return i;
                }
            }

            throw new Exception("Bad calendar");
        }
    }
}