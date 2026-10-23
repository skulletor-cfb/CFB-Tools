using System.Text.RegularExpressions;
using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Util;
using Newtonsoft.Json;

namespace ScheduleMaker
{
    /// <summary>
    /// Ports the NCAA 14 coaching staffs (the COCH table dumped to CSV) onto a CFB 27 preseason save. Both games have a head
    /// coach, offensive and defensive coordinator per team, so each NCAA 14 coach replaces the CFB 27 coach in the same job
    /// at the same school. The row keeps whether the user controls it, so the user takes over whoever lands in their seat.
    ///
    /// A replaced coach gets the NCAA 14 name, age, alma mater, prestige, years with the team, a generic coach head of the NCAA 14 skin tone
    /// (CHGT), and the NCAA 14 career record (wins and losses overall and at the school, bowls, titles, rivalry and top-25
    /// games, longest streak); the CFB 27 coach's own record and this season's numbers are cleared. Playbooks, schemes, level, talent tree, contract, hometown,
    /// body and clothing stay CFB 27's. The 12 CFB 27 teams NCAA 14
    /// didn't have, and coaches without a team, are left alone. Returns the old-to-new coach id map the web page creator's
    /// continuation file uses (OldCoachId = NCAA 14 CCID, NewCoachId = CFB 27 Coach row).
    /// </summary>
    internal sealed class Ncaa14CoachImporter
    {
        /// <summary>NCAA 14 team ids whose CFB 27 team has a different classic id (the same aliases the roster and schedule imports use).</summary>
        private static readonly Dictionary<int, int> OldIdAliases = new Dictionary<int, int>
        {
            { 34, 901 },  // App State
            { 61, 903 },  // Coastal Carolina
            { 100, 904 }, // Charlotte
            { 181, 902 }, // Georgia Southern
            { 230, 905 }, // James Madison
        };

        private const int NoNcaaTeam = 1023;
        private const int NoTeamIndex = 255;

        /// <summary>NCAA 14 COPS → CoachPosition.</summary>
        private static readonly string[] Jobs = { "HeadCoach", "OffensiveCoordinator", "DefensiveCoordinator" };

        /// <summary>
        /// NCAA 14 coach prestige (COSP, 1-6; CTOP is the same for a whole staff, so it's the program's) → CFB 27 letter grade,
        /// matched by where each sits in its league: the top ~13% of NCAA 14 coaches (6) to an A, the bottom ~10% (1) to an F.
        /// </summary>
        private static readonly Dictionary<int, string> PrestigeGrades = new Dictionary<int, string>
        {
            { 6, "A" }, { 5, "Bplus" }, { 4, "Bminus" }, { 3, "C" }, { 2, "D" }, { 1, "F" },
        };

        /// <summary>
        /// NCAA 14 coach head (CHGT, 0-64) → light / medium / dark, from looking at the heads in game. It isn't a scale: 0-18 are
        /// light (15 medium), 19-53 dark (28 light), and 54-64 light except 54, 55, 58 and 60, which are medium.
        /// </summary>
        private static readonly HashSet<int> MediumChgt = new HashSet<int> { 15, 54, 55, 58, 60 };
        private static readonly HashSet<int> LightChgtInDarkRange = new HashSet<int> { 28 };

        private static int SkinGroup(int chgt) =>
            MediumChgt.Contains(chgt) ? 1
            : chgt >= 19 && chgt <= 53 && !LightChgtInDarkRange.Contains(chgt) ? 2
            : 0;

        /// <summary>CFB 27 generic head skin digit (1-8): 1-3 light, 4-5 medium, 6-8 dark.</summary>
        private static int SkinGroupOfHead(int skin) => skin <= 3 ? 0 : skin <= 5 ? 1 : 2;

        /// <summary>A CFB 27 generic coach head: Generic_####_C_T####_&lt;letter&gt;_&lt;skin 1-8&gt;_&lt;variant&gt;.</summary>
        private static readonly Regex GenericCoachHead = new Regex(@"^Generic_\d+_C_T\d+_[A-Z]_([1-8])_");

        /// <summary>NCAA 14 coach column → CFB 27 CareerCoachStats field (the columns NCAA14DataEngine reads for the same stats).</summary>
        private static readonly (string Ncaa, string Cfb)[] CareerStatMap =
        {
            ("CCWI", "Wins"), ("CCLO", "Losses"),
            ("CTWN", "WinsAtCurrentSchool"), ("COTL", "LossesAtCurrentSchool"),
            ("CBLW", "BowlWins"), ("CBLL", "BowlLosses"),
            ("CCTW", "ConfChampWins"), ("CNTW", "NCWins"),
            ("CRVW", "RivalWins"), ("CRVL", "RivalLosses"),
            ("CTTW", "Top25Wins"), ("CTTL", "Top25Losses"),
        };

