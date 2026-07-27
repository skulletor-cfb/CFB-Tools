using DataBaker.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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
        private static ILogger logger;

        public static void Bake(ILogger loggerInstance)
        {
            logger = loggerInstance;
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
                    s.ReadAllAmericanFile() &&
                    s.ReadAwardsFile() &&
                    s.ReadTeamFile();
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

            // bake team greats history
            TeamGreats().Bake("teamgreats");

            // get the view of all teams played
            OverallTeamH2H().Bake("teamh2h");
            OverallTeamH2H(true).Bake("teamh2h.sorted");
            TeamH2HDrilldown().Bake("teamh2h.filter");
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

        #region H2H views
        public static Dictionary<int, Dictionary<int, TableDescriptor>> TeamH2HDrilldown()
        {
            var result = new Dictionary<int, Dictionary<int, TableDescriptor>>();
            foreach (var teamId in Team.TeamIds)
            {
                var inner = new ConcurrentDictionary<int, TableDescriptor>();
                Parallel.ForEach(Team.TeamIds, opp =>
                {
                    if (teamId == opp) return;
                    inner[opp] = GetTeamH2H(teamId, filter: opp);
                });

                result[teamId] = inner.ToDictionary();
            }

            return result;
        }

        public static Dictionary<int, TableDescriptor> OverallTeamH2H(bool sort=false)
        {
            var result = new Dictionary<int, TableDescriptor>();
            foreach (var teamId in Team.TeamIds)
            {
                result[teamId] = GetTeamH2H(teamId, sortByRecent:sort);
            }

            return result;
        }

        private static TableDescriptor GetTeamH2H(int teamId, int filter=0, bool sortByRecent=false)
        {
            var td = new TableDescriptor();
            var oppDict = new Dictionary<int, List<IPlayedGame>>();
            Season latest = null;
            HashSet<int> opponentsPlayed = new HashSet<int>();

            foreach (var s in seasons)
            {
                s.ReadTeamScheduleFile();
                s.ReadTeamFile();

                if (!s.Schedule.ContainsKey(teamId))
                    continue;

                // add all opponents in that season
                foreach (var opp in s.Schedule[teamId])
                {
                    // exclude fcs teams
                    if (IsFcsTeam(opp.OppId))
                    {
                        continue;
                    }

                    // we specified a team, so only care about that one team
                    if (filter != 0 && opp.OppId != filter)
                        continue;

                    List<IPlayedGame> games = null;

                    if (!oppDict.TryGetValue(opp.OppId, out games))
                    {
                        games = new List<IPlayedGame>();
                        oppDict.Add(opp.OppId, games);
                    }

                    games.Add(opp);
                }

                latest = s;
            }

            foreach (var bs in PastPlayoffHistory.GetBowlsForTeam(teamId))
            {
                var opponent = bs.GetOpponentId(teamId);

                // we specified a team, so only care about that one team
                if (filter != 0 && opponent != filter)
                    continue;

                List<IPlayedGame> games = null;

                if (!oppDict.TryGetValue(opponent, out games))
                {
                    games = new List<IPlayedGame>();
                    oppDict.Add(opponent, games);
                }

                games.Add(bs);
            }


            if (filter == 0)
            {
                var teamDict = seasons.First().Teams;

                // add teams we haven't played
                foreach (var team in teamDict.Values.Where(t => t.Id != teamId && oppDict.ContainsKey(t.Id) == false))
                {
                    oppDict[team.Id] = new List<IPlayedGame>();
                }

                // we have all games played, now we need to generate a table for them
                var list = oppDict.Select(o =>
                   new
                   {
                       Id = o.Key,
                       Win = o.Value.Where(g => g.IsWinningTeam(teamId)).Count(),
                       Loss = o.Value.Where(g => !g.IsWinningTeam(teamId)).Count(),
                       LastMeeting = o.Value.Count == 0 ? -1 : o.Value.OrderBy(g => g.Year).Last().Year,
                       Name = GetTeamName(o.Key)
                   });

                if (sortByRecent)
                {
                    list = list.OrderByDescending(e => e.LastMeeting)
                    .ThenBy(e => e.Name);
                }
                else
                {
                    list = list.OrderByDescending(e => e.Win + e.Loss)
                    .ThenBy(e => e.Name);
                }

                foreach (var r in list)
                {
                    string lastMeeting = string.Empty;
                    Season season;

                    if (RuntimeCache.SeasonsDict.TryGetValue(r.LastMeeting, out season))
                    {
                        lastMeeting = CreateYearHref(season);
                    }
                    else if (r.LastMeeting != -1)
                    {
                        lastMeeting = r.LastMeeting.ToString();
                    }


                    td.Rows.Add(new TableRow(
                        CreateTeamHrefForRecentMeetings(latest, teamId, 35),
                        r.Win + "-" + r.Loss,
                        CreateTeamHrefForRecentMeetings(latest, r.Id, 35),
                        CreateH2HLink(latest, r.Id, r.Name),
                        lastMeeting,
                        CreateRecentMeetingsLink(latest, teamId, r.Id)
                        ));
                }
            }
            else
            {
                var teamBigWin = 0;
                var oppBigWin = 0;
                IPlayedGame teamWin = null;
                IPlayedGame oppWin = null;

                if (!oppDict.TryGetValue(filter, out var gameResults))
                {
                    gameResults= new List<IPlayedGame>();
                }

                foreach (var game in gameResults)
                {
                    var diff = ScoreDiff(game.Score);
                    if (game.IsWinningTeam(teamId) && diff > teamBigWin)
                    {
                        teamWin = game;
                        teamBigWin = diff;
                    }
                    else if (diff < oppBigWin)
                    {
                        oppWin = game;
                        oppBigWin = diff;
                    }
                }

                var games = gameResults.OrderByDescending(g => g.Year).ThenByDescending(g => g.Week)
                    .Select(g => CreateTableRow(teamId, RuntimeCache.SeasonsDict.GetDictionaryValue(g.Year), g, alwaysMakeBold: false));

                var filterName = seasons.Last().Teams.TryGetValue(filter, out var opponentTeam) ? opponentTeam.Name : Team.PendingTeamNames[filter];
                var teamName = seasons.Last().Teams.TryGetValue(teamId, out var teamInstance) ? teamInstance.Name : Team.PendingTeamNames[teamId];
                td.Description = CreateSeriesHeader(gameResults.Count(g => g.IsWinningTeam(teamId)), gameResults.Count(g => g.IsWinningTeam(teamId) == false), teamName, filterName);

                if (teamWin != null)
                    td.Rows.Add(CreateTableRow(teamId, RuntimeCache.SeasonsDict.GetDictionaryValue(teamWin.Year), teamWin, alwaysMakeBold: true));
                else
                    td.Rows.Add(null);

                if (oppWin != null)
                    td.Rows.Add(CreateTableRow(filter, RuntimeCache.SeasonsDict.GetDictionaryValue(oppWin.Year), oppWin, alwaysMakeBold: false));
                else
                    td.Rows.Add(null);

                td.Rows.AddRange(games);
            }

            return td;
        }

        private static string CreateSeriesHeader(int win, int loss, string by, string against)
        {
            if (win == loss)
            {
                return string.Format("Series is tied {0}-{1}", win, loss);
            }
            else if (win > loss)
            {
                return string.Format("{0} leads the series {1}-{2}", by, win, loss);
            }
            else
            {
                return string.Format("{0} leads the series {1}-{2}", against, loss, win);
            }
        }


        private static string GetTeamName(int id)
        {
            return GetNameForTeamFromSeason(seasons, id);
        }

        private static bool IsFcsTeam(int id)
        {
            return id >= 160 && id <= 164;
        }

        private static string GetNameForTeamFromSeason(Season[] seasons, int teamId)
        {
            if (seasons.First().Teams.ContainsKey(teamId))
                return seasons.First().Teams[teamId].Name;

            if (seasons.Last().Teams.ContainsKey(teamId))
                return seasons.Last().Teams[teamId].Name;

            // return FCSTEAMS[teamId];
            return "FCS";
        }

        private static int ScoreDiff(string score)
        {
            var split = score.Split('-');
            var a = Convert.ToInt32(split[0]);
            var b = Convert.ToInt32(split[1]);
            return a - b;
        }

        private static TableRow CreateTableRow(int teamId, Season s, IPlayedGame game, int year = 0, bool alwaysMakeBold = true)
        {
            var playedGame = game as PlayedGame;

            if (playedGame == null)
            {
                var bs = game as BowlSummary;
                var myTeam = bs[teamId].Name;
                var otherTeam = bs.GetOpponent(teamId).Name;
                int otherTeamId = bs.GetOpponentId(teamId);
                if (bs.IsWinningTeam(teamId))
                {
                    myTeam = myTeam.MakeWinningTeamBold();
                }
                else
                {
                    otherTeam = otherTeam.MakeWinningTeamBold();
                }


                return new TableRow(
                    myTeam,
                   CreateTeamHrefForRecentMeetings(null, teamId, 35),
                   bs.GetTeamScore(teamId),
                    CreateTeamHrefForRecentMeetings(null, otherTeamId, 35),
                    otherTeam,
                    bs.Name,
                    bs.Year.ToString());
            }
            else
            {
                return CreateTableRow(s, playedGame, year, alwaysMakeBold);
            }
        }

        private static TableRow CreateTableRow(Season s, PlayedGame game, int year = 0, bool alwaysMakeBold = true)
        {
            var b1 = alwaysMakeBold || game.WonGame;
            var b2 = !b1;

            return new TableRow(
                    year,
                    CreateTeamHrefForRecentMeetings(s, game.TeamId, game.Team, b1),
                    CreateTeamHrefForRecentMeetings(s, game.TeamId, 35),
                    CreateBoxScoreHref(s, game),
                    CreateTeamHrefForRecentMeetings(s, game.OppId, 35),
                    CreateTeamHrefForRecentMeetings(s, game.OppId, game.Opponent, b2),
                    game.BowlId.HasValue ? CreateBowlHistoryHref(s, game) : game.Location,
                    CreateYearHref(s));
        }

        #endregion
        #region Team views
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
                    s.ReadTeamFile();

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

        private static Dictionary<int, TableSet> TeamGreats()
        {
            var result = new ConcurrentDictionary<int, TableSet>();

            void CalculateGreats(int teamId)
            {
                const int top = 15;
                var set = new TableSet();
                TeamStats stats = null;
                var sw = Stopwatch.StartNew();

                try
                {
                    foreach (var s in seasons)
                    {
                        stats = s.ProcessTeamStat(teamId);
                    }

                    if (stats != null)
                    {
                        set.AllTimeGreats = new[]
                        {
                            new TableDescriptor(stats.GetAllTimePassers(top)),
                            new TableDescriptor(stats.GetAllTimeQBRushers(top)),
                            new TableDescriptor(stats.GetAllTimeRushers(top)),
                            new TableDescriptor(stats.GetAllTimeRec(top)),
                            new TableDescriptor(stats.GetAllTimeTackles(top)),
                            new TableDescriptor(stats.GetAllTimeSacks(top)),
                            new TableDescriptor(stats.GetAllTimeInt(top)),
                        };
                    }
                }
                catch (Exception ex)
                {
                    set.Debug = ex.ToString();
                }
                sw.Stop();
                logger.WriteLine(sw.Elapsed);
                result.TryAdd(teamId, set);
            }

            Parallel.ForEach(Team.TeamIds, CalculateGreats);
            return result.ToDictionary();
        }
        #endregion
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
            s.ReadConferenceChampFile();

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
            s.ReadBowlChampFile();

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
            s.ReadAllAmericanFile();
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
            s.ReadAwardsFile();

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
            var yearPlayed = s?.Year.ToString() ?? "n/a";
            return "<a href='HeadToHead.html?id=" + oppId + "&yr=" + yearPlayed + "'>" + opp + "</a>";
        }

        static string CreateRecentMeetingsLink(Season s, int teamId, int oppId)
        {
            var yearPlayed = s?.Year.ToString() ?? "n/a";
            return "<a href='recentmeetings.html?id=" + teamId + "&opp=" + oppId + "&yr=" + yearPlayed + "'>Recent Meetings</a>";
        }
        #endregion
    }
}