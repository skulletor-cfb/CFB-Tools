using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public class SunBeltSchedule
    {
        private static bool initRun = false;

        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateD,
            CreateE, CreateF,
            CreateG, CreateA,
            CreateB, CreateC,
            CreateD, CreateE,
            CreateF, CreateG,
            };


        public static Dictionary<int, HashSet<int>> SunbeltConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        static HashSet<int> West = new HashSet<int>() 
        { NT, UTSA, TexSt, ArkSt, ULM, ULL, Troy, USM};

        static HashSet<int> East = new HashSet<int>()
        {
            USA, JMU, Coastal, ODU, GASO, GSU, AppSt, UMarsh
        };

        public static bool CrossDivision(int a, int b)
        {
            var intraDivision = (West.Contains(a) && West.Contains(b)) || (East.Contains(a) && East.Contains(b));
            return !intraDivision;
        }

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessSunbeltSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, SunbeltConferenceSchedule, RecruitingFixup.SBCId, RecruitingFixup.SBC);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2488) % Creators.Length;
            var result = Creators[idx]();

            switch(Form1.DynastyYear)
            {
                case 2491:
                    result = CreateDPrime();
                    break;

                case 2497:
                    result = CreateCPrime();
                    break;

                case 2498:
                    result = CreateDPrime();
                    break;

                default:
                    break;
            }

            result = result.Verify(14, RecruitingFixup.SBCId, "SunBelt");
            SunbeltConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int Coastal = 61;
        const int ODU = 234;
        const int UMarsh = 46;
        const int AppSt = 34;
        const int GSU = 233;
        const int GASO = 181;
        const int Troy = 143;

        const int JMU = 230;
        const int USA = 235;
        const int USM = 85;
        const int TexSt = 218;
        const int ArkSt = 7;
        const int ULM = 65;
        const int ULL = 86;
        const int Army = 8;
        const int Navy = 57;
        const int UTSA = 232;
        const int NT = 64;
        const int UAB = 98;

