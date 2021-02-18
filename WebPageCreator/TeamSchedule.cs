using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Configuration;

namespace EA_DB_Editor
{
    public class TeamSchedule : Dictionary<int, List<Game>>
    {
        private static bool opponentMetricsDone = false;
        public static Dictionary<int, TeamSchedule> TeamSchedules;
        public int OpponentWin { get; set; }
        public int OpponentLoss { get; set; }
        public int AverageOpponentRank { get; set; }
        public int TopLvl1Wins { get; set; }
        public int TopLvl1Losses { get; set; }
        public int TopLvl2Wins { get; set; }
        public int TopLvl2Losses { get; set; }
        public int TopLvl3Wins { get; set; }
        public int TopLvl3Losses { get; set; }
        public double BestWinsAvgRank { get; set; }
        public double OpponentRank { get; set; }
        public int OpponentWinPct { get; set; }
        public int TeamId { get; set; }
        public Team Team { get { return Team.Teams[this.TeamId]; } }
        private IEnumerable<Game> flattenedListOfGames;

        public TeamSchedule()
        {
        }

        public static void Create(MaddenDatabase db)
        {
            if (TeamSchedules != null)
                return;

            TeamSchedules = new Dictionary<int, TeamSchedule>();
            TeamSchedule teamSchedule;

            var table = db.lTables[113];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var teamId = table.lRecords[i].lEntries[2].Data.ToInt32().GetRealTeamId();

                // get or create team schedule
                if (TeamSchedules.TryGetValue(teamId, out teamSchedule) == false)
                {
                    teamSchedule = new TeamSchedule();
                    teamSchedule.TeamId = teamId;
                    TeamSchedules[teamId] = teamSchedule;
                }

                // now add to a team schedule
                var game = new Game
                {
                    IsHomeGame = table.lRecords[i].lEntries[0].Data.ToInt32() > 0,
                    OpponentId = table.lRecords[i].lEntries[1].Data.ToInt32().GetRealTeamId(),
                    GameNumber = table.lRecords[i].lEntries[3].Data.ToInt32(),
                    Week = table.lRecords[i].lEntries[4].Data.ToInt32(),
                    TeamId = teamId
                };

                List<Game> gamesForWeek;
                if (!teamSchedule.TryGetValue(game.Week, out gamesForWeek))
                {
                    gamesForWeek = new List<Game>();
                    teamSchedule.Add(game.Week, gamesForWeek);
                }

                gamesForWeek.Add(game);
            }
        }

        public static void CalculateOpponentMetrics(MaddenDatabase db, bool isPreseason)
        {
            TeamSchedule.Create(db);
            Team.Create(db,isPreseason);

            if (opponentMetricsDone)
                return;

            foreach (var schedule in TeamSchedules.Values)
            {
                schedule.CalculateOpponentMetrics(isPreseason);
            }

            opponentMetricsDone = true;
        }

        public IEnumerable<Game> FlattenedListOfGames
        {
            get
            {
                if (this.flattenedListOfGames == null)
                {
                    this.flattenedListOfGames = this.Values.SelectMany(log => log.ToArray());
                }

                return this.flattenedListOfGames;
            }
        }

