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
        private Dictionary<int, Dictionary<int, TeamSeasonRecord>> teamHistoricRecords;
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
        public int TeamCount => 138;

        /// <summary>
        /// Team history includes the season in progress (0-0 at week 0) until the season is over; like NCAA 14, the history
        /// files leave it out.
        /// </summary>
        public bool SkipFirstHistoricRecord => !this.IsSeasonOver();

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
                if (!schedules.TryGetValue(sg.RealHomeTeamId, out var homeSchedule))
                {
                    homeSchedule = schedules[sg.RealHomeTeamId] = new TeamSchedule
                    {
                        TeamId = sg.RealHomeTeamId,
                    };
                }

                if (!schedules.TryGetValue(sg.RealAwayTeamId, out var awaySchedule) && !sg.RealAwayTeamId.IsFcsTeam())
                {
                    awaySchedule = schedules[sg.RealAwayTeamId] = new TeamSchedule
                    {
                        TeamId = sg.RealAwayTeamId,
                    };
                }

                // now add to a team schedules
                var awayGame = new Game
                {
                    IsHomeGame = false,
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

                if (!awaySchedule?.TryGetValue(sg.SeasonWeek, out gamesForWeek) ?? false)
                {
                    gamesForWeek = new List<Game>();
                    awaySchedule.Add(awayGame.Week, gamesForWeek);
                    gamesForWeek.Add(awayGame);
                }
            }

            return schedules;
        }

        /// <summary>The save's season (SeasonInfo.CurrentSeasonYear), e.g. 2026.</summary>
        public int CurrentSeasonYear => Convert.ToInt32(dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty)["CurrentSeasonYear"]);

        /// <summary>
        /// This season's coach award credits, for the coaches on staff now (see CoachAwardHistory):
        /// - national 1st and 2nd team All-Americans: the head coach, plus the offensive coordinator for QB-RT or the defensive
        ///   coordinator for LE-SS; kickers and punters count for the head coach only
        /// - the Heisman winner: the same
        /// - a top 25 recruiting class (the game's Team.TopClassRank): the head coach
        /// - coach of the year (CoachAward BEST_HC) and the Broyles (BEST_AC, assistants): the coach named
        /// </summary>
        public List<CoachAwardCount> ReadSeasonCoachAwards()
        {
            var counts = new Dictionary<int, CoachAwardCount>();
            CoachAwardCount For(int coachRow)
            {
                // the id the site uses: the NCAA 14 id for coaches the continuation file carries over
                var id = ContinuationData.Instance != null && ContinuationData.Instance.NewToOldCoachMap.TryGetValue(coachRow, out var oldId) ? oldId : coachRow;
                if (!counts.TryGetValue(id, out var count))
                {
                    counts[id] = count = new CoachAwardCount { CoachId = id };
                }

                return count;
            }

            var staff = this.Coaches.Records
                .Where(c => c.IsOnTeam && c.PositionId >= 0 && c.PositionId <= 2)
                .GroupBy(c => (c.TeamId, c.PositionId))
                .ToDictionary(g => g.Key, g => g.First().Row);

            void Credit(int teamId, int position, Action<CoachAwardCount> add)
            {
                if (staff.TryGetValue((teamId, 0), out var headCoach))
                {
                    add(For(headCoach));
                }

                // QB (0) ... RT (9) offense, LE (10) ... SS (18) defense, K and P the head coach only
                int coordinator = position <= 9 ? 1 : position <= 18 ? 2 : -1;
                if (coordinator > 0 && staff.TryGetValue((teamId, coordinator), out var coordinatorRow))
                {
                    add(For(coordinatorRow));
                }
            }

            foreach (var allAmerican in this.CreateAllAmericans().Where(a => a.ConferenceId == NationalAllAmericanConferenceId && a.AllAmericanTeam != AllAmericanTeam.Freshman))
            {
                Credit(this.Players.Records[allAmerican.PlayerId].TeamId, allAmerican.Position, c => c.AllAmericans++);
            }

            if (this.ReadAwards().TryGetValue(0, out var heisman) && heisman.FirstOrDefault(a => a.Rank == 1) is Award winner)
            {
                var player = this.Players.Records[winner.PlayerId];
                Credit(player.TeamId, player.Position, c => c.HeismanWinners++);
            }

            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex && t.TopClassRank >= 1 && t.TopClassRank <= 25))
            {
                if (staff.TryGetValue((team.TeamId, 0), out var headCoach))
                {
                    For(headCoach).Top25Classes++;
                }
            }

            var coachTableId = dynasty.GetPrimaryTable("Coach", "FirstName").Header.TableId;
            var coachAwards = dynasty.GetPrimaryTable("CoachAward");
            coachAwards.ReadRecords();
            foreach (var award in coachAwards.Records.Where(r => !r.IsEmpty && (r["Period"] as string) == "Season"
                && ((r["AwardType"] as string) == "BEST_HC" || (r["AwardType"] as string) == "BEST_AC")))
            {
                var coach = award.GetReferenceDataByKey("Coach");
                if (coach.HasValue && coach.Value.TableId == coachTableId)
                {
                    For(coach.Value.RowNumber).CoachOfYearAwards++;
                }
            }

            return counts.Values.ToList();
        }

        /// <summary>
        /// The season is over once the game has moved to the offseason (SeasonInfo.CurrentStage: PreSeason, NFLSeason,
        /// OffSeason). It stays OffSeason until the next season's preseason, when CurrentSeasonYear moves on.
        /// </summary>
        public bool IsSeasonOver()
        {
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            return string.Equals(seasonInfo["CurrentStage"] as string, "OffSeason");
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

        /// <summary>The classic conference id the All-American report uses for the national teams.</summary>
        private const int NationalAllAmericanConferenceId = 14;

        /// <summary>
        /// The All-American teams (25 players each) from PlayerAward, nationally (conference 14, as in NCAA 14) and for each
        /// conference. The final 1st, 2nd and Freshman teams are used once they're named at the end of the season; before
        /// that, the preseason 1st and 2nd teams.
        /// </summary>
        public List<AllAmerican> CreateAllAmericans()
        {
            var finalTeams = new Dictionary<string, AllAmericanTeam>
            {
                { "ALL_AM_1ST", AllAmericanTeam.First }, { "ALL_AM_2ND", AllAmericanTeam.Second }, { "ALL_AM_FR", AllAmericanTeam.Freshman },
                { "ALL_AM_1ST_CONF", AllAmericanTeam.First }, { "ALL_AM_2ND_CONF", AllAmericanTeam.Second }, { "ALL_AM_FR_CONF", AllAmericanTeam.Freshman },
            };
            var preseasonTeams = new Dictionary<string, AllAmericanTeam>
            {
                { "ALL_AM_1ST_PRE", AllAmericanTeam.First }, { "ALL_AM_2ND_PRE", AllAmericanTeam.Second },
                { "ALL_AM_1ST_PRE_CONF", AllAmericanTeam.First }, { "ALL_AM_2ND_PRE_CONF", AllAmericanTeam.Second },
            };

            var playerTableId = this.Players.RawTable.Header.TableId;
            var awards = dynasty.GetPrimaryTable("PlayerAward").Records
                .Where(r => !r.IsEmpty && (r["Period"] as string) == "Season")
                .ToList();
            var teams = awards.Any(r => finalTeams.ContainsKey(r["AwardType"] as string)) ? finalTeams : preseasonTeams;

            var allAmericans = new List<AllAmerican>();
            foreach (var award in awards)
            {
                if (!teams.TryGetValue(award["AwardType"] as string, out var team))
                {
                    continue;
                }

                var player = award.GetReferenceDataByKey("Player").Value;
                if (player.TableId != playerTableId || !this.Players.Records[player.RowNumber].IsOnTeam)
                {
                    continue;
                }

                // national teams have no conference
                var conference = dynasty.Follow(award.GetReferenceDataByKey("Conference").Value);
                var positionField = award.GetFieldByKey("Position");

                allAmericans.Add(new AllAmerican
                {
                    AllAmericanTeam = team,
                    PlayerId = player.RowNumber,
                    // PositionE uses the classic position numbers (QB 0 ... P 20)
                    Position = (int)positionField.Offset.Enum.GetMemberByName(positionField.Value as string).Value,
                    ConferenceId = conference == null ? NationalAllAmericanConferenceId : CFBConference.ToClassicId(conference),
                });
            }

            return allAmericans
                .OrderBy(a => a.ConferenceId)
                .ThenBy(a => a.AllAmericanTeam)
                .ThenBy(a => a.Position)
                .ToList();
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
                    // CFB27 keeps the hometown as text, not an NCAA 14 cities.csv id (City); the roster csv is unquoted, so no commas
                    HomeTown = record.HomeTown?.Replace(",", string.Empty),
                    HomeState = record.HomeStateAbbreviation,
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
            var thr = this.CreateTeamHistoricRecords();

            var conferenceByTeamRow = ReadConferenceByTeamRow();
            return this.Teams.Records.ToDictionary(t => t.TeamId, t =>
            {
                var team = new Team(t, isPreseason);
                if (thr.TryGetValue(team.Id, out var records) && records.Count > 0)
                {
                    var key = records.Keys.Max();
                    var record = records[key];
                    team.PriorSeasonWin = record.Win;
                    team.PriorSeasonLoss = record.Loss;
                }

                if (conferenceByTeamRow.TryGetValue(t.Row, out var conference))
                {
                    team.ConferenceId = conference.ConferenceId;
                    team.DivisionId = conference.DivisionId;
                }
                else if (team.Id.IsFCS())
                {
                    team.ConferenceId = 17;
                }

                return team;
            });
        }

        public Dictionary<string, Coach> ReadCoaches(bool isPreseason = false)
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
                    // SeasonsWithTeam counts completed seasons; the preseason magazine (built from the week 0 save) shows the
                    // season about to start
                    YearsWithTeam = record.SeasonsWithTeam + (isPreseason ? 1 : 0),
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

        /// <summary>
        /// Each team's win-loss record per dynasty season, from Team.TeamSeriesHistory -> TeamHistoricSeriesYear (newest
        /// first; the entries for seasons before the dynasty point into game data that isn't in the save). The current
        /// season's entry holds its record so far, so it's only included once the season is over. Records from the
        /// continuation file come first, as in NCAA 14.
        /// </summary>
        public Dictionary<int, Dictionary<int, TeamSeasonRecord>> CreateTeamHistoricRecords()
        {
            if (this.teamHistoricRecords == null)
            {
                this.teamHistoricRecords = this.ExecuteCreateTeamHistoricRecords();
            }

            return this.teamHistoricRecords;
        }
        private Dictionary<int, Dictionary<int, TeamSeasonRecord>> ExecuteCreateTeamHistoricRecords()
        {
            var teamRecords = new Dictionary<int, Dictionary<int, TeamSeasonRecord>>();
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            var baseCalendarYear = Convert.ToInt32(seasonInfo["BaseCalendarYear"]);
            // the season in progress is included (0-0 at week 0), as NCAA14DataEngine includes it: the team page counts a head
            // coach's seasons by walking these back until they add up to his wins at the school
            var currentSeason = Convert.ToInt32(seasonInfo["CurrentSeasonYear"]);

            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex))
            {
                Dictionary<int, TeamSeasonRecord> seasons;
                if (ContinuationData.UsingContinuationData && ContinuationData.Instance != null && ContinuationData.Instance.TeamHistoricRecords.TryGetValue(team.TeamId, out var continued))
                {
                    seasons = continued;
                }
                else
                {
                    seasons = new Dictionary<int, TeamSeasonRecord>();
                }

                foreach (var season in dynasty.FollowArray(this.Teams.RawTable.Records[team.Row], "TeamSeriesHistory"))
                {
                    var calendarYear = Convert.ToInt32(season["Year"]);
                    if (calendarYear < baseCalendarYear || calendarYear > currentSeason)
                    {
                        continue;
                    }

                    var record = new TeamSeasonRecord
                    {
                        Year = calendarYear - baseCalendarYear + ContinuationData.ContinuationYear,
                        Win = Convert.ToInt32(season["Wins"]),
                        Loss = Convert.ToInt32(season["Losses"]),
                    };

                    seasons[record.Year] = record;
                }

                teamRecords[team.TeamId] = seasons;
            }

            return teamRecords;
        }

        /// <summary>SeasonGame.DayOfWeek -> the pages' day numbering (0 Monday ... 6 Sunday).</summary>
        private static readonly Dictionary<string, int> GameDays = new Dictionary<string, int>
        {
            { "Monday", 0 }, { "Tuesday", 1 }, { "Wednesday", 2 }, { "Thursday", 3 }, { "Friday", 4 }, { "Saturday", 5 }, { "Sunday", 6 },
        };

        /// <summary>
        /// The season's games from SeasonGame (filled in at week 0). A game's Stadium is empty when it's played at the home
        /// team's stadium; a regular-season game with any other stadium is a neutral-site game, and GameSite/SiteId name it.
        /// CFB27 has no augmented bowl games; bowl games get their bowl stadium as GameSite.
        /// </summary>
        public Dictionary<string, ScheduledGame> ReadSchedule(bool isPreseason)
        {
            var schedule = new Dictionary<string, ScheduledGame>();

            // unscheduled bowls and conference title games don't have their teams yet
            foreach (var sg in this.SeasonGames.Records.Where(g => !g.IsEmpty
                && !string.Equals(g.HomeTeam, BaseRecord.NoRefString)
                && !string.Equals(g.AwayTeam, BaseRecord.NoRefString)))
            {
                var homeStadium = this.Teams.Records[sg.HomeTeamId].Stadium;
                var playedAtHome = string.Equals(sg.Stadium, BaseRecord.NoRefString) || string.Equals(sg.Stadium, homeStadium);
                var stadiumId = playedAtHome
                    ? (string.Equals(homeStadium, BaseRecord.NoRefString) ? 0 : CFBStadium.ToStadiumId(homeStadium))
                    : CFBStadium.ToStadiumId(sg.Stadium);

                var game = new ScheduledGame
                {
                    PostSeason = sg.SeasonWeekType == "RegularSeason" ? 0 : 1,
                    AwayScore = sg.AwayScore,
                    HomeScore = sg.HomeScore,
                    AwayTeamId = sg.RealAwayTeamId,
                    HomeTeamId = sg.RealHomeTeamId,
                    DynastySeason = sg.SeasonYear,
                    GameNumber = sg.SeasonGameNum,
                    Week = sg.SeasonWeek,
                    Year = sg.SeasonYear + ContinuationData.ContinuationYear,
                    WentToOvertime = sg.IsOvertimeGame ? 1 : 0,
                    GameDay = GameDays.TryGetValue(sg.DayOfWeek ?? string.Empty, out var day) ? day : GameDays["Saturday"],
                    TimeOfDay = sg.TimeOfDay,
                    StadiumId = stadiumId,
                    GameSite = ReadStadiumName(stadiumId),
                };

                // regular-season games (including conference title games) away from the home team's stadium
                if (!playedAtHome && game.PostSeason == 0)
                {
                    game.IsNeutralSite = true;
                    game.SiteId = stadiumId;
                }

                schedule.Add(game.Key, game);
            }

            return schedule;
        }

        /// <summary>Record.Type for Team.GameStatRecords / SeasonStatRecords / CareerStatRecords.</summary>
        private static readonly (string Field, int Type)[] SchoolRecordTypes = { ("GameStatRecords", 0), ("SeasonStatRecords", 1), ("CareerStatRecords", 2) };

        /// <summary>Record.Description (TeamRecordKeys) for each PlayerStatRecords field.</summary>
        private static readonly (string Field, TeamRecordKeys Key)[] SchoolRecordStats =
        {
            ("RushYards", TeamRecordKeys.RushingYds),
            ("RushTDS", TeamRecordKeys.RushingTD),
            ("PassYards", TeamRecordKeys.PassYds),
            ("PassTDS", TeamRecordKeys.PassTD),
            ("ReceivingCatches", TeamRecordKeys.Receptions),
            ("ReceivingYards", TeamRecordKeys.RecYds),
            ("ReceivingTDS", TeamRecordKeys.RecTD),
            ("DefensiveSacks", TeamRecordKeys.Sacks),
            ("DefensiveInts", TeamRecordKeys.INT),
        };

        /// <summary>NCAA 14's year for records set before the dynasty ("Past").</summary>
        private const int PastRecordYear = 63;

        /// <summary>
        /// Each team's game, season and career records for the 9 classic stats, from Team.*StatRecords -> PlayerStatRecord.
        /// CFB27 keeps the holder's name and calendar year but not the opponent, so Opponent is empty.
        /// </summary>
        public Dictionary<int, List<Record>> ReadSchoolRecords(bool recreateUsingRecordsFile)
        {
            var schoolRecords = new Dictionary<int, List<Record>>();
            var baseCalendarYear = Convert.ToInt32(dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty)["BaseCalendarYear"]);

            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex))
            {
                var teamRecord = this.Teams.RawTable.Records[team.Row];
                var records = new List<Record>();

                foreach (var (field, type) in SchoolRecordTypes)
                {
                    var statRecords = dynasty.Follow(teamRecord.GetReferenceDataByKey(field).Value);
                    if (statRecords == null)
                    {
                        continue;
                    }

                    foreach (var (statField, key) in SchoolRecordStats)
                    {
                        var stat = dynasty.Follow(statRecords.GetReferenceDataByKey(statField).Value);
                        if (stat == null)
                        {
                            continue;
                        }

                        var calendarYear = Convert.ToInt32(stat["calendarYear"]);
                        var year = ToRecordYear(calendarYear, baseCalendarYear);
                        var holderYear = year == PastRecordYear ? calendarYear : Utility.StartingYear + year;

                        records.Add(new Record
                        {
                            Type = type,
                            Description = (int)key,
                            Holder = $"{holderYear} {ToRecordHolderName(stat)}",
                            Value = Convert.ToInt32(stat["statValue"]),
                            Opponent = string.Empty,
                            Year = year,
                        });
                    }
                }

                if (records.Count > 0)
                {
                    schoolRecords[team.TeamId] = records;
                }
            }

            return schoolRecords;
        }

        /// <summary>Records set during the dynasty get the dynasty year like the rest of the engine; older ones are "Past".</summary>
        private static int ToRecordYear(int calendarYear, int baseCalendarYear)
        {
            return calendarYear >= baseCalendarYear
                ? calendarYear - baseCalendarYear + ContinuationData.ContinuationYear
                : PastRecordYear;
        }

        /// <summary>"T.Davis" from a PlayerStatRecord.</summary>
        private static string ToRecordHolderName(FranchiseFileRecord stat)
        {
            var firstName = stat["firstName"] as string;
            return $"{(string.IsNullOrEmpty(firstName) ? string.Empty : firstName.Substring(0, 1) + ".")}{stat["lastName"]}";
        }

        /// <summary>
        /// Nat_Rec_Desc code for each League.Player*StatRecords field. CFB27's single-game rushing, passing and receiving TD
        /// records have no classic code, so they aren't listed.
        /// </summary>
        private static readonly (string Field, (string Stat, int Description)[] Stats)[] NcaaRecordCodes =
        {
            ("PlayerGameStatRecords", new[]
            {
                ("PassYards", 9), ("RushYards", 10), ("ReceivingYards", 11), ("ReceivingCatches", 12), ("DefensiveSacks", 13), ("DefensiveInts", 14),
            }),
            ("PlayerSeasonStatRecords", new[]
            {
                ("RushYards", 21), ("RushTDS", 22), ("PassYards", 23), ("PassTDS", 24), ("ReceivingCatches", 26),
                ("ReceivingYards", 27), ("ReceivingTDS", 28), ("DefensiveSacks", 29), ("DefensiveInts", 30),
            }),
            ("PlayerCareerStatRecords", new[]
            {
                ("RushYards", 32), ("RushTDS", 33), ("PassYards", 34), ("PassTDS", 35), ("ReceivingCatches", 36),
                ("ReceivingYards", 37), ("ReceivingTDS", 38), ("DefensiveSacks", 39), ("DefensiveInts", 40),
            }),
        };

        /// <summary>
        /// The national record book from League.PlayerGameStatRecords / PlayerSeasonStatRecords / PlayerCareerStatRecords.
        /// CFB27 only keeps player records for 9 stats, so the classic team, coach, longest-play, QB rating and field goal
        /// records aren't available. Holder is "1988 B.Sanders (Oklahoma State)"; CFB27 doesn't keep the opponent.
        /// </summary>
        public List<Record> ReadNcaaRecords()
        {
            var records = new List<Record>();
            var baseCalendarYear = Convert.ToInt32(dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty)["BaseCalendarYear"]);
            var league = dynasty.GetPrimaryTable("League").Records.First(r => !r.IsEmpty);

            foreach (var (field, stats) in NcaaRecordCodes)
            {
                var statRecords = dynasty.Follow(league.GetReferenceDataByKey(field).Value);
                if (statRecords == null)
                {
                    continue;
                }

                foreach (var (statField, description) in stats)
                {
                    var stat = dynasty.Follow(statRecords.GetReferenceDataByKey(statField).Value);
                    if (stat == null)
                    {
                        continue;
                    }

                    // the page shows "Past" for pre-dynasty years, so the holder keeps the year too (as school records do)
                    var calendarYear = Convert.ToInt32(stat["calendarYear"]);
                    var year = ToRecordYear(calendarYear, baseCalendarYear);
                    var holderYear = year == PastRecordYear ? calendarYear : Utility.StartingYear + year;

                    records.Add(new Record
                    {
                        Description = description,
                        Holder = $"{holderYear} {ToRecordHolderName(stat)} ({stat["teamName"]})",
                        Value = Convert.ToInt32(stat["statValue"]),
                        Opponent = string.Empty,
                        Year = year,
                    });
                }
            }

            return records;
        }

        public void ReadGameStats(Dictionary<string, ScheduledGame> games)
        {
            return;
        }

        /// <summary>
        /// Every season's conference champions from the league history: LeagueHistoryManager.LeagueHistory -> YearSummary
        /// (one per dynasty year, PeriodIndex) -> ConferenceChampions -> LeagueHistoryConferenceChampion. A season's entries
        /// appear once its title games are played; the current season's YearSummary is empty until then.
        /// </summary>
        public List<ConferenceChampion> ReadConferenceChamps()
        {
            var champions = new List<ConferenceChampion>();
            var manager = dynasty.GetPrimaryTable("LeagueHistoryManager").Records.First(r => !r.IsEmpty);

            foreach (var yearSummary in dynasty.FollowArray(manager, "LeagueHistory"))
            {
                var year = Convert.ToInt32(yearSummary["PeriodIndex"]) + ContinuationData.ContinuationYear;
                foreach (var champion in dynasty.FollowArray(yearSummary, "ConferenceChampions"))
                {
                    var conference = dynasty.Follow(champion.GetReferenceDataByKey("ConferenceIdentity").Value);
                    var team = dynasty.Follow(champion.GetReferenceDataByKey("WinningTeamIdentity").Value);
                    if (team == null || CFBConference.ToClassicId(conference) < 0)
                    {
                        continue;
                    }

                    champions.Add(new ConferenceChampion
                    {
                        ConferenceId = CFBConference.ToClassicId(conference),
                        TeamId = CFBTeam.TeamRowToOldTeamIdMap[team.Index],
                        Year = year,
                    });
                }
            }

            return champions;
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

        /// <summary>No team, as NCAA 14 recorded it.</summary>
        private const int NoTeamId = 1023;

        /// <summary>
        /// Every ranked recruit, keyed like NCAA 14: by RecruitId (the Recruit table row) and again by 10000 + national rank.
        /// </summary>
        public Dictionary<int, Recruit> ReadRecruits()
        {
            var recruits = new Dictionary<int, Recruit>();

            // signed and committed recruits are only linked to their school through Team.CommittedPlayers
            var committedTeamByPlayerRow = new Dictionary<int, int>();
            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex))
            {
                foreach (var row in ReadCommittedPlayerRows(team))
                {
                    committedTeamByPlayerRow[row] = team.TeamId;
                }
            }

            var recruitTable = dynasty.GetTable<CFBRecruit>(TableName.Recruit, CFBRecruit.Transform);
            foreach (var record in recruitTable.Records.Where(r => r.IsRanked))
            {
                var player = this.Players.Records[record.PlayerRow];
                var topSchools = record.TopSchoolTeamIndexes.Select(t => CFBTeam.TeamIndexToOldIdMap[t]).Concat(Enumerable.Repeat(NoTeamId, 3)).ToList();

                var recruit = new Recruit
                {
                    RecruitId = record.Row,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    PositionValue = player.Position,
                    Rank = record.NationalRank,
                    PositionRank = record.PositionRank,
                    StarRating = player.StarRating,
                    RealOVR = player.OverallRating,
                    // CFB27 has no pre-scouting overall; the game flags gems and busts instead
                    PreScoutOVR = player.OverallRating,
                    IsGem = record.QualityModifier == "GEM",
                    IsBust = record.QualityModifier == "BUST",
                    // NCAA 14's ATH recruits; in CFB27 a recruit listed with a second position
                    IsAthlete = record.HasAlternatePosition,
                    PlayerYear = player.SchoolYear,
                    Tendency = ToQbTendency(player.PlayerType),
                    State = player.HomeStateId,
                    Hometown = player.HomeTown,
                    Homestate = player.HomeStateAbbreviation,
                    // NCAA 14 city ids are grouped by state (state * 256 + city); without a city, use the state's first id
                    // so the recruiting page's players-by-state counts still work
                    HometownValue = player.HomeStateId >= 0 && player.HomeStateId <= 50 ? (player.HomeStateId * 256) + 1 : 0,
                    CommittedTeam = committedTeamByPlayerRow.TryGetValue(record.PlayerRow, out var committedTeam) ? committedTeam : NoTeamId,
                    Team1 = topSchools[0],
                    Team2 = topSchools[1],
                    Team3 = topSchools[2],
                };

                recruits[recruit.RecruitId] = recruit;
                recruits[10000 + recruit.Rank] = recruit;
            }

            return recruits;
        }

        /// <summary>NCAA 14 QB tendency from the CFB27 archetype: 0 pocket passer, 1 balanced, 2 scrambler; -1 for non-QBs.</summary>
        private static int ToQbTendency(string playerType)
        {
            switch (playerType)
            {
                case "QB_FieldGeneral": return 0;
                case "QB_Improviser": return 1;
                case "QB_Scrambler":
                case "QB_PureScrambler": return 2;
                default: return -1;
            }
        }

        /// <summary>
        /// Award id (award_names.xml) for each CFB27 AwardType, with its ranking list on the Awards hub. 0-16 are the classic
        /// NCAA 14 awards (BEST_SR is the Jet Award, the classic best returner); 17+ are awards new in CFB27.
        /// </summary>
        private static readonly (int Id, string AwardType, string RankingField)[] AwardMap =
        {
            (0, "HEISMAN", "HeismanRanking"),
            (1, "BEST_PLAYER", "BestPlayerRankingArray"),          // Maxwell
            (2, "BEST_POTY", "BestPOTYRankingArray"),              // Walter Camp
            (3, "BEST_DEF_1", "BestDef1RankingArray"),             // Bednarik
            (4, "BEST_DEF_2", "BestDef2RankingArray"),             // Nagurski
            (5, "BEST_QB", "BestQBRankingArray"),                  // Davey O'Brien
            (6, "BEST_RB", "BestRBRankingArray"),                  // Doak Walker
            (7, "BEST_REC", "BestWRRankingArray"),                 // Biletnikoff
            (8, "BEST_TE", "BestTERankingArray"),                  // Mackey
            (9, "BEST_IL", "BestILRankingArray"),                  // Outland
            (10, "BEST_C", "BestCRankingArray"),                   // Rimington
            (11, "BEST_DL", "BestDLRankingArray"),                 // Lombardi
            (12, "BEST_LB", "BestLBRankingArray"),                 // Butkus
            (13, "BEST_DB", "BestDBRankingArray"),                 // Thorpe
            (14, "BEST_KICK", "BestKickRankingArray"),             // Groza
            (15, "BEST_PUNT", "BestPuntRankingArray"),             // Ray Guy
            (16, "BEST_SR", "BestSRRankingArray"),                 // Jet Award (best returner)
            (17, "BEST_DE", "BestDERankingArray"),                 // Edge Rusher of the Year
            (18, "BEST_SR_QB", "BestSRQBRankingArray"),            // Johnny Unitas Golden Arm
            (19, "MOST_VERSATILE", "MostVersatileRankingArray"),   // Paul Hornung
            (20, "BEST_FRESHMAN_POTY", "BestFreshmanPOTYRankingArray"), // Shaun Alexander
            (21, "BEST_ACADEMIC", "BestAcademicRankingArray"),     // William V. Campbell
        };

        /// <summary>
        /// The current season's award rankings from the Awards hub (each award's ranked list, winner first; the Heisman has
        /// its own HeismanRanking). The lists fill in during the season and are final once the awards are given, so an
        /// award with no list yet falls back to its PlayerAward winner. Only players on FBS rosters are included, since
        /// the awards page looks them up in PlayerDB. CFB27 doesn't link past seasons' winners to players.
        /// </summary>
        public Dictionary<int, List<Award>> ReadAwards()
        {
            var awards = new Dictionary<int, List<Award>>();
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            var year = Convert.ToInt32(seasonInfo["CurrentYear"]) + ContinuationData.ContinuationYear;
            var hub = dynasty.GetPrimaryTable("Awards").Records.First(r => !r.IsEmpty);
            var playerTableId = this.Players.RawTable.Header.TableId;

            // season winners, for awards whose ranking list is empty
            var winners = dynasty.GetPrimaryTable("PlayerAward").Records
                .Where(r => !r.IsEmpty && (r["Period"] as string) == "Season")
                .GroupBy(r => r["AwardType"] as string)
                .ToDictionary(g => g.Key, g => g.Select(r => r.GetReferenceDataByKey("Player").Value).Where(p => p.TableId == playerTableId).Select(p => p.RowNumber).ToList());

            foreach (var (id, awardType, rankingField) in AwardMap)
            {
                // the Heisman list holds HeismanAwardRanking entries; the others point straight at players
                var ranked = dynasty.FollowArray(hub, rankingField)
                    .Select(entry => entry.Parent.Header.TableId == playerTableId ? entry : dynasty.Follow(entry.GetReferenceDataByKey("Player").Value))
                    .Where(player => player != null && player.Parent.Header.TableId == playerTableId)
                    .Select(player => player.Index)
                    .ToList();

                if (ranked.Count == 0 && winners.TryGetValue(awardType, out var winnerRows))
                {
                    ranked = winnerRows;
                }

                var list = ranked
                    .Where(row => this.Players.Records[row].IsOnTeam)
                    .Select((row, i) => new Award { Id = id, PlayerId = row, Rank = i + 1, Year = year })
                    .ToList();

                if (list.Count > 0)
                {
                    awards[id] = list;
                }
            }

            return awards;
        }

        public int CalculateRosterSpots(RecruitClassRanking ranking)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, TeamStat> ReadTeamStats()
        {
            return new Dictionary<int, TeamStat>();
        }

        public void CommitTeamRecords()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// NFL draft picks per college and class, from the DraftPick table. CFB27 keeps a rolling window of the last three
        /// drafts: YearOffset is how many seasons ago the class played (CurrentSeasonYear - YearOffset), Round is 0-based
        /// (0 = first round) and Team is the college. A placeholder row with no team is skipped.
        /// </summary>
        public Dictionary<int, DraftClass[]> ReadDraftHistory()
        {
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            var baseCalendarYear = Convert.ToInt32(seasonInfo["BaseCalendarYear"]);
            var currentSeasonYear = Convert.ToInt32(seasonInfo["CurrentSeasonYear"]);
            var teamTableId = this.Teams.RawTable.Header.TableId;

            return dynasty.GetPrimaryTable("DraftPick").Records
                .Where(r => !r.IsEmpty)
                .Select(r => new { Pick = r, Team = r.GetReferenceDataByKey("Team").Value })
                .Where(p => p.Team.TableId == teamTableId)
                .GroupBy(p => CFBTeam.TeamRowToOldTeamIdMap[p.Team.RowNumber])
                .ToDictionary(
                    team => team.Key,
                    team => team
                        .GroupBy(p => Convert.ToInt32(p.Pick["YearOffset"]))
                        .OrderBy(year => year.Key)
                        .Select(year =>
                        {
                            var rounds = year.Select(p => Convert.ToInt32(p.Pick["Round"])).ToList();
                            return new DraftClass
                            {
                                DynastyYear = currentSeasonYear - year.Key - baseCalendarYear + ContinuationData.ContinuationYear,
                                Round1 = rounds.Count(r => r == 0),
                                Round2 = rounds.Count(r => r == 1),
                                Round3 = rounds.Count(r => r == 2),
                                RoundLater = rounds.Count(r => r > 2),
                            };
                        })
                        .ToArray());
        }

        public Dictionary<int, MediaCoverage[]> ReadMediaCoverage()
        {
            // CFBStory.TeamId is the Team table row; pages look teams up by their old id. Teams without a story are left out
            // (CFB27 writes few stories, mostly next-game previews, rather than a preview per team)
            return this.Stories.Records
                .Where(s => !s.IsEmpty && !string.Equals(s.Team, BaseRecord.NoRefString))
                .Where(mr => mr.Category != "NEXT_GAME")  // we don't want week 1 game info
                .GroupBy(s => CFBTeam.TeamRowToOldTeamIdMap[s.TeamId])
                .ToDictionary(
                g => g.Key,
                    group => group
                        .Select(mr =>
                        new MediaCoverage
                        {
                            TeamId = group.Key,
                            Headline = mr.Header,
                            Content = mr.Tag,
                        })
                        .ToArray()
                );
        }

        /// <summary>
        /// DepthChart field -> position number. QB..P (0-20) and KR/PR/KOS/LS (21-24) use the classic numbers; CFB27 has
        /// LS 21, KR 22, PR 23, KOS 24. The sub-package positions keep CFB27's own numbers (3DRB 25 ... SLCB 34).
        /// </summary>
        private static readonly Dictionary<string, int> DepthChartPositions = new Dictionary<string, int>
        {
            { "QB", 0 }, { "HB", 1 }, { "FB", 2 }, { "WR", 3 }, { "TE", 4 }, { "LT", 5 }, { "LG", 6 }, { "C", 7 }, { "RG", 8 }, { "RT", 9 },
            { "LE", 10 }, { "RE", 11 }, { "DT", 12 }, { "LOLB", 13 }, { "MLB", 14 }, { "ROLB", 15 }, { "CB", 16 }, { "FS", 17 }, { "SS", 18 },
            { "K", 19 }, { "P", 20 },
            { "KR", 21 }, { "PR", 22 }, { "KOS", 23 }, { "LS", 24 },
            { "3DRB", 25 }, { "GAD", 26 }, { "PWHB", 27 }, { "SLWR", 28 }, { "RLE", 29 }, { "RRE", 30 }, { "RDT", 31 }, { "NT", 32 },
            { "SUBLB", 33 }, { "SLCB", 34 },
        };

        /// <summary>
        /// Each FBS team's depth chart: Team.DepthChart has a Player[] per position, already in depth order (depth 0 is the
        /// starter). Players not on the team's roster are skipped so PlayerDB lookups work.
        /// </summary>
        public Dictionary<int, Dictionary<int, DepthChartPosition[]>> ReadDepthCharts()
        {
            var depthCharts = new Dictionary<int, Dictionary<int, DepthChartPosition[]>>();
            var playerTableId = this.Players.RawTable.Header.TableId;

            foreach (var team in this.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex))
            {
                var depthChart = dynasty.Follow(this.Teams.RawTable.Records[team.Row].GetReferenceDataByKey("DepthChart").Value);
                if (depthChart == null)
                {
                    continue;
                }

                var positions = new Dictionary<int, DepthChartPosition[]>();
                foreach (var position in DepthChartPositions)
                {
                    var players = dynasty.FollowArray(depthChart, position.Key)
                        .Where(p => p.Parent.Header.TableId == playerTableId && this.Players.Records[p.Index].TeamIndex == team.TeamIndex)
                        .Select((p, depth) => new DepthChartPosition { PlayerId = p.Index, PlayerPosition = position.Value, PositionDepth = depth })
                        .ToArray();

                    if (players.Length > 0)
                    {
                        positions[position.Value] = players;
                    }
                }

                depthCharts[team.TeamId] = positions;
            }

            return depthCharts;
        }

        public Dictionary<int, int> FindCoachesOnHotSeat()
        {
            return new Dictionary<int, int>();
        }

        public string ReadStadiumName(int siteId)
        {
            return ReadStadiums().TryGetValue(siteId, out var stadium) ? stadium.Name : string.Empty;
        }
    }
}