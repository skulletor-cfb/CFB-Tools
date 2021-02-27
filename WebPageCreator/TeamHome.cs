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
    [DataContract]
    public class FourYearRecord
    {
        [DataMember(EmitDefaultValue = false)]
        public int[] NationalChampionshipYears { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int[] ConferenceChampionshipYears { get; set; }

        [DataMember]
        public int Win { get; set; }

        [DataMember]
        public int Loss { get; set; }

        [DataMember]
        public int BowlWin { get; set; }

        [DataMember]
        public int BowlLoss { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string[] BowlsWon { get; set; }
    }

    [Serializable]
    public enum PlayoffStatus
    {
        None = 0,
        LostInPlayoff = 1,
        LostInChampionshipGame = 2,
        NationalChampion = 4
    }

    [DataContract]
    public class TeamStatRanking
    {
        [DataMember(EmitDefaultValue = false)]
        public int Overall { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Passing { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Rushing { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Scoring { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Turnovers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int PassingTD { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RushingTD { get; set; }
    }

    public class Game
    {
        public int TeamId { get; set; }
        public bool IsHomeGame { get; set; }
        public int OpponentId { get; set; }
        public Team Opponent { get { return Team.Teams[OpponentId]; } }
        public Team TeamName { get { return Team.Teams[TeamId]; } }
        public int GameNumber { get; set; }
        public int Week { get; set; }
        public int BowlId
        {
            get
            {
                if (this.Week >= 16)
                {
                    return Bowl.FindByKey(this.Week, this.GameNumber).Id;
                }
                else if (this.Week < 16 && this.ScheduledGame.IsNeutralSite)
                {
                    return this.ScheduledGame.SiteId;
                }

                return 0;
            }
        }
        public string Location
        {
            get
            {
                if (this.Week >= 16)
                {
                    return Bowl.FindByKey(this.Week, this.GameNumber).Name;
                }

                if (this.ScheduledGame.IsNeutralSite)
                {
                    return this.ScheduledGame.GameSite;
                }

                return IsHomeGame ? "Home" : "Away";
            }
        }
        public ScheduledGame ScheduledGame
        {
            get
            {
                return ScheduledGame.Schedule[this.Week + "-" + this.GameNumber];
            }
        }
        public bool DidIWin
        {
            get
            {
                if (ScheduledGame.HomeTeamId == this.TeamId && ScheduledGame.HomeScore > ScheduledGame.AwayScore)
                    return true;

                if (ScheduledGame.AwayTeamId == this.TeamId && ScheduledGame.AwayScore > ScheduledGame.HomeScore)
                    return true;

                return false;
            }
        }
        public string Score
        {
            get
            {
                int myScore = 0;
                int oppScore = 0;
                if (this.ScheduledGame.HomeTeamId == this.TeamId)
                {
                    myScore = this.ScheduledGame.HomeScore;
                    oppScore = this.ScheduledGame.AwayScore;
                }
                else
                {
                    myScore = this.ScheduledGame.AwayScore;
                    oppScore = this.ScheduledGame.HomeScore;
                }

                return myScore + "-" + oppScore;
            }
        }
        public string Result
        {
            get
            {
                if ((this.ScheduledGame.HomeTeamId == this.TeamId && this.ScheduledGame.HomeScore > this.ScheduledGame.AwayScore) ||
                    (this.ScheduledGame.AwayTeamId == this.TeamId && this.ScheduledGame.AwayScore > this.ScheduledGame.HomeScore))
                {
                    return "Win";
                }

                if ((this.ScheduledGame.HomeTeamId == this.TeamId && this.ScheduledGame.HomeScore == this.ScheduledGame.AwayScore) ||
                    (this.ScheduledGame.AwayTeamId == this.TeamId && this.ScheduledGame.AwayScore == this.ScheduledGame.HomeScore))
                {
                    return " ";  //When game hasn't been played it will show 0-0
                }

                return "Loss";
            }
        }
        public string OpponentDescription { get { return GetDescription(this.Opponent, this.Opponent.Name, this.Opponent.SeasonRecord); } }
        public string TeamDescription { get { return GetDescription(this.TeamName, this.TeamName.Name, this.TeamName.SeasonRecord); } }

        private string GetDescription(Team team, string name, string record)
        {
            var prefix = string.Empty;
            int rank = 0;
            switch (Utility.TeamRankDisplay)
            {
                case TeamRankDisplay.AP:
                    rank = team.MediaPollRank;
                    break;
                case TeamRankDisplay.Coach:
                    rank = team.CoachesPollRank;
                    break;
                case TeamRankDisplay.BCS:
                default:
                    rank = team.BCSRank;

                    //Will use Coaches Poll until BCS Ranking is generated around Week 8
                    if (rank == 0) { rank = team.CoachesPollRank; }
                    break;
            }


            if (Utility.ShowTeamRank == ShowTeamRank.All || (Utility.ShowTeamRank == ShowTeamRank.Top25 && rank <= 25 && rank >= 1))
                prefix = "#" + rank + " - ";

            return string.Format("{0}{1}  ({2})", prefix, name, record);
        }
    }


    [DataContract]
    public class Team
    {
        public const string TeamFile = @".\archive\reports\team";
        public static Dictionary<int, Team> Teams { get; private set; }

        public static void ToJsonFile(bool isPreseason)
        {
            var file = isPreseason ? TeamFile.Replace("\\team", "\\ps-team") : TeamFile;
            Teams.Values.ToArray().OrderBy(team => team.CoachesPollRank).ToArray().ToJsonFile(file);
        }

        public static Team[] FromJsonFile()
        {
            return FromJsonFile(TeamFile);
        }

        public static Team[] FromJsonFile(string file)
        {
            return file.FromJsonFile<Team[]>();
        }

        public static void Prep(MaddenDatabase db, bool isPreseason)
        {
            // need historic records
            HistoricTeamRecord.Create(db);
            BowlChampion.Create(db);
            TeamSchedule.Create(db, isPreseason);
            ScheduledGame.Create(db, isPreseason);
            Bowl.Create(db, isPreseason);
            Coach.Create(db);
            Stadium.Create(db);
            ConferenceChampion.Create(db);
            TeamDraftHistory.Create(db);
        }

        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            Prep(db, isPreseason);

            if (Teams != null)
                return;

            Teams = new Dictionary<int, Team>();
            var table = db.lTables[167];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var teamId = table.lRecords[i].lEntries[40].Data.ToInt32().GetRealTeamId();

                // don't look at any team with an id greater than 235 and less than 901 which is the first teambuilder team id
                if (teamId > 235 && teamId < 901)
                    continue;

                var team = new Team(table.lRecords[i], db, isPreseason);
                Teams.Add(team.Id, team);
            }


            if( !Teams.ContainsKey(61))
                Teams.Add(61, new Team(61, "New Mexico State"));

            if (!Teams.ContainsKey(100))
                Teams.Add(100, new Team(100, "Connecticut"));
        }

        public static void TopPrograms(MaddenDatabase db, bool isPreseason)
        {
            Conference.Create(db);
            Team.Create(db, isPreseason);
            TextWriter tw = null;

            // top programs
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name,Record,Pct,TeamId");

            foreach (var team in Team.Teams.Values.Where(t => t.HistoricRecords.Count > 0).OrderByDescending(t => t.DynastyWinPct))
            {
                sb.AppendLine(string.Format("{0},{1}-{2},{3},{4}", team.Name, team.DynastyWin, team.DynastyLoss, team.DynastyWinPct, team.Id));
            }

            Utility.WriteData(@".\archive\reports\topprograms.csv", sb.ToString());

            // hot programs
            sb = new StringBuilder();
            sb.AppendLine("Name,Record,Pct,Year3,Year2,Year1,Trend,TeamId");

            foreach (var team in Team.Teams.Values.Where(t => t.HistoricRecords.Count > 0 && t.Last3Years.Length == 3).OrderByDescending(t => t.WinPct3Years).ThenByDescending(t => t.Last3Years[0].Win))
            {
                var recent = team.Last3Years;

                sb.AppendLine(string.Format("{0},{1}-{2},{3},{4}-{5},{6}-{7},{8}-{9},{10},{11}",
                    team.Name, recent.Sum(r => r.Win), recent.Sum(r => r.Loss), team.WinPct3Years,
                    recent[0].Win, recent[0].Loss, recent[1].Win, recent[1].Loss,
                    recent[2].Win, recent[2].Loss, team.Get3YearTrend(), team.Id));
            }

            Utility.WriteData(@".\archive\reports\hotprograms.csv", sb.ToString());

            // write the html file
            using (tw = new StreamWriter("./Archive/Reports/topprograms.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Top Programs", "loadTopProgramData");
                tw.Write(File.ReadAllText(@".\archive\html\topprogramtemplate"));
            }

            // top programs by conference
            sb = new StringBuilder();
            sb.AppendLine("Name,Record,Pct,Last3Yr,TeamId,ConfId,");

            foreach (var team in Team.Teams.Values.Where(t => t.HistoricRecords.Count > 0).OrderBy(t => t.ConferenceId).ThenByDescending(t => t.DynastyWinPct))
            {
                sb.AppendLine(string.Format("{0},{1}-{2},{3},{4}-{5},{6},{7}",
                    team.Name, team.DynastyWin, team.DynastyLoss, team.DynastyWinPct,
                    team.Last3Years.Sum(r => r.Win), team.Last3Years.Sum(r => r.Loss), team.Id, team.ConferenceId));
            }

            Utility.WriteData(@".\archive\reports\topprogramsconf.csv", sb.ToString());

            // hot programs by conference
            sb = new StringBuilder();
            sb.AppendLine("Name,Last3Yr,Pct,Record,Trend,TeamId,ConfId");

            foreach (var team in Team.Teams.Values.Where(t => t.HistoricRecords.Count > 0 && t.Last3Years.Length == 3).OrderBy(t => t.ConferenceId).ThenByDescending(t => t.WinPct3Years).ThenByDescending(t => t.Last3Years[0].Win))
            {
                sb.AppendLine(string.Format("{0},{1}-{2},{3},{4}-{5},{6},{7},{8}",
                    team.Name, team.Last3Years.Sum(r => r.Win), team.Last3Years.Sum(r => r.Loss),
                    team.WinPct3Years, team.DynastyWin, team.DynastyLoss,
                    team.Get3YearTrend(), team.Id, team.ConferenceId));
            }

            Utility.WriteData(@".\archive\reports\hotprogramsconf.csv", sb.ToString());

            // write the html file
            using (tw = new StreamWriter("./Archive/Reports/topprogramsbyconference.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Top Programs by Conference", "loadTopProgramConfData");
                tw.Write(File.ReadAllText(@".\archive\html\topprogrambyconftemplate"));
            }
        }


        public static Dictionary<int, Team>[] Last5TeamFiles;

        public static void LoadLast5Years()
        {
            if (Last5TeamFiles == null)
            {
                int[] Last5Years = { Seasons.CurrentYear - 1, Seasons.CurrentYear - 2, Seasons.CurrentYear - 3, Seasons.CurrentYear - 4, Seasons.CurrentYear - 5 };
                Last5TeamFiles = Last5Years.Select(yr => Seasons.DirectoryForYear(yr)).Where(s => s != null).Select(s => Team.FromJsonFile(Path.Combine(s, "team")).ToDictionary(t => t.Id)).ToArray();
            }
        }

        static int GetDivisorForRecruitClass(int win, int loss)
        {
            var total = win + loss;
            return total == 0 ? 1 : total;
        }

        public static void CreateMainPage(MaddenDatabase db, bool isPreseason = false)
        {
            Team.Create(db, isPreseason);
            TextWriter tw = null;

            ConferenceChampion.ToCCFile("tcc.csv");
            BowlChampion.ToBCFile("tbc.csv");
            HistoricTeamRecord.ToHistoricRecordFile(isPreseason ? "ps-thr.csv" : "thr.csv", isPreseason ? 5 : default(int?));
            Team.ToJsonFile(isPreseason);
            TeamSchedule.ToAllSchedulesCsv(isPreseason);

            // get the 4 year recruiting class average
            if (isPreseason)
            {
                LoadLast5Years();
                foreach (var team in Team.Teams.Values)
                {
                    if (team.Id.IsFCS() || team.Id.TeamNoLongerFBS())
                        continue;

                    var sum = Last5TeamFiles.Select(dict => dict.ContainsKey(team.Id) ? dict[team.Id].RecruitClassRating : 0).Sum();
                    team.RecruitClass5YearAverage = sum * 10 / 5;

                    // record the last 4 years is what these recruits have had an impact on
                    var records = HistoricTeamRecord.TeamRecords[team.Id].Values.OrderByDescending(t => t.Year).Skip(1).Take(4).ToArray();
                    team.RecruitClassWin = records.Sum(t => t.Win);
                    team.RecruitClassLoss = records.Sum(t => t.Loss);
                }

                var orderRecruitTeams = Team.Teams.Values.Where(t => !t.Id.IsFCS()).OrderByDescending(t => t.RecruitClass5YearAverage).ToArray();
                orderRecruitTeams.ToCsvFile(
                    new string[] { "Name", "Record", "Pct", "TeamId", "Pts", "CC", "NC", "BW,ConfId" },
                    t => string.Format("{0},{1}-{2},{3},{4},{5},{6},{7},{8},{9}",
                        t.Name, t.RecruitClassWin, t.RecruitClassLoss, (1000 * t.RecruitClassWin) / (GetDivisorForRecruitClass(t.RecruitClassWin, t.RecruitClassLoss)), t.Id, t.RecruitClass5YearAverage,
                        ConferenceChampion.ConferenceChampions.Where(cc => cc.TeamId == t.Id && cc.Year >= (BowlChampion.CurrentYear - 4)).Count(),
                        BowlChampion.GetTeamBowlChampionships(t.Id).Where(bc => bc.Year >= (BowlChampion.CurrentYear - 4) && bc.BowlId == BowlChampion.NationalChampionshipGameId).Count(),
                        BowlChampion.GetTeamBowlChampionships(t.Id).Where(bc => bc.Year >= (BowlChampion.CurrentYear - 4) && bc.BowlId != BowlChampion.NationalChampionshipGameId).Count(),
                        t.ConferenceId),
                    "teamrecruitranks.csv");
            }

#if false
            foreach (var team in Team.Teams.Values)
            {
                team.ToJsonFile(string.Format(@".\archive\reports\team{0}.txt", team.Id));
                ConferenceChampion.ToCsvFile(ConferenceChampion.GetTeamConferenceChampionships(team.Id), string.Format("tcc{0}.csv", team.Id));
                BowlChampion.ToCsvFile(BowlChampion.GetTeamBowlChampionships(team.Id), string.Format("tbc{0}.csv", team.Id));
                HistoricTeamRecord.ToCsvFile(team.HistoricRecords, string.Format("thr{0}.csv", team.Id));
                TeamSchedule.ToCsv(team.Id);
            }
#endif

            var htmlFile = isPreseason ? "PreSeasonTeam.html" : "team.html";
            using (tw = new StreamWriter("./Archive/Reports/" + htmlFile, false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamMainData", isPreseason.ToString().ToLower());

                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table id=\"topTable\" cellpadding=20 cellspacing=0 width=780>");
                // "<tr><tr><td class=c3><a href=Standings.html>Current Season</a></td><td class=c3>" + Convert.ToInt32(row["Current_Season_Wins"]) + "-" + Convert.ToInt32(row["Current_Season_Losses"]) + "</td></tr><tr><td class=c3><a href=Standings.html>Conf Record</a></td><td class=c3>" + Convert.ToInt32(row["Conf_Wins"]) + "-" + Convert.ToInt32(row["Conf_Losses"]) + "</td></tr><tr><td class=c3><a href=Standings.html>Div Record</a></td><td class=c3>" + Convert.ToInt32(row["Div_Wins"]) + "-" + Convert.ToInt32(row["Div_Losses"]) + "</td></tr><tr><td class=c3><a href=BCS_Rankings.html>BCS Rank</a></td><td class=c3>" + Convert.ToInt32(row["BCS_Rank"]) + "</td></tr><tr><td class=c3><a href=CoachesPoll.html>Coaches Poll</a></td><td class=c3>" + Convert.ToInt32(row["CP_Rank"]) + "</td></tr><tr><td class=c3><a href=MediaPoll.html>Media Poll</a></td><td class=c3>" + Convert.ToInt32(row["MP_Rank"]) + "</td></tr><tr><td class=c3>Avg Att/ Capacity</td><td class=c3>" + Convert.ToInt32(row["Avg_Att"]).ToString("###,###") + "  <b>/</b>  " + Convert.ToInt32(row5["Cap"]).ToString("###,###") + "</td></tr></table></td></tr></table>");
                // tw.Write(@"<table cellpadding=20 cellspacing=0 width=700><tr><td width=100% align=center colspan=4></td></tr>");
                tw.Write(Utility.LoadTeamTemplate("teamtemplate", isPreseason));
                tw.Write(@"</body>");
            }
        }

        public static void WritePolls()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Record,Points,FPV,Previous,Table");

            foreach (var team in Team.Teams.Values.Where(t => t.MediaPollPoints > 0).OrderBy(t => t.MediaPollRank))
            {
                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4},{5},{6},{7}",
                    team.Name, team.Id, team.Win, team.Loss, team.MediaPollPoints, team.MediaPollFirstPlaceVotes, team.MediaPollPrevious, 1));
            }

            foreach (var team in Team.Teams.Values.Where(t => t.CoachesPollPoints > 0).OrderBy(t => t.CoachesPollRank))
            {
                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4},{5},{6},{7}",
                    team.Name, team.Id, team.Win, team.Loss, team.CoachesPollPoints, team.CoachesPollFirstPlaceVotes, team.CoachesPollPrevious, 2));
            }

            Utility.WriteData(@".\archive\reports\polls.csv", sb.ToString());
        }

        int atWin, atLoss;
        Coach headCoach;
        TeamSeasonRecord[] last5Years;
        int dWin, dLoss;

        public bool IsIndependent
        {
            get
            {
                return this.ConferenceId == 5;
            }
        }

        public static Dictionary<int, Team> LastYearData
        {
            get
            {
                var file = Path.Combine(Seasons.LastYearDirectory, "team");
                if (File.Exists(file))
                {
                    return Team.FromJsonFile(file).ToDictionary(t => t.Id);
                }

                return null;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public PlayoffStatus PlayoffStatus { get; set; }
        public bool LostInPlayoffLastYear
        {
            get
            {
                var lastYear = LastYearData;
                if (lastYear != null)
                {
                    Team team = null;

                    return lastYear.TryGetValue(this.Id, out team) &&
                        (team.PlayoffStatus == EA_DB_Editor.PlayoffStatus.LostInPlayoff || team.PlayoffStatus == EA_DB_Editor.PlayoffStatus.LostInChampionshipGame);
                }

                return false;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public DraftClass DraftHistory { get; set; }
        public int RecruitClassWin { get; set; }
        public int RecruitClassLoss { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RecruitClass5YearAverage { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RecruitClassRating { get; set; }

        [DataMember]
        public string Article { get; set; }

        [DataMember]
        public TeamStatRanking OffensiveRankings { get; set; }
        [DataMember]
        public TeamStatRanking DefensiveRankings { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Mascot { get; set; }
        [DataMember]
        public int ConferenceId { get; set; }
        public Conference Conference { get { return Conference.Conferences[this.ConferenceId]; } }
        [DataMember]
        public int DivisionId { get; set; }
        public Division Division { get { return this.Conference.FindDivision(this.DivisionId); } }
        public string DivisionName
        {
            get
            {
                return this.Division == null ? "" : this.Division.Name;
            }
        }
        public int Games { get { return Win + Loss; } }
        [DataMember]
        public int BowlWin { get; set; }
        [DataMember]
        public int BowlLoss { get; set; }
        [DataMember]
        public int BowlTie { get; set; }
        [DataMember]
        public int Win { get; set; }
        [DataMember]
        public int Loss { get; set; }
        [DataMember]
        public int Streak { get; set; }
        [DataMember]
        public int PriorSeasonWin { get; set; }
        [DataMember]
        public int PriorSeasonLoss { get; set; }
        [DataMember]
        public int BCSRank { get; set; }
        [DataMember]
        public int BCSPrevious { get; set; }
        [DataMember]
        public int MediaPollRank { get; set; }
        [DataMember]
        public int MediaPollFirstPlaceVotes { get; set; }
        [DataMember]
        public int MediaPollPoints { get; set; }
        [DataMember]
        public int MediaPollPrevious { get; set; }
        [DataMember]
        public int CoachesPollRank { get; set; }
        [DataMember]
        public int CoachesPollFirstPlaceVotes { get; set; }
        [DataMember]
        public int CoachesPollPoints { get; set; }
        [DataMember]
        public int CoachesPollPrevious { get; set; }
        [DataMember]
        public int NationalTitles { get; set; }
        [DataMember]
        public int ConferenceTitles { get; set; }
        [DataMember]
        public int ConferenceWin { get; set; }
        [DataMember]
        public int ConferenceLoss { get; set; }
        [DataMember]
        public int DivisionWin { get; set; }
        [DataMember]
        public int DivisionLoss { get; set; }
        [DataMember]
        public int AverageAttendance { get; set; }
        [DataMember]
        public int RecordAttendance { get; set; }
        [DataMember]
        public int StadiumCapacity { get { return this.Stadium.Capacity; } set { } }
        public string SeasonRecord { get { return Win + "-" + Loss; } }
        [DataMember]
        public int StadiumId { get; set; }
        [DataMember]
        public int HomeWin { get; set; }
        [DataMember]
        public int HomeLoss { get; set; }
        [DataMember]
        public int HomeTie { get; set; }
        [DataMember]
        public string HomeStreak { get; set; }

        [DataMember]
        public int HomeStreakRaw { get; set; }

        [DataMember]
        public int ToughestPlaceToPlayRank { get; set; }

        public Stadium Stadium
        {
            get
            {
                return Stadium.Stadiums[this.StadiumId];
            }
        }

        public Dictionary<int, TeamSeasonRecord> HistoricRecords
        {
            get
            {
                Dictionary<int, TeamSeasonRecord> records = null;
                if (HistoricTeamRecord.TeamRecords != null && HistoricTeamRecord.TeamRecords.TryGetValue(this.Id, out records) == false)
                {
                    records = new Dictionary<int, TeamSeasonRecord>();
                }

                return records;
            }
        }

        [DataMember]
        public int AllTimeWin
        {
            get
            {
                return atWin;
            }
            set
            {
                if (value < DynastyWin)
                    value += 1024;
                atWin = value;
            }
        }
        [DataMember]
        public int AllTimeLoss
        {
            get
            {
                return atLoss;
            }
            set
            {
                if (value < DynastyLoss)
                {
                    value += 1024;
                }

                atLoss = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public MediaCoverage[] MediaCoverage { get; set; }

        //TEAM RATING INFO
        [DataMember]
        public int TeamRatingOVR { get; set; }
        [DataMember]
        public int TeamRatingOFF { get; set; }
        [DataMember]
        public int TeamRatingDEF { get; set; }
        [DataMember]
        public int TeamRatingST { get; set; }
        [DataMember]
        public int TeamRatingQB { get; set; }
        [DataMember]
        public int TeamRatingRB { get; set; }
        [DataMember]
        public int TeamRatingWR { get; set; }
        [DataMember]
        public int TeamRatingOL { get; set; }
        [DataMember]
        public int TeamRatingDL { get; set; }
        [DataMember]
        public int TeamRatingLB { get; set; }
        [DataMember]
        public int TeamRatingDB { get; set; }

        [DataMember]
        public int AllTimeTie { get; set; }
        #region Prediction Engine
        [DataMember(EmitDefaultValue = false)]
        public int? PredictedWin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? PredictedLoss { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? PredictedConferenceWin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? PredictedConferenceLoss { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? PredictedDivisionWin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? PredictedDivisionLoss { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? SwingWin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? SwingLoss { get; set; }
        #endregion
        public int DynastyWin
        {
            get
            {
                if (dWin == 0 && this.HistoricRecords != null)
                {
                    dWin = this.HistoricRecords.Values.Sum(record => record.Win);
                }

                return dWin;
            }
        }
        public int DynastyLoss
        {
            get
            {
                if (dLoss == 0 && this.HistoricRecords != null)
                {
                    dLoss = this.HistoricRecords.Values.Sum(record => record.Loss);
                }

                return dLoss;
            }
        }

        /// <summary>
        /// Win Pct expressed as a 3 digit int
        /// </summary>
        public int DynastyWinPct
        {
            get
            {
                return Utility.CalculateWinPct(DynastyWin, DynastyLoss);
            }
        }

        /// <summary>
        /// Records the last 5 years in descending order
        /// </summary>
        public TeamSeasonRecord[] Last5Years
        {
            get
            {
                try
                {
                    if (last5Years == null)
                    {
                        last5Years = BowlChampion.CurrentYear < 5 ? new TeamSeasonRecord[BowlChampion.CurrentYear + 1] : new TeamSeasonRecord[5];
                        var data = this.HistoricRecords.OrderByDescending(kvp => kvp.Key).Take(last5Years.Length).ToArray();

                        if (data.Length == 1 && data[0].Value.Win == 0)
                            return new TeamSeasonRecord[0];

                        for (int i = 0; i < Math.Min(last5Years.Length, data.Length); i++)
                        {
                            last5Years[i] = data[i].Value;
                        }
                    }
                    return last5Years.Where(r => r != null).ToArray();
                }
                catch
                {
                    throw;
                }
            }
        }

        public int WinPct3Years
        {
            get
            {
                var win = this.Last3Years.Sum(tr => tr.Win);
                var loss = this.Last3Years.Sum(tr => tr.Loss);
                return Utility.CalculateWinPct(win, loss);
            }
        }


        public TeamSeasonRecord[] Last3Years
        {
            get
            {
                return this.Last5Years.Length > 3 ? this.Last5Years.Take(3).ToArray() : new TeamSeasonRecord[0];
            }
        }

        public string Get3YearTrend()
        {
            // no seasons played, no trend
            if (this.Last3Years.Length == 0) return string.Empty;

            string trend = string.Empty;
            var third = this.Last3Years[0].Win;
            var second = this.Last3Years[1].Win;
            var first = this.Last3Years.Length > 2 ? this.Last3Years[2].Win : second;

            if (third > second && second > first)
            {
                trend = "++";
            }
            else if (third > second || (third == second && second > first))
            {
                trend = "+";
            }
            else if (third < second && second < first)
            {
                trend = "--";
            }
            else if (third < second || (third == second && second < first))
            {
                trend = "-";
            }

            return trend;
        }

        public bool DownLastYear
        {
            get
            {
                return Get3YearTrend() == "-";
            }
        }

        public bool ImprovedLastYear
        {
            get
            {
                return Get3YearTrend() == "+";
            }
        }

        public bool OnTheRise
        {
            get
            {
                return Get3YearTrend() == "++";
            }
        }

        private Coach oc, dc;
        [DataMember]
        public Coach OffensiveCoordinator
        {
            get
            {
                if (this.oc == null)
                {
                    this.oc = Coach.FindCoach(this.Id, 1);
                }

                return this.oc;
            }
            set
            {
                this.oc = value;
            }
        }

        [DataMember]
        public Coach DefensiveCoordinator
        {
            get
            {
                if (this.dc == null)
                {
                    this.dc = Coach.FindCoach(this.Id, 2);
                }

                return this.dc;
            }
            set
            {
                this.dc = value;
            }
        }


        [DataMember]
        public Coach HeadCoach
        {
            set { this.headCoach = value; }
            get
            {
                if (this.headCoach == null)
                {
                    this.headCoach = Coach.FindHeadCoach(this.Id) ?? new Coach();

                    var records = this.HistoricRecords.Values.OrderByDescending(record => record.Year).Take(this.headCoach.YearsWithTeam).ToArray();
                    if (records.Length > 0)
                    {
                        int currentYear = records[0].Year;
                        int temp = 0;
                        for (int i = 0; i < records.Length; i++)
                        {
                            temp += records[i].Win;
                            this.headCoach.YearsAsHeadCoach++;

                            if (records[i].Win > records[i].Loss)
                                this.headCoach.WinningSeasons++;

                            if (temp >= this.headCoach.TeamWin)
                                break;
                        }

                        this.headCoach.ConferenceChampionships = ConferenceChampion.GetTeamConferenceChampionships(this.Id).Where(cc => cc.Year > (currentYear - this.headCoach.YearsAsHeadCoach)).Count();
                        this.headCoach.BowlWins = BowlChampion.GetTeamBowlChampionships(this.Id).Where(bc => bc.Year > (currentYear - this.headCoach.YearsAsHeadCoach)).Count();
                        this.headCoach.NationalChampionships = BowlChampion.GetTeamBowlChampionships(this.Id).Where(bc => bc.BowlId == 39 && bc.Year > (currentYear - this.headCoach.YearsAsHeadCoach)).Count();
                    }
                }

                return this.headCoach;
            }
        }

        public double OvrRating
        {
            get
            {

                var starters = PlayerDB.GetStarters(this.Id);
                var pts = starters.Sum(p => p.Ovr);
                return Convert.ToDouble(pts) / Convert.ToDouble(starters.Count);
            }
        }

        public double OffRating
        {
            get
            {
                var players = PlayerDB.GetStarters(this.Id).Where(p => p.Position.IsOffensivePosition());
                var pts = players.Sum(p => p.Ovr);
                return Convert.ToDouble(pts) / Convert.ToDouble(players.Count());
            }
        }

        public double DefRating
        {
            get
            {
                var players = PlayerDB.GetStarters(this.Id).Where(p => p.Position.IsDefensivePosition());
                var pts = players.Sum(p => p.Ovr);
                return Convert.ToDouble(pts) / Convert.ToDouble(players.Count());
            }
        }

        public double STRating
        {
            get
            {
                var players = PlayerDB.GetStarters(this.Id).Where(p => p.Position.IsSTPosition());
                var pts = players.Sum(p => p.Ovr);
                return Convert.ToDouble(pts) / Convert.ToDouble(players.Count());
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public int? LastConferenceChampionshipYear { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? LastNationalChampionshipYear { get; set; }

        /// <summary>
        /// What Bowl they won this year
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string BowlWinsThisYear { get; set; }

        /// <summary>
        /// What Conference they won this year
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ConferenceOrDivisionChampionship { get; set; }

        /// <summary>
        /// did they win the national championship this year
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsNationalChampion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RecruitClassRank { get; set; }

        [DataMember]
        public int OffPlayBookId { get; set; }

        public List<int> ImpactPlayerIds { get; set; }
        public Player[] ImpactPlayers
        {
            get
            {
                return PlayerDB.Rosters[this.Id].Where(p => ImpactPlayerIds.Contains(p.Id)).ToArray();
            }
        }
        public int MainRival { get; set; }

        private Team(int id, string name)
        {
            this.Id = id;
            this.Name = name;
            BCSRank = CoachesPollRank = MediaPollRank = 200;
        }

        public Team(MaddenRecord record, MaddenDatabase db, bool isPreseason)
        {
            OffPlayBookId = record["TOPB"].ToInt32();
            Id = record.lEntries[40].Data.ToInt32().GetRealTeamId();
            Name = record.lEntries[7].Data;
            Mascot = record.lEntries[9].Data;
            ConferenceId = record.lEntries[36].Data.ToInt32();
            DivisionId = record.lEntries[37].Data.ToInt32();
            var atw = record.lEntries[60].Data.ToInt32();
            var atl = record.lEntries[86].Data.ToInt32();
            var att = record.lEntries[58].Data.ToInt32();
            var bw = record.lEntries[168].Data.ToInt32();
            var bl = record.lEntries[71].Data.ToInt32();
            var bt = record.lEntries[143].Data.ToInt32();
            Win = record.GetInt(61);
            Loss = record.GetInt(88);
            Streak = record.GetInt(125);
            PriorSeasonLoss = record.GetInt(73);
            PriorSeasonWin = record.GetInt(170);
            BCSRank = record.GetInt(62);
            BCSPrevious = record.GetInt(112);
            MediaPollRank = record.GetInt(64);
            MediaPollFirstPlaceVotes = record.GetInt(160);
            MediaPollPoints = record.GetInt(184);
            MediaPollPrevious = record.GetInt(99);
            CoachesPollRank = record.GetInt(63);
            CoachesPollFirstPlaceVotes = record.GetInt(97);
            CoachesPollPoints = record.GetInt(144);
            CoachesPollPrevious = record.GetInt(113);
            var nts = record.GetInt(82);
            var cts = record.GetInt(31);
            ConferenceWin = record.GetInt(193);
            ConferenceLoss = record.GetInt(181);
            DivisionWin = record.GetInt(194);
            DivisionLoss = record.GetInt(182);
            AverageAttendance = record.GetInt(5);
            RecordAttendance = record.GetInt(119);
            StadiumId = record.GetInt(39);
            MainRival = record["TRIV"].ToInt32();
            HomeWin = record["TCHW"].ToInt32();
            HomeLoss = record["TCHL"].ToInt32();
            HomeTie = record["TCHT"].ToInt32();
            HomeStreakRaw = record["TCHS"].ToInt32();
            string hs = "W";

            if (HomeStreakRaw > 900)
            {
                hs = "L" + (1024 - HomeStreakRaw);
                HomeStreakRaw = HomeStreakRaw - 1024;
            }
            else
            {
                hs = "W" + HomeStreakRaw;
            }

            HomeStreak = hs;

            ImpactPlayerIds = new List<int>();
            // Impact Player 1 : TSI1
            ImpactPlayerIds.Add(record["TSI1"].ToInt32());
            // Impact Player 2 : TSI2
            ImpactPlayerIds.Add(record["TSI2"].ToInt32());
            // Impact Player 3 : TMPI
            ImpactPlayerIds.Add(record["TMPI"].ToInt32());
            // Impact Player 4 : TPIO
            ImpactPlayerIds.Add(record["TPIO"].ToInt32());


            //Team Rating Information
            TeamRatingOVR = record.GetInt(157);
            TeamRatingOFF = record.GetInt(49);
            TeamRatingDEF = record.GetInt(46);
            TeamRatingST = record.GetInt(148);
            TeamRatingQB = record.GetInt(24);
            TeamRatingRB = record.GetInt(25);
            TeamRatingWR = record.GetInt(118);
            TeamRatingOL = record.GetInt(72);
            TeamRatingDL = record.GetInt(67);
            TeamRatingLB = record.GetInt(20);
            TeamRatingDB = record.GetInt(18);

            if (ContinuationData.UsingContinuationData)
            {
                if (ContinuationData.Instance.TeamData.ContainsKey(Id))
                {
                    AllTimeWin = atw + ContinuationData.Instance.TeamData[Id].AllTimeWin;
                    AllTimeLoss = atl + ContinuationData.Instance.TeamData[Id].AllTimeLoss;
                    AllTimeTie = att + ContinuationData.Instance.TeamData[Id].AllTimeTie;
                    BowlWin = bw + ContinuationData.Instance.TeamData[Id].BowlWin;
                    BowlLoss = bl + ContinuationData.Instance.TeamData[Id].BowlLoss;
                    BowlTie = bt + ContinuationData.Instance.TeamData[Id].BowlTie;
                    NationalTitles = nts + ContinuationData.Instance.TeamData[Id].NationalTitles;
                    ConferenceTitles = cts + ContinuationData.Instance.TeamData[Id].ConferenceTitles;
                    HomeWin = HomeWin + ContinuationData.Instance.TeamData[Id].HomeWin;
                    HomeLoss = HomeLoss + ContinuationData.Instance.TeamData[Id].HomeLoss;
                    HomeTie = HomeTie + ContinuationData.Instance.TeamData[Id].HomeTie;
                }
                else
                {
                    AllTimeWin = atw;
                    AllTimeLoss = atl;
                    AllTimeTie = att;
                    BowlWin = bw;
                    BowlLoss = bl;
                    BowlTie = bt;
                    NationalTitles = nts;
                    ConferenceTitles = cts;
                    HomeWin = HomeWin;
                    HomeLoss = HomeLoss;
                    HomeTie = HomeTie;
                }
            }
            else
            {
                AllTimeWin = atw;
                AllTimeLoss = atl;
                AllTimeTie = att;
                BowlWin = bw;
                BowlLoss = bl;
                BowlTie = bt;
                NationalTitles = nts;
                ConferenceTitles = cts;
            }

            // data about Championships
            this.IsNationalChampion = BowlChampion.IsNationalChampionshipYear(this.Id, BowlChampion.CurrentYear);
            this.BowlWinsThisYear = string.Join(", ", BowlChampion.GetBowlChampionships(this.Id, BowlChampion.CurrentYear).Select(bc => bc + " Champions"));

            // check if we are Conference champion
            this.ConferenceOrDivisionChampionship = ConferenceChampion.GetConferenceChampionship(this.Id, BowlChampion.CurrentYear);

            TeamSchedule schedule = null;

            // check to see if we are division champ, this means we played in the CCG but did not win it
            if (!isPreseason &&
                this.ConferenceOrDivisionChampionship == null &&
                TeamSchedule.TeamSchedules.TryGetValue(this.Id, out schedule) &&
                schedule.FlattenedListOfGames.Where(g => g.Week == 16 && g.GameNumber != 127).Any())
            {
                this.ConferenceOrDivisionChampionship = string.Format("{0} {1} {2}", Conference.Conferences[this.ConferenceId].Name, Conference.FindDivision(this.DivisionId).SubName, "Champions");
            }


            // last conference chanmp years
            var startYear = ConfigurationManager.AppSettings["StartingYear"].ToInt32();
            var yr = record["FCYR"].ToInt32();
            if (yr > 0)
                this.LastConferenceChampionshipYear = yr - 263 + startYear;

            // last championship is this determined if have a year greater than 262
            if (yr > 262)
                this.LastConferenceChampionshipYear = this.LastConferenceChampionshipYear.Value + ContinuationData.ContinuationYear;

            yr = record["NCYR"].ToInt32();
            if (yr > 0)
                this.LastNationalChampionshipYear = yr - 263 + startYear;

            if (yr > 262)
                this.LastNationalChampionshipYear = this.LastNationalChampionshipYear.Value + ContinuationData.ContinuationYear;

            if (ContinuationData.UsingContinuationData)
            {
                var a = this.LastConferenceChampionshipYear.HasValue ? this.LastConferenceChampionshipYear.Value : 0;
                var b = ContinuationData.Instance.TeamData.ContainsKey(Id) && ContinuationData.Instance.TeamData[Id].LastConferenceChampionshipYear.HasValue ? ContinuationData.Instance.TeamData[Id].LastConferenceChampionshipYear.Value : 0;
                this.LastConferenceChampionshipYear = Math.Max(a, b);

                a = this.LastNationalChampionshipYear.HasValue ? this.LastNationalChampionshipYear.Value : 0;
                b = ContinuationData.Instance.TeamData.ContainsKey(Id) && ContinuationData.Instance.TeamData[Id].LastNationalChampionshipYear.HasValue ? ContinuationData.Instance.TeamData[Id].LastNationalChampionshipYear.Value : 0;
                this.LastNationalChampionshipYear = Math.Max(a, b);
            }

            this.DraftHistory = TeamDraftHistory.Rollup(this.Id);

            // finally get the toughest place to play rank
            var stadium = db.GetTable("STAD").lRecords.Where(mr => mr["SGID"].ToInt32() == record["SGID"].ToInt32()).SingleOrDefault();

            if (stadium != null)
            {
                this.ToughestPlaceToPlayRank = stadium["STDR"].ToInt32();
            }
        }
    }

    public class HistoricTeamRecord
    {
        public static Dictionary<int, Dictionary<int, TeamSeasonRecord>> TeamRecords { get; set; }
        public static void Create(MaddenDatabase db)
        {
            if (TeamRecords != null)
                return;

            TeamRecords = new Dictionary<int, Dictionary<int, TeamSeasonRecord>>();
            var table = db.lTables[114];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var teamId = record.GetInt(0).GetRealTeamId();
                Dictionary<int, TeamSeasonRecord> teamSeasonRecords;
                if (TeamRecords.TryGetValue(teamId, out teamSeasonRecords) == false)
                {
                    // load the records from the continuation file
                    if (ContinuationData.UsingContinuationData && ContinuationData.Instance != null && ContinuationData.Instance.TeamHistoricRecords.ContainsKey(teamId))
                    {
                        teamSeasonRecords = ContinuationData.Instance.TeamHistoricRecords[teamId];
                    }
                    else
                    {
                        teamSeasonRecords = new Dictionary<int, TeamSeasonRecord>();
                    }

                    TeamRecords[teamId] = teamSeasonRecords;
                }

                var seasonRecord = new TeamSeasonRecord
                {
                    Year = record.GetInt(2) + ContinuationData.ContinuationYear,
                    Win = record.GetInt(3),
                    Loss = record.GetInt(1)
                };

                // check to see if the team went 16-0
                if (seasonRecord.Win == 0)
                {
                    seasonRecord.Win = BowlChampion.IsNationalChampionshipYear(teamId, seasonRecord.Year) ? 16 : 0;
                }

                teamSeasonRecords.Add(seasonRecord.Year, seasonRecord);
            }
        }

        public static void ToHistoricRecordFile(string file, int? take = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,Record,TeamId");

            foreach (var hr in HistoricTeamRecord.TeamRecords.OrderBy(kvp => kvp.Key))
            {
                // we may not want all the records
                IEnumerable<TeamSeasonRecord> records = hr.Value.Values.OrderByDescending(r => r.Year);

                // preaseason uses this only, and that means we don't have a record for the current season so we go ahead and skip it
                if (take.HasValue)
                {
                    records = records.Skip(1).Take(take.Value);
                }

                foreach (var record in records)
                {
                    sb.AppendLine(string.Format("{0},{1},{2}", record.Year, string.Format("{0}-{1}", record.Win, record.Loss), hr.Key));
                }
            }

            Utility.WriteData(@".\archive\reports\" + file, sb.ToString());
        }

        public static void ToCsvFile(Dictionary<int, TeamSeasonRecord> HistoricRecords, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,Record");

            foreach (var record in HistoricRecords.Values.OrderByDescending(r => r.Year))
            {
                sb.AppendLine(string.Format("{0},{1}", record.Year, string.Format("{0}-{1}", record.Win, record.Loss)));
            }

            Utility.WriteData(@".\archive\reports\" + file, sb.ToString());
        }

        public static Dictionary<int, Dictionary<int, TeamSeasonRecord>> FromCsvFile(string file)
        {
            var lines = file.ReadAllLines().Skip(1).Select(line => line.Split(','));
            var grouping = lines.GroupBy(line => line[2].ToInt32());
            return grouping.ToDictionary(
                group => group.Key,
                group => group.ToDictionary(
                    sa => sa[0].ToInt32(),
                    sa => new TeamSeasonRecord
                    {
                        Year = sa[0].ToInt32(),
                        Win = sa[1].Split('-').First().ToInt32(),
                        Loss = sa[1].Split('-').Last().ToInt32()
                    })
                );
        }
    }

    public class TeamSeasonRecord
    {
        public int Year { get; set; }
        public int Win { get; set; }
        public int Loss { get; set; }
    }

    public class TeamSeasonStats
    {
        public static Dictionary<int, TeamStat> TeamStats { get; set; }

        public static void Create(MaddenDatabase db)
        {
            if (TeamStats != null)
                return;

            TeamStats = new Dictionary<int, TeamStat>();

            var table = db.lTables[168];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var row = table.lRecords[i];
                var teamId = row.GetInt(0).GetRealTeamId();

                var stats = new TeamStat
                {
                    TeamId = teamId,
                    TwoPointConversionAttempts = row.GetInt(1),
                    Turnovers = row.GetInt(2),
                    PassAttempts = row.GetInt(3),
                    RushAttempts = row.GetInt(4),
                    Tssa = row.GetInt(5),
                    Tsta = row.GetInt(6),
                    TwoPointConversions = row.GetInt(7),
                    ThirdDownConversions = row.GetInt(8),
                    FourthDownConversions = row.GetInt(9),
                    FirstDowns = row.GetInt(10),
                    ThirdDownAttempts = row.GetInt(11),
                    FourthDownAttempts = row.GetInt(12),
                    Penalties = row.GetInt(14),
                    RedZoneFG = row.GetInt(16),
                    Tsdi = row.GetInt(17),
                    IntThrown = row.GetInt(19),
                    RedZoneFGAllowed = row.GetInt(15),
                    InterceptionsByDefense = row.GetInt(18),
                    Sacks = row.GetInt(20),
                    FumblesLost = row.GetInt(21),
                    PassYardsAllowed = row.GetInt(22),
                    PassYards = row.GetInt(23),
                    OpponentsInRedZone = row.GetInt(24),
                    FumblesRecovered = row.GetInt(25),
                    RushYards = row.GetInt(26),
                    PassTD = row.GetInt(27),
                    RedZoneTDAllowed = row.GetInt(28),
                    RedZoneTD = row.GetInt(29),
                    RushTD = row.GetInt(30),
                    PenaltyYards = row.GetInt(31),
                    TotalYards = row.GetInt(32),
                    RushingYardsAllowed = row.GetInt(33),
                    OffensiveYards = row.GetInt(34),
                    RedZoneVisits = row.GetInt(36),
                    SpecialTeamYards = row.GetInt(35),
                };

                TeamStats.Add(teamId, stats);
            }
        }
    }
}