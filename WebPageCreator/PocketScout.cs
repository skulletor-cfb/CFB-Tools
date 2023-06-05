using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Linq ;
using System.Xml;
using BetterListViewNS;
using MC02Handler;
using ListViewEx;

namespace EA_DB_Editor
{
    public class PocketScout
    {

        public static int Team_ID_GLOBAL;


        public static void CreateStandingsPage(MaddenDatabase db)
        {
            using (var tw = new StreamWriter("./Archive/Reports/Standings.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Conference Standings", "loadStandingsData", Utility.StartingYear.ToString());
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table id='mainTable' cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src='../HTML/Logos/FCS.jpg'></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");

                var sb = new StringBuilder();
                foreach (var cc in Conference.Conferences.Values.Where(c => c.Id != 17 && Team.Teams.Values.Any(t => t.ConferenceId == c.Id)).OrderBy(c => c.Name))
                {
                    sb.Append(string.Format("| <a href=Standings.html#{0}>{1}</a> | ", cc.Name.Replace(" ", string.Empty), cc.Name));
                }

                tw.Write(string.Format("<tr><td class=c3 colspan=3><b>Links:  {0}</b></td></tr>", sb.ToString()));
            }
        }

        public static void Polls(MaddenDatabase db)
        {

            TextWriter tw;

            // write out the BCS rank web page and csv data
            using (tw = new StreamWriter("./Archive/Reports/BCS_Rankings.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "BCS Rankings", "loadBCSData");
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/BCSRanking.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr><td class=c3 width=800 align=center colspan=8><b>| <a href='BCS_Rankings.html'>BCS Rankings</a> | | <a href='Poll.html?type=coach'>Coaches Poll</a> | | <a href='Poll.html?type=media'>Media Poll</a> |</b></td></tr> <tr>		<td width=800 align=center colspan=8>			<table id=\"bcsTable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=6><b>BCS Rankings</b></td>				</tr>				<tr>					<td class=C7 width=10%>Rank</td>					<td class=C7 width=50%>Team</td>					<td class=C7 width=10%>W-L</td>					<td class=C7 width=10%>Coaches</td>					<td class=C7 width=10%>Media</td>					<td class=C7 width=10%>Prv</td>				</tr>");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Record,Media,Coaches,BCSPrevious");
            foreach (var team in Team.Teams.Values.Where(t => t.BCSRank >= 1).OrderBy(t => t.BCSRank).Take(126))
            {
                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4},{5},{6}",
                    team.Name, team.Id, team.Win, team.Loss, team.MediaPollRank, team.CoachesPollRank, team.BCSPrevious));
            }
            Utility.WriteData(@".\archive\reports\bcs.csv", sb.ToString());


