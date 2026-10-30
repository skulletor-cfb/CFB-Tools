using CFB27.Data.Model;
using MaddenFranchise;
using static Saves;

// CoachRecordFix <EndOfYXXX folder> [--previous <EndOfYXXX folder>]... [--target <save>]... [--in-place]
// CoachRecordFix --new-hires <preseason or offseason save> [--in-place]
//
// CFB 27 credits offensive and defensive coordinators with their team's results, and a coach who changes schools keeps
// his record at the old one as his record at the new one (WinsAtCurrentSchool/LossesAtCurrentSchool aren't reset).
//
// A season's archive folder holds DYNASTY-YXXX (the week 0 save) and DYNASTY-Q5 (the end of season save, or
// DYNASTY-Q5_backup when there is one, since DYNASTY-Q5 may already be the fixed save). What each week 0 coordinator gained
// between the two in the stats below is that season's credit, and the record a coach in his first season at a school had
// there at week 0 came with him; this takes both back out of each target save (the folder's DYNASTY-Q5 by default),
// writing <target>-COACHFIX and a report next to it. A target that already has a report has been fixed and is refused.
// --previous adds earlier seasons whose credit is still in the saves (a season's credit is only taken out of saves from
// after that season ended).
//
// --new-hires fixes the records new hires bring before they show anywhere: in a preseason (or offseason) save, a coach
// who hasn't finished a season at his school gets 0-0 there.
//
// --in-place puts the fixed save where the target was: the original is kept (a DYNASTY-Q5 as DYNASTY-Q5_backup, the end of
// season save a later run reads; any other save in _coachfix_backups next to it) and the report goes with it
// (DYNASTY-Q5-COACHFIX.txt next to a DYNASTY-Q5, else in _coachfix_backups). A backup that's already there must be the
// same save.
//
// Coach rows stay put from week 0 through the offseason but are reused when the next season starts, so a coach is found
// in a target by name, and only where his stats are at least what he had at that season's end.

bool inPlace = args.Contains("--in-place");
var options = args.Where(a => a != "--in-place").ToArray();

if (options.Length == 2 && options[0] == "--new-hires")
{
    return ResetNewHires(Path.GetFullPath(options[1]), inPlace);
}

var folders = new List<string>();
var targets = new List<string>();
for (int i = 0; i < options.Length; i++)
{
    if ((options[i] == "--previous" || options[i] == "--target") && i + 1 < options.Length)
    {
        (options[i] == "--previous" ? folders : targets).Add(Path.GetFullPath(options[++i]));
    }
    else if (folders.Count == 0 && !options[i].StartsWith("--"))
    {
        folders.Insert(0, Path.GetFullPath(options[i]));
    }
    else
    {
        folders.Clear();
        break;
    }
}

if (folders.Count == 0 || folders.Any(f => !Directory.Exists(f)))
{
    Console.Error.WriteLine("usage: CoachRecordFix <EndOfYXXX folder> [--previous <EndOfYXXX folder>]... [--target <save>]... [--in-place]");
    Console.Error.WriteLine("       CoachRecordFix --new-hires <preseason or offseason save> [--in-place]");
    return 1;
}

if (targets.Count == 0)
{
    targets.Add(Path.Combine(folders[0], "DYNASTY-Q5"));
}

if (!CanWriteFixes(targets, inPlace))
{
    return 1;
}

var seasons = new List<SeasonCredit>();
foreach (var folder in folders)
{
    var season = SeasonCredit.Read(folder);
    if (season == null)
    {
        return 1;
    }

    seasons.Add(season);
    Console.WriteLine($"{Path.GetFileName(folder)} (to {season.EndOfSeasonSave}): dynasty year {season.Year}, " +
        $"{season.Coaches.Count(c => c.IsCoordinator)} coordinators credited, {season.Coaches.Count(c => c.CarriesRecord)} new hires carrying a record");
}

