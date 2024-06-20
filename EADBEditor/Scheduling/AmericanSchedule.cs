using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class AmericanSchedule
    {
        private static bool initRun = false;

        /* this order when you have 14 team conference with no cross div*/
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] {  
            CreateZ, CreateY, 
            CreateX, CreateW,
            CreateR, CreateS,
            CreateT, CreateZ, 
            CreateY, CreateX, 
            CreateW, CreateR, 
            CreateS, CreateT,
        };

        /*
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { 
            CreateA, CreateB, 
            CreateC, CreateA,
            CreateB, CreateC,
        };*/


        public static Dictionary<int, HashSet<int>> AmericanConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;
        
        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessAmericanSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
             schedule.ProcessSchedule(ScenarioForSeason, AmericanConferenceSchedule, RecruitingFixup.AmericanId, RecruitingFixup.American);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            Dictionary<int, int[]> result = null;
            var currYear = Form1.DynastyYear;

            switch (currYear)
            {
                default:
                    var idx = (Form1.DynastyYear - 2498) % Creators.Length;
                    result = Creators[idx]();
                    break;
            }

            result = result.Verify(14, RecruitingFixup.AmericanId, "American");
            AmericanConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int USM = 85;
        const int NT = 64;
        const int UAB = 98;
        const int Memphis = 48;
        const int SMU = 83;
        const int CLT = 100;
        const int USF = 144;
        const int ECU = 25;
        const int Temple = 90;
        const int Tulsa = 97;
        const int Rice = 79;
        const int Houston = 33;
        const int Tulane = 96;
        const int FAU = 229;
        const int UTSA = 232;
        const int Army = 8;
        const int Navy = 57;
        const int CincyId = 20;
        const int MemphisId = 48;
        const int SMUId = 83;
        const int CharlotteId = 100;
        const int UCFId = 18;
        const int USFId = 144;
        const int ECUId = 25;
        const int TempleId = 90;
        const int TulsaId = 97;
        const int RiceId = 79;
        const int HoustonId = 33;
        const int TulaneId = 96;