            // write out the general poll web page and csv data files
            using (tw = new StreamWriter("./Archive/Reports/Poll.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadPollData");
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=5>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img id=\"pollimage\" border=0 src=..//HTML//Logos//MediaPoll.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr><td class=c3 width=800 align=center colspan=8><b>| <a href='BCS_Rankings.html'>BCS Rankings</a> | | <a href='Poll.html?type=coach'>Coaches Poll</a> | | <a href='Poll.html?type=media'>Media Poll</a> |</b></td></tr> <tr>		<td width=800 align=center colspan=8>			<table id=\"pollTable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td  class=c2 colspan=6><b id=\"pollHeader\">Media Poll</b></td>				</tr>				<tr>					<td class=c7 width=10%>Rank</td>					<td class=c7 width=50%>Team</td>					<td class=c7 width=10%>W-L</td>					<td class=c7 width=10%>Points</td>					<td class=c7 width=10%>FPV</td>					<td class=c7 width=10%>Prv</td>				</tr>");
            }

            Team.WritePolls();

            // write out the home record table
            // write out the national titles table
            using (tw = new StreamWriter("./Archive/Reports/HFA.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Toughest Places to Play", "loadHFAData");
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=5>			<table cellpadding=0 cellspacing=0 width=100%>				</table>		</td>	</tr>	<tr>		<td width=800 align=center colspan=8>			<table id=\"nctable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=7><b>Records at Home</b></td>				</tr>				<tr>					<td class=c7 width=10%>Rank</td>					<td class=c7 width=10%></td>					<td class=c7 width=30%>Team</td>					<td class=c7 width=20%><a href='#' onclick='sortByHFAWins()'>All Time</a></td><td class=c7 width=10%><a href='#' onclick='sortByHFAPct()'>Pct</a></td>					<td class=c7 width=10%>Streak</a></td></tr>");
            }

            // write out the national titles table
            using (tw = new StreamWriter("./Archive/Reports/NC.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "National Championships", "loadNCData");
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=5>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src='../HTML/Logos/NC.jpg'></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr>		<td width=800 align=center colspan=8>			<table id=\"nctable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=7><b>National Championships</b></td>				</tr>				<tr>					<td class=c7 width=10%>Rank</td>					<td class=c7 width=10%></td>					<td class=c7 width=30%>Team</td>					<td class=c7 width=20%><a href='#' onclick='sortByAllTimeWins()'>All Time</a></td><td class=c7 width=10%><a href='#' onclick='sortByAllTimeWinPct()'>Pct</a></td>					<td class=c7 width=10%><a href='#' onclick='sortByNC()'>National Titles</a></td>					<td class=c7 width=10%><a href='#' onclick='sortByCC()'>Conf Titles</a></td>				</tr>");
            }

            sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Record,National,Conference,WinPct");
            foreach (var team in Team.Teams.Values.Where(t => t.Id.IsFCS()==false).OrderByDescending(t => t.NationalTitles).ThenByDescending(t => t.AllTimeWin))
            {
                if(team.Id.TeamNoLongerFBS())
                {
                    continue;
                }

                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4},{5},{6}",
                    team.Name, team.Id, team.AllTimeWin, team.AllTimeLoss, team.NationalTitles, team.ConferenceTitles, team.AllTimeWin * 1000 / (team.AllTimeWin + team.AllTimeTie + team.AllTimeLoss)));
            }
            Utility.WriteData(@".\archive\reports\nc.csv", sb.ToString());

            // write out the conference champions table
            using (tw = new StreamWriter("./Archive/Reports/CC.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Conference Championships", "loadCCData", Utility.StartingYear.ToString());
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img id='ccChampLogo' border=0 src=../HTML/Logos/FCS.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img id='ccChampTrophy' border=0 src=></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr><tr>		<td width=800 align=center colspan=8>			<table id='ccTable' cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=6><a href=\"CC.html\"><b>Conference Championships</b></a></td>				</tr>		");
            }

            sb = new StringBuilder();
            sb.AppendLine("Year,Team,TeamId,Conference,ConferenceId");
            foreach (var cc in ConferenceChampion.ConferenceChampions.OrderBy(cc => cc.Conference.Name).ThenByDescending(cc => cc.Year))
            {
                var team = cc.Team;
                string teamName = null;
                var teamId = cc.TeamId;

                if (team == null)
                {
                    teamName = TeamNameLookup(cc.TeamId);
                }
                else
                {
                    teamName = cc.Team.Name;
                }

                sb.AppendLine(string.Format("{0},{1},{2},{3},{4}", cc.Year, teamName, teamId, cc.Conference.Name, cc.ConferenceId));
            }
            Utility.WriteData(@".\archive\reports\cc.csv", sb.ToString());

            // standings
            CreateStandingsPage(db);

            // make sure the conference json file is written out
            Conference.ToJsonFile();
            sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Record,ConfRecord,DivRecord,Conference,Division");
            foreach (var team in Team.Teams.Values.Where(t => t.Conference.LeagueId == 0 && (t.Win + t.Loss) > 0).OrderBy(t => t.Conference.Name).ThenBy(t => t.DivisionName).ThenByDescending(t => t.ConferenceWin).ThenByDescending(t => (t.ConferenceWin + t.ConferenceLoss)).ThenByDescending(t => t.DivisionWin))
            {
                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4}-{5},{6}-{7},{8},{9}", team.Name, team.Id, team.Win, team.Loss, team.ConferenceWin, team.ConferenceLoss, team.DivisionWin, team.DivisionLoss, team.ConferenceId, team.DivisionId));
            }

            Utility.WriteData(@".\archive\reports\standings.csv", sb.ToString());

            // write out the conference round up table
            using (tw = new StreamWriter("./Archive/Reports/conference.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Conference Rankings", "loadConfRoundData");
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=6>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src='../HTML/Logos/FCS.jpg'></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr>		<td width=800 align=center colspan=8>			<table id=\"crtable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=8><b>Conference Rankings</b></td>				</tr>				<tr>					<td class=c7 width=10%>Rank</td>					<td class=c7 width=10%></td>					<td class=c7 width=30%>Conference</td>					<td class=c7 width=10%><a href='#' onclick='sortByTop6()'>Top 6 Teams</a></td>					<td class=c7 width=10%><a href='#' onclick='sortByWholeConf()'>All Teams</a></td>					<td class=c7 width=10%><a href='#' onclick='sortByBowlRecord()'>Bowl Record</a></td>");
                tw.Write("<td class=c7 width=10%><a href='#' onclick='sortByOOCRecord()'>OOC Record</a></td>");
                tw.Write("<td class=c7 width=10%><a href='#' onclick='sortByPOOCRecord()'>Power OOC Record</a></td>");
                tw.Write("</tr>");
            }

            sb = new StringBuilder();
            sb.AppendLine("Conference,ConfId,TopSix,All,BowlRecord,BowlPct,OOCRecord,OOCPct,POOCRecord,POOCPct");
            var confRankings = new Dictionary<int, Tuple<List<int>, List<int>, List<int>, List<int>, List<int>, List<int>, List<int>, Tuple<List<int>>>>();

            // build our DB
            foreach (var team in Team.Teams.Values.Where(t => t.ConferenceId != 5))
            {
                if (TeamSchedule.TeamSchedules.ContainsKey(team.Id) == false)
                    continue;

                try
                {
                    var schedule = TeamSchedule.TeamSchedules[team.Id];
                    var teamBowlGames = schedule.FlattenedListOfGames.Where(g => g.Week > 16 && g.GameNumber < 127).ToArray();
                    var teamOOCGames = schedule.FlattenedListOfGames.Where(g => g.OpponentId != 1023 && g.Opponent.ConferenceId != team.ConferenceId).ToArray();
                    var teamPowerOpponentGames = teamOOCGames.Where(g => g.OpponentId != 1023 && (Conference.PowerConferences.Contains(g.Opponent.ConferenceId) || Conference.PowerIndependents.Contains(g.Opponent.Id))).ToArray();

                    Tuple<List<int>, List<int>, List<int>, List<int>, List<int>, List<int>, List<int>, Tuple<List<int>>> conf = null;
                    if (confRankings.TryGetValue(team.ConferenceId, out conf) == false)
                    {
                        conf = new Tuple<List<int>, List<int>, List<int>, List<int>, List<int>, List<int>, List<int>, Tuple<List<int>>>(new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new Tuple<List<int>>(new List<int>()));
                        confRankings.Add(team.ConferenceId, conf);
                    }

                    conf.Item1.Add(team.CoachesPollRank);
                    conf.Item2.Add(team.MediaPollRank);
                    conf.Item3.Add(teamBowlGames.Count(g => g.DidIWin));
                    conf.Item4.Add(teamBowlGames.Count(g => !g.DidIWin));
                    conf.Item5.Add(teamOOCGames.Count(g => g.DidIWin));
                    conf.Item6.Add(teamOOCGames.Count(g => !g.DidIWin));
                    conf.Item7.Add(teamPowerOpponentGames.Count(g => g.DidIWin));
                    conf.Rest.Item1.Add(teamPowerOpponentGames.Count(g => !g.DidIWin));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{team.Id}: {team.Name}");
                }
            }

            var conferences = confRankings.Select(
                kvp =>
                new
                {
                    ConfId = kvp.Key,
                    Conference = Conference.Conferences[kvp.Key].Name,
                    BowlWin = kvp.Value.Item3.Sum(),
                    BowlLoss = kvp.Value.Item4.Sum(),
                    Top6 = kvp.Value.Item1.OrderBy(i => i).Take(6).Concat(kvp.Value.Item2.OrderBy(i => i).Take(6)).Sum() * 100 / 12,
                    All = kvp.Value.Item1.Concat(kvp.Value.Item2).Sum() * 100 / (kvp.Value.Item1.Count + kvp.Value.Item2.Count),
                    BowlPct = (kvp.Value.Item3.Sum() + kvp.Value.Item4.Sum()) == 0 ? 0 : kvp.Value.Item3.Sum() * 100 / (kvp.Value.Item3.Sum() + kvp.Value.Item4.Sum()),
                    OOCWin = kvp.Value.Item5.Sum(),
                    OOCLoss = kvp.Value.Item6.Sum(),
                    PowerOOCWin = kvp.Value.Item7.Sum(),
                    PowerOOCLoss = kvp.Value.Rest.Item1.Sum(),
                    OOCPct = kvp.Value.Item5.Sum() * 100 / (kvp.Value.Item5.Sum() + kvp.Value.Item6.Sum()),
                    PowerOOCPct = kvp.Value.Item7.Sum() * 100 / (kvp.Value.Item7.Sum() + kvp.Value.Rest.Item1.Sum()),
                }).OrderBy(c => c.Top6);

            foreach (var conf in conferences)
            {
                sb.AppendLine(string.Format("{0},{1},{2},{3},{4}-{5},{6},{7}-{8},{9},{10}-{11},{12}",
                    conf.Conference, conf.ConfId, conf.Top6, conf.All, conf.BowlWin, conf.BowlLoss, conf.BowlPct, conf.OOCWin, conf.OOCLoss, conf.OOCPct, conf.PowerOOCWin, conf.PowerOOCLoss, conf.PowerOOCPct));
            }

            Utility.WriteData(@".\archive\reports\confround.csv", sb.ToString());
        }

        public static void CreateTeamDirectoryPage()
        {
            using (var tw = new StreamWriter("./Archive/Reports/Teams.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "NCAA Football 2014", null, null);
                tw.Write("<body topmargin=0 leftmargin=0 marginheight=70 marginwidth=0><table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=800><center><img border=0 src=../HTML/Logos/FCS.jpg></center></td>				</tr>			</table>			</td>	</tr>	<tr>		<td width=800 align=center colspan=8>			<table cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=7><b><center>Teams</center></b></td>				</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsA><center>A</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=1><img src=../HTML/Logos/55/team1.png></a></td>					<td class=c3><a href=team.html?id=2><img src=../HTML/Logos/55/team2.png></a></td>					<td class=c3><a href=team.html?id=3><img src=../HTML/Logos/55/team3.png></a></td>					<td class=c3><a href=team.html?id=4><img src=../HTML/Logos/55/team4.png></a></td>					<td class=c3><a href=team.html?id=5><img src=../HTML/Logos/55/team5.png></a></td>					<td class=c3><a href=team.html?id=6><img src=../HTML/Logos/55/team6.png></a></td>						<td class=c3><a href=team.html?id=7><img src=../HTML/Logos/55/team7.png></a></td>					</tr>				<tr>					<td class=c3></td>					<td class=c3></td>					<td class=c3><a href=team.html?id=8><img src=../HTML/Logos/55/team8.png></a></td>					<td class=c3></td>						<td class=c3><a href=team.html?id=9><img src=../HTML/Logos/55/team9.png></a></td>					<td class=c3></td>					<td class=c3></td>				</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsB><center>B</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=10><img src=../HTML/Logos/55/team10.png></a></td>					<td class=c3><a href=team.html?id=11><img src=../HTML/Logos/55/team11.png></a></td>					<td class=c3><a href=team.html?id=12><img src=../HTML/Logos/55/team12.png></a></td>					<td class=c3><a href=team.html?id=13><img src=../HTML/Logos/55/team13.png></a></td>					<td class=c3><a href=team.html?id=14><img src=../HTML/Logos/55/team14.png></a></td>					<td class=c3><a href=team.html?id=15><img src=../HTML/Logos/55/team15.png></a></td>						<td class=c3><a href=team.html?id=16><img src=../HTML/Logos/55/team16.png></a></td>				</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsC><center>C</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=17><img src=../HTML/Logos/55/team17.png></a></td>					<td class=c3><a href=team.html?id=18><img src=../HTML/Logos/55/team18.png></a></td>					<td class=c3><a href=team.html?id=19><img src=../HTML/Logos/55/team19.png></a></td>					<td class=c3><a href=team.html?id=20><img src=../HTML/Logos/55/team20.png></a></td>					<td class=c3><a href=team.html?id=21><img src=../HTML/Logos/55/team21.png></a></td>					<td class=c3><a href=team.html?id=22><img src=../HTML/Logos/55/team22.png></a></td>						<td class=c3><a href=team.html?id=23><img src=../HTML/Logos/55/team23.png></a></td>				</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsD><center>D-F</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=24><img src=../HTML/Logos/55/team24.png></a></td>					<td class=c3><a href=team.html?id=25><img src=../HTML/Logos/55/team25.png></a></td>					<td class=c3><a href=team.html?id=26><img src=../HTML/Logos/55/team26.png></a></td>					<td class=c3><a href=team.html?id=27><img src=../HTML/Logos/55/team27.png></a></td>					<td class=c3><a href=team.html?id=28><img src=../HTML/Logos/55/team28.png></a></td>					<td class=c3><a href=team.html?id=229><img src=../HTML/Logos/55/team229.png></a></td>					<td class=c3><a href=team.html?id=230><img src=../HTML/Logos/55/team230.png></a></td>					</tr>				<tr>					<td class=c3></td>					<td class=c3></td>					<td class=c3></td>							<td class=c3><a href=team.html?id=29><img src=../HTML/Logos/55/team29.png></a></td>						<td class=c3></td>					<td class=c3></td>					<td class=c3></td>					</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsG><center>G-K</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=30><img src=../HTML/Logos/55/team30.png></a></td>					<td class=c3><a href=team.html?id=31><img src=../HTML/Logos/55/team31.png></a></td>					<td class=c3><a href=team.html?id=233><img src=../HTML/Logos/55/team233.png></a></td>						<td class=c3><a href=team.html?id=32><img src=../HTML/Logos/55/team32.png></a></td>					<td class=c3><a href=team.html?id=33><img src=../HTML/Logos/55/team33.png></a></td>					<td class=c3><a href=team.html?id=34><img src=../HTML/Logos/55/team34.png></a></td>					<td class=c3><a href=team.html?id=35><img src=../HTML/Logos/55/team35.png></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=36><img src=../HTML/Logos/55/team36.png></a></td>						<td class=c3><a href=team.html?id=37><img src=../HTML/Logos/55/team37.png></a></td>					<td class=c3><a href=team.html?id=38><img src=../HTML/Logos/55/team38.png></a></td>					<td class=c3><a href=team.html?id=39><img src=../HTML/Logos/55/team39.png></a></td>						<td class=c3><a href=team.html?id=40><img src=../HTML/Logos/55/team40.png></a></td>					<td class=c3><a href=team.html?id=41><img src=../HTML/Logos/55/team41.png></a></td>					<td class=c3><a href=team.html?id=42><img src=../HTML/Logos/55/team42.png></a></td>					</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsL><center>L-M</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=43><img src=../HTML/Logos/55/team43.png></a></td>					<td class=c3><a href=team.html?id=44><img src=../HTML/Logos/55/team44.png></a></td>					<td class=c3><a href=team.html?id=45><img src=../HTML/Logos/55/team45.png></a></td>					<td class=c3><a href=team.html?id=46><img src=../HTML/Logos/55/team46.png></a></td>					<td class=c3><a href=team.html?id=47><img src=../HTML/Logos/55/team47.png></a></td>					<td class=c3><a href=team.html?id=48><img src=../HTML/Logos/55/team48.png></a></td>					<td class=c3><a href=team.html?id=49><img src=../HTML/Logos/55/team49.png></a></td>					</tr>				<tr>					<td class=c3><a href=team.html?id=50><img src=../HTML/Logos/55/team50.png></a></td>					<td class=c3><a href=team.html?id=51><img src=../HTML/Logos/55/team51.png></a></td>					<td class=c3><a href=team.html?id=52><img src=../HTML/Logos/55/team52.png></a></td>					<td class=c3><a href=team.html?id=53><img src=../HTML/Logos/55/team53.png></a></td>					<td class=c3><a href=team.html?id=54><img src=../HTML/Logos/55/team54.png></a></td>					<td class=c3><a href=team.html?id=55><img src=../HTML/Logos/55/team55.png></a></td>					<td class=c3><a href=team.html?id=56><img src=../HTML/Logos/55/team56.png></a></td>					</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsN><center>N</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=57><img src=../HTML/Logos/55/team57.png></a></td>					<td class=c3><a href=team.html?id=58><img src=../HTML/Logos/55/team58.png></a></td>					<td class=c3><a href=team.html?id=59><img src=../HTML/Logos/55/team59.png></a></td>					<td class=c3><a href=team.html?id=60><img src=../HTML/Logos/55/team60.png></a></td>					<td class=c3><a href=team.html?id=61><img src=../HTML/Logos/55/team61.png></a></td>					<td class=c3><a href=team.html?id=62><img src=../HTML/Logos/55/team62.png></a></td>					<td class=c3><a href=team.html?id=63><img src=../HTML/Logos/55/team63.png></a></td>					</tr>				<tr>					<td class=c3><a href=team.html?id=64><img src=../HTML/Logos/55/team64.png></a></td>					<td class=c3></td>					<td class=c3><a href=team.html?id=66><img src=../HTML/Logos/55/team66.png></a></td>					<td class=c3></td>					<td class=c3><a href=team.html?id=67><img src=../HTML/Logos/55/team67.png></a></td>					<td class=c3></td>					<td class=c3><a href=team.html?id=68><img src=../HTML/Logos/55/team68.png></a></td>				</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsO><center>0-R</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=69><img src=../HTML/Logos/55/team69.png></a></td>					<td class=c3><a href=team.html?id=70><img src=../HTML/Logos/55/team70.png></a></td>					<td class=c3><a href=team.html?id=71><img src=../HTML/Logos/55/team71.png></a></td>					<td class=c3><a href=team.html?id=72><img src=../HTML/Logos/55/team72.png></a></td>					<td class=c3><a href=team.html?id=73><img src=../HTML/Logos/55/team73.png></a></td>					<td class=c3><a href=team.html?id=74><img src=../HTML/Logos/55/team74.png></a></td>					<td class=c3><a href=team.html?id=75><img src=../HTML/Logos/55/team75.png></a></td>					</tr>				<tr>					<td class=c3><a href=team.html?id=234><img src=../HTML/Logos/55/team234.png></a></td>					<td class=c3><a href=team.html?id=76><img src=../HTML/Logos/55/team76.png></a></td>					<td class=c3><a href=team.html?id=77><img src=../HTML/Logos/55/team77.png></a></td>					<td class=c3></td>						<td class=c3><a href=team.html?id=78><img src=../HTML/Logos/55/team78.png></a></td>					<td class=c3><a href=team.html?id=79><img src=../HTML/Logos/55/team79.png></a></td>					<td class=c3><a href=team.html?id=80><img src=../HTML/Logos/55/team80.png></a></td>					</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsS><center>S</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=81><img src=../HTML/Logos/55/team81.png></a></td>					<td class=c3><a href=team.html?id=82><img src=../HTML/Logos/55/team82.png></a></td>					<td class=c3><a href=team.html?id=83><img src=../HTML/Logos/55/team83.png></a></td>					<td class=c3><a href=team.html?id=84><img src=../HTML/Logos/55/team84.png></a></td>					<td class=c3><a href=team.html?id=85><img src=../HTML/Logos/55/team85.png></a></td>					<td class=c3><a href=team.html?id=87><img src=../HTML/Logos/55/team87.png></a></td>					<td class=c3><a href=team.html?id=88><img src=../HTML/Logos/55/team88.png></a></td>					</tr>				<tr>					<td class=c3></td>						<td class=c3></td>					<td class=c3></td>						<td class=c3><a href=team.html?id=235><img src=../HTML/Logos/55/team235.png></a></td>							<td class=c3></td>					<td class=c3></td>					<td class=c3></td>					</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsT><center>T</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=89><img src=../HTML/Logos/55/team89.png></a></td>					<td class=c3><a href=team.html?id=90><img src=../HTML/Logos/55/team90.png></a></td>					<td class=c3><a href=team.html?id=91><img src=../HTML/Logos/55/team91.png></a></td>					<td class=c3><a href=team.html?id=92><img src=../HTML/Logos/55/team92.png></a></td>					<td class=c3><a href=team.html?id=93><img src=../HTML/Logos/55/team93.png></a></td>					<td class=c3><a href=team.html?id=94><img src=../HTML/Logos/55/team94.png></a></td>					<td class=c3><a href=team.html?id=95><img src=../HTML/Logos/55/team95.png></a></td>					</tr>				<tr>					<td class=c3><a href=team.html?id=143><img src=../HTML/Logos/55/team143.png></a></td>							<td class=c3></td>						<td class=c3><a href=team.html?id=96><img src=../HTML/Logos/55/team96.png></a></td>					<td class=c3></td>						<td class=c3><a href=team.html?id=97><img src=../HTML/Logos/55/team97.png></a></td>							<td class=c3></td>					<td class=c3><a href=team.html?id=218><img src=../HTML/Logos/55/team218.png></a></td>						</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsU><center>U</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=98><img src=../HTML/Logos/55/team98.png></a></td>					<td class=c3><a href=team.html?id=99><img src=../HTML/Logos/55/team99.png></a></td>					<td class=c3><a href=team.html?id=100><img src=../HTML/Logos/55/team100.png></a></td>					<td class=c3><a href=team.html?id=101><img src=../HTML/Logos/55/team101.png></a></td>					<td class=c3><a href=team.html?id=102><img src=../HTML/Logos/55/team102.png></a></td>					<td class=c3><a href=team.html?id=103><img src=../HTML/Logos/55/team103.png></a></td>					<td class=c3><a href=team.html?id=104><img src=../HTML/Logos/55/team104.png></a></td>					</tr>				<tr>					<td class=c3><a href=team.html?id=105><img src=../HTML/Logos/55/team105.png></a></td>							<td class=c3><a href=team.html?id=144><img src=../HTML/Logos/55/team144.png></a></td>							<td class=c3><a href=team.html?id=181><img src=../HTML/Logos/55/team181.png></a></td>					<td class=c3></td>						<td class=c3><a href=team.html?id=232><img src=../HTML/Logos/55/team232.png></a></td>					<td class=c3><a href=team.html?id=65><img src=../HTML/Logos/55/team65.png></a></td>						<td class=c3><a href=team.html?id=86><img src=../HTML/Logos/55/team86.png></a></td>					</tr>				<tr>					<td class=c7 colspan=7><a name=TeamsV><center>V-W</center></a></td>				</tr>				<tr>					<td class=c3><a href=team.html?id=106><img src=../HTML/Logos/55/team106.png></a></td>					<td class=c3><a href=team.html?id=107><img src=../HTML/Logos/55/team107.png></a></td>					<td class=c3><a href=team.html?id=108><img src=../HTML/Logos/55/team108.png></a></td>					<td class=c3><a href=team.html?id=109><img src=../HTML/Logos/55/team109.png></a></td>					<td class=c3><a href=team.html?id=110><img src=../HTML/Logos/55/team110.png></a></td>					<td class=c3><a href=team.html?id=111><img src=../HTML/Logos/55/team111.png></a></td>					<td class=c3><a href=team.html?id=112><img src=../HTML/Logos/55/team112.png></a></td>					</tr>				<tr>					<td class=c3><a href=team.html?id=113><img src=../HTML/Logos/55/team113.png></a></td>							<td class=c3></td>					<td class=c3><a href=team.html?id=114><img src=../HTML/Logos/55/team114.png></a></td>					<td class=c3></td>					<td class=c3><a href=team.html?id=115><img src=../HTML/Logos/55/team115.png></a></td>							<td class=c3></td>					<td class=c3><a href=team.html?id=211><img src=../HTML/Logos/55/team211.png></a></td>				</tr>			</table>		</td>	</tr></table></body>");
            }
        }

        public static void SOS_Rnk(MaddenDatabase db, bool isPreseason)
        {
            // SOS Rank only done after the season starts
            TeamSchedule.CalculateOpponentMetrics(db,false);
            TeamSchedule.ToSOSCsv(db,isPreseason);
            TextWriter tw;
            using (tw = new StreamWriter("./Archive/Reports/SOS_Rankings.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Strength of Schedule", "loadSOSData");
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=40 height=40></td></tr></table>");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/sos.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr>		<td width=800 align=center colspan=10>			<table id=\"sosTable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=10><b>SOS Rankings</b></td>				</tr>				<tr>					<td class=C7 width=10%>Rank</td>					<td class=C7 width=20%>Team</td>					<td class=C7 width=10%>W-L</td><td class=C7 width=10%><a href='#' onclick='sortByAvgRank()'>Opp Avg Rnk</a></td><td class=C7 width=10%>Opp W</td><td class=C7 width=10%>Opp L</td><td class=C7 width=10%><a href='#' onclick='sortByWinPct()'>Opp %</a></td>					<td class=C7 width=10%>BCS</td> <td class=C7 width=10%>Coaches</td>					<td class=C7 width=10%>Media</td></tr>");
            }
        }

        public static void Roster(bool isPreseason)
        {
            var file = isPreseason ? "PreseasonTeamRoster.html" : "teamroster.html";
            using (var tw = new StreamWriter("./Archive/Reports/"+file, false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamRosterData",isPreseason.ToString().ToLower());
                tw.Write(Utility.LoadTeamTemplate("teampagetemplate", isPreseason));
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"rosterTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td id=\"schoolNameHeader\" class=c2 colspan=13></td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"breakdownTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Roster Breakdown</b></td></tr>");
                tw.Write("</table><br>");
            }

            foreach (var team in Team.Teams.Values)
            {
                var sb = new StringBuilder();
                sb.AppendLine("No,Name,Year,Position,Height,Weight,Ovr,Spd,Acc,Agl,Str,Awr,City,State");

                if (PlayerDB.Rosters.ContainsKey(team.Id))
                {
                    foreach (var player in PlayerDB.Rosters[team.Id].OrderBy(p => p.Position).ThenByDescending(p => p.Ovr).ThenByDescending(p => p.Spd).ThenByDescending(p => p.Awr))
                    {
                        string city = string.Empty, state = string.Empty;
                        if (City.Cities.ContainsKey(player.City))
                        {
                            city = City.Cities[player.City].Name;
                            state = City.Cities[player.City].State;
                        }

                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                            player.Number, player.Name, player.CalculateYear(0), player.PositionName,
                            player.Height.CalculateHgt(), player.Weight, player.Ovr, player.Spd, player.Acc,
                            player.Agl, player.Str, player.Awr, city, state));
                    }

                    Utility.WriteData(string.Format(@".\archive\reports\teamroster{0}.csv", team.Id), sb.ToString());
                }
            }

            using (var tw = new StreamWriter("./Archive/Reports/team_ratings.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Team Ratings", "loadTeamRatingsData");
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
                tw.Write("<tr><td width=900 align=center colspan=13><table id=\"ratingsTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Team Ratings</b></td></tr>");
                tw.Write("<tr>");
                tw.Write("<td class=C10 width=5%>Rank</td>");
                tw.Write("<td class=C10 width=20%>School</td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamOvr()'>OVR</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamOff()'>OFF</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamQb()'>QB</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamRb()'>RB</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamRec()'>REC</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamOl()'>OL</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamDef()'>DEF</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamDl()'>DL</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamLb()'>LB</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamDb()'>DB</a></td>");
                tw.Write("<td class=C10 width=7%><a href='#' onclick='sortByTeamST()'>ST</a></td>");
                tw.Write("</tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            var sb2 = new StringBuilder();
            sb2.AppendLine("TeamId,Name,OVR,OFF,QB,RB,WR,OL,DEF,DL,LB,DB,ST");
            foreach (var team in Team.Teams.Values.Where(t=> PlayerDB.Rosters.ContainsKey(t.Id)).OrderByDescending( t=> t.TeamRatingOVR))
            {
                sb2.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", team.Id, team.Name, team.TeamRatingOVR, team.TeamRatingOFF, team.TeamRatingQB, team.TeamRatingRB, team.TeamRatingWR, team.TeamRatingOL, team.TeamRatingDEF, team.TeamRatingDL, team.TeamRatingLB, team.TeamRatingDB, team.TeamRatingST));
            }
            Utility.WriteData(string.Format(@".\archive\reports\teamratings.csv"), sb2.ToString());
        }

        public static void Freshmen(bool isPreseason)
        {
            var file = isPreseason ? "PreseasonTeamFrosh.html" : "teamfrosh.html";
            using (var tw = new StreamWriter("./Archive/Reports/" + file, false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamFroshData",isPreseason.ToString().ToLower());
                tw.Write(Utility.LoadTeamTemplate("teampagetemplate", isPreseason));
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"froshTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td id=\"schoolNameHeader\" class=c2 colspan=13></td></tr>");
                tw.Write("<tr><td class=C10 width=3%>#</td><td class=C10 width=17%>Name</td><td class=C10 width=8%>Year</td><td class=C10 width=8%>Pos</td><td class=C10 width=5%>Hgt</td><td class=C10 width=5%>Lbs</td><td class=C10 width=5%>Ovr</td><td class=C10 width=5%>Spd</td><td class=C10 width=5%>Acc</td><td class=C10 width=5%>Agl</td><td class=C10 width=5%>Str</td><td class=C10 width=5%>Awr</td><td class=C10 width=22%>Home Town</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            var sb = new StringBuilder();
            sb.AppendLine("No,Name,Year,Position,Height,Weight,Ovr,Spd,Acc,Agl,Str,Awr,City,State,TeamId");

            foreach (var team in Team.Teams.Values)
            {

                if (PlayerDB.Rosters.ContainsKey(team.Id))
                {
                    foreach (var player in PlayerDB.Rosters[team.Id].Where(p => p.IsRedShirt == false && p.Year == 0).OrderByDescending(p => p.Ovr).ThenByDescending(p => p.Spd).ThenByDescending(p => p.Awr))
                    {
                        string city = string.Empty, state = string.Empty;
                        if (City.Cities.ContainsKey(player.City))
                        {
                            city = City.Cities[player.City].Name;
                            state = City.Cities[player.City].State;
                        }

                        sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                            player.Number, player.Name, player.CalculateYear(0), player.PositionName,
                            player.Height.CalculateHgt(), player.Weight, player.Ovr, player.Spd, player.Acc,
                            player.Agl, player.Str, player.Awr, city, state, team.Id));
                    }
                }
            }

            Utility.WriteData(@".\archive\reports\teamfrosh.csv", sb.ToString());
        }

        public static void Team_Record_Book(bool isPreseason)
        {
            var file = isPreseason ? "PreSeasonTeamRecords.html" : "teamrecords.html";
            using (var tw = new StreamWriter("./Archive/Reports/"+file, false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamRecordData",isPreseason.ToString().ToLower());
                tw.Write(Utility.LoadTeamTemplate("teampagetemplate", isPreseason));
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"recordTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td id=\"schoolNameHeader\" class=c2 colspan=13></td></tr>");
                tw.Write("<tr><td class=C10 width=20%>Type</td>");
                tw.Write("<td class=C10 width=20%>Record</td>");
                tw.Write("<td class=C10 width=20%>Holder</td>");
                tw.Write("<td class=C10 width=20%>Stat</td>");
                tw.Write("<td class=C10 width=20%>Opp</td></tr>");
                tw.Write("</table></body>");
            }

            var sb = new StringBuilder();
            sb.AppendLine("RecordType,Description,Holder,Value,Opp,TeamId,Year");
            foreach (var kvp in SchoolRecord.SchoolRecords)
            {
                foreach (var record in kvp.Value.OrderBy(r => r.Description))
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                        Rec_Type(record.Type), Rec_Desc(record.Description), record.Holder, record.Value, record.Opponent, kvp.Key, record.Year));
                }
            }
            Utility.WriteData(@".\archive\reports\teamrecords.csv", sb.ToString());
        }

        public static void National_Record_Book()
        {
            using (var tw = new StreamWriter("./Archive/Reports/nationalrecordbook.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "NCAA Record Book", null);
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
                tw.Write("<tr><td width=900 align=center colspan=8><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>NCAA Record Book</b></td></tr>");
                tw.Write("<tr><td class=C10 width=40%>Record</td>");
                tw.Write("<td class=C10 width=5%>Year</td>");
                tw.Write("<td class=C10 width=20%>Holder</td>");
                tw.Write("<td class=C10 width=15%>Stat</td>");
                tw.Write("<td class=C10 width=20%>Opp</td></tr>");


                try
                {
                    NcaaRecord.ToJson();
                    foreach (var record in NcaaRecord.AllTimeRecords.OrderBy(r => r.Description))
                    {
                        //Start With Player Info
                        tw.Write("<td class=c3>" + Nat_Rec_Desc(record.Description) + "</td>");
                        if (record.Year == 63) { tw.Write("<td class=c3>Past</td>"); }
                        else { tw.Write("<td class=c3>" + (Utility.StartingYear + record.Year) + "</td>"); }
                        tw.Write("<td class=c3>" + record.Holder + "</td>");
                        tw.Write("<td class=c3>" + record.Value + "</td>");
                        tw.Write("<td class=c3>" + record.Opponent + "</td>");
                        tw.Write("</tr>");
                    }
                }
                catch { }
                {
                    tw.Write("</table><br></td></tr>");
                    tw.Write("</table></body>");
                }
            }
        }

        public static void Awards()
        {

            using (var tw = new StreamWriter("./Archive/Reports/award.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadAwardData", null);
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img id=\"awardLogo\" border=0 src=''></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"awardTable\" cellspacing=1 cellpadding=2 width=80% class=standard>");
                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            string currentAwardName = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Number,Name,Year,Position,Height,Weight,Ovr,Team,TeamId,AwardId,AwardName");
            foreach (var key in Award.Awards.Keys)
            {
                foreach (var award in Award.Awards[key].Where(a => a.Year == BowlChampion.CurrentYear).OrderBy(a => a.Rank))
                {
                    var name = Award.GetAwardName(award.Id);
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        award.Player.Number, award.Player.Name, award.Player.CalculateYear(0),
                        award.Player.PositionName, award.Player.Height.CalculateHgt(), award.Player.Weight,
                        award.Player.Ovr, award.Player.Team.Name, award.Player.TeamId, award.Id, currentAwardName == name ? string.Empty : name));
                    currentAwardName = name;
                }
            }

            Utility.WriteData(@".\archive\reports\awards.csv", sb.ToString());
        }

        public static void WriteStatLines(StringBuilder sb, IEnumerable<PlayerStats> playerStatBooks, string[] keys, int tableIndex, bool careerStats)
        {
            var statline = "{0},{1} {2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}";

            foreach (var p in playerStatBooks)
            {
                if (PlayerDB.Players.ContainsKey(p.PlayerId) == false || (p.Player.Year - (PlayerDB.CurrentYear - p.Year) < -1))
                    continue;

                sb.AppendLine(string.Format(statline,
                    p.Player.Number, p.Player.FirstName, p.Player.LastName, p.Player.CalculateYear(careerStats ? PlayerDB.CurrentYear - p.Year : 0), p.Player.PositionName, p.Player.Height.CalculateHgt(), p.Player.Weight,
                    keys[0] != null ? p.GetValueOrDefault(keys[0], 0).ToString() : string.Empty,
                    keys[1] != null ? p.GetValueOrDefault(keys[1], 0).ToString() : string.Empty,
                    keys[2] != null ? p.GetValueOrDefault(keys[2], 0).ToString() : string.Empty,
                    keys[3] != null ? p.GetValueOrDefault(keys[3], 0).ToString() : string.Empty,
                    keys[4] != null ? p.GetValueOrDefault(keys[4], 0).ToString() : string.Empty,
                    keys[5] != null ? p.GetValueOrDefault(keys[5], 0).ToString() : string.Empty,
                    keys[6] != null ? p.GetValueOrDefault(keys[6], 0).ToString() : string.Empty,
                    tableIndex,
                    careerStats ? p.Year.ToString() : string.Empty));
            }
        }

        public static void WritePlaceKickLines(StringBuilder sb, List<PlayerStats> playerStatBooks, int tableIndex, bool careerStats)
        {
            var statline = "{0},{1} {2},{3},{4},{5},{6},{7}/{8},{9}/{10},{11}/{12},{13},{14}/{15},{16},{17},{18},{19}";
            foreach (var p in playerStatBooks.OrderByDescending(p => p.Points))
            {
                if (PlayerDB.Players.ContainsKey(p.PlayerId) == false || (p.Player.Year - (PlayerDB.CurrentYear - p.Year) < -1))
                    continue;

                sb.AppendLine(string.Format(statline,
                    p.Player.Number, p.Player.FirstName, p.Player.LastName, p.Player.CalculateYear(careerStats ? PlayerDB.CurrentYear - p.Year : 0), p.Player.PositionName, p.Player.Height.CalculateHgt(), p.Player.Weight,
                    p.GetValueOrDefault(PlayerStats.FGMade, 0), p.GetValueOrDefault(PlayerStats.FGAtt, 0),
                    p.GetValueOrDefault(PlayerStats.FG40to49Made, 0), p.GetValueOrDefault(PlayerStats.FG40to49Att, 0),
                    p.GetValueOrDefault(PlayerStats.FGOver50Made, 0), p.GetValueOrDefault(PlayerStats.FGOver50Att, 0),
                    p.GetValueOrDefault(PlayerStats.FGLong, 0),
                    p.GetValueOrDefault(PlayerStats.XPMade, 0), p.GetValueOrDefault(PlayerStats.XPAtt, 0),
                    p.GetValueOrDefault(PlayerStats.KickOffTouchBack, 0),
                    p.GetValueOrDefault(PlayerStats.KickGamesPlayed, 0),
                    tableIndex,
                    careerStats ? p.Year.ToString() : string.Empty));
            }
        }

        public static void CreateTeamPlayerStats(MaddenDatabase db, bool careerStats)
        {
            TextWriter tw = null;
            PlayerDB.Create(db);

            foreach (var key in PlayerDB.Rosters.Keys.Where(k => k != 1023))
            {
                // we need temporary tables
                List<PlayerStats> passStats = new List<PlayerStats>();
                List<PlayerStats> rushStats = new List<PlayerStats>();
                List<PlayerStats> recStats = new List<PlayerStats>();
                List<PlayerStats> olStats = new List<PlayerStats>();
                List<PlayerStats> defStats = new List<PlayerStats>();
                List<PlayerStats> kickStats = new List<PlayerStats>();
                List<PlayerStats> puntStats = new List<PlayerStats>();
                List<PlayerStats> retStats = new List<PlayerStats>();

                // get the team roster
                var roster = PlayerDB.Rosters[key];
                foreach (var player in roster)
                {
                    foreach (var stats in player.Stats.Values.Where(s => s.Year == PlayerDB.CurrentYear || careerStats))
                    {
                        if (stats.GetValueOrDefault(PlayerStats.PassAttempts, 0) > 0)
                            passStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.RushAttempts, 0) > 0)
                            rushStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.Receptions, 0) > 0)
                            recStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.OLGamesplayed, 0) > 0)
                            olStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.Tackles, 0) > 0 || stats.GetValueOrDefault(PlayerStats.AssistedTackles, 0) > 0 || stats.GetValueOrDefault(PlayerStats.Interceptions, 0) > 0 || stats.GetValueOrDefault(PlayerStats.Sacks, 0) > 0 || stats.GetValueOrDefault(PlayerStats.ForcedFumble, 0) > 0 || stats.GetValueOrDefault(PlayerStats.PassDeflections, 0) > 0)
                            defStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.FGAtt, 0) > 0 || stats.GetValueOrDefault(PlayerStats.XPAtt, 0) > 0 || stats.GetValueOrDefault(PlayerStats.Kickoffs, 0) > 0)
                            kickStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.PuntYds, 0) > 0)
                            puntStats.Add(stats);

                        if (stats.GetValueOrDefault(PlayerStats.KickReturns, 0) > 0 || stats.GetValueOrDefault(PlayerStats.PuntReturns, 0) > 0)
                            retStats.Add(stats);
                    }
                }

                //after each player has been analyzed, we now need to start writing our csv
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("No,Name,PlayerClass,Position,Height,Weight,Stat1,Stat2,Stat3,Stat4,Stat5,Stat6,Games,TableIdx,Year");

                // write passing table
                WriteStatLines(sb, passStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.PassAttempts, 0)).ThenByDescending(p => p.GetValueOrDefault(PlayerStats.PassingYards, 0)),
                    new string[] { PlayerStats.PassAttempts, PlayerStats.Completions, PlayerStats.PassingYards, PlayerStats.PassingTD, PlayerStats.IntThrown, null, PlayerStats.OffGamesPlayed },
                    1, careerStats);

                // write rushing table
                WriteStatLines(sb, rushStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.RushingYards, 0)).ThenBy(p => p.GetValueOrDefault(PlayerStats.RushAttempts, 0)),
                    new string[] { PlayerStats.RushAttempts, PlayerStats.RushingYards, PlayerStats.RushingTD, PlayerStats.RushingYdsAfterContact, PlayerStats.BrokenTackles, PlayerStats.Fumbles, PlayerStats.OffGamesPlayed },
                    2, careerStats);

                // write receiving table
                WriteStatLines(sb, recStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.Receptions, 0)).ThenByDescending(p => p.GetValueOrDefault(PlayerStats.ReceivingYards, 0)),
                    new string[] { PlayerStats.Receptions, PlayerStats.ReceivingYards, PlayerStats.ReceivingTD, PlayerStats.ReceivingYAC, PlayerStats.Fumbles, null, PlayerStats.OffGamesPlayed },
                    3, careerStats);

                // write OL table
                WriteStatLines(sb, olStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.Pancakes, 0)).ThenBy(p => p.GetValueOrDefault(PlayerStats.SacksAllowed, 0)),
                    new string[] { PlayerStats.Pancakes, PlayerStats.SacksAllowed, null, null, null, null, PlayerStats.OLGamesplayed },
                    4, careerStats);

                // write def table
                WriteStatLines(sb, defStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.TotalTackles, 0)).ThenByDescending(p => p.GetValueOrDefault(PlayerStats.TackleForLoss, 0)).ThenByDescending(p => p.GetValueOrDefault(PlayerStats.Sacks, 0)),
                    new string[] { PlayerStats.TotalTackles, PlayerStats.TackleForLoss, PlayerStats.Sacks, PlayerStats.Interceptions, PlayerStats.PassDeflections, PlayerStats.ForcedFumble, PlayerStats.DefGP },
                    5, careerStats);

                // write place kick table
                WritePlaceKickLines(sb, kickStats, 6, careerStats);

                // write punt table
                WriteStatLines(sb, puntStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.NetPuntYards, 0)).ThenByDescending(p => p.GetValueOrDefault(PlayerStats.PuntYds, 0)),
                    new string[] { PlayerStats.NetPuntYards, PlayerStats.PuntYds, PlayerStats.DownInside20, PlayerStats.PuntLong, PlayerStats.PuntsBlocked, PlayerStats.PuntTouchback, PlayerStats.KickGamesPlayed },
                    7, careerStats);

                // write return table
                WriteStatLines(sb, retStats.OrderByDescending(p => p.GetValueOrDefault(PlayerStats.KickReturns, 0)).ThenByDescending(p => p.GetValueOrDefault(PlayerStats.KRYds, 0)),
                    new string[] { PlayerStats.KickReturns, PlayerStats.KRYds, PlayerStats.KRTD, PlayerStats.PuntReturns, PlayerStats.PRYds, PlayerStats.PRTD, PlayerStats.ReturnGamesPlayed },
                    8, careerStats);

                Utility.WriteData(string.Format(@".\archive\reports\team{0}{1}stat.csv", key, careerStats ? "c" : "p"), sb.ToString());
            }

            using (tw = new StreamWriter("./Archive/Reports/teampstat.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamStatsData", string.Format("{0},{1}", (PlayerDB.CurrentYear + Utility.StartingYear).ToString(), Utility.StartingYear));
                
                // no stats in preseason
                tw.Write(Utility.LoadTeamTemplate("teampagetemplate", false));
                tw.Write(File.ReadAllText(@".\archive\html\PlayerStatstemplate"));
            }
        }

        public static void TeamOffStats()
        {
            using (var tw = new StreamWriter("./Archive/Reports/TeamStats.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Team Offensive Stats", "loadTeamOffData", null);
                tw.Write(File.ReadAllText(@".\archive\html\TeamOffStatTemplate"));
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Record,OffYds,PassAtt,PassYds,PassTD,RushAtt,RushYds,RushTD");
            Team currentTeam;
            int currentRank  =1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderByDescending(t => (10 * t.OffensiveYards) / t.Team.Games))
            {
                var games = off.Team.Games;
                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4},{5},{6},{7},{8},{9},{10}",
                    off.Team.Name, off.Team.Id, off.Team.Win, off.Team.Loss,
                    10 * off.OffensiveYards / games, 10 * off.PassAttempts / games, 10 * off.PassYards / games, 10 * off.PassTD / games,
                    10 * off.RushAttempts / games, 10 * off.RushYards / games, 10 * off.RushTD / games));

                if( Team.Teams.TryGetValue(off.Team.Id,out currentTeam))
                {
                    currentTeam.OffensiveRankings = new TeamStatRanking
                    {
                        Overall = currentRank++
                    };
                }
            }

            Utility.WriteData(@".\archive\reports\offstats.csv", sb.ToString());

            // set offensive rankings
            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderByDescending(t => (10 * t.PassYards) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.OffensiveRankings.Passing = currentRank++;
                }
            }

            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderByDescending(t => (10 * t.RushYards) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.OffensiveRankings.Rushing = currentRank++;
                }
            }


            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderByDescending(t => (1000 * t.RushTD) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.OffensiveRankings.RushingTD = currentRank++;
                }
            }

            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderByDescending(t => (1000 * t.PassTD) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.OffensiveRankings.PassingTD = currentRank++;
                }
            }

            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderBy(t => (1000 * t.Turnovers) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.OffensiveRankings.Turnovers = currentRank++;
                }
            }
        }

        public static void TeamStatsDef()
        {
            using (var tw = new StreamWriter("./Archive/Reports/TeamStatsDef.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Team Defensive Stats", "loadTeamDefData", null);
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0><tr>");
                tw.Write("<td width=100% align=center colspan=4><table cellpadding=0 cellspacing=0 width=100%><tr>");
                tw.Write("<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/FCS.jpg></center></td><td class=c8></td>");
                tw.Write("</tr></table></td></tr>	 <tr><td class=c3 width=800 align=center colspan=8><b>| <a href='TeamStats.html'>Team Stats Offense</a> | | <a href='TeamStatsDef.html'>Team Stats Defense</a> |</b></td></tr><tr><td width=800 align=center colspan=8><table id=\"defTable\" cellspacing=1 cellpadding=2 width=80% class=standard>");
                tw.Write("<tr><td class=c2 colspan=10><b>Team Defensive Yards</b></td></tr>");
                tw.Write("<tr><td class=c7 width=15%>Rank</td>");
                tw.Write("<td class=c7 width=25%>Team</td>");
                tw.Write("<td class=c7 width=15%>W-L</td>");
                tw.Write("<td class=c7 width=15%><a href='#' onclick='sortByDefYds()'>Def<br>Yds</a></td>");
                tw.Write("<td class=c7 width=15%><a href='#' onclick='sortByDefPassYds()'>Pass<br>Yds</a></td>");
                tw.Write("<td class=c7 width=15%><a href='#' onclick='sortByDefRushYds()'>Rush<br>Yds</a></td>");
                tw.Write("</tr>");
                tw.Write("</table><br></td></tr></table></body>");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Record,DefYds,PassYds,RushYds");
            Team currentTeam;
            int currentRank = 1;

            foreach (var def in TeamSeasonStats.TeamStats.Values.OrderBy(t => (10 * t.DefensiveYards) / t.Team.Games))
            {
                var games = def.Team.Games;
                sb.AppendLine(string.Format("{0},{1},{2}-{3},{4},{5},{6}",
                    def.Team.Name, def.Team.Id, def.Team.Win, def.Team.Loss,
                   10 * def.DefensiveYards / games, 10 * def.PassYardsAllowed / games, 10 * def.RushingYardsAllowed / games));

                if (Team.Teams.TryGetValue(def.Team.Id, out currentTeam))
                {
                    currentTeam.DefensiveRankings = new TeamStatRanking
                    {
                        Overall = currentRank++
                    };
                }
            }

            Utility.WriteData(@".\archive\reports\defstats.csv", sb.ToString());

            // set defensive rankings
            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderBy(t => (10 * t.PassYardsAllowed) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.DefensiveRankings.Passing = currentRank++;
                }
            }

            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderBy(t => (10 * t.RushingYardsAllowed) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.DefensiveRankings.Rushing = currentRank++;
                }
            }

            currentRank = 1;
            foreach (var off in TeamSeasonStats.TeamStats.Values.OrderByDescending(t => (1000 * (t.InterceptionsByDefense + t.FumblesRecovered)) / t.Team.Games))
            {
                if (Team.Teams.TryGetValue(off.Team.Id, out currentTeam))
                {
                    currentTeam.DefensiveRankings.Turnovers = currentRank++;
                }
            }
        }

        public static void MainPage()
        {
            SeasonReview();

            TextWriter tw = null;

            using (tw = new StreamWriter("./Archive/Reports/statLeaders.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "NCAA Football 2014", "leadNationalStatLeaderData", string.Format("{0},{1}", PlayerDB.CurrentYear, Utility.StartingYear));
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");

                // passing table
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"passingTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Passing Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Att</td>");
                tw.Write("<td class=C10>Comp</td><td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Ints</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // qb rush table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"qbRushingTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Rushing Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Car</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>YAC</td><td class=C10>BTK</td><td class=C10>Fumb</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // hb rush table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"hbRushingTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Rushing Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Car</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>YAC</td><td class=C10>BTK</td><td class=C10>Fumb</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // rec lead table
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"recTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13>");
                tw.Write("<b>Receptions Lead</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Rec</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>YAC</td><td class=C10>Fumb</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // rec yds table
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"recYdsTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13>");
                tw.Write("<b>Receiving Yards Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Rec</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>YAC</td><td class=C10>Fumb</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // blocking table
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"olTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Blocking Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td><td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Pancakes</td><td class=C10>Sacks Allowed</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // tackles table
                tw.Write("<tr><td width=900 align=center colspan=14><table  id=\"tklTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Tackles Leader</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Tck</td>");
                tw.Write("<td class=C10>TFL</td><td class=C10>Sck</td><td class=C10>Int</td><td class=C10>PDef</td><td class=C10>FF</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // sacks table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"sackTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Sacks Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Tck</td>");
                tw.Write("<td class=C10>TFL</td><td class=C10>Sck</td><td class=C10>Int</td><td class=C10>PDef</td><td class=C10>FF</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // int table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"intTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Interceptions Leader</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Tck</td>");
                tw.Write("<td class=C10>TFL</td><td class=C10>Sck</td><td class=C10>Int</td><td class=C10>PDef</td><td class=C10>FF</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // kick table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"kickTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Kicking Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>FGM/FGA</td>");
                tw.Write("<td class=C10>40-49<br>FGM/FGA</td><td class=C10>50+<br>FGM/FGA</td><td class=C10>Long</td><td class=C10>XPM/XPA</td><td class=C10>TB</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // punt table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"puntTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Punting Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Net Yds</td>");
                tw.Write("<td class=C10>Tot Yds</td><td class=C10>Inside<br>20</td><td class=C10>Long</td><td class=C10>Blkd</td><td class=C10>TB</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // kr table
                tw.Write("<tr><td width=900 align=center colspan=14><table id=\"krTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Kick Return Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>KR</td>");
                tw.Write("<td class=C10>KR YDS</td><td class=C10>KR_TD</td><td class=C10>PR</td><td class=C10>PR_YDS</td><td class=C10>PR_TD</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");

                // pr table
                tw.Write("<tr><td width=900 align=center colspan=14><table  id=\"prTable\"  cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Punt Return Leaders</b></td></tr>");
                tw.Write("<tr><td class=C10>Year</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>KR</td>");
                tw.Write("<td class=C10>KR YDS</td><td class=C10>KR_TD</td><td class=C10>PR</td><td class=C10>PR_YDS</td><td class=C10>PR_TD</td><td class=C10>Team</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            PlayerDB.ToLeaderboardCsv(Convert.ToInt32(ConfigurationManager.AppSettings["PlayerStatLeaders"]));

            // CODE FOR LINKS.html page
            using (tw = new StreamWriter("./Archive/Reports/Links.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Links", null, null);
                tw.Write("<body topmargin=0 leftmargin=0 marginheight=70 marginwidth=0><table><tr><td width=800 height=40></td></tr></table><table width=800 cellpadding=20 cellspacing=0><tr><td width=100% align=left colspan=20><table cellpadding=0 cellspacing=0 width=100%><tr><td class=c8 width=100%><center></center></td><td class=c8></td></tr></table></td></tr><tr><td width=400 align=left colspan=8><table cellspacing=1 cellpadding=2 width=100% class=standard><tr><td class=c2 colspan=9><b>Links</b></td></tr><tr><td class=c3><a href='attendance.html'><span>Attendance Rankings</span></a> </td><td class=c3><a href='bowls.html'><span>Bowls</span></a> </td><td class=c3><a href='bowlchampions.html'><span>Bowl Champions</span></a></td><td class=c3><a href='coaches.html'><span>Coaches</span></a></td></tr><tr><td class=c3><a href='NC.html'><span>National Championships</span></a> </td><td class=c3><a href='CC.html'><span>Conf Championships</span></a> </td><td class=c3><a href='BCS_Rankings.html'><span>Polls - BCS Rankings</span></a> </td><td class=c3><a href='Poll.html?type=coach'><span>Polls - Coaches Poll</span></a> </td></tr><tr><td class=c3><a href='Poll.html?type=media'><span>Polls - Media Poll</span></a> </td><td class=c3><a href='nationalrecordbook.html'><span>Record Book</span></a> </td><td class=c3><a href='recruits.html'><span>Recruits</span></a> </td><td class=c3><a href='RecruitingRankings.html'><span>Rec Rankings</span></a> </td></tr><tr><td class=c3><a href='RecruitingRankingsConf.html'><span>Rec Rankings by Conf</span></a> </td><td class=c3><a href='SOS_Rankings.html'><span>SOS Rankings</span></a> </td><td class=c3><a href='TeamStats.html'><span>Team Stats Offense</span></a> </td><td class=c3><a href='TeamStatsDef.html'><span>Team Stats Defense</span></a> </td></tr><tr><td class=c3><a href='topperf.html'><span>Top Performances</span></a> </td><td class=c3><a href='topprograms.html'><span>Top Programs</span></a> </td><td class=c3><a href='topprogramsbyconference.html'><span>Top Programs by Conf</span></a> </td><td class=c3><a href='award.html?id=0'><span>Heisman Trophy</span></a> </td></tr><tr><td class=c3><a href='award.html?id=1'><span>Maxwell Award</span></a> 	</td><td class=c3><a href='award.html?id=2'><span>Walter Camp</span></a></td><td class=c3><a href='award.html?id=3'><span>Bednarik</span></a> 	</td><td class=c3><a href='award.html?id=4'><span>Nagurski</span></a> 	</td></tr><tr><td class=c3><a href='award.html?id=5'><span>O'Brien</span></a> 	</td><td class=c3><a href='award.html?id=6'><span>Doak Walker</span></a> 	</td><td class=c3><a href='award.html?id=7'><span>Biletnikoff</span></a> 	</td><td class=c3><a href='award.html?id=8'><span>Mackey</span></a> 			</td></tr><tr><td class=c3>		 <a href='award.html?id=9'><span>Outland</span></a> 						</td><td class=c3><a href='award.html?id=10'><span>Rimington</span></a> 						</td><td class=c3><a href='award.html?id=11'><span>Lombardi</span></a> 						</td><td class=c3><a href='award.html?id=12'><span>Butkus</span></a> 						</td></tr><tr><td class=c3><a href='award.html?id=13'><span>Thorpe</span></a> 						</td><td class=c3><a href='award.html?id=14'><span>Groza</span></a> 					</td><td class=c3><a href='award.html?id=15'><span>Guy</span></a> 					</td><td class=c3><a href='award.html?id=16'><span>Best Returner</span></a></td></tr><tr><td class=c3><a href='conference.html'><span>Conference Rankings</span></a></td><td class=c3><a href='statLeaders.html'><span>Stats Leaders</span></a></td><td class=c3><a href='AllAmericans.html'><span>All Americans</span></a></td><td class=c3><a href='CoachingChanges.html'><span>Coaching Changes</span></a></td></tr><tr><td class=c3><a href='HFA.html'><span>Toughest Places to Play</span></a></td></tr></table></td></tr></table></body>	");
            }
        }

        public static void BoxScores()
        {
            Dictionary<int, WeeklySchedule> games = new Dictionary<int,WeeklySchedule>();
            foreach (var game in ScheduledGame.Schedule.Values)
            {
                if (game.AwayTeamId.IsValidTeam() && game.HomeTeamId.IsValidTeam())
                {
                    //game.ToJson();
                    WeeklySchedule ws = null;

                    // we add the game
                    if (!games.TryGetValue(game.Week,out ws))
                    {
                        games[game.Week] = ws = new WeeklySchedule();
                    }

                    // add it to the list and make sure we have all the relevant data
                    ws.Add(game);
                    game.ToJson();
                }
            }

            foreach (var week in games)
            {
                var fileName = @".\Archive\Reports\box" + week.Key;
                week.Value.Prepare();
                week.Value.ToJsonFile(fileName);
            }

            using (var tw = new StreamWriter("./Archive/Reports/boxscore.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadBoxScoreData", null);
                tw.Write(File.ReadAllText(@".\archive\html\boxscoretemplate"));
            }
        }

        public static void SeasonReview()
        {
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/Index.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Season Review", "loadSeasonReviewData", year.ToString());
                tw.Write(File.ReadAllText(@".\archive\html\SeasonReviewTemplate"));
            }
        }

        public static void TopUnits()
        {
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/TopUnits.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Top Units", "loadTopUnitsData", year.ToString());
                var content = File.ReadAllText(@".\archive\html\TopUnitsTemplate.txt");
                tw.Write(SetTOC(content));
            }
        }

        public static void TopClasses()
        {
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/TopClasses.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Recruiting Roundup", "loadTopClassesData");
                var content = File.ReadAllText(@".\archive\html\HistoricRecruitingTemplate.txt");
                tw.Write(SetTOC(content));
            }
        }

        public static void OpeningWeek()
        {
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/KickOffWeek.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Top Games", "loadKickoffWeek", year.ToString());
                var content = File.ReadAllText(@".\archive\html\KickOffWeekTemplate.txt");
                tw.Write(SetTOC(content));
            }
        }

        public static void TopGames()
        {
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/TopGames.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Top Games", "loadTopGames", year.ToString());
                var content = File.ReadAllText(@".\archive\html\TopGamesTemplate.txt");
                tw.Write(SetTOC(content));
            }
        }

        public static void PreseasonPage()
        {
            File.Copy(@".\archive\html\preaseasonindex.html", @".\Archive\Reports\Index.html");
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/PreseasonReview.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Season Preview", "loadPreSeasonReviewData", year.ToString());
                var content = File.ReadAllText(@".\archive\html\PreseasonTemplate.txt");
                tw.Write(SetTOC(content));
            }

            using (var tw = new StreamWriter("./Archive/Reports/ConferencePredictions.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Conference Champion Predictions", "loadCCPredictions", year.ToString());
                var content = File.ReadAllText(@".\archive\html\PreseasonConferenceChampTemplate.txt");
                tw.Write(SetTOC(content));
            }
        }

        public static string SetTOC(string content)
        {
            var tocTemplate = File.ReadAllText(Path.Combine(@".\archive\html\PreseasonTOCTemplate.txt"));
            return content.Replace("{TOC_TEMPLATE}", tocTemplate);
        }

        public static void Bowls()
        {
            var year = BowlChampion.CurrentYear + Utility.StartingYear;
            using (var tw = new StreamWriter("./Archive/Reports/bowls.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, year + " Bowl Games", "loadBowlGames", year.ToString());
                tw.Write(@"<table><tr><td class=c8 colspan=9 width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0><tr><td width=100% align=center colspan=4><table cellpadding=0 cellspacing=0 width=100%><tr><td class=c8 width=800><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td></tr></table></td></tr>");
                tw.Write(@"<tr><td width=800 align=center colspan=4><table id='bowlTable' cellspacing=1 cellpadding=2 width=750 class=standard align='right'>");
                tw.Write(@"<tr><td class=c2 colspan=8><b><center>Schedule</center></b></td></tr>");
                tw.Write(@"<tr><td class=c7 width=24%><b><center>Bowl</center></b></td>");
                tw.Write(@"<td class=c7 width=5%><b><center>Rank</center></b></td>");
                tw.Write(@"<td class=c7 width=18%><b><center>Home</center></b></td>");
                tw.Write(@"<td class=c7 width=10%><b><center></center></b></td>");
                tw.Write(@"<td class=c7 width=10%><b><center>Box Score</center></b></td>");
                tw.Write(@"<td class=c7 width=10%><b><center></center></b></td>");
                tw.Write(@"<td class=c7 width=5%><b><center>Rank</center></b></td>");
                tw.Write(@"<td class=c7 width=18%><b><center>Away</center></b></td></tr>");

                var keys = new string[] { "GameId", "BowlId", "Name", "HomeRank", "HomeTeamId", "HomeTeam", "Score", "AwayRank", "AwayTeamId", "AwayTeam" };
                var lines = new List<string>();

                foreach (var bowl in Bowl.GetBowlsInPlayoffOrder())
                {
                    // we have a playoff bowl here
                    if (bowl.ScheduleGame.AwayTeam.BCSPrevious <= Utility.PlayoffTeamCount)
                    {
                        // national championship
                        if (bowl.Id == 39)
                        {
                            //  national champion
                            bowl.ScheduleGame.WinningTeam.PlayoffStatus = PlayoffStatus.NationalChampion;
                            bowl.ScheduleGame.LosingTeam.PlayoffStatus = PlayoffStatus.LostInChampionshipGame;
                        }
                        else
                        {
                            bowl.ScheduleGame.LosingTeam.PlayoffStatus = PlayoffStatus.LostInPlayoff;
                        }
                    }

                    lines.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        bowl.Key,
                        bowl.Id,
                        bowl.Name,
                        bowl.ScheduleGame.HomeTeam.BCSPrevious,
                        bowl.ScheduleGame.HomeTeam.Id,
                        bowl.ScheduleGame.HomeTeam.Name + " (" + bowl.ScheduleGame.HomeTeam.SeasonRecord + ")",
                        bowl.ScheduleGame.HomeScore + "-" + bowl.ScheduleGame.AwayScore,
                        bowl.ScheduleGame.AwayTeam.BCSPrevious,
                        bowl.ScheduleGame.AwayTeam.Id,
                        bowl.ScheduleGame.AwayTeam.Name + " (" + bowl.ScheduleGame.AwayTeam.SeasonRecord + ")"));
                }

                tw.Write(@"</table></td></tr></table></body>");
                lines.ToCsvFile(keys, s => s, "bowlgames.csv");
            }

            using (var tw = new StreamWriter("./Archive/Reports/bowlchampions.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Bowl Championships", "loadBowlChampionData", Utility.StartingYear.ToString());
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img id='bowlChampLogo' border=0 src=../HTML/Logos/FCS.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img id='bowlChampTrophy' border=0 src=></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>    <tr>		<td width=800 align=center colspan=8>			<table id=\"bowlChampTable\" cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=9><b><a href=\"bowlchampions.html\">Bowl Champions</a></b></td>				</tr>	");
                tw.Write("</table><br></td></tr></table></body>");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,Name,BowlId,Team,TeamId");
            foreach (var bc in BowlChampion.GetBowlChampions())
            {
                var teamId = bc.TeamId;
                var teamName = TeamNameLookup(bc.TeamId);

                sb.AppendLine(string.Format("{0},{1},{2},{3},{4}", bc.Year, Bowl.FindById(bc.BowlId).Name, bc.BowlId, teamName, bc.TeamId));
            }

            Utility.WriteData(@".\archive\reports\bowlchamps.csv", sb.ToString());
        }

        public static void Attendance()
        {
            TextWriter tw;
            using (tw = new StreamWriter("./Archive/Reports/attendance.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Attendance", "loadAttendanceData");
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");
                tw.Write("<tr><td width=900 align=center colspan=8><table id=\"attTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Attendance</b></td></tr>");
                tw.Write("<tr><td class=C10 width=10%>Rank</td>");
                tw.Write("<td class=C10 width=20%>Team</td>");
                tw.Write("<td class=C10 width=30%>Stadium</td>");
                tw.Write("<td class=C10 width=10%>W-L</td>");
                tw.Write("<td class=C10 width=10%><a href='#' onclick='sortByAttendance()'>Avg Att</a></td>");
                tw.Write("<td class=C10 width=10%>Capacity</td>");
                tw.Write("<td class=C10 width=10%><a href='#' onclick='sortByCapacity()'>% Capacity</a></td></tr>");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Team,TeamId,Stadium,Record,AvgAtt,Capacity,PctCapacity");
            foreach (var team in Team.Teams.Values.Where(t => t.Conference.LeagueId == 0 && (t.DynastyWin + t.DynastyLoss) > 0).OrderByDescending(t => t.AverageAttendance))
            {
                sb.AppendLine(string.Format("{0},{1},{2},{3}-{4},{5},{6},{7}",
                    team.Name, team.Id, team.Stadium.Name, team.Win, team.Loss, team.AverageAttendance, team.StadiumCapacity, (10000 * team.AverageAttendance) / team.StadiumCapacity));
            }

            Utility.WriteData(@".\archive\reports\att.csv", sb.ToString());
        }

        static void WriteTopPerfData(PlayerStats perf, TextWriter tw)
        {
            tw.Write("<td class=c3 width=20%>" + perf.Player.Team.Name + "</td>");
            tw.Write("<td class=c3 width=6%>" + perf.Player.Number + "</td>");
            tw.Write("<td class=c3 width=20%><b>" + perf.Player.Name + "</b></td>");
            tw.Write("<td class=c3 width=8%>" + perf.Player.CalculateYear(0) + "</td>");
            tw.Write("<td class=c3 width=6%>" + perf.Player.PositionName + "</td>");
            tw.Write("<td class=c3 width=5%>" + perf.Player.Height.CalculateHgt() + "</td>");
            tw.Write("<td class=c3 width=5%>" + perf.Player.Weight + "</td>");
        }

        public static void TopPreformances()
        {
            TextWriter tw;
            using (tw = new StreamWriter("./Archive/Reports/topperf.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Top Performances", null);
                tw.Write("<table><tr><td height=70></td></tr></table>");
                tw.Write("<table><tr><td width=800><center><img border=0 src=../HTML/Logos/FCS.jpg></center></td></tr></table>");
                tw.Write("<table><tr><td width=800 align=center colspan=14><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Passing Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Comp</td>");
                tw.Write("<td class=C10>Att</td><td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Int</td><td class=C10>Game</td></tr>");


                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId)).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.PassingYards, 0)).Take(25))
                {
                    //Start With Player Info
                    WriteTopPerfData(perf, tw);
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.Completions, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.PassAttempts, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%><b>" + perf.GetValueOrDefault(PlayerStats.PassingYards, 0) + "</b></td>");
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.PassingTD, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.IntThrown, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%><a href=boxscore.html?id=" + perf.GameKey + ">Link</a></td>");
                    tw.Write("</tr>");
                }

                tw.Write("</table><br></td></tr>");


                tw.Write("<tr><td width=800 align=center colspan=14><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Rushing Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Att</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Game</td></tr>");

                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId)).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.RushingYards, 0)).Take(25))
                {
                    //Start With Player Info
                    WriteTopPerfData(perf, tw);
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.RushAttempts, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%><b>" + perf.GetValueOrDefault(PlayerStats.RushingYards, 0) + "</b></td>");
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.RushingTD, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%><a href=boxscore.html?id=" + perf.GameKey + ">Link</a></td>");
                    tw.Write("</tr>");
                }

                tw.Write("</table><br></td></tr>");

                tw.Write("<tr><td width=800 align=center colspan=14><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Receiving Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Rec</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Game</td></tr>");

                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId)).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.ReceivingYards, 0)).Take(25))
                {
                    //Start With Player Info
                    WriteTopPerfData(perf, tw);
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.Receptions, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%><b>" + perf.GetValueOrDefault(PlayerStats.ReceivingYards, 0) + "</b></td>");
                    tw.Write("<td class=c3 width=5%>" + perf.GetValueOrDefault(PlayerStats.ReceivingTD, 0) + "</td>");
                    tw.Write("<td class=c3 width=5%><a href=boxscore.html?id=" + perf.GameKey + ">Link</a></td>");
                    tw.Write("</tr>");
                }

                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }
        }

        public static void TopTeamPerformances()
        {
            TextWriter tw = null;

            using (tw = new StreamWriter("./Archive/Reports/teamtopperf.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamTopPerfData");
                // no preseason stats
                tw.Write(Utility.LoadTeamTemplate("teampagetemplate", false));
                tw.Write("<tr>        <td class=c3 colspan=3>  	  <b>Links: | <a id=\"careerStatsLink\" href=\"\">Career Stats</a> | | <a id=\"playerStatsLink2\" href=\"\">Player Stats</a> | | <a id=\"topPerfLink\" href=\"\">Top Performances</a> |        </b></td>      </tr>");
                tw.Write("<tr><td width=900 align=center colspan=8><table  id=\"passTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Top Passing Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Comp</td>");
                tw.Write("<td class=C10>Att</td><td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Int</td><td class=C10>Game</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("<tr><td width=800 align=center colspan=14><table id=\"rushTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Rushing Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Att</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Game</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("<tr><td width=800 align=center colspan=14><table id=\"recTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Receiving Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Rec</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Game</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            StringBuilder pass = new StringBuilder();
            StringBuilder rush = new StringBuilder();
            StringBuilder rec = new StringBuilder();
            rush.AppendLine("Team,TeamId,No,Name,PlayerClass,Position,Height,Weight,Att,Yards,TD,GameKey");
            rec.AppendLine("Team,TeamId,No,Name,PlayerClass,Position,Height,Weight,Rec,Yards,TD,GameKey");
            pass.AppendLine("Team,TeamId,No,Name,PlayerClass,Position,Height,Weight,Comp,Att,Yards,TD,Int,GameKey");
            foreach (var teamId in Team.Teams.Keys)
            {
                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.PassingYards, 0)).Take(15))
                {
                    var player = perf.Player;
                    pass.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        Team.Teams[teamId].Name, teamId, player.Number, player.Name, player.CalculateYear(0), player.PositionName, player.Height.CalculateHgt(), player.Weight,
                        perf.GetIntValue(PlayerStats.Completions), perf.GetIntValue(PlayerStats.PassAttempts), perf.GetIntValue(PlayerStats.PassingYards), perf.GetIntValue(PlayerStats.PassingTD),
                        perf.GetIntValue(PlayerStats.IntThrown), perf.GameKey));
                }

                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.RushingYards, 0)).Take(15))
                {
                    var player = perf.Player;
                    rush.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        Team.Teams[teamId].Name, teamId, player.Number, player.Name, player.CalculateYear(0), player.PositionName, player.Height.CalculateHgt(), player.Weight,
                        perf.GetIntValue(PlayerStats.RushAttempts), perf.GetIntValue(PlayerStats.RushingYards),
                        perf.GetIntValue(PlayerStats.RushingTD), perf.GameKey));
                }

                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.ReceivingYards, 0)).Take(15))
                {
                    var player = perf.Player;
                    rec.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                        Team.Teams[teamId].Name, teamId, player.Number, player.Name, player.CalculateYear(0), player.PositionName, player.Height.CalculateHgt(), player.Weight,
                        perf.GetIntValue(PlayerStats.Receptions), perf.GetIntValue(PlayerStats.ReceivingYards),
                        perf.GetIntValue(PlayerStats.ReceivingTD), perf.GameKey));
                }
            }

            Utility.WriteData(@".\archive\reports\ttppass.csv", pass.ToString());
            Utility.WriteData(@".\archive\reports\ttprush.csv", rush.ToString());
            Utility.WriteData(@".\archive\reports\ttprec.csv", rec.ToString());
        }

        // TCS GAME LOG
        public static void GameTeamPerformances()
        {
            TextWriter tw = null;

            using (tw = new StreamWriter("./Archive/Reports/gametopperf.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "title", "loadTeamTopPerfData");

                // no preseason stats
                tw.Write(Utility.LoadTeamTemplate("teampagetemplate", false));
                tw.Write("<tr><td width=900 align=center colspan=8><table  id=\"passTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Top Passing Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Comp</td>");
                tw.Write("<td class=C10>Att</td><td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Int</td><td class=C10>Game</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("<tr><td width=800 align=center colspan=14><table id=\"rushTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Rushing Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Att</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Game</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("<tr><td width=800 align=center colspan=14><table id=\"recTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=14>");
                tw.Write("<b>Top Receiving Performances</b></td></tr>");
                tw.Write("<tr><td class=C10>Team</td><td class=C10>#</td><td class=C10>Name</td><td class=C10>Year</td>");
                tw.Write("<td class=C10>Pos</td><td class=C10>Hgt</td><td class=C10>Lbs</td><td class=C10>Rec</td>");
                tw.Write("<td class=C10>Yards</td><td class=C10>TDs</td><td class=C10>Game</td></tr>");
                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            StringBuilder pass = new StringBuilder();
            StringBuilder rush = new StringBuilder();
            StringBuilder rec = new StringBuilder();
            rush.AppendLine("Team,TeamId,No,Name,PlayerClass,Position,Height,Weight,Att,Yards,TD,GameKey");
            rec.AppendLine("Team,TeamId,No,Name,PlayerClass,Position,Height,Weight,Rec,Yards,TD,GameKey");
            pass.AppendLine("Team,TeamId,No,Name,PlayerClass,Position,Height,Weight,Comp,Att,Yards,TD,Int,GameKey");
            foreach (var teamId in Team.Teams.Keys)
            {

                //foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.PassingYards, 0)).Take(50))
                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderBy(p => p.Player.LastName).OrderBy(p => p.Player.FirstName).Take(150))
                {
                    var player = perf.Player;
                    if (perf.GetIntValue(PlayerStats.PassAttempts) != 0)
                    {
                        pass.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                            Team.Teams[teamId].Name, teamId, player.Number, player.Name, player.CalculateYear(0), player.PositionName, player.Height.CalculateHgt(), player.Weight,
                            perf.GetIntValue(PlayerStats.Completions), perf.GetIntValue(PlayerStats.PassAttempts), perf.GetIntValue(PlayerStats.PassingYards), perf.GetIntValue(PlayerStats.PassingTD),
                            perf.GetIntValue(PlayerStats.IntThrown), perf.GameKey));
                    }
                }

                //foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.RushingYards, 0)).Take(150))
                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderBy(p => p.Player.LastName).OrderBy(p => p.Player.FirstName).Take(150))
                {
                    var player = perf.Player;
                    if (perf.GetIntValue(PlayerStats.RushAttempts) != 0)
                    {
                        rush.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                            Team.Teams[teamId].Name, teamId, player.Number, player.Name, player.CalculateYear(0), player.PositionName, player.Height.CalculateHgt(), player.Weight,
                            perf.GetIntValue(PlayerStats.RushAttempts), perf.GetIntValue(PlayerStats.RushingYards),
                            perf.GetIntValue(PlayerStats.RushingTD), perf.GameKey));
                    }
                }

                //foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderByDescending(p => p.GetValueOrDefault(PlayerStats.ReceivingYards, 0)).Take(150))
                foreach (var perf in PlayerStats.OffensiveGamePerformances.Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && p.Player.TeamId == teamId).OrderBy(p => p.Player.LastName).OrderBy(p => p.Player.FirstName).Take(150))
                {
                    var player = perf.Player;
                    if (perf.GetIntValue(PlayerStats.Receptions) != 0)
                    {
                        rec.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                            Team.Teams[teamId].Name, teamId, player.Number, player.Name, player.CalculateYear(0), player.PositionName, player.Height.CalculateHgt(), player.Weight,
                            perf.GetIntValue(PlayerStats.Receptions), perf.GetIntValue(PlayerStats.ReceivingYards),
                            perf.GetIntValue(PlayerStats.ReceivingTD), perf.GameKey));
                    }
                }
            }

            Utility.WriteData(@".\archive\reports\gtppass.csv", pass.ToString());
            Utility.WriteData(@".\archive\reports\gtprush.csv", rush.ToString());
            Utility.WriteData(@".\archive\reports\gtprec.csv", rec.ToString());
        }  // END TCS


        public static string Rec_Type(int type)
        {
            string Type = "";
            if (type == 0) { Type = "Game"; }
            if (type == 1) { Type = "Season"; }
            if (type == 2) { Type = "Career"; }
            return Type;
        }
        public static string Rec_Desc(int type)
        {
            string Type = "";
            if (type == 0) { Type = "Rushing Yards"; }
            if (type == 1) { Type = "Rushing Touchdowns"; }
            if (type == 2) { Type = "Passing Yards"; }
            if (type == 3) { Type = "Passing Touchdowns"; }
            if (type == 4) { Type = "Receptions"; }
            if (type == 5) { Type = "Receiving Yards"; }
            if (type == 6) { Type = "Receiving Touchdowns"; }
            if (type == 7) { Type = "Sacks"; }
            if (type == 8) { Type = "Interceptions"; }
            return Type;
        }
        public static string Nat_Rec_Desc(int type)
        {
            string Type = "";
            if (type == 1) { Type = "Individual Game - Longest Run"; }
            if (type == 2) { Type = "Individual Game - Longest Pass"; }
            if (type == 3) { Type = "Individual Game - Longest Punt"; }
            if (type == 4) { Type = "Individual Game - Longest Field Goal"; }
            if (type == 5) { Type = "Individual Game - Longest Punt Return"; }
            if (type == 6) { Type = "Individual Game - Longest Kick Return"; }
            if (type == 7) { Type = "Individual Game - Longest Int Return"; }
            if (type == 8) { Type = "Individual Game - Longest Fumble Ret"; }
            if (type == 9) { Type = "Individual Game - Passing Yards"; }
            if (type == 10) { Type = "Individual Game - Rushing Yards"; }
            if (type == 11) { Type = "Individual Game - Rec Yards"; }
            if (type == 12) { Type = "Individual Game - Receptions"; }
            if (type == 13) { Type = "Individual Game - Sacks"; }
            if (type == 14) { Type = "Individual Game - Interceptions"; }
            if (type == 15) { Type = "Team Game - Points"; }
            if (type == 16) { Type = "Team Game - Total Offense"; }
            if (type == 17) { Type = "Team Game - Passing Yards"; }
            if (type == 18) { Type = "Team Game - Rushing Yards"; }
            if (type == 19) { Type = "Team Game - Sacks"; }
            if (type == 20) { Type = "Team Game - Interceptions"; }
            if (type == 21) { Type = "Individual Season - Rushing Yards"; }
            if (type == 22) { Type = "Individual Season - Rushing TDs"; }
            if (type == 23) { Type = "Individual Season - Passing Yards"; }
            if (type == 24) { Type = "Individual Season - Passing TDs"; }
            if (type == 25) { Type = "Individual Season - QB Rating"; }
            if (type == 26) { Type = "Individual Season - Receptions"; }
            if (type == 27) { Type = "Individual Season - Rec Yards"; }
            if (type == 28) { Type = "Individual Season - Rec TDs"; }
            if (type == 29) { Type = "Individual Season - Sacks"; }
            if (type == 30) { Type = "Individual Season - Interceptions"; }
            if (type == 31) { Type = "Individual Season - FGs"; }
            if (type == 32) { Type = "Individual Career - Rushing Yards"; }
            if (type == 33) { Type = "Individual Career - Rushing TDs"; }
            if (type == 34) { Type = "Individual Career - Passing Yards"; }
            if (type == 35) { Type = "Individual Career - Passing TDs"; }
            if (type == 36) { Type = "Individual Career - Receptions"; }
            if (type == 37) { Type = "Individual Career - Rec Yards"; }
            if (type == 38) { Type = "Individual Career - Rec TDs"; }
            if (type == 39) { Type = "Individual Career - Sacks"; }
            if (type == 40) { Type = "Individual Career - Interceptions"; }
            if (type == 41) { Type = "Coach Career - Years Coached"; }
            if (type == 42) { Type = "Coach Career - Years at School"; }
            if (type == 43) { Type = "Coach Career - Games Coached"; }
            if (type == 44) { Type = "Coach Career - Career Wins"; }
            if (type == 45) { Type = "Coach Career - Longest Win Streak"; }
            if (type == 46) { Type = "Coach Career - National Titles"; }
            if (type == 47) { Type = "Coach Career - Bowl App."; }
            if (type == 48) { Type = "Coach Career - Bowl Wins"; }
            return Type;
        }
        public static string TeamNameLookup(int Team_ID)
        {
            String Team_Name;
            if (Team_ID == 1) { Team_Name = "Air Force"; }
            else if (Team_ID == 2) { Team_Name = "Akron"; }
            else if (Team_ID == 3) { Team_Name = "Alabama"; }
            else if (Team_ID == 4) { Team_Name = "Arizona"; }
            else if (Team_ID == 5) { Team_Name = "Arizona State"; }
            else if (Team_ID == 6) { Team_Name = "Arkansas"; }
            else if (Team_ID == 7) { Team_Name = "Arkansas State"; }
            else if (Team_ID == 8) { Team_Name = "Army"; }
            else if (Team_ID == 9) { Team_Name = "Auburn"; }
            else if (Team_ID == 10) { Team_Name = "Ball State"; }
            else if (Team_ID == 11) { Team_Name = "Baylor"; }
            else if (Team_ID == 12) { Team_Name = "Boise State"; }
            else if (Team_ID == 13) { Team_Name = "Boston College"; }
            else if (Team_ID == 14) { Team_Name = "Bowling Green"; }
            else if (Team_ID == 15) { Team_Name = "Buffalo"; }
            else if (Team_ID == 16) { Team_Name = "BYU"; }
            else if (Team_ID == 17) { Team_Name = "California"; }
            else if (Team_ID == 18) { Team_Name = "UCF"; }
            else if (Team_ID == 19) { Team_Name = "Central Michigan"; }
            else if (Team_ID == 20) { Team_Name = "Cincinnati"; }
            else if (Team_ID == 21) { Team_Name = "Clemson"; }
            else if (Team_ID == 22) { Team_Name = "Colorado"; }
            else if (Team_ID == 23) { Team_Name = "Colorado State"; }
            else if (Team_ID == 24) { Team_Name = "Duke"; }
            else if (Team_ID == 25) { Team_Name = "ECU"; }
            else if (Team_ID == 26) { Team_Name = "Eastern Michigan"; }
            else if (Team_ID == 27) { Team_Name = "Florida"; }
            else if (Team_ID == 28) { Team_Name = "Florida State"; }
            else if (Team_ID == 29) { Team_Name = "Fresno State"; }
            else if (Team_ID == 30) { Team_Name = "Georgia"; }
            else if (Team_ID == 31) { Team_Name = "Georgia Tech"; }
            else if (Team_ID == 32) { Team_Name = "Hawai'i"; }
            else if (Team_ID == 33) { Team_Name = "Houston"; }
            else if (Team_ID == 34) { Team_Name = "Idaho"; }
            else if (Team_ID == 35) { Team_Name = "Illinois"; }
            else if (Team_ID == 36) { Team_Name = "Indiana"; }
            else if (Team_ID == 37) { Team_Name = "Iowa"; }
            else if (Team_ID == 38) { Team_Name = "Iowa State"; }
            else if (Team_ID == 39) { Team_Name = "Kansas"; }
            else if (Team_ID == 40) { Team_Name = "Kansas State"; }
            else if (Team_ID == 41) { Team_Name = "Kent State"; }
            else if (Team_ID == 42) { Team_Name = "Kentucky"; }
            else if (Team_ID == 43) { Team_Name = "Louisiana Tech"; }
            else if (Team_ID == 44) { Team_Name = "Louisville"; }
            else if (Team_ID == 45) { Team_Name = "LSU"; }
            else if (Team_ID == 46) { Team_Name = "Marshall"; }
            else if (Team_ID == 47) { Team_Name = "Maryland"; }
            else if (Team_ID == 48) { Team_Name = "Memphis"; }
            else if (Team_ID == 49) { Team_Name = "Miami"; }
            else if (Team_ID == 50) { Team_Name = "Miami University"; }
            else if (Team_ID == 51) { Team_Name = "Michigan"; }
            else if (Team_ID == 52) { Team_Name = "Michigan State"; }
            else if (Team_ID == 53) { Team_Name = "Mid Tenn State"; }
            else if (Team_ID == 54) { Team_Name = "Minnesota"; }
            else if (Team_ID == 55) { Team_Name = "Mississippi State"; }
            else if (Team_ID == 56) { Team_Name = "Missouri"; }
            else if (Team_ID == 57) { Team_Name = "Navy"; }
            else if (Team_ID == 58) { Team_Name = "Nebraska"; }
            else if (Team_ID == 59) { Team_Name = "Nevada"; }
            else if (Team_ID == 60) { Team_Name = "New Mexico"; }
            else if (Team_ID == 61) { Team_Name = "New Mexico State"; }
            else if (Team_ID == 62) { Team_Name = "North Carolina"; }
            else if (Team_ID == 63) { Team_Name = "NC State"; }
            else if (Team_ID == 64) { Team_Name = "North Texas"; }
            else if (Team_ID == 65) { Team_Name = "UL Monroe"; }
            else if (Team_ID == 66) { Team_Name = "Northern Illinois"; }
            else if (Team_ID == 67) { Team_Name = "Northwestern"; }
            else if (Team_ID == 68) { Team_Name = "Notre Dame"; }
            else if (Team_ID == 69) { Team_Name = "Ohio"; }
            else if (Team_ID == 70) { Team_Name = "Ohio State"; }
            else if (Team_ID == 71) { Team_Name = "Oklahoma"; }
            else if (Team_ID == 72) { Team_Name = "Oklahoma State"; }
            else if (Team_ID == 73) { Team_Name = "Ole Miss"; }
            else if (Team_ID == 74) { Team_Name = "Oregon"; }
            else if (Team_ID == 75) { Team_Name = "Oregon State"; }
            else if (Team_ID == 76) { Team_Name = "Penn State"; }
            else if (Team_ID == 77) { Team_Name = "Pittsburgh"; }
            else if (Team_ID == 78) { Team_Name = "Purdue"; }
            else if (Team_ID == 79) { Team_Name = "Rice"; }
            else if (Team_ID == 80) { Team_Name = "Rutgers"; }
            else if (Team_ID == 81) { Team_Name = "San Diego State"; }
            else if (Team_ID == 82) { Team_Name = "San Jose State"; }
            else if (Team_ID == 83) { Team_Name = "SMU"; }
            else if (Team_ID == 84) { Team_Name = "South Carolina"; }
            else if (Team_ID == 85) { Team_Name = "Southern Miss"; }
            else if (Team_ID == 86) { Team_Name = "UL Lafayette"; }
            else if (Team_ID == 87) { Team_Name = "Stanford"; }
            else if (Team_ID == 88) { Team_Name = "Syracuse"; }
            else if (Team_ID == 89) { Team_Name = "TCU"; }
            else if (Team_ID == 90) { Team_Name = "Temple"; }
            else if (Team_ID == 91) { Team_Name = "Tennessee"; }
            else if (Team_ID == 92) { Team_Name = "Texas"; }
            else if (Team_ID == 93) { Team_Name = "Texas A&M"; }
            else if (Team_ID == 94) { Team_Name = "Texas Tech"; }
            else if (Team_ID == 95) { Team_Name = "Toledo"; }
            else if (Team_ID == 96) { Team_Name = "Tulane"; }
            else if (Team_ID == 97) { Team_Name = "Tulsa"; }
            else if (Team_ID == 98) { Team_Name = "UAB"; }
            else if (Team_ID == 99) { Team_Name = "UCLA"; }
            else if (Team_ID == 100) { Team_Name = "Connecticut"; }
            else if (Team_ID == 101) { Team_Name = "UNLV"; }
            else if (Team_ID == 102) { Team_Name = "USC"; }
            else if (Team_ID == 103) { Team_Name = "Utah"; }
            else if (Team_ID == 104) { Team_Name = "Utah State"; }
            else if (Team_ID == 105) { Team_Name = "UTEP"; }
            else if (Team_ID == 106) { Team_Name = "Vanderbilt"; }
            else if (Team_ID == 107) { Team_Name = "Virginia"; }
            else if (Team_ID == 108) { Team_Name = "Virginia Tech"; }
            else if (Team_ID == 109) { Team_Name = "Wake Forest"; }
            else if (Team_ID == 110) { Team_Name = "Washington"; }
            else if (Team_ID == 111) { Team_Name = "Washington State"; }
            else if (Team_ID == 112) { Team_Name = "West Virginia"; }
            else if (Team_ID == 113) { Team_Name = "Western Michigan"; }
            else if (Team_ID == 114) { Team_Name = "Wisconsin"; }
            else if (Team_ID == 115) { Team_Name = "Wyoming"; }
            else if (Team_ID == 143) { Team_Name = "Troy"; }
            else if (Team_ID == 144) { Team_Name = "USF"; }
            else if (Team_ID == 160) { Team_Name = "FCS East"; }
            else if (Team_ID == 161) { Team_Name = "FCS West"; }
            else if (Team_ID == 162) { Team_Name = "FCS Northwest"; }
            else if (Team_ID == 163) { Team_Name = "FCS Midwest"; }
            else if (Team_ID == 164) { Team_Name = "FCS Southeast"; }
            else if (Team_ID == 181) { Team_Name = "UMass"; }
            else if (Team_ID == 211) { Team_Name = "Western Kentucky"; }
            else if (Team_ID == 218) { Team_Name = "Texas State"; }
            else if (Team_ID == 229) { Team_Name = "Florida Atlantic"; }
            else if (Team_ID == 230) { Team_Name = "FIU"; }
            else if (Team_ID == 232) { Team_Name = "UTSA"; }
            else if (Team_ID == 233) { Team_Name = "Georgia State"; }
            else if (Team_ID == 234) { Team_Name = "Old Dominion"; }
            else if (Team_ID == 235) { Team_Name = "South Alabama"; }
            else if (Team_ID == 901) { Team_Name = "Appalachian State"; }
            else if (Team_ID == 902) { Team_Name = "Georgia Southern"; }
            else if (Team_ID == 903) { Team_Name = "Coastal Carolina"; }
            else if (Team_ID == 904) { Team_Name = "Charlotte"; }
            else { Team_Name = "Unemployeed"; }
            return Team_Name;
        }
    }
}