        public void CalculateOpponentMetrics(bool isPreseason)
        {
            int rankUnits = 0;
            List<double> highestRankedGames = new List<double>();
            this.OpponentLoss = 0;
            this.OpponentWin = 0;
            this.AverageOpponentRank = 0;
            this.TopLvl1Wins = 0;
            this.TopLvl1Losses = 0;
            this.TopLvl2Wins = 0;
            this.TopLvl2Losses = 0;
            this.TopLvl3Wins = 0;
            this.TopLvl3Losses = 0;
            this.BestWinsAvgRank = 0;

            foreach (var game in this.FlattenedListOfGames.Where(g => g.OpponentId != 1023))
            {
                this.OpponentRank = 0;

                if (!game.OpponentId.IsFCS())
                {
                    this.OpponentLoss += game.Opponent.Loss;
                    this.OpponentWin += game.Opponent.Win;
                }

                if (game.Opponent.CoachesPollRank < 127)
                {
                    this.OpponentRank = (game.Opponent.MediaPollRank + game.Opponent.BCSRank + game.Opponent.CoachesPollRank) / 3.0;
                    this.AverageOpponentRank += game.Opponent.MediaPollRank + game.Opponent.BCSRank + game.Opponent.CoachesPollRank;
                    rankUnits += 3;
                }

                //Top 15 Record
                if ((this.OpponentRank <= Convert.ToDouble(ConfigurationManager.AppSettings["Level1TopRanking"])) && game.Opponent.CoachesPollRank < 127)
                {
                    if (game.DidIWin == true) { this.TopLvl1Wins++; }
                    else { if (game.Score != "0-0") { this.TopLvl1Losses++; } }
                }

                //Top 25 Record
                if ((this.OpponentRank <= Convert.ToDouble(ConfigurationManager.AppSettings["Level2TopRanking"])) && game.Opponent.CoachesPollRank < 127 )
                {
                    if (game.DidIWin == true) { this.TopLvl2Wins++; }
                    else { if (game.Score != "0-0") { this.TopLvl2Losses++; } }
                }

                //Top 45 Record
                if ((this.OpponentRank <= Convert.ToDouble(ConfigurationManager.AppSettings["Level3TopRanking"])) && game.Opponent.CoachesPollRank < 127)
                {
                    if (game.DidIWin == true) { this.TopLvl3Wins++; }
                    else { if (game.Score != "0-0") { this.TopLvl3Losses++; } }
                }

                //Best 6 Wins Avg Ranking
                if (game.DidIWin == true && OpponentRank > 0)
                {
                    if (game.Score != "0-0") { highestRankedGames.Add(this.OpponentRank); }
                }
            }

            // no games played yet in preseason
            if (!isPreseason)
            {
                this.OpponentWinPct = 1000 * this.OpponentWin / (this.OpponentWin + this.OpponentLoss);
            }

            this.AverageOpponentRank = (10 * this.AverageOpponentRank) / rankUnits;

            if (highestRankedGames.Count < Convert.ToInt32(ConfigurationManager.AppSettings["BestWinsAvgRanking"]))
            {
                this.BestWinsAvgRank = 0;
            }
            else
            {
                highestRankedGames.Sort();
                highestRankedGames.RemoveRange(Convert.ToInt32(ConfigurationManager.AppSettings["BestWinsAvgRanking"]), highestRankedGames.Count - Convert.ToInt32(ConfigurationManager.AppSettings["BestWinsAvgRanking"]));           
                foreach (double avg in highestRankedGames)
                {
                    this.BestWinsAvgRank += avg;
                }
                BestWinsAvgRank = BestWinsAvgRank / Convert.ToDouble(ConfigurationManager.AppSettings["BestWinsAvgRanking"]);
            }

            
        }

