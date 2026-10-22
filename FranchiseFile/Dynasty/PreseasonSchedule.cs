using MaddenFranchise.Util;

namespace MaddenFranchise.Dynasty;

/// <summary>
/// The coming season's schedule while a dynasty is in the preseason.
///
/// Before week 0, the regular-season games aren't in SeasonGame yet. They're a "known games" list:
/// SeasonScheduleManager.ScheduleKnownGames → one ScheduleKnownGames row per season (index 0 = the
/// coming season) → KnownGames → ScheduleKnownGame rows. When the game advances to week 0 it copies
/// that list into SeasonGame (home, away, week, day, time slot, kickoff flag and stadium carry over
/// unchanged), adds the conference-championship placeholders, and empties the list. Editing these
/// rows in a preseason save therefore edits the season the game will build.
/// </summary>
public sealed class PreseasonSchedule
{
    private readonly FranchiseFile _file;
    private readonly FranchiseFileRecord _gameList;
    private readonly FranchiseFileTable _gameTable;
    private readonly List<ScheduledGame> _games;
    private readonly Dictionary<int, string> _conferenceByTeamRow;
    private readonly Dictionary<int, int> _gameCountsAtLoad;
    private readonly Dictionary<(int Team, int Week), int> _bookingsAtLoad;

    public FranchiseFileTable TeamTable { get; }
    public IReadOnlyList<ScheduledGame> Games => _games;

    private PreseasonSchedule(FranchiseFile file, FranchiseFileTable teamTable, FranchiseFileRecord gameList,
        FranchiseFileTable gameTable, List<ScheduledGame> games, Dictionary<int, string> conferenceByTeamRow)
    {
        _file = file;
        TeamTable = teamTable;
        _gameList = gameList;
        _gameTable = gameTable;
        _games = games;
        _conferenceByTeamRow = conferenceByTeamRow;
        _gameCountsAtLoad = CountGamesPerTeam();
        _bookingsAtLoad = CountBookingsPerWeek();
    }

    /// <summary>Reads the coming season's games. Throws if the save is already past the preseason.</summary>
    public static PreseasonSchedule Load(FranchiseFile file)
    {
        var teamTable = file.GetPrimaryTable("Team", "DisplayName")
            ?? throw new InvalidDataException("No Team table with team names was found in this save.");
        teamTable.ReadRecords();

        var manager = FirstRecord(file.GetPrimaryTable("SeasonScheduleManager"))
            ?? throw new InvalidDataException("No SeasonScheduleManager record was found in this save.");
        var seasons = Follow(file, manager.GetFieldByKey("ScheduleKnownGames")?.ReferenceData)
            ?? throw new InvalidDataException("SeasonScheduleManager has no known-games list.");
        var comingSeason = Follow(file, ArrayItems(seasons).FirstOrDefault())
            ?? throw new InvalidDataException("The known-games list has no entry for the coming season.");
        var gameList = Follow(file, comingSeason.GetFieldByKey("KnownGames")?.ReferenceData)
            ?? throw new InvalidDataException("The coming season has no known-games list.");

        var games = ArrayItems(gameList)
            .Select(r => Follow(file, r))
            .Where(r => r is not null && !r.IsEmpty)
            .Select(r => new ScheduledGame(r!, teamTable))
            .ToList();
        if (games.Count == 0)
        {
            throw new InvalidOperationException(
                "The coming season's schedule list is empty. That happens once the dynasty advances to week 0; " +
                "from then on the schedule lives in the SeasonGame table instead.");
        }

        return new PreseasonSchedule(file, teamTable, gameList, games[0].Record.Parent, games,
            ReadConferences(file, teamTable));
    }

    /// <summary>
    /// Adds a regular-season game: claims the games table's next free row (the same row the game
    /// itself would use next) and appends it to the coming season's list. The row is zeroed before
    /// the fields are set, since free rows keep leftover bytes from deleted games.
    /// </summary>
    public ScheduledGame AddGame(int homeTeamRow, int awayTeamRow, int week, string dayOfWeek = "Saturday",
        string timeSlot = "T1200", bool isKickoffGame = false)
    {
        int row = _gameTable.Header.NextRecordToUse;
        if (row >= _gameTable.Header.RecordCapacity)
        {
            throw new InvalidOperationException($"The games table has no free rows (all {_gameTable.Header.RecordCapacity} are in use).");
        }
        int slot = _gameList.ArraySize ?? 0;
        if (slot >= _gameList.FieldsArray.Count)
        {
            throw new InvalidOperationException($"The season's game list is full ({_gameList.FieldsArray.Count} games).");
        }

        var record = _gameTable.Records[row];
        var firstWord = _gameTable.OffsetTable.FirstOrDefault(o => o.IndexOffset < 32)
            ?? throw new InvalidOperationException("Unexpected games-table layout: no field in the row's first 4 bytes.");
        // Writing a field that overlaps the free-list pointer is what takes a row off the free list
        // (FranchiseFileTable.OnEvent), so this must happen before anything else.
        record[firstWord.Name] = BinaryUtil.GetBinaryReferenceData(0, 0);
        Array.Clear(record.TableDataArray, record.ByteOffset, record.ByteLength);
        record.ClearCachedFieldValues();

        var game = new ScheduledGame(record, TeamTable);
        record["Stadium"] = BinaryUtil.GetBinaryReferenceData(0, 0);
        game.SetHomeTeam(homeTeamRow);
        game.SetAwayTeam(awayTeamRow);
        record["IsEnabled"] = true;
        record["IsKickoffGame"] = isKickoffGame;
        record["DayOfWeek"] = dayOfWeek;
        record["SeasonWeek"] = week;
        record["SeasonWeekType"] = "RegularSeason";
        record["TimeOfDayEnum"] = timeSlot;

        _gameList.FieldsArray[slot].Value = BinaryUtil.GetBinaryReferenceData(_gameTable.Header.TableId, row);
        _games.Add(game);
        return game;
    }

