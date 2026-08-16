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
        public static CFB27Table<T> ReadJson<T>(this string file) where T : BaseRecord
        {
            return JsonConvert.DeserializeObject<CFB27Table<T>>(File.ReadAllText(file));
        }

        public static string WriteJson(this object payload)
        {
            return JsonConvert.SerializeObject(payload, Formatting.Indented);
        }
    }

    public class CFB27DataEngine : IDataEngine
    {
        private readonly string directory;
        private const string TeamFile = "2252_Team.json";
        private CFB27Table<CFB27Team> teams;

        public Dictionary<int, string> TeamNames => this.teams.Records.ToDictionary(t => t.Row, t => t.DisplayName);

        public CFB27DataEngine(string directory)
        {
            this.directory = Path.Combine(directory, "JSON");
            this.teams = Path.Combine(this.directory, TeamFile).ReadJson<CFB27Team>();
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
            return this.teams.Records.ToDictionary(t => t.TeamId, t => new Team(t, isPreseason));
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
    }
}