using CFB27.Data.Model;
using EA_DB_Editor;
using MaddenFranchise;

namespace CFB27Tester
{
    // the tests share one loaded save, and FranchiseFile reads tables lazily and isn't thread-safe
    [TestClass]
    [DoNotParallelize]
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

        [TestMethod]
        public void ReadSchedule()
        {
            var schedule = season.ReadSchedule(true);

            // week 0: 939 SeasonGame rows less 43 unscheduled bowls and 10 conference title games without teams yet
            Assert.AreEqual(886, schedule.Count);
            Assert.IsTrue(schedule.Values.All(g => g.PostSeason == 0));

            int iowaState = 38, texas = CFBTeam.TeamIndexToOldIdMap[90], oklahoma = CFBTeam.TeamIndexToOldIdMap[69];

            // home games are at the home team's stadium
            var isuHome = schedule.Values.First(g => g.HomeTeamId == iowaState);
            Assert.AreEqual("Jack Trice Stadium", isuHome.GameSite);
            Assert.IsFalse(isuHome.IsNeutralSite);

            // Oklahoma-Texas is at the Cotton Bowl
            var redRiver = schedule.Values.Single(g => (g.HomeTeamId == texas && g.AwayTeamId == oklahoma) || (g.HomeTeamId == oklahoma && g.AwayTeamId == texas));
            Assert.IsTrue(redRiver.IsNeutralSite);
            Assert.AreEqual("Cotton Bowl", redRiver.GameSite);
            Assert.AreEqual(redRiver.StadiumId, redRiver.SiteId);

            // 0 Monday ... 6 Sunday; nearly every game is on Saturday
            Assert.IsTrue(schedule.Values.All(g => g.GameDay >= 0 && g.GameDay <= 6));
            Assert.IsTrue(schedule.Values.Count(g => g.GameDay == 5) > 700);
        }

        [TestMethod]
        public void CreateTeamHistoricRecords()
        {
            // week 0 of the first dynasty season: only the 2026 season in progress, at 0-0 (NCAA14DataEngine includes it too)
            var records = season.CreateTeamHistoricRecords();
            Assert.AreEqual(138, records.Count);
            Assert.IsTrue(records.Values.All(seasons => seasons.Count == 1 && seasons.Values.All(r => r.Win == 0 && r.Loss == 0)));
        }

        [TestMethod]
        public void CreateAllAmericans()
        {
            // before the season ends only the preseason 1st and 2nd teams exist
            var allAmericans = season.CreateAllAmericans();
            Assert.AreEqual(12 * 2 * 25, allAmericans.Count);
            Assert.IsFalse(allAmericans.Any(a => a.AllAmericanTeam == AllAmericanTeam.Freshman));
        }

        [TestMethod]
        public void ReadTeamsHasPreseasonPolls()
        {
            // the values the Team(CFBTeam) constructor copies into MediaPollRank / CoachesPollRank / *PollPoints
            var ohioState = season.Teams.Records.Single(t => t.DisplayName == "Ohio State");
            Assert.AreEqual(1, ohioState.MediaPoll_CurrentRank);
            Assert.AreEqual(1, ohioState.CoachesPoll_CurrentRank);
            Assert.AreEqual(1569, ohioState.MediaPoll_CurrentPoints);
        }

        [TestMethod]
        public void MediaCoverageIsKeyedByTeamId()
        {
            // MediaCoverage.Create attaches coverage by Team.Id (the old team id), so keys must be team ids, not Team rows;
            // CFB27 only writes a few stories, so most teams have none (the pages handle that)
            var coverage = season.ReadMediaCoverage();
            var teamIds = new HashSet<int>(season.Teams.Records.Where(t => !t.IsEmpty).Select(t => t.TeamId));
            Assert.IsTrue(coverage.Keys.All(teamIds.Contains));
            Assert.IsTrue(coverage.Values.All(stories => stories.Length > 0));
            Assert.IsTrue(coverage.All(kvp => kvp.Value.All(story => story.TeamId == kvp.Key)));
        }
    }
}