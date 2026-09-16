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
            engine = new CFB27DataEngine(@"D:\CFB27\export\DYNASTY-Q5");
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

        [TestMethod]
        public void CreateTeamSchedule()
        {
            var result = engine.CreateTeamSchedule(false);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ReadConferenceMetadata()
        {
            var result = engine.ReadConferenceMetadata();
            Assert.IsNotEmpty(result);
            Assert.IsTrue(result.Values.Any(c => c.Name == "SEC"));
            Assert.IsTrue(result.Values.Any(c => c.Name == "ACC"));
        }

        [TestMethod]
        public void CreatePlayers()
        {
            var rosters = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<Player>>();
            var players = new System.Collections.Generic.Dictionary<int, Player>();
            engine.CreatePlayers(rosters, players);

            Assert.IsNotEmpty(players);
            Assert.IsTrue(players.Values.All(p => !string.IsNullOrEmpty(p.FirstName)));
            Assert.IsTrue(players.Values.All(p => p.Position >= 0));

            var knownTeamIds = engine.Teams.Records.Where(t => t.HasClassicTeamId).Select(t => t.TeamId).ToHashSet();
            Assert.IsTrue(rosters.Keys.All(knownTeamIds.Contains));
            Assert.IsTrue(rosters.Values.All(r => r.Count > 0));
        }

        [TestMethod]
        public void CalculateRosterSpots()
        {
            var ranking = new RecruitClassRanking { TeamId = engine.Teams.Records[0].TeamId };
            var spots = engine.CalculateRosterSpots(ranking);
            Assert.IsTrue(spots <= 70);
        }
    }
}