using System.Text.RegularExpressions;
using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Util;
using NCAA14Utility;
using Newtonsoft.Json;

namespace ScheduleMaker
{
    /// <summary>
    /// Ports the NCAA 14 coaching staffs (the COCH table dumped to CSV) onto a CFB 27 preseason save. Both games have a head
    /// coach, offensive and defensive coordinator per team, so each NCAA 14 coach replaces the CFB 27 coach in the same job
    /// at the same school. The row keeps whether the user controls it, so the user takes over whoever lands in their seat.
    ///
    /// A replaced coach gets the NCAA 14 name, age, alma mater, prestige, years with the team, play-calling tendencies, a generic coach head of the NCAA 14 skin tone
    /// (CHGT), a random CFB 27 playbook of each NCAA 14 playbook's style (an Air Raid coach gets one of CFB 27's Air Raid
    /// playbooks; the team takes its head coach's), and the NCAA 14 career record (wins and losses overall and at the school, bowls, titles, rivalry and top-25
    /// games, longest streak); the CFB 27 coach's own record and this season's numbers are cleared. Schemes, level, talent tree, contract, hometown,
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
        /// NCAA 14 coach prestige (PHPV, 0-11: its twelve grades A+ down to D-, which track coach rating almost exactly) → the
        /// same CFB 27 letter grade. CFB 27 also has an F, which no ported coach gets.
        /// </summary>
        private static readonly Dictionary<int, string> PrestigeGrades = new Dictionary<int, string>
        {
            { 11, "Aplus" }, { 10, "A" }, { 9, "Aminus" },
            { 8, "Bplus" }, { 7, "B" }, { 6, "Bminus" },
            { 5, "Cplus" }, { 4, "C" }, { 3, "Cminus" },
            { 2, "Dplus" }, { 1, "D" }, { 0, "Dminus" },
        };

