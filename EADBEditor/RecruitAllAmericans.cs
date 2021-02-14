using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace EA_DB_Editor
{
    public static class RecruitAllAmericans
    {
        // states, determined by RCPT[STAT]
        public static int[] EastStates = { 0, 6, 7, 8, 9, 12, 13, 16, 18, 19, 20, 21, 23, 28, 29, 31, 32, 34, 37, 38, 39, 41, 44, 45, 47, 48, 51 };
        public static int[] WestStates = { 1, 2, 3, 4, 5, 10, 11, 14, 15, 17, 22, 24, 25, 26, 27, 30, 33, 35, 36, 40, 42, 43, 46, 49, 50 };
        public static HSAARoster EastRoster = new HSAARoster();
        public static HSAARoster WestRoster = new HSAARoster();

        public static void CreateRosterFiles(string directory)
        {
            RecruitAllAmericans.EastRoster.ToJsonFile(Path.Combine(directory, "hsaa-east"));
            RecruitAllAmericans.WestRoster.ToJsonFile(Path.Combine(directory, "hsaa-west"));
        }

        public static void GetRecruits(MaddenTable recruitStatTable, MaddenTable recruitPitchTable)
        {
            RecruitPlayer.PitchTable = recruitPitchTable;
            // order recruits by rank
            foreach (var recruit in recruitStatTable.lRecords.OrderBy(mr => mr.lEntries[62].Data.ToInt32()))
            {
                var recruitState = recruit["STAT"].ToInt32();
                HSAARoster teamTryingOutFor = EastStates.Contains(recruitState) ? EastRoster : WestRoster;
                teamTryingOutFor.TryOut(recruit);

                // stop iterating once we filled our rosters
                if (EastRoster.RosterFilled && WestRoster.RosterFilled)
                {
                    EastRoster.SortPlayers();
                    WestRoster.SortPlayers();
                    break;
                }
            }
        }
    }

    [DataContract]
    public class RecruitPlayer
    {
        public static MaddenTable PitchTable { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public int PositionRank { get; set; }
        [DataMember]
        public string Hometown { get; set; }
        [DataMember]
        public string TopTeam { get; set; }
        [DataMember]
        public bool Committed { get; set; }
        [DataMember]
        public string PositionName { get; set; }
        [DataMember]
        public int TopTeamId { get; set; }
        public RecruitPlayer(MaddenRecord mr)
        {
            var HometownValue = mr.lEntries[33].Data.ToInt32();
            this.Id = mr.lEntries[53].Data.ToInt32();
            this.FirstName = mr.lEntries[14].Data;
            this.LastName = mr.lEntries[15].Data;
            this.Position = mr["PPOS"].ToInt32();
            this.PositionRank = mr["RCPR"].ToInt32();
#if !NOCITY
            this.Hometown = City.Cities.ContainsKey(HometownValue) ? string.Format("{0}, {1}", City.Cities[HometownValue].Name, City.Cities[HometownValue].State) : "N/A";
#endif
            this.PositionName = this.Position.ToPositionName();
            var pitchRecord = PitchTable.lRecords.Where(rec => rec.lEntries[34].Data.ToInt32() == this.Id).First();
            var commitedTeam = pitchRecord.lEntries[35].Data.ToInt32();
            this.Committed = commitedTeam != 1023;
            this.TopTeam = pitchRecord.lEntries[6].Data.ToInt32().ToTeamName();
            this.TopTeamId = pitchRecord.lEntries[6].Data.ToInt32();
        }

        public RecruitPlayer()
        {
        }
    }

    public class HSAARoster
    {
        public bool DualThreadQBAdded = false;
        public bool PocketQBAdded = false;
        public List<MaddenRecord> Players = new List<MaddenRecord>();
        public void ToJsonFile(string fileName)
        {
            var players = Players.Select(mr => new RecruitPlayer(mr)).ToArray();
            players.ToJsonFile(fileName);
        }
        public bool RosterFilled
        {
            get
            {
                return PositionCount.Values.All(count => count == 0);
            }
        }
        public void SortPlayers()
        {
            Players = Players.OrderBy(mr => mr["PPOS"].ToInt32()).ThenBy(mr => mr["RCRK"].ToInt32()).ToList();
        }
        public void TryOut(MaddenRecord mr)
        {
            var position = mr["PPOS"].ToInt32();

            if (PositionCount[position] > 0)
            {
                bool added = false;
                // for QBs we want at least 1 scrambler and 1 pocket passer
                if (position == 0)
                {
                    // we add a DTQB
                    if (!DualThreadQBAdded && mr["PTEN"].ToInt32() == 2)
                    {
                        Players.Add(mr);
                        added = true;
                        DualThreadQBAdded = true;
                    }
                    else if (!PocketQBAdded && mr["PTEN"].ToInt32() == 0)
                    {
                        Players.Add(mr);
                        added = true;
                        PocketQBAdded = true;
                    }
                    else if (PositionCount[position] == 3 ||
                        (PositionCount[position] == 2 && (PocketQBAdded || DualThreadQBAdded)) ||
                        (PositionCount[position] == 1 && PocketQBAdded && DualThreadQBAdded))
                    {
                        Players.Add(mr);
                        added = true;
                    }
                }
                else
                {
                    Players.Add(mr);
                    added = true;
                }

                if (added)
                {
                    PositionCount[position] = PositionCount[position] - 1;
                }
            }
        }

        // counts per position.  This is RCPT[PPOS]
        public Dictionary<int, int> PositionCount = new Dictionary<int, int>
        {
            {0,3}, // 3 QBs
            {1,4}, // 4 HBs
            {2,2}, // 2 FBs
            {3,8}, // 8 WRs
            {4,3}, // 3 TEs
            {5,3}, // 3 LTs
            {6,3}, // 3 LGs
            {7,3}, // 3 Cs
            {8,3}, // 3 RGs
            {9,3}, // 3 RTs
            {10,3}, // 3 WDEs
            {11,3}, // 3 SDEs
            {12,5}, // 5 DTs
                        {13,2}, // 2 LOLB
                        {14,4}, // 4 MLB
                        {15,3}, // 3 ROLB
                        {16,6}, // 6 CB
                        {17,3}, // 3 FS
                        {18,3}, // 3 SS
                        {19,1}, // 1 K
                        {20,1}, // 1 P
        };
    }
}