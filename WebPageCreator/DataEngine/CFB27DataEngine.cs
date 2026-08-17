using CFB27.Data.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EA_DB_Editor
{
    public static class JsonHelpers
    {
        public static CFBTable<T> ReadJson<T>(this string file) where T : BaseRecord
        {
            return JsonConvert.DeserializeObject<CFBTable<T>>(File.ReadAllText(file));
        }

        public static string WriteJson(this object payload)
        {
            return JsonConvert.SerializeObject(payload, Formatting.Indented);
        }
    }

    public class CFB27DataEngine : IDataEngine
    {
        private readonly string directory;
        private readonly DirectoryInfo directoryInfo;
        private const string TeamFile = "*_Team.json";
        public CFBTable<CFBTeam> Teams { get; private set; }
        public Dictionary<int,CFBTable<CFBTeamHistoricalData>> TeamHistoricalData { get; private set; }

        public Dictionary<int, string> TeamNames => this.Teams.Records.ToDictionary(t => t.Row, t => t.DisplayName);

        public CFB27DataEngine(string rootDirectory)
        {
            this.directory = Path.Combine(rootDirectory, "JSON");
            this.directoryInfo = new DirectoryInfo(this.directory);
            this.Teams = this.GetFile(TeamFile).ReadJson<CFBTeam>();
            this.TeamHistoricalData = this.GetTeamHistoricalData();
        }

        public Dictionary<string, Bowl> CreateBowlTable()
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, TeamSchedule> CreateTeamSchedule(bool isPreseason)
        {
            throw new NotImplementedException();
        }

        public bool IsSeasonOver()
        {
            throw new NotImplementedException();
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
            const string storiesFile = "0185_Story.json";
            throw new NotImplementedException();
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

        private string GetFile(string pattern)
        {
            var file = directoryInfo.GetFiles(pattern).OrderByDescending(f => f.Length).First();
            return file.FullName;
        }

        private string[] GetFiles(string pattern)
        {
            var files = directoryInfo.GetFiles(pattern).Select(f => f.FullName).ToArray();
            return files;
        }

        private Dictionary<int,CFBTable<CFBTeamHistoricalData>> GetTeamHistoricalData()
        {
            var files = this.GetFiles("*_TeamHistoricalData.json");
            var data = files.Select(f => f.ReadJson<CFBTeamHistoricalData>()).ToArray();
            var result = data.ToDictionary(t => t.Header.tableId);

            //match historical data to teams
            foreach (var team in Teams.Records)
            {
                var tableId = this.ConvertToTableId(team.TeamHistoricalData);
                team.HistoricalData = result[tableId].Records[0];
            }

            return result;
        }

        private int ConvertToTableId(string tableId, int prefixLength = 15)
        {
            return Convert.ToInt32(tableId.Substring(0, prefixLength), 2);
        }
    }
}