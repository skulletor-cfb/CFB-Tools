using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    public enum AllAmericanTeam
    {
        First = 0,
        Second = 1,
        Freshman = 2
    }
    public class AllAmerican
    {
        public static List<AllAmerican> AllAmericans { get; set; }
        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            if (AllAmericans != null)
                return;

            PlayerDB.Create(db);
            Conference.Create(db);
            Team.Create(db,isPreseason);
            BowlChampion.Create(db);

            AllAmericans = MaddenTable.FindMaddenTable(db.lTables, "AAPL").lRecords.Where(mr => mr["SEYR"].ToInt32() == BowlChampion.DynastyFileYear && PlayerDB.Players.ContainsKey(mr["PGID"].ToInt32())).Select(mr =>
                new AllAmerican
                {
                    AllAmericanTeam = (AllAmericanTeam)mr["TTYP"].ToInt32(),
                    ReturningAllAmerican = mr["ARET"].ToInt32() != 0,
                    Position = mr["PPOS"].ToInt32(),
                    PlayerId = mr["PGID"].ToInt32(),
                    ConferenceId = mr["CGID"].ToInt32()
                }).OrderBy(p => p.ConferenceId).ThenBy(p => p.AllAmericanTeam).ThenBy(p => p.Position).ToList();
        }

        public static void CreateReport(bool isPreseason = false)
        {
            var dict = AllAmericans.GroupBy(aa => new AllAmericanTeamKey { ConferenceId = aa.ConferenceId, Team = aa.AllAmericanTeam }).ToDictionary(g => g.Key, g => g.ToList());
            var finalList = dict.Values.Select(list => DeriveTopReturner(list,isPreseason)).SelectMany(a => a).ToArray();

            var allAmericanFile = isPreseason ? "ps-aaac.csv" : "aaac.csv";
            finalList.ToCsvFile(
                new string[] { "ConfId", "ConferenceName", "PlayerName", "TeamNum", "PlayerTeam", "PositionName", "Position", "TeamId", "Height", "Weight", "PlayerYear", "DisplayPosition", "Ovr" },
                ToCsvLine,
                allAmericanFile);

            var file = isPreseason ? "./Archive/Reports/PreseasonAllAmericans.html" : "./Archive/Reports/AllAmericans.html";
            using (var tw = new StreamWriter(file, false))
            {
                Utility.WriteNavBarAndHeader(tw, "All Americans", "loadaaacData", string.Format("{0},'{1}'", 14, allAmericanFile));
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table id='mainTable' cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src='../HTML/Logos/FCS.jpg'></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");

                var sb = new StringBuilder();
                var aaTeams = Conference.Conferences.Values.Where(c => c.Id != 17 && Team.Teams.Values.Any(t => t.ConferenceId == c.Id)).Select(c => new { Name = c.Name, Id = c.Id }).ToList();
                aaTeams.Insert(0, new { Name = "All American", Id = 14 });

                foreach (var cc in aaTeams)
                {
                    sb.Append(string.Format("| <a href='#' onclick='loadaaacData({0})'>{1}</a> | ", cc.Id, cc.Name));
                }

                tw.Write(string.Format("<tr><td class=c3 colspan=3><b>Links:  {0}</b></td></tr>", sb.ToString()));
                CreateRosterTable(tw, "1st Team ", "team1");
                CreateRosterTable(tw, "2nd Team ", "team2");
                CreateRosterTable(tw, "Freshman ", "teamfrosh");
            }
        }

        public static void CreateRosterTable(TextWriter tw, string header, string tableId)
        {
            tw.WriteLine(string.Format("<tr><td width=300 align=left colspan=8><table id=\"{0}\" cellspacing=1 cellpadding=2 width=100% class=standard><tr><td id='aateamHeader' class=c2 colspan=20><b>{1}</b></td></tr>", tableId, header));
            tw.WriteLine(@"<tr><td class=c7 width=10%><b><center>Position</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=20%><b><center>Name</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=20%><b><center>Team</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=10%><b><center>Height</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=10%><b><center>Weight</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=10%><b><center>Class</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=10%><b><center>PlayerPosition</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=10%><b><center>Ovr</center></b></td>");
            tw.WriteLine(@"</tr>");
            tw.WriteLine("</table></td>");
            tw.WriteLine("</tr>");
        }

        static List<AllAmerican> DeriveTopReturner(List<AllAmerican> players, bool isPreseason)
        {
            var year = BowlChampion.CurrentYear;
            if (isPreseason)
                year--;

            var topReturner = players.OrderByDescending(p => p.Player.GetStatsForYear(year).GetValueOrDefault(PlayerStats.KRYds, 0) + p.Player.GetStatsForYear(year).GetValueOrDefault(PlayerStats.PRYds, 0)).FirstOrDefault();

            if (topReturner != null)
            {
                topReturner.Position = 1000;
                topReturner.Yards = topReturner.Player.CurrentYearStats.GetValueOrDefault(PlayerStats.KRYds, 0) + topReturner.Player.CurrentYearStats.GetValueOrDefault(PlayerStats.PRYds, 0);
            }

            // now we need to order
            int[][] order = new[]
            {
                new [] {0},  //QB
                new []{1},   //HB
                new []{1,2},   //HB or FB
                new []{3},   //WR
                new []{4},   //TE
                new []{5,9},   //OT
                new []{6,8},   //OG
                new []{7},   // C
                new []{10,11},   //DE
                new []{12},   // DT
                new []{13,15},   //OLB
                new []{14},   // MLB
                new []{16},   // CB
                new []{17},   // FS
                new []{18},   // SS
                new []{19},   // K
                new []{20},   // P
                new []{1000},   // RET
            };

            List<string> positionOverride = new List<string>(new string[order.Length]);
            positionOverride[5]= "OT";
            positionOverride[6]= "OG";
            positionOverride[8]= "DE";
            positionOverride[10]= "OLB";
            positionOverride[order.Length - 1] = "RET";

            List<AllAmerican> result = new List<AllAmerican>();
            for (int i = 0; i < order.Length; i++)
            {
                var sublist = players.Where(p => order[i].Contains(p.Position)).ToArray();
                foreach (var s in sublist)
                {
                    players.Remove(s);
                    s.DisplayPosition = positionOverride[i] ?? s.PositionName;
                }

                result.AddRange(sublist);
            }

            return result; 
        }

        public static string ToCsvLine(AllAmerican aa)
        {
            return string.Join
                (",",
                new object[] { 
                    aa.ConferenceId,
                    aa.ConferenceId==14? "American":aa.Conference.Name,
                    aa.Player.FirstName + " " + aa.Player.LastName , 
                    (int)aa.AllAmericanTeam,
                    aa.Player.Team.Name,
                    aa.Player.PositionName,
                    aa.Position,
                    aa.Player.TeamId,
                    aa.Player.Height.CalculateHgt(),
                    aa.Player.Weight,
                    aa.Player.CalculateYear(0,false),
                    aa.DisplayPosition,
                    aa.Player.Ovr
                }
                );
        }

        public int PlayerId { get; set; }
        public int ConferenceId { get; set; }
        public AllAmericanTeam AllAmericanTeam { get; set; }
        public int TeamId
        {
            get
            {
                return this.Player.TeamId;
            }
        }
        public int Yards { get; set; }
        public int Position { get; set; }
        public bool ReturningAllAmerican { get; set; }
        public string PositionName
        {
            get
            {
                return this.Position.ToPositionName();
            }
        }
        public Team Team
        {
            get
            {
                return Team.Teams[this.TeamId];
            }
        }

        public Conference Conference
        {
            get
            {
                return Conference.Conferences[this.ConferenceId];
            }
        }
        public Player Player
        {
            get
            {
                return PlayerDB.Players[this.PlayerId];
            }
        }

        public string DisplayPosition { get; set; }
    }


    public class AllAmericanTeamKey
    {
        public int ConferenceId { get; set; }
        public AllAmericanTeam Team { get; set; }
        public override bool Equals(object obj)
        {
            var other = obj as AllAmericanTeamKey;
            return other != null && other.ConferenceId == this.ConferenceId && other.Team == this.Team;
        }

        public override int GetHashCode()
        {
            return this.ConferenceId << 8 | (int)this.Team;
        }
    }
}

/*
AAPL
CGID = conf id
PGID = player id
TTYP = team numer (0=1 , 1=2 , 2 = frosh)
SEYR = Year ? 
PPOS = Position
ARET = Returning All American

14 = All American
0-13 conference
*/