#if true // Sun Belt is real life with JMU in east
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, UMarsh),
                USA.Create(USM, TexSt, ULL, JMU),
                ULM.Create(USA, USM, ULL, AppSt),
                USM.Create(TexSt, Troy, ULL, GSU),
                TexSt.Create(ArkSt, ULM, Troy, ODU),
                Troy.Create(ArkSt, USA, ULM, Coastal),
                ULL.Create(ArkSt, TexSt, Troy, GASO),

                UMarsh.Create(ULL, GSU, ODU, Coastal),
                JMU.Create(ArkSt, UMarsh, GSU, Coastal),
                AppSt.Create(USA, UMarsh, JMU, Coastal),
                GSU.Create(ULM, AppSt, ODU, GASO),
                ODU.Create(USM, JMU, AppSt, GASO),
                Coastal.Create(TexSt, GSU, ODU, GASO),
                GASO.Create(Troy, UMarsh, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, AppSt),
                USA.Create(USM, TexSt, ULL, GSU),
                ULM.Create(USA, USM, ULL, ODU),
                USM.Create(TexSt, Troy, ULL, Coastal),
                TexSt.Create(ArkSt, ULM, Troy, GASO),
                Troy.Create(ArkSt, USA, ULM, UMarsh),
                ULL.Create(ArkSt, TexSt, Troy, JMU),

                UMarsh.Create(TexSt, GSU, ODU, Coastal),
                JMU.Create(Troy, UMarsh, GSU, Coastal),
                AppSt.Create(ULL, UMarsh, JMU, Coastal),
                GSU.Create(ArkSt, AppSt, ODU, GASO),
                ODU.Create(USA, JMU, AppSt, GASO),
                Coastal.Create(ULM, GSU, ODU, GASO),
                GASO.Create(USM, UMarsh, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, ODU),
                USA.Create(USM, TexSt, ULL, Coastal),
                ULM.Create(USA, USM, ULL, GASO),
                USM.Create(TexSt, Troy, ULL, UMarsh),
                TexSt.Create(ArkSt, ULM, Troy, JMU),
                Troy.Create(ArkSt, USA, ULM, AppSt),
                ULL.Create(ArkSt, TexSt, Troy, GSU),

                UMarsh.Create(ULM, GSU, ODU, Coastal),
                JMU.Create(USM, UMarsh, GSU, Coastal),
                AppSt.Create(TexSt, UMarsh, JMU, Coastal),
                GSU.Create(Troy, AppSt, ODU, GASO),
                ODU.Create(ULL, JMU, AppSt, GASO),
                Coastal.Create(ArkSt, GSU, ODU, GASO),
                GASO.Create(USA, UMarsh, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, GASO),
                USA.Create(USM, TexSt, ULL, UMarsh),
                ULM.Create(USA, USM, ULL, JMU),
                USM.Create(TexSt, Troy, ULL, AppSt),
                TexSt.Create(ArkSt, ULM, Troy, GSU),
                Troy.Create(ArkSt, USA, ULM, ODU),
                ULL.Create(ArkSt, TexSt, Troy, Coastal),

                UMarsh.Create(ArkSt, GSU, ODU, Coastal),
                JMU.Create(USA, UMarsh, GSU, Coastal),
                AppSt.Create(ULM, UMarsh, JMU, Coastal),
                GSU.Create(USM, AppSt, ODU, GASO),
                ODU.Create(TexSt, JMU, AppSt, GASO),
                Coastal.Create(Troy, GSU, ODU, GASO),
                GASO.Create(ULL, UMarsh, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateCPrime()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, ODU),
                USA.Create(USM, TexSt, ULL, Coastal),
                ULM.Create(USA, USM, ULL, GASO),
                USM.Create(TexSt, Troy, ULL, UMarsh),
                TexSt.Create(ArkSt, ULM, Troy, JMU),
                Troy.Create(ArkSt, USA, ULM, AppSt),
                ULL.Create(ArkSt, TexSt, Troy, GSU),

                UMarsh.Create(ArkSt, GSU, ODU, Coastal),
                JMU.Create(USA, UMarsh, GSU, Coastal),
                AppSt.Create(ULM, UMarsh, JMU, Coastal),
                GSU.Create(USM, AppSt, ODU, GASO),
                ODU.Create(TexSt, JMU, AppSt, GASO),
                Coastal.Create(Troy, GSU, ODU, GASO),
                GASO.Create(ULL, UMarsh, JMU, AppSt),
            }.Create();
        }


        public static Dictionary<int, int[]> CreateDPrime()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, GASO),
                USA.Create(USM, TexSt, ULL, UMarsh),
                ULM.Create(USA, USM, ULL, JMU),
                USM.Create(TexSt, Troy, ULL, AppSt),
                TexSt.Create(ArkSt, ULM, Troy, GSU),
                Troy.Create(ArkSt, USA, ULM, ODU),
                ULL.Create(ArkSt, TexSt, Troy, Coastal),

                UMarsh.Create(ULM, GSU, ODU, Coastal),
                JMU.Create(USM, UMarsh, GSU, Coastal),
                AppSt.Create(TexSt, UMarsh, JMU, Coastal),
                GSU.Create(Troy, AppSt, ODU, GASO),
                ODU.Create(ULL, JMU, AppSt, GASO),
                Coastal.Create(ArkSt, GSU, ODU, GASO),
                GASO.Create(USA, UMarsh, JMU, AppSt),
            }.Create();
        }


        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, JMU),
                USA.Create(USM, TexSt, ULL, AppSt),
                ULM.Create(USA, USM, ULL, GSU),
                USM.Create(TexSt, Troy, ULL, ODU),
                TexSt.Create(ArkSt, ULM, Troy, Coastal),
                Troy.Create(ArkSt, USA, ULM, GASO),
                ULL.Create(ArkSt, TexSt, Troy, UMarsh),

                UMarsh.Create(Troy, GSU, ODU, Coastal),
                JMU.Create(ULL, UMarsh, GSU, Coastal),
                AppSt.Create(ArkSt, UMarsh, JMU, Coastal),
                GSU.Create(USA, AppSt, ODU, GASO),
                ODU.Create(ULM, JMU, AppSt, GASO),
                Coastal.Create(USM, GSU, ODU, GASO),
                GASO.Create(TexSt, UMarsh, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, GSU),
                USA.Create(USM, TexSt, ULL, ODU),
                ULM.Create(USA, USM, ULL, Coastal),
                USM.Create(TexSt, Troy, ULL, GASO),
                TexSt.Create(ArkSt, ULM, Troy, UMarsh),
                Troy.Create(ArkSt, USA, ULM, JMU),
                ULL.Create(ArkSt, TexSt, Troy, AppSt),

                UMarsh.Create(USM, GSU, ODU, Coastal),
                JMU.Create(TexSt, UMarsh, GSU, Coastal),
                AppSt.Create(Troy, UMarsh, JMU, Coastal),
                GSU.Create(ULL, AppSt, ODU, GASO),
                ODU.Create(ArkSt, JMU, AppSt, GASO),
                Coastal.Create(USA, GSU, ODU, GASO),
                GASO.Create(ULM, UMarsh, JMU, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(USA, ULM, USM, Coastal),
                USA.Create(USM, TexSt, ULL, GASO),
                ULM.Create(USA, USM, ULL, UMarsh),
                USM.Create(TexSt, Troy, ULL, JMU),
                TexSt.Create(ArkSt, ULM, Troy, AppSt),
                Troy.Create(ArkSt, USA, ULM, GSU),
                ULL.Create(ArkSt, TexSt, Troy, ODU),

                UMarsh.Create(USA, GSU, ODU, Coastal),
                JMU.Create(ULM, UMarsh, GSU, Coastal),
                AppSt.Create(USM, UMarsh, JMU, Coastal),
                GSU.Create(TexSt, AppSt, ODU, GASO),
                ODU.Create(Troy, JMU, AppSt, GASO),
                Coastal.Create(ULL, GSU, ODU, GASO),
                GASO.Create(ArkSt, UMarsh, JMU, AppSt),
            }.Create();
        }

