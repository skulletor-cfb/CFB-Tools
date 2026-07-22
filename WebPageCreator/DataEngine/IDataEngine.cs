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
    }
}
