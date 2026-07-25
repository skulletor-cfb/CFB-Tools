using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class NCAA14DataEngine : IDataEngine
    {
        private Dictionary<int, string> teamNames;

        public NCAA14DataEngine(MaddenDatabase db)
        {
            this.MaddenDatabase = db;
        }

        public MaddenDatabase MaddenDatabase { get; }

        public Dictionary<int, string> TeamNames
        {
            get
            {
                if (teamNames == null || teamNames.Count == 0)
                {
                    try
                    {
                        teamNames = MaddenDatabase.lTables[167].lRecords.ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record["TDNA"]);
                    }
                    catch
                    {
                        teamNames = new Dictionary<int, string>();
                    }
                }

                return teamNames;
            }
        }
        public int CalculateRosterSpots(RecruitClassRanking ranking)
        {
            return 70 - MaddenDatabase.GetTable("PLAY").lRecords.Where(player => player["TGID"].ToInt32().GetRealTeamId() == ranking.TeamId).Count();
        }

        public List<Record> ReadNcaaRecords()
        {
            var AllTimeRecords = new List<Record>();
            var table = MaddenDatabase.lTables[91];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var row = table.lRecords[i];

                var record = new Record
                {
                    Description = row.GetInt(4),
                    Holder = row.GetData(3),
                    Value = row.GetInt(13),
                    Opponent = row.GetData(9),
                    Year = row.GetInt(14)
                };

                NcaaRecord.Reconcile(record);
                AllTimeRecords.Add(record);
            }

            return AllTimeRecords;
        }

        public Dictionary<int, List<Award>> ReadAwards()
        {
            var awards = new Dictionary<int, List<Award>>();
            var table = MaddenDatabase.lTables[71];

            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var awardID = record.GetInt(3);

                List<Award> list;
                if (!awards.TryGetValue(awardID, out list))
                {
                    list = new List<Award>();
                    awards[awardID] = list;
                }

                list.Add(new Award
                {
                    Id = awardID,
                    PlayerId = record.GetInt(0),
                    Rank = record.GetInt(1),
                    Year = record.GetInt(2) + ContinuationData.ContinuationYear
                });
            }

            return awards;
        }

        public Dictionary<int, Recruit> ReadRecruits()
        {
            var RecruitRankings = new Dictionary<int, Recruit>();
            RecruitAllAmericans.GetRecruits(MaddenDatabase.lTables[96], MaddenDatabase.lTables[95]);

            for (int i = 0; i < MaddenDatabase.lTables[96].Table.currecords; i++)
            {
                var record = MaddenDatabase.lTables[96].lRecords[i];
                Recruit recruit = new Recruit
                {
                    RecruitId = MaddenDatabase.lTables[96].lRecords[i].lEntries[53].Data.ToInt32(),
                    FirstName = MaddenDatabase.lTables[96].lRecords[i].lEntries[14].Data,
                    LastName = MaddenDatabase.lTables[96].lRecords[i].lEntries[15].Data,
                    PositionValue = MaddenDatabase.lTables[96].lRecords[i].lEntries[106].Data.ToInt32(),
                    Rank = MaddenDatabase.lTables[96].lRecords[i].lEntries[62].Data.ToInt32(),
                    PositionRank = MaddenDatabase.lTables[96].lRecords[i].lEntries[89].Data.ToInt32(),
                    StarRating = MaddenDatabase.lTables[96].lRecords[i].lEntries[23].Data.ToInt32(),
                    PreScoutOVR = MaddenDatabase.lTables[96].lRecords[i].lEntries[131].Data.ToInt32(),
                    RealOVR = MaddenDatabase.lTables[96].lRecords[i].lEntries[95].Data.ToInt32(),
                    IsAthlete = MaddenDatabase.lTables[96].lRecords[i].lEntries[47].Data.ToInt32() != 0,
                    HometownValue = MaddenDatabase.lTables[96].lRecords[i].lEntries[33].Data.ToInt32(),
                    PositionGroup = MaddenDatabase.lTables[96].lRecords[i]["RPGP"].ToInt32(),
                    PlayerYear = MaddenDatabase.lTables[96].lRecords[i]["PYEA"].ToInt32(),
                    Tendency = MaddenDatabase.lTables[96].lRecords[i]["PTEN"].ToInt32(),
                    State = MaddenDatabase.lTables[96].lRecords[i]["STAT"].ToInt32(),
                };

                RecruitRankings.Add(recruit.RecruitId, recruit);
            }

            try
            {
                for (int i = 0; i < MaddenDatabase.lTables[95].Table.currecords; i++)
                {
                    var id = MaddenDatabase.lTables[95].lRecords[i].lEntries[34].Data.ToInt32();
                    var recruit = RecruitRankings[id];
                    recruit.CommittedTeam = MaddenDatabase.lTables[95].lRecords[i].lEntries[35].Data.ToInt32().GetRealTeamId();
                    recruit.Team1 = MaddenDatabase.lTables[95].lRecords[i].lEntries[6].Data.ToInt32().GetRealTeamId();
                    recruit.Team2 = MaddenDatabase.lTables[95].lRecords[i].lEntries[10].Data.ToInt32().GetRealTeamId();
                    recruit.Team3 = MaddenDatabase.lTables[95].lRecords[i].lEntries[13].Data.ToInt32().GetRealTeamId();
                    RecruitRankings[10000 + recruit.Rank] = recruit;
                }
            }
            catch { }

            return RecruitRankings;
        }

        public Dictionary<int, RecruitClassRanking> ReadRecruitClasses()
        {
            var teamRankings = new Dictionary<int, RecruitClassRanking>();
            for (int i = 0; i < MaddenDatabase.lTables[97].Table.currecords; i++)
            {
                var ranking = new RecruitClassRanking
                {
                    TeamId = MaddenDatabase.lTables[97].lRecords[i].lEntries[4].Data.ToInt32().GetRealTeamId(),
                    Points = MaddenDatabase.lTables[97].lRecords[i].lEntries[5].Data.ToInt32(),
                    Star1 = MaddenDatabase.lTables[97].lRecords[i].lEntries[6].Data.ToInt32(),
                    Star2 = MaddenDatabase.lTables[97].lRecords[i].lEntries[7].Data.ToInt32(),
                    Star3 = MaddenDatabase.lTables[97].lRecords[i].lEntries[8].Data.ToInt32(),
                    Star4 = MaddenDatabase.lTables[97].lRecords[i].lEntries[9].Data.ToInt32(),
                    Star5 = MaddenDatabase.lTables[97].lRecords[i].lEntries[10].Data.ToInt32(),
                };

                teamRankings.Add(ranking.TeamId, ranking);
            }

            for (int i = 0; i < MaddenDatabase.lTables[167].Table.currecords; i++)
            {
                int teamId = MaddenDatabase.lTables[167].lRecords[i].lEntries[40].Data.ToInt32().GetRealTeamId();
                RecruitClassRanking ranking = null;
                if (teamRankings.TryGetValue(teamId, out ranking))
                {
                    ranking.ConferenceId = MaddenDatabase.lTables[167].lRecords[i].lEntries[36].Data.ToInt32();
                    ranking.DivisionId = MaddenDatabase.lTables[167].lRecords[i].lEntries[37].Data.ToInt32();
                    ranking.Wins = MaddenDatabase.lTables[167].lRecords[i].lEntries[61].Data.ToInt32();
                    ranking.Losses = MaddenDatabase.lTables[167].lRecords[i].lEntries[88].Data.ToInt32();
                }
            }

            return teamRankings;
        }

        public Dictionary<int, Stadium> ReadStadiums()
        {
            var stadiums = new Dictionary<int, Stadium>();
            var table = MaddenDatabase.lTables[163];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var stadium = new Stadium
                {
                    Id = record.GetInt(40),
                    Capacity = record.GetInt(63),
                    Name = record.GetData(56)
                };

                stadiums.Add(stadium.Id, stadium);
            }

            stadiums[1023] = new Stadium();
            return stadiums;
        }

        public List<ConferenceChampion> ReadConferenceChamps()
        {
            var cc = new List<ConferenceChampion>();
            var table = MaddenDatabase.lTables[20];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                cc.Add(new ConferenceChampion
                {
                    ConferenceId = record.GetInt(0),
                    TeamId = record.GetInt(1).GetRealTeamId(),
                    Year = record.GetInt(2) + ContinuationData.ContinuationYear
                });
            }

            return cc;
        }

        public void ReadGameStats(Dictionary<string, ScheduledGame> games)
        {
            // do the scoring summary for each game now
            var table = MaddenDatabase.lTables[6];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(5);
                var weekNumber = record.GetInt(6);
                var game = games[ScheduledGame.CreateKey(weekNumber, gameNumber)];

                // add the item to the list
                game.Scores.Add(new GameScore
                {
                    TeamId = record.GetInt(0),
                    Time = record.GetUShort(4),
                    Quarter = record.GetUShort(8),
                    Points = record.GetUShort(9),
                    ScoreType = record.GetUShort(13)
                });

                game.Scores.Last().Parse();
            }

            // box score data for each game
            table = MaddenDatabase.lTables[7];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(1);
                var weekNumber = record.GetInt(2);
                var teamId = record.GetInt(0).GetRealTeamId();
                var game = games[ScheduledGame.CreateKey(weekNumber, gameNumber)];

                //create the box score
                var boxScore = new TeamStat
                {
                    TeamId = teamId,
                    TwoPointConversionAttempts = record.GetInt(3),
                    Turnovers = record.GetInt(4),
                    PassAttempts = record.GetInt(5),
                    RushAttempts = record.GetInt(6),
                    TwoPointConversions = record.GetInt(7),
                    ThirdDownConversions = record.GetInt(8),
                    FourthDownConversions = record.GetInt(9),
                    PuntYards = record.GetInt(14),
                    Penalties = record.GetInt(15),
                    RedZoneFG = record.GetInt(16),
                    IntThrown = record.GetInt(17),
                    PassCompletions = record.GetInt(10),
                    FirstDowns = record.GetInt(11),
                    ThirdDownAttempts = record.GetInt(12),
                    FourthDownAttempts = record.GetInt(13),
                    FumblesLost = record.GetInt(18),
                    PassYards = record.GetInt(19),
                    KRYards = record.GetInt(20),
                    RushTD = record.GetInt(26),
                    OffensiveYards = record.GetInt(30),
                    PRYards = record.GetInt(22),
                    PassTD = record.GetInt(23),
                    RedZoneTD = record.GetInt(24),
                    TimeOfPossesion = record.GetInt(25),
                    Punts = record.GetInt(27),
                    PenaltyYards = record.GetInt(28),
                    TotalYards = record.GetInt(29),
                    RedZoneVisits = record.GetInt(31)
                };

                boxScore.RushYards = (int)((short)record.GetInt(21));

                if (teamId == game.HomeTeamId)
                {
                    game.HomeTeamBoxScore = boxScore;
                }
                else
                {
                    game.AwayTeamBoxScore = boxScore;
                }
            }

            // offensive stats
            table = MaddenDatabase.lTables[4];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(2);
                var weekNumber = record.GetInt(3);
                var playerId = record.GetInt(0);
                var game = games[ScheduledGame.CreateKey(weekNumber, gameNumber)];

                // create the offensive stats for the player
                var player = new PlayerStats { PlayerId = playerId };

                var passingYards = record.GetSignedInt(8, 4096);
                var rushingYards = record.GetSignedInt(10, 2048);
                var rececptions = record.GetInt(6);
                var passTD = record.GetInt(13);
                var rushTD = record.GetInt(15);
                var recTD = record.GetInt(14);
                var receivingYrds = record.GetSignedInt(9, 2048);

                // gaya
                player[PlayerStats.PassingYards] = passingYards;

                // gaat
                player[PlayerStats.PassAttempts] = record.GetInt(22);

                // gacm
                player[PlayerStats.Completions] = record.GetInt(17);

                // gatd
                player[PlayerStats.PassingTD] = passTD;

                // gain
                player[PlayerStats.IntThrown] = record.GetInt(18);

                // guat
                player[PlayerStats.RushAttempts] = record.GetInt(23);

                // guya
                player[PlayerStats.RushingYards] = rushingYards;

                // gutd
                player[PlayerStats.RushingTD] = rushTD;

                // gctd
                player[PlayerStats.ReceivingTD] = recTD;

                // gcca
                player[PlayerStats.Receptions] = rececptions;

                // gcya
                player[PlayerStats.ReceivingYards] = receivingYrds;

                player[PlayerStats.LongestPass] = record["galN"].ToInt32();
                player[PlayerStats.LongestReception] = record["gcrL"].ToInt32();
                player[PlayerStats.LongestRush] = record["gulN"].ToInt32();

                player.GameKey = game.Key;
                game.GamePlayerStats.Add(playerId, player);
                PlayerStats.OffensiveGamePerformances.Add(player);

                if (player.Player != null)
                {
                    TeamRecord.SetNewRecord(TeamRecordKeys.PassTD, passTD, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RushingTD, rushTD, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RecTD, recTD, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.PassYds, passingYards, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RushingYds, rushingYards, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RecYds, receivingYrds, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.Receptions, rececptions, player.Player, game);
                }
            }

            // defensive stats
            table = MaddenDatabase.lTables[1];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(1);
                var weekNumber = record.GetInt(2);
                var playerId = record.GetInt(0);
                var game = games[ScheduledGame.CreateKey(weekNumber, gameNumber)];

                // make sure the player is in the dictionary
                PlayerStats player;
                if (!game.GamePlayerStats.TryGetValue(playerId, out player))
                {
                    player = new PlayerStats { PlayerId = playerId };
                    player.GameKey = game.Key;
                    game.GamePlayerStats.Add(playerId, player);
                }

                var sacks = record.GetInt(8);
                var halfSacks = record.GetInt(13);
                var ints = record.GetInt(11);

                // add the defensive stats
                // gdta
                player[PlayerStats.Tackles] = record.GetInt(5);

                // gdpd
                player[PlayerStats.PassDeflections] = record.GetInt(6);

                //glff
                player[PlayerStats.ForcedFumble] = record.GetInt(7);

                // glsk
                player[PlayerStats.Sacks] = sacks;

                // gdtl
                player[PlayerStats.TackleForLoss] = record.GetInt(10);

                // gsin
                player[PlayerStats.Interceptions] = ints;

                //glfr
                player[PlayerStats.FumbleRec] = record.GetInt(12);

                // glhs
                player[PlayerStats.HalfSacks] = halfSacks;

                // gdht
                player[PlayerStats.AssistedTackles] = record.GetInt(15);

                player[PlayerStats.LongIntRet] = record["gslR"].ToInt32();
                player[PlayerStats.IntRetYds] = record["gsiy"].ToInt32().GetSignedInt(512);
                player[PlayerStats.FumRecYds] = record["glfy"].ToInt32().GetSignedInt(512);
                player[PlayerStats.IntReturnedForTD] = record["gsit"].ToInt32();
                player[PlayerStats.FumblesReturnedForTD] = record["glft"].ToInt32();

                if (player.Player != null)
                {
                    var totalsacks = sacks + (halfSacks > 0 ? (1 + halfSacks / 2) : 0);

                    TeamRecord.SetNewRecord(TeamRecordKeys.Sacks, totalsacks, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.INT, ints, player.Player, game);
                }
            }

            // return starts
            table = MaddenDatabase.lTables[3];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record["SGNM"].ToInt32();
                var weekNumber = record["SEWN"].ToInt32();
                var playerId = record["PGID"].ToInt32();
                var game = games[ScheduledGame.CreateKey(weekNumber, gameNumber)];

                // return game stats
                PlayerStats player;
                if (!game.GamePlayerStats.TryGetValue(playerId, out player))
                {
                    player = new PlayerStats { PlayerId = playerId };
                    player.GameKey = game.Key;
                    game.GamePlayerStats.Add(playerId, player);
                }

                player[PlayerStats.KickReturns] = record["grka"].ToInt32();
                player[PlayerStats.KRTD] = record["grkt"].ToInt32();
                player[PlayerStats.KRYds] = record.GetSignedInt(9, 2048);
                player[PlayerStats.LongestKR] = record["grkL"].ToInt32();

                player[PlayerStats.PuntReturns] = record["grpa"].ToInt32();
                player[PlayerStats.PRTD] = record["grpt"].ToInt32();
                player[PlayerStats.PRYds] = record.GetSignedInt(10, 2048);
                player[PlayerStats.LongestPR] = record["grpL"].ToInt32();
            }
        }

        public Dictionary<int, List<Record>> ReadSchoolRecords(bool recreateUsingRecordsFile)
        {
            var schoolRecords = new Dictionary<int, List<Record>>();

            var table = MaddenDatabase.lTables[159];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var teamId = record.GetInt(7);

                List<Record> records;
                if (schoolRecords.TryGetValue(teamId, out records) == false)
                {
                    records = new List<Record>();
                    schoolRecords.Add(teamId, records);
                }

                var sr = new Record
                {
                    Type = record.GetInt(12),
                    Description = record.GetInt(4),
                    Holder = record.GetData(3),
                    Value = record.GetInt(14),
                    Opponent = record.GetData(10),
                    Year = record.GetInt(15)
                };

                // fix the year of the holder
                if (ContinuationData.UsingContinuationData && !string.IsNullOrWhiteSpace(record["RCDE"]))
                {
                    var holderYear = sr.Holder.Substring(0, 4).ToInt32();
                    var newYear = holderYear + ContinuationData.ContinuationYear;
                    sr.Holder = sr.Holder.Replace(holderYear.ToString(), newYear.ToString());
                }

                records.Add(sr);
            }

            return schoolRecords;
        }

        public Dictionary<string, ScheduledGame> ReadSchedule(bool isPreseason)
        {
            var Schedule = new Dictionary<string, ScheduledGame>();
            var table = MaddenDatabase.lTables[161];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var game = new ScheduledGame
                {
                    PostSeason = table.lRecords[i].lEntries[0].Data.ToInt32(),
                    AwayScore = table.lRecords[i].lEntries[1].Data.ToInt32(),
                    HomeScore = table.lRecords[i].lEntries[2].Data.ToInt32(),
                    AwayTeamId = table.lRecords[i].lEntries[6].Data.ToInt32().GetRealTeamId(),
                    HomeTeamId = table.lRecords[i].lEntries[7].Data.ToInt32().GetRealTeamId(),
                    DynastySeason = table.lRecords[i].lEntries[8].Data.ToInt32(),
                    GameNumber = table.lRecords[i].lEntries[11].Data.ToInt32(),
                    Week = table.lRecords[i].lEntries[12].Data.ToInt32(),
                    Year = table.lRecords[i].lEntries[13].Data.ToInt32(),
                    WentToOvertime = table.lRecords[i].lEntries[15].Data.ToInt32(),
                    GameDay = table.lRecords[i]["GDAT"].ToInt32(),
                    TimeOfDay = table.lRecords[i]["GTOD"].ToInt32(),
                    StadiumId = table.lRecords[i]["SGID"].ToInt32(),
                };

                // check to see if this is an augmented bowl game
                if (!isPreseason && Bowl.TryFindByKey(game.Week, game.GameNumber, out var bowlGame))
                {
                    if (bowlGame.IsAugmentedBowl)
                    {
                        var winner = game.AwayScore > game.HomeScore ?
                            game.AwayTeamId : game.HomeTeamId;

                        BowlChampion.AddBowlChampion(winner, bowlGame.Id);
                    }
                }


                // stadium id
                var stadiumId = table.lRecords[i].lEntries[3].Data.ToInt32();

                // 1023 stadium id means it's probably a game that hasn't been filled in like a bowl or a Conf Champ Game
                if (stadiumId == 1023)
                    continue;

                // find the STAD table and the stadium
                var stadiumTable = MaddenDatabase.lTables.Where(tbl => tbl.Abbreviation == "STAD").SingleOrDefault();
                var stadium = stadiumTable.lRecords.Where(record => record["SGID"].ToInt32() == stadiumId).SingleOrDefault();
                game.GameSite = stadium.lEntries[56].Data;

                TeamSchedule homeTeamSchedule;
                TeamSchedule awayTeamSchedule;

                // check to see if we have a neutral site game only for regular season games
                // don't get to set an overrides for a week with more than 1 game
                if (TeamSchedule.TeamSchedules.TryGetValue(game.HomeTeamId, out homeTeamSchedule) &&
                    TeamSchedule.TeamSchedules.TryGetValue(game.AwayTeamId, out awayTeamSchedule) &&
                    game.Week < 16 &&
                    homeTeamSchedule[game.Week].Count == 1 &&
                    awayTeamSchedule[game.Week].Count == 1)
                {
                    // both teams are marked as home means its a neutral site game
                    if (ScheduledGame.ClassicGameEvaluators.Any(e => e(game)))
                    {
                        game.IsClassicGame = true;
                    }
                    else if (homeTeamSchedule[game.Week][0].IsHomeGame && awayTeamSchedule[game.Week][0].IsHomeGame)
                    {
                        game.IsNeutralSite = true;
                        // check to see if we have an override
                        var overrides = ScheduledGame.StadiumNickNameOverrides;
                        //we have an override, a set of comma delimited settings seperated by semi colon
                        if (overrides != null)
                        {
                            var sections = overrides.Split(';').Where(section => string.IsNullOrWhiteSpace(section) == false).ToArray();
                            var overridenNickNames = sections.Select(s => s.Split(',').ToDictionary(str => str.Split('=')[0], right => right.Split('=')[1])).ToList();
                            var currentNicknames = overridenNickNames.Where(s => s["Stadium"] == stadiumId.ToString() && game.Week < s["BeforeWeek"].ToInt32()).ToList();

                            Dictionary<string, string> stadiumOverride = currentNicknames.Count == 1 ? currentNicknames[0] : null;

                            if (stadiumOverride == null && currentNicknames.Count > 1)
                            {
                                stadiumOverride = currentNicknames.Where(s => s.TryGetValue("RivalryGame", out var value) && value.Contains(Math.Min(game.HomeTeamId, game.AwayTeamId).ToString() + "-" + Math.Max(game.HomeTeamId, game.AwayTeamId).ToString())).FirstOrDefault();
                            }

                            if (stadiumOverride != null)
                            {
                                NeutralSiteGame koGame = null;
                                game.GameSite = stadiumOverride["NickName"];
                                koGame = ScheduledGame.KickOffGames.Where(nsg => nsg.Contains(stadiumId)).FirstOrDefault();

                                if (koGame != null)
                                {
                                    game.SiteId = koGame.Id;
                                    const int PigskinClassicKickoff = 71041024;

                                    if (koGame.Id == PigskinClassicKickoff)
                                    {
                                        game.GameSite += $" ({ScheduledGame.SiteIdSuffix(stadiumId)})";
                                    }
                                }
                                else
                                {
                                    game.SiteId = stadiumId;
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(stadium["STNN"]) == false)
                            {
                                game.GameSite = stadium["STNN"];
                                game.SiteId = stadiumId;
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(stadium["STNN"]) == false)
                        {
                            game.GameSite = stadium["STNN"];
                            game.SiteId = stadiumId;
                        }
                    }
                }

                Schedule.Add(game.Key, game);
            }

            return Schedule;
        }

        public Dictionary<int, Dictionary<int, TeamSeasonRecord>> CreateTeamHistoricRecords()
        {
            var teamRecords = new Dictionary<int, Dictionary<int, TeamSeasonRecord>>();
            var table = MaddenDatabase.lTables[114];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var teamId = record.GetInt(0).GetRealTeamId();
                Dictionary<int, TeamSeasonRecord> teamSeasonRecords;
                if (teamRecords.TryGetValue(teamId, out teamSeasonRecords) == false)
                {
                    // load the records from the continuation file
                    if (ContinuationData.UsingContinuationData && ContinuationData.Instance != null && ContinuationData.Instance.TeamHistoricRecords.ContainsKey(teamId))
                    {
                        teamSeasonRecords = ContinuationData.Instance.TeamHistoricRecords[teamId];
                    }
                    else
                    {
                        teamSeasonRecords = new Dictionary<int, TeamSeasonRecord>();
                    }

                    teamRecords[teamId] = teamSeasonRecords;
                }

                var seasonRecord = new TeamSeasonRecord
                {
                    Year = record.GetInt(2) + ContinuationData.ContinuationYear,
                    Win = record.GetInt(3),
                    Loss = record.GetInt(1)
                };

                // check to see if the team went 16-0
                if (seasonRecord.Win < 4)
                {
                    seasonRecord.Win += BowlChampion.IsNationalChampionshipYear(teamId, seasonRecord.Year) ? 16 : 0;
                }

                teamSeasonRecords.Add(seasonRecord.Year, seasonRecord);
            }

            return teamRecords;
        }

        public Dictionary<string, Coach> ReadCoaches()
        {
            var coaches = new Dictionary<string, Coach>();

            var coachTable = MaddenDatabase.lTables[133];
            for (int i = 0; i < MaddenDatabase.lTables[133].Table.currecords; i++)
            {
                // only coaches on valid teams should be analyzed
                if (coachTable.lRecords[i].lEntries[23].Data.ToInt32().IsValidTeam() == false)
                    continue;

                int level, exp;
                GetLevelAndXP(MaddenDatabase.lTables[132].lRecords, i, out level, out exp);
                var coach = new Coach
                {
                    Id = MaddenDatabase.lTables[133].lRecords[i].lEntries[20].Data.ToInt32(),
                    TeamId = MaddenDatabase.lTables[133].lRecords[i].lEntries[23].Data.ToInt32().GetRealTeamId(),
                    Position = MaddenDatabase.lTables[133].lRecords[i].lEntries[100].Data.ToInt32(),
                    FirstName = MaddenDatabase.lTables[133].lRecords[i].lEntries[65].Data,
                    LastName = MaddenDatabase.lTables[133].lRecords[i].lEntries[66].Data,
                    Age = MaddenDatabase.lTables[133].lRecords[i].lEntries[29].Data.ToInt32(),
                    ContractLength = coachTable.lRecords[i].lEntries[69].Data.ToInt32(),
                    YearsIntoContract = coachTable.lRecords[i].lEntries[88].Data.ToInt32(),
                    YearsWithTeam = 1 + coachTable.lRecords[i].lEntries[89].Data.ToInt32(),
                    Rating = coachTable.lRecords[i].lEntries[109].Data.ToInt32(),
                    OriginalJob = coachTable.lRecords[i].lEntries[5].Data,
                    CareerWin = coachTable.lRecords[i]["CCWI"].ToInt32(),
                    CareerLoss = coachTable.lRecords[i]["CCLO"].ToInt32(),
                    TeamWin = coachTable.lRecords[i]["CTWN"].ToInt32(),
                    TeamLoss = coachTable.lRecords[i]["COTL"].ToInt32(),
                    Level = level,
                    Exp = exp,
                    OffPlaybookId = coachTable.lRecords[i]["CPID"].ToInt32(),
                    DefPlaybookId = (DefensivePlaybook)coachTable.lRecords[i]["CDID"].ToInt32(),
                    AlmaMaterId = coachTable.lRecords[i]["CHFT"].ToInt32().GetRealTeamId(),
                    CoachBowlWin = coachTable.lRecords[i]["CBLW"].ToInt32(),
                    CoachBowlLoss = coachTable.lRecords[i]["CBLL"].ToInt32(),
                    AllAmericans = coachTable.lRecords[i]["CNAA"].ToInt32(),
                    Top25Classes = coachTable.lRecords[i]["CNTC"].ToInt32(),
                    CoachOfYearAwards = coachTable.lRecords[i]["CYRA"].ToInt32(),
                    Top25Win = coachTable.lRecords[i]["CTTW"].ToInt32(),
                    Top25Loss = coachTable.lRecords[i]["CTTL"].ToInt32(),
                    RivalWin = coachTable.lRecords[i]["CRVW"].ToInt32(),
                    RivalLoss = coachTable.lRecords[i]["CRVL"].ToInt32(),
                    LongestWinStreak = coachTable.lRecords[i]["CCLS"].ToInt32(),
                    HeismanWinners = coachTable.lRecords[i]["CHTW"].ToInt32(),
                    CareerConferenceChampionships = coachTable.lRecords[i]["CCTW"].ToInt32(),
                    CareerNationalChampionships = coachTable.lRecords[i]["CNTW"].ToInt32(),
                };

                coaches.Add(coach.Key, coach);
            }

            return coaches;
        }

        public Dictionary<int, Team> ReadTeams(bool isPreseason)
        {
            var teams = new Dictionary<int, Team>();
            var table = MaddenDatabase.lTables[167];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var teamId = table.lRecords[i].lEntries[40].Data.ToInt32().GetRealTeamId();

                // don't look at any team with an id greater than 235 and less than 901 which is the first teambuilder team id
                if (teamId > 235 && teamId < 901)
                    continue;

                var team = new Team(table.lRecords[i], MaddenDatabase, isPreseason);
                teams.Add(team.Id, team);
            }


            if (!teams.ContainsKey(61))
                teams.Add(61, new Team(61, "New Mexico State") { ToughestPlaceToPlayRank = 1000 });

            if (!teams.ContainsKey(100))
                teams.Add(100, new Team(100, "Connecticut") { ToughestPlaceToPlayRank = 1000 });

            if (!teams.ContainsKey(230))
                teams.Add(230, new Team(230, "FIU") { ToughestPlaceToPlayRank = 1000 });

            return teams;
        }

        public Dictionary<int, Conference> ReadConferenceMetadata()
        {
            var conferences = new Dictionary<int, Conference>();

            for (int i = 0; i < MaddenDatabase.lTables[134].Table.currecords; i++)
            {
                var conf = new Conference
                {
                    Id = MaddenDatabase.lTables[134].lRecords[i].lEntries[0].Data.ToInt32(),
                    LeagueId = MaddenDatabase.lTables[134].lRecords[i].lEntries[1].Data.ToInt32(),
                    Name = MaddenDatabase.lTables[134].lRecords[i].lEntries[2].Data
                };

                conferences.Add(conf.Id, conf);
            }

            // now go thru the divisions
            var table = MaddenDatabase.lTables[136];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var confId = table.lRecords[i].lEntries[0].Data.ToInt32();
                var division = new Division(table.lRecords[i]);
                conferences[confId].Divisions.Add(division);
            }

            return conferences;
        }

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

        public Dictionary<int, TeamStat> ReadTeamStats()
        {
            var teamStats = new Dictionary<int, TeamStat>();

            var table = MaddenDatabase.lTables[168];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var row = table.lRecords[i];
                var teamId = row.GetInt(0).GetRealTeamId();

                var stats = new TeamStat
                {
                    TeamId = teamId,
                    TwoPointConversionAttempts = row.GetInt(1),
                    Turnovers = row.GetInt(2),
                    PassAttempts = row.GetInt(3),
                    RushAttempts = row.GetInt(4),
                    Tssa = row.GetInt(5),
                    Tsta = row.GetInt(6),
                    TwoPointConversions = row.GetInt(7),
                    ThirdDownConversions = row.GetInt(8),
                    FourthDownConversions = row.GetInt(9),
                    FirstDowns = row.GetInt(10),
                    ThirdDownAttempts = row.GetInt(11),
                    FourthDownAttempts = row.GetInt(12),
                    Penalties = row.GetInt(14),
                    RedZoneFG = row.GetInt(16),
                    Tsdi = row.GetInt(17),
                    IntThrown = row.GetInt(19),
                    RedZoneFGAllowed = row.GetInt(15),
                    InterceptionsByDefense = row.GetInt(18),
                    Sacks = row.GetInt(20),
                    FumblesLost = row.GetInt(21),
                    PassYardsAllowed = row.GetInt(22),
                    PassYards = row.GetInt(23),
                    OpponentsInRedZone = row.GetInt(24),
                    FumblesRecovered = row.GetInt(25),
                    RushYards = row.GetInt(26),
                    PassTD = row.GetInt(27),
                    RedZoneTDAllowed = row.GetInt(28),
                    RedZoneTD = row.GetInt(29),
                    RushTD = row.GetInt(30),
                    PenaltyYards = row.GetInt(31),
                    TotalYards = row.GetInt(32),
                    RushingYardsAllowed = row.GetInt(33),
                    OffensiveYards = row.GetInt(34),
                    RedZoneVisits = row.GetInt(36),
                    SpecialTeamYards = row.GetInt(35),
                };

                teamStats.Add(teamId, stats);
            }

            return teamStats;
        }

        public void CommitTeamRecords()
        {
            // pull in the latest records from the table for career/season
            var table = MaddenDatabase.lTables[159].lRecords.Where(mr => mr["RCDY"].ToInt32() == BowlChampion.DynastyFileYear && mr["RCDT"].ToInt32() != 0).ToArray();
            for (int i = 0; i < table.Length; i++)
            {
                var record = table[i];
                var teamId = record.GetInt(7).GetRealTeamId();
                var holder = record["RCDH"].Substring(5);

                TeamRecord.SetNewRecord(
                    (TeamRecordKeys)record["RCDI"].ToInt32(),
                    record["RCDV"].ToInt32(),
                    PlayerDB.Find(teamId, holder[0], holder.Substring(2)),
                    null,
                    Int32.MaxValue,
                    record["RCDT"].ToInt32());
            }
        }

        public Dictionary<int, DraftClass[]> ReadDraftHistory()
        {
            var draftHistoryTable = MaddenTable.FindTable(MaddenDatabase.lTables, "TPHS");
            return draftHistoryTable.lRecords.GroupBy(
                mr => mr["TGID"].ToInt32().GetRealTeamId(),
                mr => new DraftClass
                {
                    DynastyYear = mr["dryr"].ToInt32(),
                    Round1 = mr["PDR1"].ToInt32(),
                    Round2 = mr["PDR2"].ToInt32(),
                    Round3 = mr["PDR3"].ToInt32(),
                    RoundLater = mr["PDRL"].ToInt32(),
                }).ToDictionary(g => g.Key, g => g.ToArray());
        }

        public Dictionary<int, MediaCoverage[]> ReadMediaCoverage()
        {
            var mediaTable = MaddenTable.FindMaddenTable(MaddenDatabase.lTables, "MCOV");
            return mediaTable.lRecords
                .GroupBy(mr => mr["TGID"].ToInt32().GetRealTeamId())
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Where(mr => mr["SGNM"].ToInt32() == 127)  // we don't want week 1 game info
                        .Select(mr =>
                        new MediaCoverage
                        {
                            TeamId = mr["TGID"].ToInt32().GetRealTeamId(),
                            GameNumber = mr["SGNM"].ToInt32(),
                            Week = mr["SEWN"].ToInt32(),
                            PlayerId = mr["PGID"].ToInt32(),
                            Headline = MediaCoverage.Transform(mr["MHTX"]),
                            Content = MediaCoverage.Transform(mr["MCTX"])
                        }
                        )
                        .ToArray()
                );
        }

        public Dictionary<int, Dictionary<int, DepthChartPosition[]>> ReadDepthCharts()
        {
            var depthChartTable = MaddenTable.FindMaddenTable(MaddenDatabase.lTables, "DCHT");
            return depthChartTable.lRecords.Where(mr => mr["TGID"].ToInt32().GetRealTeamId().IsValidTeam()).GroupBy(mr => mr["TGID"].ToInt32().GetRealTeamId()).ToDictionary(
                group => group.Key,
                group => group.Select(g =>
                    new DepthChartPosition
                    {
                        PlayerId = g["PGID"].ToInt32(),
                        PlayerPosition = g["PPOS"].ToInt32(),
                        PositionDepth = g["ddep"].ToInt32()
                    }).ToArray()
                .GroupBy(dpp => dpp.PlayerPosition)
                .ToDictionary(g => g.Key, g => g.OrderBy(pos => pos.PositionDepth).ToArray()));
        }

        public Dictionary<int, int> FindCoachesOnHotSeat()
        {
            var table = MaddenTable.FindTable(MaddenDatabase.lTables, "CPRF");
            return table.lRecords
                .Where(mr => mr["JSCR"].ToInt32() > 100)
                .GroupBy(mr => mr["CCID"].ToInt32(), mr => mr["JSCR"].ToInt32())
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r).First());
        }

        public string ReadStadiumName(int siteId)
        {
            var stadiumTable = MaddenDatabase.lTables.Where(tbl => tbl.Abbreviation == "STAD").SingleOrDefault();
            var stadium = stadiumTable.lRecords.Where(record => record["SGID"].ToInt32() == siteId).SingleOrDefault();
            return stadium.lEntries[56].Data;
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
        #region Helpers
        private static void GetLevelAndXP(List<MaddenRecord> table, int idx, out int level, out int exp)
        {
            if (idx >= table.Count)
            {
                level = 1;
                exp = 0;
            }
            else
            {
                level = table[idx].lEntries[8].Data.ToInt32();
                exp = table[idx].lEntries[14].Data.ToInt32();
            }
        }
        #endregion
    }
}