    /// <summary>Days a game can be played on (the DayOfWeek enum, Sunday ... Saturday).</summary>
    public IReadOnlyList<string> DayOptions => EnumOptions("DayOfWeek");

    /// <summary>Kickoff slots (the GameTimeRange enum): exact times such as T1200 and ranges such as NIGHT_FIRST.</summary>
    public IReadOnlyList<string> TimeSlotOptions => EnumOptions("TimeOfDayEnum");

    private IReadOnlyList<string> EnumOptions(string field)
    {
        var members = _gameTable.OffsetTable.FirstOrDefault(o => o.Name == field)?.Enum?.Members;
        // skip markers like First_ and the Invalid/INVALID placeholders
        return members?
            .Where(m => !m.Name.EndsWith("_") && !string.Equals(m.Name, "Invalid", StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Name)
            .ToList() ?? new List<string>();
    }

    public string? TeamName(int teamRow) =>
        teamRow >= 0 && teamRow < TeamTable.Records.Count ? TeamTable.Records[teamRow]["DisplayName"] as string : null;

    /// <summary>Team row for a display name such as "Iowa State", or null if no team has that name.</summary>
    public int? FindTeam(string displayName) =>
        TeamTable.Records.FirstOrDefault(r => !r.IsEmpty && string.Equals(r["DisplayName"] as string, displayName, StringComparison.OrdinalIgnoreCase))?.Index;

    /// <summary>The team's conference name ("Big 12", "Independent"...), or null for teams outside the conference lists.</summary>
    public string? ConferenceOf(int teamRow) => _conferenceByTeamRow.TryGetValue(teamRow, out var c) ? c : null;

    public IEnumerable<ScheduledGame> GamesFor(int teamRow) =>
        Games.Where(g => g.HomeTeamRow == teamRow || g.AwayTeamRow == teamRow).OrderBy(g => g.Week);

    public bool IsConferenceGame(ScheduledGame game)
    {
        var home = ConferenceOf(game.HomeTeamRow);
        return home is not null && home != "Independent" && home == ConferenceOf(game.AwayTeamRow);
    }

    /// <summary>
    /// Problems introduced since the schedule was loaded: a team booked more times in a week than it
    /// was, or a team whose number of games changed. Empty when the edits look consistent. (The
    /// game's generic FCS opponents legitimately appear several times in one week, so this compares
    /// against the loaded schedule rather than requiring one game per team per week.)
    /// </summary>
    public List<string> Validate()
    {
        var problems = new List<string>();
        var bookingsNow = CountBookingsPerWeek();
        foreach (var entry in bookingsNow)
        {
            if (entry.Value > Math.Max(1, _bookingsAtLoad.GetValueOrDefault(entry.Key)))
            {
                problems.Add($"{TeamName(entry.Key.Team)} is scheduled {entry.Value} times in week {entry.Key.Week}.");
            }
        }
        var countsNow = CountGamesPerTeam();
        foreach (var team in countsNow.Keys.Union(_gameCountsAtLoad.Keys))
        {
            int before = _gameCountsAtLoad.GetValueOrDefault(team);
            int now = countsNow.GetValueOrDefault(team);
            if (now != before)
            {
                problems.Add($"{TeamName(team)} now has {now} games (had {before}).");
            }
        }
        return problems;
    }

    private Dictionary<(int Team, int Week), int> CountBookingsPerWeek() =>
        Games.Where(g => g.IsEnabled)
            .SelectMany(g => new[] { (Team: g.HomeTeamRow, g.Week), (Team: g.AwayTeamRow, g.Week) })
            .Where(x => x.Team >= 0)
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

    private Dictionary<int, int> CountGamesPerTeam() =>
        Games.Where(g => g.IsEnabled)
            .SelectMany(g => new[] { g.HomeTeamRow, g.AwayTeamRow })
            .Where(t => t >= 0)
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());

