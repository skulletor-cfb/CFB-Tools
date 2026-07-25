using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    [JsonObject]
    public class CFB27Team
    {
        [JsonProperty]
        public string DisplayName { get; set; }

        [JsonProperty]
        public int TeamIndex { get; set; }

        /// <summary>
        /// ids in CFB27 mapped to the classic ids
        /// </summary>
        public static readonly Dictionary<int, int> TeamIdToOldIdMap = new Dictionary<int, int>
        {
            { 0, 1 },   // Air Force
            { 1, 2 },   // Akron
            { 2, 3 },   // Alabama
            { 125, 901 }, // App St.
            { 3, 4 },   // Arizona
            { 4, 5 },   // Arizona State
            { 5, 6 },   // Arkansas
            { 6, 7 },   // Arkansas State
            { 7, 8 },   // Army
            { 8, 9 },   // Auburn
            { 9, 10 },   // Ball State
            { 10, 11 },  // Baylor
            { 11, 12 },  // Boise State
            { 12, 13 },  // Boston College
            { 13, 14 },  // Bowling Green
            { 14, 15 },  // Buffalo
            { 15, 16 },  // BYU
            { 16, 17 },  // California
            { 18, 19 },  // C. Michigan
            { 126, 904 }, // Charlotte
            { 19, 20 },  // Cincinnati
            { 20, 21 },  // Clemson
            { 127, 903 }, // C. Carolina
            { 21, 22 },  // Colorado
            { 22, 23 },  // Colorado State
            { 98, 100 },  // UConn
            { 134, 906 }, // Delaware
            { 23, 24 },  // Duke
            { 25, 26 },  // E. Michigan
            { 24, 25 },  // East Carolina
            { 115, 230 }, // FIU
            { 26, 27 },  // Florida
            { 114, 229 }, // FLA Atlantic
            { 27, 28 },  // Florida State
            { 28, 29 },  // Fresno State
            { 29, 30 },  // Georgia
            { 128, 902 }, // Ga Southern
            { 116, 233 }, // Georgia State
            { 30, 31 },  // Georgia Tech
            { 31, 32 },  // Hawai'i
            { 32, 33 },  // Houston
            { 33, 35 },  // Illinois
            { 34, 36 },  // Indiana
            { 35, 37 },  // Iowa
            { 36, 38 },  // Iowa State
            { 129, 907 }, // Jax State
            { 130, 905 }, // James Madison
            { 37, 39 },  // Kansas
            { 38, 40 },  // Kansas State
            { 133, 908 }, // Kennesaw St.
            { 39, 41 },  // Kent State
            { 40,42 },  // Kentucky
            { 131, 909 }, // Liberty
            { 84, 86 },  // Louisiana
            { 41, 43 },  // Louisiana Tech
            { 42, 44 },  // Louisville
            { 43, 45 },  // LSU
            { 44, 46 },  // Marshall
            { 45, 47 },  // Maryland
            { 46, 48 },  // Memphis
            { 47, 49 },  // Miami
            { 48, 50 },  // Miami (OH)
            { 49, 51 },  // Michigan
            { 50, 52 },  // Michigan State
            { 51, 53 },  // Middle Tenn
            { 52, 54 },  // Minnesota
            { 53, 55 },  // Mississippi St
            { 54, 56 },  // Missouri
            { 135, 910 }, // Missouri State
            { 55, 57 },  // Navy
            { 61, 63 },  // NC State
            { 56, 58 },  // Nebraska
            { 57, 59 },  // Nevada
            { 58, 60 },  // New Mexico
            { 59, 61 },  // New Mexico St.
            { 60, 62 },  // North Carolina
            { 136, 911 }, // NDSU
            { 62, 64 },  // North Texas
            { 64, 66 },  // NIU
            { 65, 67 },  // Northwestern
            { 66, 68 },  // Notre Dame
            { 67, 69 },  // Ohio
            { 68, 70 },  // Ohio State
            { 69, 71 },  // Oklahoma
            { 70, 72 },  // Oklahoma State
            { 118, 234 }, // Old Dominion
            { 71, 73 },  // Ole Miss
            { 72, 74 },  // Oregon
            { 73, 75 },  // Oregon State
            { 74, 76 },  // Penn State
            { 75, 77 },  // Pittsburgh
            { 76, 78 },  // Purdue
            { 77, 79 },  // Rice
            { 78, 80 },  // Rutgers
            { 137, 912 }, // Sac State
            { 132, 913 }, // Sam Houston
            { 79, 81 },  // San Diego St.
            { 80, 82 },  // San Jose State
            { 81, 83 },  // SMU
            { 120, 235 }, // South Alabama
            { 82, 84 },  // South Carolina
            { 83, 85 },  // Southern Miss
            { 85, 87 },  // Stanford
            { 86, 88 },  // Syracuse
            { 87, 89 },  // TCU
            { 88, 90 },  // Temple
            { 89, 91 },  // Tennessee
            { 90, 92 },  // Texas
            { 91, 93 },  // Texas A&M
            { 124, 218 }, // Texas State
        };
    }
}