#elif false // sun belt is 14 teams, usa-troy cross over with LT/USM in the west
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, USM, TexSt, UMarsh),
                ULM.Create(USA, ULL, TexSt, ODU),
                ArkSt.Create(USA, ULM, ULL, GASO),
                ULL.Create(USM, LT, TexSt, AppSt),
                USM.Create(ULM, ArkSt, LT, GSU),
                LT.Create(USA, ULM, ArkSt, Coastal),
                TexSt.Create(ArkSt, USM, LT, Troy),

                Troy.Create(USA, UMarsh, GSU, Coastal),
                UMarsh.Create(ULM, ODU, GASO, Coastal),
                ODU.Create(ArkSt, Troy, AppSt, GSU),
                GASO.Create(ULL, Troy, ODU, AppSt),
                AppSt.Create(USM, Troy, UMarsh, Coastal),
                GSU.Create(LT, UMarsh, GASO, AppSt),
                Coastal.Create(TexSt, ODU, GASO, GSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, USM, TexSt, ODU),
                ULM.Create(USA, ULL, TexSt, GASO),
                ArkSt.Create(USA, ULM, ULL, AppSt),
                ULL.Create(USM, LT, TexSt, GSU),
                USM.Create(ULM, ArkSt, LT, Coastal),
                LT.Create(USA, ULM, ArkSt, Troy),
                TexSt.Create(ArkSt, USM, LT, UMarsh),

                Troy.Create(USA, UMarsh, GSU, Coastal),
                UMarsh.Create(ArkSt, ODU, GASO, Coastal),
                ODU.Create(ULL, Troy, AppSt, GSU),
                GASO.Create(USM, Troy, ODU, AppSt),
                AppSt.Create(LT, Troy, UMarsh, Coastal),
                GSU.Create(TexSt, UMarsh, GASO, AppSt),
                Coastal.Create(ULM, ODU, GASO, GSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, USM, TexSt, GASO),
                ULM.Create(USA, ULL, TexSt, AppSt),
                ArkSt.Create(USA, ULM, ULL, GSU),
                ULL.Create(USM, LT, TexSt, Coastal),
                USM.Create(ULM, ArkSt, LT, Troy),
                LT.Create(USA, ULM, ArkSt, UMarsh),
                TexSt.Create(ArkSt, USM, LT, ODU),

                Troy.Create(USA, UMarsh, GSU, Coastal),
                UMarsh.Create(ULL, ODU, GASO, Coastal),
                ODU.Create(USM, Troy, AppSt, GSU),
                GASO.Create(LT, Troy, ODU, AppSt),
                AppSt.Create(TexSt, Troy, UMarsh, Coastal),
                GSU.Create(ULM, UMarsh, GASO, AppSt),
                Coastal.Create(ArkSt, ODU, GASO, GSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, USM, TexSt, AppSt),
                ULM.Create(USA, ULL, TexSt, GSU),
                ArkSt.Create(USA, ULM, ULL, Coastal),
                ULL.Create(USM, LT, TexSt, Troy),
                USM.Create(ULM, ArkSt, LT, UMarsh),
                LT.Create(USA, ULM, ArkSt, ODU),
                TexSt.Create(ArkSt, USM, LT, GASO),

                Troy.Create(USA, UMarsh, GSU, Coastal),
                UMarsh.Create(TexSt, ODU, GASO, Coastal),
                ODU.Create(ULM, Troy, AppSt, GSU),
                GASO.Create(ArkSt, Troy, ODU, AppSt),
                AppSt.Create(ULL, Troy, UMarsh, Coastal),
                GSU.Create(USM, UMarsh, GASO, AppSt),
                Coastal.Create(LT, ODU, GASO, GSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, USM, TexSt, GSU),
                ULM.Create(USA, ULL, TexSt, Coastal),
                ArkSt.Create(USA, ULM, ULL, Troy),
                ULL.Create(USM, LT, TexSt, UMarsh),
                USM.Create(ULM, ArkSt, LT, ODU),
                LT.Create(USA, ULM, ArkSt, GASO),
                TexSt.Create(ArkSt, USM, LT, AppSt),

                Troy.Create(USA, UMarsh, GSU, Coastal),
                UMarsh.Create(LT, ODU, GASO, Coastal),
                ODU.Create(TexSt, Troy, AppSt, GSU),
                GASO.Create(ULM, Troy, ODU, AppSt),
                AppSt.Create(ArkSt, Troy, UMarsh, Coastal),
                GSU.Create(ULL, UMarsh, GASO, AppSt),
                Coastal.Create(USM, ODU, GASO, GSU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, USM, TexSt, Coastal),
                ULM.Create(USA, ULL, TexSt, Troy),
                ArkSt.Create(USA, ULM, ULL, UMarsh),
                ULL.Create(USM, LT, TexSt, ODU),
                USM.Create(ULM, ArkSt, LT, GASO),
                LT.Create(USA, ULM, ArkSt, AppSt),
                TexSt.Create(ArkSt, USM, LT, GSU),

                Troy.Create(USA, UMarsh, GSU, Coastal),
                UMarsh.Create(USM, ODU, GASO, Coastal),
                ODU.Create(LT, Troy, AppSt, GSU),
                GASO.Create(TexSt, Troy, ODU, AppSt),
                AppSt.Create(ULM, Troy, UMarsh, Coastal),
                GSU.Create(ArkSt, UMarsh, GASO, AppSt),
                Coastal.Create(ULL, ODU, GASO, GSU),
            }.Create();
        }

