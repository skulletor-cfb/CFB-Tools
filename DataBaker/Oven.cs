using DataBaker.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace DataBaker
{
    public class PlayoffDescriptor
    {
        public string Error { get; set; }
        public int Apperances { get; set; }
        public int Last { get; set; }
    }

    public interface ILogger
    {
        void WriteLine(object s);
    }

    public class ConsoleLogger : ILogger
    {
        public void WriteLine(object s)
        {
            Console.WriteLine(s);
        }
    }

    public static class Oven
    {
        private const string HtmlDir = "HTML/";
        private static Season[] seasons;

        public static void Bake(ILogger logger)
        {
            if (!Directory.Exists(Helper.BakedPath))
            {
                Directory.CreateDirectory(Helper.BakedPath);
            }

            var sw = Stopwatch.StartNew();
            var seasonsToRun = Helper.Seasons.Season.Where(s => !s.Loaded).ToArray();
            Task<bool>[][] loads = new Task<bool>[seasonsToRun.Length][];
            var latestSeason = seasonsToRun.Last();


            Parallel.ForEach(seasonsToRun,
                s =>
                {
                    s.Loaded = s.ReadTeamScheduleFile() &&
                    s.ReadFromFile("aaac.csv", Season.AllAmericanKey) &&
                    s.ReadFromFile("awards.csv", Season.AwardsKey) &&
                    s.ReadFromFile("team", Season.TeamKey);
                    logger.WriteLine(s.Year);
                });

            sw.Stop();

            logger.WriteLine("ReadPlayoffHistory");
            Helper.ReadFromPlayoffHistory();

            var status = "Started in " + sw.Elapsed;
            logger.WriteLine(status);
            seasons = Helper.Seasons.Season.Where(s => s.Loaded).ToArray();

            // bake playoff apperances
            PlayoffAppearances().Bake("playoffs");

            // bake awards
            TeamAwards().Bake("awards");

            // bake team history
            TeamHistory().Bake("teamhistory");
        }

        /// <summary>
        /// will write to a file prefix.key.txt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="prefix"></param>
        private static void Bake<T>(this Dictionary<int, T> dict, string prefix)
        {
            foreach (var kvp in dict)
            {
                var fileName = $"{prefix}.{kvp.Key}.txt";
                var file = Path.Combine(Helper.BakedPath, fileName);
                File.WriteAllText(file, JsonConvert.SerializeObject(kvp.Value));
            }
        }

        private static Dictionary<int, TableSet> TeamHistory()
        {
            var result = new Dictionary<int, TableSet>();

            foreach (var teamId in Team.TeamIds)
            {
                var set = new TableSet();
                set.CoachHistory = new TableDescriptor();
                set.TeamHistory = new TableDescriptor();

                foreach (var s in seasons)
                {
                    s.ReadFromFile("team", Season.TeamKey);

                    Team team = null;

                    if (s.Teams.TryGetValue(teamId, out team))
                    {
                        set.CoachHistory.Rows.Insert(0, CreateCoachHistoryRow(s, team));
                        set.TeamHistory.Rows.Insert(0, CreateTeamHistoryRow(s, team));
                    }
                }

                result[teamId] = set;
            }
            return result;
        }


        private static Dictionary<int, TableSet> TeamAwards()
        {
            var result = new Dictionary<int, TableSet>();
            foreach (var teamId in Team.TeamIds)
            {
                var set = new TableSet
                {
                    Awards = new TableDescriptor(),
                    AllAmericans = new TableDescriptor(),
                    BowlWins = new TableDescriptor(),
                    ConferenceChampionships = new TableDescriptor()
                };


                foreach (var s in seasons)
                {
                    CreateAwardTable(s, teamId, set.Awards);
                    CreateAATable(s, teamId, set.AllAmericans);
                }

                set.AllAmericans.Rows = set.AllAmericans.Rows.Select(r => r as AllAmericanTableRow).OrderBy(r => r.TeamNum).ThenByDescending(r => r.Year).ThenBy(r => r.Pos).Select(r => r as TableRow).ToList();

                foreach (var s in seasons.OrderByDescending(s => s.Year).Take(1))
                {
                    CreateBowlTable(s, teamId, set.BowlWins);
                    CreateConfChampTable(s, teamId, set.ConferenceChampionships);
                }

                result[teamId] = set;
            }

            return result;
        }

        private static Dictionary<int, PlayoffDescriptor> PlayoffAppearances()
        {
            var result = new Dictionary<int, PlayoffDescriptor>();

            foreach (var teamId in Team.TeamIds)
            {
                var dict = new PlayoffDescriptor();

                try
                {
                    var appearances = new List<int>();
                    foreach (var s in seasons)
                    {
                        if (!s.Schedule.ContainsKey(teamId))
                            continue;

                        s.ReadTeamScheduleFile();
                        var games = s.Schedule[teamId].Where(g => g.IsPlayoffBowl()).ToArray();
                        appearances.AddRange(games.Select(pg => pg.Year).Distinct());
                    }

                    dict.Apperances = appearances.Count;
                    dict.Last = appearances.Max();
                }
                catch (Exception ex)
                {
                    dict.Error = ex.ToString();
                }

                result[teamId] = dict;
            }

            return result;
        }

        #region Html
        static TableRow CreateTeamHistoryRow(Season s, Team team)
        {
            return new TableRow(
                CreateYearHref(s),
                createCoachLink(team.CoachingStaff[0], s, false),
                CreateTeamHrefForRecentMeetings(s, team.Id, string.Empty, false, team.Win, team.Loss, true),
                team.RecruitClassRank.ToString(),
                team.TeamRatingOVR.MakeBold(),
                team.TeamRatingOFF.MakeBold(),
                team.TeamRatingQB.ToString(),
                team.TeamRatingRB.ToString(),
                team.TeamRatingWR.ToString(),
                team.TeamRatingOL.ToString(),
                team.TeamRatingDEF.MakeBold(),
                team.TeamRatingDL.ToString(),
                team.TeamRatingLB.ToString(),
                team.TeamRatingDB.ToString(),
                team.TeamRatingST.MakeBold(),
                team.OffensiveRankings?.Overall.ToString(),
                team.OffensiveRankings?.Passing.ToString(),
                team.OffensiveRankings?.Rushing.ToString(),
                team.DefensiveRankings?.Overall.ToString(),
                team.DefensiveRankings?.Passing.ToString(),
                team.DefensiveRankings?.Rushing.ToString()
                );
        }

        static TableRow CreateCoachHistoryRow(Season s, Team team)
        {
            return new TableRow(
                CreateYearHref(s),
                createCoachLink(team.CoachingStaff[0], s, false),
                createCoachLink(team.CoachingStaff[1], s, false),
                createCoachLink(team.CoachingStaff[2], s, false),
                CreateTeamHrefForRecentMeetings(s, team.Id, string.Empty, false, team.Win, team.Loss, true),
                team.CoachesPollRank.ToDisplayRank(),
                team.MediaPollRank.ToDisplayRank(),
                string.Join(", ", CreateSummary(team)));
        }
        static List<string> CreateSummary(Team team)
        {
            List<string> summary = new List<string>();

            if (team.IsNationalChampion)
                summary.Add("<b>National Champions</b>");

            if (!string.IsNullOrWhiteSpace(team.BowlWinsThisYear))
                summary.Add("<b>" + team.BowlWinsThisYear + "</b>");

            if (!string.IsNullOrWhiteSpace(team.ConferenceOrDivisionChampionship))
                summary.Add("<b>" + team.ConferenceOrDivisionChampionship + "</b>");

            return summary;
        }

        private static void CreateConfChampTable(Season s, int teamId, TableDescriptor td)
        {
            s.ReadFromFile("cc.csv", Season.ConfChampKey);

            if (s.ConferenceChampions.ContainsKey(teamId))
            {
                foreach (var c in s.ConferenceChampions[teamId])
                {
                    td.Rows.Add(new TableRow(c.Year.ToString(), createTrophyCaseConfTrophyLink(c.ConfId), createTrophyCaseConferenceLogo(c.ConfId), CreateTeamHrefForRecentMeetings(s, teamId, 65)));
                }
            }
        }

        private static void CreateBowlTable(Season s, int teamId, TableDescriptor td)
        {
            s.ReadFromFile("bowlchamps.csv", Season.BowlChampKey);

            if (s.BowlChampions.ContainsKey(teamId))
            {
                foreach (var a in s.BowlChampions[teamId])
                {
                    td.Rows.Add(new TableRow(a.Year.ToString(), createTrophyCaseBowlTrophyLink(a.BowlId), createTrophyCaseBowlLogoLink(a.BowlId), CreateTeamHrefForRecentMeetings(s, teamId, 65)));
                }
            }
        }

        private static void CreateAATable(Season s, int teamId, TableDescriptor td)
        {
            s.ReadFromFile("aaac.csv", Season.AllAmericanKey);
            if (s.AllAmericans.ContainsKey(teamId))
            {
                foreach (var a in s.AllAmericans[teamId])
                {
                    td.Rows.Add(new AllAmericanTableRow(a, a.Year.ToString(), a.AATeam, a.Name, a.Height, a.Weight, a.Class, a.Position, a.OVR.ToString()));
                }
            }
        }

        private static void CreateAwardTable(Season s, int teamId, TableDescriptor td)
        {
            s.ReadFromFile("awards.csv", Season.AwardsKey);

            if (s.Awards.ContainsKey(teamId))
            {
                foreach (var a in s.Awards[teamId])
                {
                    td.Rows.Insert(0, new TableRow(a.Year.ToString(), createTrophyCaseAwardLogoLink(a.AwardId), a.AwardName, a.Class, a.Position, a.Name));
                }
            }
        }

        static string CreateYearHref(Season s)
        {
            return "<a href=" + s.SeasonPath + "/index.html>" + s.Year + "</a>";
        }

        static string CreateTeamBowlHistoryHref(Season s, PlayedGame g, int teamId)
        {
            return "<a href='PostSeasonGames.html?yr=" + g.Year + "&id=" + teamId + "&bowlId=" + g.BowlId + "'/>Link</a>";
        }

        static string CreateBowlHistoryHref(Season s, PlayedGame g)
        {
            return "<a href='BowlHistory.html?yr=" + g.Year + "&id=" + g.BowlId + "'/>" + g.Location + "</a>";
        }

        static string CreateBowlHistoryHref(int year, int id, string location)
        {
            return "<a href='BowlHistory.html?yr=" + year + "&id=" + id + "'/>" + location + "</a>";
        }


        static string CreateBoxScoreHref(Season s, PlayedGame g)
        {
            return "<a href=" + s.SeasonPath + "/boxscore.html?id=" + g.Week + "-" + g.Game + ">" + g.Score + "</a>";
        }

        static string CreateTeamHrefForRecentMeetings(Season s, int teamId, string Name, bool makeBold = false, int win = 0, int loss = 0, bool setRecord = false)
        {
            var n = Name;

            if (setRecord)
            {
                n = n + " (" + win + "-" + loss + ")";
            }

            var href = string.Format("<a href={0}/team.html?id={1}>{2}</a>", s.SeasonPath, teamId, n);

            if (makeBold)
            {
                href = "<b><font size=2>" + href + "</font></b>";
            }

            return href;
        }


        static string CreateTeamHrefForRecentMeetings(Season s, int teamId, int logoSize)
        {
            if (s == null)
            {
                return "<img border = '0' src= '" + CreateTeamLogoSrc(teamId, logoSize) + "'/>";
            }

            return string.Format("<a href={0}/team.html?id={1}>{2}</a>", s.SeasonPath, teamId, "<img border = '0' src= '" + CreateTeamLogoSrc(teamId, logoSize) + "'/>");
        }

        static string CreateTeamLogoSrc(int teamId, int size)
        {
            return HtmlDir + "Logos/" + size + "/team" + teamId + ".png";
        }

        static string createBowlLogoLink(int bowlId)
        {
            return "<a href='bowlchampions.html?id=" + bowlId + "'><img src='../HTML/Logos/bowls/65/" + bowlId + ".jpg' /></a>'";
        }

        static string createTrophyCaseBowlLogoLink(int awardId)
        {
            return "<img src='./HTML/Logos/bowls/65/" + awardId + ".jpg' /></a>";
        }

        static string createAwardLogoLink(int awardId)
        {
            return "<a href='award.html?id=" + awardId + "'><img src='../HTML/Logos/awards/65/" + awardId + ".png' /></a>";
        }

        static string createTrophyCaseAwardLogoLink(int awardId)
        {
            return "<a href='./HTML/Logos/awards/" + awardId + ".png'> <img src='./HTML/Logos/awards/65/" + awardId + ".png' /></a>";
        }

        static string createBowlTrophyLink(int bowlId)
        {
            return "<a href='bowlchampions.html?id=" + bowlId + "'><img src='../HTML/Logos/bowl_trophies/65/" + bowlId + ".png' /></a>";
        }

        static string CreateTeamHistoryLink(Season s, int id)
        {
            return "<a href='./TeamHistory.html?yr=" + s.Year + "&id=" + id + "'><img border='0' src='" + CreateTeamLogoSrc(id, 35) + "' /></a>";
        }

        static string createCoachLink(Coach c, Season s, bool appendJob = true, string location = ".")
        {
            if (c == null)
                return string.Empty;

            var title = appendJob ? c.Job + ": " + c.Name : c.Name;
            return "<a href='" + location + "/CoachCareer.html?yr=" + s.Year + "&name=" + Uri.EscapeUriString(c.Name) + "&id=" + c.Id + "'>" + title + "</a>"; ;
        }

        static string createTrophyCaseBowlTrophyLink(int awardId)
        {
            return "<a href='./HTML/Logos/bowl_trophies/" + awardId + ".png'><img src='./HTML/Logos/bowl_trophies/65/" + awardId + ".png' /></a>";
        }

        static string createTrophyCaseConfTrophyLink(int awardId)
        {
            return "<a href='./HTML/Logos/conference_trophies/" + awardId + ".png'> <img src='./HTML/Logos/conference_trophies/65/" + awardId + ".png' /></a>";
        }

        static string createConfTrophyLink(int confId)
        {
            return "<a href='CC.html?id=" + confId + "'><img src='../HTML/Logos/conference_trophies/65/" + confId + ".png' /></a>";
        }

        static string createTrophyCaseConferenceLogo(int id)
        {
            return "<img src='./HTML/Logos/conferences/65/" + id + ".jpg' />";
        }
        static string CreateH2HLink(Season s, int oppId, string opp)
        {
            return "<a href='HeadToHead.html?id=" + oppId + "&yr=" + s.Year + "'>" + opp + "</a>";
        }

        static string CreateRecentMeetingsLink(Season s, int teamId, int oppId)
        {
            return "<a href='recentmeetings.html?id=" + teamId + "&opp=" + oppId + "&yr=" + s.Year + "'>Recent Meetings</a>";
        }
        #endregion
    }
}