        public static void ToAllSchedulesCsv(bool isPreason = false)
        {
            string[] keys = "Week,Game,Location,OppId,Opponent,Result,Score,TeamId,TeamName,BowlId".Split(',');
            if (isPreason)
            {
                keys = "Week,Game,Location,OppId,Opponent,WinPredicted,Spread,TeamId,TeamName".Split(',');
            }

            List<string> lines = new List<string>();
            foreach (var schedule in TeamSchedules.Values)
            {
                schedule.CalculateOpponentMetrics(isPreason);
                Team currentTeam = null;
                foreach (var game in schedule.FlattenedListOfGames.Where(g => g.OpponentId != 1023).OrderBy(g => g.Week))
                {
                    string line = null;
                    if (!isPreason)
                    {
                        line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", game.Week, game.GameNumber, game.Location, game.OpponentId, game.OpponentDescription, game.Result, game.Score, game.TeamId, game.TeamDescription, game.BowlId > 0 ? game.BowlId.ToString() : string.Empty);
                    }
                    else
                    {
                        var gamePrediction = PredictedGame.Predict(game.Week, game.GameNumber);
                        var spread = gamePrediction.Spread;
                        var winPredicted = gamePrediction.Winner.Id == game.TeamId;
                        line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", game.Week, game.GameNumber, game.Location, game.OpponentId, game.OpponentDescription, winPredicted.ToString().ToLowerInvariant(), spread, game.TeamId, game.TeamDescription);
                    }
                    lines.Add(line);
                    currentTeam = Team.Teams[game.TeamId];
                }

                // sb.AppendLine(string.Format(",,,,Opp Record ({0}-{1})<br>Avg Rank ({2}),,,{3}", schedule.OpponentWin, schedule.OpponentLoss, string.Format("{0}.{1}", schedule.AverageOpponentRank / 10, schedule.AverageOpponentRank % 10), currentTeam));
                if (!isPreason)
                {
                    lines.Add(string.Format(",,,,Opp Record ({0}-{1})<br>Avg Rank ({2})<br><br>vs Top " + Convert.ToInt32(ConfigurationManager.AppSettings["Level1TopRanking"]) + " ({3}-{4})<Br>vs Top " + Convert.ToInt32(ConfigurationManager.AppSettings["Level2TopRanking"]) + " ({5}-{6})<Br>vs Top " + Convert.ToInt32(ConfigurationManager.AppSettings["Level3TopRanking"]) + " ({7}-{8})<Br>Best " + ConfigurationManager.AppSettings["BestWinsAvgRanking"] + " Wins Avg Ranking ({9}),,,{10}", schedule.OpponentWin, schedule.OpponentLoss, string.Format("{0}.{1}", schedule.AverageOpponentRank / 10, schedule.AverageOpponentRank % 10), schedule.TopLvl1Wins, schedule.TopLvl1Losses, schedule.TopLvl2Wins, schedule.TopLvl2Losses, schedule.TopLvl3Wins, schedule.TopLvl3Losses, String.Format("{0:0.0}", schedule.BestWinsAvgRank), currentTeam.Id));
                }
                else
                {
                    lines.Add(string.Format(",,,,Predicted Record:  {0}-{1},,,{2},", currentTeam.PredictedWin.Value, currentTeam.PredictedLoss.Value, currentTeam.Id));
                }
            }

            var file = isPreason ? "ps-tsch.csv" : "tsch.csv";
            lines.ToCsvFile(keys, s => s, file);
        }

        public static void ToCsv(int teamId, bool isPreseason)
        {
            if (TeamSchedules.ContainsKey(teamId) == false)
                return;

            var schedule = TeamSchedules[teamId];
            schedule.CalculateOpponentMetrics(isPreseason);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Week,Game,Location,OppId,Opponent,Result,Score");
            foreach (var game in schedule.FlattenedListOfGames.Where(g => g.OpponentId != 1023).OrderBy(g => g.Week))
            {
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", game.Week, game.GameNumber, game.Location, game.OpponentId, game.OpponentDescription, game.Result, game.Score));
            }
            sb.AppendLine(string.Format(",,,,Opp Record ({0}-{1})<br>Avg Rank ({2}),,", schedule.OpponentWin, schedule.OpponentLoss, string.Format("{0}.{1}", schedule.AverageOpponentRank / 10, schedule.AverageOpponentRank % 10)));
            Utility.WriteData(@".\archive\reports\tsch" + teamId + ".csv", sb.ToString());
        }

        public static void ToSOSCsv(MaddenDatabase db, bool isPreseason)
        {
            TeamSchedule.Create(db);
            Team.Create(db,isPreseason);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Team,Record,OppAvgRank,OppWin,OppLoss,OppWinPct,BCS,Coaches,Media,TeamId");
            foreach (var schedule in TeamSchedules.Values.OrderBy(s => s.AverageOpponentRank))
            {
                sb.AppendLine(string.Format("{0},{1}-{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    schedule.Team.Name, schedule.Team.Win, schedule.Team.Loss, schedule.AverageOpponentRank,
                    schedule.OpponentWin, schedule.OpponentLoss, schedule.OpponentWinPct,
                    schedule.Team.BCSRank, schedule.Team.CoachesPollRank, schedule.Team.MediaPollRank, schedule.TeamId));
            }

            Utility.WriteData(@".\archive\reports\sos.csv", sb.ToString());
        }
    }
}