        /// <summary>Coach fields holding career or season records; cleared for a ported coach (the longest streak is then set from NCAA 14).</summary>
        private static readonly string[] RecordFields =
        {
            "CareerPointsAgainst", "CareerPointsFor", "CareerLongWinStreak", "CareerTies", "CareerWinSeasons", "CareerPlayoffsMade",
            "CareerBigLossMargin", "CareerBigWinMargin", "CurrentWinStreak", "RegularWinStreak", "WinSeasStreak",
            "ConfPlayoffWinStreak", "DivPlayoffWinStreak", "SuperbowlWinStreak", "WCPlayoffWinStreak", "AwardPoints", "YearlyAwardCount",
            "SeasPointsFor", "SeasPointsAgainst", "SeasTies", "SeasWinStreak", "SeasLongWinStreak", "SeasBigLossMargin", "SeasBigWinMargin",
        };

        private readonly FranchiseFile dynasty;
        private readonly FranchiseFileTable coaches;
        private readonly FranchiseFileTable teams;
        private readonly Dictionary<string, int> prestigeScores;
        private readonly Dictionary<string, int> statOwners;

        /// <summary>
        /// Head recipes worn by CFB 27 coaches with generic heads, by skin group, one entry per wearer. A coach's portrait isn't
        /// the head's number (it's usually 10 more), so the portrait is carried from the same wearer; AssetName holds the head.
        /// </summary>
        private readonly List<(string Head, int Portrait)>[] headsBySkin = { new List<(string, int)>(), new List<(string, int)>(), new List<(string, int)>() };
        private readonly List<string> report = new List<string>();
        private readonly List<(int OldCoachId, int NewCoachId)> mapping = new List<(int, int)>();
        private int ported, almaMaters, careerRowsAdded, statRowsUnlinked, facesChanged;

