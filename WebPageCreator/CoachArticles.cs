using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    [DataContract]
    public class PreseasonCoachPage
    {
        [DataMember]
        public CoachArticle[] TopCoordinators { get; set; }

        [DataMember]
        public CoachArticle[] TopMidMajorHC { get; set; }

        [DataMember]
        public CoachArticle[] HotSeatCoaches { get; set; }

        [DataMember]
        public CoachArticle[] NewCoaches { get; set; }

        [DataMember]
        public CoachArticle[] LongTermCoaches { get; set; }
    }

    public class CoachArticle
    {
        public CoachArticle()
        {
        }

        public CoachArticle(Coach coach, Dictionary<int, Coach[]> lastYearStaff, bool getOldCoachData = false)
            : this(coach)
        {
            var lastYearEntry = lastYearStaff.Values.SelectMany(c => c).Where(c => c.Id == this.Coach.Id && c.Name == this.Coach.Name).SingleOrDefault();
            this.OldJob = lastYearEntry == null ?
                "FCS" :
                string.Format("{0}, {1}", lastYearEntry.Position.ToJob(), lastYearEntry.Team.Name);

            if (getOldCoachData)
            {
                this.OldCoach = lastYearStaff.ContainsKey(this.TeamId) ? lastYearStaff[this.TeamId].Where(c => c.Position == 0).SingleOrDefault() : null;
            }
        }

        public CoachArticle(Coach coach, bool calcPoints = false)
        {
            this.Coach = coach;
            this.TeamId = this.Coach.TeamId;
            this.TeamName = this.Coach.Team.Name;

            if (calcPoints)
            {
                // 1 point per HC win
                // 1 point per bowl win
                // 5 points per conf win
                // 10 points per nc win
                // +/- (Win-Loss)/2

                this.Points += coach.CareerWin;
                this.Points += coach.CoachBowlWin;
                this.Points += coach.CareerConferenceChampionships * 5;
                this.Points += coach.CareerNationalChampionships * 10;
                this.Points += ((coach.CareerWin - coach.CareerLoss) / 2);
            }
        }

        [DataMember]
        public string OldJob { get; set; }

        [DataMember]
        public Coach Coach { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Coach OldCoach { get; set; }

        [DataMember]
        public int TeamId { get; set; }

        [DataMember]
        public string TeamName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Points { get; set; }
    }

    [DataContract]
    public class RankedCoaches
    {
        [DataMember]
        public CoachArticle[] HeadCoaches { get; set; }
    }

    public static class CoachArticles
    {
        /// <summary>
        /// Top 5 Coordinators, Top 5 G5 HCs, Top 10 Hot Seat, New Coaches
        /// </summary>
        public static void CreateCoachingChangePage(MaddenDatabase db)
        {
            var lastYearCoaches = Coach.FromCsv(Seasons.LastYearDirectory, "coaches.csv");
            var table = MaddenTable.FindTable(db.lTables, "CPRF");
            var hotSeat = table.lRecords.Where(mr => mr["JSCR"].ToInt32() > 100).GroupBy(mr => mr["CCID"].ToInt32(), mr => mr["JSCR"].ToInt32()).ToDictionary(g => g.Key, g => g.OrderByDescending(r => r).First());

            // hot OC/DC/MidMajor coaches should be kinda young so they can be ready to take over for years
            var coachPage = new PreseasonCoachPage
            {
                LongTermCoaches = Coach.Coaches.Values.Where(c => c.YearsWithTeam > 1 && c.Position == 0).OrderByDescending(c => c.YearsWithTeam).Take(10).Select(c => new CoachArticle(c, lastYearCoaches)).ToArray(),
                HotSeatCoaches = Coach.Coaches.Values.Where(c => c.YearsWithTeam > 1 && hotSeat.ContainsKey(c.Id) && c.Position == 0).OrderByDescending(c => hotSeat[c.Id]).Take(10).Select(c => new CoachArticle(c, lastYearCoaches)).ToArray(),
                NewCoaches = Coach.Coaches.Values.Where(c => c.Position == 0 && (c.TeamWin + c.TeamLoss) == 0).OrderByDescending(c => c.Rating).Select(c => new CoachArticle(c, lastYearCoaches, true)).ToArray(),
                TopCoordinators = Coach.Coaches.Values.Where(c => c.Position != 0 && c.Age < 60).OrderByDescending(c => c.Rating).Take(5).Select(c => new CoachArticle(c, lastYearCoaches)).ToArray(),
                TopMidMajorHC = Coach.Coaches.Values.Where(c => c.YearsWithTeam > 1 && c.Age < 60 && c.Position == 0 && !Conference.PowerIndependents.Contains(c.Team.Id) && Conference.PowerConferences.Contains(c.Team.ConferenceId) == false).OrderByDescending(c => c.CareerWin == 0 ? 0 : (c.CareerWin * 100) / (c.CareerWin + c.CareerLoss)).Take(5).Select(c => new CoachArticle(c, lastYearCoaches)).ToArray(),
            };

            coachPage.ToJsonFile(@".\archive\reports\ps-coachpage");

            using (var tw = new StreamWriter("./Archive/Reports/CoachingCarousel.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Coaching Carousel", "loadCoachCarouselData", (BowlChampion.CurrentYear + Utility.StartingYear).ToString());
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");

                var tocTemplate = File.ReadAllText(Path.Combine(@".\archive\html\PreseasonTOCTemplate.txt"));

                tw.Write(tocTemplate);

                tw.Write("<tr><td width=900 align=center colspan=10><table id=\"longTimeCoachTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Tenured</b></td></tr>");
                tw.Write("</table><br></td></tr>");

                tw.Write("<tr><td width=900 align=center colspan=10><table id=\"newCoachTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Coaching Changes</b></td></tr>");
                tw.Write("</table><br></td></tr>");

                tw.Write("<tr><td width=900 align=center colspan=10><table id=\"topMidMajorHCTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Top 5 Mid Major Head Coaches</b></td></tr>");
                tw.Write("</table><br></td></tr>");

                tw.Write("<tr><td width=900 align=center colspan=10><table id=\"topOCDCTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Top 5 Coordinators</b></td></tr>");
                tw.Write("</table><br></td></tr>");

                tw.Write("<tr><td width=900 align=center colspan=10><table id=\"topHotSeatTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>On the Hot Seat</b></td></tr>");
                tw.Write("</table><br></td></tr>");

                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

            var rankedCoaches = new RankedCoaches
            {
                HeadCoaches = Coach.Coaches.Values.Where(c => c.Position == 0).Select(c => new CoachArticle(c, true)).OrderByDescending(c => c.Points).ToArray()
            };

            rankedCoaches.ToJsonFile(@".\archive\reports\ps-topcoaches");

            using (var tw = new StreamWriter("./Archive/Reports/TopCoaches.html", false))
            {
                Utility.WriteNavBarAndHeader(tw, "Ranking the Coaches", "loadTopCoachData", (BowlChampion.CurrentYear + Utility.StartingYear).ToString());
                tw.Write("<table><tr><td width=900 height=40></td></tr></table><table cellpadding=20 cellspacing=0>	<tr>		<td width=100% align=center colspan=4>			<table cellpadding=0 cellspacing=0 width=100%>				<tr>					<td class=c8 width=100%><center><img border=0 src=../HTML/Logos/NCAA2014.jpg></center></td><td class=c8></td>				</tr>			</table>		</td>	</tr>");

                var tocTemplate = File.ReadAllText(Path.Combine(@".\archive\html\PreseasonTOCTemplate.txt"));

                tw.Write(tocTemplate);

                tw.Write("<tr><td width=900 align=center colspan=10><table id=\"topCoachesTable\" cellspacing=1 cellpadding=2 width=80% class=standard><tr><td class=c2 colspan=13><b>Ranked Coaches</b></td></tr>");
                tw.Write("</table><br></td></tr>");

                tw.Write("</table><br></td></tr>");
                tw.Write("</table></body>");
            }

        }
    }
}