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
    }
}
