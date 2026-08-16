using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public class ConferenceChamp
    {
        public int DynastyYear { get; set; }
        public int Year { get; set; }
        public int TeamId { get; set; }
        public int ConfId { get; set; }

        //Year,Team,TeamId,Conference,ConferenceId
        public static ConferenceChamp Generate(string[] parts, int year = 0)
        {
            var dy = parts[0].ToInt();
            return new ConferenceChamp
            {
                Year = BowlChamp.StartingYear + dy,
                DynastyYear = parts[0].ToInt(),
                ConfId = parts[4].ToInt(),
                TeamId = parts[2].ToInt()
            };
        }
    }
}
