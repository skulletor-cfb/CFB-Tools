using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public class BowlChamp
    {
        public static int StartingYear = 2013;
        public int DynastyYear { get; set; }
        public int Year { get; set; }
        public int TeamId { get; set; }
        public int BowlId { get; set; }

        //Year,Name,BowlId,Team,TeamId
        public static BowlChamp Generate(string[] parts, int year = 0)
        {
            var dy = parts[0].ToInt();
            return new BowlChamp
            {
                DynastyYear = dy,
                Year = dy + StartingYear,
                BowlId = parts[2].ToInt(),
                TeamId = parts[4].ToInt()
            };
        }
    }
}
