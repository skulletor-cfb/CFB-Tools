using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Util;

namespace ScheduleMaker
{
    /// <summary>
    /// Ports NCAA 14 team metadata (the TEAM table dumped to CSV) onto a CFB 27 preseason save: the media and coaches polls,
    /// the recruiting pitch grades and team prestige. Each team's history totals (TeamHistoricalData: all-time record, home
    /// record and streaks, bowls, titles ...) are zeroed, since the web page creator's continuation data carries NCAA 14's.
    /// Rivalries and the season-by-season history are left as they are. The 12 CFB 27 teams NCAA 14 didn't have keep their
    /// own polls (moved below the NCAA 14 teams'), grades and prestige, but their history totals are zeroed too.
    /// </summary>
    internal sealed class Ncaa14TeamImporter
    {
        /// <summary>NCAA 14 team ids whose CFB 27 team has a different classic id (the same aliases the other imports use).</summary>
        private static readonly Dictionary<int, int> OldIdAliases = new Dictionary<int, int>
        {
            { 34, 901 },  // App State
            { 61, 903 },  // Coastal Carolina
            { 100, 904 }, // Charlotte
            { 181, 902 }, // Georgia Southern
            { 230, 905 }, // James Madison
        };

        private const int NoTeamIndex = 255;

        /// <summary>NCAA 14 pitch grades (0-11) are its twelve grades A+ down to D-; CFB 27's F isn't used.</summary>
        private static readonly string[] Grades = { "Dminus", "D", "Dplus", "Cminus", "C", "Cplus", "Bminus", "B", "Bplus", "Aminus", "A", "Aplus" };

        /// <summary>NCAA 14 pitch column → CFB 27 MySchoolTrackingTable grade. NCAA 14 has no campus lifestyle, conference prestige or stadium atmosphere grade.</summary>
        private static readonly (string Ncaa, string[] Cfb)[] PitchMap =
        {
            ("PAPV", new[] { "AcademicPrestigeGrade" }),
            ("PCPV", new[] { "CoachPrestigeGrade" }),
            ("PCHV", new[] { "ChampionshipContenderGrade" }),
            ("PAFV", new[] { "AthleticFacilitiesGrade" }),
            ("PTDV", new[] { "ProgramTraditionGrade" }),
            ("PTVV", new[] { "BrandExposureGrade" }),
            ("PPSV", new[] { "CoachStabilityGrade" }), // NCAA 14 Program Stability
            ("PPFV", new[]                             // one NCAA 14 grade for every position
            {
                "ProPotentialGradeQB", "ProPotentialGradeRB", "ProPotentialGradeWR", "ProPotentialGradeTE", "ProPotentialGradeOL",
                "ProPotentialGradeDL", "ProPotentialGradeLB", "ProPotentialGradeDB", "ProPotentialGradeK", "ProPotentialGradeP",
            }),
        };

        /// <summary>NCAA 14 team prestige (TPRX, 1-6 stars) → CFB 27 TeamPrestige (0-10).</summary>
        private static readonly Dictionary<int, int> PrestigeScale = new Dictionary<int, int>
        {
            { 1, 2 }, { 2, 4 }, { 3, 5 }, { 4, 7 }, { 5, 8 }, { 6, 10 },
        };

        private readonly FranchiseFile dynasty;
        private readonly FranchiseFileTable teams;
        private readonly List<string> report = new List<string>();
        private int ported, gradesSet, historiesZeroed;

        private Ncaa14TeamImporter(FranchiseFile dynasty)
        {
            this.dynasty = dynasty;
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();
        }

        public static List<string> Import(FranchiseFile dynasty, string teamCsvPath) => new Ncaa14TeamImporter(dynasty).Run(teamCsvPath);

        private List<string> Run(string teamCsvPath)
        {
            var map = CFBTeam.OldTeamIdToTeamRowMap;
            var fbs = teams.Records.Where(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) != NoTeamIndex).ToList();
            var portedRows = new HashSet<int>();

            foreach (var n in ReadCsv(teamCsvPath))
            {
                int oldId = n.Int("TGID");
                if (!map.TryGetValue(OldIdAliases.TryGetValue(oldId, out var alias) ? alias : oldId, out var row)
                    || !(teams.Records[row] is var team) || team.IsEmpty || Convert.ToInt32(team["TeamIndex"]) == NoTeamIndex)
                {
                    continue; // FCS, unused rows and teams CFB 27 doesn't have
                }

                WritePolls(n, team);
                WritePitchGrades(n, team);
                if (PrestigeScale.TryGetValue(n.Int("TPRX"), out var prestige))
                {
                    team["TeamPrestige"] = prestige;
                }

                ZeroHistory(team);
                portedRows.Add(row);
                ported++;
            }

            // the teams NCAA 14 didn't have start their history from zero too
            foreach (var team in fbs.Where(t => !portedRows.Contains(t.Index)))
            {
                ZeroHistory(team);
            }

