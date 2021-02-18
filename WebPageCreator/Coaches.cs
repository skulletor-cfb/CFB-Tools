using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    [DataContract]
    public class Coach
    {
        public static bool IsPostSeason { get; set; }
        public static Dictionary<string, Coach> Coaches { get; set; }

        public static void Create(MaddenDatabase db)
        {
            if (Coaches != null)
                return;

            Coaches = new Dictionary<string, Coach>();

            var coachTable = db.lTables[133];
            for (int i = 0; i < db.lTables[133].Table.currecords; i++)
            {
                // only coaches on valid teams should be analyzed
                if (coachTable.lRecords[i].lEntries[23].Data.ToInt32().IsValidTeam() == false)
                    continue;

                int level, exp;
                GetLevelAndXP(db.lTables[132].lRecords, i, out level, out exp);
                var coach = new Coach
                {
                    Id = db.lTables[133].lRecords[i].lEntries[20].Data.ToInt32(),
                    TeamId = db.lTables[133].lRecords[i].lEntries[23].Data.ToInt32(),
                    Position = db.lTables[133].lRecords[i].lEntries[100].Data.ToInt32(),
                    FirstName = db.lTables[133].lRecords[i].lEntries[65].Data,
                    LastName = db.lTables[133].lRecords[i].lEntries[66].Data,
                    Age = db.lTables[133].lRecords[i].lEntries[29].Data.ToInt32(),
                    ContractLength = coachTable.lRecords[i].lEntries[69].Data.ToInt32(),
                    YearsIntoContract = coachTable.lRecords[i].lEntries[88].Data.ToInt32(),
                    YearsWithTeam = 1 + coachTable.lRecords[i].lEntries[89].Data.ToInt32(),
                    Rating = coachTable.lRecords[i].lEntries[109].Data.ToInt32(),
                    OriginalJob = coachTable.lRecords[i].lEntries[5].Data,
                    CareerWin = coachTable.lRecords[i]["CCWI"].ToInt32(),
                    CareerLoss = coachTable.lRecords[i]["CCLO"].ToInt32(),
                    TeamWin = coachTable.lRecords[i]["CTWN"].ToInt32(),
                    TeamLoss = coachTable.lRecords[i]["COTL"].ToInt32(),
                    Level = level,
                    Exp = exp,
                    OffPlaybookId = coachTable.lRecords[i]["CPID"].ToInt32(),
                    DefPlaybookId = (DefensivePlaybook)coachTable.lRecords[i]["CDID"].ToInt32(),
                    AlmaMaterId = coachTable.lRecords[i]["CHFT"].ToInt32(),
                    CoachBowlWin = coachTable.lRecords[i]["CBLW"].ToInt32(),
                    CoachBowlLoss = coachTable.lRecords[i]["CBLL"].ToInt32(),
                    AllAmericans = coachTable.lRecords[i]["CNAA"].ToInt32(),
                    Top25Classes = coachTable.lRecords[i]["CNTC"].ToInt32(),
                    CoachOfYearAwards = coachTable.lRecords[i]["CYRA"].ToInt32(),
                    Top25Win = coachTable.lRecords[i]["CTTW"].ToInt32(),
                    Top25Loss = coachTable.lRecords[i]["CTTL"].ToInt32(),
                    RivalWin = coachTable.lRecords[i]["CRVW"].ToInt32(),
                    RivalLoss = coachTable.lRecords[i]["CRVL"].ToInt32(),
                    LongestWinStreak = coachTable.lRecords[i]["CCLS"].ToInt32(),
                    HeismanWinners = coachTable.lRecords[i]["CHTW"].ToInt32(),
                    CareerConferenceChampionships = coachTable.lRecords[i]["CCTW"].ToInt32(),
                    CareerNationalChampionships = coachTable.lRecords[i]["CNTW"].ToInt32(),
                };

                Coaches.Add(coach.Key, coach);
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public int HeismanWinners { get; set; }

        public static void GetLevelAndXP(List<MaddenRecord> table, int idx, out int level, out int exp)
        {
            if (idx >= table.Count)
            {
                level = 1;
                exp = 0;
            }
            else
            {
                level = table[idx].lEntries[8].Data.ToInt32();
                exp = table[idx].lEntries[14].Data.ToInt32();
            }
        }

        public static Coach FindHeadCoach(int teamId)
        {
            return FindCoach(teamId, 0);
        }

        public static Coach FindCoach(int teamId, int position)
        {
            try
            {
                return Coaches.Values.Where(c => c.TeamId == teamId && c.Position == position).SingleOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public static Tuple<Coach, Coach> CoachesAreDifferent(Dictionary<int, Coach> newStaff, Dictionary<int, Coach> oldStaff, int position)
        {
            Coach newCoach = null;
            Coach oldCoach = null;

            if (newStaff.TryGetValue(position, out newCoach) && oldStaff.TryGetValue(position, out oldCoach) &&
                oldCoach.Name != newCoach.Name)
            {
                return new Tuple<Coach, Coach>(newCoach, oldCoach);
            }
            else if (oldCoach == null && newCoach == null)
            {
                return null;
            }
            else if (oldCoach == null || newCoach == null)
            {
                // Cover case where there weren't enough new coaches to fill all positions
                return new Tuple<Coach, Coach>(newCoach, oldCoach);
            }

            return null;
        }

        public static string ToDiffLine(Tuple<Coach, Coach> diff, Dictionary<string, Coach> oldStaff)
        {
            Coach formerJob = null;
            if (diff.Item1 != null && !oldStaff.TryGetValue(diff.Item1.Key, out formerJob))
            {
                formerJob = null;
            }

            try
            {
                return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    diff.Item1 == null ? "TBH" : diff.Item1.Name,
                    diff.Item1 == null ? string.Empty : diff.Item1.Id.ToString(),
                    diff.Item1 == null ? string.Empty : diff.Item1.Team.Name,
                    diff.Item1 == null ? string.Empty : diff.Item1.TeamId.ToString(),
                    diff.Item1 == null ? string.Empty : diff.Item1.Position.ToJob(),
                    diff.Item1 == null ? string.Empty : diff.Item1.Position.ToString(),
                    diff.Item2.Name,
                    diff.Item2.Id,
                    formerJob == null ? string.Empty : formerJob.Position.ToPositionAbbreviation(),
                    formerJob == null ? string.Empty : formerJob.Team.Name,
                    formerJob == null ? string.Empty : formerJob.TeamId.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Diff()
        {
            var newCoaches = FromCsv("newcoaches.csv");
            var oldCoaches = FromCsv("coaches.csv");
            var allOldCoaches = oldCoaches.Values.SelectMany(c => c).ToDictionary(coach => coach.Key);
            List<Tuple<Coach, Coach>> coachDiff = new List<Tuple<Coach, Coach>>();

            foreach (var key in newCoaches.Keys.OrderBy(k => newCoaches[k].First().Team.Name))
            {
                var newStaff = newCoaches[key].ToDictionary(coach => coach.Position, coach => coach);
                var oldStaff = oldCoaches[key].ToDictionary(coach => coach.Position, coach => coach);

                for (int i = 0; i < 3; i++)
                {
                    var tuple = CoachesAreDifferent(newStaff, oldStaff, i);

                    if (tuple != null)
                        coachDiff.Add(tuple);
                }
            }

            // now we have a list of different coaches
            coachDiff.ToCsvFile(
                new[] { "Name", "CoachId", "Team", "TeamId", "PositionName", "Position", "OldCoachName", "OldCoachCoachId", "FormerPosition", "FormerTeam", "FormerTeamId" },
                tuple => ToDiffLine(tuple, allOldCoaches),
                "coachingChanges.csv"
                );

            // find teams with missing coaches
            var teamsWithMissingCoaches = newCoaches.Where(kvp => kvp.Value.Length < 3);
            var sb = new StringBuilder();
            foreach (var team in teamsWithMissingCoaches)
            {
                List<int> missingCoach = new List<int>();
                // no head coach
                if (team.Value.Where(c => c.Position == 0).Any() == false)
                {
                    missingCoach.Add(0);
                }

                // no OC
                if (team.Value.Where(c => c.Position == 1).Any() == false)
                {
                    missingCoach.Add(1);
                }

                // no DC
                if (team.Value.Where(c => c.Position == 2).Any() == false)
                {
                    missingCoach.Add(2);
                }

                foreach (var missing in missingCoach)
                {
                    sb.AppendLine(string.Format("({0}) {1} missing : {2}", team.Value.First().TeamId, team.Value.First().Team.Name, missing));
                }
            }

            Utility.WriteData(@".\archive\reports\missingcoaches.txt", sb.ToString(), true);
        }

        public static Dictionary<int, Coach[]> FromCsv(string file)
        {
            return FromCsv(@".\archive\reports\", file);
        }


        public static Dictionary<int, Coach[]> FromCsv(string path, string file)
        {
            var lines = Path.Combine(path, file).ReadAllLines().Skip(1);
            var coaches = lines.Select(line => FromCsvLine(line)).OrderBy(c => c.TeamId).ThenBy(c => c.Position).ToArray();
            return coaches.GroupBy(c => c.TeamId).ToDictionary(g => g.Key, g => g.ToArray());
        }

        public static Coach FromCsvLine(string line)
        {
            var parts = line.Split(',');
            return new Coach
            {
                Name = parts[0],
                Id = parts[1].ToInt32(),
                TeamId = parts[3].ToInt32(),
                Position = parts[5].ToInt32()
            };
        }

        public static void ToCoachCsv()
        {
            var file = IsPostSeason ? "newcoaches.csv" : "coaches.csv";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name,CoachId,Team,TeamId,PositionName,Position,Age,YWT,CoachRating,Level,CareerRecord,TeamRecord");
            foreach (var coach in Coaches.Values.Where(c => c.TeamId.IsValidTeam()).OrderBy(c => c.Team.Name).ThenBy(c => c.Position).ToArray())
            {
                sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                    coach.Name, coach.Id, coach.Team.Name, coach.TeamId, coach.Position.ToJob(), coach.Position, coach.Age, coach.YearsWithTeam, coach.Rating, coach.Level, coach.CareerRecord, coach.TeamRecord));
            }

            Utility.WriteData(@".\archive\reports\" + file, sb.ToString());
        }
        private int coachId = 0;

        [DataMember]
        public int AlmaMaterId { get; set; }

        [DataMember]
        public string AlmaMaterName
        {
            get
            {
                try
                {
                    var team = this.AlmaMater;
                    return team == null ? string.Empty : team.Name;
                }
                catch
                {
                    return PocketScout.TeamNameLookup(this.AlmaMaterId) ?? string.Empty;
                }
            }
            set { }
        }

        public Team AlmaMater
        {
            get
            {
                Team team = null;
                return Team.Teams.TryGetValue(this.AlmaMaterId, out team) ? team : null;
            }
        } 

        public Team Team { get { return Team.Teams[this.TeamId]; } }
        [DataMember]
        public int Id
        {
            get
            {
                return coachId;
            }
            set
            {
                if (ContinuationData.Instance != null && ContinuationData.Instance.NewToOldCoachMap.ContainsKey(value))
                {
                    coachId = ContinuationData.Instance.NewToOldCoachMap[value];
                }
                else
                {
                    coachId = value;
                }
            }
        }
        public int TeamId { get; set; }
        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Name
        {
            get
            {
                return string.Format("{0} {1}", this.FirstName, this.LastName);
            }
            set
            {
                var spaceIdx = value.IndexOf(' ');
                this.FirstName = value.Substring(0, spaceIdx);
                this.LastName = value.Substring(spaceIdx + 1);
            }
        }
        [DataMember]
        public int Age { get; set; }
        public int ContractLength { get; set; }
        public int YearsIntoContract { get; set; }
        [DataMember]
        public int YearsWithTeam { get; set; }
        public string OriginalJob { get; set; }
        public string LastJob { get; set; }
        [DataMember]
        public int CareerWin { get; set; }
        [DataMember]
        public int CareerLoss { get; set; }
        [DataMember]
        public int TeamWin { get; set; }
        public int Rating { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        [DataMember]
        public int TeamLoss { get; set; }
        [DataMember]
        public int YearsAsHeadCoach { get; set; }
        [DataMember]
        public string CareerRecord { get { return (CareerWin + CareerLoss) > 0 ? string.Format("{0}-{1}", CareerWin, CareerLoss) : string.Empty; } set { } }
        [DataMember]
        public string TeamRecord { get { return (TeamWin + TeamLoss) > 0 ? string.Format("{0}-{1}", TeamWin, TeamLoss) : string.Empty; } set { } }
        [DataMember]
        public int WinningSeasons { get; set; }
        [DataMember]
        public int BowlWins { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int CareerConferenceChampionships { get; set; }
        [DataMember(EmitDefaultValue=false)]
        public int CareerNationalChampionships { get; set; }
        [DataMember]
        public int ConferenceChampionships { get; set; }
        [DataMember]
        public int NationalChampionships { get; set; }
        [DataMember]
        public int OffPlaybookId { get; set; }
        [DataMember]
        public DefensivePlaybook DefPlaybookId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int CoachBowlWin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int CoachBowlLoss { get; set; }
        [DataMember(EmitDefaultValue=false)]
        public int AllAmericans { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Top25Classes { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int CoachOfYearAwards { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Top25Win { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Top25Loss { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RivalWin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RivalLoss { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int LongestWinStreak { get; set; }

        public string Key
        {
            get
            {
                return this.Id.ToString() + this.Name;
            }
        }

        public static void CreateCoachingChangePage()
        {
            using (var tw = new StreamWriter("./Archive/Reports/CoachingChanges.html", false))
            {
                    Utility.WriteNavBarAndHeader(tw, "Coaching Changes", "loadCoachChangeData");
                    tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
                    tw.Write("<tr><td width=900 align=center colspan=10><table id=\"coachTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Coaching Changes</b></td></tr>");
                    tw.Write("</table><br></td></tr>");
                    tw.Write("</table><br></td></tr>");
                    tw.Write("</table></body>");
            }
        }

        public static void CreatePage(MaddenDatabase db, bool isPreseason)
        {
            Create(db);
            Team.Create(db, isPreseason);
            ToCoachCsv();

            using (var tw = new StreamWriter("./Archive/Reports/coaches.html", false))
            {
                try
                {
                    Utility.WriteNavBarAndHeader(tw, "Coaches", "loadCoachData");
                    tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
                    tw.Write("<tr><td class=C3 width=900 align=center colspan=10><a href='CoachingChanges.html'><b>Coaching Changes</b></tr>");
                    tw.Write("<tr><td width=900 align=center colspan=10><table id=\"coachTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Coaches</b></td></tr>");
                    tw.Write("<tr>");
                    tw.Write("<td class=C10 width=28%>Name</td>");
                    tw.Write("<td class=C10 width=14%>Team</td>");
                    tw.Write("<td class=C10 width=13%>Position</td>");
                    tw.Write("<td class=C10 width=5%>Age</td>");
                    tw.Write("<td class=C10 width=5%>Seasons<br>W/ Team</td>");
                    tw.Write("<td class=C10 width=5%>Coaching<br>Rating</td>");
                    tw.Write("<td class=C10 width=5%>Level</td>");
                    tw.Write("<td class=C10 width=15%>Career Record</td>");
                    tw.Write("<td class=C10 width=15%>Team Record</td>");
                    tw.Write("</tr>");
                    tw.Write("</table><br></td></tr>");

                }
                catch { }
                finally
                {
                    tw.Write("</table><br></td></tr>");
                    tw.Write("</table></body>");
                }
            }

        }
    }
}