foreach (var targetPath in targets)
{
    var target = FranchiseFile.Load(targetPath);
    var (year, stage) = SeasonInfo(target);
    var coaches = target.GetPrimaryTable("Coach")!;
    coaches.ReadRecords();
    var byName = coaches.Records.Where(r => !r.IsEmpty).ToLookup(Name);
    var report = new List<string>();
    int fixedCount = 0, unmatched = 0;
    // coaches whose whole record at their school came from an old job and was taken off: earlier seasons' credit there went with it
    var carriedRemoved = new HashSet<int>();

    // newest season first, so a streak still running through a later season is taken apart in order
    foreach (var season in seasons.OrderByDescending(s => s.Year))
    {
        // the season's credit is in saves from its offseason on
        if (year < season.Year || (year == season.Year && stage != "OffSeason"))
        {
            report.Add($"-- dynasty year {season.Year}: not over in this save; nothing to take back");
            continue;
        }

        report.Add($"-- dynasty year {season.Year} ({Path.GetFileName(season.Folder)})");
        foreach (var credit in season.Coaches)
        {
            // the same row when the season hasn't rolled over, else the coach by name whose stats include the season's
            var candidates = byName[credit.Name]
                .Select(r => (coach: r, stats: CareerStats(target, r)))
                .Where(c => c.stats != null && credit.Fits(c.stats))
                .ToList();
            var match = candidates.FirstOrDefault(c => c.coach.Index == credit.Row && year == season.Year);
            if (match.coach == null && candidates.Count == 1)
            {
                match = candidates[0];
            }

            if (match.coach == null)
            {
                report.Add($"{credit.Name} ({credit.Position}): {(candidates.Count == 0 ? "not found" : $"{candidates.Count} coaches by that name")}; skipped");
                unmatched++;
                continue;
            }

            var changes = credit.TakeBack(match.stats!, atSchool: !carriedRemoved.Contains(match.coach.Index));
            if (credit.CarriesRecord)
            {
                carriedRemoved.Add(match.coach.Index);
            }

            if (changes.Count > 0)
            {
                fixedCount++;
                report.Add($"{credit.Name}, {credit.Position} team {credit.Team} in year {season.Year} (now row {match.coach.Index}): {string.Join(", ", changes)}");
            }
        }
    }

    // coaches hired since (after the end of season save) bring their old records too
    var newHires = IsBetweenSeasons(stage) ? ResetNewHireRecords(target) : new List<string>();
    if (newHires.Count > 0)
    {
        report.Add("-- new hires");
        report.AddRange(newHires);
    }

    var written = WriteFix(target, targetPath, report, inPlace);
    Console.WriteLine($"{Path.GetFileName(targetPath)} (dynasty year {year}, {stage}): records fixed {fixedCount}, skipped {unmatched}, " +
        $"new hires reset {newHires.Count}; {written}");
}

return 0;

static string OutputPath(string target) => target + "-COACHFIX";

// where --in-place keeps the original: DYNASTY-Q5_backup is the end of season save the next run reads
static bool IsEndOfSeasonSave(string target) => string.Equals(Path.GetFileName(target), "DYNASTY-Q5", StringComparison.OrdinalIgnoreCase);

static string BackupsFolder(string target) => Path.Combine(Path.GetDirectoryName(target)!, "_coachfix_backups");

static string BackupPath(string target) =>
    IsEndOfSeasonSave(target) ? target + "_backup" : Path.Combine(BackupsFolder(target), Path.GetFileName(target));

static string ReportPath(string target, bool inPlace) =>
    inPlace && !IsEndOfSeasonSave(target)
        ? Path.Combine(BackupsFolder(target), Path.GetFileName(OutputPath(target)) + ".txt")
        : OutputPath(target) + ".txt";

