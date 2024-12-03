using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class PlayerDB
    {
        public static SortedSet<Player> PassingLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.PassingYards));
        public static SortedSet<Player> QBRushingLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.RushingYards));
        public static SortedSet<Player> RushingLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.RushingYards));
        public static SortedSet<Player> ReceptionLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.Receptions));
        public static SortedSet<Player> ReceivingYardsLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.ReceivingYards));
        public static SortedSet<Player> PancakesLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.Pancakes));
        public static SortedSet<Player> TacklesLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.TotalTackles));
        public static SortedSet<Player> SackLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.Sacks));
        public static SortedSet<Player> IntLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.Interceptions));
        public static SortedSet<Player> PKLeaders = new SortedSet<Player>(new PlayerPointsComparer());
        public static SortedSet<Player> PuntLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.PuntYds));
        public static SortedSet<Player> KRLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.KRYds));
        public static SortedSet<Player> PRLeaders = new SortedSet<Player>(new PlayerStatComparer(PlayerStats.PRYds));
        public static SortedSet<Player> AllPurposeLeaders = new SortedSet<Player>(new PlayerStatComparer(p => p.CurrentYearStats.AllPurposeYards));
        public static Dictionary<int, List<Player>> Rosters;
        public static Dictionary<int, Player> Players;
        public static int CurrentYear;

        public static List<Player> GetStarters(int teamId)
        {
            var result = new List<Player>();
            foreach (var group in Utility.GetPositionGroups())
            {
                result.AddRange(Rosters[teamId].Where(p => group.Take(group.Length - 1).Contains(p.Position)).OrderByDescending(p => p.Ovr).Take(group.Last()));
            }
            return result;
        }

        public static void ToLeaderboardCsv(int top)
        {
            using (var tw = new StreamWriter("./Archive/Reports/leaders.csv", false))
            {
                tw.WriteLine("No,Name,PlayerClass,Position,Height,Weight,Stat1,Stat2,Stat3,Stat4,Stat5,Stat6,TableIdx,Team,TeamId");
                foreach (var p in PassingLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 0, new string[] { PlayerStats.PassAttempts, PlayerStats.Completions, PlayerStats.PassingYards, PlayerStats.PassingTD, PlayerStats.IntThrown, null });
                }

                foreach (var p in QBRushingLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 1, new string[] { PlayerStats.RushAttempts, PlayerStats.RushingYards, PlayerStats.RushingTD, PlayerStats.RushingYdsAfterContact, PlayerStats.BrokenTackles, PlayerStats.Fumbles });
                }

                foreach (var p in RushingLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 2, new string[] { PlayerStats.RushAttempts, PlayerStats.RushingYards, PlayerStats.RushingTD, PlayerStats.RushingYdsAfterContact, PlayerStats.BrokenTackles, PlayerStats.Fumbles });
                }

                foreach (var p in ReceptionLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 3, new string[] { PlayerStats.Receptions, PlayerStats.ReceivingYards, PlayerStats.ReceivingTD, PlayerStats.ReceivingYAC, PlayerStats.Fumbles, null });
                }

                foreach (var p in ReceivingYardsLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 4, new string[] { PlayerStats.Receptions, PlayerStats.ReceivingYards, PlayerStats.ReceivingTD, PlayerStats.ReceivingYAC, PlayerStats.Fumbles, null });
                }

                foreach (var p in PancakesLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 5, new string[] { PlayerStats.Pancakes, PlayerStats.SacksAllowed, null, null, null, null });
                }

                foreach (var p in TacklesLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 6, new string[] { PlayerStats.TotalTackles, PlayerStats.TackleForLoss, PlayerStats.Sacks, PlayerStats.Interceptions, PlayerStats.ForcedFumble, PlayerStats.PassDeflections });
                }

                foreach (var p in SackLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 7, new string[] { PlayerStats.TotalTackles, PlayerStats.TackleForLoss, PlayerStats.Sacks, PlayerStats.Interceptions, PlayerStats.ForcedFumble, PlayerStats.PassDeflections });
                }

                foreach (var p in IntLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 8, new string[] { PlayerStats.TotalTackles, PlayerStats.TackleForLoss, PlayerStats.Sacks, PlayerStats.Interceptions, PlayerStats.ForcedFumble, PlayerStats.PassDeflections });
                }

                foreach (var p in PKLeaders.Take(top))
                {
                    CreateKickStatLine(p, tw, 9);
                }

                foreach (var p in PuntLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 10, new string[] { PlayerStats.NetPuntYards, PlayerStats.PuntYds, PlayerStats.DownInside20, PlayerStats.PuntLong, PlayerStats.PuntsBlocked, PlayerStats.PuntTouchback });
                }

                foreach (var p in KRLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 11, new string[] { PlayerStats.KickReturns, PlayerStats.KRYds, PlayerStats.KRTD, PlayerStats.PuntReturns, PlayerStats.PRYds, PlayerStats.PRTD });
                }

                foreach (var p in PRLeaders.Take(top))
                {
                    CreateStatLine(p, tw, 12, new string[] { PlayerStats.KickReturns, PlayerStats.KRYds, PlayerStats.KRTD, PlayerStats.PuntReturns, PlayerStats.PRYds, PlayerStats.PRTD });
                }
            }
        }

        public static void CreateKickStatLine(Player p, TextWriter tw, int tableIndex)
        {
            var statline = "{0},{1},{2},{3},{4},{5}{6},{7}/{8},{9}/{10},{11}/{12},{13},{14}/{15},{16},{17},{18},{19}";

            tw.WriteLine(string.Format(statline,
                p.Number, p.Name, p.CalculateYear(0), p.PositionName, p.Height.CalculateHgt(), p.Weight, string.Empty,
                    p.CurrentYearStats.GetValueOrDefault(PlayerStats.FGMade, 0), p.CurrentYearStats.GetValueOrDefault(PlayerStats.FGAtt, 0),
                    p.CurrentYearStats.GetValueOrDefault(PlayerStats.FG40to49Made, 0), p.CurrentYearStats.GetValueOrDefault(PlayerStats.FG40to49Att, 0),
                    p.CurrentYearStats.GetValueOrDefault(PlayerStats.FGOver50Made, 0), p.CurrentYearStats.GetValueOrDefault(PlayerStats.FGOver50Att, 0),
                    p.CurrentYearStats.GetValueOrDefault(PlayerStats.FGLong, 0),
                    p.CurrentYearStats.GetValueOrDefault(PlayerStats.XPMade, 0), p.CurrentYearStats.GetValueOrDefault(PlayerStats.XPAtt, 0),
                    p.CurrentYearStats.GetValueOrDefault(PlayerStats.KickOffTouchBack, 0),
                tableIndex, p.Team.Name, p.TeamId));
        }

        public static void CreateStatLine(Player p, TextWriter tw, int tableIndex, string[] key)
        {
            var statline = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}";


            tw.WriteLine(string.Format(statline,
                p.Number, p.Name, p.CalculateYear(0), p.PositionName, p.Height.CalculateHgt(), p.Weight,
                p.CurrentYearStats.GetIntStringValue(key[0]), p.CurrentYearStats.GetIntStringValue(key[1]), p.CurrentYearStats.GetIntStringValue(key[2]),
                p.CurrentYearStats.GetIntStringValue(key[3]), p.CurrentYearStats.GetIntStringValue(key[4]), p.CurrentYearStats.GetIntStringValue(key[5]),
                tableIndex, p.Team.Name, p.TeamId));
        }


        public static Player Find(int teamId, char firstInitial, string LastName)
        {
            var roster = Rosters[teamId];
            var lastNames = roster.Where(p => p.LastName == LastName && p.FirstName.Length > 0 && p.FirstName[0] == firstInitial).ToArray();
            return lastNames.Length == 0 ? null : lastNames.First();
        }

        public static void Create(MaddenDatabase db)
        {
            if (Rosters != null)
                return;

            BowlChampion.Create(db);
            CurrentYear = BowlChampion.CurrentYear; // CurrentYear doesn't need to function on continuation year -ContinuationData.ContinuationYear;

            Rosters = new Dictionary<int, List<Player>>();
            Players = new Dictionary<int, Player>();

            // first get the players
            var table = db.lTables[146].lRecords; // use to trouble shoot specific player stats.Where(mr => mr.lEntries[34].Data.ToInt32() == 4896).ToList();
            for (int i = 0; i < table.Count; i++)
            {
                var record = table[i];

                // don't look at any player with a team id greater than 235
                if (record.GetInt(35) > 235 && record.GetInt(35) < 901)
                    continue;

                var player = new Player
                {
                    Year = record.GetInt(8),
                    FirstName = record.GetData(15),
                    LastName = record.GetData(16),
                    Acc = record.GetInt(29),
                    Id = record.GetInt(34),
                    TeamId = record.GetInt(35).GetRealTeamId(),
                    OriginalPlayerId = record.GetInt(36),
                    Spd = record.GetInt(39),
                    IsRedShirt = record.GetInt(40) == 2,
                    Agl = record.GetInt(54),
                    Hand = record.GetInt(78),
                    Number = record.GetInt(79),
                    Str = record.GetInt(102),
                    Ovr = record.GetInt(103),
                    Awr = record.GetInt(104),
                    Height = record.GetInt(122),
                    Weight = record.GetInt(125) + 160,
                    GamesPlayed = record.GetInt(148),
                    Position = record.GetInt(114),
                    City = record.GetInt(33)
                };

                // add player to the rosters
                if (player.TeamId == 1023)
                    continue;

                Players[player.Id] = player;

                List<Player> roster;
                if (!Rosters.TryGetValue(player.TeamId, out roster))
                {
                    roster = new List<Player>();
                    Rosters[player.TeamId] = roster;
                }

                roster.Add(player);
            }


            // add stats for all players
            AddReturnTeamStats(db);
            AddDefensiveStats(db);
            AddOffensiveStats(db);
            AddKickingStats(db);
            AddOLStats(db);
            AddAllPurposeStats(db);
        }

        public static void AddAllPurposeStats(MaddenDatabase db)
        {

        }

        public static void AddOLStats(MaddenDatabase db)
        {
            AddStats(db, 87, 0, 2,
                new Tuple<string, int, Func<int, int>>[] { MakeTuple(PlayerStats.OLGamesplayed, 5), MakeTuple(PlayerStats.SacksAllowed, 4), MakeTuple(PlayerStats.Pancakes, 3) },
                (p) =>
                {          // add pancakes
                    if (p[PlayerStats.Pancakes] > 0)
                        PancakesLeaders.Add(p);
                });

        }

        public static void AddStats(MaddenDatabase db, int tableIndex, int playerIdIndex, int yearIndex, Tuple<string, int, Func<int, int>>[] keys, Action<Player> leaderAction)
        {
            var table = db.lTables[tableIndex];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var playerId = record.GetInt(playerIdIndex);

                // not a player found in the players db
                if (Players.ContainsKey(playerId) == false)
                    continue;

                var year = record.GetInt(yearIndex) + ContinuationData.ContinuationYear;
                var player = Players[playerId];

                foreach (var key in keys)
                {
                    var value = record.GetInt(key.Item2);
                    if (key.Item3 != null)
                    {
                        value = key.Item3(value);
                    }
                    player.Add(year, key.Item1, value);
                }

                // add to leaderboard
                if (year == (CurrentYear)) //+ContinuationData.ContinuationYear))
                    leaderAction(player);
            }
        }

        public static void AddReturnTeamStats(MaddenDatabase db)
        {
            var keys = new Tuple<string, int, Func<int, int>>[]{
                MakeTuple(PlayerStats.LongestKR, 2),
                MakeTuple(PlayerStats.LongestPR,3),
                MakeTuple(PlayerStats.KickReturns,5),
                MakeTuple(PlayerStats.PuntReturns,6),
                MakeTuple(PlayerStats.ReturnGamesPlayed,7),
                MakeTuple(PlayerStats.KRTD, 8),
                MakeTuple(PlayerStats.PRTD, 9),
                MakeTuple(PlayerStats.KRYds, 10),
                MakeTuple(PlayerStats.PRYds, 11)};
            AddStats(db, 84, 0, 4, keys, AddReturnLeaders);
        }

        public static int GetSeasonRushingYardsTransform(int value)
        {
            var result = value > 10000 ? value - 0x4000 : value;
            return result;
        }

        public static int GetSeasonOffensiveYardsTransform(int value)
        {
            var result = value > 10000 ? value - 32768 : value;
            return result;
        }

        private static Tuple<string, int, Func<int, int>> MakeTuple(string a, int b)
        {
            return MakeTuple(a, b, null);
        }

        private static Tuple<string, int, Func<int, int>> MakeTuple(string a, int b, Func<int, int> transform)
        {
            return new Tuple<string, int, Func<int, int>>(a, b, transform);
        }

        public static void AddReturnLeaders(Player player)
        {
            // total KR yards
            if (player[PlayerStats.KRYds] > 0)
                KRLeaders.Add(player);

            // total PR yards
            if (player[PlayerStats.PRYds] > 0)
                PRLeaders.Add(player);
        }

        public static void AddDefensiveLeaders(Player player)
        {
            // add tackles
            if (player[PlayerStats.Tackles] > 0 || player[PlayerStats.AssistedTackles] > 0)
            {
                TacklesLeaders.Add(player);
            }

            // sacks
            if (player[PlayerStats.Sacks] > 0)
            {
                SackLeaders.Add(player);
            }

            // ints
            if (player[PlayerStats.Interceptions] > 0)
                IntLeaders.Add(player);
        }

        public static void AddOffensiveLeaders(Player player)
        {
            // adding passing leaders
            if (player[PlayerStats.PassingYards] > 0)
                PassingLeaders.Add(player);

            // add rushing leaders
            if (player[PlayerStats.RushingYards] > 0 && player.Position != 0)
                RushingLeaders.Add(player);
            else if (player[PlayerStats.RushingYards] > 0)
                QBRushingLeaders.Add(player);

            // add receiving leaders
            if (player[PlayerStats.ReceivingYards] > 0 || player[PlayerStats.Receptions] > 0)
            {
                ReceivingYardsLeaders.Add(player);
                ReceptionLeaders.Add(player);
            }
        }

        public static void AddKickingLeaders(Player player)
        {
            // place kick points = XP*1 + FGMade*3
            if (player.Points > 0 && (player[PlayerStats.FGAtt] > 0 || player[PlayerStats.XPAtt] > 0))
                PKLeaders.Add(player);

            // total punt yards
            if (player[PlayerStats.PuntYds] > 0)
                PuntLeaders.Add(player);
        }

        public static void AddDefensiveStats(MaddenDatabase db)
        {
            var keys = new Tuple<string, int, Func<int, int>>[]
            {
                MakeTuple(PlayerStats.Sacks,8),
                MakeTuple(PlayerStats.Tackles,5),
                MakeTuple(PlayerStats.Interceptions,11),
                MakeTuple(PlayerStats.IntRetYds,19),
                MakeTuple(PlayerStats.FumRecYds,18),
                MakeTuple(PlayerStats.DefTD,17),
                MakeTuple(PlayerStats.AssistedTackles,16),
                MakeTuple(PlayerStats.Slft,15),
                MakeTuple(PlayerStats.HalfSacks,14),
                MakeTuple(PlayerStats.FumRecYds,13),
                MakeTuple(PlayerStats.DefGP,12),
                MakeTuple(PlayerStats.LongIntRet,3),
                MakeTuple(PlayerStats.Safeties,4),
                MakeTuple(PlayerStats.PassDeflections,6),
                MakeTuple(PlayerStats.ForcedFumble,7),
                MakeTuple(PlayerStats.BlockedKicks,9),
                MakeTuple(PlayerStats.TackleForLoss,10)
            };

            AddStats(db, 82, 0, 2, keys, AddDefensiveLeaders);
        }

        public static void AddOffensiveStats(MaddenDatabase db)
        {
            var keys = new Tuple<string, int, Func<int, int>>[]
            {
                MakeTuple(PlayerStats.LongestPass,3),
                MakeTuple(PlayerStats.LongestRush,4),
                MakeTuple(PlayerStats.Receptions,6),
                MakeTuple(PlayerStats.SacksTaken,7),
                MakeTuple(PlayerStats.PassingYards,8,GetSeasonOffensiveYardsTransform),
                MakeTuple(PlayerStats.RushingYards,10,GetSeasonRushingYardsTransform),
                MakeTuple(PlayerStats.ReceivingYards,9,GetSeasonRushingYardsTransform),
                MakeTuple(PlayerStats.ReceivingYAC,11),
                MakeTuple(PlayerStats.PassingTD,12),
                MakeTuple(PlayerStats.ReceivingTD,13),
                MakeTuple(PlayerStats.RushingTD,14),
                MakeTuple(PlayerStats.RushingYdsAfterContact,15),
                MakeTuple(PlayerStats.Completions,16),
                MakeTuple(PlayerStats.IntThrown,17),
                MakeTuple(PlayerStats.OffGamesPlayed,18),
                MakeTuple(PlayerStats.PassAttempts,20),
                MakeTuple(PlayerStats.RushAttempts,21),
                MakeTuple(PlayerStats.BrokenTackles,22),
                MakeTuple(PlayerStats.Fumbles,23),
                MakeTuple(PlayerStats.RushOver20,24)
            };
            AddStats(db, 86, 0, 5, keys, AddOffensiveLeaders);
        }

        public static void AddKickingStats(MaddenDatabase db)
        {
            var keys = new Tuple<string, int, Func<int, int>>[]
            {
                MakeTuple(PlayerStats.FGLong,2),
            MakeTuple(PlayerStats.PuntLong,3),
            MakeTuple(PlayerStats.XPAtt,5),
            MakeTuple(PlayerStats.FGAtt,6),
            MakeTuple(PlayerStats.PuntYds,7),
            MakeTuple(PlayerStats.FGUnder30Att,8),
            MakeTuple(PlayerStats.XPBlocked,9),
            MakeTuple(PlayerStats.FGBlocked,10),
            MakeTuple(PlayerStats.FGUnder30Made,11),
            MakeTuple(PlayerStats.KickOffTouchBack,12),
            MakeTuple(PlayerStats.PuntTouchback,13),
            MakeTuple(PlayerStats.FG30to39Att,14),
            MakeTuple(PlayerStats.FG30to39Made,15),
            MakeTuple(PlayerStats.FG40to49Att,16),
            MakeTuple(PlayerStats.FG40to49Made,17),
            MakeTuple(PlayerStats.FGOver50Att,18),
            MakeTuple(PlayerStats.FGOver50Made,19),
            MakeTuple(PlayerStats.Kickoffs,20),
            MakeTuple(PlayerStats.PuntsBlocked,21),
            MakeTuple(PlayerStats.XPMade,22),
            MakeTuple(PlayerStats.FGMade,23),
            MakeTuple(PlayerStats.KickGamesPlayed,24),
            MakeTuple(PlayerStats.Spat,25),
            MakeTuple(PlayerStats.DownInside20,26),
            MakeTuple(PlayerStats.NetPuntYards,27),
            };

            AddStats(db, 83, 0, 4, keys, AddKickingLeaders);
        }
    }

    public class PlayerStats : Dictionary<string, int>
    {
        #region Stat Names
        // special teams kicking
        public const string Kickoffs = "KOs";
        public const string PuntsBlocked = "PuntBlk";
        public const string XPMade = "XPMade";
        public const string FGMade = "FGMade";
        public const string KickGamesPlayed = "KickGP";
        public const string Spat = "spat"; // what is this?
        public const string DownInside20 = "Inside20";
        public const string NetPuntYards = "NetPuntYds";
        public const string XPBlocked = "XPBlk";
        public const string FGBlocked = "FGBlk";
        public const string KickOffTouchBack = "KOTB";
        public const string PuntTouchback = "PuntTB";
        public const string FGUnder30Att = "FGUnder30Att";
        public const string FG30to39Att = "FG3039Att";
        public const string FG40to49Att = "FG4049Att";
        public const string FGOver50Att = "FGOver50Att";
        public const string FGUnder30Made = "FGUnder30";
        public const string FG30to39Made = "FG3039";
        public const string FG40to49Made = "FG4049";
        public const string FGOver50Made = "FGOver50";
        public const string FGAtt = "FGAtt";
        public const string PuntLong = "PuntLong";
        public const string FGLong = "FGLong";
        public const string PuntYds = "PuntYds";
        public const string XPAtt = "XPAtt";

        // passing
        public const string LongestPass = "PassLong";
        public const string PassingYards = "PassYds";
        public const string SacksTaken = "Sacked";
        public const string PassingTD = "PassTD";
        public const string Completions = "Comp";
        public const string IntThrown = "IntThrown";
        public const string OffGamesPlayed = "OffGP";
        public const string PassAttempts = "PassAtt";

        //rushing
        public const string RushOver20 = "RushOver20";
        public const string Fumbles = "Fumbles";
        public const string BrokenTackles = "BrokenTkl";
        public const string RushAttempts = "RushAtt";
        public const string RushingYards = "RushYds";
        public const string LongestRush = "RushLong";
        public const string RushingTD = "RushTD";
        public const string RushingYdsAfterContact = "RushYac";

        // receiving
        public const string Receptions = "Rec";
        public const string ReceivingYards = "RecYds";
        public const string ReceivingTD = "RecTD";
        public const string ReceivingYAC = "RecYac";
        public const string LongestReception = "RecLong";

        // OL
        public const string Pancakes = "Pancakes";
        public const string SacksAllowed = "SA";
        public const string OLGamesplayed = "OLGP";

        // def
        public const string TotalTackles = "TotalTkl";
        public const string Tackles = "Tkl";
        public const string Sacks = "Sack";
        public const string Interceptions = "Int";
        public const string TackleForLoss = "TFL";
        public const string BlockedKicks = "BlockKick";
        public const string ForcedFumble = "FFum";
        public const string PassDeflections = "PassDefl";
        public const string Safeties = "Safeties";
        public const string LongIntRet = "LongIntRet";
        public const string DefGP = "DefGP";
        public const string FumbleRec = "FumRec";
        public const string HalfSacks = "HalfSack";
        public const string Slft = "Slft"; // what is this?
        public const string AssistedTackles = "AssTkl";
        public const string DefTD = "DefTD";
        public const string FumRecYds = "FRYds";
        public const string IntRetYds = "IntRetYds";
        public const string FumblesReturnedForTD = "FumblesReturnedForTD";
        public const string IntReturnedForTD = "IntReturnedForTD";

        // special teams returning
        public const string KRYds = "KRYds";
        public const string PRYds = "PRYds";
        public const string LongestPR = "PRLong";
        public const string LongestKR = "KRLong";
        public const string PuntReturns = "PR";
        public const string KickReturns = "KR";
        public const string KRTD = "KRTD";
        public const string PRTD = "PRTD";
        public const string ReturnGamesPlayed = "RETGP";

        #endregion
        public int AllPurposeYards
        {
            get
            {
                return this.GetValueOrDefault(RushingYards, 0) +
                    this.GetValueOrDefault(ReceivingYards, 0) +
                    this.GetValueOrDefault(KRYds, 0) +
                    this.GetValueOrDefault(PRYds, 0) +
                    this.GetValueOrDefault(IntRetYds, 0) +
                    this.GetValueOrDefault(FumRecYds, 0);
            }
        }

        public int TotalTouchDowns
        {
            get
            {
                return this.GetValueOrDefault(KRTD, 0) + this.GetValueOrDefault(PRTD, 0) + this.GetValueOrDefault(DefTD, 0) + this.GetValueOrDefault(RushingTD, 0) + this.GetValueOrDefault(ReceivingTD, 0);
            }
        }

        public int Points
        {
            get
            {
                return 6 * this.TotalTouchDowns + 3 * this.GetValueOrDefault(FGMade, 0) + this.GetValueOrDefault(XPMade, 0);
            }
        }

        public int PlayerId { get; set; }
        public Player Player
        {
            get
            {
                Player player;
                PlayerDB.Players.TryGetValue(this.PlayerId, out player);
                return player;
            }
        }

        public int Year { get; set; }
        public string GameKey { get; set; }
        public static List<PlayerStats> OffensiveGamePerformances = new List<PlayerStats>();
        public string GetIntStringValue(string key)
        {
            if (key == null)
                return string.Empty;

            return this.GetValueOrDefault(key, 0).ToString();
        }

        public int GetIntValue(string key)
        {
            return this.GetValueOrDefault(key, 0);
        }
    }

    public class Player
    {
        public Dictionary<int, PlayerStats> Stats { get; set; }

        public Player()
        {
            Stats = new Dictionary<int, PlayerStats>();
        }

        public int TotalTouchDowns
        {
            get
            {
                return this.Stats[PlayerDB.CurrentYear].TotalTouchDowns;
            }
        }

        public int Points
        {
            get
            {
                return this.Stats[PlayerDB.CurrentYear].Points;
            }
        }

        public int this[string name]
        {
            get
            {
                return GetStatsForYear(PlayerDB.CurrentYear).GetValueOrDefault(name, 0);
            }
            set
            {
                if (value != 0)
                {
                    GetStatsForYear(PlayerDB.CurrentYear)[name] = value;
                }
            }
        }

        public PlayerStats CurrentYearStats
        {
            get
            {
                return GetStatsForYear(PlayerDB.CurrentYear);
            }
        }

        public PlayerStats GetStatsForYear(int year)
        {
            PlayerStats book;
            if (!this.Stats.TryGetValue(year, out book))
            {
                book = new PlayerStats();
                this.Stats.Add(year, book);
            }

            return book;
        }

        public void Add(int year, string stat, int value)
        {
            PlayerStats book = GetStatsForYear(year);
            book.Year = year;
            book.PlayerId = this.Id;
            if (value != 0)
            {
                if (stat == PlayerStats.Tackles || stat == PlayerStats.AssistedTackles)
                {
                    var tt = book.GetValueOrDefault(PlayerStats.TotalTackles, 0);
                    tt += value;
                    book[PlayerStats.TotalTackles] = tt;
                }
                else if (stat == PlayerStats.HalfSacks)
                {
                    var sacks = book.GetValueOrDefault(PlayerStats.Sacks, 0);
                    sacks += 10 * (value / 2);
                    sacks += 5 * (value % 2);
                    book[PlayerStats.Sacks] = sacks;
                }
                else if (stat == PlayerStats.Sacks)
                {
                    var sacks = book.GetValueOrDefault(PlayerStats.Sacks, 0);
                    value *= 10;
                    book[stat] = value + sacks;
                    return;
                }
                book[stat] = value;
            }
        }

        public int Acc { get; set; }
        public int Spd { get; set; }
        public int Agl { get; set; }
        public int Str { get; set; }
        public int Ovr { get; set; }
        public int Awr { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int GamesPlayed { get; set; }
        public int City { get; set; }
        public int Hand { get; set; }
        public int Number { get; set; }
        public bool IsRedShirt { get; set; }
        public int OriginalPlayerId { get; set; }
        public int Year { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get { return string.Format("{0} {1}", FirstName, LastName); } }
        public string PositionName { get { return this.Position.ToPositionName(); } }
        public int Position { get; set; }
        public int TeamId { get; set; }
        public int Id { get; set; }
        public Team Team { get { return Team.Teams[this.TeamId]; } }
    }

    public class PlayerStatComparer : IComparer<Player>
    {
        private string key;
        private Func<Player, int> func;

        public PlayerStatComparer(string key)
        {
            this.key = key;
        }

        public PlayerStatComparer(Func<Player, int> func)
        {
            this.func = func;
        }

        public int Compare(Player x, Player y)
        {
            if (func == null)
            {
                return x[key] >= y[key] ? -1 : 1;
            }
            else
            {
                return func(x) >= func(y) ? -1 : 1;
            }
        }
    }

    public class PlayerPointsComparer : IComparer<Player>
    {
        public int Compare(Player x, Player y)
        {
            if (x.Points > y.Points)
                return -1;

            if (x.Points < y.Points)
                return 1;

            return 0;
        }
    }
}