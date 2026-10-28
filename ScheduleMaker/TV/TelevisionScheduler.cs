#nullable disable
using System.Text.Json;
using CFB27.Data.Model;
using MaddenFranchise;
using MaddenFranchise.Dynasty;
using NCAA14Utility;
using NCAA14Utility.Scheduling;

namespace ScheduleMaker.TV
{
    /// <summary>
    /// Ctrl+V. EADBEditor's NCAA 14 TV scheduler on a CFB 27 preseason schedule: each conference offers its games to the
    /// networks, the networks put them in time slots, and what's left goes to streaming. A slot sets the game's day and its
    /// kickoff, the nearest CFB 27 GameTimeRange (Eastern time). Writes the schedule for the site (tv-schedule.txt and
    /// schedule.html, in last season's national champion's colors, with rivalry names and neutral sites) and the games no
    /// one took (unassigned-games.txt).
    /// </summary>
    public static class TelevisionScheduler
    {
        public const string TVScheduleFile = "tv-schedule.txt";
        public const string ScheduleHtmlFile = "schedule.html";
        public const string UnassignedFile = "unassigned-games.txt";
        private const string TvAllPlaceholder = "{tvall}";
        private const string ScheduleHtmlResource = "ScheduleMaker.TV.tv-schedule.html";
        private const int LastWeek = 13;

