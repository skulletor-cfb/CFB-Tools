using CFB27.Data.Model;
using EA_DB_Editor;
using MaddenFranchise;

namespace CFB27Tester
{
    [TestClass]
    public class CFB27EndOfSeasonDataEngineTest
    {
        private static CFB27DataEngine season;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var dynasty = FranchiseFile.Load(@"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\DYNASTY-Y587P-Q5");
            season = new CFB27DataEngine(dynasty);
        }

        [TestMethod]
        public void TestSeasonOver()
        {
            Assert.IsTrue(season.IsSeasonOver());
        }

        [TestMethod]
        public void TestBowlChampions()
        {
            var champs = new Dictionary<string, BowlChampion>();
            season.ReadBowlChampions(champs);
            Assert.AreEqual(43, champs.Count);
            Assert.IsTrue(champs.Values.Any(bc => bc.BowlId == BowlChampion.NationalChampionshipGameId));
            Assert.IsFalse(champs.Values.Any(bc => bc.BowlId == 0 && bc.TeamId != CFBTeam.TeamRowToOldTeamIdMap[/* Arkansas State row */ 7]));
        }

        [TestMethod]
        public void TestCreatePlayers()
        {
            var rosters = new Dictionary<int, List<Player>>();
            var players = new Dictionary<int, Player>();
            season.CreatePlayers(rosters, players);

            // 138 FBS teams with 85-man rosters
            Assert.AreEqual(138, rosters.Count);
            Assert.IsTrue(rosters.Values.All(r => r.Count == 85));
            Assert.AreEqual(138 * 85, players.Count);

            var allah = rosters[/* Iowa State */ 38].Single(p => p.FirstName == "Salahadin" && p.LastName == "Allah");
            Assert.AreEqual("HB", allah.PositionName);
            Assert.AreEqual(2, allah.Year); // Junior
            Assert.AreEqual(7, allah.Number);
            Assert.AreEqual(72, allah.Height);
            Assert.AreEqual(225, allah.Weight);
            Assert.AreEqual(79, allah.Ovr);
            Assert.AreEqual(85, allah.Spd);
        }

        [TestMethod]
        public void TestReadCoaches()
        {
            var coaches = season.ReadCoaches();

            // HC, OC and DC for each of the 138 FBS teams
            Assert.AreEqual(138 * 3, coaches.Count);
            Assert.IsTrue(coaches.Values.GroupBy(c => c.TeamId).All(g => g.Select(c => c.Position).OrderBy(p => p).SequenceEqual(new[] { 0, 1, 2 })));

            var smart = coaches.Values.Single(c => c.FirstName == "Kirby" && c.LastName == "Smart");
            Assert.AreEqual(/* Georgia */ 30, smart.TeamId);
            Assert.AreEqual(0, smart.Position);
            Assert.AreEqual("131-23", smart.CareerRecord);
            Assert.AreEqual(13, smart.CoachBowlWin);
            Assert.AreEqual(3, smart.CareerNationalChampionships);
            Assert.AreEqual(11, smart.YearsWithTeam);
            Assert.AreEqual(/* Georgia */ 30, smart.AlmaMaterId);

            var deion = coaches.Values.Single(c => c.FirstName == "Deion" && c.LastName == "Sanders");
            Assert.AreEqual(/* Florida State */ 28, deion.AlmaMaterId);
            Assert.IsTrue(coaches.Values.All(c => c.AlmaMaterId != 0));

            var isuHeadCoach = coaches.Values.Single(c => c.TeamId == /* Iowa State */ 38 && c.Position == 0);
            Assert.AreEqual("Jimmy Rogers", isuHeadCoach.Name);
            Assert.AreEqual("1-11", isuHeadCoach.TeamRecord);
        }

        [TestMethod]
        public void TestReadRecruitClasses()
        {
            var rankings = season.ReadRecruitClasses();

            Assert.AreEqual(138, rankings.Count);
            Assert.AreEqual(2373, rankings.Values.Sum(r => r.Star1 + r.Star2 + r.Star3 + r.Star4 + r.Star5));
            Assert.IsTrue(rankings.Values.All(r => r.ConferenceId >= 0));

            // the game's #1 class: 6 five-stars, 16 four-stars, 3 three-stars = 6*18 + 16*12 + 3*4
            var top = rankings.Values.OrderByDescending(r => r.Points).First();
            Assert.AreEqual(/* Georgia */ 30, top.TeamId);
            Assert.AreEqual(6, top.Star5);
            Assert.AreEqual(16, top.Star4);
            Assert.AreEqual(3, top.Star3);
            Assert.AreEqual(312, top.Points);

            // Iowa State signed nobody
            var isu = rankings[/* Iowa State */ 38];
            Assert.AreEqual(0, isu.Star1 + isu.Star2 + isu.Star3 + isu.Star4 + isu.Star5);
            Assert.AreEqual(0, isu.Points);
        }

