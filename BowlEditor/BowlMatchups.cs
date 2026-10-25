using MaddenFranchise;
using MaddenFranchise.Util;
using System.Linq;

namespace BowlEditor
{
    /// <summary>
    /// The bowl games whose teams can still be changed: the regular bowls of the first two bowl weeks, from the current
    /// week on. A matchup is only SeasonGame.HomeTeam / AwayTeam (nothing else in the save names the teams). The CFP
    /// games are listed but not editable: the game builds the bracket from the week-16 CFP poll (see <see cref="CfpPoll"/>).
    /// <para>
    /// Edit in bowl week 1 (week 17). The matchups seen at week 16 are only projections: advancing re-picks every bowl
    /// from the standings, tie-ins and CFP rankings, then simulates the new week's games straight away. Those results
    /// aren't posted (records, season stats) until the next advance; until then a game's stats hang off its
    /// Home/AwayTeamStatCache, Home/AwayPlayerStatCache, ScoringSummaries and InjuryCache. So an edited game that was
    /// already simulated is put back to Unplayed with those links cleared (the old result is never posted), and the next
    /// advance simulates it like the user's own unplayed game. Games that aren't changed keep their result. (MyDynasty
    /// wrote a final score instead, so the game never simulated its edited bowls and they kept no box score or stats.)
    /// </para>
    /// <para>
    /// The CFP bracket is the BowlGame rows with IsPlayoffBowl (PlayoffBracketSlot 0-3 first round, 4-7 quarterfinals,
    /// 8-9 semifinals, 10 title game), whose Conference1Rank / Conference2Rank hold the 0-based seeds. Seeds 1-4 wait at
    /// home in quarterfinals 4-7 for the winner of first-round slot 0-3. Nothing stores which team holds a seed: the
    /// bracket screen seeds the field from the CFP rankings (Team.CFPPoll_CurrentRank) each time it opens.
    /// </para>
    /// </summary>
    internal sealed class BowlMatchups
    {
        private const int NoTeamIndex = 255;
        private const int FirstBowlWeek = 17;
        private const int LastEditableWeek = 18;
        private const int FirstQuarterfinalSlot = 4;
        private const string NoRef = "00000000000000000000000000000000";

        /// <summary>A simulated game's links to its not-yet-posted results.</summary>
        private static readonly string[] ResultLinks =
            { "HomeTeamStatCache", "AwayTeamStatCache", "HomePlayerStatCache", "AwayPlayerStatCache", "ScoringSummaries", "InjuryCache" };

        private static readonly string[] Scores =
        {
            "HomeScore", "AwayScore", "HomeScoreOT", "AwayScoreOT",
            "HomeScoreQuarter1", "HomeScoreQuarter2", "HomeScoreQuarter3", "HomeScoreQuarter4",
            "AwayScoreQuarter1", "AwayScoreQuarter2", "AwayScoreQuarter3", "AwayScoreQuarter4",
        };