#if false // uab, nt are in the AAC, no more Cincy/UCF
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, CLT),
                Houston.Create(Tulsa, UTSA, Tulane, Memphis),
                Tulsa.Create(UTSA, SMU, Rice, USF),
                UTSA.Create(NT, Rice, Tulane, Temple),
                SMU.Create(Houston, UTSA, Tulane, ECU),
                Rice.Create(NT, Houston, SMU, UAB),
                Tulane.Create(NT, Tulsa, Rice, FAU),

                CLT.Create(Tulane, Memphis, USF, Temple),
                Memphis.Create(NT, USF, Temple, ECU),
                USF.Create(Houston, Temple, ECU, UAB),
                Temple.Create(Tulsa, ECU, UAB, FAU),
                ECU.Create(UTSA, CLT, UAB, FAU),
                UAB.Create(SMU, CLT, Memphis, FAU),
                FAU.Create(Rice, CLT, Memphis,USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, USF),
                Houston.Create(Tulsa, UTSA, Tulane, Temple),
                Tulsa.Create(UTSA, SMU, Rice, ECU),
                UTSA.Create(NT, Rice, Tulane, UAB),
                SMU.Create(Houston, UTSA, Tulane, FAU),
                Rice.Create(NT, Houston, SMU, CLT),
                Tulane.Create(NT, Tulsa, Rice, Memphis),

                CLT.Create(SMU, Memphis, USF, Temple),
                Memphis.Create(Rice, USF, Temple, ECU),
                USF.Create(Tulane, Temple, ECU, UAB),
                Temple.Create(NT, ECU, UAB, FAU),
                ECU.Create(Houston, CLT, UAB, FAU),
                UAB.Create(Tulsa, CLT, Memphis, FAU),
                FAU.Create(UTSA, CLT, Memphis,USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, ECU),
                Houston.Create(Tulsa, UTSA, Tulane, UAB),
                Tulsa.Create(UTSA, SMU, Rice, FAU),
                UTSA.Create(NT, Rice, Tulane, CLT),
                SMU.Create(Houston, UTSA, Tulane, Memphis),
                Rice.Create(NT, Houston, SMU, USF),
                Tulane.Create(NT, Tulsa, Rice, Temple),

                CLT.Create(Tulsa, Memphis, USF, Temple),
                Memphis.Create(UTSA, USF, Temple, ECU),
                USF.Create(SMU, Temple, ECU, UAB),
                Temple.Create(Rice, ECU, UAB, FAU),
                ECU.Create(Tulane, CLT, UAB, FAU),
                UAB.Create(NT, CLT, Memphis, FAU),
                FAU.Create(Houston, CLT, Memphis,USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, CLT),
                Houston.Create(Tulsa, UTSA, Tulane, Memphis),
                Tulsa.Create(UTSA, SMU, Rice, USF),
                UTSA.Create(NT, Rice, Tulane, Temple),
                SMU.Create(Houston, UTSA, Tulane, ECU),
                Rice.Create(NT, Houston, SMU, UAB),
                Tulane.Create(NT, Tulsa, Rice, FAU),

                CLT.Create(Houston, Memphis, USF, Temple),
                Memphis.Create(Tulsa, USF, Temple, ECU),
                USF.Create(UTSA, Temple, ECU, UAB),
                Temple.Create(SMU, ECU, UAB, FAU),
                ECU.Create(Rice, CLT, UAB, FAU),
                UAB.Create(Tulane, CLT, Memphis, FAU),
                FAU.Create(NT, CLT, Memphis,USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, USF),
                Houston.Create(Tulsa, UTSA, Tulane, Temple),
                Tulsa.Create(UTSA, SMU, Rice, ECU),
                UTSA.Create(NT, Rice, Tulane, UAB),
                SMU.Create(Houston, UTSA, Tulane, FAU),
                Rice.Create(NT, Houston, SMU, CLT),
                Tulane.Create(NT, Tulsa, Rice, Memphis),

                CLT.Create(Tulane, Memphis, USF, Temple),
                Memphis.Create(NT, USF, Temple, ECU),
                USF.Create(Houston, Temple, ECU, UAB),
                Temple.Create(Tulsa, ECU, UAB, FAU),
                ECU.Create(UTSA, CLT, UAB, FAU),
                UAB.Create(SMU, CLT, Memphis, FAU),
                FAU.Create(Rice, CLT, Memphis,USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, ECU),
                Houston.Create(Tulsa, UTSA, Tulane, UAB),
                Tulsa.Create(UTSA, SMU, Rice, FAU),
                UTSA.Create(NT, Rice, Tulane, CLT),
                SMU.Create(Houston, UTSA, Tulane, Memphis),
                Rice.Create(NT, Houston, SMU, USF),
                Tulane.Create(NT, Tulsa, Rice, Temple),

                CLT.Create(SMU, Memphis, USF, Temple),
                Memphis.Create(Rice, USF, Temple, ECU),
                USF.Create(Tulane, Temple, ECU, UAB),
                Temple.Create(NT, ECU, UAB, FAU),
                ECU.Create(Houston, CLT, UAB, FAU),
                UAB.Create(Tulsa, CLT, Memphis, FAU),
                FAU.Create(UTSA, CLT, Memphis,USF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateG()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                NT.Create(Houston, Tulsa, SMU, UAB),
                Houston.Create(Tulsa, UTSA, Tulane, FAU),
                Tulsa.Create(UTSA, SMU, Rice, CLT),
                UTSA.Create(NT, Rice, Tulane, Memphis),
                SMU.Create(Houston, UTSA, Tulane, USF),
                Rice.Create(NT, Houston, SMU, Temple),
                Tulane.Create(NT, Tulsa, Rice, ECU),

                CLT.Create(Houston, Memphis, USF, Temple),
                Memphis.Create(Tulsa, USF, Temple, ECU),
                USF.Create(UTSA, Temple, ECU, UAB),
                Temple.Create(SMU, ECU, UAB, FAU),
                ECU.Create(Rice, CLT, UAB, FAU),
                UAB.Create(Tulane, CLT, Memphis, FAU),
                FAU.Create(NT, CLT, Memphis,USF),
            }.Create();
        }
