using DataBaker.Contracts;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;

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
        private static int[] allBowlGames;

        public static int[] AllBowlGames
        {
            get
            {
                if (allBowlGames == null)
                {
                    allBowlGames = seasons.SelectMany(s => s.BowlResults.Keys).Distinct().ToArray();
                }

                return allBowlGames;
            }
        }

        private static Dictionary<int, Tuple<int, int>[]> RivalryGames = new Dictionary<int, Tuple<int, int>[]>()
        {
            { 279, new[]{Tuple.Create(6,93) ,Tuple.Create(11,94), Tuple.Create(83, 89) } },
            { 275, new[]{Tuple.Create(47,112) } },
            { 182, new[]{Tuple.Create(92,71) } },
            { 181, new[]{Tuple.Create(57,8) } },
            { 183, new[]{Tuple.Create(30,27) } },
            { 272, new[]{Tuple.Create(33,79) } },
        };


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
            TeamH2HDrilldown().Bake("teamh2h.filter", true);

            // bowl team records
            BowlTeamRecords().Bake("bowlteamrecords");
            BowlHistory().Bake("bowlhistory", false);
            // TODO RIVALRY GAME HISTORY

            sw.Restart();
            CoachCareer().CoachPlayoffAndH2H().Bake("coachcareer", true);
            sw.Stop();
            logger.WriteLine("CoachCareer baked in " + sw.Elapsed);

            // coaching greats
            CoachingGreats().Bake("coachingGreats", true);

            // game group history
            GameGroup().Bake("groupHistory");

            // team post season
            TeamPostSeasonReport().Bake("bowls", true);
        }

        private static Dictionary<int, Dictionary<string, TableDescriptor>> TeamPostSeasonReport()
        {
            var result = new Dictionary<int, Dictionary<string, TableDescriptor>>();
            foreach (var team in Team.TeamIds)
            {
                result[team] = new Dictionary<string, TableDescriptor>
                {
                    { "bowl", PostSeasonReport(team) },
                    { "playoffs", PostSeasonReport(team, playoffs: true) },
                    { "ccg", PostSeasonReport(team, ccg: true) },
                    { "ko", PostSeasonReport(team, kickoffGames: true) }
                };

                foreach (var bowlId in AllBowlGames)
                {
                    result[team][bowlId.ToString()] = PostSeasonReport(team, bowlId: bowlId);
                }
            }

            return result;
        }

        private static TableDescriptor PostSeasonReport(int teamId, bool playoffs=false, bool ccg=false, int bowlId=0, bool kickoffGames=false)
        {
            Func<PlayedGame, bool> filter = null;
            int win = 0;
            int loss = 0;
            List<TableRow> rows = new List<TableRow>();
            string desc = "Bowl Record ";

            if (playoffs)
            {
                filter = s => s.IsPlayoffBowl();
                desc = "Playoff Record ";
            }
            else if (ccg)
            {
                filter = s => s.Week == 16;
                desc = "Conference Championship Game Record ";
            }
            else if (bowlId != 0)
            {
                filter = s => s.BowlId == bowlId;
            }
            else if (kickoffGames)
            {
                filter = s => s.IsKickoff && s.Year >= 2050;
                desc = "Kickoff Game Record ";
            }
            else
            {
                // all bowl games
                filter = s => s.Week > 16;
            }

            foreach (var s in seasons)
            {
                if (!s.Schedule.ContainsKey(teamId))
                    continue;

                s.ReadTeamScheduleFile();

                var games = s.Schedule[teamId].Where(filter).ToArray();

                var wins = games.Count(g => g.WonGame);
                win += wins;
                loss += (games.Length - wins);

                foreach (var g in games)
                {
                    rows.Insert(0,
                        new TableRow
                        {
                            Cells = new List<string>(
                                new[]
                                {
                                CreateTeamHrefForRecentMeetings(s,teamId,g.Team,g.WonGame),
                                CreateTeamHrefForRecentMeetings(s,teamId,35),
                                CreateBoxScoreHref(s,g),
                                CreateTeamHrefForRecentMeetings(s,g.OppId,35),
                                CreateTeamHrefForRecentMeetings(s,g.OppId,g.Opponent,!g.WonGame),
                                CreateBowlHistoryHref(s,g),
                                CreateTeamBowlHistoryHref(s,g,teamId),
                                CreateYearHref(s)
                                })
                        });
                }
            }

            if (!ccg && !kickoffGames)
            {
                foreach (var bs in PastPlayoffHistory.GetBowlsForTeam(teamId).Where(bowl => !playoffs || bowl.Id.IsPlayoffBowl(bowl.Year)))
                {
                    if (bowlId != 0 && bs.Id != bowlId)
                        continue;

                    //if (bs.Year >= 2014)
                    //{
                    if (bs.WinningTeam.Id == teamId)
                        win++;
                    else
                        loss++;
                    //}

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


                    var row = new TableRow(
                        myTeam,
                       CreateTeamHrefForRecentMeetings(null, teamId, 35),
                       bs.GetTeamScore(teamId),
                        CreateTeamHrefForRecentMeetings(null, otherTeamId, 35),
                        otherTeam,
                        bs.Name,
                        string.Empty,
                        bs.Year.ToString());

                    rows.Add(row);
                }
            }

            desc += win + "-" + loss;

            return new TableDescriptor { Rows = rows, Description = desc };
        }

        public static Dictionary<string,TableDescriptor> GameGroup()
        {
            var result = new Dictionary<string,TableDescriptor>();
            const string kickoff = "kickoff";
            const string ny6 = "ny6";
            const string playoff = "playoff";

            result[kickoff] = new TableDescriptor();
            result[ny6] = new TableDescriptor();
            result[playoff] = new TableDescriptor();

            foreach (var s in seasons)
            {
                s.ReadTeamScheduleFile();
                result[kickoff].Rows.InsertRange(0, s.KickOffGames.Select(g => CreateTableRow(s, g, s.Year)));
                result[ny6].Rows.InsertRange(0, s.NY6Games.Select(g => CreateTableRow(s, g, s.Year)));
                result[playoff].Rows.InsertRange(0, s.PlayoffGames.Select(g => CreateTableRow(s, g, s.Year)));
            }

            foreach (var past in PastPlayoffHistory.Years)
            {
                foreach (var kvp in PastPlayoffHistory.years[past].Where(kvp => kvp.Key.IsPlayoffBowl(past)))
                {
                    var bs = kvp.Value;
                    var row = new TableRow(
                        past,
                        bs.WinningTeam.Name.MakeWinningTeamBold(),
                       CreateTeamHrefForRecentMeetings(null, bs.Winner, 35),
                       bs.Score,
                        CreateTeamHrefForRecentMeetings(null, bs.Loser, 35),
                        bs.LosingTeam.Name,
                        bs.Name,
                        past.ToString());

                    result[playoff].Rows.Add(row);
                }
            }

            return result;
        }

        public static Dictionary<string, List<Coach>> CoachingGreats()
        {
            var result = new Dictionary<string, List<Coach>>();
            Dictionary<CoachKey, Coach> coaches = new Dictionary<CoachKey, Coach>();

            for (int i = seasons.Length - 1; i >= 0; i--)
            {
                var s = seasons[i];
                s.ReadTeamFile();

                foreach (var sk in s.Coaches)
                {
                    if (coaches.ContainsKey(sk.Key) == false && (sk.Value.CareerWin > 0 || sk.Value.CareerLoss > 0))
                    {
                        coaches.Add(sk.Key, sk.Value);
                    }
                }
            }

            var sorts = new string[] {"win", "bowlwin", "cc", "pct", "nc" };

            foreach (var sort in sorts)
            {

                Func<Coach, int> selector = null;

                if (sort == "win") selector = c => c.CareerWin;
                else if (sort == "bowlwin") selector = c => c.CoachBowlWin;
                else if (sort == "cc") selector = c => c.CareerConferenceChampionships;
                else if (sort == "pct") selector = c => c.WinPct;
                else selector = c => c.CareerNationalChampionships;

                var sorted = coaches.Values.OrderByDescending(selector).ThenByDescending(c => c.CareerWin).ToList();
                result[sort] = sorted;
            }

            return result;
        }

        private static void EvaluateCoachPlayoff(KeyValuePair<HashedCoachKey, TableSet> kvp)
        {
            // Perform H2H calculations for the coach
            var key = new CoachKey(kvp.Key.Id, kvp.Key.Name);
            Func<PlayedGame, bool> filterFunc = null;
            var filters = new string[] { "PLAYOFF", "KOG", "BOWL", "CCG" };

            foreach (var filter in filters)
            {
                int win = 0;
                int loss = 0;
                List<TableRow> rows = new List<TableRow>();
                switch (filter.ToUpper())
                {
                    case "PLAYOFF":
                        filterFunc = s => s.IsPlayoffBowl();
                        break;
                    case "KOG":
                        filterFunc = s => s.IsKickoff;
                        break;
                    case "BOWL":
                        filterFunc = s => s.Week > 16;
                        break;
                    case "CCG":
                        filterFunc = s => s.Week == 16;
                        break;
                }

                foreach (var s in seasons)
                {
                    s.ReadTeamScheduleFile();
                    s.ReadTeamFile();

                    if (!s.Coaches.TryGetValue(key, out var coach) || coach.Position != 0)
                    {
                        continue;
                    }

                    var games = s.Schedule[coach.TeamId].Where(filterFunc).ToArray();
                    var wins = games.Count(g => g.WonGame);
                    win += wins;
                    loss += (games.Length - wins);

                    foreach (var g in games)
                    {
                        rows.Insert(0,
                            new TableRow
                            {
                                Cells = new List<string>(
                                    new[]
                                    {
                                CreateTeamHrefForRecentMeetings(s,coach.TeamId,g.Team,g.WonGame),
                                CreateTeamHrefForRecentMeetings(s,coach.TeamId,35),
                                CreateBoxScoreHref(s,g),
                                CreateTeamHrefForRecentMeetings(s,g.OppId,35),
                                CreateTeamHrefForRecentMeetings(s,g.OppId,g.Opponent,!g.WonGame),
                                CreateBowlHistoryHref(s,g),
                                CreateYearHref(s)
                                    })
                            });
                    }
                }

                var desc = kvp.Key.Name + ": " + win + "-" + loss;
                var td = new TableDescriptor { Rows = rows, Description = desc };
                switch (filter.ToUpper())
                {
                    case "PLAYOFF":
                        kvp.Value.CoachPLAYOFF = td;
                        break;
                    case "KOG":
                        kvp.Value.CoachKOG = td;
                        break;
                    case "BOWL":
                        kvp.Value.CoachBOWL = td;
                        break;
                    case "CCG":
                        kvp.Value.CoachCCG = td;
                        break;
                }
            }
        }

        private static void EvaluateCoachH2H(KeyValuePair<HashedCoachKey, TableSet> kvp)
        {
            // Perform H2H calculations for the coach
            var key = new CoachKey(kvp.Key.Id, kvp.Key.Name);
            var h2hSummary = new TableDescriptor()
            {
                Description = kvp.Key.Name,
            };

            var oppDict = new Dictionary<int, List<PlayedGame>>();
            Season latest = null;

            foreach (var s in seasons)
            {
                s.ReadTeamScheduleFile();
                s.ReadTeamFile();

                if (s.Coaches.TryGetValue(key, out var coach) &&
                    coach.Position == 0)
                {
                    latest = s;

                    // add all opponents in that season
                    foreach (var opp in s.Schedule[coach.TeamId])
                    {
                        if (IsFcsTeam(opp.OppId))
                        {
                            continue;
                        }

                        if (!oppDict.TryGetValue(opp.OppId, out var games))
                        {
                            games = new List<PlayedGame>();
                            oppDict.Add(opp.OppId, games);
                        }

                        games.Add(opp);
                    }
                }

                if (coach != null)
                {
                    latest = s;
                }
            }

            // we have all games played, now we need to generate a table for them
            var list = oppDict.Select(o =>
               new
               {
                   Id = o.Key,
                   Win = o.Value.Where(g => g.WonGame).Count(),
                   Loss = o.Value.Where(g => !g.WonGame).Count(),
                   LastMeeting = o.Value.OrderBy(g => g.Year).Last().Year,
                   Name = GetNameForTeamFromSeason(seasons, o.Key),
               })
                .OrderByDescending(e => e.Win + e.Loss)
                .ThenBy(e => e.Name);

            foreach (var r in list)
            {
                h2hSummary.Rows.Add(new TableRow(
                    r.Win + "-" + r.Loss,
                    CreateTeamHistoryLink(latest, r.Id),
                    CreateTeamHrefForRecentMeetings(latest, r.Id, r.Name),
                    CreateYearHref(RuntimeCache.SeasonsDict[r.LastMeeting]),
                    "<a href='coachrecentmeetings.html?id=" + kvp.Key.Id + "&opp=" + r.Id + "&name=" + Uri.EscapeDataString(kvp.Key.Name) + "'>Recent Meetings</a>"
                    ));
            }

            var drilldown = new Dictionary<string, TableDescriptor>();

            foreach(var filter in Team.TeamIds)
            {
                if (!oppDict.ContainsKey(filter))
                {
                    continue;
                }

                var td = new TableDescriptor()
                {
                    Description = CreateSeriesHeader(oppDict[filter].Count(g => g.WonGame), oppDict[filter].Count(g => g.WonGame == false), kvp.Key.Name, TeamNameFromId(filter)),
                };

                var games = oppDict[filter].OrderByDescending(g => g.Year).ThenByDescending(g => g.Week)
                    .Select(g => CreateTableRow(RuntimeCache.SeasonsDict[g.Year], g, alwaysMakeBold: false));

                td.Rows.AddRange(games);
                drilldown[filter.ToString()] = td;
            }

            kvp.Value.CoachH2HSummary = h2hSummary;
            kvp.Value.CoachH2HDrilldown = drilldown;
        }

        private static string TeamNameFromId(int id)
        {
            return seasons.Last().Teams.TryGetValue(id, out var team) ? team.Name : Team.PendingTeamNames[id];
        }

        private static Dictionary<HashedCoachKey, TableSet> CoachPlayoffAndH2H(this Dictionary<HashedCoachKey, TableSet> coaches)
        {
            var headCoaches = coaches.Where(kvp => kvp.Value.CoachBio.HasBeenHeadCoach).ToList();
            Parallel.ForEach(headCoaches, EvaluateCoachH2H);
            Parallel.ForEach(headCoaches, EvaluateCoachPlayoff);
            return coaches;
        }

        private static Dictionary<HashedCoachKey, TableSet> CoachCareer()
        {
            var result = new ConcurrentDictionary<HashedCoachKey, TableSet>();


            for (int i = seasons.Length - 1; i >= 0; i--)
            {
                var season = seasons[i];

                Parallel.ForEach(season.Coaches,
                    coach =>
                    {
                        var key = new HashedCoachKey(coach.Key.Id, coach.Key.Name);

                        if (result.TryGetValue(key, out var set))
                        {
                            set.CoachCareer.Rows.Add(CreateTableRow(season, coach.Value));
                        }
                        else
                        {
                            set = new TableSet();
                            var td = new TableDescriptor();
                            td.Rows.Add(CreateTableRow(season, coach.Value));
                            set.CoachBio = coach.Value;
                            set.CoachCareer = td;
                            result[key] = set;
                        }
                    });
            }

            return result.ToDictionary();
        }

        private static TableRow CreateTableRow(Season s, Coach coach)
        {
            var team = s.Teams[coach.TeamId];

            var mediaRank = "-";

            if (team.MediaPollRank <= 25)
                mediaRank = "#" + team.MediaPollRank;

            List<string> summary = CreateSummary(team);

            switch (coach.Position)
            {
                case 0:
                    summary.Add("#" + team.RecruitClassRank + " Recruiting Class");
                    break;
                case 1:
                    summary.Add("#" + team.OffensiveRankings.Overall + " Offense");
                    summary.Add("#" + team.OffensiveRankings.Passing + " Passing Offense");
                    summary.Add("#" + team.OffensiveRankings.Rushing + " Rushing Offense");
                    break;
                case 2:
                    summary.Add("#" + team.DefensiveRankings.Overall + " Defense");
                    break;
                default:
                    break;
            }

            var teamName = team.Name;

            if (team.CoachesPollRank <= 25)
            {
                teamName = "#" + team.CoachesPollRank + " " + teamName;
            }

            string coach1 = null;
            string coach2 = null;

            if (coach.Position == 0)
            {
                coach1 = createCoachLink(team.CoachingStaff[1], s);
                coach2 = createCoachLink(team.CoachingStaff[2], s);
            }
            else if (coach.Position == 1)
            {
                coach1 = createCoachLink(team.CoachingStaff[0], s);
                coach2 = createCoachLink(team.CoachingStaff[2], s);
            }
            if (coach.Position == 2)
            {
                coach1 = createCoachLink(team.CoachingStaff[0], s);
                coach2 = createCoachLink(team.CoachingStaff[1], s);
            }

            return new TableRow(s.Year,
                CreateYearHref(s),
                coach.Age.ToString(),
                coach.Job,
                CreateTeamHrefForRecentMeetings(s, team.Id, teamName, false, team.Win, team.Loss, true),
                CreateTeamHistoryLink(s, team.Id),
                mediaRank,
                string.Join(", ", summary),
                coach1,
                coach2);
        }

        /// <summary>
        /// will write to a file prefix.key.txt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="prefix"></param>
        private static void Bake<K,T>(this Dictionary<K, T> dict, string prefix, bool compress = false)
        {
            foreach (var kvp in dict)
            {
                var fileName = $"{prefix}.{kvp.Key}.txt";
                var file = Path.Combine(Helper.BakedPath, fileName);

                if (compress)
                {
                    File.WriteAllText(file, Convert.ToBase64String(JsonConvert.SerializeObject(kvp.Value).ZipItGood()));
                }
                else
                {
                    File.WriteAllText(file, JsonConvert.SerializeObject(kvp.Value));
                }
            }
        }

        private static Dictionary<int, TableDescriptor> BowlHistory()
        {
            var dict = new Dictionary<int, TableDescriptor>();
            foreach (var bowlId in AllBowlGames)
            {
                dict[bowlId] = BowlHistory(bowlId);
            }

            return dict;
        }

        private static TableDescriptor BowlHistory(int bowlId, int teamId = 0)
        {
            Tuple<int, int>[] matchup = null;
            if (RivalryGames.TryGetValue(bowlId, out matchup))
            {
                if ((bowlId == 272 && teamId != 0) || bowlId != 272)
                {
                    return RivalryGameHistory(
                        bowlId,
                        matchup.Length == 1 ?
                        matchup[0] :
                        matchup.First(t => t.Item1 == teamId || t.Item2 == teamId));
                }
            }

            var td = new TableDescriptor();
            string name = null;
            foreach (var s in seasons)
            {
                s.ReadFromFile("tsch.csv", Season.scheduleKey);

                if (s.BowlResults.ContainsKey(bowlId))
                {
                    foreach (var bowl in s.BowlResults[bowlId])
                    {
                        td.Rows.Insert(0, CreateTableRow(s, bowl));
                        name = bowl.Location;
                    }
                }
            }

            foreach (var past in PastPlayoffHistory.Years)
            {
                BowlSummary bs;
                if (PastPlayoffHistory.years[past].TryGetValue(bowlId, out bs))
                {
                    var row = new TableRow(
                        bs.WinningTeam.Name.MakeWinningTeamBold(),
                       CreateTeamHrefForRecentMeetings(null, bs.Winner, 35),
                       bs.Score,
                        CreateTeamHrefForRecentMeetings(null, bs.Loser, 35),
                        bs.LosingTeam.Name,
                        bs.Name,
                        past.ToString());

                    td.Rows.Add(row);
                }
            }

            td.Description = name;
            return td;
        }

        private static TableDescriptor RivalryGameHistory(int gameLocation, Tuple<int, int> matchup)
        {
            var td = new TableDescriptor();

            foreach (var s in seasons)
            {
                s.ReadFromFile("tsch.csv", Season.scheduleKey);
                var game = s.Schedule.SelectMany(kvp => kvp.Value).Where(pg => pg.BowlId.HasValue && pg.BowlId == gameLocation && pg.WonGame && (pg.TeamId == matchup.Item1 || pg.TeamId == matchup.Item2)).FirstOrDefault();

                if (game != null)
                {
                    td.Rows.Insert(0, CreateTableRow(s, game));
                }
            }

            return td;
        }


        private static Dictionary<int, Dictionary<int, TableDescriptor>> BowlTeamRecords()
        {
            var result = new Dictionary<int, Dictionary<int, TableDescriptor>>();

            // find all the possible bowls
            foreach (var id in AllBowlGames)
            {
                var sorts = new Dictionary<int, TableDescriptor>();
                for (int i = 0; i <= 3; i++)
                {
                    sorts[i] = BowlTeamRecords(id, i);
                }

                result[id] = sorts;
            }

            return result;
        }

        private static TableDescriptor BowlTeamRecords(int bowlId, int sort)
        {
            var td = new TableDescriptor();
            Dictionary<int, TeamBowlAppearances> dict = new Dictionary<int, TeamBowlAppearances>();
            int season = seasons[0].Year;

            foreach (var s in seasons)
            {
                s.ReadFromFile("tsch.csv", Season.scheduleKey);

                if (s.BowlResults.ContainsKey(bowlId))
                {
                    foreach (var bowl in s.BowlResults[bowlId])
                    {
                        HandleTeam(dict, bowl.TeamId, bowl.WonGame, bowl.Team);
                        HandleTeam(dict, bowl.OppId, !bowl.WonGame, bowl.Opponent);
                    }
                }
            }

            foreach (var past in PastPlayoffHistory.Years)
            {
                BowlSummary bs;
                if (PastPlayoffHistory.years[past].TryGetValue(bowlId, out bs))
                {
                    HandleTeam(dict, bs.Winner, true, bs.WinningTeam.Name);
                    HandleTeam(dict, bs.Loser, false, bs.LosingTeam.Name);
                }
            }

            IEnumerable<TeamBowlAppearances> appearances = null;

            switch (sort)
            {
                case 2:
                    appearances = dict.Values.OrderByDescending(t => t.Pct).ThenByDescending(t => t.Appearances).ThenBy(t => t.Name);
                    break;
                case 3:
                    appearances = dict.Values.OrderByDescending(t => t.Appearances).ThenByDescending(t => t.Wins).ThenBy(t => t.Name);
                    break;
                case 1:
                    appearances = dict.Values.OrderByDescending(t => t.Loss).ThenByDescending(t => t.Appearances).ThenBy(t => t.Name);
                    break;
                case 0:
                default:
                    appearances = dict.Values.OrderByDescending(t => t.Wins).ThenByDescending(t => t.Appearances).ThenBy(t => t.Name);
                    break;
            }

            foreach (var team in appearances)
            {
                var row = new TableRow(
                            string.Format("<a href=PostSeasonGames.html?id={0}&year={1}><b>{2}</b></a>", team.TeamId, season, team.Name),
                  CreateTeamHrefForRecentMeetings(null, team.TeamId, 35),
                   team.Wins.ToString(),
                   team.Loss.ToString(),
                   team.Tie.ToString(),
                   team.Pct,
                            string.Format("<a href=PostSeasonGames.html?id={0}&year={1}&bowlId={2}><b>{3}</b></a>", team.TeamId, season, bowlId, team.Appearances));

                td.Rows.Add(row);
            }

            return td;
        }

        private static void HandleTeam(Dictionary<int, TeamBowlAppearances> dict, int teamId, bool wonGame, string name)
        {
            if (!dict.TryGetValue(teamId, out var appearance))
            {
                dict[teamId] = appearance = new TeamBowlAppearances { TeamId = teamId };
            }

            appearance.Wins += wonGame ? 1 : 0;
            appearance.Loss += wonGame ? 0 : 1;
            var lastSpace = name.LastIndexOf(" (");

            if (lastSpace > 0)
                appearance.Name = name.Substring(0, lastSpace);
            else
                appearance.Name = name;

            if (appearance.Name.StartsWith("#"))
            {
                int idx = 0;
                while (true)
                {
                    if (Char.IsLetter(appearance.Name[idx]))
                        break;
                    idx++;
                }

                appearance.Name = appearance.Name.Substring(idx);
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

        public static Dictionary<int, TableDescriptor> OverallTeamH2H(bool sort = false)
        {
            var result = new Dictionary<int, TableDescriptor>();
            foreach (var teamId in Team.TeamIds)
            {
                result[teamId] = GetTeamH2H(teamId, sortByRecent: sort);
            }

            return result;
        }

        private static TableDescriptor GetTeamH2H(int teamId, int filter = 0, bool sortByRecent = false)
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
                    gameResults = new List<IPlayedGame>();
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
            return "<a href='" + location + "/CoachCareer.html?yr=" + s.Year + "&name=" + Uri.EscapeDataString(c.Name) + "&id=" + c.Id + "'>" + title + "</a>"; ;
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