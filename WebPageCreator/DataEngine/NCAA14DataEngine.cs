using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class NCAA14DataEngine : IDataEngine
    {
        public NCAA14DataEngine(MaddenDatabase db)
        {
            this.MaddenDatabase = db;
        }

        public MaddenDatabase MaddenDatabase { get; }

        public bool IsSeasonOver()
        {
            // check to see if the season is still going on
            // BUGBUG if the NCG has been played, but the week hasn't advanced this info is incorrect, but that's probably ok
            var record = MaddenDatabase.lTables[161].lRecords.OrderByDescending(mr => mr.lEntries[12].Data.ToInt32()).Take(1).First();

            // if the score is 0-0 the season is not over
            return !(record.lEntries[1].Data.ToInt32() == 0 && record.lEntries[2].Data.ToInt32() == 0);
        }

        public int ReadBowlChampions(bool didNotEnterBowlChampLoop, int currentYear, Dictionary<string, BowlChampion> bowlChampions)
        {
            var table = this.MaddenDatabase.lTables[0];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                didNotEnterBowlChampLoop = false;
                var record = table.lRecords[i];
                var bc = new BowlChampion
                {
                    TeamId = record.GetInt(0).GetRealTeamId(),
                    Year = record.GetInt(1) + ContinuationData.ContinuationYear,
                    BowlId = record.GetInt(2)
                };

                if (Bowl.BowlIdOverrides.ContainsKey(bc.BowlId) && Bowl.BowlIdOverrides[bc.BowlId].Item2 <= bc.Year)
                    bc.BowlId = Bowl.BowlIdOverrides[bc.BowlId].Item1;


                if (!bowlChampions.ContainsKey(bc.GetKey()))
                {
                    bowlChampions.Add(bc.GetKey(), bc);
                }

                currentYear = Math.Max(currentYear, bc.Year);
            }

            return currentYear;
        }

        public Dictionary<int, TeamSchedule> CreateTeamSchedule(bool isPreseason)
        {
            var TeamSchedules = new Dictionary<int, TeamSchedule>();
            var table = this.MaddenDatabase.lTables[113];

            for (int i = 0; i < table.Table.currecords; i++)
            {
                var teamId = table.lRecords[i].lEntries[2].Data.ToInt32().GetRealTeamId();

                // get or create team schedule
                if (TeamSchedules.TryGetValue(teamId, out var teamSchedule) == false)
                {
                    teamSchedule = new TeamSchedule();
                    teamSchedule.TeamId = teamId;
                    TeamSchedules[teamId] = teamSchedule;
                }

                // now add to a team schedule
                var game = new Game
                {
                    IsHomeGame = table.lRecords[i].lEntries[0].Data.ToInt32() > 0,
                    OpponentId = table.lRecords[i].lEntries[1].Data.ToInt32().GetRealTeamId(),
                    GameNumber = table.lRecords[i].lEntries[3].Data.ToInt32(),
                    Week = table.lRecords[i].lEntries[4].Data.ToInt32(),
                    TeamId = teamId
                };

                if (game.Week > 14 && isPreseason)
                    continue;

                List<Game> gamesForWeek;
                if (!teamSchedule.TryGetValue(game.Week, out gamesForWeek))
                {
                    gamesForWeek = new List<Game>();
                    teamSchedule.Add(game.Week, gamesForWeek);
                }

                gamesForWeek.Add(game);
            }

            return TeamSchedules;
        }

        public Dictionary<string, Bowl> CreateBowlTable()
        {
            var bowls = new Dictionary<string, Bowl>();
            var table = this.MaddenDatabase.lTables[129];
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

                if (Bowl.BowlIdOverrides.ContainsKey(bowl.Id) && Bowl.BowlIdOverrides[bowl.Id].Item2 <= BowlChampion.CurrentYear)
                    bowl.Id = Bowl.BowlIdOverrides[bowl.Id].Item1;

                if (bowl.Game != 255)
                    bowls.Add(bowl.Key, bowl);
            }

            var cureBowl = new Bowl
            {
                Id = Bowl.CureBowl,
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
                Id = Bowl.MyrtleBeachBowl,
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
                Id = Bowl.ArizonaBowl,
                Name = "Arizona Bowl",
                Week = 18,
                Game = 45,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var saluteVetsBowl = new Bowl
            {
                Id = Bowl.SaluteVetsBowl,
                Name = "Salute to Veterans Bowl",
                Week = 18,
                Game = 51,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var xboxBowl = new Bowl
            {
                Id = Bowl.XboxBowl,
                Name = "Xbox Bowl",
                Week = 18,
                Game = 52,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var venturesBowl = new Bowl
            {
                Id = Bowl.MobileAlabamaBowl,
                Name = "68 Ventures Bowl",
                Week = 18,
                Game = 46,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var fgsChampionship = new Bowl
            {
                Id = Bowl.FGSChampionship,
                Name = "FGS Championship Game",
                Week = 20,
                Game = 53,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };



            var cfp8v9 = new Bowl
            {
                Id = Bowl.CFB8v9,
                Name = "CFP 1st Round 8v9",
                Week = 18,
                Game = 47,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var cfp7v10 = new Bowl
            {
                Id = Bowl.CFB7v10,
                Name = "CFP 1st Round 7v10",
                Week = 18,
                Game = 48,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var cfp6v11 = new Bowl
            {
                Id = Bowl.CFB6v11,
                Name = "CFP 1st Round 6v11",
                Week = 18,
                Game = 49,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            var cfp5v12 = new Bowl
            {
                Id = Bowl.CFB5v12,
                Name = "CFP 1st Round 5v12",
                Week = 18,
                Game = 50,
                ConferenceTieInId1 = 0,
                ConferenceTieInId2 = 1,
                ConferenceTieInSelection1 = 0,
                ConferenceTieInSelection2 = 1,
            };

            bowls.Add(cureBowl.Key, cureBowl);
            bowls.Add(mbBowl.Key, mbBowl);
            bowls.Add(arizonaBowl.Key, arizonaBowl);
            bowls.Add(venturesBowl.Key, venturesBowl);
            bowls.Add(saluteVetsBowl.Key, saluteVetsBowl);
            bowls.Add(xboxBowl.Key, xboxBowl);
            //            Bowls.Add(fgsChampionship.Key, fgsChampionship);
            bowls.Add(cfp8v9.Key, cfp8v9);
            bowls.Add(cfp7v10.Key, cfp7v10);
            bowls.Add(cfp6v11.Key, cfp6v11);
            bowls.Add(cfp5v12.Key, cfp5v12);
            return bowls;
        }

        public List<AllAmerican> CreateAllAmericans()
        {
            return MaddenTable.FindMaddenTable(MaddenDatabase.lTables, "AAPL").lRecords.Where(mr => mr["SEYR"].ToInt32() == BowlChampion.DynastyFileYear && PlayerDB.Players.ContainsKey(mr["PGID"].ToInt32())).Select(mr =>
                new AllAmerican
                {
                    AllAmericanTeam = (AllAmericanTeam)mr["TTYP"].ToInt32(),
                    ReturningAllAmerican = mr["ARET"].ToInt32() != 0,
                    Position = mr["PPOS"].ToInt32(),
                    PlayerId = mr["PGID"].ToInt32(),
                    ConferenceId = mr["CGID"].ToInt32()
                }).OrderBy(p => p.ConferenceId).ThenBy(p => p.AllAmericanTeam).ThenBy(p => p.Position).ToList();
        }

        public void CreatePlayers(Dictionary<int, List<Player>> Rosters, Dictionary<int, Player> Players)
        {
            // first get the players
            var table = MaddenDatabase.lTables[146].lRecords; // use to trouble shoot specific player stats.Where(mr => mr.lEntries[34].Data.ToInt32() == 4896).ToList();
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
                    City = record.GetInt(33),
                    Face = record["PGHE"].ToInt32(),
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
        }
        #region stat engine
        public void ReadStats()
        {

            // add stats for all players
            AddReturnTeamStats(MaddenDatabase);
            AddDefensiveStats(MaddenDatabase);
            AddOffensiveStats(MaddenDatabase);
            AddKickingStats(MaddenDatabase);
            AddOLStats(MaddenDatabase);
            AddAllPurposeStats(MaddenDatabase);

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
                        PlayerDB.PancakesLeaders.Add(p);
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
                if (PlayerDB.Players.ContainsKey(playerId) == false)
                    continue;

                var year = record.GetInt(yearIndex) + ContinuationData.ContinuationYear;
                var player = PlayerDB.Players[playerId];

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
                if (year == (PlayerDB.CurrentYear)) //+ContinuationData.ContinuationYear))
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
            AddStats(db, 84, 0, 4, keys, PlayerDB.AddReturnLeaders);
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

            AddStats(db, 82, 0, 2, keys, PlayerDB.AddDefensiveLeaders);
        }

        public static void AddOffensiveStats(MaddenDatabase db)
        {
            var keys = new Tuple<string, int, Func<int, int>>[]
            {
                MakeTuple(PlayerStats.LongestPass,3),
                MakeTuple(PlayerStats.LongestRush,4),
                MakeTuple(PlayerStats.Receptions,6),
                MakeTuple(PlayerStats.SacksTaken,7),
                MakeTuple(PlayerStats.PassingYards,8,PlayerDB.GetSeasonOffensiveYardsTransform),
                MakeTuple(PlayerStats.RushingYards,10,PlayerDB.GetSeasonRushingYardsTransform),
                MakeTuple(PlayerStats.ReceivingYards,9,PlayerDB.GetSeasonRushingYardsTransform),
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
            AddStats(db, 86, 0, 5, keys, PlayerDB.AddOffensiveLeaders);
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

            AddStats(db, 83, 0, 4, keys, PlayerDB.AddKickingLeaders);
        }
        private static Tuple<string, int, Func<int, int>> MakeTuple(string a, int b)
        {
            return MakeTuple(a, b, null);
        }

        private static Tuple<string, int, Func<int, int>> MakeTuple(string a, int b, Func<int, int> transform)
        {
            return new Tuple<string, int, Func<int, int>>(a, b, transform);
        }

        #endregion
    }
}