using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public class Award
    {
        //Number,Name,Year,Position,Height,Weight,Ovr,Team,TeamId,AwardId,AwardName
        public int TeamId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public int AwardId { get; set; }
        public string AwardName { get; set; }
        public string Class { get; set; }
        public int Year { get; set; }

        public static Award Generate(string[] parts, int year = 0)
        {
            return new Award()
            {
                Year = year,
                Name = parts[1],
                Class = parts[2],
                Position = parts[3],
                TeamId = parts[8].ToInt(),
                AwardId = parts[9].ToInt(),
                AwardName = parts[10]
            };
        }
    }
}
