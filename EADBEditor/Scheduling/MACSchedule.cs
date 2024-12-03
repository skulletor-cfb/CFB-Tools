using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public class MACSchedule
    {
        private const int Miami = 50;
        private const int Toledo = 95;
        private const int EMU = 26;
        private const int WMU = 113;
        private const int NIU = 66;
        private const int BallSt = 10;
        private const int CMU = 19;
        private const int Buffalo = 15;
        private const int Akron = 2;
        private const int KentSt = 41;
        private const int Ohio = 69;
        private const int BGSU = 14;
        private static bool initRun = false;
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] 
        { 
            CreateA, CreateB, 
            CreateC, CreateA, 
            CreateB, CreateC
        };
        public static Dictionary<int, HashSet<int>> MACConferenceSchedule = null;
        public static Dictionary<int, int[]> ScenarioForSeason = null;

        public static void Init()
        {
            if (!initRun)
            {
                ScenarioForSeason = CreateScenarioForSeason();
                initRun = true;
            }
        }

        public static void ProcessMACSchedule(Dictionary<int, TeamSchedule> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, MACConferenceSchedule, RecruitingFixup.MACId, RecruitingFixup.MAC);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2491) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.MACId, "MAC");
            MACConferenceSchedule = result.BuildHashSet();
            return result;
        }


#if true // no divisions
        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Akron.Create(Miami, Ohio, Buffalo, CMU),
                KentSt.Create(Akron, NIU, Buffalo, WMU),
                BGSU.Create(KentSt, NIU, Miami, EMU),
                Toledo.Create(Akron, BGSU, BallSt, CMU),
                NIU.Create(Akron, Toledo, Ohio, WMU),
                BallSt.Create(KentSt, BGSU, NIU, EMU),
                Miami.Create(Toledo, BallSt, Buffalo, CMU),
                Ohio.Create(KentSt, Toledo, Miami, WMU),
                Buffalo.Create(BGSU, BallSt, Ohio, EMU),
                EMU.Create(KentSt, NIU, Ohio, CMU),
                CMU.Create(BGSU, BallSt, Buffalo, WMU),
                WMU.Create(Akron, Toledo, Miami, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Akron.Create(NIU, Ohio, Buffalo, EMU),
                KentSt.Create(Akron, BallSt, Buffalo, CMU),
                BGSU.Create(Akron, KentSt, NIU, WMU),
                Toledo.Create(KentSt, BGSU, BallSt, EMU),
                NIU.Create(Toledo, Miami, Buffalo, CMU),
                BallSt.Create(Akron, NIU, Ohio, WMU),
                Miami.Create(KentSt, BGSU, BallSt, EMU),
                Ohio.Create(BGSU, Toledo, Miami, CMU),
                Buffalo.Create(Toledo, Miami, Ohio, WMU),
                EMU.Create(KentSt, NIU, Ohio, CMU),
                CMU.Create(BGSU, BallSt, Buffalo, WMU),
                WMU.Create(Akron, Toledo, Miami, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Akron.Create(BallSt, Miami, Buffalo, EMU),
                KentSt.Create(Akron, NIU, Ohio, CMU),
                BGSU.Create(Akron, KentSt, Buffalo, WMU),
                Toledo.Create(Akron, KentSt, BGSU, EMU),
                NIU.Create(Toledo, Miami, Ohio, CMU),
                BallSt.Create(BGSU, NIU , Buffalo, WMU),
                Miami.Create(KentSt, Toledo, BallSt, EMU),
                Ohio.Create(BGSU, BallSt, Miami, CMU),
                Buffalo.Create(Toledo, NIU, Ohio, WMU),
                EMU.Create(BGSU, BallSt, Buffalo, CMU),
                CMU.Create(Akron, Toledo, Miami, WMU),
                WMU.Create(KentSt, NIU, Ohio, EMU),
            }.Create();
        }

