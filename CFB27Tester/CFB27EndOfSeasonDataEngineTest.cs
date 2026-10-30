using CFB27.Data.Model;
using EA_DB_Editor;
using MaddenFranchise;

namespace CFB27Tester
{
    // the tests share one loaded save, and FranchiseFile reads tables lazily and isn't thread-safe
    [TestClass]
    [DoNotParallelize]
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
        public void TestReadConferenceChamps()
        {
            var champions = season.ReadConferenceChamps();

            // 2026 (dynasty year 0): the 10 conferences with title games; Independents have none
            Assert.AreEqual(10, champions.Count);
            Assert.IsTrue(champions.All(c => c.Year == 0));
            Assert.AreEqual(10, champions.Select(c => c.ConferenceId).Distinct().Count());
            Assert.IsFalse(champions.Any(c => c.ConferenceId == 5));

            Assert.AreEqual(CFBTeam.TeamRowToOldTeamIdMap[/* Texas A&M */ 113], champions.Single(c => c.ConferenceId == 11).TeamId);
            Assert.AreEqual(CFBTeam.TeamRowToOldTeamIdMap[/* Ohio State */ 87], champions.Single(c => c.ConferenceId == 1).TeamId);
            Assert.AreEqual(CFBTeam.TeamRowToOldTeamIdMap[/* South Alabama */ 104], champions.Single(c => c.ConferenceId == 13).TeamId);
        }

        [TestMethod]
        public void TestReadRecruits()
        {
            var recruits = season.ReadRecruits();

            // 4100 ranked recruits, each keyed by RecruitId and by 10000 + rank (ranks 1..4100, as the recruiting page expects)
            Assert.AreEqual(4100 * 2, recruits.Count);
            Assert.IsTrue(Enumerable.Range(10001, 4100).All(recruits.ContainsKey));

            var top = recruits[10001];
            Assert.AreEqual("Ruke Uwazurike", top.FullName);
            Assert.AreEqual("LE", top.Position);
            Assert.AreEqual(5, top.StarRating);
            Assert.AreEqual(77, top.RealOVR);
            Assert.AreEqual("Alpharetta, GA", top.Location);
            Assert.AreEqual(/* Georgia */ 9, top.State);
            Assert.AreEqual(CFBTeam.TeamIndexToOldIdMap[/* Texas A&M */ 91], top.Team1);

            // the 2,373 signees each have a school
            Assert.AreEqual(2373, recruits.Values.Distinct().Count(r => r.CommittedTeam != 1023));

            // the game's gem and bust flags
            Assert.AreEqual(304, recruits.Values.Distinct().Count(r => r.IsGem));
            Assert.AreEqual(196, recruits.Values.Distinct().Count(r => r.IsBust));

            // Texas is state 42 in both games (Mr. Texas Football page)
            Assert.AreEqual(563, recruits.Values.Distinct().Count(r => r.State == 42));
        }

        [TestMethod]
        public void TestReadSchoolRecords()
        {
            var schoolRecords = season.ReadSchoolRecords(false);

            // game, season and career records for 9 stats per team
            Assert.AreEqual(138, schoolRecords.Count);
            Assert.IsTrue(schoolRecords.Values.All(r => r.Count == 27));

            var isu = schoolRecords[/* Iowa State */ 38];
            var seasonRushing = isu.Single(r => r.Type == 1 && r.Description == (int)TeamRecordKeys.RushingYds);
            Assert.AreEqual("1996 T.Davis", seasonRushing.Holder);
            Assert.AreEqual(2185, seasonRushing.Value);
            Assert.AreEqual(63, seasonRushing.Year);

            var careerPassing = isu.Single(r => r.Type == 2 && r.Description == (int)TeamRecordKeys.PassYds);
            Assert.AreEqual("2021 B.Purdy", careerPassing.Holder);
            Assert.AreEqual(12170, careerPassing.Value);

            // set during the dynasty's first season (2026): dynasty year 0, shown in the league's years
            var dynastyRecord = schoolRecords[CFBTeam.TeamIndexToOldIdMap[/* Minnesota */ 52]].Single(r => r.Holder.EndsWith("A.Smith"));
            Assert.AreEqual(0, dynastyRecord.Year);
            Assert.AreEqual($"{Utility.StartingYear} A.Smith", dynastyRecord.Holder);
            Assert.AreEqual(9, dynastyRecord.Value);
        }