static bool CanWriteFixes(IEnumerable<string> targets, bool inPlace)
{
    foreach (var path in targets.Where(t => !File.Exists(t)))
    {
        Console.Error.WriteLine($"{path} not found.");
        return false;
    }

    foreach (var output in targets.Select(OutputPath).Where(File.Exists))
    {
        Console.Error.WriteLine($"{output} already exists; not overwriting it.");
        return false;
    }

    // the report stays with the fixed save, so a target with one was fixed already (fixing it again would take the credit twice)
    foreach (var report in targets.SelectMany(t => new[] { ReportPath(t, false), ReportPath(t, true) }).Distinct().Where(File.Exists))
    {
        Console.Error.WriteLine($"{report} exists: that save has been fixed already.");
        return false;
    }

    // a backup that's already there has to be this save, or replacing it would lose the original
    foreach (var target in targets.Where(t => inPlace && File.Exists(BackupPath(t))))
    {
        if (!File.ReadAllBytes(target).AsSpan().SequenceEqual(File.ReadAllBytes(BackupPath(target))))
        {
            Console.Error.WriteLine($"{BackupPath(target)} exists and isn't the same save as {target}; not replacing it.");
            return false;
        }
    }

    return true;
}

/// <summary>
/// Saves the fixed save as &lt;target&gt;-COACHFIX with its report, or with <paramref name="inPlace"/> in the target's place
/// (the original kept as its backup first); returns what was written.
/// </summary>
static string WriteFix(FranchiseFile save, string target, List<string> report, bool inPlace)
{
    var outputPath = OutputPath(target);
    save.Save(outputPath);
    if (!inPlace)
    {
        File.WriteAllLines(ReportPath(target, false), report);
        return $"wrote {outputPath}";
    }

    var backup = BackupPath(target);
    if (!File.Exists(backup))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
        File.Copy(target, backup);
    }

    File.Move(outputPath, target, overwrite: true);
    var reportPath = ReportPath(target, true);
    File.WriteAllLines(reportPath, report);
    return $"fixed {target} in place (original: {backup}; report: {reportPath})";
}

// --new-hires: a coach with no finished season at his school (SeasonsWithTeam 0) has no record there yet. Only before the
// season starts, since in season a first-year coach's games there count.
static int ResetNewHires(string savePath, bool inPlace)
{
    if (!CanWriteFixes(new[] { savePath }, inPlace))
    {
        return 1;
    }

    var save = FranchiseFile.Load(savePath);
    var (year, stage) = SeasonInfo(save);
    if (!IsBetweenSeasons(stage))
    {
        Console.Error.WriteLine($"{Path.GetFileName(savePath)} is in the {stage} stage; new hires are reset in a preseason or offseason save.");
        return 1;
    }

    var report = ResetNewHireRecords(save);
    var written = WriteFix(save, savePath, report, inPlace);
    Console.WriteLine($"{Path.GetFileName(savePath)} (dynasty year {year}, {stage}): {report.Count} new hires reset to 0-0 at their school; {written}");
    return 0;
}

static class Saves
{
    public static string Name(FranchiseFileRecord coach) => $"{coach["FirstName"]} {coach["LastName"]}";

    public static bool IsBetweenSeasons(string stage) => stage == "PreSeason" || stage == "OffSeason";

    /// <summary>In a save between seasons, coaches with no finished season at their school get 0-0 there; returns who changed.</summary>
    public static List<string> ResetNewHireRecords(FranchiseFile save)
    {
        var coaches = save.GetPrimaryTable("Coach")!;
        coaches.ReadRecords();
        var report = new List<string>();
        foreach (var coach in coaches.Records.Where(r => !r.IsEmpty && Convert.ToInt32(r["TeamIndex"]) != CFBPlayer.NoTeamIndex && Convert.ToInt32(r["SeasonsWithTeam"]) == 0))
        {
            var stats = CareerStats(save, coach);
            int wins = stats == null ? 0 : Convert.ToInt32(stats["WinsAtCurrentSchool"]);
            int losses = stats == null ? 0 : Convert.ToInt32(stats["LossesAtCurrentSchool"]);
            if (wins != 0 || losses != 0)
            {
                stats!["WinsAtCurrentSchool"] = 0;
                stats["LossesAtCurrentSchool"] = 0;
                report.Add($"{Name(coach)}, {coach["Position"]} team {coach["TeamIndex"]} (row {coach.Index}): record at school {wins}-{losses} -> 0-0");
            }
        }

        return report;
    }

