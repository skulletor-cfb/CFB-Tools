using CFB27.Data.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EA_DB_Editor
{

    public class CFB27DataEngine : IDataEngine
    {
        private readonly string directory;
        private readonly DirectoryInfo directoryInfo;
        private const string TeamFile = "Team";
        public CFBTable<CFBTeam> Teams { get; private set; }
        public Dictionary<int, CFBTable<CFBTeamHistoricalData>> TeamHistoricalData { get; private set; }
        public CFBTable<CFBBowl> Bowls { get; private set; }
        public CFBTable<CFBSeasonGame> SeasonGames { get; private set; }
        public Dictionary<int, CFBSeasonGame> BowlSeasonGames { get; }
        public CFBTable<CFBStory> Stories { get; }

        public Dictionary<int, string> TeamNames => this.Teams.Records.ToDictionary(t => t.Row, t => t.DisplayName);

        public CFB27DataEngine(string rootDirectory)
        {
            this.directory = Path.Combine(rootDirectory, "JSON");
            this.directoryInfo = new DirectoryInfo(this.directory);
            this.Teams = this.GetFile(TeamFile).ReadJson<CFBTeam>();
            this.TeamHistoricalData = this.GetTeamHistoricalData();
            this.SeasonGames = this.GetFile("SeasonGame").ReadJson<CFBSeasonGame>();
            this.BowlSeasonGames = this.SeasonGames.Records.Where(g => !g.IsEmpty && g.IsBowlGame).ToDictionary(g => g.BowlId);
            this.Bowls = this.GetFile("BowlGame").ReadJson<CFBBowl>();
            this.Stories = this.GetFile("Story").ReadJson<CFBStory>();
        }

        public Dictionary<string, Bowl> CreateBowlTable()
        {
            var bowls = new Dictionary<string, Bowl>();

            foreach (var b in this.Bowls.Records)
            {
                if (string.Equals(string.Empty, b.AssetName))
                {
                    continue;
                }

                b.StadiumId = BowlSeasonGames[b.Row].Stadium.CFBToInt64();

                var bowl = new Bowl
                {
                    Name = b.Name,
                    Week = BowlSeasonGames[b.Row].SeasonWeek,
                    Game = BowlSeasonGames[b.Row].SeasonGameNum,
                    ConferenceTieInId1 = b.Conference1.ToRowId(),
                    ConferenceTieInId2 = b.Conference2.ToRowId(),
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

        public int ReadBowlChampions(bool didNotEnterBowlChampLoop, int currentYear, Dictionary<string, BowlChampion> bowlChampions)
        {
            throw new NotImplementedException();
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

        private string GetFile(string fileName)
        {
            var file = directoryInfo.GetFiles($"*_{fileName}.json").OrderByDescending(f => f.Length).First();
            return file.FullName;
        }

        private string[] GetFiles(string fileName)
        {
            var files = directoryInfo.GetFiles($"*_{fileName}.json").Select(f => f.FullName).ToArray();
            return files;
        }

        private Dictionary<int, CFBTable<CFBTeamHistoricalData>> GetTeamHistoricalData()
        {
            var files = this.GetFiles("TeamHistoricalData");
            var data = files.Select(f => f.ReadJson<CFBTeamHistoricalData>()).ToArray();
            var result = data.ToDictionary(t => t.Header.tableId);

            //match historical data to teams
            foreach (var team in Teams.Records)
            {
                var tableId = team.TeamHistoricalData.ToTableId();
                team.HistoricalData = result[tableId].Records[0];
            }

            return result;
        }
    }
}