        [TestMethod]
        public void TestReadNcaaRecords()
        {
            var records = season.ReadNcaaRecords();

            // 6 game records (no classic codes for game TD records) + 9 season + 9 career
            Assert.AreEqual(24, records.Count);
            Assert.AreEqual(24, records.Select(r => r.Description).Distinct().Count());

            var seasonRushing = records.Single(r => r.Description == 21);
            Assert.AreEqual("1988 B.Sanders (Oklahoma State)", seasonRushing.Holder);
            Assert.AreEqual(2628, seasonRushing.Value);
            Assert.AreEqual(63, seasonRushing.Year);

            var careerPassing = records.Single(r => r.Description == 34);
            Assert.AreEqual("2011 C.Keenum (Houston)", careerPassing.Holder);
            Assert.AreEqual(19217, careerPassing.Value);

            var gamePassing = records.Single(r => r.Description == 9);
            Assert.AreEqual(734, gamePassing.Value);
        }

        [TestMethod]
        public void TestReadSchedule()
        {
            var schedule = season.ReadSchedule(false);

            // every game is filled in once the season is over: 896 regular season (incl. conference title games) + 43 bowls
            Assert.AreEqual(939, schedule.Count);
            Assert.AreEqual(43, schedule.Values.Count(g => g.PostSeason == 1));

            // bowls are at their bowl stadium and aren't flagged as neutral-site regular-season games
            var title = schedule.Values.Single(g => g.Week == 20);
            Assert.AreEqual(CFBTeam.TeamRowToOldTeamIdMap[/* Georgia */ 40], title.AwayTeamId);
            Assert.AreEqual(35, title.AwayScore);
            Assert.IsFalse(title.IsNeutralSite);
            Assert.IsTrue(schedule.Values.Any(g => g.PostSeason == 1 && g.GameSite == "Caesars Superdome"));

            // conference title games at neutral sites, e.g. the Big Ten at Lucas Oil Stadium
            Assert.IsTrue(schedule.Values.Any(g => g.Week == 15 && g.IsNeutralSite && g.GameSite == "Lucas Oil Stadium"));
        }

        [TestMethod]
        public void TestReadAwards()
        {
            var awards = season.ReadAwards();
            var rosters = new Dictionary<int, List<Player>>();
            var players = new Dictionary<int, Player>();
            season.CreatePlayers(rosters, players);

            // all 22 awards: the 17 classic ones plus 5 new in CFB27
            CollectionAssert.AreEquivalent(Enumerable.Range(0, 22).ToArray(), awards.Keys.ToArray());

            // ranked finalists, winner first, every one a rostered player the awards page can look up
            Assert.IsTrue(awards.Values.All(list => list.Select(a => a.Rank).SequenceEqual(Enumerable.Range(1, list.Count))));
            Assert.IsTrue(awards.Values.SelectMany(list => list).All(a => players.ContainsKey(a.PlayerId)));
            Assert.IsTrue(awards.Values.SelectMany(list => list).All(a => a.Year == 0));

            Assert.AreEqual("Trinidad Chambliss", players[awards[0].Single(a => a.Rank == 1).PlayerId].Name); // Heisman
            Assert.AreEqual("Trinidad Chambliss", players[awards[5].Single(a => a.Rank == 1).PlayerId].Name); // Davey O'Brien
            Assert.AreEqual("Keelon Russell", players[awards[20].Single(a => a.Rank == 1).PlayerId].Name);    // Shaun Alexander (freshman)
            Assert.AreEqual(12, awards[5].Count);
        }

        [TestMethod]
        public void TestCreateTeamHistoricRecords()
        {
            var records = season.CreateTeamHistoricRecords();

            // the 2026 season is over, so every FBS team has its dynasty year 0 record
            Assert.AreEqual(138, records.Count);
            Assert.IsTrue(records.Values.All(seasons => seasons.Keys.SequenceEqual(new[] { 0 })));

            var georgia = records[/* Georgia */ 30][0];
            Assert.AreEqual(14, georgia.Win);
            Assert.AreEqual(2, georgia.Loss);

            var iowaState = records[/* Iowa State */ 38][0];
            Assert.AreEqual(1, iowaState.Win);
            Assert.AreEqual(11, iowaState.Loss);
        }

