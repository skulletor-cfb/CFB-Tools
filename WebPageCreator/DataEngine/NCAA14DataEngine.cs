using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class NCAA14DataEngine: IDataEngine
    {
        public NCAA14DataEngine(MaddenDatabase db)
        {
            this.MaddenDatabase = db;
        }

        public MaddenDatabase MaddenDatabase { get; }

        public bool IsSeasonOver()
        {
            // check to see if the season is still going on
            // BUGBUG if the NCG has been played, but the week hasn't advanced this info is incorrect, but that's probably ok
            var record = MaddenDatabase.lTables[161].lRecords.OrderByDescending(mr => mr.lEntries[12].Data.ToInt32()).Take(1).First();

            // if the score is 0-0 the season is not over
            return !(record.lEntries[1].Data.ToInt32() == 0 && record.lEntries[2].Data.ToInt32() == 0);
        }

        public int ReadBowlChampions(bool didNotEnterBowlChampLoop, int currentYear, Dictionary<string, BowlChampion> bowlChampions)
        {
            var table = this.MaddenDatabase.lTables[0];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                didNotEnterBowlChampLoop = false;
                var record = table.lRecords[i];
                var bc = new BowlChampion
                {
                    TeamId = record.GetInt(0).GetRealTeamId(),
                    Year = record.GetInt(1) + ContinuationData.ContinuationYear,
                    BowlId = record.GetInt(2)
                };

                if (Bowl.BowlIdOverrides.ContainsKey(bc.BowlId) && Bowl.BowlIdOverrides[bc.BowlId].Item2 <= bc.Year)
                    bc.BowlId = Bowl.BowlIdOverrides[bc.BowlId].Item1;


                if (!bowlChampions.ContainsKey(bc.GetKey()))
                {
                    bowlChampions.Add(bc.GetKey(), bc);
                }

                currentYear = Math.Max(currentYear, bc.Year);
            }

            return currentYear;
        }
    }
}
