using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public class AllAmerican
    {
        public const int AllAmericanTeamConfId = 14;
        // ConfId,ConferenceName,PlayerName,TeamNum,PlayerTeam,PositionName,Position,TeamId,Height,Weight,PlayerYear,DisplayPosition,Ovr
        public string AATeam { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public int TeamId { get; set; }
        public int Year { get; set; }
        public int OVR { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public int AATeamInt { get; set; }
        public int Pos { get; set; }
        public int ConfId { get; set; }

        public static AllAmerican Generate(string[] parts, int year = 0)
        {
            var team = string.Empty;

            switch (parts[3].ToInt())
            {
                case 0:
                    team = "1st Team All-American";
                    break;
                case 1:
                    team = "2nd Team All-American";
                    break;
                case 2:
                    team = "Freshman All-American";
                    break;
                default:
                    team = "error";
                    break;
            }

            return new AllAmerican
            {
                ConfId = parts[0].ToInt(),
                AATeamInt = parts[3].ToInt(),
                Year = year,
                Name = parts[2],
                AATeam = team,
                TeamId = parts[7].ToInt(),
                Height = parts[8],
                Weight = parts[9],
                Class = parts[10],
                Pos = parts[6].ToInt(),
                Position = parts[11],
                OVR = parts[12].ToInt(),
            };
        }
    }
}