#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Tulsa.Create(SMU, Rice, Tulane, CLT),
                SMU.Create(Memphis, UTSA, Houston, Temple),
                Memphis.Create(Tulsa, Rice, UTSA, USF),
                Rice.Create(SMU, Tulane, Houston, Cincy),
                UTSA.Create(Tulsa, Rice, Tulane, ECU),
                Tulane.Create(SMU, Memphis, Houston ,FAU),
                Houston.Create(Tulsa, Memphis, UTSA, UCF),

                UCF.Create(Tulsa, Temple, ECU, FAU),
                CLT.Create(SMU, UCF, Cincy, FAU),
                Temple.Create(Memphis, CLT, USF, FAU),
                USF.Create(Rice, UCF, CLT, Cincy),
                Cincy.Create(UTSA, UCF, Temple, ECU),
                ECU.Create(Tulane, CLT, Temple, USF),
                FAU.Create(Houston, USF, Cincy, ECU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Tulsa.Create(Temple,SMU, Rice, Tulane),
                SMU.Create(USF,Memphis, UTSA, Houston),
                Memphis.Create(Cincy,Tulsa, Rice, UTSA),
                Rice.Create(ECU,SMU, Tulane, Houston),
                UTSA.Create(FAU,Tulsa, Rice, Tulane),
                Tulane.Create(UCF,SMU, Memphis, Houston),
                Houston.Create(CLT,Tulsa, Memphis, UTSA),

                UCF.Create(Tulsa,Temple, ECU, FAU),
                CLT.Create(Memphis, UCF, Cincy, FAU),
                Temple.Create(Rice,CLT, USF, FAU),
                USF.Create(UTSA,UCF, CLT, Cincy),
                Cincy.Create(Tulane,UCF, Temple, ECU),
                ECU.Create(Houston,CLT, Temple, USF),
                FAU.Create(SMU,USF, Cincy, ECU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Tulsa.Create(USF,SMU, Rice, Tulane),
                SMU.Create(Cincy,Memphis, UTSA, Houston),
                Memphis.Create(ECU,Tulsa, Rice, UTSA),
                Rice.Create(FAU,SMU, Tulane, Houston),
                UTSA.Create(UCF,Tulsa, Rice, Tulane),
                Tulane.Create(CLT,SMU, Memphis, Houston),
                Houston.Create(Temple,Tulsa, Memphis, UTSA),

                UCF.Create(Tulsa,Temple, ECU, FAU),
                CLT.Create( Rice,UCF, Cincy, FAU),
                Temple.Create(UTSA,CLT, USF, FAU),
                USF.Create(Tulane,UCF, CLT, Cincy),
                Cincy.Create(Houston,UCF, Temple, ECU),
                ECU.Create(SMU,CLT, Temple, USF),
                FAU.Create(Memphis,USF, Cincy, ECU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Tulsa.Create(Cincy,SMU, Rice, Tulane),
                SMU.Create(ECU,Memphis, UTSA, Houston),
                Memphis.Create(FAU,Tulsa, Rice, UTSA),
                Rice.Create(UCF,SMU, Tulane, Houston),
                UTSA.Create(CLT,Tulsa, Rice, Tulane),
                Tulane.Create(Temple,SMU, Memphis, Houston),
                Houston.Create(USF,Tulsa, Memphis, UTSA),

                UCF.Create(Tulsa,Temple, ECU, FAU),
                CLT.Create( Houston,UCF, Cincy, FAU),
                Temple.Create(SMU,CLT, USF, FAU),
                USF.Create(Memphis,UCF, CLT, Cincy),
                Cincy.Create(Rice,UCF, Temple, ECU),
                ECU.Create(UTSA,CLT, Temple, USF),
                FAU.Create(Tulane,USF, Cincy, ECU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Tulsa.Create(ECU,SMU, Rice, Tulane),
                SMU.Create(Memphis,FAU, UTSA, Houston),
                Memphis.Create(Tulsa, UCF,Rice, UTSA),
                Rice.Create(SMU, Tulane,CLT, Houston),
                UTSA.Create(Tulsa, Rice, Temple,Tulane),
                Tulane.Create(USF,SMU, Memphis, Houston),
                Houston.Create(Cincy,Tulsa, Memphis, UTSA),

                UCF.Create(Tulsa,Temple, ECU, FAU),
                CLT.Create(Tulane, UCF, Cincy, FAU),
                Temple.Create(Houston,CLT, USF, FAU),
                USF.Create(SMU,UCF, CLT, Cincy),
                Cincy.Create(Memphis,UCF, Temple, ECU),
                ECU.Create(CLT, Temple,Rice, USF),
                FAU.Create(USF, Cincy, ECU,UTSA),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Tulsa.Create(FAU,SMU, Rice, Tulane),
                SMU.Create(Memphis,UCF, UTSA, Houston),
                Memphis.Create(Tulsa, CLT,Rice, UTSA),
                Rice.Create(SMU, Tulane,Temple, Houston),
                UTSA.Create(Tulsa, Rice, Tulane,USF),
                Tulane.Create(SMU, Memphis, Houston,Cincy),
                Houston.Create(Tulsa, Memphis, UTSA,ECU),

                UCF.Create(Temple, ECU, FAU,Tulsa),
                CLT.Create( UCF, Cincy, FAU,UTSA),
                Temple.Create(CLT, USF, FAU,Tulane),
                USF.Create(UCF, CLT, Cincy, Houston),
                Cincy.Create(UCF, Temple, ECU,SMU),
                ECU.Create(CLT, Temple, USF,Memphis),
                FAU.Create(USF, Cincy, ECU,Rice),
            }.Create();
        }

#elif false
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, USFId),
                UAB.Create(TulsaId, MemphisId, HoustonId, TempleId),
                SMUId.Create(UAB, MemphisId, HoustonId, CharlotteId),
                MemphisId.Create(USM, TulsaId, RiceId, ECUId),
                RiceId.Create(UAB, SMUId, HoustonId, CincyId),
                HoustonId.Create(USM, TulsaId, MemphisId, TulaneId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(USM, TulsaId, ECUId, TempleId),
                CincyId.Create(HoustonId, UCFId, TempleId, CharlotteId),
                ECUId.Create(RiceId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(MemphisId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(UAB, TulaneId, UCFId, USFId),
                USFId.Create(SMUId, UCFId, CincyId, ECUId),
            }.Create();
        }


        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, ECUId),
                UAB.Create(TulsaId, MemphisId, HoustonId, CincyId),
                SMUId.Create(UAB, MemphisId, HoustonId, TulaneId),
                MemphisId.Create(USM, TulsaId, RiceId, USFId),
                RiceId.Create(UAB, SMUId, HoustonId, CharlotteId),
                HoustonId.Create(USM, TulsaId, MemphisId, TempleId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, MemphisId, ECUId, TempleId),
                CincyId.Create(SMUId, UCFId, TempleId, CharlotteId),
                ECUId.Create(UAB, TulaneId, CincyId, CharlotteId),
                TempleId.Create(USM, ECUId, CharlotteId, USFId),
                CharlotteId.Create(HoustonId, TulaneId, UCFId, USFId),
                USFId.Create(RiceId, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, CharlotteId),
                UAB.Create(TulsaId, MemphisId, HoustonId, TempleId),
                SMUId.Create(UAB, MemphisId, HoustonId, ECUId),
                MemphisId.Create(USM, TulsaId, RiceId, CincyId),
                RiceId.Create(UAB, SMUId, HoustonId, TulaneId),
                HoustonId.Create(USM, TulsaId, MemphisId, USFId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, HoustonId, ECUId, TempleId),
                CincyId.Create(RiceId, UCFId, TempleId, CharlotteId),
                ECUId.Create(MemphisId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(SMUId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(UAB, TulaneId, UCFId, USFId),
                USFId.Create(USM, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, TulaneId),
                UAB.Create(TulsaId, MemphisId, HoustonId, USFId),
                SMUId.Create(UAB, MemphisId, HoustonId, CharlotteId),
                MemphisId.Create(USM, TulsaId, RiceId, TempleId),
                RiceId.Create(UAB, SMUId, HoustonId, ECUId),
                HoustonId.Create(USM, TulsaId, MemphisId, CincyId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, UAB, ECUId, TempleId),
                CincyId.Create(USM, UCFId, TempleId, CharlotteId),
                ECUId.Create(HoustonId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(RiceId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(MemphisId, TulaneId, UCFId, USFId),
                USFId.Create(SMUId, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, TempleId),
                UAB.Create(TulsaId, MemphisId, HoustonId, ECUId),
                SMUId.Create(UAB, MemphisId, HoustonId, CincyId),
                MemphisId.Create(USM, TulsaId, RiceId, TulaneId),
                RiceId.Create(UAB, SMUId, HoustonId, USFId),
                HoustonId.Create(USM, TulsaId, MemphisId, CharlotteId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, RiceId, ECUId, TempleId),
                CincyId.Create(MemphisId, UCFId, TempleId, CharlotteId),
                ECUId.Create(SMUId, TulaneId, CincyId, CharlotteId),
                TempleId.Create(UAB, ECUId, CharlotteId, USFId),
                CharlotteId.Create(USM, TulaneId, UCFId, USFId),
                USFId.Create(HoustonId, UCFId, CincyId, ECUId),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateF()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USM.Create(UAB, SMUId, RiceId, TulaneId),
                TulsaId.Create(USM, SMUId, RiceId, CincyId),
                UAB.Create(TulsaId, MemphisId, HoustonId, TulaneId),
                SMUId.Create(UAB, MemphisId, HoustonId, USFId),
                MemphisId.Create(USM, TulsaId, RiceId, CharlotteId),
                RiceId.Create(UAB, SMUId, HoustonId, TempleId),
                HoustonId.Create(USM, TulsaId, MemphisId, ECUId),
                TulaneId.Create(UCFId, CincyId, TempleId, USFId),
                UCFId.Create(TulsaId, SMUId, ECUId, TempleId),
                CincyId.Create(UAB, UCFId, TempleId, CharlotteId),
                ECUId.Create(USM, TulaneId, CincyId, CharlotteId),
                TempleId.Create(HoustonId, ECUId, CharlotteId, USFId),
                CharlotteId.Create(RiceId, TulaneId, UCFId, USFId),
                USFId.Create(MemphisId, UCFId, CincyId, ECUId),
            }.Create();
        }

#elif true // 14 east-west close to real life??
        public static Dictionary<int, int[]> CreateZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Army.Create(FAU, ECU, Temple, Tulsa),
                FAU.Create(Temple, Navy, CLT, UTSA),
                ECU.Create(FAU, Navy, CLT, Memphis),
                Temple.Create(ECU, Navy, USF, Tulane),
                Navy.Create(Army, USF, CLT, Rice),
                USF.Create(Army, FAU, ECU, NT),
                CLT.Create(Army, Temple, USF, UAB),

                Tulsa.Create(CLT, UTSA, Memphis, Tulane),
                UTSA.Create(Army, Tulane, NT, UAB),
                Memphis.Create(FAU, UTSA, Tulane, Rice),
                Tulane.Create(ECU, Rice, NT, UAB),
                Rice.Create(Temple, Tulsa, UTSA, UAB),
                NT.Create(Navy, Tulsa, Memphis, Rice),
                UAB.Create(USF, Tulsa, Memphis, NT),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateT()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateW()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateR()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }

        public static Dictionary<int, int[]> CreateS()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }
#elif false // 12 team AAC with no div
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, Memphis, CLT, Rice),
                Tulane.Create(UAB, USF, CLT, SMU),
                UAB.Create(Memphis, UCFId, CLT, SMU),
                Memphis.Create(Temple, UCFId, Tulsa, Rice),
                Temple.Create(ECU, Tulane, USF, Houston),
                USF.Create(UAB, Memphis, UCFId, Rice),
                UCFId.Create(ECU, Temple, Tulsa, Houston),
                Tulsa.Create(ECU, Tulane, UAB, Houston),
                CLT.Create(Temple, USF, Tulsa, SMU),
                SMU.Create(Temple, USF, Tulsa, Houston),
                Rice.Create(UAB, UCFId, CLT, SMU),
                Houston.Create(ECU, Tulane, Memphis, Rice),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, USF, CLT, Houston),
                Tulane.Create(Memphis, UCFId, CLT, Rice),
                UAB.Create(ECU, Tulane, Memphis, Rice),
                Memphis.Create(Temple, Tulsa, CLT, Houston),
                Temple.Create(ECU, UAB, USF, SMU),
                USF.Create(Tulane, Memphis, UCFId, Houston),
                UCFId.Create(UAB, Temple, Tulsa, SMU),
                Tulsa.Create(ECU, Temple, USF, SMU),
                CLT.Create(UAB, UCFId, Tulsa, Rice),
                SMU.Create(ECU, Tulane, Memphis, Houston),
                Rice.Create(Temple, USF, Tulsa, SMU),
                Houston.Create(UAB, UCFId, CLT, Rice),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, USF, CLT, Rice),
                Tulane.Create(UAB, Temple, UCFId, Rice),
                UAB.Create(ECU, Memphis, Tulsa, Houston),
                Memphis.Create(ECU, Tulane, Temple, SMU),
                Temple.Create(UAB, USF, CLT, Rice),
                USF.Create(UAB, UCFId, CLT, Houston),
                UCFId.Create(ECU, Memphis, Tulsa, SMU),
                Tulsa.Create(Tulane, Temple, USF, Houston),
                CLT.Create(Memphis, UCFId, Tulsa, SMU),
                SMU.Create(ECU, UAB, USF, Houston),
                Rice.Create(Memphis ,UCFId, Tulsa, SMU),
                Houston.Create(Tulane, Temple, CLT, Rice),
            }.Create();
        }

