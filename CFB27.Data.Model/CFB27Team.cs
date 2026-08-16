using Newtonsoft.Json;
using System.Collections.Generic;

namespace CFB27.Data.Model
{
    [JsonObject]
    public class CFB27Team : BaseRecord
    {
        [JsonProperty]
        public string DisplayName { get; set; }

        [JsonProperty]
        public int TeamIndex { get; set; }
        
        [JsonProperty]
        public string NickName { get; set; }

        [JsonProperty]
        public string ShortName { get; set; }

        [JsonProperty]
        public int ToughestPlacesScore { get; set; }

        [JsonProperty]
        public int AverageAttendance { get; set; }

        [JsonProperty]
        public int ConfWin { get; set; }

        [JsonProperty]
        public int ConfLoss { get; set; }

        [JsonProperty]
        public int MediaPoll_CurrentRank { get; set; }

        [JsonProperty]
        public int MediaPoll_LastWeeksRank { get; set; }

        [JsonProperty]
        public int CFPPoll_CurrentRank { get; set; }

        [JsonProperty]
        public int CFPPoll_LastWeeksRank { get; set; }

        [JsonProperty]
        public int CoachesPoll_CurrentRank { get; set; }

        [JsonProperty]
        public int CoachesPoll_LastWeeksRank { get; set; }

        [JsonProperty]
        public int ToughestPlacesRank { get; set; }

        [JsonProperty]
        public int TeamPrestige { get; set; }

        [JsonProperty]
        public int TEAM_PREVSEASLOSSES { get; set; }

        [JsonProperty]
        public int TEAM_PREVSEASWINS { get; set; }

        [JsonIgnore]
        public int TeamId => TeamIdToOldIdMap[this.TeamIndex];

