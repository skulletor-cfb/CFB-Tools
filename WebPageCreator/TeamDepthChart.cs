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
        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            Team.Create(db,isPreseason);
            PlayerDB.Create(db);

            if (MediaReports != null)
                return;

            var mediaTable = MaddenTable.FindMaddenTable(db.lTables, "MCOV");
            MediaReports = mediaTable.lRecords
                .GroupBy(mr => mr["TGID"].ToInt32())
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Where(mr => mr["SGNM"].ToInt32() == 127)  // we don't want week 1 game info
                        .Select(mr =>
                        new MediaCoverage
                        {
                            TeamId = mr["TGID"].ToInt32(),
                            GameNumber = mr["SGNM"].ToInt32(),
                            Week = mr["SEWN"].ToInt32(),
                            PlayerId = mr["PGID"].ToInt32(),
                            Headline = Transform(mr["MHTX"]),
                            Content = Transform(mr["MCTX"])
                        }
                        )
                        .ToArray()
                );

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
        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            Team.Create(db,isPreseason);
            PlayerDB.Create(db);

            if (TeamDepthCharts != null)
                return;

            var depthChartTable = MaddenTable.FindMaddenTable(db.lTables, "DCHT");
            TeamDepthCharts = depthChartTable.lRecords.Where(mr => mr["TGID"].ToInt32().IsValidTeam()).GroupBy(mr => mr["TGID"].ToInt32()).ToDictionary(
                group => group.Key,
                group => group.Select(g =>
                    new DepthChartPosition
                    {
                        PlayerId = g["PGID"].ToInt32(),
                        PlayerPosition = g["PPOS"].ToInt32(),
                        PositionDepth = g["ddep"].ToInt32()
                    }).ToArray()
                .GroupBy(dpp => dpp.PlayerPosition)
                .ToDictionary(g => g.Key, g => g.OrderBy(pos => pos.PositionDepth).ToArray()));
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