        public static Dictionary<string, string> ChannelNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ChannelName.ABC.ToString()] = ChannelName.ABC.ToString(),
            [ChannelName.ACCNetwork.ToString()] = "ACCN",
            [ChannelName.BigTenNetwork.ToString()] = "BTN",
            [ChannelName.CBSSportsNetwork.ToString()] = "CBSSN",
            [ChannelName.CBS.ToString()] = ChannelName.CBS.ToString(),
            [ChannelName.CW.ToString()] = "The CW",
            [ChannelName.ESPN2.ToString()] = ChannelName.ESPN2.ToString(),
            [ChannelName.ESPN.ToString()] = ChannelName.ESPN.ToString(),
            [ChannelName.ESPNU.ToString()] = ChannelName.ESPNU.ToString(),
            [ChannelName.FOX.ToString()] = ChannelName.FOX.ToString(),
            [ChannelName.NBC.ToString()] = ChannelName.NBC.ToString(),
            [ChannelName.FoxSports1.ToString()] = "FS1",
            [ChannelName.SECNetwork.ToString()] = "SECN",
            [StreamingProvider.ESPNPlus.ToString()] = "ESPN+",
            [StreamingProvider.FoxOne.ToString()] = "FOX One",
            [StreamingProvider.ParamountPlus.ToString()] = "Paramount+",
            [StreamingProvider.Peacock.ToString()] = StreamingProvider.Peacock.ToString(),
            [StreamingProvider.MWPlus.ToString()] = "MW+",
            [ChannelName.TheMW.ToString()] = "The Mtn.",
            [StreamingProvider.ACCNX.ToString()] = "ACCNX",
        };

        public static SeasonCalendar CurrentSeason { get; private set; }

        /// <summary>The run's random numbers, seeded by the season so a rerun gives the same schedule.</summary>
        public static Random Random { get; private set; }

        private static Dictionary<ChannelName, ChannelSchedule> AllNetworks = new Dictionary<ChannelName, ChannelSchedule>();
        private static Dictionary<StreamingProvider, StreamingSchedule> AllStreamers = new Dictionary<StreamingProvider, StreamingSchedule>();

        public static Dictionary<int, ConferenceGameSlate> AllGames = null;

        /// <summary>CFB 27's exact kickoffs (T1100 ... T2300), in minutes after midnight.</summary>
        private static List<(string Name, int Minutes)> gameTimes;

        /// <summary>Every GameTimeRange name (exact or a named range such as NIGHT_FIRST) that stands for an exact kickoff, in minutes.</summary>
        private static Dictionary<string, int> minutesByName;

        /// <summary>
        /// Puts the season's games (weeks 0-13) on TV and writes the schedule files to <paramref name="outputDirectory"/>.
        /// Returns a summary.
        /// </summary>
        public static List<string> FixTelevisionSchedule(FranchiseFile dynasty, PreseasonSchedule schedule, int year, string outputDirectory)
        {
            Reset(schedule, year);
            var rivalries = ReadRivalryNames(dynasty, schedule);
            var stadiums = StadiumNames(schedule);
            var champion = LastNationalChampion(dynasty, schedule);
            var fullset = schedule.Games
                .Where(g => g.IsEnabled && g.Week >= 0 && g.Week <= LastWeek && g.HomeTeamRow >= 0 && g.AwayTeamRow >= 0)
                // FCS-vs-FCS placeholders aren't real games
                .Where(g => !(g.HomeTeamRow.IsFcsTeam() && g.AwayTeamRow.IsFcsTeam()))
                .Select(g => new TelevisedGame(g, schedule, rivalries))
                .OrderBy(g => g.Week)
                .ThenBy(g => g.Score)
                .ToArray();
            var daysBefore = fullset.ToDictionary(g => g, g => g.Record.DayOfWeek);

            // we let ND games get evaluated first
            var gamesNeedingAssignment = fullset.Where(g => g.IsNotreDameGame)
                .Concat(fullset.Where(g => !g.IsNotreDameGame))
                .Where(g => g.GameNeedsAssignment())
                .OrderBy(g => g.Week)
                .ThenBy(g => g.Score)
                .ToArray();

            var games = AllGames = gamesNeedingAssignment.Organize();

            // make offers
            int notOffered = ConferenceGameSlateHelper.MakeOffers();

            // assign the games
            CBSNetwork.Instance.AssignGames();
            NBCNetwork.Instance.AssignGames();
            Peacock.Instance.AssignGames();
            CBSSportsNetwork.Instance.AssignGames();
            CWNetwork.Instance.AssignGames();
            MountainWestSportsNetwork.Instance.AssignGames();
            ESPNNetworks.Instance.AssignGames();
            FoxNetworks.Instance.AssignGames();

            // not all games made it to broadcast, so they go back to bigger carriers
            ConferenceGameSlateHelper.DistributeUnassigned();

            // what's left goes to streaming
            CBSNetwork.Instance.AssignStreaming();
            NBCNetwork.Instance.AssignStreaming();
            Peacock.Instance.AssignStreaming();
            CBSSportsNetwork.Instance.AssignStreaming();
            CWNetwork.Instance.AssignStreaming();
            MountainWestSportsNetwork.Instance.AssignStreaming();
            ESPNNetworks.Instance.AssignStreaming();
            FoxNetworks.Instance.AssignStreaming();

            // all the games ordered by week, then day, then time
            var streaming = AllStreamers.SelectMany(s => s.Value.Games.Select(g => new { g.game, g.time, channel = s.Key.ToString() }));
            var tv = AllNetworks.SelectMany(kvp => kvp.Value.Games.Select(g => new { g.game, g.time, channel = kvp.Key.ToString() }));
            var allAssignedGames = tv.Concat(streaming).ToArray();
            var allGames = allAssignedGames
                .OrderBy(t => t.time.Week)
                .ThenBy(t => TimeSlot.DaysFromSaturday(t.time.Day))
                .ThenBy(t => t.time.GTOD)
                .Select(g => new
                {
                    Day = g.time.DayOfWeek,
                    Month = g.time.Month,
                    Date = g.time.DayOfMonth,
                    AwayRank = g.game.AwayRank,
                    HomeRank = g.game.HomeRank,
                    Home = g.game.HomeName,
                    Away = g.game.AwayName,
                    // the kickoff CFB 27 will show (the nearest one it has to the network's slot)
                    Time = g.time.ToGameTimeString(),
                    Channel = ChannelNames[g.channel],
                    Week = g.time.Week,
                    HomeConference = g.game.HomeConference,
                    AwayConference = g.game.AwayConference,
                    Score = g.game.Score,
                    g.game.Rivalry,
                    // where a neutral-site game is played
                    Site = g.game.IsNeutralSite ? stadiums.GetValueOrDefault(g.game.Record.StadiumId, $"Stadium 0x{g.game.Record.StadiumId:X8}") : null,
                }).ToList();

            // what is unassigned
            var set = new HashSet<TelevisedGame>(allAssignedGames.Select(g => g.game));
            var unassigned = fullset.Where(g => !set.Contains(g)).ToList();

            Directory.CreateDirectory(outputDirectory);
            var json = JsonSerializer.Serialize(
                new
                {
                    count = unassigned.Count,
                    unassigned = unassigned.Select(g => new { g.Week, g.Day, g.AwayName, g.HomeName, g.AwayConference, g.HomeConference, g.Score, TimeSlot = g.Record.TimeSlot }),
                }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(outputDirectory, UnassignedFile), json);

            json = JsonSerializer.Serialize(
                new
                {
                    year = CurrentSeason.Year,
                    // the page's colors; none keeps its default green
                    theme = champion == null ? null : TeamTheme.ForTeam(schedule.TeamTable.Records[champion.Value]),
                    themeNote = champion == null ? null : $"Colors of {year - 1} national champion {schedule.TeamName(champion.Value)}",
                    allGames,
                });
            File.WriteAllText(Path.Combine(outputDirectory, TVScheduleFile), json);
            File.WriteAllText(Path.Combine(outputDirectory, ScheduleHtmlFile), ReadScheduleHtml().Replace(TvAllPlaceholder, json));

            var report = new List<string>
            {
                $"{allGames.Count} of {fullset.Length} games on TV or streaming; {unassigned.Count} unassigned (they keep their time).",
                $"{fullset.Count(g => g.Record.DayOfWeek != daysBefore[g])} games moved off their day (Thursday, Friday, MACtion...).",
            };

            if (notOffered > 0)
            {
                report.Add($"{notOffered} games weren't offered to any network.");
            }

            report.Add(champion == null
                ? "No national champion in the league history yet; the page keeps its default colors."
                : $"Page colors: {year - 1} national champion {schedule.TeamName(champion.Value)}.");
            report.Add(string.Join(", ", allGames.GroupBy(g => g.Channel).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
            report.Add($"Wrote {TVScheduleFile}, {ScheduleHtmlFile} and {UnassignedFile} to {outputDirectory}.");
            return report;
        }

        public static bool GameNeedsAssignment(this TelevisedGame game)
        {
            var preassigner = new Func<TelevisedGame, bool>[]
                {
                    ESPNNetworks.Instance.PreassignGame,
                    Big10AssignmentRotator.PreassignGame,
                    CBSSportsNetwork.Instance.PreassignGame,
                    CWNetwork.Instance.PreassignGame,
                    MountainWestSportsNetwork.Instance.PreassignGame,
                    Peacock.Instance.PreassignGame,
                };

            return preassigner.All(f => f(game)) && !(game.KeepsDay && PreassignOnItsDay(game));
        }

        /// <summary>
        /// A week 0-1 game off Saturday that no network's rules took (they can only take it at its own day and kickoff) goes
        /// on the first ESPN channel free then (ESPN+ when they're all busy).
        /// </summary>
        private static bool PreassignOnItsDay(TelevisedGame game)
        {
            var slot = game.OwnSlot;
            var espn = ESPNNetworks.Instance;
            return new[] { espn.ESPN, espn.ESPN2, espn.ESPNU, FoxNetworks.Instance.FS1 }.Any(channel => channel.PreassignGame(game, slot, false)) ||
                espn.Streaming.PreassignGame(game, slot);
        }

        /// <summary>The save's rivalries by team-row pair (Rivalry.Team1 / Team2), with their names.</summary>
        private static Dictionary<(int, int), string> ReadRivalryNames(FranchiseFile dynasty, PreseasonSchedule schedule)
        {
            var rivalries = new Dictionary<(int, int), string>();
            var table = dynasty.GetPrimaryTable("Rivalry");
            if (table == null)
            {
                return rivalries;
            }

            table.ReadRecords();
            int teamTableId = schedule.TeamTable.Header.TableId;
            foreach (var rivalry in table.Records.Where(r => !r.IsEmpty))
            {
                var team1 = rivalry.GetReferenceDataByKey("Team1");
                var team2 = rivalry.GetReferenceDataByKey("Team2");
                if (team1?.TableId == teamTableId && team2?.TableId == teamTableId)
                {
                    int a = team1.Value.RowNumber, b = team2.Value.RowNumber;
                    rivalries.TryAdd((Math.Min(a, b), Math.Max(a, b)), rivalry["Name"] as string ?? "Rivalry");
                }
            }

            return rivalries;
        }

        /// <summary>Stadium names by asset id: the neutral sites, then the teams' homes (Lincoln Financial Field hosts Army-Navy).</summary>
        private static Dictionary<uint, string> StadiumNames(PreseasonSchedule schedule)
        {
            var names = new Dictionary<uint, string>(CFBStadium.NeutralSites);
            foreach (var team in schedule.TeamTable.Records.Where(r => !r.IsEmpty))
            {
                if (team["Stadium"] is string bits && Convert.ToUInt32(bits, 2) is var id && id != 0 &&
                    CFBStadium.FindHome(team["AssetName"] as string, team["ShortName"] as string) is { } home)
                {
                    names.TryAdd(id, home.Name);
                }
            }

            return names;
        }

        /// <summary>
        /// Last season's national champion (Team row): the newest LeagueHistoryManager.LeagueHistory YearSummary before this
        /// season (PeriodIndex = season - BaseCalendarYear) with a winner. Null in a dynasty's first season.
        /// </summary>
        private static int? LastNationalChampion(FranchiseFile dynasty, PreseasonSchedule schedule)
        {
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo")?.Records.FirstOrDefault(r => !r.IsEmpty);
            var manager = dynasty.GetPrimaryTable("LeagueHistoryManager")?.Records.FirstOrDefault(r => !r.IsEmpty);
            if (seasonInfo == null || manager == null)
            {
                return null;
            }

            int thisSeason = Convert.ToInt32(seasonInfo["CurrentSeasonYear"]) - Convert.ToInt32(seasonInfo["BaseCalendarYear"]);
            int teamTableId = schedule.TeamTable.Header.TableId;
            return dynasty.FollowArray(manager, "LeagueHistory")
                .Where(y => Convert.ToInt32(y["PeriodIndex"]) < thisSeason)
                .OrderByDescending(y => Convert.ToInt32(y["PeriodIndex"]))
                .Select(y => y.GetReferenceDataByKey("WinningTeamIdentity"))
                .Where(team => team?.TableId == teamTableId)
                .Select(team => (int?)team.Value.RowNumber)
                .FirstOrDefault();
        }

        /// <summary>The kickoff, in minutes after midnight, of a GameTimeRange name; -1 for the random ranges (RDAY...).</summary>
        public static int Minutes(string gameTimeRange) =>
            gameTimeRange != null && minutesByName.TryGetValue(gameTimeRange, out var minutes) ? minutes : -1;

        /// <summary>The CFB 27 kickoff closest to <paramref name="minutes"/>; ties go to the earlier one.</summary>
        public static (string Name, int Minutes) NearestGameTime(int minutes) =>
            gameTimes.OrderBy(t => Math.Abs(t.Minutes - minutes)).ThenBy(t => t.Minutes).First();

        public static int LaborDayWeek()
        {
            return CurrentSeason.IsLaborDayWeekendFirstWeek ? 0 : 1;
        }

        public static int MACtionStartWeek => LastWeekOfOctober() + 1;

        public static int LastWeekOfOctober()
        {
            for (int i = CurrentSeason.Weeks.Length - 1; i >= 0; i--)
            {
                if (CurrentSeason.IsOctober(i))
                {
                    return i;
                }
            }

            throw new Exception("Bad calendar");
        }

        public static int FirstWeekOfOctober()
        {
            for (int i = 0; i <= LastWeek; i++)
            {
                if (CurrentSeason.IsOctober(i))
                {
                    return i;
                }
            }

            throw new Exception("Bad calendar");
        }

        public static ChannelSchedule Register(this ChannelSchedule schedule)
        {
            AllNetworks[schedule.Name] = schedule;
            return schedule;
        }

        public static StreamingSchedule Register(this StreamingSchedule streamer)
        {
            AllStreamers[streamer.Provider] = streamer;
            return streamer;
        }

        /// <summary>Fresh networks, slates and calendar for a run.</summary>
        private static void Reset(PreseasonSchedule schedule, int year)
        {
            CurrentSeason = new SeasonCalendar(year);
            Random = new Random(year);
            AllNetworks = new Dictionary<ChannelName, ChannelSchedule>();
            AllStreamers = new Dictionary<StreamingProvider, StreamingSchedule>();
            AllGames = null;
            ReadGameTimes(schedule);

            CBSNetwork.Reset();
            NBCNetwork.Reset();
            Peacock.Reset();
            CBSSportsNetwork.Reset();
            CWNetwork.Reset();
            MountainWestSportsNetwork.Reset();
            ESPNNetworks.Reset();
            FoxNetworks.Reset();
            Big10AssignmentRotator.Reset();
            ConferenceGameSlateHelper.Reset();
        }

        /// <summary>
        /// The GameTimeRange enum: exact kickoffs are named T1100 ... T2300, and the named ranges (DAY_LAST, NIGHT_FIRST...)
        /// share their values, so a range read from the save maps back to its kickoff.
        /// </summary>
        private static void ReadGameTimes(PreseasonSchedule schedule)
        {
            var members = schedule.Games[0].Record.Parent.OffsetTable.First(o => o.Name == "TimeOfDayEnum").Enum.Members;
            var exact = members
                .Where(m => m.Name.Length == 5 && m.Name[0] == 'T' && m.Name.Skip(1).All(char.IsDigit))
                .Select(m => (m.Name, m.Value, Minutes: int.Parse(m.Name.Substring(1, 2)) * 60 + int.Parse(m.Name.Substring(3, 2))))
                .ToList();
            gameTimes = exact.Select(t => (t.Name, t.Minutes)).ToList();
            var minutesByValue = exact.ToDictionary(t => t.Value, t => t.Minutes);
            minutesByName = members
                .Where(m => minutesByValue.ContainsKey(m.Value))
                .GroupBy(m => m.Name)
                .ToDictionary(g => g.Key, g => minutesByValue[g.First().Value]);
        }

        private static string ReadScheduleHtml()
        {
            using var stream = typeof(TelevisionScheduler).Assembly.GetManifestResourceStream(ScheduleHtmlResource)
                ?? throw new InvalidOperationException($"{ScheduleHtmlResource} isn't embedded in ScheduleMaker.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }

    public static class Big10AssignmentRotator
    {
        private static Func<TelevisedGame, bool>[] networks;

        private static int idx = 0;

        private static int NextIndex => (idx++ % networks.Length);

        public static void Reset()
        {
            networks = new Func<TelevisedGame, bool>[]
                {
                    FoxNetworks.Instance.PreassignGame,
                    NBCNetwork.Instance.PreassignGame,
                    CBSNetwork.Instance.PreassignGame,
                };
            idx = 0;
        }

        public static bool PreassignGame(TelevisedGame game)
        {
            /// fox friday for the first two weeks gets assigned at 8pm
            if ((game.Week <= 1 && (game.Day == 4 || game.Day == 6)) ||
                game.CheckMatchup(ScheduleBase.Michigan, ScheduleBase.OhioSt))
            {
                return FoxNetworks.Instance.PreassignGame(game);
            }

            // we need to be exhaustive
            return networks[NextIndex](game) &&
                networks[NextIndex](game) &&
                networks[NextIndex](game);
        }
    }
}
