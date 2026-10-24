using System.Text;
using CFB27.Data.Model;
using MaddenFranchise;

namespace ScheduleMaker
{
    /// <summary>
    /// The FBS teams in CFP ranking order (Team.CFPPoll_CurrentRank, 1-138) with their season record and, for conference
    /// champions, the conference they won. CFB 27 only fills in the CFP rank once the CFP poll starts; before that it's 0 for
    /// every team.
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
            var csv = new StringBuilder();
            csv.AppendLine("Ranking,Team,Wins,Losses,ConferenceChampion");
            foreach (var team in fbs.Where(t => t.Wins >= 6).OrderBy(t => t.Rank == 0 ? int.MaxValue : t.Rank).ThenBy(t => t.Name))
            {
                champions.TryGetValue(team.Row, out var conference);
                csv.AppendLine(string.Join(",", team.Rank, Quote(team.Name), team.Wins, team.Losses, Quote(conference ?? string.Empty)));
            }

            foreach (var team in fbs.Where(t => t.Wins == 5).OrderBy(t => t.Rank == 0 ? int.MaxValue : t.Rank).ThenBy(t => t.Name))
            {
                champions.TryGetValue(team.Row, out var conference);
                csv.AppendLine(string.Join(",", team.Rank, Quote(team.Name), team.Wins, team.Losses, Quote(conference ?? string.Empty)));
            }

            File.WriteAllText(path, csv.ToString());
            return fbs.Count;
        }

        /// <summary>
        /// This season's conference champions (team row → conference name), from the season's entry in the league history
        /// (LeagueHistoryManager.LeagueHistory, PeriodIndex = season - first season). It's written once the title games are played.
        /// </summary>
        private static Dictionary<int, string> ConferenceChampions(FranchiseFile dynasty)
        {
            var champions = new Dictionary<int, string>();
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
                        champions[team.Value.RowNumber] = champion["ConferenceName"] as string ?? string.Empty;
                    }
                }
            }

            return champions;
        }

        private static string Quote(string value) =>
            value.IndexOfAny(new[] { ',', '"' }) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }
}
