using System.Text;
using CFB27.Data.Model;
using MaddenFranchise;

namespace ScheduleMaker
{
    /// <summary>
    /// The FBS teams in CFP ranking order (Team.CFPPoll_CurrentRank, 1-138) with their season record and conference, marked
    /// "**" for this season's conference champions. CFB 27 only fills in the CFP rank once the CFP poll starts; before that it's
    /// 0 for every team.
    /// </summary>
    internal static class RankingsExporter
    {
        private const int NoTeamIndex = 255;

        /// <summary>Writes the csv; returns how many teams it holds, or 0 if the save has no CFP rankings yet.</summary>
        public static int Write(FranchiseFile dynasty, string path)
        {
            var teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();
            var fbs = teams.Records
                .Where(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) != NoTeamIndex)
                .Select(t => (Row: t.Index, Rank: Convert.ToInt32(t["CFPPoll_CurrentRank"]), Name: t["DisplayName"] as string ?? string.Empty,
                    // the season record, as CFBTeam.Win / Loss add it up
                    Wins: Convert.ToInt32(t["ConfWin"]) + Convert.ToInt32(t["NonConfWin"]),
                    Losses: Convert.ToInt32(t["ConfLoss"]) + Convert.ToInt32(t["NonConfLoss"])))
                .ToList();

            if (fbs.All(t => t.Rank == 0))
            {
                return 0;
            }

            var champions = ConferenceChampions(dynasty);
            var conferences = CFBConference.Read(dynasty)
                .SelectMany(c => c.TeamRows.Select(row => (Row: row, c.Name)))
                .GroupBy(x => x.Row)
                .ToDictionary(g => g.Key, g => g.First().Name ?? string.Empty);

            var csv = new StringBuilder();
            csv.AppendLine("Ranking,Team,Wins,Losses,Conference");
            void AddRows(IEnumerable<(int Row, int Rank, string Name, int Wins, int Losses)> rows)
            {
                foreach (var team in rows.OrderBy(t => t.Rank == 0 ? int.MaxValue : t.Rank).ThenBy(t => t.Name))
                {
                    var conference = (conferences.TryGetValue(team.Row, out var name) ? name : string.Empty) + (champions.Contains(team.Row) ? "**" : string.Empty);
                    csv.AppendLine(string.Join(",", team.Rank, Quote(team.Name), team.Wins, team.Losses, Quote(conference)));
                }
            }

            var bowlEligbleTeams = fbs.Where(t => t.Wins >= 6).ToList();

            if (bowlEligbleTeams.Count < 76)
            {
                bowlEligbleTeams.AddRange(fbs.Where(t => t.Wins == 5));
            }
            AddRows(bowlEligbleTeams);

            File.WriteAllText(path, csv.ToString());
            return fbs.Count;
        }

        /// <summary>
        /// This season's conference champions (team rows), from the season's entry in the league history
        /// (LeagueHistoryManager.LeagueHistory, PeriodIndex = season - first season). It's written once the title games are played.
        /// </summary>
        private static HashSet<int> ConferenceChampions(FranchiseFile dynasty)
        {
            var champions = new HashSet<int>();
            var seasonInfo = dynasty.GetPrimaryTable("SeasonInfo").Records.First(r => !r.IsEmpty);
            int periodIndex = Convert.ToInt32(seasonInfo["CurrentSeasonYear"]) - Convert.ToInt32(seasonInfo["BaseCalendarYear"]);
            var teamTableId = dynasty.GetPrimaryTable("Team", "DisplayName").Header.TableId;

            var manager = dynasty.GetPrimaryTable("LeagueHistoryManager").Records.FirstOrDefault(r => !r.IsEmpty);
            if (manager == null)
            {
                return champions;
            }

            foreach (var season in dynasty.FollowArray(manager, "LeagueHistory").Where(y => Convert.ToInt32(y["PeriodIndex"]) == periodIndex))
            {
                foreach (var champion in dynasty.FollowArray(season, "ConferenceChampions"))
                {
                    var team = champion.GetReferenceDataByKey("WinningTeamIdentity");
                    if (team.HasValue && team.Value.TableId == teamTableId)
                    {
                        champions.Add(team.Value.RowNumber);
                    }
                }
            }

            return champions;
        }

        private static string Quote(string value) =>
            value.IndexOfAny(new[] { ',', '"' }) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }
}
