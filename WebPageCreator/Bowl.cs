using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.Linq;
using System.Security.AccessControl;

namespace EA_DB_Editor
{
    public class Bowl
    {
        /// <summary>
        /// starting with 2584 the Rose Bowl is only a quarterfinal
        /// </summary>
        public const int CFP12TeamRoseBowlQFOnlyStartingYear = 2584;
        public const int CFP12TeamPlayoffStartingYear = 2542;
        public const int CureBowl = 987043;
        public const int SaluteVetsBowl = 987051;
        public const int XboxBowl = 987052;
        public const int MyrtleBeachBowl = 987044;
        public const int ArizonaBowl = 987045;
        public const int MobileAlabamaBowl = 0;
        public const int FGSChampionship = 987053;
        public const int CFB8v9 = 987047;
        public const int CFB7v10 = 987048;
        public const int CFB6v11 = 987049;
        public const int CFB5v12 = 987050;
        public const int SugarBowl = 27;
        public const int RoseBowl = 25;
        public const int FiestaBowl = 26;
        public const int PeachBowl = 12;
        public const int OrangeBowl = 28;
        public const int CottonBowl = 17;
        public const int NationalChampionship = 39;

        private static HashSet<int> AugmentedBowls = new HashSet<int>()
        {
            CureBowl,
            MyrtleBeachBowl,
            ArizonaBowl,
            MobileAlabamaBowl,
            SaluteVetsBowl,
            XboxBowl,
            FGSChampionship,
        };

        public bool IsAugmentedBowl => AugmentedBowls.Contains(this.Id);

        public static Dictionary<int, Tuple<int, int>> BowlIdOverrides = new Dictionary<int, Tuple<int, int>>();
        public static Dictionary<string, Bowl> Bowls { get; private set; }
        public static Bowl FindById(int id)
        {
            return Bowls.Values.Where(b => b.Id == id).SingleOrDefault();
        }

        public static Bowl FindByKey(int week, int game)
        {
            return Bowls[CreateKey(week,game)];
        }

        public static bool TryFindByKey(int week, int game, out Bowl bowl)
        {
            bowl = null;
            if (Bowls == null) return false;
            return Bowls.TryGetValue(CreateKey(week, game), out bowl);
        }

        public static string CreateKey(int week, int game)
        {
            return week + "-" + game;
        }

        public static void Create(IDataEngine dataEngine, bool isPreseason)
        {
            if (Bowls != null)
                return;

            Bowls = dataEngine.CreateBowlTable();

            if (!isPreseason)
            {
                BowlRecords.EnsureInstanceExists();
            }
        }

