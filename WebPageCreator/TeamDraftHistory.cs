using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    public class TeamDraftHistory
    {
        public static Dictionary<int, DraftClass[]> DraftHistory;
        public static void Create(MaddenDatabase db)
        {
            // Team Draft History
            var draftHistoryTable = MaddenTable.FindTable(db.lTables, "TPHS");
            DraftHistory = draftHistoryTable.lRecords.GroupBy(
                mr => mr["TGID"].ToInt32(),
                mr => new DraftClass
                {
                    DynastyYear = mr["dryr"].ToInt32(),
                    Round1 = mr["PDR1"].ToInt32(),
                    Round2 = mr["PDR2"].ToInt32(),
                    Round3 = mr["PDR3"].ToInt32(),
                    RoundLater = mr["PDRL"].ToInt32(),
                }).ToDictionary(g => g.Key, g => g.ToArray());
        }

        public static DraftClass Rollup(int teamId)
        {
            if (DraftHistory.ContainsKey(teamId))
            {
                var values = DraftHistory[teamId];
                return new DraftClass
                {
                    Round1 = values.Sum(dc =>dc.Round1),
                    Round2 = values.Sum(dc => dc.Round2),
                    Round3 = values.Sum(dc => dc.Round3),
                    RoundLater = values.Sum(dc => dc.RoundLater),
                };
            }

            return null; 
        }
    }

    [DataContract]
    public class DraftClass
    {
        [DataMember(EmitDefaultValue=false)]
        public int DynastyYear { get; set; }
        [DataMember]
        public int Round1 { get; set; }
        [DataMember]
        public int Round2 { get; set; }
        [DataMember]
        public int Round3 { get; set; }
        [DataMember]
        public int RoundLater { get; set; }
    }
}
