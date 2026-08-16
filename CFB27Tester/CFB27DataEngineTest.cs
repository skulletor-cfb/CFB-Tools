using EA_DB_Editor;

namespace CFB27Tester
{
    [TestClass]
    public sealed class CFB27DataEngineTest
    {
        private CFB27DataEngine engine;
        [TestInitialize]
        public void Init()
        {
            engine = new CFB27DataEngine(@"d:\\cfb27");
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
            Assert.AreEqual(CFB27Team.TeamIdToOldIdMap.Count, 143);
        }
    }
}