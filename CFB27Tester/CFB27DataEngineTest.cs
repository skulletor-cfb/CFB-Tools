using CFB27.Data.Model;
using EA_DB_Editor;
using System.Linq;

namespace CFB27Tester
{
    [TestClass]
    public sealed class CFB27DataEngineTest
    {
        private static CFB27DataEngine engine;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            engine = new CFB27DataEngine(@"D:\CFB27\export\DYNASTY-SDBAK");
        }


        [TestMethod]
        public void CreateTeamMap()
        {
            Assert.IsNotNull(engine.TeamNames);
        }

        [TestMethod]
        public void CreateReverseTeamMap()
        {
            var reverse = engine.TeamNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            Assert.IsNotNull(reverse.WriteJson());
        }

        [TestMethod]
        public void VerifyMap()
        {
            Assert.HasCount(143, CFBTeam.TeamIdToOldIdMap);
        }

        [TestMethod]
        public void EveryTeamHasHistoricalData()
        {
            Assert.IsTrue(engine.Teams.Records.All(t => t.HistoricalData != null));
         }

        [TestMethod]
        public void CreateBowls()
        {
            var dict = engine.CreateBowlTable();
            Assert.IsNotNull(dict);
        }

        [TestMethod]
        public void TestSeasonOver()
        {
            Assert.IsTrue(engine.IsSeasonOver());
        }

        [TestMethod]
        public void ReadMediaCoverage()
        {
            var result = engine.ReadMediaCoverage();
            Assert.IsNotNull(result);
        }
    }
}