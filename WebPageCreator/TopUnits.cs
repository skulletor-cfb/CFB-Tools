using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    [DataContract]
    public class TopUnits
    {
        public static int[] QB = { 0 };
        public static int[] RB = { 1, 2 };
        public static int[] REC = { 3, 4 };
        public static int[] OL = { 5, 6, 7 ,8,9};
        public static int[] DL = { 10, 11, 12 };
        public static int[] LB = { 13, 14, 15 };
        public static int[] DB = { 16, 17, 18 };

        public static TopUnits Instance { get; set; }
        
        [DataMember]
        public TopUnit[] TopQB { get; set; }
        
        [DataMember]
        public TopUnit[] TopHB { get; set; }
        
        [DataMember]
        public TopUnit[] TopRec { get; set; }
        
        [DataMember]
        public TopUnit[] TopOL { get; set; }
        
        [DataMember]
        public TopUnit[] TopDL { get; set; }
        
        [DataMember]
        public TopUnit[] TopLB { get; set; }

        [DataMember]
        public TopUnit[] TopDB { get; set; }

        public static void Create(MaddenDatabase db)
        {
            if (Instance != null)
                return;

            Instance = new TopUnits
            {
                TopQB = GetTop10(t => t.TeamRatingQB,QB),
                TopHB = GetTop10(t => t.TeamRatingRB,RB),
                TopRec = GetTop10(t => t.TeamRatingWR,REC),
                TopOL = GetTop10(t => t.TeamRatingOL,OL),
                TopDL = GetTop10(t => t.TeamRatingDL,DL),
                TopLB = GetTop10(t => t.TeamRatingLB,LB),
                TopDB = GetTop10(t => t.TeamRatingDB,DB),
            };

            Instance.ToJsonFile(@".\archive\reports\topunits");
            PocketScout.TopUnits();
        }

        public static TopUnit[] GetTop10(Func<Team, int> ratingFunc, int[] group)
        {
            return Team.Teams.Values.Where(t => !t.Id.IsFCS() && !t.Id.TeamNoLongerFBS())
                .OrderByDescending(t => ratingFunc(t)).ThenByDescending(t => PlayerDB.Rosters[t.Id].Where(p => group.Contains(p.Position)).OrderByDescending(p => p.Ovr).Select(p => p.Ovr).First()).Take(10)
                .Select(t => CreateTopUnit(t.Id, t.Name, group))
                .ToArray();
        }

        public static TopUnit CreateTopUnit(int teamId, string name, int[] group)
        {
            var players = PlayerDB.Rosters[teamId].Where(p => group.Contains(p.Position)).OrderByDescending(p => p.Ovr).ToArray();
            var unit = new TopUnit();
            unit.TeamId = teamId;
            unit.TeamName = name;
            unit.TopPlayerId = players.First().Id;
            unit.TopPlayer = players.First().Name;
            return unit;
        }
    }

    [DataContract]
    public class TopUnit
    {
        [DataMember]
        public string TeamName { get; set; }
        [DataMember]
        public int TeamId { get; set; }
        [DataMember]
        public string TopPlayer { get; set; }
        [DataMember]
        public int TopPlayerId { get; set; }
    }
}