        private static readonly string[] MonthNames =
            { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        private readonly FranchiseFileTable teams;
        private readonly FranchiseFileTable seasonGames;

        public List<BowlSlot> Games { get; } = new List<BowlSlot>();

        /// <summary>FBS teams, by name.</summary>
        public List<TeamChoice> Teams { get; } = new List<TeamChoice>();

        /// <summary>Why nothing can be edited (bowl week already under way, or no bowls yet); null when the games are editable.</summary>
        public string? NotEditableReason { get; }

        public BowlMatchups(FranchiseFile dynasty)
        {
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();
            var bowls = dynasty.GetPrimaryTable("BowlGame");
            bowls.ReadRecords();
            seasonGames = dynasty.GetPrimaryTable("SeasonGame");
            seasonGames.ReadRecords();
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            int currentWeek = Convert.ToInt32(seasonInfo["CurrentWeek"]);

            var allTeams = teams.Records.Where(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) != NoTeamIndex).ToArray();

            // only bowl eligble teams
            var eligbleTeams = allTeams
                .Where(t => (Convert.ToInt32(t["ConfWin"]) + Convert.ToInt32(t["NonConfWin"])) >= 6)
                .ToList();

            // if we dont have enough we need those with 5 wins
            if (eligbleTeams.Count < 76)
            {
                eligbleTeams.AddRange(allTeams.Where(t => (Convert.ToInt32(t["ConfWin"]) + Convert.ToInt32(t["NonConfWin"])) == 5));
            }

            foreach (var team in eligbleTeams)
            {
                int wins = Convert.ToInt32(team["ConfWin"]) + Convert.ToInt32(team["NonConfWin"]);
                int losses = Convert.ToInt32(team["ConfLoss"]) + Convert.ToInt32(team["NonConfLoss"]);
                int cfp = Convert.ToInt32(team["CFPPoll_CurrentRank"]);
                var name = team["DisplayName"] as string ?? string.Empty;
                Teams.Add(new TeamChoice(team.Index, name, $"{name} ({wins}-{losses}{(cfp >= 1 && cfp <= 25 ? $", CFP #{cfp}" : string.Empty)})"));
            }

            Teams.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            // earlier weeks' results are already posted
            var postseason = seasonGames.Records
                .Where(g => !g.IsEmpty && Convert.ToInt32(g["SeasonWeek"]) is int week && week >= Math.Max(FirstBowlWeek, currentWeek) && week <= LastEditableWeek)
                .ToList();
            if (currentWeek < FirstBowlWeek)
            {
                NotEditableReason = $"This save is from week {currentWeek}. The bowl matchups you see before bowl week are projections: " +
                    "advancing to bowl week picks every bowl again (and the CFP field from the CFP poll), undoing any change. " +
                    "Advance to bowl week 1 (week 17), save, and edit that save.";
            }
            else if (currentWeek > LastEditableWeek)
            {
                NotEditableReason = "The bowls and the CFP first round have already been played.";
            }

            // the seeds of each first-round game, for naming the quarterfinal it feeds
            var firstRoundSeeds = bowls.Records
                .Where(b => !b.IsEmpty && Convert.ToBoolean(b["IsPlayoffBowl"]) && Convert.ToInt32(b["PlayoffBracketSlot"]) < FirstQuarterfinalSlot)
                .GroupBy(b => Convert.ToInt32(b["PlayoffBracketSlot"]))
                .ToDictionary(g => g.Key, g => Seeds(g.First(), quarterfinal: false));

            foreach (var game in postseason)
            {
                var bowlRef = game.GetReferenceDataByKey("BowlGame");
                if (bowlRef?.TableId != bowls.Header.TableId)
                {
                    continue;
                }

                var bowl = bowls.Records[bowlRef.Value.RowNumber];
                bool playoff = Convert.ToBoolean(bowl["IsPlayoffBowl"]);
                int slot = playoff ? Convert.ToInt32(bowl["PlayoffBracketSlot"]) : -1;
                int? home = TeamRow(game, "HomeTeam");
                int? away = TeamRow(game, "AwayTeam");

                // a quarterfinal waits for its first-round winner; any other game without both teams isn't set yet
                bool quarterfinal = playoff && slot >= FirstQuarterfinalSlot;
                if (home == null || (away == null && !quarterfinal))
                {
                    continue;
                }

                int month = Convert.ToInt32(game["GameDateMonth"]);
                Games.Add(new BowlSlot(game)
                {
                    Week = Convert.ToInt32(game["SeasonWeek"]),
                    Date = $"{game["DayOfWeek"]?.ToString()?.Substring(0, 3)} {(month >= 1 && month <= 12 ? MonthNames[month] : "?")} {game["GameDateDay"]}",
                    SortKey = ((month < 7 ? month + 12 : month) * 100 + Convert.ToInt32(game["GameDateDay"])) * 10000 + Convert.ToInt32(game["TimeOfDay"]),
                    Bowl = bowl["Name"] as string ?? string.Empty,
                    PlayoffSlot = slot,
                    Seeds = playoff ? Seeds(bowl, quarterfinal) : string.Empty,
                    WasSimulated = Played(game),
                    OriginalHome = home,
                    OriginalAway = away,
                    Home = home,
                    Away = away,
                    // the bracket comes from the CFP poll; shown, not edited
                    Locked = playoff,
                    // quarterfinal slot 4-7 gets the winner of first-round slot 0-3
                    AwayNote = quarterfinal ? $"Winner of {(firstRoundSeeds.TryGetValue(slot - FirstQuarterfinalSlot, out var fed) ? fed : "the first round")}" : null,
                });
            }

            Games.Sort((a, b) => a.Week != b.Week ? a.Week.CompareTo(b.Week)
                : a.IsPlayoff != b.IsPlayoff ? a.IsPlayoff.CompareTo(b.IsPlayoff)
                : a.IsPlayoff ? a.PlayoffSlot.CompareTo(b.PlayoffSlot)
                : a.SortKey.CompareTo(b.SortKey));
        }

        public string TeamName(int? row) => row == null ? string.Empty : teams.Records[row.Value]["DisplayName"] as string ?? "?";

        /// <summary>Teams in more than one game, or on both sides of one.</summary>
        public List<string> Problems()
        {
            var problems = new List<string>();
            foreach (var game in Games.Where(g => g.Home != null && g.Home == g.Away))
            {
                problems.Add($"{game.Bowl}: {TeamName(game.Home)} can't play itself.");
            }

            var byTeam = Games
                .SelectMany(g => new[] { (Row: g.Home, Game: g), (Row: g.Away, Game: g) })
                .Where(x => x.Row != null)
                .GroupBy(x => x.Row!.Value)
                .Where(group => group.Select(x => x.Game).Distinct().Count() > 1);
            foreach (var group in byTeam)
            {
                problems.Add($"{TeamName(group.Key)} is in {string.Join(" and ", group.Select(x => x.Game.Bowl).Distinct())}.");
            }

            return problems;
        }

        /// <summary>Teams that had a game in these weeks and no longer do.</summary>
        public List<string> TeamsLeftOut()
        {
            var now = Games.SelectMany(g => new[] { g.Home, g.Away }).Where(r => r != null).ToHashSet();
            return Games.SelectMany(g => new[] { g.OriginalHome, g.OriginalAway })
                .Where(r => r != null && !now.Contains(r))
                .Select(r => TeamName(r))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Puts a team on one side of a game. If the team was already in another editable game (or on the other side of
        /// this one), the team it replaces takes its old place, so the two swap; returns that other game, or null. A CFP
        /// team can't move, so picking one is left as a conflict.
        /// </summary>
        public BowlSlot? Assign(BowlSlot game, bool home, int? team)
        {
            var replaced = home ? game.Home : game.Away;
            BowlSlot? swapped = null;
            if (team != null && replaced != null && team != replaced)
            {
                foreach (var other in Games.Where(g => !g.Locked))
                {
                    if (other.Home == team && !(other == game && home))
                    {
                        other.Home = replaced;
                        swapped = other;
                        break;
                    }

                    if (other.Away == team && !(other == game && !home))
                    {
                        other.Away = replaced;
                        swapped = other;
                        break;
                    }
                }
            }

            if (home)
            {
                game.Home = team;
            }
            else
            {
                game.Away = team;
            }

            return swapped;
        }

        /// <summary>
        /// Writes the changed matchups into the save (in memory), putting an already simulated one back to Unplayed; returns
        /// how many games changed.
        /// </summary>
        public int Apply()
        {
            int changed = 0;
            foreach (var game in Games.Where(g => g.IsChanged && !g.Locked))
            {
                game.Record["HomeTeam"] = BinaryUtil.GetBinaryReferenceData(teams.Header.TableId, game.Home!.Value);
                game.Record["AwayTeam"] = BinaryUtil.GetBinaryReferenceData(teams.Header.TableId, game.Away!.Value);
                game.Record["IsRematch"] = MetBefore(game.Home.Value, game.Away.Value);
                if (game.WasSimulated)
                {
                    ResetToUnplayed(game.Record);
                    game.WasSimulated = false;
                }

                game.OriginalHome = game.Home;
                game.OriginalAway = game.Away;
                changed++;
            }

            return changed;
        }

        /// <summary>
        /// Back to the state of a game the week hasn't played yet (as the user's own game is): no result, not simulated, and
        /// no links to the old result, which is then never posted.
        /// </summary>
        private static void ResetToUnplayed(FranchiseFileRecord game)
        {
            game["GameStatus"] = "Unplayed";
            game["IsSimmed"] = false;
            game["IsOvertimeGame"] = false;
            game["ForceWin"] = "None";
            foreach (var field in Scores)
            {
                game[field] = 0;
            }

            foreach (var field in ResultLinks)
            {
                game[field] = NoRef;
            }
        }

        /// <summary>Whether the two teams already played each other this season (SeasonGame.IsRematch).</summary>
        private bool MetBefore(int a, int b) => seasonGames.Records.Any(g => !g.IsEmpty && Convert.ToInt32(g["SeasonWeek"]) < FirstBowlWeek && Played(g)
            && TeamRow(g, "HomeTeam") is int home && TeamRow(g, "AwayTeam") is int away && ((home == a && away == b) || (home == b && away == a)));

        private static bool Played(FranchiseFileRecord game) =>
            game["GameStatus"] as string is "HomeWon" or "AwayWon";

        private int? TeamRow(FranchiseFileRecord game, string field)
        {
            var reference = game.GetReferenceDataByKey(field);
            return reference?.TableId == teams.Header.TableId ? reference.Value.RowNumber : null;
        }

        /// <summary>The seeds a playoff game is for (Conference1Rank / Conference2Rank hold them 0-based).</summary>
        private static string Seeds(FranchiseFileRecord bowl, bool quarterfinal)
        {
            int high = Convert.ToInt32(bowl["Conference1Rank"]) + 1;
            int low = Convert.ToInt32(bowl["Conference2Rank"]) + 1;
            return quarterfinal ? $"#{high} seed" : $"#{high} vs #{low}";
        }
    }

    internal sealed class BowlSlot
    {
        public BowlSlot(FranchiseFileRecord record)
        {
            Record = record;
        }

        public FranchiseFileRecord Record { get; }
        public int Week { get; init; }
        public string Date { get; init; } = string.Empty;
        public int SortKey { get; init; }
        public string Bowl { get; init; } = string.Empty;
        /// <summary>PlayoffBracketSlot, -1 for a bowl</summary>
        public int PlayoffSlot { get; init; }
        public bool IsPlayoff => PlayoffSlot >= 0;
        public string Seeds { get; init; } = string.Empty;
        /// <summary>already simulated this week (results not yet posted); an edit puts it back to Unplayed</summary>
        public bool WasSimulated { get; set; }
        public int? OriginalHome { get; set; }
        public int? OriginalAway { get; set; }
        public int? Home { get; set; }
        public int? Away { get; set; }
        /// <summary>a CFP game: its teams come from the CFP poll (a quarterfinal's visitor is the first-round winner)</summary>
        public bool Locked { get; init; }
        public string? AwayNote { get; init; }
        public bool IsChanged => Home != OriginalHome || Away != OriginalAway;
    }

    internal sealed record TeamChoice(int Row, string Name, string Label);
}
