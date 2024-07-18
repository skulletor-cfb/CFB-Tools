using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public static class AdditionalGameProvider
    {
        private const int NationalChampId = 42;
        public const int CureBowl = NationalChampId + 1;
        public const int MyrtleBeachBowl = NationalChampId + 2;
        public const int ArizonaBowl = NationalChampId + 3;
        public const int Sixty8VenturesBowl = NationalChampId + 4;
        public const int CFP8v9 = NationalChampId + 5;
        public const int CFP7v10 = NationalChampId + 6;
        public const int CFP6v11 = NationalChampId + 7;
        public const int CFP5v12 = NationalChampId + 8;
        public const int BowlId = 45;

        public static Dictionary<int, int> BowlIdToAddedGame = new Dictionary<int, int>()
        {
            { BowlId, CureBowl },
            { BowlId + 1, MyrtleBeachBowl },
            { BowlId + 2, ArizonaBowl },
            { BowlId + 3, Sixty8VenturesBowl }, //forward 48 to 0
            { BowlId + 4, CFP8v9 },
            { BowlId + 5, CFP7v10 },
            { BowlId + 6, CFP6v11 },
            { BowlId + 7, CFP5v12},
        };

        public static Dictionary<int, int> AddedGameToBowlId = BowlIdToAddedGame.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static Dictionary<int, string> BowlIdToName = new Dictionary<int, string>
        {
            { BowlId, "Cure Bowl" },
            { BowlId + 1, "Myrtle Beach Bowl" },
            { BowlId + 2, "Arizona Bowl" },
            { BowlId + 3, "68 Ventures Bowl" }, //forward 48 to 0
            { BowlId + 4, "CFP 8 v 9" },
            { BowlId + 5, "CFP 7 v 10" },
            { BowlId + 6, "CFP 6 v 11" },
            { BowlId + 7, "CFP 5 v 12" },
        };
    }
}