using CFB27.Data.Model;
using EA_DB_Editor;
using MaddenFranchise;
using System.Linq;

namespace CFB27Tester
{
    [TestClass]
    public sealed class CFB27PreseasonDataEngineTest
    {
        private static CFB27DataEngine season;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var dynasty = FranchiseFile.Load(@"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\DYNASTY-Y587");
            season = new CFB27DataEngine(dynasty);
        }


        [TestMethod]
        public void CreateTeamMap()
        {
            Assert.IsNotNull(season.TeamNames);
        }

        [TestMethod]
        public void CreateReverseTeamMap()
        {
            var reverse = season.TeamNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
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
            Assert.IsGreaterThan(0, season.Teams.Records.Count());
            Assert.IsTrue(season.Teams.Records.All(t => t.HistoricalData != null));
         }

        [TestMethod]
        public void CreateBowls()
        {
            var dict = season.CreateBowlTable();
            Assert.IsNotNull(dict);
        }

        [TestMethod]
        public void TestSeasonOver()
        {
            Assert.IsFalse(season.IsSeasonOver());
        }

        [TestMethod]
        public void ReadMediaCoverage()
        {
            var result = season.ReadMediaCoverage();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateTeamSchedule()
        {
            var result = season.CreateTeamSchedule(false);
            Assert.IsNotNull(result);
        }
    }
}