        [TestMethod]
        public void TestReadDraftHistory()
        {
            var history = season.ReadDraftHistory();

            // the last three drafts in the save: the 2022-2024 classes (726 picks; the empty placeholder pick is skipped)
            Assert.AreEqual(726, history.Values.SelectMany(classes => classes).Sum(c => c.Round1 + c.Round2 + c.Round3 + c.RoundLater));
            // 32 + 31 + 32 first-rounders (the 2023 NFL draft had 31)
            Assert.AreEqual(95, history.Values.SelectMany(classes => classes).Sum(c => c.Round1));
            CollectionAssert.AreEquivalent(new[] { -4, -3, -2 }, history.Values.SelectMany(classes => classes).Select(c => c.DynastyYear).Distinct().ToArray());

            // Ohio State had 11 players drafted from its 2024 class, 4 in the first round
            var ohioState = history[CFBTeam.TeamRowToOldTeamIdMap[/* Ohio State */ 87]].Single(c => c.DynastyYear == -2);
            Assert.AreEqual(11, ohioState.Round1 + ohioState.Round2 + ohioState.Round3 + ohioState.RoundLater);
            Assert.AreEqual(4, ohioState.Round1);
        }

        [TestMethod]
        public void TestCreateAllAmericans()
        {
            var allAmericans = season.CreateAllAmericans();

            // the season is over: final 1st, 2nd and Freshman teams of 25, nationally (14) and for the 11 conferences
            Assert.AreEqual(12 * 3 * 25, allAmericans.Count);
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4, 5, 7, 9, 10, 11, 13, 14 }, allAmericans.Select(a => a.ConferenceId).Distinct().ToArray());
            Assert.IsTrue(allAmericans.GroupBy(a => new { a.ConferenceId, a.AllAmericanTeam }).All(g => g.Count() == 25));

            // 1 QB, 2 HB, 3 WR ... per team
            var national1st = allAmericans.Where(a => a.ConferenceId == 14 && a.AllAmericanTeam == AllAmericanTeam.First).ToList();
            Assert.AreEqual(1, national1st.Count(a => a.Position == 0));
            Assert.AreEqual(3, national1st.Count(a => a.Position == 3));

            var rosters = new Dictionary<int, List<Player>>();
            var players = new Dictionary<int, Player>();
            season.CreatePlayers(rosters, players);
            Assert.IsTrue(allAmericans.All(a => players.ContainsKey(a.PlayerId)));
            Assert.AreEqual("Trinidad Chambliss", players[national1st.Single(a => a.Position == 0).PlayerId].Name);
        }

        [TestMethod]
        public void TestReadDepthCharts()
        {
            var depthCharts = season.ReadDepthCharts();
            var rosters = new Dictionary<int, List<Player>>();
            var players = new Dictionary<int, Player>();
            season.CreatePlayers(rosters, players);

            Assert.AreEqual(138, depthCharts.Count);

            // every depth chart entry is a player on that team's roster, ordered from the starter down
            foreach (var team in depthCharts)
            {
                foreach (var position in team.Value.Values)
                {
                    Assert.IsTrue(position.All(p => players.ContainsKey(p.PlayerId) && players[p.PlayerId].TeamId == team.Key));
                    Assert.IsTrue(position.Select(p => p.PositionDepth).SequenceEqual(Enumerable.Range(0, position.Length)));
                }
            }

            var iowaState = depthCharts[/* Iowa State */ 38];
            Assert.AreEqual("Jaylen Raynor", players[iowaState[0][0].PlayerId].Name);   // QB1
            Assert.AreEqual("Kyle Konrardy", players[iowaState[19][0].PlayerId].Name);  // K
            Assert.AreEqual("Easton Miller", players[iowaState[21][0].PlayerId].Name);  // KR
            Assert.AreEqual("Jordyn Bailey", players[iowaState[22][0].PlayerId].Name);  // PR
            Assert.AreEqual(6, iowaState[3].Length);                                    // WR
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

            // every neutral site in the list exists in the game's stadium table, except Aloha Stadium (Team Builder only)
            foreach (var neutral in CFBStadium.NeutralSites.Where(n => n.Value != "Aloha Stadium"))
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