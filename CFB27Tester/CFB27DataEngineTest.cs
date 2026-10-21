using CFB27.Data.Model;
using EA_DB_Editor;
using MaddenFranchise;
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
            var dynasty = FranchiseFile.Load(@"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\DYNASTY-Y587P-AUTOSAVE");
            engine = new CFB27DataEngine(dynasty);
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
            Assert.HasCount(143, CFBTeam.TeamRowToOldTeamIdMap);
        }

        [TestMethod]
        public void EveryTeamHasHistoricalData()
        {
            Assert.IsGreaterThan(0, engine.Teams.Records.Count());
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
            Assert.IsFalse(engine.IsSeasonOver());
        }

        [TestMethod]
        public void ReadMediaCoverage()
        {
            var result = engine.ReadMediaCoverage();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateTeamSchedule()
        {
            var result = engine.CreateTeamSchedule(false);
            Assert.IsNotNull(result);
        }
    }
}