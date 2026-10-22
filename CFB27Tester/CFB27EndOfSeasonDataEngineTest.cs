using CFB27.Data.Model;
using EA_DB_Editor;
using MaddenFranchise;
using System.Linq;
using EA_DB_Editor;
using MaddenFranchise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFB27Tester
{
    [TestClass]
    public class CFB27EndOfSeasonDataEngineTest
    {
        private static CFB27DataEngine season;

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            var dynasty = FranchiseFile.Load(@"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\DYNASTY-Y587P-AUTOSAVE");
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
    }
}