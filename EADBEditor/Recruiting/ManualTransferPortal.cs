using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static EA_DB_Editor.Form1;

namespace EA_DB_Editor
{
    internal class ManualTransferPortal
    {
        public static Dictionary<int, string> PlayerStates = new Dictionary<int, string>();

        public static void RunTransferPortal(MaddenDatabase maddenDB)
        {
            if (PlayerStates.Count == 0)
            {
                var lines = File.ReadAllLines("cities.csv");

                foreach (var line in lines)
                {
                    var split = line.Split(',');
                    PlayerStates.Add(split[0].Trim().ToInt32(), split[2].Trim());
                }
            }

            /*
             * EASP = heisman watch?  (kemp, stanford, stephens,nichols, maxwell)
MCOV = media coverage
DCHT = depth chart  player id, team id, pos=0, depth =0
PLAY = player table

PGID - player id
TGID - team id
PFNA - first name 
PLNA - last name
PYEA - year (3) = senior
POVR = overall
PPOS = Position
             */
            Dictionary<int, TransferCandidate[]> GetPlayers(Func<int, bool> positionPredicate = null)
            {
                if (positionPredicate == null) positionPredicate = i => true;
                // QB depth chart
                return MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && positionPredicate(mr["PPOS"].ToInt32()))
                    .GroupBy(
                        mr => mr["TGID"].ToInt32(),
                        mr => new TransferCandidate
                        {
                            Id = mr["PGID"].ToInt32(),
                            OVR = mr["POVR"].ToInt32(),
                            Year = mr["PYEA"].ToInt32(),
                            First = mr["PFNA"],
                            Last = mr["PLNA"],
                            Team = RecruitingFixup.TeamNames[mr["TGID"].ToInt32()],
                            TeamId = mr["TGID"].ToInt32(),
                            Redshirted = mr["PRSD"].ToInt32() == 2,
                            State = PlayerStates.TryGetValue(mr["RCHD"].ToInt32(), out var st) ? st : "unknown",
                            Position = mr["PPOS"].ToInt32().ToPositionName(),
                        })
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.OVR).ThenBy(p => p.Year).ToArray());
            }

            DumpRosters(maddenDB);

            // each one with SR backup greater than 85
            // not Qbs, 3rd stringers
            var other = new StringBuilder();
            for (int i = 1; i <= 18; i++)
            {
                var otherPlayers = GetPlayers(pos => pos == i);
                var otherCandidates = otherPlayers.Values.SelectMany(p => p.Skip(2)).Where(p => p.OVR >= 85 && (p.Year >= 2)).OrderByDescending(p => p.OVR).ToList();
                otherCandidates.ForEach(c => other.AppendLine(c.ToCsvLine()));
            }

            // g5 superstars, jr/sr above 95
            var g5stars = new StringBuilder();

            for (int i = 0; i <= 18; i++)
            {
                var otherPlayers = GetPlayers(pos => pos == i);
                var otherCandidates = otherPlayers.Values.SelectMany(p => p).Where(p => p.TeamId.IsG5() && p.OVR >= 95 && (p.Year >= 2)).OrderByDescending(p => p.OVR).ToList();
                otherCandidates.ForEach(c => g5stars.AppendLine(c.ToCsvLine()));
            }

            // QBs
            var players = GetPlayers(pos => pos == 0);
            var candidates = players.Values.SelectMany(p => p.Skip(1)).Where(p => p.OVR >= 85 && (p.Year == 3 || (p.Year == 2 && p.Redshirted))).OrderByDescending(p => p.OVR).ToList();
            var inNeed = players.Where(kvp => kvp.Key.IsP5OrND() && kvp.Value.First().OVR < 90).Select(kvp => kvp.Value.First().Team).ToList();
            var g5InNeed = players.Where(kvp => !kvp.Key.IsFcsTeam() && !kvp.Key.IsP5OrND() && kvp.Value.First().OVR < 85).Select(kvp => kvp.Value.First().Team).ToList();
            inNeed.AddRange(g5InNeed);

            StringBuilder sb = new StringBuilder();

            // write transfers
            candidates.ForEach(c => sb.AppendLine(c.ToCsvLine()));
            //inNeed.ForEach(c => sb.AppendLine(c));
            sb.AppendLine();

            // each teams QB depth chart
            foreach (var dc in players.Values.Where(tc => inNeed.Contains(tc.First().Team)).OrderBy(tc => tc.First().P5).ThenBy(tc => tc.First().OVR).ThenBy(tc => tc.First().Team))
            {
                sb.AppendLine(string.Empty);
                sb.AppendLine(string.Empty);
                foreach (var p in dc)
                {
                    sb.AppendLine(p.ToCsvLine());
                }
            }

            sb.AppendLine(string.Empty);
            sb.AppendLine(string.Empty);

            try
            {
                File.WriteAllText("transfercandidates.csv", sb.ToString());
                File.WriteAllText("transferPortal.csv", other.ToString());
                File.WriteAllText("g5stars.csv", g5stars.ToString());
            }
            catch { }


            var spotsFilled = TransferPortal.FindOpenRosterSpots();

            sb = new StringBuilder();
            foreach (var value in spotsFilled.Values.OrderBy(v => v.Team))
            {
                sb.AppendLine(value.ToCsv());
            }

            try
            {
                File.WriteAllText("Roster.csv", sb.ToString());
            }
            catch { }

            var entry = new PlayerEntry();
            if (entry.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (entry.From == 999999)
                {
                    var lines = File.ReadAllLines("from.txt");
                    var offset = lines.Length / 2;

                    var fromLines = lines.Take(lines.Length / 2).ToArray();
                    var toLines = lines.Skip(lines.Length / 2).ToArray();

                    // check to make sure we don't have duplicates
                    bool CheckForUniqueness(string[] linesToCheck, string scenario)
                    {
                        var set = new HashSet<string>();

                        foreach (var line in lines)
                        {
                            if (!set.Add(line))
                            {
                                MessageBox.Show($"Duplicate value in {scenario}", line);
                                return false;
                            }
                        }

                        return true;
                    }

                    if (!CheckForUniqueness(fromLines, "from") || !CheckForUniqueness(toLines, "to"))
                    {
                        return;
                    }

                    for (int i = 0; i < offset; i++)
                    {
                        var from = Convert.ToInt32(lines[i]);
                        var to = Convert.ToInt32(lines[i + offset]);

                        var player = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PGID"].ToInt32() == from).FirstOrDefault();

                        if (player != null)
                        {
                            player["PGID"] = to.ToString();
                            player["TGID"] = (to / 70).ToString();
                        }
                    }
                }
                else
                {
                    var player = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PGID"].ToInt32() == entry.From).FirstOrDefault();

                    if (player != null)
                    {
                        player["PGID"] = entry.To.ToString();
                        player["TGID"] = (entry.To / 70).ToString();
                    }
                }
            }
        }
        public static void DumpRosters(MaddenDatabase maddenDB)
        {
            Dictionary<int, List<TransferCandidate>> GetRosters()
            {
                return MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023)
                    .GroupBy(
                        mr => mr["TGID"].ToInt32(),
                        mr => new TransferCandidate
                        {
                            Id = mr["PGID"].ToInt32(),
                            OVR = mr["POVR"].ToInt32(),
                            Year = mr["PYEA"].ToInt32(),
                            First = mr["PFNA"],
                            Last = mr["PLNA"],
                            //                            Team = RecruitingFixup.TeamNames[mr["TGID"].ToInt32()],
                            TeamId = mr["TGID"].ToInt32(),
                            Redshirted = mr["PRSD"].ToInt32() == 2,
                            State = PlayerStates.TryGetValue(mr["RCHD"].ToInt32(), out var st) ? st : "unknown",
                            Position = mr["PPOS"].ToInt32().ToPositionName(),
                            PositionNumber = mr["PPOS"].ToInt32(),
                        })
                    .ToDictionary(g => g.Key, g => g.OrderBy(p => p.PositionNumber).ThenByDescending(p => p.OVR).ThenByDescending(p => p.Year).ToList());
            }

            var allRosters = GetRosters();
            var dir = Directory.CreateDirectory("rosters");
            foreach (var kvp in allRosters)
            {
                var roster = new StringBuilder();
                kvp.Value.ForEach(p => roster.AppendLine(p.ToCsvLine()));

                var file = Path.Combine(dir.FullName, $"{kvp.Key}.csv");

                try
                {
                    File.WriteAllText(file, roster.ToString());
                }
                catch { }
            }
        }
    }
}