        public int ConferenceTieInId1 { get; set; }
        public int ConferenceTieInId2 { get; set; }
        public int ConferenceTieInSelection1 { get; set; }
        public int ConferenceTieInSelection2 { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Week { get; set; }
        public int Game { get; set; }
        public int Day { get; set; }
        public ScheduledGame ScheduleGame { get { return ScheduledGame.Schedule[this.Key]; } }
        public string Key => $"{Week}-{Game}{(Day == 0 ? string.Empty : Day.ToString())}";

        // Order the bowl games will show up in on the bowls.html page
        public static int[] PlayoffBowlOrder = ConfigurationManager.AppSettings["BowlOrder"]?.Split(',').Select(s => Convert.ToInt32(s.Trim())).ToArray();

        public static bool IsPlayoffRound1( Game g)
        {
            if (Form1.CalendarYear >= CFP12TeamPlayoffStartingYear &&
                TryFindByKey(g.Week, g.GameNumber, out var bowl))
            {

                var bowlId = bowl.Id;
                return bowlId == CFB5v12 ||
                    bowlId == CFB6v11 ||
                    bowlId == CFB7v10 ||
                    bowlId == CFB8v9;
            }

            return false;
        }

        public static bool IsSemiFinal(Game g)
        {
            if(Form1.CalendarYear < CFP12TeamRoseBowlQFOnlyStartingYear)
            {
                return IsSemiFinalOld(g);
            }

            if (TryFindByKey(g.Week, g.GameNumber, out var bowl))
            {
                var rotation = (Form1.CalendarYear - CFP12TeamRoseBowlQFOnlyStartingYear) % 5;
                var semiFinalBowls = new HashSet<int>();

                switch (rotation)
                {
                    case 0:
                        // Sugar & Orange  
                        semiFinalBowls = new HashSet<int>() { SugarBowl, OrangeBowl };
                        break;

                    case 1:
                        // Cotton & Peach
                        semiFinalBowls = new HashSet<int>() { CottonBowl, PeachBowl };
                        break;

                    case 2:
                        // Fiesta & Sugar
                        semiFinalBowls = new HashSet<int>() { FiestaBowl, SugarBowl };
                        break;

                    case 3:
                        // Orange & Cotton
                        semiFinalBowls = new HashSet<int>() { OrangeBowl, CottonBowl};
                        break;

                    case 4:
                        // Fiesta & Peach
                        semiFinalBowls = new HashSet<int>() { FiestaBowl, PeachBowl };
                        break;

                    default:
                        throw new InvalidOperationException("BAD PLAYOFF ORDER");
                }

                return semiFinalBowls.Contains(bowl.Id);
            }

            return false;
        }

        public static bool IsQuarterfinal(Game g)
        {
            if (Form1.CalendarYear < CFP12TeamRoseBowlQFOnlyStartingYear)
            {
                return IsQuarterfinalOld(g);
            }

            if (TryFindByKey(g.Week, g.GameNumber, out var bowl))
            {
                var rotation = (Form1.CalendarYear - CFP12TeamRoseBowlQFOnlyStartingYear) % 5;
                var quarterFinalBowls = new HashSet<int>();

                switch (rotation)
                {
                    case 0:
                        // Sugar & Orange are semi
                        // Rose, Cotton, Peach, Fiesta  are quarter
                        quarterFinalBowls = new HashSet<int>() { RoseBowl, CottonBowl, PeachBowl, FiestaBowl };
                        break;

                    case 1:
                        // Cotton & Peach are semi
                        // Sugar, Fiesta, Orange, Rose
                        quarterFinalBowls = new HashSet<int>() { RoseBowl, SugarBowl, OrangeBowl, FiestaBowl };
                        break;

                    case 2:
                        // Fiesta & Sugar are semi
                        // Rose, Orange, Cotton, Peach are quarter
                        quarterFinalBowls = new HashSet<int>() { RoseBowl, OrangeBowl, PeachBowl, CottonBowl};
                        break;

                    case 3:
                        // Orange & Cotton
                        // Rose, Sugar, Peach, Fiesta
                        quarterFinalBowls = new HashSet<int>() { RoseBowl, SugarBowl, PeachBowl, FiestaBowl };
                        break;

                    case 4:
                        // Fiesta & Peach
                        // Rose, Sugar, Orange, Cotton
                        quarterFinalBowls = new HashSet<int>() { RoseBowl, CottonBowl, OrangeBowl, SugarBowl};
                        break;

                    default:
                        throw new InvalidOperationException("BAD PLAYOFF ORDER");
                }

                return quarterFinalBowls.Contains(bowl.Id);
            }

            return false;
        }

        public static bool IsSemiFinalOld( Game g)
        {
            if (Form1.CalendarYear >= CFP12TeamPlayoffStartingYear &&
                TryFindByKey(g.Week, g.GameNumber, out var bowl))
            {
                var rotation = (Form1.CalendarYear - CFP12TeamPlayoffStartingYear) % 3;
                var quarterFinalBowls = new HashSet<int>();

                switch (rotation)
                {
                    case 0:
                        // orange, cotton, peach, fiesta
                        quarterFinalBowls = new HashSet<int>() { 25, 27 };
                        break;

                    case 1:
                        // rose, sugar, peach, fiesta
                        quarterFinalBowls = new HashSet<int>() { 28, 17 };
                        break;

                    case 2:
                        // rose, sugar, orange, cotton
                        quarterFinalBowls = new HashSet<int>() { 12, 26 };
                        break;

                    default:
                        throw new InvalidOperationException("BAD PLAYOFF ORDER");
                }

                return quarterFinalBowls.Contains(bowl.Id);
            }

            return false;
        }

        public static bool IsQuarterfinalOld( Game g)
        {
            if (Form1.CalendarYear >= CFP12TeamPlayoffStartingYear &&
                TryFindByKey(g.Week, g.GameNumber, out var bowl))
            {
                var rotation = (Form1.CalendarYear - CFP12TeamPlayoffStartingYear) % 3;
                var quarterFinalBowls = new HashSet<int>();

                switch (rotation)
                {
                    case 0:
                        // orange, cotton, peach, fiesta
                        quarterFinalBowls = new HashSet<int>() { 28, 17, 12, 26 };
                        break;

                    case 1:
                        // rose, sugar, peach, fiesta
                        quarterFinalBowls = new HashSet<int>() { 25, 27, 12, 26 };
                        break;

                    case 2:
                        // rose, sugar, orange, cotton
                        quarterFinalBowls = new HashSet<int>() { 25, 27, 28, 17 };
                        break;

                    default:
                        throw new InvalidOperationException("BAD PLAYOFF ORDER");
                }

                return quarterFinalBowls.Contains(bowl.Id);
            }

            return false;
        }

        public static List<Bowl> GetBowlsInPlayoffOrder()
        {

            if (Form1.CalendarYear < CFP12TeamRoseBowlQFOnlyStartingYear)
            {
                return GetBowlsInPlayoffOrderOld();
            }

            List<Bowl> bowls = new List<Bowl>();
            var firstRnd = new[] { CFB5v12, CFB6v11, CFB7v10, CFB8v9};
            var order = new List<int>() { NationalChampionship };
            var rotation = (Form1.CalendarYear - CFP12TeamRoseBowlQFOnlyStartingYear) % 5;

            switch (rotation)
            {
                case 0:
                    // Sugar & Orange are semi
                    // Rose, Cotton, Peach, Fiesta  are quarter
                    order.AddBowls(SugarBowl, OrangeBowl, RoseBowl, CottonBowl, PeachBowl, FiestaBowl);
                    break;

                case 1:
                    // Cotton & Peach are semi
                    // Sugar, Fiesta, Orange, Rose
                    order.AddBowls(CottonBowl, PeachBowl, RoseBowl, SugarBowl, OrangeBowl, FiestaBowl);
                    break;

                case 2:
                    // Fiesta & Sugar are semi
                    // Rose, Orange, Cotton, Peach are quarter
                    order.AddBowls(SugarBowl, FiestaBowl, RoseBowl, OrangeBowl, CottonBowl, PeachBowl);
                    break;

                case 3:
                    // Orange & Cotton
                    // Rose, Sugar, Peach, Fiesta
                    order.AddBowls(OrangeBowl, CottonBowl, RoseBowl, SugarBowl, PeachBowl, FiestaBowl);
                    break;

                case 4:
                    // Fiesta & Peach
                    // Rose, Sugar, Orange, Cotton
                    order.AddBowls(PeachBowl, FiestaBowl, RoseBowl, SugarBowl, OrangeBowl, CottonBowl);
                    break;

                default:
                    throw new InvalidOperationException("BAD PLAYOFF ORDER");

            }

            order.AddRange(firstRnd);
            bowls.AddRange(order.Select(i => Bowl.FindById(i)));

            // we have expanded playoffs find teams where rank < than playoff count
            foreach (var bowl in Bowls.Values.Where(b => b.Week > 16 && b.ScheduleGame.IsPlayoffGame(Utility.PlayoffTeamCount) && !bowls.Any(bowl => bowl.Id == b.Id)).OrderBy(b => b.ScheduleGame.HomeTeam.BCSRank))
            {
                bowls.Add(bowl);
            }

            foreach (var bowl in Bowls.Values.Where(b => b.Week > 16 && !bowls.Any(bowl => bowl.Id == b.Id)).OrderByDescending(b => b.Week).ThenByDescending(b => b.Game))
            {
                bowls.Add(bowl);
            }

            return bowls;
        }

        public static List<Bowl> GetBowlsInPlayoffOrderOld()
        {
            List<Bowl> bowls = new List<Bowl>();
            var order = PlayoffBowlOrder;

            if (Form1.CalendarYear >= CFP12TeamPlayoffStartingYear)
            {
                var rotation = (Form1.CalendarYear - CFP12TeamPlayoffStartingYear) % 3;

                switch (rotation)
                {
                    case 0:
                        order = new[] { 39, 25, 27, 28, 17, 12, 26, 987050, 987049, 987048, 987047 };
                        break;

                    case 1:
                        order = new[] { 39, 28, 17, 25, 27, 12, 26, 987050, 987049, 987048, 987047 };
                        break;

                    case 2:
                        order = new[] { 39, 12, 26, 25, 27, 28, 17, 987050, 987049, 987048, 987047 };
                        break;

                    default:
                        throw new InvalidOperationException("BAD PLAYOFF ORDER");
                }
            }

            for (int i = 0; i < order.Length; i++)
            {
                bowls.Add(Bowl.FindById(order[i]));
            }

            // we have expanded playoffs find teams where rank < than playoff count
            foreach (var bowl in Bowls.Values.Where(b => b.Week > 16 && b.ScheduleGame.IsPlayoffGame(Utility.PlayoffTeamCount) && !bowls.Any(bowl => bowl.Id == b.Id)).OrderBy(b => b.ScheduleGame.HomeTeam.BCSRank))
            {
                bowls.Add(bowl);
            }

            foreach (var bowl in Bowls.Values.Where(b => b.Week > 16 && !bowls.Any(bowl => bowl.Id == b.Id)).OrderByDescending(b => b.Week).ThenByDescending(b => b.Game))
            {
                bowls.Add(bowl);
            }

            return bowls;
        }
    }
}
