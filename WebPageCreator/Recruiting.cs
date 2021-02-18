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
    public class Recruiting : PocketScout
    {
        static int R_AL = 0, R_AK = 0, R_AZ = 0, R_AR = 0, R_CA = 0, R_CO = 0, R_CT = 0, R_DE = 0, R_FL = 0, R_GA = 0, R_HI = 0;
        static int R_ID = 0, R_IL = 0, R_IN = 0, R_IA = 0, R_KS = 0, R_KY = 0, R_LA = 0, R_ME = 0, R_MD = 0, R_MA = 0, R_MI = 0;
        static int R_MN = 0, R_MS = 0, R_MO = 0, R_MT = 0, R_NE = 0, R_NV = 0, R_NH = 0, R_NJ = 0, R_NM = 0, R_NY = 0, R_NC = 0;
        static int R_ND = 0, R_OH = 0, R_OK = 0, R_OR = 0, R_PA = 0, R_RI = 0, R_SC = 0, R_SD = 0, R_TN = 0, R_TX = 0, R_UT = 0;
        static int R_VT = 0, R_VA = 0, R_WA = 0, R_WV = 0, R_WI = 0, R_WY = 0, R_CN = 0, R_DC = 0;
        public static void CreateRecruitsPage(MaddenDatabase db)
        {
            using (var tw = new StreamWriter("./Archive/Reports/hsaaroster.html", false))
            {
                    Utility.WriteNavBarAndHeader(tw, "All American Game Rosters", "loadHSAAData");
                    tw.WriteLine("<table><tr><td width=800 height=40></td></tr></table>");
                    tw.WriteLine("<table width=800 cellpadding=20 cellspacing=0><tr><td width=100% align=left colspan=20>");
                    tw.WriteLine("<table cellpadding=0 cellspacing=0 width=100%>");
                    tw.WriteLine("<tr><td class=c8 width=100%><center><img border=0 src=../HTML/Logos/ESPNU.jpg></center></td><td class=c8></td></tr>	 <tr><td class=c3 width=800 align=center colspan=8><b>| <a href='recruits.html'>Recruits</a> | | <a href='hsaaroster.html'>All American Game Rosters</a> | | <a href='RecruitingRankings.html'>National Team Recruiting Rankings</a> || <a href='RecruitingRankingsConf.html'>Recruiting Rankings by Conference</a> |</b></td></tr>");
                    tw.WriteLine("</table>");
                    tw.WriteLine("</td></tr>");

                    CreateRosterTable(tw, "All American East Roster", "hsaaeast");
                    CreateRosterTable(tw, "All American West Roster", "hsaawest");
            }

            using (var tw = new StreamWriter("./Archive/Reports/recruits.html", false))
            {

                try
                {
                    Utility.WriteNavBarAndHeader(tw, "Recruits", "loadRecruitData");

                    tw.WriteLine("<table><tr><td width=800 height=40></td></tr></table>");
                    tw.WriteLine("<table width=800 cellpadding=20 cellspacing=0><tr><td width=100% align=left colspan=20>");
                    tw.WriteLine("<table cellpadding=0 cellspacing=0 width=100%>");
                    tw.WriteLine("<tr><td class=c8 width=100%><center><img id='currentSchoolLogo' border=0 src=../HTML/Logos/ESPNU.jpg></center></td><td class=c8></td></tr>	 <tr><td class=c3 width=800 align=center colspan=8><b>| <a href='recruits.html'>Recruits</a> | | <a href='hsaaroster.html'>All American Game Rosters</a> | | <a href='recruits.html?juco=true'>JUCO Recruits</a> | | <a href='RecruitingRankings.html'>National Team Recruiting Rankings</a> || <a href='RecruitingRankingsConf.html'>Recruiting Rankings by Conference</a> |</b></td></tr>");
                    tw.WriteLine("</table>");
                    tw.WriteLine("</td></tr>");


                    int count = Convert.ToInt32(ConfigurationManager.AppSettings["RecruitingGemsBusts"]);
                    int i = 0;
                    Recruit.ToCsvFile(Recruit.RecruitRankings.Values.OrderByDescending(r => r.OvrDiff).Distinct().Take(count), "gems.csv");
                    Recruit.ToCsvFile(Recruit.RecruitRankings.Values.OrderBy(r => r.OvrDiff).Distinct().Take(count), "busts.csv");
                    Recruit.ToCsvFile(Recruit.RecruitRankings.Values.OrderBy(r => r.Rank).Distinct(), "recruits.csv",true);
                    CreateRecruitTable(tw, "Top Gems", "gemsTable");
                    CreateRecruitTable(tw, "Top Busts", "bustsTable");
                    CreateRecruitTable(tw, "Recruits", "recruitsTable");

                    for (int t = 0; t < Recruit.RecruitRankings.Keys.Count / 2; t++)
                    {
                        var gem = Recruit.RecruitRankings[10001 + t];

                        if (gem.HometownValue > 0 && gem.HometownValue < 256) { R_AL++; }
                        else if (gem.HometownValue > 255 && gem.HometownValue < 512) { R_AK++; }
                        else if (gem.HometownValue > 511 && gem.HometownValue < 768) { R_AZ++; }
                        else if (gem.HometownValue > 767 && gem.HometownValue < 1024) { R_AR++; }
                        else if (gem.HometownValue > 1023 && gem.HometownValue < 1280) { R_CA++; }
                        else if (gem.HometownValue > 1279 && gem.HometownValue < 1536) { R_CO++; }
                        else if (gem.HometownValue > 1535 && gem.HometownValue < 1792) { R_CT++; }
                        else if (gem.HometownValue > 1791 && gem.HometownValue < 2048) { R_DE++; }
                        else if (gem.HometownValue > 2047 && gem.HometownValue < 2304) { R_FL++; }
                        else if (gem.HometownValue > 2303 && gem.HometownValue < 2560) { R_GA++; }
                        else if (gem.HometownValue > 2559 && gem.HometownValue < 2816) { R_HI++; }
                        else if (gem.HometownValue > 2815 && gem.HometownValue < 3072) { R_ID++; }
                        else if (gem.HometownValue > 3071 && gem.HometownValue < 3328) { R_IL++; }
                        else if (gem.HometownValue > 3327 && gem.HometownValue < 3584) { R_IN++; }
                        else if (gem.HometownValue > 3583 && gem.HometownValue < 3840) { R_IA++; }
                        else if (gem.HometownValue > 3839 && gem.HometownValue < 4096) { R_KS++; }
                        else if (gem.HometownValue > 4095 && gem.HometownValue < 4352) { R_KY++; }
                        else if (gem.HometownValue > 4351 && gem.HometownValue < 4608) { R_LA++; }
                        else if (gem.HometownValue > 4607 && gem.HometownValue < 4864) { R_ME++; }
                        else if (gem.HometownValue > 4863 && gem.HometownValue < 5120) { R_MD++; }
                        else if (gem.HometownValue > 5119 && gem.HometownValue < 5376) { R_MA++; }
                        else if (gem.HometownValue > 5375 && gem.HometownValue < 5632) { R_MI++; }
                        else if (gem.HometownValue > 5631 && gem.HometownValue < 5888) { R_MN++; }
                        else if (gem.HometownValue > 5887 && gem.HometownValue < 6144) { R_MS++; }
                        else if (gem.HometownValue > 6143 && gem.HometownValue < 6400) { R_MO++; }
                        else if (gem.HometownValue > 6399 && gem.HometownValue < 6656) { R_MT++; }
                        else if (gem.HometownValue > 6655 && gem.HometownValue < 6912) { R_NE++; }
                        else if (gem.HometownValue > 6911 && gem.HometownValue < 7168) { R_NV++; }
                        else if (gem.HometownValue > 7167 && gem.HometownValue < 7424) { R_NH++; }
                        else if (gem.HometownValue > 7423 && gem.HometownValue < 7680) { R_NJ++; }
                        else if (gem.HometownValue > 7679 && gem.HometownValue < 7936) { R_NM++; }
                        else if (gem.HometownValue > 7935 && gem.HometownValue < 8192) { R_NY++; }
                        else if (gem.HometownValue > 8191 && gem.HometownValue < 8448) { R_NC++; }
                        else if (gem.HometownValue > 8447 && gem.HometownValue < 8704) { R_ND++; }
                        else if (gem.HometownValue > 8703 && gem.HometownValue < 8960) { R_OH++; }
                        else if (gem.HometownValue > 8959 && gem.HometownValue < 9216) { R_OK++; }
                        else if (gem.HometownValue > 9215 && gem.HometownValue < 9472) { R_OR++; }
                        else if (gem.HometownValue > 9471 && gem.HometownValue < 9728) { R_PA++; }
                        else if (gem.HometownValue > 9727 && gem.HometownValue < 9984) { R_RI++; }
                        else if (gem.HometownValue > 9983 && gem.HometownValue < 10240) { R_SC++; }
                        else if (gem.HometownValue > 10239 && gem.HometownValue < 10496) { R_SD++; }
                        else if (gem.HometownValue > 10495 && gem.HometownValue < 10752) { R_TN++; }
                        else if (gem.HometownValue > 10751 && gem.HometownValue < 11008) { R_TX++; }
                        else if (gem.HometownValue > 11007 && gem.HometownValue < 11264) { R_UT++; }
                        else if (gem.HometownValue > 11263 && gem.HometownValue < 11520) { R_VT++; }
                        else if (gem.HometownValue > 11519 && gem.HometownValue < 11776) { R_VA++; }
                        else if (gem.HometownValue > 11775 && gem.HometownValue < 12032) { R_WA++; }
                        else if (gem.HometownValue > 12031 && gem.HometownValue < 12288) { R_WV++; }
                        else if (gem.HometownValue > 12287 && gem.HometownValue < 12544) { R_WI++; }
                        else if (gem.HometownValue > 12543 && gem.HometownValue < 12800) { R_WY++; }
                        else if (gem.HometownValue > 12799 && gem.HometownValue < 13056) { R_CN++; }
                        else if (gem.HometownValue == 13056) { R_DC++; }
                    }


                    //AREA TO PRINT OUT ROSTER ANALYZER 
                    tw.Write("<tr><td width=900 align=center colspan=8><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Players by State</b></td></tr>");
                    tw.Write("<tr><td class=C10 width=10%>STATE</td><td class=C10 width=10%>PLAYERS</td><td class=C10 width=10%>STATE</td><td class=C10 width=10%>PLAYERS</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=AL'>AL</a> </td><td class=c3>" + R_AL + "</td><td class=c3><a href='recruits.html?state=NE'>NE </a></td><td class=c3>" + R_NE + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=AK'>AK</a></a> </td><td class=c3>" + R_AK + "</td><td class=c3><a href='recruits.html?state=NV'>NV</a> </td><td class=c3>" + R_NV + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=AZ'>AZ</a> </td><td class=c3>" + R_AZ + "</td><td class=c3><a href='recruits.html?state=NH'>NH</a> </td><td class=c3>" + R_NH + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=AR'>AR</a> </td><td class=c3>" + R_AR + "</td><td class=c3><a href='recruits.html?state=NJ'>NJ</a> </td><td class=c3>" + R_NJ + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=CA'>CA</a> </td><td class=c3>" + R_CA + "</td><td class=c3><a href='recruits.html?state=NM'>NM</a> </td><td class=c3>" + R_NM + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=CO'>CO</a> </td><td class=c3>" + R_CO + "</td><td class=c3><a href='recruits.html?state=NY'>NY</a> </td><td class=c3>" + R_NY + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=CT'>CT</a> </td><td class=c3>" + R_CT + "</td><td class=c3><a href='recruits.html?state=NC'>NC</a> </td><td class=c3>" + R_NC + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=DE'>DE</a> </td><td class=c3>" + R_DE + "</td><td class=c3><a href='recruits.html?state=ND'>ND</a> </td><td class=c3>" + R_ND + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=FL'>FL</a> </td><td class=c3>" + R_FL + "</td><td class=c3><a href='recruits.html?state=OH'>OH</a> </td><td class=c3>" + R_OH + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=GA'>GA</a> </td><td class=c3>" + R_GA + "</td><td class=c3><a href='recruits.html?state=OK'>OK</a> </td><td class=c3>" + R_OK + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=HI'>HI</a> </td><td class=c3>" + R_HI + "</td><td class=c3><a href='recruits.html?state=OR'>OR</a> </td><td class=c3>" + R_OR + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=ID'>ID</a> </td><td class=c3>" + R_ID + "</td><td class=c3><a href='recruits.html?state=PA'>PA</a> </td><td class=c3>" + R_PA + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=IL'>IL</a> </td><td class=c3>" + R_IL + "</td><td class=c3><a href='recruits.html?state=RI'>RI</a> </td><td class=c3>" + R_RI + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=IN'>IN</a> </td><td class=c3>" + R_IN + "</td><td class=c3><a href='recruits.html?state=SC'>SC</a> </td><td class=c3>" + R_SC + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=IA'>IA</a> </td><td class=c3>" + R_IA + "</td><td class=c3><a href='recruits.html?state=SD'>SD</a> </td><td class=c3>" + R_SD + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=KS'>KS</a> </td><td class=c3>" + R_KS + "</td><td class=c3><a href='recruits.html?state=TN'>TN</a> </td><td class=c3>" + R_TN + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=KY'>KY</a> </td><td class=c3>" + R_KY + "</td><td class=c3><a href='recruits.html?state=TX'>TX</a> </td><td class=c3>" + R_TX + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=LA'>LA</a> </td><td class=c3>" + R_LA + "</td><td class=c3><a href='recruits.html?state=UT'>UT</a> </td><td class=c3>" + R_UT + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=ME'>ME</a> </td><td class=c3>" + R_ME + "</td><td class=c3><a href='recruits.html?state=VT'>VT</a> </td><td class=c3>" + R_VT + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MD'>MD</a> </td><td class=c3>" + R_MD + "</td><td class=c3><a href='recruits.html?state=VA'>VA</a> </td><td class=c3>" + R_VA + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MA'>MA</a> </td><td class=c3>" + R_MA + "</td><td class=c3><a href='recruits.html?state=WA'>WA</a> </td><td class=c3>" + R_WA + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MI'>MI</a> </td><td class=c3>" + R_MI + "</td><td class=c3><a href='recruits.html?state=WV'>WV</a> </td><td class=c3>" + R_WV + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MN'>MN</a> </td><td class=c3>" + R_MN + "</td><td class=c3><a href='recruits.html?state=WI'>WI</a> </td><td class=c3>" + R_WI + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MS'>MS</a> </td><td class=c3>" + R_MS + "</td><td class=c3><a href='recruits.html?state=WY'>WY</a> </td><td class=c3>" + R_WY + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MO'>MO</a> </td><td class=c3>" + R_MO + "</td><td class=c3><a href='recruits.html?state=CN'>CN</a> </td><td class=c3>" + R_CN + "</td></tr>");
                    tw.Write("<tr><td class=c3><a href='recruits.html?state=MT'>MT</a> </td><td class=c3>" + R_MT + "</td><td class=c3><a href='recruits.html?state=DC'>DC</a> </td><td class=c3>" + R_DC + "</td></tr>");
                    tw.Write("</table><br></td></tr>");
                    tw.WriteLine("</td></tr></table>"); // ORG END
                }
                catch { }
                finally
                {
                    tw.Write(@"</body>");
                }
            }

            R_AL = 0; R_AK = 0; R_AZ = 0; R_AR = 0; R_CA = 0; R_CO = 0; R_CT = 0; R_DE = 0; R_FL = 0; R_GA = 0; R_HI = 0;
            R_ID = 0; R_IL = 0; R_IN = 0; R_IA = 0; R_KS = 0; R_KY = 0; R_LA = 0; R_ME = 0; R_MD = 0; R_MA = 0; R_MI = 0;
            R_MN = 0; R_MS = 0; R_MO = 0; R_MT = 0; R_NE = 0; R_NV = 0; R_NH = 0; R_NJ = 0; R_NM = 0; R_NY = 0; R_NC = 0;
            R_ND = 0; R_OH = 0; R_OK = 0; R_OR = 0; R_PA = 0; R_RI = 0; R_SC = 0; R_SD = 0; R_TN = 0; R_TX = 0; R_UT = 0;
            R_VT = 0; R_VA = 0; R_WA = 0; R_WV = 0; R_WI = 0; R_WY = 0; R_CN = 0; R_DC = 0;
        }


        public static void CreateRecruitingPages(MaddenDatabase db)
        {
            Recruit.CreateRecruits(db);
            Conference.Create(db);
            RecruitClassRanking.Create(db);
            CreateRecruitsPage(db);

            #region RECRUITING_RANKINGS_HTML
            using (var tw = new StreamWriter("./Archive/Reports/RecruitingRankings.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Recruit Rankings", "loadRecruitRanks");
                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/ESPNU.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>		 <tr><td class=c3 width=800 align=center colspan=8><b>| <a href='recruits.html'>Recruits</a> | | <a href='hsaaroster.html'>All American Game Rosters</a> | | <a href='RecruitingRankings.html'>National Team Recruiting Rankings</a> || <a href='RecruitingRankingsConf.html'>Recruiting Rankings by Conference</a> |</b></td></tr> <tr>		<td width=800 align=center colspan=8>			<table id='recruitRankTable' cellspacing=1 cellpadding=2 width=80% class=standard>				<tr>					<td class=c2 colspan=9><b>Recruiting Rankings</b></td>				</tr>				<tr>					<td class=c7 width=8%>Rank</td>					<td class=c7 width=34%>Team</td>					<td class=c7 width=10%>W-L</td>					<td class=c7 width=8%>Points</td>					<td class=c7 width=8%>Star5</td>					<td class=c7 width=8%>Star4</td>					<td class=c7 width=8%>Star3</td>					<td class=c7 width=8%>Star2</td>					<td class=c7 width=8%>Star1</td>				</tr>");

                int i = 1;
                var rankings = RecruitClassRanking.TeamRankings.Values.OrderByDescending(tr => tr.Points).ThenByDescending(tr => tr.Star5).ThenByDescending(tr => tr.Star4).ThenByDescending(tr => tr.Star3).ThenByDescending(tr => tr.Star2).ToArray();
                rankings.ToJsonFile(@".\Archive\Reports\RecruitRankings");
#if false
                foreach(var ranking in rankings)
                {
                    tw.Write("<tr><td class=c3>" + i + "</td>");
                    tw.Write("<td class=c3><a href=team.html?id=" + ranking.TeamId + ">" + ranking.Team + "</a></td>");
                    tw.Write("<td class=c3>" + ranking.Wins + "-" + ranking.Losses + "</td>");
                    tw.Write("<td class=c3>" + ranking.Points + "</td>");
                    tw.Write("<td class=c3>" + ranking.Star5 + "</td>");
                    tw.Write("<td class=c3>" + ranking.Star4 + "</td>");
                    tw.Write("<td class=c3>" + ranking.Star3 + "</td>");
                    tw.Write("<td class=c3>" + ranking.Star2 + "</td>");
                    tw.Write("<td class=c3>" + ranking.Star1 + "</td>");
                    i++;
                }
#endif
                tw.Write("</table><br></td></tr></table></body>");
            }
            #endregion

            #region RecruitingByConference
            using (var tw = new StreamWriter("./Archive/Reports/RecruitingRankingsConf.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Conference Recruit Rankings", null);

                tw.Write("<table><tr><td width=800 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/ESPNU.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>	 <tr><td class=c3 width=800 align=center colspan=8><b>| <a href='recruits.html'>Recruits</a> | | <a href='hsaaroster.html'>All American Game Rosters</a> | | <a href='RecruitingRankings.html'>National Team Recruiting Rankings</a> || <a href='RecruitingRankingsConf.html'>Recruiting Rankings by Conference</a> |</b></td></tr>");

               var sb = new StringBuilder();
                foreach (var cc in Conference.Conferences.Values.Where(c => c.Id != 17 && Team.Teams.Values.Any(t => t.ConferenceId == c.Id)).OrderBy(c => c.Name))
                {
                    sb.Append(string.Format("| <a href=RecruitingRankingsConf.html#{0}>{1}</a> | ", cc.Name.Replace(" ", string.Empty), cc.Name));
                }

                tw.Write(string.Format("<tr><td class=c3 colspan=3><b>Links:  {0}</b></td></tr>", sb.ToString()));

                int[] conferenceIds = new[] {0,3,2,1,4,5,7,9,10,11,13 };

                foreach (var confId in conferenceIds)
                {
                    var rankings = RecruitClassRanking.TeamRankings.Values.Where(t => t.ConferenceId == confId).OrderByDescending(tr => tr.Points).ThenByDescending(tr => tr.Star5).ThenByDescending(tr => tr.Star4).ThenByDescending(tr => tr.Star3).ThenByDescending(tr => tr.Star2);

                    tw.Write("<tr>		<td width=800 align=center colspan=12><table cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=12><b>");
                    tw.Write(string.Format("<a name={0}><center>{1} Standings</center></a></b></td></tr><tr>", Conference.Conferences[confId].Name.Replace(" ", string.Empty), Conference.Conferences[confId].Name));
                    tw.Write("<td class=c7 width=5%>Rnk</td>");
                    tw.Write("<td class=c7 width=10%></td>");
                    tw.Write("<td class=c7 width=16%>Team</td>");
                    tw.Write("<td class=c7 width=10%>W-L</td>");
                    tw.Write("<td class=c7 width=10%>Points</td>");
                    tw.Write("<td class=c7 width=8%>5 Star</td>");
                    tw.Write("<td class=c7 width=8%>4 Star</td>");
                    tw.Write("<td class=c7 width=8%>3 Star</td>");
                    tw.Write("<td class=c7 width=8%>2 Star</td>");
                    tw.Write("<td class=c7 width=8%>1 Star</td>");
                    tw.Write("<td class=c7 width=8%>Total</td>");
                    tw.Write("<td class=c7 width=8%>Spots</td>");
                    tw.Write("</tr>");

                    int i = 1;

                    foreach (var ranking in rankings)
                    {
                        var rosterOpenings = 70 - db.GetTable("PLAY").lRecords.Where(player => player["TGID"].ToInt32() == ranking.TeamId).Count();

                        tw.Write("<tr>");
                        tw.Write(@"<td class=c3>" + i + "</td>");
                        tw.Write(@"<td class=c3><img src=../HTML/Logos/35/team" + ranking.TeamId + ".png></td>");
                        tw.Write("<td class=c3><a href=teamroster.html?id=" + ranking.TeamId + ">" + ranking.Team + "</a></td>");
                        tw.Write(@"<td class=c3>" + ranking.Wins + "-" + ranking.Losses + "</td>");
                        tw.Write(@"<td class=c3><b>" + ranking.Points + "</b></td>");
                        tw.Write(@"<td class=c3>" + ranking.Star5 + "</td>");
                        tw.Write(@"<td class=c3>" + ranking.Star4 + "</td>");
                        tw.Write(@"<td class=c3>" + ranking.Star3 + "</td>");
                        tw.Write(@"<td class=c3>" + ranking.Star2 + "</td>");
                        tw.Write(@"<td class=c3>" + ranking.Star1 + "</td>");
                        tw.Write(@"<td class=c3>" + (ranking.Star1 + ranking.Star2 + ranking.Star3 + ranking.Star4 + ranking.Star5) + "</td>");
                        tw.Write(@"<td class=c3>" + rosterOpenings + "</td>");
                        tw.Write(@"</tr>");
                        i++;
                    }
                    tw.Write("</table><br></td></tr>");
                }


                tw.Write("</table></body>");
            }
            #endregion
        }

        public static void CreateRecruitTable(TextWriter tw, string header, string tableId)
        {
            tw.WriteLine(string.Format("<tr><td width=400 align=left colspan=8><table id=\"{0}\" cellspacing=1 cellpadding=2 width=100% class=standard><tr><td class=c2 colspan=20><b>{1}</b></td></tr>", tableId, header));
            tw.WriteLine(@"<tr><td class=c7 width=3%><b><center>Rnk</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>Pos<br>Rnk</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=16%><b><center>Recruit</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>Pos</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>ATH</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>Star</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>Pre<br>Scout</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>Act<br>Ovr</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=3%><b><center>Dif</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=13%><b><center>Team #1</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=13%><b><center>Team #2</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=13%><b><center>Team #3</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=18%><b><center>Hometown</center></b></td>");
            tw.WriteLine(@"</tr>");
            tw.WriteLine("</table></td>");
            tw.WriteLine("</tr>");
        }

        public static void CreateRosterTable(TextWriter tw, string header, string tableId)
        {
            tw.WriteLine(string.Format("<tr><td width=300 align=left colspan=8><table id=\"{0}\" cellspacing=1 cellpadding=2 width=100% class=standard><tr><td class=c2 colspan=20><b>{1}</b></td></tr>", tableId, header));
            tw.WriteLine(@"<tr><td class=c7 width=10%><b><center>Rank</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=10%><b><center>Position</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=25%><b><center>Recruit</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=20%><b><center>Top Team</center></b></td>");
            tw.WriteLine(@"<td class=c7 width=35%><b><center>Hometown</center></b></td>");
            tw.WriteLine(@"</tr>");
            tw.WriteLine("</table></td>");
            tw.WriteLine("</tr>");
        }
    }

    public class Recruit
    {
        public static Dictionary<int, Recruit> RecruitRankings { get; set; }
        public static void CreateRecruits(MaddenDatabase NCAADB)
        {
            if (RecruitRankings != null)
                return;

            RecruitRankings = new Dictionary<int, Recruit>();
            RecruitAllAmericans.GetRecruits(NCAADB.lTables[96], NCAADB.lTables[95]);

            for (int i = 0; i < NCAADB.lTables[96].Table.currecords; i++)
            {
                var record = NCAADB.lTables[96].lRecords[i]; 
                Recruit recruit = new Recruit
                {
                    RecruitId = NCAADB.lTables[96].lRecords[i].lEntries[53].Data.ToInt32(),
                    FirstName = NCAADB.lTables[96].lRecords[i].lEntries[14].Data,
                    LastName = NCAADB.lTables[96].lRecords[i].lEntries[15].Data,
                    PositionValue = NCAADB.lTables[96].lRecords[i].lEntries[106].Data.ToInt32(),
                    Rank = NCAADB.lTables[96].lRecords[i].lEntries[62].Data.ToInt32(),
                    PositionRank = NCAADB.lTables[96].lRecords[i].lEntries[89].Data.ToInt32(),
                    StarRating = NCAADB.lTables[96].lRecords[i].lEntries[23].Data.ToInt32(),
                    PreScoutOVR = NCAADB.lTables[96].lRecords[i].lEntries[131].Data.ToInt32(),
                    RealOVR = NCAADB.lTables[96].lRecords[i].lEntries[95].Data.ToInt32(),
                    IsAthlete = NCAADB.lTables[96].lRecords[i].lEntries[47].Data.ToInt32() != 0,
                    HometownValue = NCAADB.lTables[96].lRecords[i].lEntries[33].Data.ToInt32(),
                    PositionGroup = NCAADB.lTables[96].lRecords[i]["RPGP"].ToInt32(),
                    PlayerYear = NCAADB.lTables[96].lRecords[i]["PYEA"].ToInt32(),
                };

                RecruitRankings.Add(recruit.RecruitId, recruit);
            }

            for (int i = 0; i < NCAADB.lTables[95].Table.currecords; i++)
            {
                var id = NCAADB.lTables[95].lRecords[i].lEntries[34].Data.ToInt32();
                var recruit = RecruitRankings[id];
                recruit.CommittedTeam = NCAADB.lTables[95].lRecords[i].lEntries[35].Data.ToInt32();
                recruit.Team1 = NCAADB.lTables[95].lRecords[i].lEntries[6].Data.ToInt32();
                recruit.Team2 = NCAADB.lTables[95].lRecords[i].lEntries[10].Data.ToInt32();
                recruit.Team3 = NCAADB.lTables[95].lRecords[i].lEntries[13].Data.ToInt32();
                RecruitRankings[10000 + recruit.Rank] = recruit;
            }
        }

        public static void ToCsvFile(IEnumerable<Recruit> recruits, string filename, bool showLogo = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("CommittedTeam,Rnk,PosRnk,Recruit,Pos,IsAth,Star,PreScout,ActOvr,Dif,Team1,Team2,Team3,Hometown,TopTeamID,Group,PlayerYear");
            foreach (var gem in recruits)
            {
                sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                    gem.CommittedTeam == 1023 ? 0 : gem.Team1,
                    gem.Rank, gem.PositionRank, gem.FullName, gem.Position, gem.IsAthlete ? "Yes" : " ", gem.StarRating, gem.PreScoutOVR, gem.RealOVR, gem.OvrDiff,
                    gem.Team1.ToTeamName(),
                    gem.Team2.ToTeamName(),
                    gem.Team3.ToTeamName(),
                    gem.Location.Replace(",", "%x2C"),
                    gem.Team1,
                    gem.PositionGroup,
                    gem.PlayerYear
                    ));
            }

            Utility.WriteData(@".\archive\reports\" + filename, sb.ToString());
        }

        public int PlayerYear { get; set; }
        public int PositionGroup { get; set; }
        public int RecruitId { get; set; }
        public bool IsAthlete { get; set; }
        public int Rank { get; set; }
        public int PositionRank { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }
        public int PositionValue { get; set; }
        public string Position { get { return this.PositionValue.ToPositionName(); } }
        public int StarRating { get; set; }
        public int PreScoutOVR { get; set; }
        public int RealOVR { get; set; }
        public int OvrDiff { get { return this.RealOVR - this.PreScoutOVR; } }
        public int Team1 { get; set; }
        public int Team2 { get; set; }
        public int Team3 { get; set; }
        public string Hometown { get; set; }
        public string Homestate { get; set; }
        public string Location
        {
            get
            {
                return City.Cities.ContainsKey(this.HometownValue) ? string.Format("{0}, {1}", City.Cities[this.HometownValue].Name, City.Cities[this.HometownValue].State) : "N/A";
            }
        }
    
        public int CommittedTeam { get; set; }
        public int HometownValue { get; set; }
    }

    [DataContract]
    public class RecruitClassRanking
    {
        private static Dictionary<int, RecruitClassRanking> teamRankings = new Dictionary<int, RecruitClassRanking>();
        public static Dictionary<int, RecruitClassRanking> TeamRankings { get { return teamRankings; } }

        public static void Create(MaddenDatabase db)
        {
            TeamRankings.Clear();
            
            for (int i = 0; i < db.lTables[97].Table.currecords; i++)
            {
                var ranking = new RecruitClassRanking
                {
                    TeamId = db.lTables[97].lRecords[i].lEntries[4].Data.ToInt32(),
                    Points = db.lTables[97].lRecords[i].lEntries[5].Data.ToInt32(),
                    Star1 = db.lTables[97].lRecords[i].lEntries[6].Data.ToInt32(),
                    Star2 = db.lTables[97].lRecords[i].lEntries[7].Data.ToInt32(),
                    Star3 = db.lTables[97].lRecords[i].lEntries[8].Data.ToInt32(),
                    Star4 = db.lTables[97].lRecords[i].lEntries[9].Data.ToInt32(),
                    Star5 = db.lTables[97].lRecords[i].lEntries[10].Data.ToInt32(),
                };

                teamRankings.Add(ranking.TeamId, ranking);
            }

            for (int i = 0; i < db.lTables[167].Table.currecords; i++)
            {
                int teamId = db.lTables[167].lRecords[i].lEntries[40].Data.ToInt32();
                RecruitClassRanking ranking = null;
                if (TeamRankings.TryGetValue(teamId, out ranking))
                {
                    ranking.ConferenceId = db.lTables[167].lRecords[i].lEntries[36].Data.ToInt32();
                    ranking.DivisionId = db.lTables[167].lRecords[i].lEntries[37].Data.ToInt32();
                    ranking.Wins = db.lTables[167].lRecords[i].lEntries[61].Data.ToInt32();
                    ranking.Losses = db.lTables[167].lRecords[i].lEntries[88].Data.ToInt32();
                }
            }
        }

        [DataMember]
        public string Team { get { return TeamId.ToTeamName(); } set { } }
        [DataMember]
        public int TeamId { get; set; }
        [DataMember]
        public int Points { get; set; }
        [DataMember]
        public int Star1 { get; set; }
        [DataMember]
        public int Star2 { get; set; }
        [DataMember]
        public int Star3 { get; set; }
        [DataMember]
        public int Star4 { get; set; }
        [DataMember]
        public int Star5 { get; set; }
        [DataMember]
        public int ConferenceId { get; set; }
        [DataMember]
        public int DivisionId { get; set; }
        [DataMember]
        public int Wins { get; set; }
        [DataMember]
        public int Losses { get; set; }
    }
}