#elif false // sun belt is real life-ish
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ULL.Create(Troy, ArkSt, GSU, AppSt),
                TexSt.Create(ULL, Troy, ArkSt, AppSt ),
                Troy.Create(ULM, USA, UMarsh, GSU),
                ArkSt.Create(Troy, ULM, ODU, Coastal),
                ULM.Create(ULL, TexSt, USA, GASO),
                USA.Create(ULL, TexSt, ArkSt, ODU),

                UMarsh.Create(ULL, TexSt, GSU, ODU),
                GSU.Create(TexSt, ODU, GASO, Coastal),
                AppSt.Create(Troy, UMarsh, GSU, Coastal),
                ODU.Create(ULM, AppSt, GASO, Coastal),
                GASO.Create(ArkSt, USA, UMarsh, AppSt),
                Coastal.Create(ULM, USA, UMarsh, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ULL.Create(TexSt, Troy, GASO, Coastal),
                TexSt.Create(ArkSt, ULM, USA, ODU ),
                Troy.Create(TexSt, ULM , USA, ODU),
                ArkSt.Create(ULL, Troy, ULM, GSU),
                ULM.Create(ULL, USA, UMarsh, AppSt),
                USA.Create(ULL, ArkSt, GSU, AppSt),

                UMarsh.Create(ArkSt, USA, GSU, ODU),
                GSU.Create(ULM, ODU, GASO, Coastal),
                AppSt.Create(ArkSt, UMarsh, GSU, Coastal),
                ODU.Create(ULL, AppSt, GASO, Coastal),
                GASO.Create(TexSt, Troy, UMarsh, AppSt),
                Coastal.Create(TexSt, Troy, UMarsh, GASO),
            }.Create();
        }
#elif false // sun belt is a southern eastern conference
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(ULL, ArkSt, UTSA, USM),
                ULL.Create(TexSt, ArkSt, Troy, LT),
                TexSt.Create(NT, ArkSt, ULM, LT),
                ArkSt.Create(UTSA, ULM, USA, MTSU),
                UTSA.Create(ULL, TexSt, ULM, UAB),
                ULM.Create(NT, ULL, USA, MTSU),

                USA.Create(UTSA, MTSU, UAB, USM),
                MTSU.Create(UTSA, USM, Troy, LT),
                UAB.Create(ArkSt, ULM, MTSU, Troy),
                USM.Create(ULL, TexSt, UAB, Troy),
                Troy.Create(NT, TexSt, USA, LT),
                LT.Create(NT, USA, UAB, USM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(ULL, ArkSt, UTSA, USA),
                ULL.Create(TexSt, ArkSt, MTSU, UAB),
                TexSt.Create(NT, ArkSt, ULM, MTSU),
                ArkSt.Create(UTSA, ULM, USM, Troy),
                UTSA.Create(ULL, TexSt, ULM, LT),
                ULM.Create(NT, ULL, USM, Troy),
                USA.Create(ULL, TexSt, MTSU, UAB),
                MTSU.Create(NT, USM, Troy, LT),
                UAB.Create(NT, TexSt, MTSU, LT),
                USM.Create(UTSA, USA, UAB, Troy),
                Troy.Create(UTSA, USA, UAB, LT),
                LT.Create(ArkSt, ULM, USA, USM),
            }.Create();
        }

#elif false // 16 team, 8 conference games with classic SBC minus UAB + USA, Troy, east coast teams
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, Troy),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, GSU),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, USA),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, AppSt),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, ODU, USA, Coastal),
                UMarsh.Create(Troy, GSU, ODU, LT),
                GSU.Create(ODU,  USA, GASO, Coastal),
                ODU.Create(USA, GASO, AppSt, ULL),
                USA.Create(UMarsh, GASO, AppSt, Coastal),
                GASO.Create(Troy, UMarsh, AppSt, ULM),
                AppSt.Create(Troy, UMarsh, GSU, Coastal),
                Coastal.Create(UMarsh, ODU, GASO, USM),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, GSU),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, USA),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, AppSt),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, Troy),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, ODU, USA, Coastal),
                UMarsh.Create(Troy, GSU, ODU, USM),
                GSU.Create(ODU,  USA, GASO, Coastal),
                ODU.Create(USA, GASO, AppSt, LT),
                USA.Create(UMarsh, GASO, AppSt, Coastal),
                GASO.Create(Troy, UMarsh, AppSt, ULL),
                AppSt.Create(Troy, UMarsh, GSU, Coastal),
                Coastal.Create(UMarsh, ODU, GASO, ULM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, USA),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, AppSt),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, Troy),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, GSU),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, ODU, USA, Coastal),
                UMarsh.Create(Troy, GSU, ODU, ULM),
                GSU.Create(ODU,  USA, GASO, Coastal),
                ODU.Create(USA, GASO, AppSt, USM),
                USA.Create(UMarsh, GASO, AppSt, Coastal),
                GASO.Create(Troy, UMarsh, AppSt, LT),
                AppSt.Create(Troy, UMarsh, GSU, Coastal),
                Coastal.Create(UMarsh, ODU, GASO, ULL),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, AppSt),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, Troy),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, GSU),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, USA),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, ODU, USA, Coastal),
                UMarsh.Create(Troy, GSU, ODU, ULL),
                GSU.Create(ODU,  USA, GASO, Coastal),
                ODU.Create(USA, GASO, AppSt, ULM),
                USA.Create(UMarsh, GASO, AppSt, Coastal),
                GASO.Create(Troy, UMarsh, AppSt, USM),
                AppSt.Create(Troy, UMarsh, GSU, Coastal),
                Coastal.Create(UMarsh, ODU, GASO, LT),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateW()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, Coastal),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, UMarsh),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, ODU),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, GASO),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, LT),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, ULL),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, ULM),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, USM),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, UMarsh),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, ODU),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, GASO),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, Coastal),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, USM),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, LT),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, ULL),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, ULM),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, ODU),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, GASO),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, Coastal),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, UMarsh),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, ULM),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, USM),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, LT),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, ULL),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(LT, NT, ArkSt, GASO),
                LT.Create(NT, ULM, ArkSt, USM),
                NT.Create(ULL, ArkSt, USM, Coastal),
                ULL.Create(UTSA, LT, TexSt, ArkSt),
                TexSt.Create(UTSA, LT, NT, UMarsh),
                ULM.Create(UTSA, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, ODU),
                USM.Create(UTSA, ULL, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, ULL),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, ULM),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, USM),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, LT),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }
