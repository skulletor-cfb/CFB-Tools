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
        public static Func<Dictionary<int, int[]>>[] Creators = new Func<Dictionary<int, int[]>>[] { CreateA, CreateA, CreateB, CreateB, CreateC, CreateC, CreateD, CreateD, CreateE, CreateE };
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

        public static void ProcessMACSchedule(Dictionary<int, PreseasonScheduledGame[]> schedule)
        {
            schedule.ProcessSchedule(ScenarioForSeason, MACConferenceSchedule, RecruitingFixup.MACId, RecruitingFixup.MAC);
        }


        public static Dictionary<int, int[]> CreateScenarioForSeason()
        {
            var idx = (Form1.DynastyYear - 2211) % Creators.Length;
            var result = Creators[idx]();
            result = result.Verify(12, RecruitingFixup.MACId, "MAC");
            MACConferenceSchedule = result.BuildHashSet();
            return result;
        }


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
    }
}
