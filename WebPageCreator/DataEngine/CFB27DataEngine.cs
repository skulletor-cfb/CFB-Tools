using CFB27.Data.Model;
using MaddenFranchise;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EA_DB_Editor
{

    public class CFB27DataEngine : IDataEngine
    {
        private FranchiseFile dynasty;
        public CFBTable<CFBTeam> Teams { get; private set; }
        public CFBTable<CFBBowl> Bowls { get; private set; }
        public CFBTable<CFBSeasonGame> SeasonGames { get; private set; }
        public Dictionary<int, CFBSeasonGame> BowlSeasonGames { get; }
        public CFBTable<CFBStory> Stories { get; }

        // the Player table is large (~16k rows), so it's only read when players are needed
        private CFBTable<CFBPlayer> players;
        public CFBTable<CFBPlayer> Players => this.players ?? (this.players = dynasty.GetTable<CFBPlayer>(TableName.Player, CFBPlayer.Transform));

        private CFBTable<CFBCoach> coaches;
        public CFBTable<CFBCoach> Coaches => this.coaches ?? (this.coaches = dynasty.GetTable<CFBCoach>(TableName.Coach, CFBCoach.Transform));

        private List<CFBConference> conferences;
        public List<CFBConference> Conferences => this.conferences ?? (this.conferences = CFBConference.Read(dynasty));

        public Dictionary<int, string> TeamNames { get; private set; }

        public CFB27DataEngine(FranchiseFile dynasty)
        {
            this.dynasty = dynasty;
            this.Teams = dynasty.GetTable<CFBTeam>(TableName.Team, CFBTeam.Transform);
            this.TeamNames = this.Teams.Records.ToDictionary(t => t.TeamId, t => t.DisplayName);
            this.Bowls = dynasty.GetTable<CFBBowl>(TableName.BowlGame, CFBBowl.Transform);
            this.SeasonGames = dynasty.GetTable<CFBSeasonGame>(TableName.SeasonGame);
            this.BowlSeasonGames = this.SeasonGames.Records.Where(g => !g.IsEmpty && g.IsBowlGame).ToDictionary(g => g.BowlId);

            // CFP quarterfinal/semifinal rows (12-17) rotate between the NY6 bowls; the host bowl is
            // only identified by the SeasonGame's stadium (BowlGame.Stadium is empty for those rows).
            foreach (var bsg in this.BowlSeasonGames.Values)
            {
                this.Bowls.Records[bsg.BowlId].StadiumId = bsg.Stadium.CFBToInt64();
            }

            this.Stories = dynasty.GetTable<CFBStory>(TableName.Story);
        }

        public Dictionary<string, Bowl> CreateBowlTable()
        {
            var bowls = new Dictionary<string, Bowl>();
            foreach (var bsg in BowlSeasonGames.Values)
            {
                var b = this.Bowls.Records[bsg.BowlId];

                var bowl = new Bowl
                {
                    Name = b.Name,
                    Week = bsg.SeasonWeek,
                    Game = bsg.SeasonGameNum,
                    Day = bsg.GameDateDay,
                    ConferenceTieInId1 = b.ConferenceId1,
                    ConferenceTieInId2 = b.ConferenceId2,
                    ConferenceTieInSelection1 = b.Conference1Rank,
                    ConferenceTieInSelection2 = b.Conference2Rank,
                    Id = b.BowlId,
                };

                bowls.Add(bowl.Key, bowl);
            }

            return bowls;
        }

        public Dictionary<int, TeamSchedule> CreateTeamSchedule(bool isPreseason)
        {
            var schedules = new Dictionary<int, TeamSchedule>();

            foreach (var sg in this.SeasonGames.Records.Where(n => !n.IsEmpty).OrderBy(n => n.SeasonWeek))
            {
                // create the team schedule as needed
                if (!schedules.TryGetValue(sg.HomeTeamId, out var homeSchedule))
                {
                    homeSchedule = schedules[sg.RealHomeTeamId] = new TeamSchedule
                    {
                        TeamId = sg.RealHomeTeamId,
                    };
                }

                if (!schedules.TryGetValue(sg.AwayTeamId, out var awaySchedule))
                {
                    awaySchedule = schedules[sg.RealAwayTeamId] = new TeamSchedule
                    {
                        TeamId = sg.RealAwayTeamId,
                    };
                }

                // now add to a team schedules
                var awayGame = new Game
                {
                    IsHomeGame  = false,
                    OpponentId = sg.RealHomeTeamId,
                    GameNumber = sg.SeasonGameNum,
                    Week = sg.SeasonWeek,
                    TeamId = sg.RealAwayTeamId,
                };

                var homeGame = new Game
                {
                    IsHomeGame = true,
                    OpponentId = sg.RealAwayTeamId,
                    GameNumber = sg.SeasonGameNum,
                    Week = sg.SeasonWeek,
                    TeamId = sg.RealHomeTeamId,
                };

                if (sg.SeasonWeek > 14 && isPreseason)
                    continue;

                if (!homeSchedule.TryGetValue(sg.SeasonWeek, out var gamesForWeek))
                {
                    gamesForWeek = new List<Game>();
                    homeSchedule.Add(homeGame.Week, gamesForWeek);
                    gamesForWeek.Add(homeGame);
                }

                if (!awaySchedule.TryGetValue(sg.SeasonWeek, out  gamesForWeek))
                {
                    gamesForWeek = new List<Game>();
                    awaySchedule.Add(awayGame.Week, gamesForWeek);
                    gamesForWeek.Add(awayGame);
                }
            }

            return schedules;
        }

        public bool IsSeasonOver()
        {
            const int nationalChampionshipId = 11;

            if(BowlSeasonGames.TryGetValue(nationalChampionshipId, out var championshipGame))
            {
                return (championshipGame.HomeScore + championshipGame.AwayScore) > 0;
            }

            return false;
        }

        public void ReadBowlChampions(Dictionary<string, BowlChampion> bowlChampions)
        {
            // no changes in preseason
            if (!this.IsSeasonOver())
            {
                return; 
            }

            foreach (var sg in BowlSeasonGames.Values)
            {
                if (sg.GameStatus != "HomeWon" && sg.GameStatus != "AwayWon")
                    continue;

                var bc = new BowlChampion
                {
                    TeamId = sg.GameStatus == "HomeWon" ? sg.RealHomeTeamId : sg.RealAwayTeamId,
                    Year = sg.SeasonYear + ContinuationData.ContinuationYear,
                    BowlId = this.Bowls.Records[sg.BowlId].BowlId,
                };

                if (bowlChampions.TryGetValue(bc.GetKey(), out var existing))
                {
                    // already loaded (e.g. from the archive); a different team means two bowls mapped to the same id
                    if (existing.TeamId != bc.TeamId)
                        throw new InvalidOperationException($"Bowl key {bc.GetKey()} already has team {existing.TeamId}; season game for bowl row {sg.BowlId} maps to the same bowl");
                }
                else
                {
                    bowlChampions.Add(bc.GetKey(), bc);
                }
            }
        }

        public List<AllAmerican> CreateAllAmericans()
        {
            throw new NotImplementedException();
        }

        public void CreatePlayers(Dictionary<int, List<Player>> Rosters, Dictionary<int, Player> Players)
        {
            // only FBS rosters; generic FCS teams, recruits and unattached players all have TeamIndex 255
            foreach (var record in this.Players.Records.Where(p => p.IsOnTeam))
            {
                var player = new Player
                {
                    // PresentationId isn't unique, so use the Player table row (what the save's references point to)
                    Id = record.Row,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    TeamId = record.TeamId,
                    Year = record.SchoolYear,
                    IsRedShirt = record.HasRedshirted,
                    Position = record.Position,
                    Number = record.JerseyNum,
                    Height = record.Height,
                    Weight = record.Weight + 160,
                    Ovr = record.OverallRating,
                    Spd = record.SpeedRating,
                    Acc = record.AccelerationRating,
                    Agl = record.AgilityRating,
                    Str = record.StrengthRating,
                    Awr = record.AwarenessRating,
                    Face = record.Portrait,
                    // TODO: City is an NCAA 14 cities.csv id; CFB27 only has HomeTown/HomeState strings
                    // TODO: GamesPlayed comes from the player's stats, see ReadStats
                };

                Players[player.Id] = player;

                if (!Rosters.TryGetValue(player.TeamId, out var roster))
                {
                    roster = new List<Player>();
                    Rosters[player.TeamId] = roster;
                }

                roster.Add(player);
            }
        }

        public void ReadStats()
        {
            // no-op for now, not needed for preseason
        }

        /// <summary>Conferences keyed by classic id; only the Sun Belt still has divisions (West 16, East 17).</summary>
        public Dictionary<int, Conference> ReadConferenceMetadata()
        {
            var conferences = new Dictionary<int, Conference>();
            foreach (var cfbConference in this.Conferences)
            {
                var conference = new Conference
                {
                    Id = cfbConference.ClassicId,
                    Name = cfbConference.ClassicName,
                };

                foreach (var division in cfbConference.Divisions.Where(d => d.ClassicId != CFBConference.NoDivisionId))
                {
                    conference.Divisions.Add(new Division
                    {
                        Id = division.ClassicId,
                        Name = $"{cfbConference.ClassicName} ({division.Name})",
                        SubName = division.Name,
                    });
                }

                conferences.Add(conference.Id, conference);
            }

            return conferences;
        }

        public Dictionary<int, Team> ReadTeams(bool isPreseason)
        {
            var conferenceByTeamRow = ReadConferenceByTeamRow();
            return this.Teams.Records.ToDictionary(t => t.TeamId, t =>
            {
                var team = new Team(t, isPreseason);
                if (conferenceByTeamRow.TryGetValue(t.Row, out var conference))
                {
                    team.ConferenceId = conference.ConferenceId;
                    team.DivisionId = conference.DivisionId;
                }

                return team;
            });
        }

        public Dictionary<string, Coach> ReadCoaches()
        {
            var coaches = new Dictionary<string, Coach>();

            // Coach.AlmaMater is TEAM_ORIGID - 1100 (Florida State 1132 -> 32, Georgia 1134 -> 34)
            var teamIdByAlmaMater = this.Teams.Records
                .Where(t => !t.IsEmpty)
                .GroupBy(t => t.TEAM_ORIGID - CFBCoach.AlmaMaterOrigIdOffset)
                .ToDictionary(g => g.Key, g => g.First().TeamId);

            // only FBS staffs (HC/OC/DC); coaches without a job have TeamIndex 255
            foreach (var record in this.Coaches.Records.Where(c => c.IsOnTeam && c.PositionId >= 0))
            {
                var coach = new Coach
                {
                    // PresentationId isn't unique, so use the Coach table row
                    Id = record.Row,
                    TeamId = record.TeamId,
                    Position = record.PositionId,
                    FirstName = record.FirstName,
                    LastName = record.LastName,
                    Age = record.Age,
                    ContractLength = record.ContractLength,
                    YearsIntoContract = record.ContractLength - record.ContractYearsRemaining,
                    YearsWithTeam = record.SeasonsWithTeam,
                    Level = record.Level,
                    Exp = record.ExperiencePoints,
                    CareerWin = record.CareerWins,
                    CareerLoss = record.CareerLosses,
                    TeamWin = record.WinsAtCurrentSchool,
                    TeamLoss = record.LossesAtCurrentSchool,
                    CoachBowlWin = record.BowlWins,
                    CoachBowlLoss = record.BowlLosses,
                    CareerConferenceChampionships = record.ConfChampWins,
                    CareerNationalChampionships = record.NCWins,
                    Top25Win = record.Top25Wins,
                    Top25Loss = record.Top25Losses,
                    RivalWin = record.RivalWins,
                    RivalLoss = record.RivalLosses,
                    LongestWinStreak = record.CareerLongWinStreak,
                    AlmaMaterId = teamIdByAlmaMater.TryGetValue(record.AlmaMater, out var almaMaterId) ? almaMaterId : 0,
                    // TODO: Rating - COACH_RATING is always 0;
                    // OffPlaybookId/DefPlaybookId - playbooks are references into game asset tables
                };

                coaches.Add(coach.Key, coach);
            }

            return coaches;
        }

        public Dictionary<int, Dictionary<int, TeamSeasonRecord>> CreateTeamHistoricRecords()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, ScheduledGame> ReadSchedule(bool isPreseason)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<Record>> ReadSchoolRecords(bool recreateUsingRecordsFile)
        {
            throw new NotImplementedException();
        }

        public void ReadGameStats(Dictionary<string, ScheduledGame> games)
        {
            throw new NotImplementedException();
        }

        public List<ConferenceChampion> ReadConferenceChamps()
        {
            throw new NotImplementedException();
        }

        private Dictionary<int, Stadium> stadiums;

        /// <summary>
        /// Stadiums keyed by the save's stadium reference (CFBStadium.ToStadiumId): every team's home plus the
        /// neutral sites in CFBStadium.NeutralSites. Names and capacities come from the game's stadium table,
        /// since the save doesn't hold them.
        /// </summary>
        public Dictionary<int, Stadium> ReadStadiums()
        {
            if (this.stadiums != null)
            {
                return this.stadiums;
            }

            // placeholder for teams without a stadium (the generic FCS teams), like NCAA 14's 1023
            var result = new Dictionary<int, Stadium> { [0] = new Stadium() };

            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && !string.Equals(t.Stadium, BaseRecord.NoRefString)))
            {
                var id = CFBStadium.ToStadiumId(team.Stadium);
                var home = CFBStadium.FindHome(team.AssetName, team.ShortName);
                if (home != null && !result.ContainsKey(id))
                {
                    result.Add(id, new Stadium { Id = id, Name = home.Name, Capacity = home.Capacity });
                }
            }

            foreach (var neutral in CFBStadium.NeutralSites)
            {
                var id = unchecked((int)neutral.Key);
                var stadium = CFBStadium.FindByName(neutral.Value);
                if (stadium != null && !result.ContainsKey(id))
                {
                    result.Add(id, new Stadium { Id = id, Name = stadium.Name, Capacity = stadium.Capacity });
                }
            }

            return this.stadiums = result;
        }

        public Dictionary<int, RecruitClassRanking> ReadRecruitClasses()
        {
            var rankings = new Dictionary<int, RecruitClassRanking>();
            var conferenceByTeamRow = ReadConferenceByTeamRow();

            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex))
            {
                var stars = ReadCommittedPlayerRows(team).Select(row => this.Players.Records[row].StarRating).ToList();

                var ranking = new RecruitClassRanking
                {
                    TeamId = team.TeamId,
                    // the save has the game's class rank (TopClassRank) but no class points, so score the class by stars
                    Points = stars.Sum(s => RecruitStarPoints[s]),
                    Star1 = stars.Count(s => s == 1),
                    Star2 = stars.Count(s => s == 2),
                    Star3 = stars.Count(s => s == 3),
                    Star4 = stars.Count(s => s == 4),
                    Star5 = stars.Count(s => s == 5),
                    ConferenceId = conferenceByTeamRow.TryGetValue(team.Row, out var conference) ? conference.ConferenceId : -1,
                    DivisionId = conferenceByTeamRow.TryGetValue(team.Row, out conference) ? conference.DivisionId : CFBConference.NoDivisionId,
                    Wins = team.Win,
                    Losses = team.Loss,
                };

                rankings.Add(ranking.TeamId, ranking);
            }

            return rankings;
        }

        /// <summary>Class points per recruit, indexed by star rating (0 = no rating).</summary>
        private static readonly int[] RecruitStarPoints = { 0, 1, 2, 4, 12, 18 };

        /// <summary>Player table rows in the team's CommittedPlayers list (signed recruits keep TeamIndex 255 until they join the roster).</summary>
        private IEnumerable<int> ReadCommittedPlayerRows(CFBTeam team)
        {
            var record = this.Teams.RawTable.Records[team.Row];
            var committed = dynasty.Follow(record.GetReferenceDataByKey("CommittedPlayers").Value);
            if (committed == null)
            {
                return Enumerable.Empty<int>();
            }

            return committed.FieldsArray
                .Take(committed.ArraySize ?? 0)
                .Select(f => f.ReferenceData)
                .Where(r => r.HasValue && r.Value.TableId == this.Players.RawTable.Header.TableId)
                .Select(r => r.Value.RowNumber)
                .ToList();
        }

        /// <summary>Team table row -> (classic conference id, classic division id), from each conference's divisions.</summary>
        private Dictionary<int, (int ConferenceId, int DivisionId)> ReadConferenceByTeamRow()
        {
            var result = new Dictionary<int, (int, int)>();
            foreach (var conference in this.Conferences)
            {
                foreach (var division in conference.Divisions)
                {
                    foreach (var row in division.TeamRows)
                    {
                        result[row] = (conference.ClassicId, division.ClassicId);
                    }
                }
            }

            return result;
        }

        public Dictionary<int, Recruit> ReadRecruits()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, List<Award>> ReadAwards()
        {
            throw new NotImplementedException();
        }

        public int CalculateRosterSpots(RecruitClassRanking ranking)
        {
            throw new NotImplementedException();
        }

        public List<Record> ReadNcaaRecords()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, TeamStat> ReadTeamStats()
        {
            throw new NotImplementedException();
        }

        public void CommitTeamRecords()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, DraftClass[]> ReadDraftHistory()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, MediaCoverage[]> ReadMediaCoverage()
        {
            return this.Stories.Records
                .Where(s => !s.IsEmpty)
                .GroupBy(s => s.TeamId)
                .ToDictionary(
                g => g.Key,
                    group => group
                        .Where(mr => mr.Category != "NEXT_GAME")  // we don't want week 1 game info
                        .Select(mr =>
                        new MediaCoverage
                        {
                            TeamId = mr.TeamId,
                            Headline = mr.Header,
                            Content = mr.Tag,
                        })
                        .ToArray()
                );
        }

        public Dictionary<int, Dictionary<int, DepthChartPosition[]>> ReadDepthCharts()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, int> FindCoachesOnHotSeat()
        {
            throw new NotImplementedException();
        }

        public string ReadStadiumName(int siteId)
        {
            return ReadStadiums().TryGetValue(siteId, out var stadium) ? stadium.Name : string.Empty;
        }
    }
}