#elif false //uab and usm make 16, 8 conference games
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, UMarsh),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, ODU),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, GASO),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, Coastal),

                Troy.Create(UMarsh, GSU, USA, UAB),
                UMarsh.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GASO, AppSt, NT),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GASO, AppSt, TexSt),
                GASO.Create(Troy, UMarsh, ODU, AppSt),
                AppSt.Create(Troy, UMarsh, Coastal, ArkSt),
                Coastal.Create(Troy, GSU, USA, GASO),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, ODU),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, GASO),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, Coastal),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, UMarsh),

                Troy.Create(UMarsh, GSU, USA, ArkSt),
                UMarsh.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GASO, AppSt, UAB),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GASO, AppSt, NT),
                GASO.Create(Troy, UMarsh, ODU, AppSt),
                AppSt.Create(Troy, UMarsh, Coastal, TexSt),
                Coastal.Create(Troy, GSU, USA, GASO),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, GASO),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, Coastal),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, UMarsh),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, ODU),

                Troy.Create(UMarsh, GSU, USA, TexSt),
                UMarsh.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GASO, AppSt, ArkSt),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GASO, AppSt, UAB),
                GASO.Create(Troy, UMarsh, ODU, AppSt),
                AppSt.Create(Troy, UMarsh, Coastal, NT),
                Coastal.Create(Troy, GSU, USA, GASO),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, ULL, TexSt, ArkSt),
                LT.Create(NT, ULM, USM, Coastal),
                NT.Create(UAB, ULL, ArkSt, USM),
                ULL.Create(LT, TexSt, ArkSt, UMarsh),
                TexSt.Create(LT, NT, ULM, USM),
                ULM.Create(UAB, NT, ULL, ODU),
                ArkSt.Create(LT, TexSt, ULM, USM),
                USM.Create(UAB, ULL, ULM, GASO),

                Troy.Create(UMarsh, GSU, USA, NT),
                UMarsh.Create(GSU, ODU, USA, Coastal),
                GSU.Create(USA, GASO, AppSt, TexSt),
                ODU.Create(Troy, GSU, AppSt, Coastal),
                USA.Create(ODU, GASO, AppSt, ArkSt),
                GASO.Create(Troy, UMarsh, ODU, AppSt),
                AppSt.Create(Troy, UMarsh, Coastal, UAB),
                Coastal.Create(Troy, GSU, USA, GASO),

            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, Coastal),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, UMarsh),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, ODU),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, GASO),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, LT),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, ULL),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, ULM),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, USM),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, UMarsh),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, ODU),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, GASO),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, Coastal),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, USM),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, LT),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, ULL),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, ULM),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, ODU),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, GASO),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, Coastal),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, UMarsh),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, ULL),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, ULM),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, USM),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, LT),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateH()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UAB.Create(LT, NT, ArkSt, GASO),
                LT.Create(TexSt, ULM, ArkSt, USM),
                NT.Create(LT, ULL, ArkSt, Coastal),
                ULL.Create(UAB, LT, ArkSt, USM),
                TexSt.Create(UAB, NT, ULL, UMarsh),
                ULM.Create(UAB, NT, ULL, TexSt),
                ArkSt.Create(TexSt, ULM, USM, ODU),
                USM.Create(UAB, NT, TexSt, ULM),

                Troy.Create(GSU, USA, AppSt, USM),
                UMarsh.Create(Troy, GSU, ODU, Coastal),
                GSU.Create(ODU, USA, GASO, LT),
                ODU.Create(Troy, GASO, AppSt, Coastal),
                USA.Create(UMarsh, ODU, AppSt, ULL),
                GASO.Create(Troy, UMarsh, USA, AppSt),
                AppSt.Create(UMarsh, GSU, Coastal, ULM),
                Coastal.Create(Troy, GSU, USA, GASO),
            }.Create();
        }
