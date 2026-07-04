using CFB27.FileReader;
namespace CFB27.FileReader.Test
{
    [TestClass]
    public sealed class FileTest
    {
        public const string TestDynastySaveFilePath = @"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\DYNASTY-DEION";
        public const string TestRosterSaveFilePath = @"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\ROSTER-Official";

        [TestMethod]
        public void DynastyFileLoad()
        {
            var file = new DynastyFile(TestDynastySaveFilePath);
            Assert.IsNotNull(file.FileDetails);
        }

        [TestMethod]
        public void RosterFileLoad()
        {
            var file = new DynastyFile(TestRosterSaveFilePath);
            Assert.IsNotNull(file.FileDetails);
        }
    }
}
