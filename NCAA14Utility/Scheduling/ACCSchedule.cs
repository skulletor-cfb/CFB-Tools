using System;
using System.Collections.Generic;

namespace NCAA14Utility.Scheduling
{
    public class ACCSchedule : ScheduleBase
    {
        public ACCSchedule(int year) : base(year, 16, ScheduleHelpers.ACC, nameof(ScheduleHelpers.ACC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateC, CreateA,
            CreateB, CreateC,
            CreateA, CreateB,
        };

        protected override int InitialYear => 2487;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Miami.Create(UVA, Duke, FSU, Pitt),
                BC.Create(Miami, SU, GT, WF),
                WVU.Create(BC, VT, Pitt, UL),
                VT.Create(Miami, BC, FSU, Pitt),
                Clemson.Create(WVU, VT, NCSU, UL),
                SU.Create(Miami, WVU, NCSU, UMD),
                UVA.Create(VT, SU, Duke, UMD),
                Duke.Create(BC, UNC, UMD, Pitt),
                NCSU.Create(WVU, VT, Duke, WF),
                UNC.Create(Clemson, UVA, NCSU, UL),
                FSU.Create(BC, Clemson, UVA, NCSU),
                UMD.Create(Miami, WVU, Clemson, UNC),
                Pitt.Create(SU, GT, WF, UL),
                GT.Create(Clemson, UVA, UNC, FSU),
                WF.Create(Duke, UNC, UMD, GT),
                UL.Create(SU, FSU, GT, WF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            // redo home/away for ACC rivalries
            return new List<KeyValuePair<int, int[]>>
            {
                Miami.Create(WVU, UNC, FSU, Pitt),
                BC.Create(Miami, SU, UVA, UMD),
                WVU.Create(VT, Duke, Pitt, UL),
                VT.Create(Miami, BC, GT, WF),
                Clemson.Create(Miami, BC, NCSU, UL),
                SU.Create(WVU, UNC, FSU, GT),
                UVA.Create(VT,Clemson, NCSU, UMD),
                Duke.Create(SU, UNC, UMD, UL),
                NCSU.Create(Duke, Pitt, GT, WF),
                UNC.Create(BC, WVU, UVA, NCSU),
                FSU.Create(Clemson, UVA, Duke, GT),
                UMD.Create(WVU, VT, NCSU, FSU),
                Pitt.Create(BC, SU, UMD, UL),
                GT.Create(Clemson, UVA, Pitt, WF),
                WF.Create(Clemson, Duke, UNC, FSU),
                UL.Create(Miami, VT, SU, WF),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                Miami.Create(SU, FSU, GT, WF),
                BC.Create(Miami, SU, GT, UL),
                WVU.Create(VT, FSU, Pitt, WF),
                VT.Create(Miami, BC, UNC, GT),
                Clemson.Create(BC, SU, NCSU, Pitt),
                SU.Create(WVU, VT, UMD, WF),
                UVA.Create(WVU, VT, UMD, Pitt),
                Duke.Create(VT, Clemson, UNC, UL),
                NCSU.Create(Miami, BC, Duke, WF),
                UNC.Create(UVA, NCSU, UMD, Pitt),
                FSU.Create(Clemson, UVA, UNC, UL),
                UMD.Create(WVU, NCSU, FSU, UL),
                Pitt.Create(BC, SU, Duke, FSU),
                GT.Create(WVU, Clemson, Duke, UMD),
                WF.Create(Clemson, UVA, Duke, UNC),
                UL.Create(Miami, UVA, NCSU, GT),
            }.Create();
        }

    }
}