#elif false// ODU in the mix, no UTSA
        // 14 team sbc like real life
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, LT, TexSt, UMarsh),
                ULM.Create(USA, ULL, TexSt, ODU),
                ArkSt.Create(USA, ULM, TexSt, GASO),
                ULL.Create(ArkSt, NT, LT, AppSt),
                NT.Create(USA, ULM, ArkSt, GSU),
                LT.Create(ULM, ArkSt,NT, Coastal),
                TexSt.Create(ULL, NT, LT, Troy),

                Troy.Create(USA, UMarsh, AppSt, GSU),
                UMarsh.Create(ULM, ODU, GSU, Coastal),
                ODU.Create(ArkSt, Troy, GASO, AppSt),
                GASO.Create(ULL, Troy, UMarsh, AppSt),
                AppSt.Create(NT, UMarsh, GSU, Coastal),
                GSU.Create(LT, ODU, GASO, Coastal),
                Coastal.Create(TexSt, Troy, ODU, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, LT, TexSt, ODU),
                ULM.Create(USA, ULL, TexSt, GASO),
                ArkSt.Create(USA, ULM, TexSt, AppSt),
                ULL.Create(ArkSt, NT, LT, GSU),
                NT.Create(USA, ULM, ArkSt, Coastal),
                LT.Create(ULM, ArkSt,NT, Troy),
                TexSt.Create(ULL, NT, LT, UMarsh),

                Troy.Create(USA, UMarsh, AppSt, GSU),
                UMarsh.Create(ArkSt, ODU, GSU, Coastal),
                ODU.Create(ULL, Troy, GASO, AppSt),
                GASO.Create(NT, Troy, UMarsh, AppSt),
                AppSt.Create(LT, UMarsh, GSU, Coastal),
                GSU.Create(TexSt, ODU, GASO, Coastal),
                Coastal.Create(ULM, Troy, ODU, GASO),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USA.Create(ULL, LT, TexSt, GASO),
                ULM.Create(USA, ULL, TexSt, AppSt),
                ArkSt.Create(USA, ULM, TexSt, GSU),
                ULL.Create(ArkSt, NT, LT, Coastal),
                NT.Create(USA, ULM, ArkSt, Troy),
                LT.Create(ULM, ArkSt, NT, UMarsh),
                TexSt.Create(ULL, NT, LT, ODU),

                Troy.Create(USA, UMarsh, AppSt, GSU),
                UMarsh.Create(ULL, ODU, GSU, Coastal),
                ODU.Create(NT, Troy, GASO, AppSt),
                GASO.Create(LT, Troy, UMarsh, AppSt),
                AppSt.Create(TexSt, UMarsh, GSU, Coastal),
                GSU.Create(ULM, ODU, GASO, Coastal),
                Coastal.Create(ArkSt, Troy, ODU, GASO),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateD() => null;
        public static Dictionary<int, int[]> CreateE() => null;
        public static Dictionary<int, int[]> CreateF() => null;

#elif false
        // 14 team sbc like real life
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, UMarsh),
                NT.Create(ArkSt, TexSt, LT, Troy),
                ULM.Create(NT, UTSA, ULL, AppSt),
                UTSA.Create(ArkSt, NT, ULL, Coastal),
                ULL.Create(NT, TexSt, LT, GSU),
                TexSt.Create(ULM, UTSA, LT, USA),
                LT.Create(ArkSt, ULM, UTSA, GASO),
                UMarsh.Create(LT, Troy, Coastal, GSU),
                Troy.Create(ArkSt, AppSt, GSU, USA),
                AppSt.Create(NT, UMarsh, Coastal, USA),
                Coastal.Create(ULM, Troy, USA, GASO),
                GSU.Create(UTSA, AppSt, Coastal, GASO),
                USA.Create(ULL, UMarsh, GSU, GASO),
                GASO.Create(TexSt, UMarsh, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, AppSt),
                NT.Create(ArkSt, TexSt, LT, Coastal),
                ULM.Create(NT, UTSA, ULL, GSU),
                UTSA.Create(ArkSt, NT, ULL, USA),
                ULL.Create(NT, TexSt, LT, GASO),
                TexSt.Create(ULM, UTSA, LT, UMarsh),
                LT.Create(ArkSt, ULM, UTSA, Troy),
                UMarsh.Create(ULL, Troy, Coastal, GSU),
                Troy.Create(TexSt, AppSt, GSU, USA),
                AppSt.Create(LT, UMarsh, Coastal, USA),
                Coastal.Create(ArkSt, Troy, USA, GASO),
                GSU.Create(NT, AppSt, Coastal, GASO),
                USA.Create(ULM, UMarsh, GSU, GASO),
                GASO.Create(UTSA, UMarsh, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, GSU),
                NT.Create(ArkSt, TexSt, LT, USA),
                ULM.Create(NT, UTSA, ULL, GASO),
                UTSA.Create(ArkSt, NT, ULL, UMarsh),
                ULL.Create(NT, TexSt, LT, Troy),
                TexSt.Create(ULM, UTSA, LT, AppSt),
                LT.Create(ArkSt, ULM, UTSA, Coastal),
                UMarsh.Create(ULM, Troy, Coastal, GSU),
                Troy.Create(UTSA, AppSt, GSU, USA),
                AppSt.Create(ULL, UMarsh, Coastal, USA),
                Coastal.Create(TexSt, Troy, USA, GASO),
                GSU.Create(LT, AppSt, Coastal, GASO),
                USA.Create(ArkSt, UMarsh, GSU, GASO),
                GASO.Create(NT, UMarsh, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, GASO),
                NT.Create(ArkSt, TexSt, LT, UMarsh),
                ULM.Create(NT, UTSA, ULL, Troy),
                UTSA.Create(ArkSt, NT, ULL, AppSt),
                ULL.Create(NT, TexSt, LT, Coastal),
                TexSt.Create(ULM, UTSA, LT, GSU),
                LT.Create(ArkSt, ULM, UTSA, USA),
                UMarsh.Create(ArkSt, Troy, Coastal, GSU),
                Troy.Create(NT, AppSt, GSU, USA),
                AppSt.Create(ULM, UMarsh, Coastal, USA),
                Coastal.Create(UTSA, Troy, USA, GASO),
                GSU.Create(ULL, AppSt, Coastal, GASO),
                USA.Create(TexSt, UMarsh, GSU, GASO),
                GASO.Create(LT, UMarsh, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, Troy),
                NT.Create(ArkSt, TexSt, LT, AppSt),
                ULM.Create(NT, UTSA, ULL, Coastal),
                UTSA.Create(ArkSt, NT, ULL, GSU),
                ULL.Create(NT, TexSt, LT, USA),
                TexSt.Create(ULM, UTSA, LT, GASO),
                LT.Create(ArkSt, ULM, UTSA, UMarsh),
                UMarsh.Create(TexSt, Troy, Coastal, GSU),
                Troy.Create(LT, AppSt, GSU, USA),
                AppSt.Create(ArkSt, UMarsh, Coastal, USA),
                Coastal.Create(NT, Troy, USA, GASO),
                GSU.Create(ULM, AppSt, Coastal, GASO),
                USA.Create(UTSA, UMarsh, GSU, GASO),
                GASO.Create(ULL, UMarsh, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, Coastal),
                NT.Create(ArkSt, TexSt, LT, GSU),
                ULM.Create(NT, UTSA, ULL, USA),
                UTSA.Create(ArkSt, NT, ULL, GASO),
                ULL.Create(NT, TexSt, LT, UMarsh),
                TexSt.Create(ULM, UTSA, LT, Troy),
                LT.Create(ArkSt, ULM, UTSA, AppSt),
                UMarsh.Create(UTSA, Troy, Coastal, GSU),
                Troy.Create(ULL, AppSt, GSU, USA),
                AppSt.Create(TexSt, UMarsh, Coastal, USA),
                Coastal.Create(LT, Troy, USA, GASO),
                GSU.Create(ArkSt, AppSt, Coastal, GASO),
                USA.Create(NT, UMarsh, GSU, GASO),
                GASO.Create(ULM, UMarsh, Troy, AppSt),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateH()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ArkSt.Create(ULM, ULL, TexSt, USA),
                NT.Create(ArkSt, TexSt, LT, GASO),
                ULM.Create(NT, UTSA, ULL, UMarsh),
                UTSA.Create(ArkSt, NT, ULL, Troy),
                ULL.Create(NT, TexSt, LT, AppSt),
                TexSt.Create(ULM, UTSA, LT, Coastal),
                LT.Create(ArkSt, ULM, UTSA, GSU),
                UMarsh.Create(NT, Troy, Coastal, GSU),
                Troy.Create(ULM, AppSt, GSU, USA),
                AppSt.Create(UTSA, UMarsh, Coastal, USA),
                Coastal.Create(ULL, Troy, USA, GASO),
                GSU.Create(TexSt, AppSt, Coastal, GASO),
                USA.Create(LT, UMarsh, GSU, GASO),
                GASO.Create(ArkSt, UMarsh, Troy, AppSt),
            }.Create();
        }