        /// <summary>
        /// The CoachPrestigeScore that goes with each grade. CFB 27 grades each job on its own scale (a head coach's A+ starts
        /// near 1100, a coordinator's near 490), so these are each grade's median score per job among the game's own coaches
        /// (DYNASTY-Y587P before any port). Coordinator D- is set to 60, since most of those sit at 0, to keep the order.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, int>> PrestigeScores = new Dictionary<string, Dictionary<string, int>>
        {
            ["HeadCoach"] = new Dictionary<string, int>
            {
                ["Aplus"] = 1250, ["A"] = 945, ["Aminus"] = 860, ["Bplus"] = 755, ["B"] = 670, ["Bminus"] = 625, ["Cplus"] = 505,
                ["C"] = 475, ["Cminus"] = 380, ["Dplus"] = 300, ["D"] = 230, ["Dminus"] = 180, ["F"] = 90,
            },
            ["OffensiveCoordinator"] = new Dictionary<string, int>
            {
                ["Aplus"] = 490, ["A"] = 450, ["Aminus"] = 430, ["Bplus"] = 375, ["B"] = 330, ["Bminus"] = 285, ["Cplus"] = 250,
                ["C"] = 205, ["Cminus"] = 170, ["Dplus"] = 145, ["D"] = 115, ["Dminus"] = 60, ["F"] = 35,
            },
            ["DefensiveCoordinator"] = new Dictionary<string, int>
            {
                ["Aplus"] = 490, ["A"] = 460, ["Aminus"] = 420, ["Bplus"] = 370, ["B"] = 350, ["Bminus"] = 280, ["Cplus"] = 255,
                ["C"] = 200, ["Cminus"] = 165, ["Dplus"] = 155, ["D"] = 125, ["Dminus"] = 60, ["F"] = 30,
            },
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

        /// <summary>
        /// NCAA 14 offensive playbook ids (COCH.CPID) by the CFB 27 style they become. The style of each id is the web page
        /// creator's OffPlaybookName (Archive\HTML\utility.js), with 162 and 163 as named from 2372 on; NCAA 14's named variants
        /// go to the closest CFB 27 style (Power Spread, Spread Option, and One Back to Multiple, since CFB 27 has no one-back).
        /// </summary>
        private static readonly Dictionary<string, int[]> OffensePlaybookIdsByStyle = new Dictionary<string, int[]>
        {
            { "OFF_AIR_RAID", new[] { 15, 16, 23, 33, 36, 42, 54, 72, 85, 90, 92, 95, 113, 115, 133, 173 } },
            { "OFF_MULTIPLE_OFFENSE", new[] { 2, 6, 11, 12, 13, 17, 18, 20, 21, 24, 26, 27, 35, 39, 41, 46, 48, 50, 53, 55, 60, 73, 76, 84, 88, 99, 101, 106, 109, 110, 120, 129, 164 } },
            { "OFF_OPTION", new[] { 0, 7, 30, 59 } },
            { "OFF_PISTOL", new[] { 19, 61, 62, 71, 130, 168 } },
            { "OFF_POWER_SPREAD", new[] { 170 } },
            { "OFF_PRO_STYLE", new[] { 5, 28, 31, 32, 34, 38, 40, 43, 51, 52, 66, 74, 77, 78, 80, 81, 82, 86, 87, 96, 98, 104, 105, 111, 114, 116, 117, 125, 132 } },
            { "OFF_RUN_AND_SHOOT", new[] { 83 } },
            { "OFF_SPREAD", new[] { 1, 3, 4, 8, 9, 10, 14, 22, 25, 29, 37, 44, 45, 47, 49, 56, 57, 58, 63, 64, 65, 67, 68, 69, 70, 75, 79, 89, 91, 93, 94, 97, 100, 102, 103, 107, 108, 112, 118, 121, 131, 134, 162, 165, 166 } },
            { "OFF_SPREAD_OPTION", new[] { 135, 163, 167, 169, 174 } },
        };

        /// <summary>NCAA 14 defensive playbook (COCH.CDID, utility.js DefPlaybookName) → CFB 27 style.</summary>
        private static readonly string[] DefenseStyles =
        {
            "DEF_BASE4_3",          // 0 4-3
            "DEF_BASE3_4",          // 1 3-4
            "DEF_3_3_5",            // 2 3-3-5
            "DEF_4_2_5",            // 3 4-2-5
            "DEF_MULTIPLE_DEFENSE", // 4 Multiple D
            "DEF_3_4_MULTIPLE",     // 5 3-4 Multiple
            "DEF_4_3_MULTIPLE",     // 6 4-3 Multiple
        };

        /// <summary>CFB 27 styles an NCAA 14 style may draw from when it isn't just its own: Spread covers all three spread families.</summary>
        private static readonly Dictionary<string, string[]> CompatibleStyles = new Dictionary<string, string[]>
        {
            { "OFF_SPREAD", new[] { "OFF_SPREAD", "OFF_POWER_SPREAD", "OFF_VEER_AND_SHOOT" } },
        };

        /// <summary>
        /// CFB 27 has only two Pro Style playbooks, so an NCAA 14 Pro coach gets one of them this often and a Multiple playbook
        /// otherwise.
        /// </summary>
        private const int ProStyleChance = 15;

        private static readonly Dictionary<int, string> OffenseStyleById =
            OffensePlaybookIdsByStyle.SelectMany(kvp => kvp.Value.Select(id => (id, kvp.Key))).ToDictionary(x => x.id, x => x.Key);

        /// <summary>
        /// NCAA 14 play-calling tendencies (Xan's config: sim run/pass ratio and playcalling aggressiveness) → CFB 27 coach
        /// fields. Both games use the same 0-100 style scale; CFB 27's own coaches all have 0 defensive run/pass.
        /// </summary>
        private static readonly (string Ncaa, string Cfb)[] TendencyMap =
        {
            ("COTR", "COACH_OFFTENDENCYRUNPASS"),
            ("CDTR", "COACH_DEFTENDENCYRUNPASS"),
            ("COTA", "COACH_OFFTENDENCYAGGRESSCONSERV"),
            ("CDTA", "COACH_DEFTENDENCYAGGRESSCONSERV"),
        };

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
        private readonly Dictionary<string, int> statOwners;

        /// <summary>
        /// Head recipes worn by CFB 27 coaches with generic heads, by skin group, one entry per wearer. A coach's portrait isn't
        /// the head's number (it's usually 10 more), so the portrait is carried from the same wearer; AssetName holds the head.
        /// </summary>
        private readonly List<(string Head, int Portrait)>[] headsBySkin = { new List<(string, int)>(), new List<(string, int)>(), new List<(string, int)>() };
        private readonly List<string> report = new List<string>();
        private readonly List<(int OldCoachId, int NewCoachId)> mapping = new List<(int, int)>();
        private int ported, almaMaters, careerRowsAdded, statRowsUnlinked, facesChanged, playbooksChanged, levelsRaised;

        /// <summary>NCAA 14 coaches top out at level 54 and CFB 27's at 100; levels scale linearly between them.</summary>
        private const int NcaaMaxLevel = 54;
        private const int CfbMaxLevel = 100;

        /// <summary>Coach points a CFB 27 coach earns per level, given as unspent points for each level a ported coach gains.</summary>
        private const int CoachPointsPerLevel = 25;

        /// <summary>NCAA 14 coach level (CSKL.CLVL) by coach id, from CSKL.csv next to COCH.csv; empty if it isn't there.</summary>
        private Dictionary<int, int> ncaaLevels = new Dictionary<int, int>();

        /// <summary>The save's own playbook lists (OffensivePlaybookDataType[] / DefensivePlaybookDataType[]) by style, school playbooks only.</summary>
        private readonly Dictionary<string, List<CFBPlaybook>> offensePlaybooks;
        private readonly Dictionary<string, List<CFBPlaybook>> defensePlaybooks;

        private Ncaa14CoachImporter(FranchiseFile dynasty)
        {
            this.dynasty = dynasty;
            coaches = dynasty.GetPrimaryTable("Coach", "FirstName");
            coaches.ReadRecords();
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();

            var all = coaches.Records.Where(r => !r.IsEmpty).ToList();

            foreach (var r in all)
            {
                var head = r["GenericHeadAssetName"] as string ?? string.Empty;
                var match = GenericCoachHead.Match(head);
                if (match.Success)
                {
                    headsBySkin[SkinGroupOfHead(int.Parse(match.Groups[1].Value))].Add((head, Convert.ToInt32(r["Portrait"])));
                }
            }

            offensePlaybooks = PlaybooksByStyle(dynasty, "OffensivePlaybookDataType[]");
            defensePlaybooks = PlaybooksByStyle(dynasty, "DefensivePlaybookDataType[]");

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
            var skillsPath = Path.Combine(Path.GetDirectoryName(coachCsvPath) ?? string.Empty, "CSKL.csv");
            if (File.Exists(skillsPath))
            {
                ncaaLevels = ReadCsv(skillsPath).GroupBy(k => k.Int("CCID")).ToDictionary(g => g.Key, g => g.First().Int("CLVL"));
            }
            else
            {
                report.Add($"No CSKL.csv next to {Path.GetFileName(coachCsvPath)}, so coach levels were left as they were.");
            }

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

                if (PrestigeGrades.TryGetValue(c.Int("PHPV"), out var grade))
                {
                    r["CoachPrestige"] = grade;
                    r["CoachPrestigeScore"] = PrestigeScores[Jobs[job]][grade];
                }

                WriteHead(c, r);
                WritePlaybooks(c, r);
                WriteRecords(c, r);
                WriteLevel(c, r);

                foreach (var (ncaa, cfb) in TendencyMap)
                {
                    r[cfb] = Math.Clamp(c.Int(ncaa), 0, 100);
                }
                mapping.Add((c.Int("CCID"), r.Index));
                ported++;
            }

            // the team's playbooks are its head coach's (as in CFB 27's own saves)
            var portedRows = mapping.Select(m => m.NewCoachId).ToHashSet();
            foreach (var headCoach in coaches.Records.Where(r => !r.IsEmpty && portedRows.Contains(r.Index) && r["Position"] as string == "HeadCoach"))
            {
                var team = teams.Records.FirstOrDefault(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) == Convert.ToInt32(headCoach["TeamIndex"]));
                if (team != null && CFBPlaybook.Find(headCoach["OffensivePlaybook"] as string) is CFBPlaybook offense
                    && CFBPlaybook.Find(headCoach["DefensivePlaybook"] as string) is CFBPlaybook defense)
                {
                    team["TEAM_OFFPLAYBOOK"] = offense.PlaybookId;
                    team["TEAM_DEFPLAYBOOK"] = defense.PlaybookId;
                }
            }

