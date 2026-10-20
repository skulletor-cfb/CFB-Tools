using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace EA_DB_Editor
{
    public static class Big12Schedule
    {
        public const int Nebraska = 58;
        public const int KSU = 40;
        public const int KU = 39;
        public const int ISU = 38;
        public const int Cincy = 20;
        public const int Colorado = 22;

        public const int OU = 71;
        public const int OkSt = 72;
        public const int Baylor = 11;
        public const int Texas = 92;
        public const int TCU = 89;
        public const int TT = 94;

        private static bool initRun = false;

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[]
          {
            CreateNDY, CreateNDA,

            CreateNDZ, CreateNDY,
            CreateNDAPrime, CreateNDZ,
            CreateNDY, CreateNDAPrime,

            CreateNDZ, CreateNDY,
            CreateNDA, CreateNDZ,

          };

        public static Dictionary<int, HashSet<int>> Big12ConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
            var currYear = Form1.DynastyYear;

            switch (currYear)
            {
                default:
                    var idx = (Form1.DynastyYear - 2587) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(12, TableUtility.Big12Id, "Big12", expectedGames: 4, ifNotExpectedThen: 4);
            Big12ConferenceSchedule = result.BuildHashSet();
            return result;
        }

        public static void SwapHomeAwayTeam(this PreseasonScheduledGame game, MaddenRecord mr)
        {
            var realHomeTeam = game.AwayTeam;
            game.AwayTeam = game.HomeTeam;
            game.HomeTeam = realHomeTeam;
            mr["GATG"] = game.AwayTeam.ToString();
            mr["GHTG"] = game.HomeTeam.ToString();
        }

        public static void SetNewTeams(this PreseasonScheduledGame game, Dictionary<int, TeamSchedule> schedule, Dictionary<int, int[]> homeSchedules, int week, int a, int b)
        {
            game.HomeTeam = a;
            game.AwayTeam = b;
            game.MaddenRecord["GATG"] = game.AwayTeam.ToString();
            game.MaddenRecord["GHTG"] = game.HomeTeam.ToString();
            game.SetHomeTeam(homeSchedules);
            game.AssignGame(schedule, week);
        }

        public static void ProcessBig12Schedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(
                ScenarioForSeason,
                Big12ConferenceSchedule,
                TableUtility.Big12Id,
                TableUtility.Big12);
        }

        private static (PreseasonScheduledGame[], int) GetAllConferenceGames(this Dictionary<int, TeamSchedule> schedule, Dictionary<int, int[]> homeSchedules)
        {
            var result = new HashSet<PreseasonScheduledGame>();

            foreach (var kvp in homeSchedules)
            {
                foreach (var g in schedule[kvp.Key].GetAllConferenceGames())
                {
                    if (!result.Add(g))
                    {
                        Console.WriteLine(g.ToString());
                    }
                }
            }
            return (result.ToArray(), result.Count);

        }


        private static HashSet<Tuple<int, int>> CreateExpectedPairs(Dictionary<int, HashSet<int>> opponents)
        {
            var result = new HashSet<Tuple<int, int>>();

            foreach (var kvp in opponents)
            {
                foreach (var opp in kvp.Value)
                {
                    result.Add(CreateNeededMatchup(kvp.Key, opp));
                }
            }

            return result;
        }

        public static void ProcessSchedule(this Dictionary<int, TeamSchedule> schedule, Dictionary<int, int[]> homeSchedules, Dictionary<int, HashSet<int>> opponents, int confId, int[] conference, int? excludeTeam = null, int? avoidWeek = null)
        {
            var neededToSchedule = CreateExpectedPairs(opponents);

            // get all conference games - should be 54
            var (confGames, expectedGames) = schedule.GetAllConferenceGames(homeSchedules);
            int successfullyScheduleGames = 0;

            if (confGames.Length != expectedGames)
            {
                throw new Exception("Error reading schedule!");
            }

            var notScheduled = new List<PreseasonScheduledGame>();

            foreach (var game in confGames)
            {
                // set the home away properly
                if (game.GameOnSchedule(opponents))
                {
                    game.SetHomeTeam(homeSchedules);
                    successfullyScheduleGames++;

                    if (!neededToSchedule.Remove(CreateNeededMatchup(game.HomeTeam, game.AwayTeam)))
                    {
                        throw new InvalidOperationException("Expected a match up to be present!");
                    }
                }
                else
                {
                    notScheduled.Add(game);
                    schedule[game.AwayTeam][game.WeekIndex] = null;
                    schedule[game.HomeTeam][game.WeekIndex] = null;
                }
            }

            int idx = 0;

            // games that still need to be set
            foreach (var need in neededToSchedule)
            {
                int week = -1;
                if (!ConfScheduleFixer.FindCommonOpenWeek(schedule[need.Item1].FindOpenWeeks(avoidWeek), schedule[need.Item2].FindOpenWeeks(avoidWeek), out week))
                {
                    week = 14;
                }

                notScheduled[idx].SetNewTeams(schedule, homeSchedules, week, need.Item1, need.Item2);
                idx++;
            }
        }

        public static List<List<Tuple<int, int>>> CreatePairs(int[] a, int[] b)
        {
            var result = new List<List<Tuple<int, int>>>();
            return result;
        }

        public static List<List<Tuple<int, int>>> AllPairs = new List<List<Tuple<int, int>>>();

        public static void MakePairs(int[] arr, int start, List<Tuple<int, int>> current)
        {
            if (start >= arr.Length)
            {
                AllPairs.Add(current);
            }
            else
            {
                for (int i = start + 1; i < arr.Length; i++)
                {
                    Swap(arr, start + 1, i);
                    var next = current.ToList();
                    next.Add(new Tuple<int, int>(arr[start], arr[start + 1]));
                    MakePairs(arr, start + 2, next);
                    Swap(arr, start + 1, i);
                }
            }
        }

        public static void Swap(int[] arr, int i, int j)
        {
            var t = arr[i];
            arr[i] = arr[j];
            arr[j] = t;
        }


        public static Tuple<int, int> CreateNeededMatchup(int a, int b)
        {
            return a < b ? new Tuple<int, int>(a, b) : new Tuple<int, int>(b, a);
        }

        public static void SetHomeTeam(this PreseasonScheduledGame game, Dictionary<int, int[]> homeSchedules)
        {
            // even year means we follow what the home schedule says
            if (Form1.IsEvenYear.Value)
            {
                // we must flip if home team is incorrect
                if (!homeSchedules[game.HomeTeam].Contains(game.AwayTeam))
                {
                    game.SwapHomeAwayTeam(game.MaddenRecord);
                }
            }
            else
            {
                // we must flip if home team is incorrect
                if (homeSchedules[game.HomeTeam].Contains(game.AwayTeam))
                {
                    game.SwapHomeAwayTeam(game.MaddenRecord);
                }
            }
        }

        public static bool GameOnSchedule(this PreseasonScheduledGame game, Dictionary<int, HashSet<int>> opponents)
        {
            return opponents[game.HomeTeam].Contains(game.AwayTeam) &&
                opponents[game.AwayTeam].Contains(game.HomeTeam);
        }

        public static Dictionary<int, int[]> CreateNDY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(Nebraska, ISU, TCU, TT),
                OU.Create(Texas, OkSt, KU, Baylor),
                OkSt.Create(Texas, Colorado, KSU, TT),
                Colorado.Create(OU, Nebraska, ISU, Baylor),
                Nebraska.Create(OU, OkSt, KSU, Cincy),
                KU.Create(OkSt, Colorado, Nebraska, TCU),
                KSU.Create(Texas, KU, Cincy, Baylor),
                ISU.Create(OU, KSU, Cincy, TCU),
                Cincy.Create(OU, Colorado, KU, TT),
                TCU.Create(OkSt, Nebraska, Cincy, TT),
                Baylor.Create(Texas, KU, ISU, TCU),
                TT.Create(Colorado, KSU, ISU, Baylor),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(Colorado, KU, TCU, TT),
                OU.Create(Texas, OkSt, KSU, TCU),
                OkSt.Create(Colorado, Nebraska, ISU, TT),
                Colorado.Create(OU, Nebraska, KSU, Baylor),
                Nebraska.Create(OU, ISU, Cincy, Baylor),
                KU.Create(OkSt, Colorado, Nebraska, ISU),
                KSU.Create(Texas, KU, Cincy, TCU),
                ISU.Create(OU, KSU, Cincy, TT),
                Cincy.Create(Texas, OkSt, Baylor, TT),
                TCU.Create(Colorado, KU, ISU, Cincy),
                Baylor.Create(Texas, OkSt, KSU, TCU),
                TT.Create(OU, Nebraska, KU, Baylor),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDAPrime()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(OkSt, Colorado, KU, TT),
                OU.Create(Texas, OkSt, KU, TCU),
                OkSt.Create(Colorado, KSU, Cincy, Baylor),
                Colorado.Create(Nebraska, KSU, ISU, TT),
                Nebraska.Create(OU, ISU, TCU, Baylor),
                KU.Create(Nebraska, ISU, Cincy, TT),
                KSU.Create(OU, Nebraska, KU, TCU),
                ISU.Create(Texas, OkSt, KSU, Cincy),
                Cincy.Create(Texas, OU, Colorado, Baylor),
                TCU.Create(OkSt, Colorado, Cincy, TT),
                Baylor.Create(Texas, KU, ISU, TCU),
                TT.Create(OU, Nebraska, KSU, Baylor),

            }.Create();
        }


        public static Dictionary<int, int[]> CreateNDA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(Nebraska, ISU, Cincy, TT),
                OU.Create(Texas, OkSt, KU, Baylor),
                OkSt.Create(Colorado, KSU, TCU, TT),
                Colorado.Create(Texas, Nebraska, KSU, Cincy),
                Nebraska.Create(OU, KSU, ISU, TCU),
                KU.Create(Texas, Nebraska, ISU, Baylor),
                KSU.Create(OU, KU, TCU, TT),
                ISU.Create(OkSt, Colorado, KSU, Cincy),
                Cincy.Create(OU, OkSt, KU, Baylor),
                TCU.Create(OU, Colorado, Cincy, TT),
                Baylor.Create(Texas, OkSt, ISU, TCU),
                TT.Create(Colorado, Nebraska, KU, Baylor),

            }.Create();
        }
    }
}