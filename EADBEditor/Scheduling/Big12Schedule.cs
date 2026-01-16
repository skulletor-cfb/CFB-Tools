using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public static class Big12Schedule
    {
        public const int Nebraska = 58;
        public const int KSU = 40;
        public const int KU = 39;
        public const int ISU = 38;
        public const int Cincy = 20;
        public const int BSUId = 12;
        public const int BSU = BSUId;
        public const int Colorado = 22;

        public const int OU = 71;
        public const int OkSt = 72;
        public const int UCF = 18;
        public const int Baylor = 11;
        public const int Texas = 92;
        public const int TCU = 89;
        public const int TT = 94;
        public const int HOU = 33;
        public const int SMU = 83;
        public const int USF = 144;

        private static bool initRun = false;

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[]
        {
            Create15A, Create15A,
            Create15B, Create15B,
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
                    var idx = (Form1.DynastyYear - 2564) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(15, RecruitingFixup.Big12Id, "Big12");
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
                RecruitingFixup.Big12Id,
                RecruitingFixup.Big12);
        }

        private static (PreseasonScheduledGame[], int) GetAllConferenceGames(this Dictionary<int, TeamSchedule> schedule, Dictionary<int, int[]> homeSchedules)
        {
            int games = 0;
            var result = new List<PreseasonScheduledGame>();

            foreach (var kvp in homeSchedules)
            {
                result.AddRange(schedule[kvp.Key].GetAllConferenceGames());
                games += kvp.Value.Length;
            }

            return (result.Distinct().ToArray(), games);
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

        public static void ProcessSchedule(this Dictionary<int, TeamSchedule> schedule, Dictionary<int, int[]> homeSchedules, Dictionary<int, HashSet<int>> opponents, int confId, int[] conference, int? excludeTeam = null)
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
                if (!ConfScheduleFixer.FindCommonOpenWeek(schedule[need.Item1].FindOpenWeeks(), schedule[need.Item2].FindOpenWeeks(), out week))
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



        // big 12 has 15 teams, no divisions, smu, ucf, hou are in it
        public static Dictionary<int, int[]> Create15A()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                OU.Create(Texas, Baylor, UCF, OkSt),
                Texas.Create(TT, HOU, Cincy, KSU),
                TT.Create(Baylor, SMU, UCF, ISU),
                Baylor.Create(TCU, UCF, ISU, Colorado),
                TCU.Create(Texas, SMU, Cincy, KU),
                SMU.Create(OU, Baylor, HOU, ISU),
                HOU.Create(TCU, Cincy, Nebraska, OkSt),
                UCF.Create(SMU, HOU, KSU, Colorado),
                Cincy.Create(UCF, KU, Colorado, OkSt),
                ISU.Create(OU, Cincy, KSU, Nebraska),
                KSU.Create(Baylor, SMU, KU, Nebraska),
                KU.Create(OU, TT, HOU, Colorado),
                Nebraska.Create(OU, TT, TCU, KU),
                Colorado.Create(Texas, ISU, Nebraska, OkSt),
                OkSt.Create(Texas, TT, TCU, KSU),
            }.Create();
        }

        public static Dictionary<int, int[]> Create15B()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                OU.Create(Texas, TT, TCU, OkSt),
                Texas.Create(TT, SMU, ISU, KU),
                TT.Create(Baylor, HOU, KSU, Colorado),
                Baylor.Create(Texas, TCU, Cincy, Nebraska),
                TCU.Create(TT, SMU, UCF, ISU),
                SMU.Create(HOU, Cincy, Nebraska, Colorado),
                HOU.Create(OU, Baylor, KSU, Colorado),
                UCF.Create(Texas, HOU, Nebraska, OkSt ),
                Cincy.Create(OU, TT, UCF, ISU),
                ISU.Create(HOU, UCF, KSU, OkSt),
                KSU.Create(OU, TCU , Cincy, KU),
                KU.Create(Baylor, SMU, UCF, ISU),
                Nebraska.Create(Texas, Cincy, KU, OkSt),
                Colorado.Create(OU, TCU ,KSU, Nebraska),
                OkSt.Create(Baylor, SMU, KU, Colorado),
            }.Create();
        }
    }
}