            // and go below its 126 in the polls, in their own order
            int next = portedRows.Count + 1, nextCoaches = portedRows.Count + 1;
            foreach (var team in fbs.Where(t => !portedRows.Contains(t.Index)).OrderBy(t => Convert.ToInt32(t["MediaPoll_CurrentRank"])))
            {
                SetMediaRank(team, next++);
            }

            foreach (var team in fbs.Where(t => !portedRows.Contains(t.Index)).OrderBy(t => Convert.ToInt32(t["CoachesPoll_CurrentRank"])))
            {
                SetCoachesRank(team, nextCoaches++);
            }

            // the prestige rank is the teams ordered by prestige; ties go by the media poll
            int rank = 1;
            foreach (var team in fbs.OrderByDescending(t => Convert.ToInt32(t["TeamPrestige"])).ThenBy(t => Convert.ToInt32(t["MediaPoll_CurrentRank"])))
            {
                team["PrestigeRank"] = rank++;
            }

            report.Insert(0, $"Ported {ported} teams: polls, team prestige, {gradesSet} teams' pitch grades; {historiesZeroed} history totals zeroed. " +
                $"The {fbs.Count - portedRows.Count} CFB 27-only teams are ranked {portedRows.Count + 1}-{fbs.Count} in both polls.");
            return report;
        }

        private static void WritePolls(NcaaTeam n, FranchiseFileRecord team)
        {
            SetMediaRank(team, n.Int("TMRK"));
            team["MediaPoll_CurrentPoints"] = n.Int("tmpp");
            team["MediaPoll_FirstPlaceVotes"] = n.Int("TMPV");

            SetCoachesRank(team, n.Int("TCRK"));
            team["CoachesPoll_CurrentPoints"] = n.Int("TCPT");
            team["CoachesPoll_FirstPlaceVotes"] = n.Int("TCFP");
        }

        /// <summary>In the preseason a team's current, start-of-season, last week's and hidden ranks are all the same.</summary>
        private static void SetMediaRank(FranchiseFileRecord team, int rank)
        {
            team["MediaPoll_CurrentRank"] = rank;
            team["MediaPoll_StartOfSeasonRank"] = rank;
            team["MediaPoll_LastWeeksRank"] = rank;
            team["MediaPoll_HiddenCurrentRank"] = rank;
        }

        private static void SetCoachesRank(FranchiseFileRecord team, int rank)
        {
            team["CoachesPoll_CurrentRank"] = rank;
            team["CoachesPoll_LastWeeksRank"] = rank;
            team["CoachesPoll_HiddenCurrentRank"] = rank;
        }

        private void WritePitchGrades(NcaaTeam n, FranchiseFileRecord team)
        {
            var grades = Follow(team, "MySchoolTrackingTable");
            if (grades == null)
            {
                report.Add($"{team["DisplayName"]} has no pitch grade record.");
                return;
            }

            foreach (var (ncaa, cfb) in PitchMap)
            {
                var grade = Grades[Math.Clamp(n.Int(ncaa), 0, Grades.Length - 1)];
                foreach (var field in cfb)
                {
                    grades[field] = grade;
                }
            }

            gradesSet++;
        }

        /// <summary>Every count in the team's TeamHistoricalData goes to 0 (its TeamSeriesHistory reference is left alone).</summary>
        private void ZeroHistory(FranchiseFileRecord team)
        {
            var history = Follow(team, "TeamHistoricalData");
            if (history == null)
            {
                return;
            }

            foreach (var o in history.Parent.OffsetTable.Where(o => o.Enum == null && !o.IsReference && !o.ValueInSecondTable && o.Type is "int" or "s_int"))
            {
                history[o.Name] = 0;
            }

            historiesZeroed++;
        }

        private FranchiseFileRecord? Follow(FranchiseFileRecord record, string field)
        {
            if (!(record.GetReferenceDataByKey(field) is RecordReference reference) || (reference.TableId == 0 && reference.RowNumber == 0))
            {
                return null;
            }

            var table = dynasty.GetTableById(reference.TableId);
            if (table == null)
            {
                return null;
            }

            table.ReadRecords();
            return table.Records[reference.RowNumber];
        }

        #region csv

        private sealed class NcaaTeam
        {
            private readonly Dictionary<string, string> values;

            public NcaaTeam(Dictionary<string, string> values) => this.values = values;

            public int Int(string column) => values.TryGetValue(column, out var v) && int.TryParse(v.Trim(), out var i) ? i : 0;
        }

        /// <summary>TEAM.csv: a header row of NCAA 14 column names (case matters: tmpp is media poll points, TMPP the previous rank).</summary>
        private static List<NcaaTeam> ReadCsv(string path)
        {
            var lines = File.ReadAllLines(path, System.Text.Encoding.Latin1).Where(l => l.Length > 0).ToList();
            var header = lines[0].Split(',');
            return lines.Skip(1)
                .Select(l => l.Split(','))
                .Select(c => new NcaaTeam(header.Select((h, i) => (h, v: i < c.Length ? c[i] : string.Empty))
                    .GroupBy(x => x.h, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().v, StringComparer.Ordinal)))
                .ToList();
        }

        #endregion
    }
}