        private Ncaa14CoachImporter(FranchiseFile dynasty)
        {
            this.dynasty = dynasty;
            coaches = dynasty.GetPrimaryTable("Coach", "FirstName");
            coaches.ReadRecords();
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();

            var all = coaches.Records.Where(r => !r.IsEmpty).ToList();

            // the score that goes with each grade: the median of the coaches who have it now
            prestigeScores = all
                .GroupBy(r => r["CoachPrestige"] as string ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(r => Convert.ToInt32(r["CoachPrestigeScore"])).OrderBy(v => v).ElementAt(g.Count() / 2));

            foreach (var r in all)
            {
                var head = r["GenericHeadAssetName"] as string ?? string.Empty;
                var match = GenericCoachHead.Match(head);
                if (match.Success)
                {
                    headsBySkin[SkinGroupOfHead(int.Parse(match.Groups[1].Value))].Add((head, Convert.ToInt32(r["Portrait"])));
                }
            }

            // a stats row shared by two coaches is unlinked rather than zeroed, so the other coach keeps his record
            statOwners = all
                .SelectMany(r => new[] { r["CareerStats"] as string, r["SeasonStats"] as string })
                .Where(s => s != null && Convert.ToUInt32(s, 2) != 0)
                .GroupBy(s => s!)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public static (List<string> Report, List<(int OldCoachId, int NewCoachId)> Mapping) Import(FranchiseFile dynasty, string coachCsvPath)
        {
            var importer = new Ncaa14CoachImporter(dynasty);
            var report = importer.Run(coachCsvPath);
            return (report, importer.mapping);
        }

        /// <summary>
        /// The continuation file the web page creator reads: the coach id map, {"CoachMapping":[{"NewCoachId":..,"OldCoachId":..}]},
        /// and each carried-over coach's NCAA 14 award counts, which CFB 27 doesn't keep per coach (CoachAwardHistory adds
        /// each new season to them): All-Americans (CNAA), Heisman winners (CHTW), top 25 classes (CNTC), coach of the year (CYRA).
        /// </summary>
        public static void WriteContinuationFile(IEnumerable<(int OldCoachId, int NewCoachId)> mapping, string coachCsvPath, string path)
        {
            var mapped = mapping.ToList();
            var ncaa = ReadCsv(coachCsvPath).GroupBy(c => c.Int("CCID")).ToDictionary(g => g.Key, g => g.First());
            var json = JsonConvert.SerializeObject(new
            {
                CoachMapping = mapped.Select(m => new { m.NewCoachId, m.OldCoachId }),
                CoachAwards = mapped
                    .Where(m => ncaa.ContainsKey(m.OldCoachId))
                    .Select(m => ncaa[m.OldCoachId])
                    .Select(c => new
                    {
                        CoachId = c.Int("CCID"),
                        AllAmericans = c.Int("CNAA"),
                        HeismanWinners = c.Int("CHTW"),
                        Top25Classes = c.Int("CNTC"),
                        CoachOfYearAwards = c.Int("CYRA"),
                    }),
            });
            File.WriteAllText(path, json);
        }

        private List<string> Run(string coachCsvPath)
        {
            var map = CFBTeam.OldTeamIdToTeamRowMap;
            int? TeamRow(int oldId) => map.TryGetValue(OldIdAliases.TryGetValue(oldId, out var alias) ? alias : oldId, out var row) ? row : (int?)null;

            // CFB 27 Coach.AlmaMater = Team.TEAM_ORIGID - 1100 (Air Force is 1100, so 0 is a real alma mater)
            int? AlmaMater(int oldId) =>
                TeamRow(oldId) is int row && Convert.ToInt32(teams.Records[row]["TEAM_ORIGID"]) is var origId && origId >= CFBCoach.AlmaMaterOrigIdOffset
                    ? origId - CFBCoach.AlmaMaterOrigIdOffset
                    : (int?)null;

            var staff = coaches.Records
                .Where(r => !r.IsEmpty && Convert.ToInt32(r["TeamIndex"]) != NoTeamIndex)
                .ToDictionary(r => (Team: Convert.ToInt32(r["TeamIndex"]), Job: r["Position"] as string ?? string.Empty));

            foreach (var c in ReadCsv(coachCsvPath).Where(c => c.Int("TGID") != NoNcaaTeam))
            {
                int job = c.Int("COPS");
                if (TeamRow(c.Int("TGID")) is not int teamRow || job < 0 || job >= Jobs.Length)
                {
                    report.Add($"{c.Name}: NCAA 14 team {c.Int("TGID")} / job {job} has no CFB 27 match.");
                    continue;
                }

                if (!staff.TryGetValue((Convert.ToInt32(teams.Records[teamRow]["TeamIndex"]), Jobs[job]), out var r))
                {
                    report.Add($"{c.Name}: {teams.Records[teamRow]["DisplayName"]} has no {Jobs[job]} to replace.");
                    continue;
                }

                var first = c["CFNM"];
                var last = c["CLNM"];
                r["FirstName"] = first;
                r["LastName"] = last;
                var shortName = first.Length > 0 ? $"{first[0]}. {last}" : last;
                r["Name"] = shortName.Length > 18 ? shortName.Substring(0, 18) : shortName;
                r["Age"] = c.Int("CAGE");

                // both games count completed seasons at the school (the sites show one more)
                r["SeasonsWithTeam"] = c.Int("CTYR");

                if (AlmaMater(c.Int("CHFT")) is int almaMater)
                {
                    r["AlmaMater"] = almaMater;
                    almaMaters++;
                }

                if (PrestigeGrades.TryGetValue(c.Int("COSP"), out var grade))
                {
                    r["CoachPrestige"] = grade;
                    if (prestigeScores.TryGetValue(grade, out var score))
                    {
                        r["CoachPrestigeScore"] = score;
                    }
                }

                WriteHead(c, r);
                WriteRecords(c, r);
                mapping.Add((c.Int("CCID"), r.Index));
                ported++;
            }

            var user = coaches.Records.FirstOrDefault(r => !r.IsEmpty && r["IsUserControlled"] is true);
            report.Insert(0,
                $"Ported {ported} coaches ({almaMaters} with an alma mater found, {facesChanged} given a head matching their NCAA 14 skin tone). NCAA 14 records copied ({careerRowsAdded} coaches " +
                $"needed a new career stats row, {statRowsUnlinked} shared rows unlinked)." + (user == null ? string.Empty : $" You control {user["FirstName"]} {user["LastName"]} ({user["Position"]})."));
            return report;
        }

        /// <summary>A generic coach head of the NCAA 14 skin tone, picked by the coach's NCAA 14 id so a re-run gives the same faces.</summary>
        private void WriteHead(NcaaCoach c, FranchiseFileRecord r)
        {
            var pool = headsBySkin[SkinGroup(c.Int("CHGT"))];
            if (pool.Count == 0)
            {
                return;
            }

            var (head, portrait) = pool[new Random(c.Int("CCID")).Next(pool.Count)];
            r["GenericHeadAssetName"] = head;
            r["AssetName"] = head;
            r["Portrait"] = portrait;
            facesChanged++;
        }

        /// <summary>
        /// The NCAA 14 record goes into a career stats row the coach has to himself: his own, or a new one when he has none (most
        /// coordinators) or shares it with another coach (that coach keeps the shared row). This season's stats are cleared.
        /// </summary>
        private void WriteRecords(NcaaCoach c, FranchiseFileRecord coach)
        {
            foreach (var field in RecordFields)
            {
                coach[field] = 0;
            }

            coach["CareerLongWinStreak"] = c.Int("CCLS");

            var season = OwnStatsRow(coach, "SeasonStats", addIfMissing: false);
            if (season != null)
            {
                ClearStats(season);
            }

            var career = OwnStatsRow(coach, "CareerStats", addIfMissing: true)!;
            ClearStats(career);
            foreach (var (ncaa, cfb) in CareerStatMap)
            {
                career[cfb] = c.Int(ncaa);
            }
        }

        /// <summary>The stats row this coach alone points to; a shared row is unlinked first, and a new one claimed if asked.</summary>
        private FranchiseFileRecord? OwnStatsRow(FranchiseFileRecord coach, string field, bool addIfMissing)
        {
            var reference = coach[field] as string;
            uint value = reference == null ? 0 : Convert.ToUInt32(reference, 2);
            if (value != 0 && statOwners.TryGetValue(reference!, out var owners) && owners > 1)
            {
                coach[field] = BinaryUtil.GetBinaryReferenceData(0, 0);
                statOwners[reference!] = owners - 1;
                statRowsUnlinked++;
                value = 0;
            }

            if (value != 0)
            {
                var table = dynasty.GetTableById((int)(value >> 17));
                if (table != null)
                {
                    table.ReadRecords();
                    return table.Records[(int)(value & 0x1FFFF)];
                }
            }

            if (!addIfMissing)
            {
                return null;
            }

            var stats = dynasty.GetPrimaryTable("CareerCoachStats");
            stats.ReadRecords();
            var row = AddRow(stats);
            coach[field] = BinaryUtil.GetBinaryReferenceData(stats.Header.TableId, row.Index);
            careerRowsAdded++;
            return row;
        }

        /// <summary>
        /// Claims the table's next free row the way PreseasonSchedule.AddGame does: writing a field in the row's first 4 bytes takes
        /// it off the free list, then the row is zeroed since free rows keep leftover bytes.
        /// </summary>
        private static FranchiseFileRecord AddRow(FranchiseFileTable table)
        {
            int index = table.Header.NextRecordToUse;
            if (index >= table.Header.RecordCapacity)
            {
                throw new InvalidOperationException($"{table.Name} has no free rows (all {table.Header.RecordCapacity} are in use).");
            }

            var record = table.Records[index];
            var firstWord = table.OffsetTable.FirstOrDefault(o => o.IndexOffset < 32)
                ?? throw new InvalidOperationException($"Unexpected {table.Name} layout: no field in the row's first 4 bytes.");
            record[firstWord.Name] = firstWord.IsReference ? BinaryUtil.GetBinaryReferenceData(0, 0) : (object)0;
            Array.Clear(record.TableDataArray, record.ByteOffset, record.ByteLength);
            record.ClearCachedFieldValues();
            return record;
        }

        private static void ClearStats(FranchiseFileRecord row)
        {
            foreach (var o in row.Parent.OffsetTable.Where(o => o.Enum == null && !o.IsReference && !o.ValueInSecondTable && o.Type is "int" or "s_int"))
            {
                // RecentYearNCWon uses -2 for "never"
                row[o.Name] = o.Name == "RecentYearNCWon" ? -2 : 0;
            }
        }

        #region csv

        private sealed class NcaaCoach
        {
            private readonly Dictionary<string, string> values;

            public NcaaCoach(Dictionary<string, string> values) => this.values = values;

            public string this[string column] => values.TryGetValue(column, out var v) ? v.Trim() : string.Empty;

            public int Int(string column) => int.TryParse(this[column], out var v) ? v : 0;

            public string Name => $"{this["CFNM"]} {this["CLNM"]}";
        }

        private static List<NcaaCoach> ReadCsv(string path)
        {
            var lines = File.ReadAllLines(path, System.Text.Encoding.Latin1).Where(l => l.Length > 0).ToList();
            var header = lines[0].Split(',');
            return lines.Skip(1)
                .Select(l => l.Split(','))
                .Select(c => new NcaaCoach(header.Select((h, i) => (h, v: i < c.Length ? c[i] : string.Empty)).ToDictionary(x => x.h, x => x.v)))
                .ToList();
        }

        #endregion
    }
}
