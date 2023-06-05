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

        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateC, CreateC, CreateD, CreateD, CreateE, CreateE, CreateA, CreateA, CreateB, CreateB };

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
            var idx = (Form1.DynastyYear - 2468) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(14, RecruitingFixup.Big12Id, "Big12");
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

        public static void SetNewTeams(this PreseasonScheduledGame game, Dictionary<int, PreseasonScheduledGame[]> schedule, Dictionary<int, int[]> homeSchedules, int week, int a, int b)
        {
            game.HomeTeam = a;
            game.AwayTeam = b;
            game.MaddenRecord["GATG"] = game.AwayTeam.ToString();
            game.MaddenRecord["GHTG"] = game.HomeTeam.ToString();
            game.SetHomeTeam(homeSchedules);
            game.AssignGame(schedule, week);
        }

        public static void ProcessBig12Schedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(
                ScenarioForSeason,
                Big12ConferenceSchedule,
                RecruitingFixup.Big12Id,
                RecruitingFixup.Big12);
        }


        public static void ProcessSchedule(this Dictionary<int, PreseasonScheduledGame[]> schedule, Dictionary<int, int[]> homeSchedules, Dictionary<int, HashSet<int>> opponents, int confId, int[] conference)
        {
            // get all conference games - should be 54
            var confGames = schedule.Where(kvp => homeSchedules.ContainsKey(kvp.Key)).SelectMany(kvp => kvp.Value).Where(g => g != null && g.IsConferenceGame()).Distinct().ToArray();
            var notScheduled = new List<PreseasonScheduledGame>();

            foreach (var game in confGames)
            {
                // set the home away properly
                if (game.GameOnSchedule(opponents))
                {
                    game.SetHomeTeam(homeSchedules);
                }
                else
                {
                    notScheduled.Add(game);
                    schedule[game.AwayTeam][game.WeekIndex] = null;
                    schedule[game.HomeTeam][game.WeekIndex] = null;
                }
            }

            // now we need to evaluate what's not scheduled
            var successfullyScheduled = schedule.Where(kvp => conference.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => new HashSet<int>(kvp.Value.Where(g => g != null && g.IsConferenceGame()).Select(g => g.OpponentId(kvp.Key))));
            HashSet<Tuple<int, int>> neededToSchedule = new HashSet<Tuple<int, int>>();

            // find the games that need to be scheduled
            foreach (var key in successfullyScheduled.Keys)
            {
                // already scheduled
                if (successfullyScheduled[key].Count == 8)
                    continue;

                var missing = opponents[key].Where(i => !successfullyScheduled[key].Contains(i)).ToArray();
                foreach (var m in missing)
                {
                    neededToSchedule.Add(CreateNeededMatchup(key, m));
                }
            }

            int idx = 0;

            // games that still need to be set
            foreach (var need in neededToSchedule)
            {
                int week = -1;
                if (!ConfScheduleFixer.FindCommonOpenWeek(schedule[need.Item1].FindOpenWeeks(), schedule[need.Item2].FindOpenWeeks(), out week))
                {
                    week = 0;
                }

                notScheduled[idx].SetNewTeams(schedule, homeSchedules, week, need.Item1, need.Item2);
                idx++;
            }

            var openings = schedule.Where(kvp => conference.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.FindOpenWeeks());

            AllPairs = new List<List<Tuple<int, int>>>();
            MakePairs(conference, 0, new List<Tuple<int, int>>());
            var allPairs = AllPairs;

            //var allPairs = CreatePairs(divA, divB);

            foreach (var pairSet in allPairs)
            {
                if (pairSet.All(p =>
                {
                    int week = -1;
                    return ConfScheduleFixer.FindCommonOpenWeek(openings[p.Item1], openings[p.Item2], out week);
                }))
                {
                    return;
                }
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

#if false
        public static Dictionary<int, int[]> CreateA()
        {
            var dict = new Dictionary<int, int[]>();

            dict.Add(Nebraska, OU, KSU, Baylor, SMU);
            dict.Add(OU,OkSt, KU, ISU, Texas);
            dict.Add(OkSt, Nebraska, KSU, Colorado, TT);
            dict.Add(KSU, OU, KU, Baylor, TCU);
            dict.Add(KU, OkSt, ISU, Colorado, Nebraska);
            dict.Add(ISU, OkSt, KSU, SMU, Nebraska);

            dict.Add(Colorado, Nebraska, Texas, TCU, TT);
            dict.Add(Texas, OkSt, KSU, TCU, TT);
            dict.Add(Baylor, ISU, Colorado, Texas, TCU);
            dict.Add(TCU, OU, KU, SMU, TT);
            dict.Add(SMU, KU, Colorado, Texas, Baylor);
            dict.Add(TT, OU, ISU, Baylor, SMU);
            return dict; 
        }

        public static Dictionary<int, int[]> CreateB()
        {
            var dict = new Dictionary<int, int[]>();

            dict.Add(Nebraska, OU, KSU, TCU,TT);
            dict.Add(OU, OkSt, KU, ISU, Texas);
            dict.Add(OkSt, Nebraska, KSU, Baylor,SMU);
            dict.Add(KSU, OU, KU, Colorado,SMU);
            dict.Add(KU, OkSt, ISU, Baylor, Nebraska);
            dict.Add(ISU, OkSt, KSU, Colorado, Nebraska);

            dict.Add(Colorado, Nebraska, Texas, TCU, TT);
            dict.Add(Texas, KU,ISU, TCU, TT);
            dict.Add(Baylor, OU, Colorado, Texas, TCU);
            dict.Add(TCU, OkSt, ISU, SMU, TT);
            dict.Add(SMU, OU, Colorado, Texas, Baylor);
            dict.Add(TT, KU, KSU, Baylor, SMU);

            return dict;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            var dict = new Dictionary<int, int[]>();

            dict.Add(Nebraska, OU, KSU, KU, Baylor);
            dict.Add(OU, OkSt, KU, ISU, Texas);
            dict.Add(OkSt, KSU, ISU, Colorado, Nebraska);
            dict.Add(KSU, OU, KU, TCU, SMU);
            dict.Add(KU, OkSt, ISU, Baylor, SMU);
            dict.Add(ISU, Nebraska, KSU, SMU, TT);

            dict.Add(Colorado, Nebraska, OU, TCU, TT);
            dict.Add(Texas, Nebraska, OkSt, Colorado, TT);
            dict.Add(Baylor, ISU, Colorado, Texas, TCU);
            dict.Add(TCU, OU, KU, Texas, SMU);
            dict.Add(SMU, Colorado, Texas, Baylor, TT);
            dict.Add(TT, OkSt, KSU, Baylor, TCU);

            return dict;
        }

        public static Dictionary<int, int[]> CreateD()
        {
            var dict = new Dictionary<int, int[]>();

            dict.Add(Nebraska, OU, KSU, TCU, SMU);
            dict.Add(OU, OkSt, KU, ISU, Texas);
            dict.Add(OkSt, Nebraska, KSU, Baylor, SMU);
            dict.Add(KSU, OU, KU, Colorado, TT);
            dict.Add(KU, OkSt, ISU, Colorado, Nebraska);
            dict.Add(ISU, OkSt, KSU, Baylor, Nebraska);

            dict.Add(Colorado, Nebraska, Texas, TCU, TT);
            dict.Add(Texas, KU, KSU, TCU, TT);
            dict.Add(Baylor, OU, Colorado, Texas, TCU);
            dict.Add(TCU, OkSt, ISU, SMU, TT);
            dict.Add(SMU, ISU, Colorado, Texas, Baylor);
            dict.Add(TT, KU, OU, Baylor, SMU);

            return dict;
        }

        public static Dictionary<int, int[]> CreateE()
        {
            var dict = new Dictionary<int, int[]>();

            dict.Add(Nebraska, OU, KSU, KU, TT);
            dict.Add(OU, OkSt, KU, ISU, Texas);
            dict.Add(OkSt, Nebraska,KSU, ISU, TCU);
            dict.Add(KSU, OU, KU, TCU, SMU);
            dict.Add(KU, OkSt, ISU, Baylor, SMU);
            dict.Add(ISU, Nebraska, KSU, Colorado, Texas);

            dict.Add(Colorado, Nebraska, OU, Baylor, TT);
            dict.Add(Texas, Nebraska, Colorado, SMU, TT);
            dict.Add(Baylor, OkSt, KSU, Texas, TCU);
            dict.Add(TCU, KU, Colorado, Texas, SMU);
            dict.Add(SMU, OU, Colorado, Baylor, TT);
            dict.Add(TT, OkSt, ISU, Baylor, TCU);

            return dict;
        }
#endif
#if false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {58,new[] {71,40,11,16} },
                {71,new[] {72,39,38,92} },
                {72,new[] {58,38,16,81} },
                {40,new[] {71,72,39,94} },
                {39,new[] {58,72,38,89} },
                {38,new[] {58,40,11,81} },
                {92,new[] {39,40,94,81} },
                {94,new[] {71,39,11,89} },
                {11,new[] {72,92,16,81} },
                {89,new[] {71,40,92,11} },
                {16,new[] {38,92,94,89} },
                {81,new[] {58,94,89,16} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {58,new[] {71,40,11,16} },
                {71,new[] {72,39,38,92} },
                {72,new[] {58,39,38,94} },
                {40,new[] {71,72,39,81} },
                {39,new[] {58,38,16,81} },
                {38,new[] {58,40,94,89} },
                {92,new[] {58,38,94,81} },
                {94,new[] {40,11,89,16} },
                {11,new[] {71,40,92,81} },
                {89,new[] {72,39,92,11} },
                {16,new[] {71,92,11,89} },
                {81,new[] {72,94,89,16} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {58,new[] {71,40,94,89} },
                {71,new[] {72,39,38,92} },
                {72,new[] {58,40,39,11} },
                {40,new[] {71,39,92,16} },
                {39,new[] {58,38,94,89} },
                {38,new[] {58,72,40,81} },
                {92,new[] {72,94,16,81} },
                {94,new[] {71,11,89,16} },
                {11,new[] {39,40,92,81} },
                {89,new[] {38,92,11,81} },
                {16,new[] {72,38,11,89} },
                {81,new[] {58,71,94,16} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {58,new[] {71,40,11,16} },
                {71,new[] {72,39,38,92} },
                {72,new[] {58,40,38,81} },
                {40,new[] {71,39,89,81} },
                {39,new[] {58,72,38,94} },
                {38,new[] {58,40,92,94} },
                {92,new[] {39,94,16,81} },
                {94,new[] {72,11,89,16} },
                {11,new[] {71,40,92,81} },
                {89,new[] {58,71,92,11} },
                {16,new[] {72,39,11,89} },
                {81,new[] {38,94,89,16} },
            };
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                {58,new[] {71,40,94,81} },
                {71,new[] {72,39,38,92} },
                {72,new[] {58,40,38,89} },
                {40,new[] {71,39,89,16} },
                {39,new[] {58,72,38,81} },
                {38,new[] {58,40,11,94} },
                {92,new[] {58,72,94,16} },
                {94,new[] {40,11,16,81} },
                {11,new[] {72,39,92,81} },
                {89,new[] {38,92,94,11} },
                {16,new[] {71,39,11,89} },
                {81,new[] {71,92,89,16} },
            };
        }
#elif false // Oklahoma and Nebraska seperate divisions
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,92} },
                { 22,new[]{ 58,BSUId,39,72} },
                { BSUId,new[]{ 40,38,89,71} },
                { 40,new[]{ 22,39,94,89} },
                { 39,new[]{ 58,BSUId,38,11} },
                { 38,new[]{ 58,22,40,94} },
                { 71,new[]{ 22,89,72,92} },
                { 94,new[]{ BSUId,71,11,72} },
                { 11,new[]{ 22,38,71,92} },
                { 89,new[]{ 39,94,11,72} },
                { 72,new[]{ 58,39,11,92} },
                { 92,new[]{ 40,38,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,11} },
                { 22,new[]{ 58,BSUId,39,94} },
                { BSUId,new[]{ 40,38,89,92} },
                { 40,new[]{ 22,39,94,72} },
                { 39,new[]{ 58,BSUId,38,71} },
                { 38,new[]{ 58,22,40,89} },
                { 71,new[]{ 38,89,72,92} },
                { 94,new[]{ 39,71,11,72} },
                { 11,new[]{ 22,40,71,92} },
                { 89,new[]{ 58,94,11,72} },
                { 72,new[]{ 22,BSUId,11,92} },
                { 92,new[]{ 39,38,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,11} },
                { 22,new[]{ 58,BSUId,39,92} },
                { BSUId,new[]{ 40,38,89,94} },
                { 40,new[]{ 22,39,89,71} },
                { 39,new[]{ 58,BSUId,38,72} },
                { 38,new[]{ 58,22,40,94} },
                { 71,new[]{ 22,89,72,92} },
                { 94,new[]{ 39,71,11,72} },
                { 11,new[]{ BSUId,39,71,92} },
                { 89,new[]{ 22,94,11,72} },
                { 72,new[]{ 40,38,11,92} },
                { 92,new[]{ 58,38,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,94} },
                { 22,new[]{ 58,BSUId,39,94} },
                { BSUId,new[]{ 40,38,89,71} },
                { 40,new[]{ 22,39,11,92} },
                { 39,new[]{ 58,BSUId,38,89} },
                { 38,new[]{ 58,22,40,72} },
                { 71,new[]{ 39,89,72,92} },
                { 94,new[]{ 40,71,11,72} },
                { 11,new[]{ 22,38,71,92} },
                { 89,new[]{ 38,94,11,72} },
                { 72,new[]{ 58,BSUId,11,92} },
                { 92,new[]{ 22,39,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,89} },
                { 22,new[]{ 58,BSUId,39,94} },
                { BSUId,new[]{ 40,38,89,92} },
                { 40,new[]{ 22,39,71,11} },
                { 39,new[]{ 58,BSUId,38,94} },
                { 38,new[]{ 58,22,40,72} },
                { 71,new[]{ 38,89,72,92} },
                { 94,new[]{ 58,71,11,72} },
                { 11,new[]{ BSUId,38,71,92} },
                { 89,new[]{ 22,94,11,72} },
                { 72,new[]{ 40,39,11,92} },
                { 92,new[]{ 22,39,94,89} },
            };
        }
