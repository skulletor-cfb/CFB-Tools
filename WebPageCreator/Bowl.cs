using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EA_DB_Editor
{
    public class Bowl
    {
        public const int CFP12TeamPlayoffStartingYear = 2542;
        public const int CureBowl = 987043;
        public const int MyrtleBeachBowl = 987044;
        public const int ArizonaBowl = 987045;
        public const int MobileAlabamaBowl = 0;
        public const int CFB8v9 = 987047;
        public const int CFB7v10 = 987048;
        public const int CFB6v11 = 987049;
        public const int CFB5v12 = 987050;
        public const int SaluteVetsBowl = 987051;
        public const int XboxBowl = 987052;
        public const int FGSChampionship = 987053;

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

        public static bool IsSemiFinal( Game g)
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

        public static bool IsQuarterfinal( Game g)
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
