using EA_DB_Editor.Scheduling.TV;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

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

        private static Dictionary<ChannelName, ChannelSchedule> AllNetworks = new Dictionary<ChannelName, ChannelSchedule>();

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
            CBSNetwork.Instance.AssignGames();

            NBCNetwork.Instance.SelectGames(games);
            NBCNetwork.Instance.AssignGames();

            CBSSportsNetwork.Instance.SelectGames(games);
            CBSSportsNetwork.Instance.AssignGames();

            CWNetwork.Instance.SelectGames(games);
            CWNetwork.Instance.AssignGames();

            // espn and fox now select
            ESPNNetworks.Instance.SelectGames(games);
            FoxNetworks.Instance.SelectGames(games);

            // assign the games
            ESPNNetworks.Instance.AssignGames();
            FoxNetworks.Instance.AssignGames();

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
            var preassigner = new Func<TelevisedGame, bool>[]
                {
                    CWNetwork.Instance.PreassignGame,
                    ESPNNetworks.Instance.PreassignGame,
                    CBSNetwork.Instance.PreassignGame,
                    NBCNetwork.Instance.PreassignGame,
                    FoxNetworks.Instance.PreassignGame,
                    CBSSportsNetwork.Instance.PreassignGame,
                };

            return preassigner.All(f => f(game));
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
            foreach (var game in games.Where(g => !g.Assigned))
            {
                game.Deselect();
            }
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

        public static void Register(ChannelSchedule schedule) => AllNetworks[schedule.Name] = schedule;
    }
}