        /// <summary>
        /// ids in CFB27 mapped to the classic ids
        /// </summary>
        /// <summary>
        /// ids in CFB27 mapped to the classic ids
        /// </summary>
        public static readonly Dictionary<int, int> TeamIdToOldIdMap = new Dictionary<int, int>
        {
            { 0, 1 },     // Air Force
            { 1, 2 },     // Akron
            { 2, 3 },     // Alabama
            { 3, 901 },   // App St.
            { 4, 4 },     // Arizona
            { 5, 5 },     // Arizona State
            { 6, 6 },     // Arkansas
            { 7, 7 },     // Arkansas State
            { 8, 8 },     // Army
            { 9, 9 },     // Auburn
            { 10, 10 },   // Ball State
            { 11, 11 },   // Baylor
            { 12, 12 },   // Boise State
            { 13, 13 },   // Boston College
            { 14, 14 },   // Bowling Green
            { 15, 15 },   // Buffalo
            { 16, 16 },   // BYU
            { 17, 17 },   // California
            { 18, 19 },   // C. Michigan
            { 19, 904 },  // Charlotte
            { 20, 20 },   // Cincinnati
            { 21, 21 },   // Clemson
            { 22, 903 },  // C. Carolina
            { 23, 22 },   // Colorado
            { 24, 23 },   // Colorado State
            { 25, 100 },  // UConn
            { 26, 906 },  // Delaware
            { 27, 24 },   // Duke
            { 28, 26 },   // E. Michigan
            { 29, 25 },   // East Carolina
            { 30, 160 },    // FCS East
            { 31, 163 },    // FCS Midwest
            { 32, 162 },    // FCS Northwest
            { 33, 164 },    // FCS Southeast
            { 34, 161 },    // FCS West
            { 35, 230 },  // FIU
            { 36, 27 },   // Florida
            { 37, 229 },  // FLA Atlantic
            { 38, 28 },   // Florida State
            { 39, 29 },   // Fresno State
            { 40, 30 },   // Georgia
            { 41, 902 },  // Ga Southern
            { 42, 233 },  // Georgia State
            { 43, 31 },   // Georgia Tech
            { 44, 32 },   // Hawai'i
            { 45, 33 },   // Houston
            { 46, 35 },   // Illinois
            { 47, 36 },   // Indiana
            { 48, 37 },   // Iowa
            { 49, 38 },   // Iowa State
            { 50, 907 },  // Jax State
            { 51, 905 },  // James Madison
            { 52, 39 },   // Kansas
            { 53, 40 },   // Kansas State
            { 54, 908 },  // Kennesaw St.
            { 55, 41 },   // Kent State
            { 56, 42 },   // Kentucky
            { 57, 909 },  // Liberty
            { 58, 86 },   // Louisiana
            { 59, 43 },   // Louisiana Tech
            { 60, 44 },   // Louisville
            { 61, 45 },   // LSU
            { 62, 46 },   // Marshall
            { 63, 47 },   // Maryland
            { 64, 48 },   // Memphis
            { 65, 49 },   // Miami
            { 66, 50 },   // Miami (OH)
            { 67, 51 },   // Michigan
            { 68, 52 },   // Michigan State
            { 69, 53 },   // Middle Tenn
            { 70, 54 },   // Minnesota
            { 71, 55 },   // Mississippi St
            { 72, 56 },   // Missouri
            { 73, 910 },  // Missouri State
            { 74, 57 },   // Navy
            { 75, 63 },   // NC State
            { 76, 58 },   // Nebraska
            { 77, 59 },   // Nevada
            { 78, 60 },   // New Mexico
            { 79, 61 },   // New Mexico St.
            { 80, 62 },   // North Carolina
            { 81, 911 },  // NDSU
            { 82, 64 },   // North Texas
            { 83, 66 },   // NIU
            { 84, 67 },   // Northwestern
            { 85, 68 },   // Notre Dame
            { 86, 69 },   // Ohio
            { 87, 70 },   // Ohio State
            { 88, 71 },   // Oklahoma
            { 89, 72 },   // Oklahoma State
            { 90, 234 },  // Old Dominion
            { 91, 73 },   // Ole Miss
            { 92, 74 },   // Oregon
            { 93, 75 },   // Oregon State
            { 94, 76 },   // Penn State
            { 95, 77 },   // Pittsburgh
            { 96, 78 },   // Purdue
            { 97, 79 },   // Rice
            { 98, 80 },   // Rutgers
            { 99, 912 },  // Sac State
            { 100, 913 }, // Sam Houston
            { 101, 81 },  // San Diego St.
            { 102, 82 },  // San Jose State
            { 103, 83 },  // SMU
            { 104, 235 }, // South Alabama
            { 105, 84 },  // South Carolina
            { 106, 85 },  // Southern Miss
            { 107, 87 },  // Stanford
            { 108, 88 },  // Syracuse
            { 109, 89 },  // TCU
            { 110, 90 },  // Temple
            { 111, 91 },  // Tennessee
            { 112, 92 },  // Texas
            { 113, 93 },  // Texas A&M
            { 114, 218 }, // Texas State
            { 115, 94 },   // Texas Tech
            { 116, 95 },   // Toledo
            { 117, 143 },   // Troy
            { 118, 96 },   // Tulane
            { 119, 97 },   // Tulsa
            { 120, 98 },   // UAB
            { 121, 18 },   // UCF
            { 122, 99 },   // UCLA
            { 123, 65 },   // UL Monroe
            { 124, 181 },   // UMass
            { 125, 101 },   // UNLV
            { 126, 102 },   // USC
            { 127, 144 },   // USF
            { 128, 103 },   // Utah
            { 129, 104 },   // Utah State
            { 130, 105 },   // UTEP
            { 131, 232 },   // UTSA
            { 132, 106 },   // Vanderbilt
            { 133, 107 },   // Virginia
            { 134, 108 },   // Virginia Tech
            { 135, 109 },   // Wake Forest
            { 136, 110 },   // Washington
            { 137, 111 },   // Washington St.
            { 138, 112 },   // West Virginia
            { 139, 211 },   // W. Kentucky
            { 140, 113 },   // W. Michigan
            { 141, 114 },   // Wisconsin
            { 142, 115 },    // Wyoming
        };
    }
}