#endif

#if false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{UtahId,40,71,94 } },
                { 22,new[]{58,39,38,71 } },
                { UtahId,new[]{22,40,89,72 } },
                { 40,new[]{22,39,94,89 } },
                { 39,new[]{58,UtahId,38,11 } },
                { 38,new[]{58,UtahId,40,92 } },
                { 71,new[]{39,89,72,92 } },
                { 94,new[]{22,71,11,72 } },
                { 11,new[]{22,38,71,92 } },
                { 89,new[]{38,94,11,72 } },
                { 72,new[]{58,40,11,92 } },
                { 92,new[]{UtahId,39,94,89 } },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ UtahId,40,71,11} },
                { 22,new[]{ 58,39,38,94} },
                { UtahId,new[]{ 22,40,71,92} },
                { 40,new[]{ 22,39,94,89} },
                { 39,new[]{ 58,UtahId,38,72} },
                { 38,new[]{ 58,UtahId,40,89} },
                { 71,new[]{ 38,89,72,92} },
                { 94,new[]{ 38,71,11,72} },
                { 11,new[]{ UtahId,39,71,92} },
                { 89,new[]{ 22,94,11,72} },
                { 72,new[]{ 22,40,11,92} },
                { 92,new[]{ 58,39,94,89} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ UtahId,40,71,94} },
                { 22,new[]{ 58,39,38,92} },
                { UtahId,new[]{ 22,40,94,89} },
                { 40,new[]{ 22,39,71,11} },
                { 39,new[]{ 58,UtahId,38,89} },
                { 38,new[]{ 58,UtahId,40,72} },
                { 71,new[]{ 22,89,72,92} },
                { 94,new[]{ 39,71,11,72} },
                { 11,new[]{ 22,38,71,92} },
                { 89,new[]{ 58,94,11,72} },
                { 72,new[]{ UtahId,39,11,92} },
                { 92,new[]{ 40,38,94,89} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ UtahId,40,71,72} },
                { 22,new[]{ 58,39,38,94} },
                { UtahId,new[]{ 22,40,71,11} },
                { 40,new[]{ 22,39,89,92} },
                { 39,new[]{ 58,UtahId,38,89} },
                { 38,new[]{ 58,UtahId,40,94} },
                { 71,new[]{ 39,89,72,92} },
                { 94,new[]{ 39,71,11,72} },
                { 11,new[]{ 58,40,71,92} },
                { 89,new[]{ 22,94,11,72} },
                { 72,new[]{ UtahId,38,11,92} },
                { 92,new[]{ 22,38,94,89} },
            };
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ UtahId,40,71,89} },
                { 22,new[]{ 58,39,38,89} },
                { UtahId,new[]{ 22,40,94,92} },
                { 40,new[]{ 22,39,71,11} },
                { 39,new[]{ 58,UtahId,38,72} },
                { 38,new[]{ 58,UtahId,40,94} },
                { 71,new[]{ 38,89,72,92} },
                { 94,new[]{ 40,71,11,72} },
                { 11,new[]{ 22,39,71,92} },
                { 89,new[]{ UtahId,94,11,72} },
                { 72,new[]{ 22,38,11,92} },
                { 92,new[]{ 58,39,94,89} },
            };
        }