#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{NT, ULM, LT, UAB} },
                {UTSA, new[]{TexSt, ArkSt, ULL, USM} },
                {NT, new[]{UTSA, ArkSt, ULL, USM} },
                {ArkSt, new[]{TexSt, ULM, LT, UAB} },
                {ULM, new[]{UTSA, NT, ULL, UAB} },
                {ULL, new[]{TexSt, ArkSt, LT, USM} },
                {LT, new[]{UTSA, NT, ULM, USM} },
                {USM, new[]{TexSt, ArkSt, ULM, UAB} },
                {UAB, new[]{UTSA, NT, ULL, LT} },
            };
        }

#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{USM, ULL, ArkSt, WKU} },
                {UTSA, new[]{TexSt, LT, ULM, NT} },
                {UAB, new[]{UTSA, ULL, ArkSt, MTSU} },
                {USM, new[]{UAB, ULM, NT , WKU} },
                {LT, new[]{TexSt, USM, ArkSt, MTSU} },
                {ULL, new[]{UTSA, LT, NT, WKU} },
                {ULM, new[]{TexSt, UAB, ULL, MTSU} },
                {ArkSt, new[]{UTSA, USM, ULM, WKU} },
                {NT, new[]{TexSt, UAB, LT, ArkSt} },
                {MTSU, new[]{UTSA, USM, ULL, NT} },
                {WKU, new[]{UAB, LT, ULM, MTSU} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{UAB, ULL, ArkSt, WKU} },
                {UTSA, new[]{TexSt, USM, ULM, NT} },
                {UAB, new[]{UTSA, LT, ArkSt, MTSU} },
                {USM, new[]{UAB, ULL, NT, WKU} },
                {LT, new[]{TexSt, USM, ULM, MTSU} },
                {ULL, new[]{ UTSA, LT, ArkSt, WKU} },
                {ULM, new[]{TexSt, UAB, ULL, NT} },
                {ArkSt, new[]{UTSA, USM, ULM, MTSU} },
                {NT, new[]{UAB, LT, ArkSt, WKU} },
                {MTSU, new[]{TexSt, USM, ULL, NT} },
                {WKU, new[]{UTSA, LT, ULM ,MTSU} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{UAB, ULL, NT, WKU} },
                {UTSA, new[]{TexSt, USM, ULM, MTSU} },
                {UAB, new[]{UTSA, LT ,ArkSt, WKU} },
                {USM, new[]{TexSt, UAB, ULL, NT} },
                {LT, new[]{UTSA, USM, ULM, MTSU} },
                {ULL, new[]{ UAB, LT , ArkSt, WKU} },
                {ULM, new[]{TexSt, USM, ULL, NT} },
                {ArkSt, new[]{UTSA, LT , ULM, MTSU} },
                {NT, new[]{UAB, ULL, ArkSt, WKU} },
                {MTSU, new[]{TexSt, USM, ULM , NT} },
                {WKU, new[]{UTSA, LT, ArkSt, MTSU} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {TexSt, new[]{UAB, LT, NT, WKU} },
                {UTSA, new[]{TexSt, USM, ULL, MTSU} },
                {UAB, new[]{UTSA, LT ,ULM, WKU} },
                {USM, new[]{TexSt, UAB, ULL, ArkSt} },
                {LT, new[]{UTSA, USM, ULM, NT} },
                {ULL, new[]{ UAB, LT, ArkSt, MTSU} },
                {ULM, new[]{USM, ULL, NT, WKU} },
                {ArkSt, new[]{TexSt, LT, ULM, MTSU} },
                {NT, new[]{UTSA, ULL, ArkSt, WKU} },
                {MTSU, new[]{TexSt, UAB, ULM, NT} },
                {WKU, new[]{UTSA, USM, ArkSt, MTSU} },
            };
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return Create(
                new[]
                {
                Create(NT, LT, UTSA, ArkSt, USM),
                Create(LT, ULM, UTSA, ArkSt, USM),
                Create(ULM, NT, TexSt, ULL, WKU),
                Create(UTSA, ULM, TexSt, ULL, UAB),
                Create(ArkSt, ULM, UTSA, ULL, WKU),
                Create(TexSt, NT, LT, ArkSt, FAU),
                Create(ULL, NT, LT, TexSt, MTSU),

                Create(MTSU, NT, FAU, Troy, USA),
                Create(USM, MTSU, FAU, Troy, UAB),
                Create(FAU, ULL, Troy, WKU, UAB),
                Create(Troy, ArkSt, TexSt, WKU, USA),
                Create(WKU, MTSU, USM, UAB, USA),
                Create(UAB, ULM, MTSU, Troy, USA),
                Create(USA, LT, UTSA, USM, FAU),
                });
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return null;
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return null;
        }

        private static KeyValuePair<int, int[]> Create(params int[] values)
        {
            var key = values[0];
            var value = values.Skip(1).ToArray();
            return new KeyValuePair<int, int[]>(key, value);
        }

        private static Dictionary<int, int[]> Create(KeyValuePair<int, int[]>[] kvps)
        {
            var dict = new Dictionary<int, int[]>();
            foreach( var kvp in kvps)
            {
                dict.Add(kvp.Key, kvp.Value);
            }
            return dict;
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, UTSA, ULM, UAB),
                Army.Create(TexSt, ArkSt, ULL, USM),
                TexSt.Create(UTSA, ULM, LT, UAB),
                UTSA.Create(NT, ULL, LT, USM),
                NT.Create(Navy, Army, ArkSt, ULL),
                ArkSt.Create(Navy, TexSt, ULM, LT),
                ULM.Create(Army, UTSA, ULL, USM),
                ULL.Create(Navy, TexSt, LT, UAB),
                LT.Create(Navy, Army, NT, USM),
                USM.Create(TexSt, NT, ArkSt, UAB),
                UAB.Create(UTSA, NT, ArkSt, ULM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, ULM, USM, UAB),
                Army.Create(TexSt, UTSA, ArkSt, ULL),
                TexSt.Create(Navy, UTSA, ULM, LT),
                UTSA.Create(NT, ULL, LT, USM),
                NT.Create(Navy, TexSt, ArkSt, UAB),
                ArkSt.Create(Navy, UTSA, ULM, ULL),
                ULM.Create(Army, NT, ULL, UAB),
                ULL.Create(Navy, TexSt, LT, USM),
                LT.Create(Army, NT, ULM, USM),
                USM.Create(TexSt, NT, ArkSt, UAB),
                UAB.Create(Army, UTSA, ArkSt, LT),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, ULM, USM, UAB),
                Army.Create(TexSt, UTSA, NT, ULL),
                TexSt.Create(Navy, UTSA, ArkSt, LT),
                UTSA.Create(Navy, NT, LT, USM),
                NT.Create(TexSt, ArkSt, ULL, UAB),
                ArkSt.Create(Navy, UTSA, ULM, ULL),
                ULM.Create(Army, UTSA, NT, ULL),
                ULL.Create(TexSt, LT, USM, UAB),
                LT.Create(Navy, ArkSt, ULM, USM),
                USM.Create(Army, NT, ULM, UAB),
                UAB.Create(Army, TexSt, ArkSt, LT),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Navy.Create(Army, NT, USM, UAB),
                Army.Create(TexSt, UTSA, NT, LT),
                TexSt.Create(Navy, UTSA, ArkSt, USM),
                UTSA.Create(Navy, NT, ULL, UAB),
                NT.Create(TexSt, ArkSt, ULL, LT),
                ArkSt.Create(Army, UTSA, ULM, ULL),
                ULM.Create(TexSt, UTSA, NT, ULL),
                ULL.Create(Navy, LT, USM, UAB),
                LT.Create(Navy, ArkSt, ULM, USM),
                USM.Create(Army, ArkSt, ULM, UAB),
                UAB.Create(Army, TexSt, ULM, LT),
            }.Create();
        }

#endif
    }
}