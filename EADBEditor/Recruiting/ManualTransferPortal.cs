using EA_DB_Editor.CAPGen;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static EA_DB_Editor.Form1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace EA_DB_Editor
{
    internal static class ManualTransferPortal
    {
        internal static void WriteTransferPortalFiles(MaddenDatabase maddenDB)
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

            DumpRosters(maddenDB);

            // each one with SR backup greater than 85
            // not Qbs, 3rd stringers
            var other = FindTransferPortalCandidates(maddenDB);

            // g5 superstars, sr above 95, jr above 88
            var g5stars = FindG5tars(maddenDB);

            // find qbs to transfer
            var qbs = FindQBs(maddenDB);

            // coach might be able to bring new players
            var poach = CoachPoachCandidates(maddenDB);
            try
            {
                File.WriteAllText("transfercandidates.csv", qbs.ToString());
                File.WriteAllText("transferPortal.csv", other.ToString());
                File.WriteAllText("g5stars.csv", g5stars.ToString());
                File.WriteAllText("coachpoach.csv", poach.ToString());
            }
            catch { }


            var spotsFilled = TransferPortal.FindOpenRosterSpots();

            var sb = new StringBuilder();
            foreach (var value in spotsFilled.Values.OrderBy(v => v.Team))
            {
                sb.AppendLine(value.ToCsv());
            }

            try
            {
                File.WriteAllText("Roster.csv", sb.ToString());
            }
            catch { }
        }

        public static Dictionary<int, string> PlayerStates = new Dictionary<int, string>();

        public static void RunTransferPortal(MaddenDatabase maddenDB)
        {
            WriteTransferPortalFiles(maddenDB);

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

                        if (player != null && !PlayersTransferred.Contains(to))
                        {
                            player["PGID"] = to.ToString();
                            player["TGID"] = (to / 70).ToString();
                            PlayersTransferred.Add(to);
                        }
                    }
                }
                else
                {
                    var player = MaddenTable.FindTable(maddenDB.lTables, "PLAY").lRecords.Where(mr => mr["TGID"].ToInt32() != 1023 && mr["PGID"].ToInt32() == entry.From).FirstOrDefault();

                    if (player != null && !PlayersTransferred.Contains(entry.To))
                    {
                        player["PGID"] = entry.To.ToString();
                        player["TGID"] = (entry.To / 70).ToString();
                        PlayersTransferred.Add(entry.To);
                    }
                }
            }

            WriteTransferPortalFiles(maddenDB);
        }

        /// <summary>
        ///  once a  player has transferred , we don't want to have them move again in an offseason
        /// </summary>
        public static HashSet<int> PlayersTransferred = new HashSet<int>();
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

        /// <summary>
        /// EASP = heisman watch?  (kemp, stanford, stephens, nichols, maxwell)
        /// MCOV = media coverage
        /// DCHT = depth chart player id, team id, pos = 0, depth = 0
        /// PLAY = player table

        /// PGID - player id
        /// TGID - team id
        /// PFNA - first name
        /// PLNA - last name
        /// PYEA - year(3) = senior
        /// POVR = overall
        /// PPOS = Position
        /// </summary>
        /// <param name="maddenDB"></param>
        /// <param name="positionPredicate"></param>
        /// <returns></returns>
        private static Dictionary<int, List<TransferCandidate>> GetPlayers(MaddenDatabase maddenDB, Func<int, bool> positionPredicate = null)
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
                        PositionNumber = mr["PPOS"].ToInt32(),
                    })
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.OVR).ThenBy(p => p.Year).ToList());
        }

        private static bool IsG5Superstar(TransferCandidate player)
        {
            return player.TeamId.IsG5() && ((player.Year == 3 && player.OVR >= 95) || (player.Year == 2 && player.OVR >= 88));
        }

        private static IEnumerable<TransferCandidate> GetBackupQB(List<TransferCandidate> players) => players.Skip(1);

        private static IEnumerable<TransferCandidate> GetThirdStringers(List<TransferCandidate> players)
        {
            if (players.Count == 0)
            {
                return Array.Empty<TransferCandidate>();
            }

            var skipCount = HowManyToSkip(players[0].PositionNumber);
            return players.Skip(skipCount);
        }

        private static StringBuilder FindQBs(MaddenDatabase maddenDB)
        {
            var rosterSpotsDict = TransferPortal.FindOpenRosterSpots().ToDictionary(kvp => kvp.Key, kvp => new Stack<int>(kvp.Value.NotFilled));
            var recruitClasses = TransferPortal.FindCommittedRecruits();
            var qbNeedyTeams = RecruitingFixup.PrestigeMap.Where(kvp => kvp.Key.IsP5OrND()).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key)
                .Concat(RecruitingFixup.PrestigeMap.Where(kvp => kvp.Key.IsG5()).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key)).ToArray();

            // QBs
            var players = GetPlayers(maddenDB, pos => pos == 0);
            var candidates = players.Values.SelectMany(GetBackupQB).Where(p => p.OVR >= 85 && (p.Year == 3 || (p.Year == 2 && p.Redshirted))).OrderByDescending(p => p.OVR).ToList();
            var inNeed = players.Where(kvp => kvp.Key.IsP5OrND() && kvp.Value.First().OVR < 90).Select(kvp => kvp.Value.First().Team).ToList();
            var g5InNeed = players.Where(kvp => !kvp.Key.IsFcsTeam() && !kvp.Key.IsP5OrND() && kvp.Value.First().OVR < 85).Select(kvp => kvp.Value.First().Team).ToList();
            inNeed.AddRange(g5InNeed);

            StringBuilder sb = new StringBuilder();

            // write transfers
            candidates.ForEach(c => c.FindPlayerDestinations(sb, qbNeedyTeams, players, recruitClasses, rosterSpotsDict));
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
            return sb;
        }

        private static StringBuilder FindG5tars(MaddenDatabase maddenDB)
        {
            var rosterSpotsDict = TransferPortal.FindOpenRosterSpots().ToDictionary(kvp => kvp.Key, kvp => new Stack<int>(kvp.Value.NotFilled));
            var recruitClasses = TransferPortal.FindCommittedRecruits();
            var rosters = GetPlayers(maddenDB);
            var p5Teams = RecruitingFixup.PrestigeMap.Where(kvp => kvp.Key.IsP5OrND()).OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();

            var sb = new StringBuilder();
            for (int i = 0; i <= 18; i++)
            {
                var otherPlayers = GetPlayers(maddenDB, pos => pos == i);
                var otherCandidates = otherPlayers.Values.SelectMany(p => p).Where(IsG5Superstar).OrderByDescending(p => p.OVR).ToList();
                otherCandidates.ForEach(c => c.FindPlayerDestinations(sb, p5Teams, rosters, recruitClasses, rosterSpotsDict));
            }

            return sb;
        }

        private static StringBuilder FindTransferPortalCandidates(MaddenDatabase maddenDB)
        {
            var other = new StringBuilder();

            // we use position groups
            GetPlayersByPosition(maddenDB, other, 1);
            GetPlayersByPosition(maddenDB, other, 2);
            GetPlayersByPosition(maddenDB, other, 3);
            GetPlayersByPosition(maddenDB, other, 4);
            GetPlayersByPosition(maddenDB, other, 5, 9);
            GetPlayersByPosition(maddenDB, other, 6, 8);
            GetPlayersByPosition(maddenDB, other, 7);
            GetPlayersByPosition(maddenDB, other, 10, 11);
            GetPlayersByPosition(maddenDB, other, 12);
            GetPlayersByPosition(maddenDB, other, 13, 15);
            GetPlayersByPosition(maddenDB, other, 14);
            GetPlayersByPosition(maddenDB, other, 16);
            GetPlayersByPosition(maddenDB, other, 17);
            GetPlayersByPosition(maddenDB, other, 18);

            return other;
        }

        private static void GetPlayersByPosition(MaddenDatabase maddenDB, StringBuilder other, params int[] position)
        {
            var otherPlayers = GetPlayers(maddenDB, pos => position.Contains(pos));
            var otherCandidates = otherPlayers.Values.SelectMany(GetThirdStringers).Where(p => p.OVR >= 85 && (p.Year >= 2)).OrderByDescending(p => p.OVR).ToList();
            otherCandidates.ForEach(c => other.AppendLine(c.ToCsvLine()));
        }

        private static int HowManyToSkip(int position)
        {
            switch (position)
            {
                case 3: // wr
                case 16: // cb
                case 5:
                case 9: // OT
                case 6:
                case 8: // OG
                case 10:
                case 11: // DE
                case 13:
                case 15: // OLB
                    return 3;

                default:
                    return 2;
            }
        }

        private static int StarterCount(int position)
        {
            switch (position)
            {
                case 5: // OT
                case 9: // OT
                case 6: // OG
                case 8: // OG
                case 10: // DE
                case 11: // DE
                case 12: // DT
                case 13: // OLB
                case 15: // OLB
                case 16: // cb
                    return 2;

                case 3: // wr
                    return 3;

                case 0: // QB
                case 1: // HB
                case 2: // FB
                case 4: // TE
                case 7: // C
                case 14: // MLB
                case 17: // FS
                case 18: // SS
                default:
                    return 1;
            }
        }

        private static bool IsLateralOrBetterMove(this MaddenRecord mr, Dictionary<int, int> teamPrestigeMap)
        {
            var newTeam = mr["TGID"].ToInt32();
            var previousTeam = mr["CLTF"].ToInt32();
            var previousTeamPrestige = teamPrestigeMap.TryGetValue(previousTeam, out var prestige) ? prestige : 0;
            var newteamPrestige = teamPrestigeMap.TryGetValue(newTeam, out var newPrestige) ? newPrestige : 0;

            // did i go to a higher prestige team?
            if (newteamPrestige > previousTeamPrestige)
            {
                return true;
            }

            // equal prestige but new team is p5
            if (newteamPrestige == previousTeamPrestige && newTeam.IsP5OrND())
            {
                return true;
            }

            // went from g5 to g5 or p5 to g5
            return false;
        }

        /// <summary>
        /// when a g5 coach goes to p5, he might poach players from his old team.  
        /// </summary>
        /// <param name="maddenDB"></param>
        /// <returns></returns>
        private static StringBuilder CoachPoachCandidates(MaddenDatabase maddenDB)
        {
            var sb = new StringBuilder();
            var coachTable = MaddenTable.FindMaddenTable(maddenDB.lTables, "COCH");
            var teamTable = MaddenTable.FindMaddenTable(maddenDB.lTables, "TEAM");
            var teamPrestigeMap = teamTable.lRecords.ToDictionary(mr => mr["TGID"].ToInt32(), mr => mr["TPRX"].ToInt32());
            var allPlayers = GetPlayers(maddenDB);
            var reviewed = new HashSet<int>();

            // new head coaches
            var newPowerCoaches = coachTable.lRecords
                .Where(mr => mr["CTYR"].ToInt32() == 0)
                .Where(mr => mr["COPS"].ToInt32() == 0)
                .Where(mr => mr["TGID"].ToInt32().IsP5OrND())
                .Where(mr => mr["CLTF"].ToInt32().IsG5())
                .ToArray();

            // coaches who went to a higher prestige team
            var prestigeUpgradeCoaches = coachTable.lRecords
                .Where(mr => mr["CTYR"].ToInt32() == 0)
                .Where(mr => mr["COPS"].ToInt32() == 0)
                .Where(mr => mr.IsLateralOrBetterMove(teamPrestigeMap))
                .ToArray();

            var rosterSpotsDict = TransferPortal.FindOpenRosterSpots();
            var recruitClasses = TransferPortal.FindCommittedRecruits();

            foreach (var coach in newPowerCoaches.Concat(prestigeUpgradeCoaches))
            {
                var currentTeamBeingReviewed = coach["TGID"].ToInt32();
                if (reviewed.Contains(currentTeamBeingReviewed))
                {
                    continue;
                }

                reviewed.Add(currentTeamBeingReviewed);

                var rosterSpots = new Stack<int>(rosterSpotsDict[currentTeamBeingReviewed].NotFilled);
                var newRoster = allPlayers[currentTeamBeingReviewed];
                var recruits = recruitClasses[currentTeamBeingReviewed];

                // get all players from my old team
                var oldTeamRoster = allPlayers.TryGetValue(coach["CLTF"].ToInt32(), out var roster) ? roster : new List<TransferCandidate>();

                // no players, we continue
                if (oldTeamRoster.Count == 0) continue;

                sb.AppendLine($"{RecruitingFixup.TeamNames[coach["TGID"].ToInt32()]} {coach["CFNM"]} {coach["CLNM"]}");
                var candidates = oldTeamRoster.Where(p => (p.OVR >= 85 && (p.Year >= 2)) || (p.OVR >= 80 && p.Year == 1) || (p.OVR >= 75 && p.Year == 0)).OrderByDescending(p => p.OVR).ToList();
                candidates.ForEach(c => c.ShouldCoachPoach(sb, newRoster, recruits, rosterSpots));
                sb.AppendLine();
            }

            return sb;
        }

        private static void FindPlayerDestinations(
            this TransferCandidate player,
            StringBuilder sb,
            int[] teams,
            Dictionary<int, List<TransferCandidate>> rosters,
            Dictionary<int, List<RecruitInfo>> recruitClasses,
            Dictionary<int, Stack<int>> rosterSpots)
        {
            var teamsRecruiting = new (string Team, int PlayerId)[5];
            var idx = 0;
            var teamMatch = new HashSet<long>();

            // loop through each one and see if they will recruit the player
            foreach (var team in teams)
            {
                // g5 players want to start
                if (rosterSpots[team].Count == 0 || !player.ShouldPlayerTransfer(rosters[team], recruitClasses[team], true))
                {
                    continue;
                }

                teamsRecruiting[idx++] = (RecruitingFixup.TeamNames[team], rosterSpots[team].Pop());

                // once we found 5 teams, let's go
                if (idx >= 5)
                    break;
            }

            teamsRecruiting.Shuffle();
            var teamList = teamsRecruiting.Select(t => $"{t.Team},{t.PlayerId}");
            sb.AppendLine($"{player.ToCsvLine()},,,{string.Join(",", teamList)}");
        }

        private static void ShouldCoachPoach(this TransferCandidate player, StringBuilder sb, List<TransferCandidate> roster, List<RecruitInfo> incoming, Stack<int> rosterSpots)
        {
            // if i'm better than what they have, i should transfer
            if (rosterSpots.Count == 0 || !player.ShouldPlayerTransfer(roster, incoming))
            {
                return;
            }

            sb.AppendLine($"{player.ToCsvLine()},,,{rosterSpots.Pop()}");
        }

        private static bool ShouldPlayerTransfer(this TransferCandidate player, List<TransferCandidate> roster, List<RecruitInfo> incoming, bool mustBeStarter = false)
        {
            // find the existing players at my position group
            var fullRoster = roster.Concat(incoming.Select(r => r.ToTransferCandidate())).ToList();
            var competition = fullRoster.Where(p => p.SamePositionGroup(player)).ToList();
            var depth = StarterCount(player.PositionNumber);

            // if I'm a senior i need to crack the starting lineup
            if (mustBeStarter || player.Year == 3)
            {
                return competition.Where(p => p.OVR < player.OVR).Count() < depth;
            }

            // if i'm a junior  I need to be in 2 deep
            if (player.Year == 2)
            {
                return competition.Where(p => p.OVR < player.OVR).Count() <= depth;
            }

            // if i'm a sophomore I need to be in 3 deep
            if (player.Year == 1)
            {
                return competition.Where(p => p.OVR < player.OVR).Count() <= (depth + 1);
            }

            // if i'm a freshman, i should be better than incoming recruits and other freshman
            if (player.Year == 0)
            {
                return competition.Where(p => p.OVR < player.OVR && p.Year == 0).Count() == 0;
            }

            return false;
        }

        private static bool SamePositionGroup(this TransferCandidate player, TransferCandidate other)
        {
            // same position is easy
            if (player.PositionNumber == other.PositionNumber)
            {
                return true;
            }

            if (player.IsOT && other.IsOT)
            {
                return true;
            }

            if (player.IsOG && other.IsOG)
            {
                return true;
            }

            if (player.IsDE && other.IsDE)
            {
                return true;
            }

            if (player.IsOLB && other.IsOLB)
            {
                return true;
            }

            return false;
        }

    }
}