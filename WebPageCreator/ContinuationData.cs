using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace EA_DB_Editor
{
    [DataContract]
    public class ContinuationData
    {
        public static bool UsingContinuationData { get; set; }
        private Dictionary<int, int> newToOldCoachMap;
        private Dictionary<int, int> oldToNewCoachMap;

        public static int ContinuationYear
        {
            get
            {
                return Instance == null ? 0 : Instance.Year;
            }
        }

        public int Year { get; set; }
        public Dictionary<int,Team> TeamData { get; set; }
        public static string ContinuationDirectory;
        public static int? ContinuationYearConfig = null;
        public static ContinuationData Instance { get; private set; }
        public static string ContinuationFile = Path.Combine(@".\archive", "continuationfile.txt");
        public NcaaRecord NcaaRecords { get; set; }
        public List<BowlChampion> BowlChamps { get; set; }
        public List<ConferenceChampion> ConferenceChamps { get; set; }
        public Dictionary<int, Dictionary<int, TeamSeasonRecord>> TeamHistoricRecords { get; set; }
        public static void Create(IDataEngine engine)
        {
            if (UsingContinuationData == false || Instance != null)
                return;

            try
            {
                if (Directory.Exists(ConfigurationManager.AppSettings["ContinuationDirectory"]) &&
                    File.Exists(ContinuationFile))
                {

                    if (ContinuationYearConfig.HasValue == false)
                    {
                        ContinuationYearConfig = ConfigurationManager.AppSettings["ContinuationYear"].ToInt32();
                    }

                    if (ContinuationDirectory == null)
                    {
                        ContinuationDirectory = ConfigurationManager.AppSettings["ContinuationDirectory"];
                    }

                    Instance = ContinuationFile.FromJsonFile<ContinuationData>(true);
                    Instance.Year = ContinuationYearConfig.Value;
                    Instance.TeamData = Team.FromJsonFile(Path.Combine(ContinuationDirectory, "team")).ToDictionary(team => team.Id);
                    Instance.NcaaRecords = Path.Combine(ContinuationDirectory, "ncaarecords").FromJsonFile<NcaaRecord>();
                    Instance.BowlChamps = BowlChampion.FromCsvFile(Path.Combine(ContinuationDirectory, "bowlchamps.csv"));
                    Instance.ConferenceChamps = ConferenceChampion.FromCsvFile(Path.Combine(ContinuationDirectory, "cc.csv"));
                    Instance.TeamHistoricRecords = HistoricTeamRecord.FromCsvFile(Path.Combine(ContinuationDirectory, "thr.csv"));
                    MergeRetiredTeams();
                }
            }
            catch { }
        }

        /// <summary>
        /// NCAA 14 teams whose slot was later given to another school (FIU, UMass, New Mexico State, UConn), by the season folder
        /// they last played in. Their history stops after that season, but CFB 27 has them again.
        /// </summary>
        private static readonly Dictionary<int, string> RetiredTeamLastSeasons = new Dictionary<int, string>
        {
            { 230, "2487_Season" }, // FIU
            { 181, "2197_Season" }, // UMass
            { 61, "2371_Season" },  // New Mexico State
            { 100, "2371_Season" }, // UConn
        };

        /// <summary>
        /// Brings a retired team's all-time totals, season records, bowl wins and conference titles back from its last season
        /// when the continuation season doesn't have them (it has no entry, or a placeholder with no games). Once a season has
        /// been built with them, later seasons carry them forward and this does nothing.
        /// </summary>
        private static void MergeRetiredTeams()
        {
            var archive = Path.GetDirectoryName(Path.GetFullPath(ContinuationDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            foreach (var retired in RetiredTeamLastSeasons)
            {
                int id = retired.Key;
                if (Instance.TeamData.TryGetValue(id, out var current) && current.AllTimeWin + current.AllTimeLoss + current.AllTimeTie > 0)
                {
                    continue;
                }

                var lastSeason = Path.Combine(archive, retired.Value);
                if (!Directory.Exists(lastSeason))
                {
                    continue;
                }

                var team = Team.FromJsonFile(Path.Combine(lastSeason, "team")).FirstOrDefault(t => t.Id == id);
                if (team != null)
                {
                    Instance.TeamData[id] = team;
                }

                if (HistoricTeamRecord.FromCsvFile(Path.Combine(lastSeason, "thr.csv")).TryGetValue(id, out var records) && !Instance.TeamHistoricRecords.ContainsKey(id))
                {
                    Instance.TeamHistoricRecords[id] = records;
                }

                Instance.BowlChamps.AddRange(BowlChampion.FromCsvFile(Path.Combine(lastSeason, "bowlchamps.csv"))
                    .Where(b => b.TeamId == id && !Instance.BowlChamps.Any(x => x.Year == b.Year && x.BowlId == b.BowlId)));
                Instance.ConferenceChamps.AddRange(ConferenceChampion.FromCsvFile(Path.Combine(lastSeason, "cc.csv"))
                    .Where(c => c.TeamId == id && !Instance.ConferenceChamps.Any(x => x.Year == c.Year && x.TeamId == c.TeamId)));
            }
        }

        [DataMember]
        public List<CoachMapping> CoachMapping { get; set; }

        /// <summary>NCAA 14's All-American, Heisman, top 25 class and coach of the year counts for each carried-over coach.</summary>
        [DataMember]
        public List<CoachAwardCount> CoachAwards { get; set; }

        public ContinuationData()
        {
            this.CoachMapping = new List<CoachMapping>();
        }

        public Dictionary<int, int> NewToOldCoachMap
        {
            get
            {
                if (newToOldCoachMap == null)
                {
                    newToOldCoachMap = this.CoachMapping.ToDictionary(cm => cm.NewCoachId, cm => cm.OldCoachId);
                }

                return newToOldCoachMap;
            }
        }

        public Dictionary<int, int> OldToNewCoachMap
        {
            get
            {
                if (oldToNewCoachMap == null)
                {
                    oldToNewCoachMap = this.CoachMapping.ToDictionary(cm => cm.OldCoachId, cm => cm.NewCoachId);
                }

                return oldToNewCoachMap;
            }
        }
    }

    public class CoachMapping
    {
        public int OldCoachId { get; set; }
        public int NewCoachId { get; set; }
    }

}
