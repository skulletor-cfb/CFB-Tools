using EA_DB_Editor.CAPGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    public class NamesFile
    {

    }

    public class TeamNumbers
    {
        public int Id { get; set; }
        public HashSet<int> NumbersUsed { get; set; }

        public TeamNumbers(int id)
        {
            Id = id;
            NumbersUsed = new HashSet<int>();
        }
    }

    public static class PlayerHelper
    {
        public static int JerseyNumber(this MaddenRecord mr)
        {
            return mr["PJEN"].ToInt32();
        }

        public static void AssignJerseuNumber(this MaddenRecord mr, int num)
        {
            mr["PJEN"] = num.ToString();
        }

        public static int Position(this MaddenRecord mr)
        {
            return mr["PPOS"].ToInt32();
        }

        public static int Overall(this MaddenRecord mr)
        {
            return mr["POVR"].ToInt32();
        }

        public static int TeamId(this MaddenRecord mr)
        {
            return mr["TGID"].ToInt32();
        }

        public static int RightSleeve(this MaddenRecord mr)
        {
            return mr["PLSR"].ToInt32();
        }

        public static int LeftSleeve(this MaddenRecord mr)
        {
            return mr["PLSL"].ToInt32();
        }

        public static void AssignLeftSleeve(this MaddenRecord mr, int num)
        {
             mr["PLSL"] = num.ToString();
        }

        /*
         * 0 = short
1 = long white
2 = long black
3 = long team color
4 = btm 1/2 sleeve white
5 = btm 1/2 sleeve black 
6 = btm 1/2 sleeve team color
7 = top 1/2 white
8  = top 1/2 black
9 = top 1/2 team color
10 = 3/4 white
11 = 3/4 black
12 = 3/4 team color
*/
        public static void FixPlayerSleeve(this MaddenRecord mr)
        {
            // if left/rigt sleeve don't match make them match
            // 1-3 may be 0 or the same
            // 4-6 may be 0 or the same
            // 7-12 must match
            if (mr.LeftSleeve() != mr.RightSleeve())
            {
                // 10 percent chance of being 0
                if (mr.RightSleeve() < 7 && Rand() < 26)
                {
                    mr.AssignLeftSleeve(0);
                }
                else if (mr.RightSleeve() > 0)
                {
                    mr.AssignLeftSleeve(mr.RightSleeve());
                }
            }
        }

        public static int Rand()
        {
            var guid = Guid.NewGuid().ToByteArray();
            return (int)guid[(guid[0] % 15)+1];
        }
    }

    public class PositionNumbers
    {
        public static Dictionary<int, PositionNumbers> NumberLookup = new Dictionary<int, PositionNumbers>();
        public int Position { get; set; }
        public List<int> Range { get; set; }
        private static bool initRun = false;
        private static object initLock = new object();

        public bool ReplaceOnlyIfDuplicate { get; set; }

        public int Next()
        {
            var rand = PlayerHelper.Rand();
            return Range[rand % Range.Count];
        }

        public PositionNumbers(int position, params Tuple<int, int>[] range)
        {
            this.ReplaceOnlyIfDuplicate = false;
            this.Position = position;
            this.Range = new List<int>();

            if (range == null || range.Length == 0)
                return;

            foreach (var t in range)
            {
                for (int i = t.Item1; i <= t.Item2; i++)
                {
                    this.Range.Add(i);
                }
            }
        }

        public static void Run()
        {
            Run(Form1.MainForm);
        }

        public static void Run(Form1 form)
        {
            Init();
            var playerTable = MaddenTable.FindTable(form.maddenDB.lTables, "PLAY");

            Dictionary<int, TeamNumbers> dict = new Dictionary<int, TeamNumbers>();
            foreach (var row in playerTable.lRecords)
            {
                row.FixPlayerSleeve();
                var team = row.TeamId();

                if (team == 1023)
                    continue;

                TeamNumbers numbers = null;

                if (!dict.TryGetValue(team, out numbers))
                {
                    numbers = new TeamNumbers(team);
                    dict.Add(team, numbers);
                }

                // don't add new frosh numbers to the mix just yet
                if (IsNewFrosh(row))
                    continue;

                if (!numbers.NumbersUsed.Add(row.JerseyNumber()))
                {
                    var position = row.Position();
                    var positionNums = NumberLookup[position];

                    // transfer might have dupe number, fix that
                    while (true)
                    {
                        var num = positionNums.Next();
                        if (numbers.NumbersUsed.Add(num))
                        {
                            row.AssignJerseuNumber(num);
                            break;
                        }
                    }
                }
            }

            // now we know all the numbers of all the players on all teams
            var incomingFrosh = playerTable.lRecords.Where(r => IsNewFrosh(r)).ToArray();
            incomingFrosh = incomingFrosh.Where(r => r.Position() == 0).OrderByDescending(r => r.Overall()).Concat(incomingFrosh.Where(r => r.Position() != 0 && r.Position() < 19).OrderByDescending(r => r.Overall())).Concat(incomingFrosh.Where(r => r.Position() >= 19)).ToArray();
            foreach (var row in incomingFrosh)
            {
                var team = row.TeamId();

                if (team == 1023)
                    continue;

                TeamNumbers numbers = null;

                if (!dict.TryGetValue(team, out numbers))
                {
                    continue;
                }

                var position = row.Position();
                var positionNums = NumberLookup[position];

                if (positionNums.ReplaceOnlyIfDuplicate && numbers.NumbersUsed.Add(row.JerseyNumber()))
                {
                    continue;
                }

                while (true)
                {
                    var num = positionNums.Next();
                    if (numbers.NumbersUsed.Add(num))
                    {
                        row.AssignJerseuNumber(num);
                        break;
                    }
                }
            }
        }

        public static bool IsNewFrosh(MaddenRecord mr)
        {
            return  mr["PYEA"].ToInt32() == 0 && mr["PRSD"].ToInt32() == 0;
        }

        public static Dictionary<int, string> PositionGroups = new Dictionary<int, string>()
        {
            {0,"QB" },
            {1,"HB" },
            {3,"WR" },
            {4,"TE" },
            {5,"OL" },
            {6,"OL" },
            {7,"OL" },
            {8,"DE" },
            {9,"DT" },
            {10,"LB" },
            {11,"LB" },
            {12,"DB" },
            {13,"DB" },
            {14,"DB" },
        };

        // Position:  10x: Height adjustment : Weight Adjustment
        public static Dictionary<string, Tuple<int, int>> SizeAdjustments = new Dictionary<string, Tuple<int, int>>()
        {
            {"OL", new Tuple<int, int>(10,7) },
            {"LB", new Tuple<int, int>(5,0) },
            {"DE", new Tuple<int, int>(9,0) },
            {"TE", new Tuple<int, int>(6,0) },
            {"WR", new Tuple<int, int>(5,0) },
            {"DT", new Tuple<int, int>(8,15) },
            {"QB", new Tuple<int, int>(5,0) },
        };

        static  bool FixSizeRun = false;

        public static void FixSizes(List<MaddenRecord> records)
        {
            if (FixSizeRun) return;

            FixSizeRun = true;
            var table = records.OrderBy(mr => mr["RCRK"].ToInt32()).ToArray();
            var bucketSize = table.Length / 10;

            for (int i = 0; i < 3; i++)
            {
                var groupCounts = table
                    .Where(mr => mr["RCRK"].ToInt32() < (1+i) * bucketSize)
                    .Where( mr => PositionGroups.ContainsKey(mr["RPGP"].ToInt32()) && SizeAdjustments.ContainsKey(PositionGroups[mr["RPGP"].ToInt32()]))
                    .GroupBy(mr => PositionGroups[mr["RPGP"].ToInt32()]).ToDictionary(g => g.Key, g => g.Count());

                var dict = groupCounts.ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                    {
                        var tuple = SizeAdjustments[kvp.Key];
                        var height = kvp.Value * tuple.Item1 * (10-(3*i));
                        var weight = kvp.Value * tuple.Item2 * (10 -(3*i));
                        height = (height / 100) + (height % 10);
                        weight = (weight / 10) + (weight % 10);
                        return new SizeAdjustment { Height = height, Weight = weight };
                    });

                for( int j = 0; j < bucketSize; j++)
                {
                    var mr = table[(i * bucketSize) + j];

                    if (!PositionGroups.ContainsKey(mr["RPGP"].ToInt32()))
                        continue;

                    var sizeKey = PositionGroups[mr["RPGP"].ToInt32()];
                    if ( !dict.ContainsKey(sizeKey))
                    {
                        continue;
                    }

                    // height goes up between 0 and 2 inches
                    var size = dict[sizeKey];
                    if( size.Height > 0)
                    {
                        var heightFix = Guid.NewGuid().ToByteArray().First() % 3;
                        mr["PHGT"] = (mr["PHGT"].ToInt32() + heightFix).ToString();
                        size.Height -= heightFix;
                    }

                    if( size.Weight > 0)
                    {
                        var adj = 2*SizeAdjustments[sizeKey].Item2;

                        if( adj > 0 )
                        {
                            var weightFix = Guid.NewGuid().ToByteArray().First() % adj;
                            mr["PWGT"] = (mr["PWGT"].ToInt32() + weightFix).ToString();
                            size.Weight -= weightFix;
                        }
                    }
                }
            }
        }
       
        public class SizeAdjustment
        {
            public int Height { get; set; }
            public int Weight { get; set; }
        }

        public static List<PositionGroup> PositionSizes = null;

        [DataContract]
        public class PositionGroup
        {
            [DataMember]
            public double Height { get; set; }

            [DataMember]
            public double Weight { get; set; }

            [DataMember]
            public Rating[] Ratings { get; set; }

            [DataMember]
            public Rating[] TopRatings { get; set; }


            [DataMember]
            public string Position { get; set; }
        }

        [DataContract]
        public class Rating
        {
            public Rating() { }

            public Rating(string key,double value)
            {
                this.Value = value;
                this.Name = Player.RatingMap[key];
            }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public double Value { get; set; }

            [DataMember]
            public double BPlusValue { get { return Value * 1.1; } set { } }

            [DataMember]
            public double AValue { get { return Value * 1.2; } set { } }

            [DataMember]
            public double APlusValue { get { return Value * 1.25; } set { } }
        }

        public static void Report(MaddenTable table)
        {
            var firstNames = table.lRecords.Select(mr => mr["PFNA"]).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            var lastNames = table.lRecords.Select(mr => mr["PLNA"]).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
            var groups = table.lRecords.Where(mr => PositionGroups.ContainsKey(mr["RPGP"].ToInt32())).GroupBy(mr => PositionGroups[mr["RPGP"].ToInt32()]);
            var keys = Player.RatingMap.Keys.ToArray();

            List<PositionGroup> sizes = new List<PositionGroup>();

            foreach (var group in groups)
            {
                sizes.Add(new PositionGroup
                {
                    Position = group.Key,
                    Height = group.Average(mr => mr["PHGT"].ToInt32()),
                    Weight = group.Average(mr => mr["PWGT"].ToInt32() + 160),
                    Ratings = keys.Select(key => new Rating(key, group.Average(mr => mr[key].ToInt32()))).ToArray(),
                    TopRatings = keys.Select(key => new Rating(key, group.Where(mr => mr["RCRK"].ToInt32() <= 250).Average(mr => mr[key].ToInt32()))).ToArray()
                });
            }

            PositionSizes = sizes;
            sizes.ToJsonFile("ratings.json");            
        }

        public static void Init()
        {
            if (!initRun)
            {
                lock (initLock)
                {
                    if (!initRun)
                    {
                        // QB
                        NumberLookup.Add(0, new PositionNumbers(0, Create(1, 19)));

                        // HB
                        NumberLookup.Add(1, new PositionNumbers(1, Create(1, 9), Create(20, 49), Create(1, 9), Create(20, 39), Create(1, 9), Create(20, 39), Create(1, 9), Create(20, 39)));

                        //FB
                        NumberLookup.Add(2, new PositionNumbers(2, Create(20, 49)));

                        //WR
                        NumberLookup.Add(3, new PositionNumbers(3, Create(1, 9), Create(1, 19), Create(1, 9), Create(1, 19), Create(1, 19), Create(1, 19), Create(80, 89), Create(80, 89), Create(80, 89), Create(80, 89), Create(80, 89), Create(80, 89), Create(80, 89), Create(20, 39)));

                        //TE
                        NumberLookup.Add(4, new PositionNumbers(4, Create(11, 19), Create(80, 89), Create(80, 89), Create(11, 19), Create(80, 89), Create(80, 89), Create(40, 49)));

                        // OL
                        NumberLookup.Add(5, new PositionNumbers(5, Create(50, 79)) { ReplaceOnlyIfDuplicate = true });
                        NumberLookup[6] = NumberLookup[5];
                        NumberLookup[7] = NumberLookup[5];
                        NumberLookup[8] = NumberLookup[5];
                        NumberLookup[9] = NumberLookup[5];

                        // P/K
                        NumberLookup.Add(19, new PositionNumbers(19, Create(10, 49), Create(80, 99)) { ReplaceOnlyIfDuplicate = true });
                        NumberLookup[20] = NumberLookup[19];

                        // DE
                        NumberLookup.Add(10, new PositionNumbers(10, Create(1, 49), Create(70, 79), Create(90, 99), Create(70, 79), Create(90, 99), Create(70, 99), Create(80, 89)));
                        NumberLookup[11] = NumberLookup[10];

                        // DT
                        NumberLookup.Add(12, new PositionNumbers(12, Create(1, 49), Create(60, 69), Create(70, 79), Create(90, 99), Create(70, 79), Create(90, 99), Create(70, 99)));

                        // LB
                        NumberLookup.Add(13, new PositionNumbers(13, Create(1, 59), Create(90, 99), Create(50, 59)));
                        NumberLookup[14] = NumberLookup[13];
                        NumberLookup[15] = NumberLookup[13];

                        // DB
                        NumberLookup.Add(16, new PositionNumbers(1, Create(1, 19), Create(20, 39), Create(20, 39), Create(1, 19), Create(20, 39), Create(20, 39), Create(40, 49)));
                        NumberLookup[17] = NumberLookup[16];
                        NumberLookup[18] = NumberLookup[16];
                        initRun = true;
                    }
                }
            }
        }

        public static void CreateEmpty(int position)
        {
            NumberLookup.Add(position, new PositionNumbers(position));
        }

        public static Tuple<int, int> Create(int low, int high)
        {
            return new Tuple<int, int>(low, high);
        }
    }
}
 