            var user = coaches.Records.FirstOrDefault(r => !r.IsEmpty && r["IsUserControlled"] is true);
            report.Insert(0,
                $"Ported {ported} coaches ({almaMaters} with an alma mater found, {facesChanged} given a head matching their NCAA 14 skin tone, {playbooksChanged} given CFB 27 playbooks of their NCAA 14 styles, {levelsRaised} raised to their NCAA 14 level). NCAA 14 records copied ({careerRowsAdded} coaches " +
                $"needed a new career stats row, {statRowsUnlinked} shared rows unlinked)." + (user == null ? string.Empty : $" You control {user["FirstName"]} {user["LastName"]} ({user["Position"]})."));
            return report;
        }

        private static Dictionary<string, List<CFBPlaybook>> PlaybooksByStyle(FranchiseFile dynasty, string listTable)
        {
            var available = new HashSet<uint>();
            foreach (var list in dynasty.GetAllTablesByName(listTable))
            {
                list.ReadRecords();
                foreach (var record in list.Records.Where(r => !r.IsEmpty))
                {
                    foreach (var field in record.FieldsArray.Take(record.ArraySize ?? record.FieldsArray.Count))
                    {
                        if (field.Value is string bits && bits.Length == 32)
                        {
                            available.Add(Convert.ToUInt32(bits, 2));
                        }
                    }
                }
            }

            return CFBPlaybook.Embedded
                .Where(p => available.Contains(p.AssetId) && p.IsCollegeTeamPlaybook)
                .GroupBy(p => p.Scheme)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => p.AssetId).ToList());
        }

        /// <summary>
        /// NCAA 14 level x 100 / 54, but never below the CFB 27 coach's level: his talent tree was bought with the points of that
        /// level, so a raise comes with 25 unspent coach points per level gained to buy more.
        /// </summary>
        private void WriteLevel(NcaaCoach c, FranchiseFileRecord r)
        {
            if (!ncaaLevels.TryGetValue(c.Int("CCID"), out var ncaaLevel))
            {
                return;
            }

            int level = Math.Clamp((int)Math.Round(ncaaLevel * (double)CfbMaxLevel / NcaaMaxLevel), 1, CfbMaxLevel);
            int current = Convert.ToInt32(r["Level"]);
            if (level <= current)
            {
                return;
            }

            r["Level"] = level;
            r["CoachPoints"] = Math.Min(4095, Convert.ToInt32(r["CoachPoints"]) + (level - current) * CoachPointsPerLevel);
            levelsRaised++;
        }

        /// <summary>The playbooks of a style, plus those of the styles it's compatible with.</summary>
        private static List<CFBPlaybook> Pool(Dictionary<string, List<CFBPlaybook>> byStyle, string style) =>
            (CompatibleStyles.TryGetValue(style, out var styles) ? styles : new[] { style })
                .SelectMany(s => byStyle.TryGetValue(s, out var list) ? list : new List<CFBPlaybook>())
                .ToList();

        /// <summary>
        /// A random CFB 27 playbook of the coach's NCAA 14 style on each side; each run picks anew. A style CFB 27 has no
        /// playbook for leaves that side as it was.
        /// </summary>
        private void WritePlaybooks(NcaaCoach c, FranchiseFileRecord r)
        {
            bool changed = false;
            if (OffenseStyleById.TryGetValue(c.Int("CPID"), out var offense) && offense == "OFF_PRO_STYLE" && CommonUtility.Check(100-ProStyleChance))
            {
                offense = "OFF_MULTIPLE_OFFENSE";
            }

            if (offense != null && Pool(offensePlaybooks, offense) is var offensePool && offensePool.Count > 0)
            {
                r["OffensivePlaybook"] = CommonUtility.Choose(offensePool).Reference;
                changed = true;
            }

            int defense = c.Int("CDID");
            if (defense >= 0 && defense < DefenseStyles.Length && Pool(defensePlaybooks, DefenseStyles[defense]) is var defensePool && defensePool.Count > 0)
            {
                r["DefensivePlaybook"] = CommonUtility.Choose(defensePool).Reference;
                changed = true;
            }

            if (changed)
            {
                playbooksChanged++;
            }
        }

        /// <summary>A random generic coach head of the NCAA 14 skin tone; each run picks anew.</summary>
        private void WriteHead(NcaaCoach c, FranchiseFileRecord r)
        {
            var pool = headsBySkin[SkinGroup(c.Int("CHGT"))];
            if (pool.Count == 0)
            {
                return;
            }

            var (head, portrait) = pool.Choose();
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
