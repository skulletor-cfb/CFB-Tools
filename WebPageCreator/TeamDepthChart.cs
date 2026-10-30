using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    [DataContract]
    public class MediaCoverage
    {
        public static Dictionary<int, MediaCoverage[]> MediaReports { get; set; }
        public static void Create(IDataEngine dataEngine, bool isPreseason)
        {
            Team.Create(dataEngine, isPreseason);
            PlayerDB.Create(dataEngine);

            if (MediaReports != null)
                return;

            MediaReports = dataEngine.ReadMediaCoverage();

            foreach (var team in Team.Teams.Values)
            {
                MediaCoverage[] reports = null;
                if (MediaReports.TryGetValue(team.Id, out reports))
                {
                    team.MediaCoverage = reports;
                }
            }
        }

        public static string Transform(string value)
        {
            string result = value;
            // fix the year of the holder
            if (ContinuationData.UsingContinuationData)
            {
                var dynastyYear = BowlChampion.DynastyFileYear + 2013;
                var realYear = BowlChampion.CurrentYear + Utility.StartingYear;
                result = result.Replace(dynastyYear.ToString(), realYear.ToString());
            }

            return result;
        }

        [DataMember]
        public int TeamId { get; set; }

        [DataMember]
        public int GameNumber { get; set; }

        [DataMember]
        public int Week { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string Headline { get; set; }

        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public string PlayerName
        {
            get
            {
                return this.Player == null ? string.Empty : this.Player.Name;
            }
            set { }
        }
        public Player Player { get { return PlayerDB.Players.ContainsKey(this.PlayerId) ? PlayerDB.Players[this.PlayerId] : null; } }
        public Team Team { get { return Team.Teams[this.TeamId]; } }
    }

    public class TeamDepthChart
    {
        public static Dictionary<int, Dictionary<int, DepthChartPosition[]>> TeamDepthCharts { get; private set; }
        public static void Create(IDataEngine dataEngine, bool isPreseason)
        {
            Team.Create(dataEngine, isPreseason);
            PlayerDB.Create(dataEngine);

            if (TeamDepthCharts != null)
                return;

            TeamDepthCharts = dataEngine.ReadDepthCharts();
        }
    }

   public class DepthChartPosition
   {
       public Player Player { get { return PlayerDB.Players[this.PlayerId]; } }
       public int PlayerId { get; set; }
       public int PlayerPosition { get; set; }
       public int PositionDepth { get; set; }
   }
}