    private static Dictionary<int, string> ReadConferences(FranchiseFile file, FranchiseFileTable teamTable)
    {
        var result = new Dictionary<int, string>();
        var conferences = file.GetPrimaryTable("Conference", "TeamSlots");
        if (conferences is null)
        {
            return result;
        }
        conferences.ReadRecords();
        foreach (var conference in conferences.Records.Where(r => !r.IsEmpty))
        {
            var slots = Follow(file, conference.GetFieldByKey("TeamSlots")?.ReferenceData);
            if (slots is null || conference["Name"] is not string name || name.Length == 0)
            {
                continue;
            }
            foreach (var team in ArrayItems(slots).Where(r => r.TableId == teamTable.Header.TableId))
            {
                result[team.RowNumber] = name;
            }
        }
        return result;
    }

    private static FranchiseFileRecord? FirstRecord(FranchiseFileTable? table)
    {
        table?.ReadRecords();
        return table?.Records.FirstOrDefault(r => !r.IsEmpty);
    }

    internal static FranchiseFileRecord? Follow(FranchiseFile file, RecordReference? reference)
    {
        if (reference is not { TableId: > 0 } r)
        {
            return null;
        }
        var table = file.GetTableById(r.TableId);
        table?.ReadRecords();
        return table is not null && r.RowNumber < table.Records.Count ? table.Records[r.RowNumber] : null;
    }

    /// <summary>The live references held by a row of an array table (Player[], ScheduleKnownGame[]...).</summary>
    internal static IEnumerable<RecordReference> ArrayItems(FranchiseFileRecord arrayRow)
    {
        int size = arrayRow.ArraySize ?? arrayRow.FieldsArray.Count;
        foreach (var field in arrayRow.FieldsArray.Take(size))
        {
            if (field.ReferenceData is { TableId: > 0 } r)
            {
                yield return r;
            }
        }
    }
}

/// <summary>One game in the preseason known-games list (a ScheduleKnownGame row).</summary>
public sealed class ScheduledGame
{
    private readonly FranchiseFileTable _teamTable;

    internal ScheduledGame(FranchiseFileRecord record, FranchiseFileTable teamTable)
    {
        Record = record;
        _teamTable = teamTable;
    }

    public FranchiseFileRecord Record { get; }

    /// <summary>Row in the Team table, or -1 when the slot is empty or points elsewhere.</summary>
    public int HomeTeamRow => TeamRow("HomeTeam");
    public int AwayTeamRow => TeamRow("AwayTeam");
    /// <summary>Regular-season week, 0-14.</summary>
    public int Week
    {
        get => Convert.ToInt32(Record["SeasonWeek"]);
        set => Record["SeasonWeek"] = value;
    }

    public string? WeekType => Record["SeasonWeekType"] as string;

    /// <summary>A <see cref="PreseasonSchedule.DayOptions"/> value, e.g. "Saturday".</summary>
    public string? DayOfWeek
    {
        get => Record["DayOfWeek"] as string;
        set => Record["DayOfWeek"] = value;
    }

    /// <summary>Kickoff slot such as "T1200", "NIGHT_FIRST" or "DAY_LAST" (<see cref="PreseasonSchedule.TimeSlotOptions"/>).</summary>
    public string? TimeSlot
    {
        get => Record["TimeOfDayEnum"] as string;
        set => Record["TimeOfDayEnum"] = value;
    }

    public bool IsKickoffGame
    {
        get => Record["IsKickoffGame"] is true;
        set => Record["IsKickoffGame"] = value;
    }

    public bool IsEnabled => Record["IsEnabled"] is true;

    /// <summary>
    /// The stadium's 32-bit asset reference (the same ids Team.Stadium uses). 0 means the home team's stadium, which is
    /// how almost every game is stored; a neutral-site game names its stadium.
    /// </summary>
    public uint StadiumId
    {
        get => Record["Stadium"] is string bits ? Convert.ToUInt32(bits, 2) : 0;
        set => Record["Stadium"] = BinaryUtil.GetBinaryReferenceData((int)(value >> 17), (int)(value & 0x1FFFF));
    }

    public void SetHomeTeam(int teamRow) => Record["HomeTeam"] = BinaryUtil.GetBinaryReferenceData(_teamTable.Header.TableId, teamRow);
    public void SetAwayTeam(int teamRow) => Record["AwayTeam"] = BinaryUtil.GetBinaryReferenceData(_teamTable.Header.TableId, teamRow);

    /// <summary>Swaps which team hosts the game.</summary>
    public void FlipHomeAndAway()
    {
        int home = HomeTeamRow, away = AwayTeamRow;
        SetHomeTeam(away);
        SetAwayTeam(home);
    }

    private int TeamRow(string field) =>
        Record.GetFieldByKey(field)?.ReferenceData is { } r && r.TableId == _teamTable.Header.TableId ? r.RowNumber : -1;

    public override string ToString()
    {
        string Name(int row) => row >= 0 ? _teamTable.Records[row]["DisplayName"] as string ?? $"Team {row}" : "TBD";
        return $"Week {Week} {DayOfWeek} {TimeSlot}: {Name(AwayTeamRow)} at {Name(HomeTeamRow)}";
    }
}