    public static (int Year, string Stage) SeasonInfo(FranchiseFile file)
    {
        var table = file.GetPrimaryTable("SeasonInfo")!;
        table.ReadRecords();
        var info = table.Records.First(r => !r.IsEmpty);
        return (Convert.ToInt32(info["CurrentYear"]), info["CurrentStage"] as string ?? "");
    }

    public static FranchiseFileRecord? CareerStats(FranchiseFile file, FranchiseFileRecord coach) =>
        coach.GetReferenceDataByKey("CareerStats") is { TableId: > 0 } stats ? file.Follow(stats) : null;
}

/// <summary>
/// What the season credited its week 0 coordinators with (their stats at week 0 and at the end of the season), and the
/// records its new hires brought from their old schools.
/// </summary>
class SeasonCredit
{
    public string Folder { get; private init; } = "";
    public string EndOfSeasonSave { get; private init; } = "";
    public int Year { get; private init; }
    public List<CoachCredit> Coaches { get; } = new();

    public static SeasonCredit? Read(string folder)
    {
        var suffix = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar)).Replace("EndOf", "", StringComparison.OrdinalIgnoreCase);
        var weekZeroPath = Path.Combine(folder, $"DYNASTY-{suffix}");
        // the end of season save as the game left it
        var endOfSeasonPath = Path.Combine(folder, "DYNASTY-Q5_backup");
        if (!File.Exists(endOfSeasonPath))
        {
            endOfSeasonPath = Path.Combine(folder, "DYNASTY-Q5");
        }

        foreach (var path in new[] { weekZeroPath, endOfSeasonPath }.Where(p => !File.Exists(p)))
        {
            Console.Error.WriteLine($"{path} not found.");
            return null;
        }

        var weekZero = FranchiseFile.Load(weekZeroPath);
        var endOfSeason = FranchiseFile.Load(endOfSeasonPath);
        var startCoaches = weekZero.GetPrimaryTable("Coach")!;
        var endCoaches = endOfSeason.GetPrimaryTable("Coach")!;
        startCoaches.ReadRecords();
        endCoaches.ReadRecords();

        var season = new SeasonCredit { Folder = folder, EndOfSeasonSave = Path.GetFileName(endOfSeasonPath), Year = SeasonInfo(weekZero).Year };
        foreach (var start in startCoaches.Records.Where(r => !r.IsEmpty && Convert.ToInt32(r["TeamIndex"]) != CFBPlayer.NoTeamIndex))
        {
            // a coach without a CareerCoachStats row at week 0 gets one when the season first credits him
            var startStats = CareerStats(weekZero, start);
            bool isCoordinator = start["Position"] as string != "HeadCoach";
            if (!isCoordinator && !CoachCredit.CarriedRecord(start, startStats))
            {
                continue;
            }

            var end = start.Index < endCoaches.Records.Count ? endCoaches.Records[start.Index] : null;
            var endStats = end == null || end.IsEmpty || Name(end) != Name(start) ? null : CareerStats(endOfSeason, end);
            if (endStats == null)
            {
                Console.Error.WriteLine($"  {Name(start)} (row {start.Index}) isn't in the end of season save; left out");
                continue;
            }

            season.Coaches.Add(new CoachCredit(start, startStats, endStats, isCoordinator));
        }

        return season;
    }
}

class CoachCredit
{
    // CareerCoachStats counts: what a coordinator gained over the season is taken back
    private static readonly string[] Counts =
    {
        "Wins", "Losses", "WinsAtCurrentSchool", "LossesAtCurrentSchool", "BowlWins", "BowlLosses", "RivalWins", "RivalLosses",
        "Top25Wins", "Top25Losses", "ConfChampWins", "ConfChampLosses", "PlayoffWins", "PlayoffLosses", "NCWins", "NCLosses",
        "NumPrestigeIncreases",
    };
    // streaks: what the season added comes off, unless the streak has since been broken (then it's later seasons' and
    // stays); they start at 0 for a coach whose stats row the season created
    private static readonly string[] Streaks = { "ConfChampWinStreak", "RivalWinStreak" };
    // the last title's year goes back to the week 0 value while it's still the year the season set
    private const string RecentTitle = "RecentYearNCWon";

