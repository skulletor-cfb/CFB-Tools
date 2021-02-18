using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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
        public static void Create(MaddenDatabase db)
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
                }
            }
            catch { }
        }

        [DataMember]
        public List<CoachMapping> CoachMapping { get; set; }

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
