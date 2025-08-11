using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
            CreateNDA, CreateNDZ,
            CreateNDY, CreateNDA,
            CreateNDZ, CreateNDY,

            CreateNDAPrime, CreateNDZ,
            CreateNDY, CreateNDAPrime,
            CreateNDZ, CreateNDY,
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

            var idx = (Form1.DynastyYear - 2546) % Creators.Length;

            if (idx == 5 || idx == 11)
            {
                MessageBox.Show("Is SMU still the right call???  Or time to evaluate?");
            }

            result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.Big12Id, "Big12");
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

        private static (PreseasonScheduledGame[],int) GetAllConferenceGames(this Dictionary<int, TeamSchedule> schedule, Dictionary<int, int[]> homeSchedules)
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

            foreach(var kvp in opponents)
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

            if(confGames.Length != expectedGames)
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


        public static Dictionary<int, int[]> CreateNDY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDAPrime()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }


        public static Dictionary<int, int[]> CreateNDA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                TT.Create(OU, SMU, Baylor, KU),
                Texas.Create(TT, OkSt, Colorado, KSU),
                OU.Create(Texas, OkSt, TCU, ISU),
                OkSt.Create(Colorado, Nebraska, SMU, KU),
                Colorado.Create(TT,OU, Nebraska, ISU),
                Nebraska.Create(OU, TCU, Baylor, KU),
                SMU.Create(Texas, Nebraska, Baylor, KSU),
                TCU.Create(TT, OkSt, SMU, KSU),
                Baylor.Create(Texas, Colorado, TCU, ISU),
                ISU.Create(Texas, Nebraska, TCU, KSU),
                KU.Create(OU, SMU, Baylor, ISU),
                KSU.Create(TT, OkSt, Colorado, KU),
            }.Create();
        }

        public static Dictionary<int, int[]> Create16A()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(TT, TCU, Colorado, ISU),
                TT.Create(Baylor, KU, Cincy, OU),
                Baylor.Create(Texas, TCU, UCF, KSU, Cincy),  // 1
                TCU.Create(SMU, USF, ISU, OkSt),
                SMU.Create(TT, HOU, UCF, KU, OU), // 2
                HOU.Create(Texas, Baylor, TCU, UCF, Nebraska), // 3
                UCF.Create(Texas, USF, KU, OkSt),
                USF.Create(TT, SMU, KSU, ISU, Cincy), // 4
                Colorado.Create(Baylor, SMU, UCF, Nebraska, KSU), // 5
                Nebraska.Create(TT, TCU, USF, OU),
                KU.Create(Texas, Colorado, Nebraska, ISU, OkSt),  // 6
                KSU.Create(TCU, HOU, KU, Cincy),
                ISU.Create(SMU, Nebraska, KSU, Cincy),
                Cincy.Create(HOU, UCF, Colorado, OU),
                OU.Create(Texas, Baylor, USF, KSU, OkSt),  // 7
                OkSt.Create(TT, HOU, Colorado, Nebraska, ISU),  // 8
            }.Create();
        }

        public static Dictionary<int, int[]> Create16B()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(TT, USF, Nebraska, OkSt),
                TT.Create(Baylor, TCU, Colorado, ISU),
                Baylor.Create(Texas, TCU, USF, ISU, OkSt),
                TCU.Create(SMU, Colorado, Cincy, OU),
                SMU.Create(Texas, Baylor, HOU, KSU),
                HOU.Create(TT, TCU, UCF, KU, ISU),
                UCF.Create(TT, TCU, USF, Nebraska, ISU),
                USF.Create(SMU, HOU, KU, Cincy, OkSt),
                Colorado.Create(HOU, USF, Nebraska, KSU, OU),
                Nebraska.Create(Baylor, SMU, KSU, OU),
                KU.Create(Baylor, TCU, Nebraska, ISU, Cincy),
                KSU.Create(Texas, TT, UCF, KU),
                ISU.Create(Colorado, KSU, Cincy, OU),
                Cincy.Create(Texas, SMU, UCF, Nebraska),
                OU.Create(Texas, HOU, UCF, KU, OkSt),
                OkSt.Create(TT, SMU, Colorado, KSU, Cincy),
            }.Create();
        }
    }
}