#elif false // 11 team AAC
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(Rice, CLT, Tulsa, UCFId),
                Rice.Create(UCFId, SMU, Temple, Houston),
                CLT.Create(Rice, Tulsa, UCFId, Memphis),
                Tulsa.Create(Rice, Temple, ECU, Tulane),
                UCFId.Create(SMU, Memphis, ECU, Tulane),
                SMU.Create(USF, Houston, Memphis, ECU),
                Temple.Create(USF, SMU, Houston, Memphis),
                Houston.Create(CLT, Tulsa, UCFId, Tulane),
                Memphis.Create(Rice, Tulsa, ECU, Tulane),
                ECU.Create(USF, CLT, Temple, Houston),
                Tulane.Create(USF, CLT, SMU, Temple),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(),
                Rice.Create(),
                CLT.Create(),
                Tulsa.Create(),
                UCFId.Create(),
                SMU.Create(),
                Temple.Create(),
                Houston.Create(),
                Memphis.Create(),
                ECU.Create(),
                Tulane.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(),
                Rice.Create(),
                CLT.Create(),
                Tulsa.Create(),
                UCFId.Create(),
                SMU.Create(),
                Temple.Create(),
                Houston.Create(),
                Memphis.Create(),
                ECU.Create(),
                Tulane.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(),
                Rice.Create(),
                CLT.Create(),
                Tulsa.Create(),
                UCFId.Create(),
                SMU.Create(),
                Temple.Create(),
                Houston.Create(),
                Memphis.Create(),
                ECU.Create(),
                Tulane.Create(),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                USF.Create(),
                Rice.Create(),
                CLT.Create(),
                Tulsa.Create(),
                UCFId.Create(),
                SMU.Create(),
                Temple.Create(),
                Houston.Create(),
                Memphis.Create(),
                ECU.Create(),
                Tulane.Create(),
            }.Create();
        }
