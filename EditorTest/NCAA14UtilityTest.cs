using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCAA14Utility.Scheduling;
using System.Linq;

namespace EditorTest
{
    [TestClass]
    public class NCAA14UtilityTest
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
        }

        [TestMethod]
        public void AmericanScheduleA()
        {
            var dict = AmericanSchedule.CreateA();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(14, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }

        [TestMethod]
        public void AmericanScheduleB()
        {
            var dict = AmericanSchedule.CreateB();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(14, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }


        [TestMethod]
        public void MWCScheduleA()
        {
            var dict = MWCSchedule.CreateA();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(16, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void MWCScheduleB()
        {
            var dict = MWCSchedule.CreateB();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(16, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Pac12ScheduleA()
        {
            var dict = Pac12Schedule.CreateA();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Pac12ScheduleB()
        {
            var dict = Pac12Schedule.CreateB();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }
        [TestMethod]
        public void Pac12ScheduleC()
        {
            var dict = Pac12Schedule.CreateC();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Pac12ScheduleD()
        {
            var dict = Pac12Schedule.CreateD();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Big10ScheduleA()
        {
            var dict = Big10Schedule.CreateA();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Big10ScheduleB()
        {
            var dict = Big10Schedule.CreateB();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }
        [TestMethod]
        public void Big10ScheduleC()
        {
            var dict = Big10Schedule.CreateC();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Big10ScheduleD()
        {
            var dict = Big10Schedule.CreateD();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void Big10ScheduleE()
        {
            var dict = Big10Schedule.CreateE();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(12, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void CUSAScheduleA()
        {
            var dict = CUSASchedule.CreateA();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(10, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 9));
        }

        [TestMethod]
        public void MACScheduleX()
        {
            var dict = MACSchedule.CreateX();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(13, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }
        [TestMethod]
        public void MACScheduleY()
        {
            var dict = MACSchedule.CreateY();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(13, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }
        [TestMethod]
        public void MACScheduleZ()
        {
            var dict = MACSchedule.CreateZ();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(13, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }
        [TestMethod]
        public void MACScheduleP()
        {
            var dict = MACSchedule.CreateP();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(13, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }
        [TestMethod]
        public void MACScheduleQ()
        {
            var dict = MACSchedule.CreateQ();
            var hash = dict.BuildHashSet();
            Assert.AreEqual(13, dict.Keys.Count);
            Assert.IsTrue(hash.All(kvp => kvp.Value.Count == 8));
        }
    }
}