using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class SECSchedule
    {
        private static bool initRun = false;
        #region TeamIds
        public const int Alabama = 3;
        public const int Ark = 6;
        public const int Aub = 9;
        public const int UF = 27;
        public const int UGA = 30;
        public const int UK = 42;
        public const int LSU = 45;
        public const int MissSt = 55;
        public const int Mizzou = 56;
        public const int OleMiss = 73;
        public const int SCAR = 84;
        public const int Tenn = 91;
        public const int TAMU = 93;
        public const int Vandy = 106;
        #endregion

        private static Func<Dictionary<int, int[]>>[] CorrectCreators = new Func<Dictionary<int, int[]>>[]
        {
            CreateA, CreateA,
            CreateB, CreateB,
        };

        public static Dictionary<int, HashSet<int>> SECConfSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessPac12Schedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, SECConfSchedule, RecruitingFixup.Pac16Id, RecruitingFixup.Pac12);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var creatorsToUse = CorrectCreators;
            var idx = (Form1.DynastyYear - 2548) % creatorsToUse.Length;

            var result = creatorsToUse[idx]();
            result = result.Verify(14, RecruitingFixup.SECId, "SEC");
            SECConfSchedule = result.BuildHashSet();
            return result;
        }

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Alabama.Create(UF, LSU, Tenn, TAMU),
                Ark.Create(MissSt, Mizzou, Tenn, TAMU),
                Aub.Create(Alabama, Ark,OleMiss, SCAR),
                UF.Create(Aub, UGA, OleMiss, Vandy),
                UGA.Create(Ark, Aub,MissSt, SCAR),
                UK.Create(Alabama, UF, Mizzou, Tenn),
                LSU.Create(Ark, UGA, SCAR, Vandy),
                MissSt.Create(Alabama, UK, LSU, Vandy),
                Mizzou.Create(Alabama, UGA, SCAR, TAMU),
                OleMiss.Create(LSU, MissSt, Mizzou, Tenn),
                SCAR.Create(Ark, UF, UK, OleMiss),
                Tenn.Create(Aub,UGA, TAMU, Vandy),
                TAMU.Create(UF, UK, LSU, MissSt),
                Vandy.Create(Aub, UK, Mizzou, OleMiss),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Alabama.Create(Ark, UGA, SCAR, Tenn),
                Ark.Create(UF, Mizzou, TAMU, Vandy),
                Aub.Create(Alabama, MissSt, Mizzou, TAMU),
                UF.Create(Aub, UGA, Mizzou, Tenn),
                UGA.Create(Aub, UK, OleMiss, SCAR),
                UK.Create(Ark, Aub, OleMiss, Tenn),
                LSU.Create(Ark, Aub, UF, UK),
                MissSt.Create(Alabama, UF, UK, Mizzou),
                Mizzou.Create(LSU, SCAR, Tenn, TAMU),
                OleMiss.Create(Alabama, Ark, LSU, MissSt),
                SCAR.Create(UF, MissSt, TAMU, Vandy),
                Tenn.Create(LSU, MissSt, SCAR, Vandy),
                TAMU.Create(UGA, LSU, OleMiss, Vandy),
                Vandy.Create(Alabama, UGA, UK, OleMiss),
            }.Create();
        }
    }
}