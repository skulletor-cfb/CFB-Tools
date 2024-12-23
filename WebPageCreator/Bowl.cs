using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EA_DB_Editor
{
    public class Bowl
    {
        private const int CureBowl = 987043;
        private const int MyrtleBeachBowl = 987044;
        private const int ArizonaBowl = 987045;
        private const int MobileAlabamaBowl = 0;

        private static HashSet<int> AugmentedBowls = new HashSet<int>()
        {
            CureBowl,
            MyrtleBeachBowl,
            ArizonaBowl,
            MobileAlabamaBowl,
        };

        public bool IsAugmentedBowl => AugmentedBowls.Contains(this.Id);

        public static Dictionary<int, Tuple<int, int>> BowlIdOverrides = new Dictionary<int, Tuple<int,int>>();
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

        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            if (Bowls != null)
                return;

            Bowls = new Dictionary<string, Bowl>();
            var table = db.lTables[129];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var bowl = new Bowl
                {
                    Id = table.lRecords[i].lEntries[15].Data.ToInt32(),
                    Name = table.lRecords[i].lEntries[8].Data,
                    Week = table.lRecords[i].lEntries[12].Data.ToInt32(),
                    Game = table.lRecords[i].lEntries[10].Data.ToInt32(),
                    ConferenceTieInId1 = table.lRecords[i]["BCI1"].ToInt32(),
                    ConferenceTieInId2 = table.lRecords[i]["BCI2"].ToInt32(),
                    ConferenceTieInSelection1 = table.lRecords[i]["BCR1"].ToInt32(),
                    ConferenceTieInSelection2 = table.lRecords[i]["BCR2"].ToInt32(),
                };

                if (BowlIdOverrides.ContainsKey(bowl.Id) && BowlIdOverrides[bowl.Id].Item2 <= BowlChampion.CurrentYear)
                    bowl.Id = BowlIdOverrides[bowl.Id].Item1;

                if (bowl.Game != 255)
                    Bowls.Add(bowl.Key, bowl);
            }

            var cureBowl = new Bowl
            {
                Id = CureBowl,
                Name = "Cure Bowl",
                Week = 18,
                Game = 43,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var mbBowl = new Bowl
            {
                Id = MyrtleBeachBowl,
                Name = "Myrtle Beach Bowl",
                Week = 18,
                Game = 44,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var arizonaBowl = new Bowl
            {
                Id = ArizonaBowl,
                Name = "Arizona Bowl",
                Week = 18,
                Game = 45,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var venturesBowl = new Bowl
            {
                Id = MobileAlabamaBowl,
                Name = "68 Ventures Bowl",
                Week = 18,
                Game = 46,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            Bowls.Add(cureBowl.Key, cureBowl);
            Bowls.Add(mbBowl.Key, mbBowl);
            Bowls.Add(arizonaBowl.Key, arizonaBowl);
            Bowls.Add(venturesBowl.Key, venturesBowl);

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
        public ScheduledGame ScheduleGame { get { return ScheduledGame.Schedule[this.Key]; } }
        public string Key { get { return string.Format("{0}-{1}", Week, Game); } }

        // Order the bowl games will show up in on the bowls.html page
        public static int[] PlayoffBowlOrder = ConfigurationManager.AppSettings["BowlOrder"].Split(',').Select(s => Convert.ToInt32(s.Trim())).ToArray();
        public static List<Bowl> GetBowlsInPlayoffOrder()
        {
            List<Bowl> bowls = new List<Bowl>();
            var order = PlayoffBowlOrder;
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
