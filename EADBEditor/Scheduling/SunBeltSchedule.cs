using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public class SunBeltSchedule
    {
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB, CreateC, CreateC, CreateD, CreateD, CreateE, CreateE };
        public static Dictionary<int, HashSet<int>> SunbeltConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

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
            var idx = (Form1.DynastyYear - 2356) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.SBCId, "SunBelt");
            SunbeltConferenceSchedule = result.BuildHashSet();
            return result;
        }

        const int TexStId = 218;
        const int ArkStId = 7;
        const int NorthTexasId = 64;
        const int UTEPId = 105;
        const int WKUId = 211;
        const int MTSUId = 53;


        const int UTSAId = 232;
        const int USMId = 85;
        const int UABId = 98;
        const int LTId = 43;
        const int ULMId = 65;
        const int ULLId = 86;

        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                { ArkStId,new[]{ TexStId,MTSUId,ULMId,LTId} },
                { NorthTexasId,new[]{ ArkStId,TexStId,WKUId,ULLId} },
                { TexStId,new[]{ MTSUId,UTEPId,UTSAId,ULMId} },
                { MTSUId,new[]{ NorthTexasId,WKUId,UABId,UTSAId} },
                { WKUId,new[]{ ArkStId,TexStId,UTEPId,USMId} },
                { UTEPId,new[]{ ArkStId,NorthTexasId,MTSUId,UABId} },
                { ULMId,new[]{ NorthTexasId,UTSAId,ULLId,LTId} },
                { UABId,new[]{ TexStId,ULMId,USMId,ULLId} },
                { USMId,new[]{ NorthTexasId,UTEPId,ULMId,LTId} },
                { UTSAId,new[]{ WKUId,UABId,USMId,ULLId} },
                { ULLId,new[]{ ArkStId,WKUId,USMId,LTId} },
                { LTId,new[]{ MTSUId,UTEPId,UABId,UTSAId} },
            };
        }
        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                { ArkStId,new[]{ TexStId,MTSUId,ULMId,USMId} },
                { NorthTexasId,new[]{ ArkStId,TexStId,WKUId,UABId} },
                { TexStId,new[]{ MTSUId,UTEPId,UTSAId,LTId} },
                { MTSUId,new[]{ NorthTexasId,WKUId,UABId,ULLId} },
                { WKUId,new[]{ ArkStId,TexStId,UTEPId,ULMId} },
                { UTEPId,new[]{ ArkStId,NorthTexasId,MTSUId,UTSAId} },
                { ULMId,new[]{ UTEPId,UTSAId,ULLId,LTId} },
                { UABId,new[]{ WKUId,ULMId,USMId,ULLId} },
                { USMId,new[]{ NorthTexasId,MTSUId,ULMId,LTId} },
                { UTSAId,new[]{ ArkStId,UABId,USMId,ULLId} },
                { ULLId,new[]{ NorthTexasId,TexStId,USMId,LTId} },
                { LTId,new[]{ WKUId,UTEPId,UABId,UTSAId} },
            };
        }
        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                { ArkStId,new[]{ TexStId,MTSUId,ULMId,USMId} },
                { NorthTexasId,new[]{ ArkStId,TexStId,WKUId,LTId} },
                { TexStId,new[]{ MTSUId,UTEPId,UTSAId,UABId} },
                { MTSUId,new[]{ NorthTexasId,WKUId,UTSAId,ULMId} },
                { WKUId,new[]{ ArkStId,TexStId,UTEPId,ULLId} },
                { UTEPId,new[]{ ArkStId,NorthTexasId,MTSUId,UABId} },
                { ULMId,new[]{ NorthTexasId,UTSAId,ULLId,LTId} },
                { UABId,new[]{ WKUId,ULMId,USMId,ULLId} },
                { USMId,new[]{ TexStId,WKUId,ULMId,LTId} },
                { UTSAId,new[]{ NorthTexasId,UABId,USMId,ULLId} },
                { ULLId,new[]{ MTSUId,UTEPId,USMId,LTId} },
                { LTId,new[]{ ArkStId,UTEPId,UABId,UTSAId} },
            };
        }
        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                { ArkStId,new[]{ TexStId,MTSUId,ULMId,UABId} },
                { NorthTexasId,new[]{ ArkStId,TexStId,WKUId,UABId} },
                { TexStId,new[]{ MTSUId,UTEPId,UTSAId,ULMId} },
                { MTSUId,new[]{ NorthTexasId,WKUId,USMId,LTId} },
                { WKUId,new[]{ ArkStId,TexStId,UTEPId,UTSAId} },
                { UTEPId,new[]{ ArkStId,NorthTexasId,MTSUId,ULLId} },
                { ULMId,new[]{ WKUId,UTSAId,ULLId,LTId} },
                { UABId,new[]{ MTSUId,ULMId,USMId,ULLId} },
                { USMId,new[]{ NorthTexasId,UTEPId,ULMId,LTId} },
                { UTSAId,new[]{ UTEPId,UABId,USMId,ULLId} },
                { ULLId,new[]{ ArkStId,TexStId,USMId,LTId} },
                { LTId,new[]{ NorthTexasId,WKUId,UABId,UTSAId} },
            };
        }
        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                { ArkStId,new[]{ TexStId,MTSUId,ULMId,UTSAId} },
                { NorthTexasId,new[]{ ArkStId,TexStId,WKUId,UABId} },
                { TexStId,new[]{ MTSUId,UTEPId,UTSAId,LTId} },
                { MTSUId,new[]{ NorthTexasId,WKUId,ULMId,USMId} },
                { WKUId,new[]{ ArkStId,TexStId,UTEPId,UABId} },
                { UTEPId,new[]{ ArkStId,NorthTexasId,MTSUId,ULLId} },
                { ULMId,new[]{ UTEPId,UTSAId,ULLId,LTId} },
                { UABId,new[]{ ArkStId,ULMId,USMId,ULLId} },
                { USMId,new[]{ TexStId,UTEPId,ULMId,LTId} },
                { UTSAId,new[]{ NorthTexasId,UABId,USMId,ULLId} },
                { ULLId,new[]{ MTSUId,WKUId,USMId,LTId} },
                { LTId,new[]{ NorthTexasId,WKUId,UABId,UTSAId} },
            };
        }
    }
}
