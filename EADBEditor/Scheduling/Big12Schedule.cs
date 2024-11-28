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

#if false
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[]
        {
            CreateNDAPrime, CreateNDAPrime,
            CreateNDY, CreateNDY,
            CreateNDZ, CreateNDZ,
            CreateNDA, CreateNDA,
            CreateNDY, CreateNDY,
            CreateNDZ, CreateNDZ,

            /*
            CreateNDAPrime, CreateNDZ,
            CreateNDY, CreateNDA,
            CreateNDY, CreateNDZ,

            CreateNDA, CreateNDY,
            CreateNDZ, CreateNDAPrime,
            CreateNDZ, CreateNDY,*/
        };

/*        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] 
        {
            CreateNDY, CreateNDZ,
            CreateNDAPrime, CreateNDY,
            CreateNDZ, CreateNDAPrime,

            CreateNDY, CreateNDZ,
            CreateNDA, CreateNDY,
            CreateNDZ, CreateNDA,
        };

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[]
        {
            Create15A, Create15A,
            Create15B, Create15B,
        };*/
#else  //16 team big 12
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[]
        {
            Create16A, Create16A,
            Create16B, Create16B,
        };
#endif

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
                    var idx = (Form1.DynastyYear - 2516) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(16, RecruitingFixup.Big12Id, "Big12");
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

#elif false // big 12 has 15 teams, no divisions, smu, ucf, hou are in it
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

#elif true // big 12 with north/south cincy in it instead of bsu.  No divisions
        public static Dictionary<int, int[]> CreateNDY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                TT.Create(OU, Baylor, ISU, KU),
                Texas.Create(TT, TCU, Colorado, Cincy),
                OU.Create(Texas, OkSt, Baylor, KU),
                OkSt.Create(TT, Nebraska, Colorado, ISU),
                TCU.Create(TT, OkSt, Cincy, KSU),
                Baylor.Create(Texas, TCU, Colorado, KSU),
                Nebraska.Create(Texas, OU, TCU, Cincy),
                Colorado.Create(TT, Nebraska, Cincy, KU),
                Cincy.Create(OkSt, Baylor, ISU, KSU),
                ISU.Create(OU, Baylor, Nebraska, KSU),
                KU.Create(OkSt, TCU, Nebraska, ISU),
                KSU.Create(Texas, OU, Colorado, KU),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                TT.Create(TCU, Baylor, Nebraska, KSU),
                Texas.Create(TT, OkSt, Cincy, ISU),
                OU.Create(TT, Texas, OkSt, Cincy),
                OkSt.Create(Baylor, Colorado, ISU, KSU),
                TCU.Create(OU, Colorado, Cincy, ISU),
                Baylor.Create(Texas, TCU, Colorado, KU),
                Nebraska.Create(OU, TCU, Baylor, KU),
                Colorado.Create(Texas, OU, Nebraska, KU),
                Cincy.Create(TT, OkSt, ISU, KSU),
                ISU.Create(Baylor, Nebraska, Colorado, KSU),
                KU.Create(TT, Texas, OkSt, Cincy),
                KSU.Create(OU, TCU , Nebraska, KU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateNDAPrime()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                TT.Create(Baylor, Colorado, Cincy, ISU),
                Texas.Create(TT, OkSt, Nebraska, ISU),
                OU.Create(Texas, OkSt, Cincy, ISU),
                OkSt.Create(TT, Baylor, Nebraska, KSU),
                TCU.Create(Texas, OkSt, Cincy, KU),
                Baylor.Create(Texas, OU, TCU, KSU),
                Nebraska.Create(TT, OU, Cincy, KSU),
                Colorado.Create(OU, TCU, Nebraska, KU),
                Cincy.Create(OkSt, Baylor, Colorado, KU),
                ISU.Create(TCU, Nebraska, Colorado, KSU),
                KU.Create(Texas, OU, Baylor, ISU),
                KSU.Create(TT, TCU, Colorado, KU),
            }.Create();
        }


        public static Dictionary<int, int[]> CreateNDA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                TT.Create(Baylor, Colorado, Cincy, ISU),
                Texas.Create(TT, OkSt, ISU, KU),
                OU.Create(Texas, OkSt, TCU, Cincy),
                OkSt.Create(TT, Baylor, Nebraska, KSU),
                TCU.Create(Texas, OkSt, Cincy, ISU),
                Baylor.Create(Texas, TCU, Nebraska, KSU),
                Nebraska.Create(TT, OU, Cincy, KSU),
                Colorado.Create(OU, TCU, Nebraska, KU),
                Cincy.Create(OkSt, Baylor, Colorado, KU),
                ISU.Create(OU, Nebraska, Colorado, KSU),
                KU.Create(OU, TCU, Baylor, ISU),
                KSU.Create(TT, Texas, Colorado, KU),
            }.Create();
        }

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
                Nebraska.Create(KU, Cincy, OU, TT),
                ISU.Create(Nebraska, KSU, Cincy, Baylor),
                KU.Create(ISU, Colorado, Texas, OkSt),
                Colorado.Create(Nebraska, ISU, KSU, Texas),
                KSU.Create(Nebraska, KU, Cincy, OU),
                Cincy.Create(KU, Colorado, TCU, Baylor),

                OU.Create(Colorado, Texas, TCU, OkSt),
                Texas.Create(Cincy, TCU, OkSt, TT),
                TCU.Create(KSU, KU, OkSt, TT),
                OkSt.Create(Nebraska, ISU, TT, Baylor),
                TT.Create(ISU, KSU, OU, Baylor),
                Baylor.Create(Colorado, OU, Texas, TCU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Nebraska.Create(KU, Cincy, OU, Baylor),
                ISU.Create(Nebraska, KSU, Cincy, TT),
                KU.Create(ISU, Colorado, OU, OkSt),
                Colorado.Create(Nebraska, ISU, KSU, Texas),
                KSU.Create(Nebraska, KU, Cincy, Baylor),
                Cincy.Create(KU, Colorado, TCU, Texas),

                OU.Create(Colorado, Texas, TCU, OkSt),
                Texas.Create(KSU, TCU, OkSt, TT),
                TCU.Create(Nebraska, KU, OkSt, TT),
                OkSt.Create(Cincy, ISU, TT, Baylor),
                TT.Create(Colorado, KSU, OU, Baylor),
                Baylor.Create(ISU, OU, Texas, TCU),
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