#endif

#if true // 14 team big 12 with cincy+ucf
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                OU.Create(Colorado, OkSt, ISU, TCU),
                Colorado.Create(Nebraska, KU, OkSt, UCF),
                Nebraska.Create(OU, ISU, KSU, BSU),
                KU.Create(OU, Nebraska, ISU, Texas),
                OkSt.Create(Nebraska, KU, KSU, Cincy),
                ISU.Create(Colorado, OkSt, KSU, Baylor),
                KSU.Create(OU, Colorado, KU, TT),

                Texas.Create(OU, TCU, UCF, TT),
                BSU.Create(Colorado, Texas, UCF, Baylor),
                TCU.Create(Nebraska, BSU, Cincy, TT),
                UCF.Create(ISU, TCU, Baylor, Cincy),
                Baylor.Create(OkSt, Texas, TCU, Cincy),
                Cincy.Create(KSU, Texas, BSU, TT),
                TT.Create(KU, BSU, UCF, Baylor),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                OU.Create(Colorado, OkSt, ISU, Baylor),
                Colorado.Create(Nebraska, KU, OkSt, Cincy),
                Nebraska.Create(OU, ISU, KSU, TT),
                KU.Create(OU, Nebraska, ISU, UCF),
                OkSt.Create(Nebraska, KU, KSU, BSU),
                ISU.Create(Colorado, OkSt, KSU, Texas),
                KSU.Create(OU, Colorado, KU, TCU),

                Texas.Create(OU, TCU, UCF, TT),
                BSU.Create(Colorado, Texas, UCF, Baylor),
                TCU.Create(OkSt, BSU, Cincy, TT),
                UCF.Create(KSU, TCU, Baylor, Cincy),
                Baylor.Create(KU, Texas, TCU, Cincy),
                Cincy.Create(Nebraska, Texas, BSU, TT),
                TT.Create(ISU, BSU, UCF, Baylor),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                OU.Create(Colorado, OkSt, ISU, Cincy),
                Colorado.Create(Nebraska, KU, OkSt, KSU),
                Nebraska.Create(OU, ISU, KSU, TCU),
                KU.Create(OU, Nebraska, ISU, TT),
                OkSt.Create(Nebraska, KU, KSU, UCF),
                ISU.Create(Colorado, OkSt, KSU, BSU),
                KSU.Create(OU, KU, Texas, Baylor),

                Texas.Create(OU, TCU, UCF, TT),
                BSU.Create(Colorado, Texas, UCF, Baylor),
                TCU.Create(ISU, BSU, Cincy, TT),
                UCF.Create(Nebraska, TCU, Baylor, Cincy),
                Baylor.Create(OkSt, Texas, TCU, Cincy),
                Cincy.Create(KU, Texas, BSU, TT),
                TT.Create(Colorado, BSU, UCF, Baylor),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }
        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

