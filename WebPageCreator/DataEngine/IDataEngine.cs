using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public interface IDataEngine
    {
        /// <summary>
        /// returns an annual list of bowl champions
        /// </summary>
        /// <param name="didNotEnterBowlChampLoop"></param>
        /// <param name="currentYear"></param>
        /// <param name="bowlChampions"></param>
        /// <returns>returns the current year</returns>
        int ReadBowlChampions(bool didNotEnterBowlChampLoop, int currentYear, Dictionary<string, BowlChampion> bowlChampions);

        /// <summary>
        /// checks if the current season is over
        /// </summary>
        /// <returns>return true if it is over</returns>
        bool IsSeasonOver();

        /// <summary>
        /// Reads metadata about bowls
        /// </summary>
        /// <returns></returns>
        Dictionary<string, Bowl> CreateBowlTable();

        /// <summary>
        /// creates the team schedule
        /// </summary>
        /// <returns></returns>
        Dictionary<int, TeamSchedule> CreateTeamSchedule(bool isPreseason);

        /// <summary>
        /// returns the list of all americans
        /// </summary>
        /// <returns></returns>
        List<AllAmerican> CreateAllAmericans();

        /// <summary>
        /// Read the database for players
        /// </summary>
        /// <param name="Rosters"></param>
        /// <param name="Players"></param>
        void CreatePlayers(Dictionary<int, List<Player>> Rosters, Dictionary<int, Player> Players);

        /// <summary>
        /// get the stats
        /// </summary>
        void ReadStats();
    }
}
