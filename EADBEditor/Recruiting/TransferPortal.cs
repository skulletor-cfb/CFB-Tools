using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static EA_DB_Editor.Form1;

namespace EA_DB_Editor
{
    internal class TransferPortal
    {
        private static bool transfersEligbleRun = false;
        public static void MakeTransfersImmediatelyEligble()
        {
            if (transfersEligbleRun) return;

            transfersEligbleRun = true;
            var transferTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TRAN");

            foreach (var mr in transferTable.lRecords)
            {
                mr["TRYR"] = "1";
            }
        }

        public static Dictionary<int, TeamRosterFilled> FindOpenRosterSpots()
        {
            Dictionary<int, TeamRosterFilled> spotsFilled = new Dictionary<int, TeamRosterFilled>();

            // all team ranges start at a multiple of 70 and go to a multiple of 70 -1 (e.g.  140-209)
            foreach (var player in MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023))
            {
                var team = player["TGID"].ToInt32();
                var pgid = player["PGID"].ToInt32();

                if (spotsFilled.ContainsKey(team) == false)
                {
                    spotsFilled[team] = new TeamRosterFilled(team);
                }

                spotsFilled[team].Spots[pgid % 70] = true;

                if (spotsFilled[team].Offset == 0)
                {
                    spotsFilled[team].Offset = (pgid / 70) * 70;
                }
            }

            return spotsFilled;
        }

        public static Dictionary<int, List<RecruitInfo>> FindCommittedRecruits()
        {
            var recruits = new Dictionary<int, List<RecruitInfo>>();
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");
            var recruitPitchTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPR");

            foreach (var recruitPlayer in recruitTable.lRecords)
            {
                var recruitId = recruitPlayer["PRSI"].ToInt32();
                var recruitPitch = FindRecruit(recruitPitchTable, recruitId, true);

                if (recruitPitch == null)
                {
                    continue;
                }

                // recruit did not commit
                var teamCommittedTo = recruitPitch["PTCM"].ToInt32();
                if (teamCommittedTo == 1023)
                {
                    continue;
                }

                // we have a committed recruit, add them to the list of recruits for that team
                var recruit = new RecruitInfo(recruitPlayer, recruitId, teamCommittedTo);
                if (recruits.TryGetValue(teamCommittedTo, out var list))
                {
                    list.Add(recruit);
                }
                else
                {
                    recruits[teamCommittedTo] = new List<RecruitInfo>() { recruit };
                }
            }

            return recruits;
        }

        public static Dictionary<int, TeamRoster> BuildTeamRosterPicture()
        {
            var recruits = FindCommittedRecruits();
            var rosterSpots = FindOpenRosterSpots();
            var result = new Dictionary<int, TeamRoster>();
            foreach (var rosterSpot in rosterSpots)
            {
                result[rosterSpot.Key] = new TeamRoster(rosterSpot.Key, rosterSpot.Value, recruits.TryGetValue(rosterSpot.Key, out var list) ? list : new List<RecruitInfo>());
            }

            return result;
        }

        public static MaddenRecord FindRecruit(MaddenTable pitchTable, int recruitId, bool returnNull = false)
        {
            foreach (var recruit in pitchTable.lRecords)
            {
                foreach (var entry in recruit.lEntries)
                {
                    if (entry.field.Abbreviation == "PRSI")
                    {
                        if (Int32.Parse(entry.Data) == recruitId)
                        {
                            return recruit;
                        }
                    }
                }
            }

            if (returnNull)
            {
                return null;
            }

            throw new Exception("bad data");
        }

    }

    public class TeamRoster
    { 
        public int Id { get; }
        public TeamRosterFilled Filled { get; }
        public List<RecruitInfo> Recruits { get; }
        public int OpenSpots => Filled.NotFilled.Count - Recruits.Count;

        public TeamRoster(int id, TeamRosterFilled filled, List<RecruitInfo> recruits)
        {
            this.Id = id;
            this.Filled = filled;
            this.Recruits = recruits;
        }
    }


    public class RecruitInfo
    {
        public int Id { get; }
        public int TeamId { get; }
        public string State { get; }
        public int PositionGroup { get; }
        public int Overall { get; }
        public string Name { get; }
        public int Position { get; }


        public RecruitInfo(MaddenRecord mr, int id, int teamId)
        {
            this.Id = id;
            this.TeamId = teamId;
            this.State = States.TryGetValue(mr["STAT"].ToInt32(), out var state) ? state : "International";
            this.PositionGroup = mr["RPGP"].ToInt32();
            this.Overall = mr["POVR"].ToInt32(); //rcov is pre scout
            this.Name = $"{mr.lEntries[14].Data} {mr.lEntries[15].Data}";
            this.PositionGroup = mr.lEntries[106].Data.ToInt32();
        }

        public TransferCandidate ToTransferCandidate()
        {
            return new TransferCandidate
            {
                OVR = this.Overall,
                PositionNumber = this.Position,
                Year = 0,
            };
        }

        private static Dictionary<int, string> States = new Dictionary<int, string>()
        {
            [0] = "Alabama",
            [1] = "Alaska",
            [2] = "Arizona",
            [3] = "Arkansas",
            [4] = "California",
            [5] = "Colorado",
            [6] = "Connecticut",
            [7] = "Delaware",
            [8] = "Florida",
            [9] = "Georgia",
            [10] = "Hawaii",
            [11] = "Idaho",
            [12] = "Illinois",
            [13] = "Indiana",
            [14] = "Iowa",
            [15] = "Kansas",
            [16] = "Kentucky",
            [17] = "Louisiana",
            [18] = "Maine",
            [19] = "Maryland",
            [20] = "Massachusetts",
            [21] = "Michigan",
            [22] = "Minnesota",
            [23] = "Mississippi",
            [24] = "Missouri",
            [25] = "Montana",
            [26] = "Nebraska",
            [27] = "Nevada",
            [28] = "New Hampshire",
            [29] = "New Jersey",
            [30] = "New Mexico",
            [31] = "New York",
            [32] = "North Carolina",
            [33] = "North Dakota",
            [34] = "Ohio",
            [35] = "Oklahoma",
            [36] = "Oregon",
            [37] = "Pennsylvania",
            [38] = "Rhode Island",
            [39] = "South Carolina",
            [40] = "South Dakota",
            [41] = "Tennessee",
            [42] = "Texas",
            [43] = "Utah",
            [44] = "Vermont",
            [45] = "Virginia",
            [46] = "Washington State",
            [47] = "West Virginia",
            [48] = "Wisconsin",
            [49] = "Wyoming",
            [50] = "Canada",
            [51] = "D.C.",
        };
    }
}