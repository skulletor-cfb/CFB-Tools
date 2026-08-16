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
        public static void Create(IDataEngine dataEngine)
        {
            DraftHistory = dataEngine.ReadDraftHistory();
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
