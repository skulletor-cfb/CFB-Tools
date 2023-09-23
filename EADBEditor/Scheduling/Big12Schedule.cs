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
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] 
        {
            CreateA, CreateB, 
            CreateX, CreateY,
            CreateZ, CreateA,
            CreateB, CreateX,
            CreateY, CreateZ,
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
                    var idx = (Form1.DynastyYear - 2480) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

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

        public static void SetNewTeams(this PreseasonScheduledGame game, Dictionary<int, PreseasonScheduledGame[]> schedule, Dictionary<int, int[]> homeSchedules, int week, int a, int b)
        {
            game.HomeTeam = a;
            game.AwayTeam = b;
            game.MaddenRecord["GATG"] = game.AwayTeam.ToString();
            game.MaddenRecord["GHTG"] = game.HomeTeam.ToString();
            game.SetHomeTeam(homeSchedules);
            game.AssignGame(schedule, week);
        }

        public static void ProcessACCSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(
                ScenarioForSeason,
                Big12ConferenceSchedule,
                RecruitingFixup.Big12Id,
                RecruitingFixup.Big12);
        }


        public static void ProcessSchedule(this Dictionary<int, PreseasonScheduledGame[]> schedule, Dictionary<int, int[]> homeSchedules, Dictionary<int, HashSet<int>> opponents, int confId, int[] conference, int? excludeTeam = null)
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
                if(excludeTeam.HasValue && key == excludeTeam.Value)
                {
                    continue;
                }

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


#if false  // 14 team big 12 no divisions!
        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(TT, Cincy, BSU, KSU),
                TCU.Create(Texas, BSU, OkSt, Nebraska),
                Baylor.Create(Texas, TCU, ISU, OU),
                TT.Create(Baylor, UCF, Colorado, KU),
                Cincy.Create(TCU, BSU, KSU, OkSt),
                BSU.Create(Baylor, Colorado, KU, OkSt),
                UCF.Create(TCU,Cincy, ISU, OU),

                Colorado.Create(Baylor, UCF, KU, Nebraska),
                ISU.Create(TT, Cincy, Colorado, KSU),
                KU.Create(Baylor, UCF, ISU, OU),
                KSU.Create(TCU, BSU, KU, Nebraska),
                OU.Create(Texas, TT, Colorado, OkSt ),
                OkSt.Create(TT, UCF, KSU, Nebraska),
                Nebraska.Create(Texas, Cincy, ISU, OU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(),
                TCU.Create(),
                Baylor.Create(),
                TT.Create(),
                Cincy.Create(),
                BSU.Create(),
                UCF.Create( ),

                Colorado.Create(),
                ISU.Create(),
                KU.Create(),
                KSU.Create(),
                OU.Create(),
                OkSt.Create(),
                Nebraska.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(TT, Cincy, Colorado, OkSt),
                TCU.Create(Texas, BSU, KU, Nebraska),
                Baylor.Create(Texas, TCU, ISU, OU),
                TT.Create(Baylor, BSU, UCF, KSU),
                Cincy.Create(TCU, BSU, KU, OkSt),
                BSU.Create(Baylor, Colorado, OU, Nebraska),
                UCF.Create( TCU, Cincy, Colorado, ISU),

                Colorado.Create(TCU, Cincy, KU, Nebraska),
                ISU.Create(TT, Cincy, KSU, OU),
                KU.Create(Texas, BSU, ISU, OkSt),
                KSU.Create(Baylor, UCF, KU, Nebraska),
                OU.Create(Texas, TT, KSU, OkSt),
                OkSt.Create(TT, UCF, Colorado, KSU),
                Nebraska.Create(Baylor, UCF, ISU, OU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Texas.Create(TT, BSU, UCF, KSU),
                TCU.Create(TT, BSU, ISU, OkSt),
                Baylor.Create(Texas, TCU, UCF, Colorado),
                TT.Create(Baylor, Cincy, KU, Nebraska),
                Cincy.Create(Baylor, BSU, KSU, OU),
                BSU.Create(Colorado, ISU, KSU, OkSt),
                UCF.Create( TCU, Cincy, BSU, OU),

                Colorado.Create(TT, ISU, KU , Nebraska),
                ISU.Create(Texas, Cincy, KSU, OkSt),
                KU.Create(Baylor, UCF, ISU, OU),
                KSU.Create(TCU, Colorado, KU, Nebraska),
                OU.Create(Texas, TCU, Colorado, OkSt),
                OkSt.Create(Baylor, TT, UCF, Nebraska),
                Nebraska.Create(Texas, Cincy, KU, OU),
            }.Create();
        }
#elif false // 14 team big 12 with cincy+ucf
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

#elif true // big 12 with north/south cincy in it instead of bsu
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Nebraska.Create(KU, Cincy, OU, TCU),
                ISU.Create(Nebraska, KSU, Cincy, Texas),
                KU.Create(ISU, Colorado, OU, Baylor),
                Colorado.Create(Nebraska, ISU, KSU, TT),
                KSU.Create(Nebraska, KU, Cincy, Texas),
                Cincy.Create(KU, Colorado, OkSt, Baylor),

                OU.Create(ISU, Texas, TCU, OkSt),
                Texas.Create(Nebraska, TCU, OkSt, TT),
                TCU.Create(ISU, Colorado, OkSt, TT),
                OkSt.Create(Colorado, KSU, TT, Baylor),
                TT.Create(KU, Cincy, OU, Baylor),
                Baylor.Create(KSU, OU, Texas, TCU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Nebraska.Create(),
                ISU.Create(),
                KU.Create(),
                Colorado.Create(),
                KSU.Create(),
                Cincy.Create(),

                OU.Create(),
                Texas.Create(),
                TCU.Create(),
                OkSt.Create(),
                TT.Create(),
                Baylor.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Nebraska.Create(),
                ISU.Create(),
                KU.Create(),
                Colorado.Create(),
                KSU.Create(),
                Cincy.Create(),

                OU.Create(),
                Texas.Create(),
                TCU.Create(),
                OkSt.Create(),
                TT.Create(),
                Baylor.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Nebraska.Create(),
                ISU.Create(),
                KU.Create(),
                Colorado.Create(),
                KSU.Create(),
                Cincy.Create(),

                OU.Create(),
                Texas.Create(),
                TCU.Create(),
                OkSt.Create(),
                TT.Create(),
                Baylor.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Nebraska.Create(),
                ISU.Create(),
                KU.Create(),
                Colorado.Create(),
                KSU.Create(),
                Cincy.Create(),

                OU.Create(),
                Texas.Create(),
                TCU.Create(),
                OkSt.Create(),
                TT.Create(),
                Baylor.Create(),
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