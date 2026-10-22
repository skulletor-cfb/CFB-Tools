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
            throw new NotImplementedException();
        }

        public void ReadStats()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Conference> ReadConferenceMetadata()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, Team> ReadTeams(bool isPreseason)
        {
            return this.Teams.Records.ToDictionary(t => t.TeamId, t => new Team(t, isPreseason));
        }

        public Dictionary<string, Coach> ReadCoaches()
        {
            throw new NotImplementedException();
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

        public Dictionary<int, Stadium> ReadStadiums()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, RecruitClassRanking> ReadRecruitClasses()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}