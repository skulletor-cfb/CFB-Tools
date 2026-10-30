using System;
using System.Collections.Generic;
using System.Linq;

namespace NCAA14Utility.Scheduling
{
    public class MACSchedule : ScheduleBase
    {
        public MACSchedule(int year) : base(year, 12, ScheduleHelpers.MAC, nameof(ScheduleHelpers.MAC))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateX, CreateY,
            CreateZ, CreateP,
            CreateQ, CreateX,
            CreateY, CreateZ,
            CreateP, CreateQ,
        };

        protected override int InitialYear => 2589;

        // EADBEditor's MACLocks; which pair of rivalries ends the season goes by the year
        protected override Func<int, int, int?>[] LockChecks => Array.Empty<Func<int, int, int?>>();

        public static Dictionary<int, int[]> CreateP()
        {
            return new List<KeyValuePair<int, int[]>>
            {
    Akron.Create(Miami, Buffalo, UMass, WMU),
    KentSt.Create(Akron, BallSt, UMass, EMU),
    BGSU.Create(KentSt, BallSt, Ohio, WMU),
    Toledo.Create(KentSt, BGSU, Buffalo, CMU),
    NIU.Create(KentSt, Toledo, Miami, WMU),
    BallSt.Create(Akron, NIU, UMass, EMU),
    Miami.Create(BGSU, Toledo, BallSt, EMU),
    Ohio.Create(Akron, NIU, Miami, CMU),
    Buffalo.Create(BGSU, BallSt, Ohio, CMU),
    UMass.Create(Toledo, NIU, Ohio, Buffalo),
    EMU.Create(Akron, BGSU, NIU, CMU),
    CMU.Create(KentSt, Miami, UMass, WMU),
    WMU.Create(Toledo, Ohio, Buffalo, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateQ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
    Akron.Create(Toledo, NIU, UMass, CMU),
    KentSt.Create(Akron, NIU, Miami, WMU),
    BGSU.Create(Akron, KentSt, Buffalo, UMass),
    Toledo.Create(BGSU, BallSt, Ohio, EMU),
    NIU.Create(BGSU, Toledo, Buffalo, CMU),
    BallSt.Create(BGSU, NIU, Ohio, CMU),
    Miami.Create(Akron, Toledo, BallSt, WMU),
    Ohio.Create(KentSt, Miami, UMass, EMU),
    Buffalo.Create(KentSt, Miami, Ohio, EMU),
    UMass.Create(BallSt, Miami, Buffalo, WMU),
    EMU.Create(KentSt, NIU, UMass, CMU),
    CMU.Create(BGSU, Toledo, Buffalo, WMU),
    WMU.Create(Akron, BallSt, Ohio, EMU),
            }.Create();
        }
        public static Dictionary<int, int[]> CreateX()
        {
            return new List<KeyValuePair<int, int[]>>
            {
    Akron.Create(BallSt, Ohio, UMass, EMU),
    KentSt.Create(Akron, Toledo, Buffalo, CMU),
    BGSU.Create(KentSt, NIU, Miami, EMU),
    Toledo.Create(Akron, BGSU, UMass, WMU),
    NIU.Create(Toledo, Ohio, UMass, CMU),
    BallSt.Create(Toledo, NIU, Buffalo, WMU),
    Miami.Create(KentSt, NIU, BallSt, WMU),
    Ohio.Create(BGSU, BallSt, Miami, CMU),
    Buffalo.Create(Akron, NIU, Miami, Ohio),
    UMass.Create(KentSt, BGSU, Buffalo, EMU),
    EMU.Create(Toledo, BallSt, Ohio, CMU),
    CMU.Create(Akron, Miami, UMass, WMU),
    WMU.Create(KentSt, BGSU, Buffalo, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateY()
        {
            return new List<KeyValuePair<int, int[]>>
            {
    Akron.Create(BGSU, Buffalo, UMass, WMU),
    KentSt.Create(Akron, Ohio, UMass, EMU),
    BGSU.Create(KentSt, BallSt, Ohio, CMU),
    Toledo.Create(KentSt, BGSU, Miami, Buffalo),
    NIU.Create(Akron, Toledo, Miami, EMU),
    BallSt.Create(Akron, KentSt, NIU, EMU),
    Miami.Create(Akron, BallSt, UMass, CMU),
    Ohio.Create(Toledo, NIU, Miami, WMU),
    Buffalo.Create(BGSU, BallSt, Ohio, WMU),
    UMass.Create(Toledo, NIU, Buffalo, CMU),
    EMU.Create(BGSU, Miami, Buffalo, CMU),
    CMU.Create(KentSt, BallSt, Ohio, WMU),
    WMU.Create(Toledo, NIU, UMass, EMU),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateZ()
        {
            return new List<KeyValuePair<int, int[]>>
            {
    Akron.Create(Toledo, NIU, UMass, CMU),
    KentSt.Create(Akron, NIU, BallSt, Miami),
    BGSU.Create(Akron, KentSt, UMass, WMU),
    Toledo.Create(BGSU, BallSt, Ohio, CMU),
    NIU.Create(BGSU, Toledo, Buffalo, WMU),
    BallSt.Create(NIU, Ohio, UMass, CMU),
    Miami.Create(BGSU, BallSt, Buffalo, EMU),
    Ohio.Create(Akron, KentSt, Miami, EMU),
    Buffalo.Create(KentSt, Toledo, Ohio, EMU),
    UMass.Create(Miami, Ohio, Buffalo, WMU),
    EMU.Create(Akron, Toledo, UMass, CMU),
    CMU.Create(BGSU, NIU, Buffalo, WMU),
    WMU.Create(KentSt, BallSt, Miami, EMU),
            }.Create();
        }
    }
}