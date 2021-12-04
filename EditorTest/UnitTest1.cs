using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EA_DB_Editor;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace EditorTest
{
    [TestClass]
    public class UnitTest1
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            Form1.DynastyYear = 2184;
        }

        [TestMethod]
        public void AccScheduleA()
        {
            ACCPodSchedule.Verify(ACCPodSchedule.CreateA(), 16, RecruitingFixup.ACCId, "ACC", false);
        }

        [TestMethod]
        public void AccScheduleB()
        {
            ACCPodSchedule.Verify(ACCPodSchedule.CreateB(), 16, RecruitingFixup.ACCId, "ACC", false);
        }

        [TestMethod]
        public void AccScheduleC()
        {
            ACCPodSchedule.Verify(ACCPodSchedule.CreateC(), 16, RecruitingFixup.ACCId, "ACC", false);
        }

        [TestMethod]
        public void Big12ScheduleA()
        {
            Big12Schedule.CreateA().Verify(12, RecruitingFixup.Big12Id, "Big12", false);
        }

        [TestMethod]
        public void Big12ScheduleB()
        {
            Big12Schedule.CreateB().Verify(12, RecruitingFixup.Big12Id, "Big12", false);
        }

        [TestMethod]
        public void Big12ScheduleC()
        {
            Big12Schedule.CreateC().Verify(12, RecruitingFixup.Big12Id, "Big12", false);
        }

        [TestMethod]
        public void Big12ScheduleD()
        {
            Big12Schedule.CreateD().Verify(12, RecruitingFixup.Big12Id, "Big12", false);
        }

        [TestMethod]
        public void Big12ScheduleE()
        {
            Big12Schedule.CreateE().Verify(12, RecruitingFixup.Big12Id, "Big12", false);
        }

        [TestMethod]
        public void Pac12ScheduleA()
        {
            Pac12Schedule.CreateA().Verify(12, RecruitingFixup.Pac16Id, "Pac12  ", false);
        }

        [TestMethod]
        public void Pac12ScheduleB()
        {
            Pac12Schedule.CreateB().Verify(12, RecruitingFixup.Pac16Id, "Pac12  ", false);
        }

        [TestMethod]
        public void MWCScheduleA()
        {
            MWCSchedule.CreateA().Verify(12, RecruitingFixup.MWCId, "MWC  ", false);
        }

        [TestMethod]
        public void MWCScheduleB()
        {
            MWCSchedule.CreateB().Verify(12, RecruitingFixup.MWCId, "MWC  ", false);
        }



        [TestMethod]
        public void AmericanScheduleA()
        {
            AmericanSchedule.CreateA().Verify(12, RecruitingFixup.AmericanId, "American  ", false);
            var hash = AmericanSchedule.CreateA().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void AmericanScheduleB()
        {
            AmericanSchedule.CreateB().Verify(12, RecruitingFixup.AmericanId, "American  ", false);
            var hash = AmericanSchedule.CreateB().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void AmericanScheduleC()
        {
            AmericanSchedule.CreateC().Verify(12, RecruitingFixup.AmericanId, "American  ", false);
            var hash = AmericanSchedule.CreateC().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void AmericanScheduleD()
        {
            AmericanSchedule.CreateD().Verify(12, RecruitingFixup.AmericanId, "American  ", false);
            var hash = AmericanSchedule.CreateD().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void AmericanScheduleE()
        {
            AmericanSchedule.CreateE().Verify(12, RecruitingFixup.AmericanId, "American  ", false);
            var hash = AmericanSchedule.CreateE().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void Big10ScheduleA()
        {
            Big10Schedule.CreateA().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateA().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void Big10ScheduleE()
        {
            Big10Schedule.CreateE().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateE().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void Big10ScheduleB()
        {
            Big10Schedule.CreateB().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateB().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void Big10ScheduleC()
        {
            Big10Schedule.CreateC().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateC().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void Big10ScheduleD()
        {
            Big10Schedule.CreateD().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateD().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

#if false

        [TestMethod]
        public void Big10ScheduleG()
        {
            Big10Schedule.CreateG().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateG().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }


        [TestMethod]
        public void Big10ScheduleF()
        {
            Big10Schedule.CreateF().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateF().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }


        [TestMethod]
        public void Big10ScheduleI()
        {
            Big10Schedule.CreateI().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateI().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void Big10ScheduleH()
        {
            Big10Schedule.CreateH().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateH().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }


        [TestMethod]
        public void Big10ScheduleJ()
        {
            Big10Schedule.CreateJ().Verify(12, RecruitingFixup.Big10Id, "Big10 ", false);
            var hash = Big10Schedule.CreateJ().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }
#endif


        [TestMethod]
        public void TestPairing()
        {
            Big12Schedule.MakePairs(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 0, new System.Collections.Generic.List<Tuple<int, int>>());
            Trace.WriteLine(Big12Schedule.AllPairs.Count);
            foreach (var set in Big12Schedule.AllPairs)
            {
                Trace.WriteLine(string.Join(" , ", set.Select(p => string.Format("({0},{1})", p.Item1, p.Item2))));
            }
        }


        [TestMethod]
        [Ignore]
        public void LoadFromStream()
        {
            var file = File.ReadAllBytes("DYNASTY-Y182P");
            var guid = Guid.NewGuid();
            var form = new Form1();
            form.OpenDynastyFile(guid, file);
            PositionNumbers.Run(form);
            form.SaveFile();
            File.Copy(form.FilePath, @"d:\vso\test\test.mc02");
        }

        [TestMethod]
        public void SBCScheduleA()
        {
            SunBeltSchedule.CreateA().Verify(11, RecruitingFixup.SBCId, "SunBelt  ", false);
            var hash = SunBeltSchedule.CreateA().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void SBCScheduleB()
        {
            SunBeltSchedule.CreateB().Verify(11, RecruitingFixup.SBCId, "SunBelt  ", false);
            var hash = SunBeltSchedule.CreateB().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void SBCScheduleC()
        {
            SunBeltSchedule.CreateC().Verify(11, RecruitingFixup.SBCId, "SunBelt  ", false);
            var hash = SunBeltSchedule.CreateC().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void SBCScheduleD()
        {
            SunBeltSchedule.CreateD().Verify(11, RecruitingFixup.SBCId, "SunBelt  ", false);
            var hash = SunBeltSchedule.CreateD().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void CUSAScheduleA()
        {
            CUSASchedule.CreateA().Verify(12, RecruitingFixup.CUSAId, "CUSA  ", false);
            var hash = CUSASchedule.CreateA().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void CUSAScheduleB()
        {
            CUSASchedule.CreateB().Verify(12, RecruitingFixup.CUSAId, "CUSA  ", false);
            var hash = CUSASchedule.CreateB().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }


        /*
        [TestMethod]
        public void CUSAScheduleF()
        {
            CUSASchedule.CreateF().Verify(14, RecruitingFixup.CUSAId, "CUSA  ", false);
            var hash = CUSASchedule.CreateF().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }*/

        [TestMethod]
        public void MACScheduleA()
        {
            MACSchedule.CreateA().Verify(12, RecruitingFixup.MACId, "MAC  ", false);
            var hash = MACSchedule.CreateA().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void MACScheduleB()
        {
            MACSchedule.CreateB().Verify(12, RecruitingFixup.MACId, "MAC  ", false);
            var hash = MACSchedule.CreateB().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void MACScheduleC()
        {
            MACSchedule.CreateC().Verify(12, RecruitingFixup.MACId, "MAC  ", false);
            var hash = MACSchedule.CreateC().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void MACScheduleD()
        {
            MACSchedule.CreateD().Verify(12, RecruitingFixup.MACId, "MAC  ", false);
            var hash = MACSchedule.CreateD().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void MACScheduleE()
        {
            MACSchedule.CreateE().Verify(12, RecruitingFixup.MACId, "MAC  ", false);
            var hash = MACSchedule.CreateE().BuildHashSet();
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

    }
}