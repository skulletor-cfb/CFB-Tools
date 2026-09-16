using System.Collections.Generic;
using Newtonsoft.Json;

namespace CFB27.Data.Model
{
    public class CFBPlayer : BaseRecord
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string SchoolYear { get; set; }
        public string RedshirtStatus { get; set; }
        public int TeamIndex { get; set; }
        public int JerseyNum { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int OverallRating { get; set; }
        public int AwarenessRating { get; set; }
        public int SpeedRating { get; set; }
        public int AccelerationRating { get; set; }
        public int AgilityRating { get; set; }
        public int StrengthRating { get; set; }

        [JsonIgnore]
        public bool HasTeam => CFBTeam.TeamIdToOldIdMap.ContainsKey(this.TeamIndex);

        [JsonIgnore]
        public int TeamId => CFBTeam.TeamIdToOldIdMap[this.TeamIndex];

        /// <summary>
        /// maps the CFB27 position enum names to the classic position ids used throughout the site (see Utility.ToPositionName).
        /// positions with no classic equivalent are folded into their closest base position.
        /// </summary>
        public static readonly Dictionary<string, int> PositionMap = new Dictionary<string, int>
        {
            { "QB", 0 },
            { "HB", 1 },
            { "3DRB", 1 },
            { "PWHB", 1 },
            { "FB", 2 },
            { "WR", 3 },
            { "SLWR", 3 },
            { "GAD", 3 },
            { "TE", 4 },
            { "LT", 5 },
            { "LG", 6 },
            { "C", 7 },
            { "RG", 8 },
            { "RT", 9 },
            { "LE", 10 },
            { "RLE", 10 },
            { "RE", 11 },
            { "RRE", 11 },
            { "DT", 12 },
            { "RDT", 12 },
            { "NT", 12 },
            { "LOLB", 13 },
            { "MLB", 14 },
            { "SUBLB", 14 },
            { "ROLB", 15 },
            { "CB", 16 },
            { "SLCB", 16 },
            { "FS", 17 },
            { "SS", 18 },
            { "K", 19 },
            { "P", 20 },
            { "LS", 20 },
            { "KR", 21 },
            { "PR", 22 },
            { "KOS", 19 },
        };

        [JsonIgnore]
        public int PositionId => PositionMap.TryGetValue(this.Position, out var pos) ? pos : -1;

        private static readonly Dictionary<string, int> SchoolYearMap = new Dictionary<string, int>
        {
            { "Freshman", 0 },
            { "Sophomore", 1 },
            { "Junior", 2 },
            { "Senior", 3 },
        };

        [JsonIgnore]
        public int SchoolYearValue => SchoolYearMap.TryGetValue(this.SchoolYear, out var year) ? year : 0;

        [JsonIgnore]
        public bool IsRedShirt => string.Equals(this.RedshirtStatus, "Current");
    }
}
