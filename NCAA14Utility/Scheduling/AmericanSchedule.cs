using System;
using System.Collections.Generic;

namespace NCAA14Utility.Scheduling
{
    public class AmericanSchedule : ScheduleBase
    {
        public AmericanSchedule(int year) : base(year, 12, ScheduleHelpers.American, nameof(ScheduleHelpers.American))
        {
        }

        protected override Func<Dictionary<int, int[]>>[] Creators => new Func<Dictionary<int, int[]>>[] {
            CreateA, CreateB,
            CreateC, CreateA,
            CreateB, CreateC,
        };

        protected override int InitialYear => 2587;

        public static Dictionary<int, int[]> CreateA()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, Memphis, Charlotte, Rice),
                Tulane.Create(UAB, USF, Charlotte, SMU),
                UAB.Create(Memphis, UCF, Charlotte, SMU),
                Memphis.Create(Temple, UCF, Tulsa, Rice),
                Temple.Create(ECU, Tulane, USF, Houston),
                USF.Create(UAB, Memphis, UCF, Rice),
                UCF.Create(ECU, Temple, Tulsa, Houston),
                Tulsa.Create(ECU, Tulane, UAB, Houston),
                Charlotte.Create(Temple, USF, Tulsa, SMU),
                SMU.Create(Temple, USF, Tulsa, Houston),
                Rice.Create(UAB, UCF, Charlotte, SMU),
                Houston.Create(ECU, Tulane, Memphis, Rice),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateB()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, USF, Charlotte, Houston),
                Tulane.Create(Memphis, UCF, Charlotte, Rice),
                UAB.Create(ECU, Tulane, Memphis, Rice),
                Memphis.Create(Temple, Tulsa, Charlotte, Houston),
                Temple.Create(ECU, UAB, USF, SMU),
                USF.Create(Tulane, Memphis, UCF, Houston),
                UCF.Create(UAB, Temple, Tulsa, SMU),
                Tulsa.Create(ECU, Temple, USF, SMU),
                Charlotte.Create(UAB, UCF, Tulsa, Rice),
                SMU.Create(ECU, Tulane, Memphis, Houston),
                Rice.Create(Temple, USF, Tulsa, SMU),
                Houston.Create(UAB, UCF, Charlotte, Rice),
            }.Create();
        }

        public static Dictionary<int, int[]> CreateC()
        {
            return new List<KeyValuePair<int, int[]>>
            {
                ECU.Create(Tulane, USF, Charlotte, Rice),
                Tulane.Create(UAB, Temple, UCF, Rice),
                UAB.Create(ECU, Memphis, Tulsa, Houston),
                Memphis.Create(ECU, Tulane, Temple, SMU),
                Temple.Create(UAB, USF, Charlotte, Rice),
                USF.Create(UAB, UCF, Charlotte, Houston),
                UCF.Create(ECU, Memphis, Tulsa, SMU),
                Tulsa.Create(Tulane, Temple, USF, Houston),
                Charlotte.Create(Memphis, UCF, Tulsa, SMU),
                SMU.Create(ECU, UAB, USF, Houston),
                Rice.Create(Memphis ,UCF, Tulsa, SMU),
                Houston.Create(Tulane, Temple, Charlotte, Rice),
            }.Create();
        }
    }
}