#elif false // big 12 with 12 teams
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,92} },
                { 22,new[]{ 58,BSUId,39,72} },
                { BSUId,new[]{ 40,38,89,71} },
                { 40,new[]{ 22,39,94,89} },
                { 39,new[]{ 58,BSUId,38,11} },
                { 38,new[]{ 58,22,40,94} },
                { 71,new[]{ 22,89,72,92} },
                { 94,new[]{ BSUId,71,11,72} },
                { 11,new[]{ 22,38,71,92} },
                { 89,new[]{ 39,94,11,72} },
                { 72,new[]{ 58,39,11,92} },
                { 92,new[]{ 40,38,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,11} },
                { 22,new[]{ 58,BSUId,39,94} },
                { BSUId,new[]{ 40,38,89,92} },
                { 40,new[]{ 22,39,94,72} },
                { 39,new[]{ 58,BSUId,38,71} },
                { 38,new[]{ 58,22,40,89} },
                { 71,new[]{ 38,89,72,92} },
                { 94,new[]{ 39,71,11,72} },
                { 11,new[]{ 22,40,71,92} },
                { 89,new[]{ 58,94,11,72} },
                { 72,new[]{ 22,BSUId,11,92} },
                { 92,new[]{ 39,38,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,11} },
                { 22,new[]{ 58,BSUId,39,92} },
                { BSUId,new[]{ 40,38,89,94} },
                { 40,new[]{ 22,39,89,71} },
                { 39,new[]{ 58,BSUId,38,72} },
                { 38,new[]{ 58,22,40,94} },
                { 71,new[]{ 22,89,72,92} },
                { 94,new[]{ 39,71,11,72} },
                { 11,new[]{ BSUId,39,71,92} },
                { 89,new[]{ 22,94,11,72} },
                { 72,new[]{ 40,38,11,92} },
                { 92,new[]{ 58,38,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,94} },
                { 22,new[]{ 58,BSUId,39,94} },
                { BSUId,new[]{ 40,38,89,71} },
                { 40,new[]{ 22,39,11,92} },
                { 39,new[]{ 58,BSUId,38,89} },
                { 38,new[]{ 58,22,40,72} },
                { 71,new[]{ 39,89,72,92} },
                { 94,new[]{ 40,71,11,72} },
                { 11,new[]{ 22,38,71,92} },
                { 89,new[]{ 38,94,11,72} },
                { 72,new[]{ 58,BSUId,11,92} },
                { 92,new[]{ 22,39,94,89} },
            };
        }
        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                { 58,new[]{ BSUId,40,71,89} },
                { 22,new[]{ 58,BSUId,39,94} },
                { BSUId,new[]{ 40,38,89,92} },
                { 40,new[]{ 22,39,71,11} },
                { 39,new[]{ 58,BSUId,38,94} },
                { 38,new[]{ 58,22,40,72} },
                { 71,new[]{ 38,89,72,92} },
                { 94,new[]{ 58,71,11,72} },
                { 11,new[]{ BSUId,38,71,92} },
                { 89,new[]{ 22,94,11,72} },
                { 72,new[]{ 40,39,11,92} },
                { 92,new[]{ 22,39,94,89} },
            };
        }
#endif
    }
}