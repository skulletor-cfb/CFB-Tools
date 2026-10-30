using MaddenFranchise;

namespace BowlEditor
{
    /// <summary>
    /// The week-16 CFP poll (Team.CFPPoll_CurrentRank / CFPPoll_CurrentPoints), which is what decides the playoff: advancing
    /// to bowl week builds the 12-team bracket from it (and re-picks the regular bowls), so the bracket itself can't be
    /// edited, only the poll. Moving a team to a rank shifts the teams in between by one; the points stay with the
    /// positions, so the poll stays in points order.
    /// </summary>
    internal sealed class CfpPoll
    {
        public const int PlayoffTeams = 12;
        private const int NoTeamIndex = 255;
        private const string Rank = "CFPPoll_CurrentRank";
        private const string Points = "CFPPoll_CurrentPoints";

        private readonly FranchiseFileTable teams;
        /// <summary>the poll's points, best first (position i keeps value i)</summary>
        private readonly List<object?> pointsByPosition;

        /// <summary>The ranked FBS teams, in poll order.</summary>
        public List<PollEntry> Entries { get; }

        public CfpPoll(FranchiseFile dynasty)
        {
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();

            Entries = teams.Records
                .Where(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) != NoTeamIndex && Convert.ToInt32(t[Rank]) >= 1)
                .OrderBy(t => Convert.ToInt32(t[Rank]))
                .Select(t => new PollEntry
                {
                    Row = t.Index,
                    Name = t["DisplayName"] as string ?? string.Empty,
                    Record = $"{Convert.ToInt32(t["ConfWin"]) + Convert.ToInt32(t["NonConfWin"])}-{Convert.ToInt32(t["ConfLoss"]) + Convert.ToInt32(t["NonConfLoss"])}",
                    MediaRank = Convert.ToInt32(t["MediaPoll_CurrentRank"]),
                    OriginalRank = Convert.ToInt32(t[Rank]),
                })
                .ToList();
            pointsByPosition = Entries.Select(e => teams.Records[e.Row][Points]).ToList();
        }

        public bool IsChanged => Entries.Select((e, i) => e.OriginalRank != i + 1).Any(changed => changed);

        /// <summary>Puts a team at a rank (1-based), shifting the teams between its old and new rank.</summary>
        public void Move(PollEntry entry, int rank)
        {
            rank = Math.Clamp(rank, 1, Entries.Count);
            Entries.Remove(entry);
            Entries.Insert(rank - 1, entry);
        }

        public int RankOf(PollEntry entry) => Entries.IndexOf(entry) + 1;

        /// <summary>Writes the new order into the save (in memory): rank, and the points of that position. Returns how many teams moved.</summary>
        public int Apply()
        {
            int moved = 0;
            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                if (entry.OriginalRank == i + 1)
                {
                    continue;
                }

                var record = teams.Records[entry.Row];
                record[Rank] = i + 1;
                record[Points] = pointsByPosition[i];
                entry.OriginalRank = i + 1;
                moved++;
            }

            return moved;
        }
    }

    internal sealed class PollEntry
    {
        public int Row { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Record { get; init; } = string.Empty;
        public int MediaRank { get; init; }
        /// <summary>the rank in the save</summary>
        public int OriginalRank { get; set; }
    }
}
