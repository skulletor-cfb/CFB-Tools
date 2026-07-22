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
    }
}