    private readonly Dictionary<string, int> start = new();
    private readonly Dictionary<string, int> end = new();
    // what comes off each count: a coordinator's gain over the season, plus the record a new hire brought to his school
    private readonly Dictionary<string, int> taken = new();
    private readonly int? startTitleYear;

    public CoachCredit(FranchiseFileRecord coach, FranchiseFileRecord? startStats, FranchiseFileRecord endStats, bool isCoordinator)
    {
        Row = coach.Index;
        Name = $"{coach["FirstName"]} {coach["LastName"]}";
        Position = coach["Position"] as string ?? "";
        Team = Convert.ToInt32(coach["TeamIndex"]);
        IsCoordinator = isCoordinator;
        CarriesRecord = CarriedRecord(coach, startStats);
        foreach (var stat in Counts.Concat(Streaks).Append(RecentTitle))
        {
            start[stat] = startStats == null ? 0 : Convert.ToInt32(startStats[stat]);
            end[stat] = Convert.ToInt32(endStats[stat]);
        }

        foreach (var stat in Counts)
        {
            taken[stat] = IsCoordinator ? Math.Max(0, end[stat] - start[stat]) : 0;
        }

        if (CarriesRecord)
        {
            // he'd finished no season there, so all of it is from his old school
            taken["WinsAtCurrentSchool"] += start["WinsAtCurrentSchool"];
            taken["LossesAtCurrentSchool"] += start["LossesAtCurrentSchool"];
        }

        startTitleYear = startStats == null ? null : start[RecentTitle];
    }

    public int Row { get; }
    public string Name { get; }
    public string Position { get; }
    public int Team { get; }
    public bool IsCoordinator { get; }

    /// <summary>A new hire (no finished season at his school at week 0) with a record there that came from his old job.</summary>
    public bool CarriesRecord { get; }

    public static bool CarriedRecord(FranchiseFileRecord coach, FranchiseFileRecord? stats) =>
        Convert.ToInt32(coach["SeasonsWithTeam"]) == 0 && stats != null &&
        (Convert.ToInt32(stats["WinsAtCurrentSchool"]) != 0 || Convert.ToInt32(stats["LossesAtCurrentSchool"]) != 0);

    private static readonly string[] AtSchool = { "WinsAtCurrentSchool", "LossesAtCurrentSchool" };

    /// <summary>
    /// The stats could be this coach's from then on: counts never go down (except his record at his school, which a later
    /// season's fix may have taken off).
    /// </summary>
    public bool Fits(FranchiseFileRecord stats) => Counts.Except(AtSchool).All(s => Convert.ToInt32(stats[s]) >= end[s]);

    /// <summary>
    /// Takes the season's credit (and a record he brought to the school) out of the coach's stats now; returns what changed.
    /// Without <paramref name="atSchool"/> his record at his school is left alone (a later season took it all off already).
    /// </summary>
    public List<string> TakeBack(FranchiseFileRecord stats, bool atSchool = true)
    {
        var changes = new List<string>();
        void Set(string stat, int value)
        {
            int now = Convert.ToInt32(stats[stat]);
            if (now != value)
            {
                stats[stat] = value;
                changes.Add($"{stat} {now}->{value}");
            }
        }

        foreach (var stat in Counts.Where(s => taken[s] > 0 && (atSchool || !AtSchool.Contains(s))))
        {
            Set(stat, Math.Max(0, Convert.ToInt32(stats[stat]) - taken[stat]));
        }

        if (!IsCoordinator)
        {
            return changes;
        }

        foreach (var stat in Streaks.Where(s => end[s] != start[s]))
        {
            int now = Convert.ToInt32(stats[stat]);
            // still the season's streak (or one that continued from it)
            if (now >= end[stat])
            {
                Set(stat, Math.Max(0, now - (end[stat] - start[stat])));
            }
        }

        if (startTitleYear is int year && end[RecentTitle] != year && Convert.ToInt32(stats[RecentTitle]) == end[RecentTitle])
        {
            Set(RecentTitle, year);
        }

        return changes;
    }
}