        [TestMethod]
        public void TestReadConferenceMetadata()
        {
            var conferences = season.ReadConferenceMetadata();
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4, 5, 7, 9, 10, 11, 13 }, conferences.Keys.ToArray());
            // classic names, not the save's (CUSA, MW, Independent)
            Assert.AreEqual("SEC", conferences[11].Name);
            Assert.AreEqual("C-USA", conferences[4].Name);
            Assert.AreEqual("Independents", conferences[5].Name);
            Assert.AreEqual("MWC", conferences[9].Name);
            Assert.AreEqual("Sun Belt (West)", conferences[13].FindDivision(16).Name);

            // only the Sun Belt has divisions
            Assert.IsTrue(conferences.Values.Where(c => c.Id != 13).All(c => c.Divisions.Count == 0));
            var sunBelt = conferences[13];
            Assert.AreEqual("West", sunBelt.FindDivision(16).SubName);
            Assert.AreEqual("East", sunBelt.FindDivision(17).SubName);

            // team membership uses the same classic ids
            var classes = season.ReadRecruitClasses();
            Assert.AreEqual(2, classes[/* Iowa State */ 38].ConferenceId);
            Assert.AreEqual(CFBConference.NoDivisionId, classes[/* Iowa State */ 38].DivisionId);
            Assert.AreEqual(17, classes[/* App State */ 901].DivisionId);
            Assert.AreEqual(16, classes[/* Arkansas State */ 7].DivisionId);
            Assert.AreEqual(7, classes.Values.Count(c => c.DivisionId == 16));
            Assert.AreEqual(7, classes.Values.Count(c => c.DivisionId == 17));

            // bowl tie-ins are classic ids too
            Assert.IsTrue(season.Bowls.Records.All(b => b.ConferenceId1 == -1 || conferences.ContainsKey(b.ConferenceId1)));
            Assert.IsTrue(season.Bowls.Records.All(b => b.ConferenceId2 == -1 || conferences.ContainsKey(b.ConferenceId2)));
        }

        [TestMethod]
        public void TestReadStadiums()
        {
            var stadiums = season.ReadStadiums();

            // every FBS team's home stadium resolves to the game's stadium table
            var teams = season.Teams.Records.Where(t => !t.IsEmpty && t.TeamIndex != CFBPlayer.NoTeamIndex).ToList();
            Assert.AreEqual(138, teams.Count);
            foreach (var team in teams)
            {
                var stadium = stadiums[CFBStadium.ToStadiumId(team.Stadium)];
                Assert.IsFalse(string.IsNullOrEmpty(stadium.Name), team.DisplayName);
                Assert.IsTrue(stadium.Capacity > 0, team.DisplayName);
            }

            var isu = teams.Single(t => t.DisplayName == "Iowa State");
            Assert.AreEqual("Jack Trice Stadium", stadiums[CFBStadium.ToStadiumId(isu.Stadium)].Name);
            Assert.AreEqual(61500, stadiums[CFBStadium.ToStadiumId(isu.Stadium)].Capacity);

            // neutral sites: the Sugar Bowl is at the Superdome, the Citrus Bowl at Camping World Stadium
            Assert.AreEqual("Caesars Superdome", season.ReadStadiumName(unchecked((int)0x806385CF)));
            Assert.AreEqual("Camping World Stadium", season.ReadStadiumName(unchecked((int)0x806A4715)));
            Assert.AreEqual("M&T Bank Stadium", season.ReadStadiumName(unchecked((int)0x8063B3C6)));
            Assert.AreEqual("Gillette Stadium", season.ReadStadiumName(unchecked((int)0x80B7A946)));
            Assert.AreEqual("Lambeau Field", season.ReadStadiumName(unchecked((int)0x80C7E4D7)));

            // every neutral site in the list exists in the game's stadium table
            foreach (var neutral in CFBStadium.NeutralSites)
            {
                Assert.IsNotNull(CFBStadium.FindByName(neutral.Value), neutral.Value);
            }

            // every bowl with a fixed site resolves
            foreach (var bowl in season.Bowls.Records.Where(b => !b.IsEmpty && !string.Equals(b.Stadium, BaseRecord.NoRefString)))
            {
                Assert.AreNotEqual(string.Empty, season.ReadStadiumName(CFBStadium.ToStadiumId(bowl.Stadium)), bowl.Name);
            }
        }
    }
}