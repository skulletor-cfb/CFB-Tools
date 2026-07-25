using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class CFB27DataEngine : IDataEngine
    {
        public CFB27DataEngine(string directory)
        {
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Dictionary<int, Dictionary<int, DepthChartPosition[]>> ReadDepthCharts()
        {
            throw new NotImplementedException();
        }
    }
}