#elif false //14 team with no cincy, utsa/uab/usm no div!
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                UTSA.Create(CLT, UCFId, USM, UAB),
                SMU.Create(UTSA, Houston, USF, Tulane),
                Houston.Create(UTSA, Tulsa, CLT, Memphis),
                Rice.Create(SMU, Houston, ECU, Temple),
                Tulsa.Create(Rice, Temple, USM, UAB),
                CLT.Create(SMU, Tulsa, UCFId, Memphis),
                ECU.Create(UTSA, CLT, UAB, Tulane),
                UCFId.Create(Houston, Tulsa, ECU, Temple),
                USF.Create(Rice, CLT, UCFId, Tulane),
                Temple.Create(SMU, ECU, USF, Memphis),
                Memphis.Create(SMU, Tulsa, USF, USM),
                USM.Create(Rice, ECU, UCFId, UAB),
                UAB.Create(Rice, USF, Memphis, Tulane),
                Tulane.Create(UTSA, Houston, Temple, USM),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
            }.Create();
        }
#elif false // 12 team with cincy/ucf
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,TempleId,MemphisId,TulaneId} },
                {CharlotteId,new[] {UCFId,ECUId,CincyId,RiceId} },
                {CincyId,new[] {USFId,ECUId,TempleId,HoustonId} },
                {TempleId,new[] {USFId,CharlotteId,SMUId,TulaneId} },
                {TulsaId,new[] {CincyId,CharlotteId,RiceId,TulaneId} },
                {RiceId,new[] {UCFId,CincyId,SMUId,HoustonId} },
                {SMUId,new[] {ECUId,TulsaId,MemphisId,TulaneId} },
                {HoustonId,new[] {UCFId,CharlotteId,TulsaId,SMUId} },
                {MemphisId,new[] {TempleId,TulsaId,RiceId,HoustonId} },
                {TulaneId,new[] {USFId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CincyId,TempleId,RiceId} },
                {CharlotteId,new[] {UCFId,ECUId,CincyId,TulaneId} },
                {CincyId,new[] {USFId,TempleId,MemphisId,TulaneId} },
                {TempleId,new[] {USFId,CharlotteId,RiceId,HoustonId} },
                {TulsaId,new[] {USFId,TempleId,RiceId,TulaneId} },
                {RiceId,new[] {CharlotteId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {UCFId,CharlotteId,TulsaId,TulaneId} },
                {HoustonId,new[] {ECUId,CincyId,TulsaId,SMUId} },
                {MemphisId,new[] {UCFId,TulsaId,SMUId,HoustonId} },
                {TulaneId,new[] {ECUId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,RiceId,HoustonId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CharlotteId,CincyId,SMUId} },
                {CharlotteId,new[] {UCFId,CincyId,TulsaId,MemphisId} },
                {CincyId,new[] {USFId,TempleId,RiceId,HoustonId} },
                {TempleId,new[] {USFId,ECUId,CharlotteId,TulaneId} },
                {TulsaId,new[] {ECUId,RiceId,MemphisId,TulaneId} },
                {RiceId,new[] {UCFId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {CincyId,CharlotteId,TulsaId,TulaneId} },
                {HoustonId,new[] {TempleId,TulsaId,SMUId,TulaneId} },
                {MemphisId,new[] {ECUId,TempleId,SMUId,HoustonId} },
                {TulaneId,new[] {USFId,UCFId,RiceId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,SMUId,MemphisId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CharlotteId,TempleId,TulaneId} },
                {CharlotteId,new[] {UCFId,CincyId,HoustonId,TulaneId} },
                {CincyId,new[] {USFId,ECUId,TempleId,RiceId} },
                {TempleId,new[] {USFId,CharlotteId,TulsaId,RiceId} },
                {TulsaId,new[] {CincyId,RiceId,MemphisId,TulaneId} },
                {RiceId,new[] {ECUId,SMUId,HoustonId,MemphisId} },
                {SMUId,new[] {UCFId,CharlotteId,TulsaId,TulaneId} },
                {HoustonId,new[] {USFId,UCFId,TulsaId,SMUId} },
                {MemphisId,new[] {ECUId,CincyId,SMUId,HoustonId} },
                {TulaneId,new[] {TempleId,RiceId,HoustonId,MemphisId} },
            };
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                {USFId,new[] {UCFId,CharlotteId,RiceId,TulaneId} },
                {UCFId,new[] {ECUId,CincyId,TempleId,TulsaId} },
                {ECUId,new[] {USFId,CharlotteId,TempleId,HoustonId} },
                {CharlotteId,new[] {UCFId,CincyId,HoustonId,MemphisId} },
                {CincyId,new[] {USFId,ECUId,TempleId,TulaneId} },
                {TempleId,new[] {USFId,CharlotteId,SMUId,RiceId} },
                {TulsaId,new[] {USFId,ECUId,RiceId,MemphisId} },
                {RiceId,new[] {CharlotteId,SMUId,MemphisId,TulaneId} },
                {SMUId,new[] {ECUId,CincyId,TulsaId,TulaneId} },
                {HoustonId,new[] {TempleId,TulsaId,RiceId,SMUId} },
                {MemphisId,new[] {UCFId,CincyId,SMUId,HoustonId} },
                {TulaneId,new[] {UCFId,TulsaId,HoustonId,MemphisId} },
            };
        }
#endif
    }
}