#else
        public static Dictionary<int, int[]> CreateA()
        {
            return new Dictionary<int, int[]>()
            {
                {Miami,new[] {BGSU,Buffalo,WMU,BallSt} },
                {BGSU,new[] {KentSt,Ohio,Akron,Toledo} },
                {KentSt,new[] {Miami,Akron,BallSt,NIU} },
                {Buffalo,new[] {BGSU,KentSt,Ohio,EMU} },
                {Ohio,new[] {Miami,KentSt,Akron,CMU} },
                {Akron,new[] {Miami,Buffalo,WMU,NIU} },
                {Toledo,new[] {Ohio,Buffalo,EMU,NIU} },
                {EMU,new[] {BGSU,Ohio,WMU,CMU} },
                {WMU,new[] {KentSt,Toledo,BallSt,NIU} },
                {CMU,new[] {BGSU,Buffalo,Toledo,WMU} },
                {BallSt,new[] {Akron,Toledo,EMU,CMU} },
                {NIU,new[] {Miami,EMU,CMU,BallSt} },
            };
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new Dictionary<int, int[]>()
            {
                {Miami,new[] {BGSU,Buffalo,WMU,BallSt} },
                {BGSU,new[] {KentSt,Ohio,Akron,Toledo} },
                {KentSt,new[] {Miami,Ohio,Akron,EMU} },
                {Buffalo,new[] {BGSU,KentSt,Ohio,NIU} },
                {Ohio,new[] {Miami,Akron,BallSt,NIU} },
                {Akron,new[] {Miami,Buffalo,EMU,CMU} },
                {Toledo,new[] {Miami,Akron,EMU,NIU} },
                {EMU,new[] {Buffalo,WMU,CMU,BallSt} },
                {WMU,new[] {BGSU,Buffalo,Toledo,NIU} },
                {CMU,new[] {KentSt,Ohio,Toledo,WMU} },
                {BallSt,new[] {BGSU,Toledo,WMU,CMU} },
                {NIU,new[] {KentSt,EMU,CMU,BallSt} },
            };
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new Dictionary<int, int[]>()
            {
                {Miami,new[] {BGSU,Buffalo,EMU,CMU} },
                {BGSU,new[] {KentSt,Ohio,Akron,Toledo} },
                {KentSt,new[] {Miami,Buffalo,Ohio,WMU} },
                {Buffalo,new[] {BGSU,Ohio,Toledo,BallSt} },
                {Ohio,new[] {Miami,Akron,EMU,CMU} },
                {Akron,new[] {Miami,KentSt,Buffalo,NIU} },
                {Toledo,new[] {KentSt,EMU,BallSt,NIU} },
                {EMU,new[] {BGSU,WMU,CMU,BallSt} },
                {WMU,new[] {Ohio,Buffalo,Toledo,NIU} },
                {CMU,new[] {Akron,Toledo,WMU,NIU} },
                {BallSt,new[] {KentSt,Akron,WMU,CMU} },
                {NIU,new[] {Miami,BGSU,EMU,BallSt} },
            };
        }

        public static Dictionary<int, int[]> CreateD()
        {
            return new Dictionary<int, int[]>()
            {
                {Miami,new[] {BGSU,Buffalo,WMU,BallSt} },
                {BGSU,new[] {KentSt,Ohio,Akron,Toledo} },
                {KentSt,new[] {Miami,Buffalo,Akron,NIU} },
                {Buffalo,new[] {BGSU,Ohio,CMU,NIU} },
                {Ohio,new[] {Miami,KentSt,Akron,EMU} },
                {Akron,new[] {Miami,Buffalo,Toledo,EMU} },
                {Toledo,new[] {Ohio,EMU,BallSt,NIU} },
                {EMU,new[] {KentSt,WMU,CMU,BallSt} },
                {WMU,new[] {BGSU,Buffalo,Toledo,NIU} },
                {CMU,new[] {Miami,BGSU,Toledo,WMU} },
                {BallSt,new[] {KentSt,Ohio,WMU,CMU} },
                {NIU,new[] {Akron,EMU,CMU,BallSt} },
            };
        }

        public static Dictionary<int, int[]> CreateE()
        {
            return new Dictionary<int, int[]>()
            {
                {Miami,new[] {BGSU,Buffalo,EMU,NIU} },
                {BGSU,new[] {KentSt,Ohio,Akron,Toledo} },
                {KentSt,new[] {Miami,Buffalo,Akron,CMU} },
                {Buffalo,new[] {BGSU,Ohio,CMU,BallSt} },
                {Ohio,new[] {Miami,KentSt,Akron,NIU} },
                {Akron,new[] {Miami,Buffalo,WMU,EMU} },
                {Toledo,new[] {Miami,KentSt,EMU,BallSt} },
                {EMU,new[] {Buffalo,WMU,BallSt,NIU} },
                {WMU,new[] {KentSt,Ohio,Toledo,NIU} },
                {CMU,new[] {Akron,Toledo,EMU,WMU} },
                {BallSt,new[] {BGSU,Ohio,WMU,CMU} },
                {NIU,new[] {BGSU,Toledo,CMU,BallSt} },
            };
        }
#endif
    }
}
