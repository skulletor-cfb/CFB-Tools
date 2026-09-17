using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EA_DB_Editor
{
    public static class ConfScheduleFixer
    {

        /// <summary>
        /// put fcs games the earliest possible
        /// </summary>
        /// <param name="schedules"></param>
        public static void FcsGamesEarly(Dictionary<int, TeamSchedule> schedules)
        {
            var fcsGames = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsFCSGame())).Distinct().ToArray();
            var secLateFCSGame = new HashSet<int>();

            foreach (var game in fcsGames)
            {
                var team = game.HomeTeam.IsFcsTeam() ? game.AwayTeam : game.HomeTeam;
                var currentGameWeek = game.WeekIndex;
                var teamSchedule = schedules[team];

                var firstTime = secLateFCSGame.Add(team);
                int week = 0;

                if (team.IsSECTeam() && firstTime)
                {
                    week = teamSchedule.FindLastOpenWeekForFcs();
                }
                else
                {
                    week = teamSchedule.FindOpenWeeks().First();
                }

                game.SetWeek(week);
                teamSchedule[week] = game;
                teamSchedule[currentGameWeek] = null;
            }
        }

        private static void RemoveFromQueue(this Queue<PreseasonScheduledGame> queue, int team)
        {
            var list = new List<PreseasonScheduledGame>();
            int teamToLookFor = 0;

            while (queue.Count > 0)
            {
                var g = queue.Dequeue();

                if (g.HomeTeam != team && g.AwayTeam != team)
                {
                    list.Add(g);
                    continue;
                }

                teamToLookFor = g.HomeTeam == team ? g.AwayTeam : g.HomeTeam;
            }

            var first = list.Where(g => g.HomeTeam == teamToLookFor || g.AwayTeam == teamToLookFor).FirstOrDefault();

            if (first != null)
            {
                list.Remove(first);
                queue.Enqueue(first);
            }

            foreach (var l in list.Where(i => i != null))
            {
                queue.Enqueue(l);
            }
        }

        private static List<PreseasonScheduledGame> FindExtraBig12Games(Dictionary<int, TeamSchedule> schedules)
        {
#if true
            var result = new List<PreseasonScheduledGame>();
            var normalized = new Dictionary<int, int>();

            // take away one conference game per team
            var found = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsBig12ConfGame()))
                .OrderBy(g => g.WeekIndex)
                .Distinct().ToArray();


            // we already did this
            if (found.Length == (15 * 4))
            {
                return new List<PreseasonScheduledGame>();
            }

            found.Shuffle();
            var queue = new Queue<PreseasonScheduledGame>(found);


            while (result.Count < 15)
            {
                if (queue.Count == 0)
                {
                    return FindExtraBig12Games(schedules);
                }

                var game = queue.Dequeue();

                var htc = 0;
                var atc = 0;
                // we haven't seen these teams before, easy add them
                if ((!normalized.TryGetValue(game.HomeTeam, out htc) || htc < 2) &&
                    (!normalized.TryGetValue(game.AwayTeam, out atc) || atc < 2))
                {
                    result.Add(game);
                    normalized[game.HomeTeam] = ++htc;
                    normalized[game.AwayTeam] = ++atc;

                    if (htc >= 2)
                    {
                        queue.RemoveFromQueue(game.HomeTeam);
                    }

                    if (atc >= 2)
                    {
                        queue.RemoveFromQueue(game.AwayTeam);
                    }
                }
                else
                {
                    queue.Enqueue(game);
                }
            }

            return result;
#else
            return new List<PreseasonScheduledGame>();
#endif
        }

        private static List<PreseasonScheduledGame> FindExtraAccGames(Dictionary<int, TeamSchedule> schedules)
        {
#if true
            var result = new List<PreseasonScheduledGame>();
            var normalized = new HashSet<int>();

            // take away one conference game per team
            var found = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsAccConfGame()))
                .OrderBy(g => g.WeekIndex)
                .Distinct().ToArray();

            // we already did this
            if (found.Length == (16 * 4))
            {
                return new List<PreseasonScheduledGame>();
            }

            found.Shuffle();

            var queue = new Queue<PreseasonScheduledGame>(found);


            while (result.Count < 8)
            {
                if (queue.Count == 0)
                {
                    return FindExtraAccGames(schedules);
                }

                var game = queue.Dequeue();

                // we haven't seen these teams before, easy add them
                if (!normalized.Contains(game.HomeTeam) && !normalized.Contains(game.AwayTeam))
                {
                    result.Add(game);
                    normalized.Add(game.HomeTeam);
                    normalized.Add(game.AwayTeam);
                    queue.RemoveFromQueue(game.HomeTeam);
                    queue.RemoveFromQueue(game.AwayTeam);
                }
                else
                {
                    queue.Enqueue(game);
                }
            }

            return result;
#else
            return new List<PreseasonScheduledGame>();
#endif
        }

        private static List<PreseasonScheduledGame> FindExtraSunBeltGames(Dictionary<int, TeamSchedule> schedules)
        {
            var result = new List<PreseasonScheduledGame>();
            return result;
            var normalized = new HashSet<int>();

            // take away one conference game per team
            var found = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsSunBeltGame() && g.IsCrossDivisionGame()))
                .OrderBy(g => g.WeekIndex)
                .Distinct().ToArray();
            var queue = new Queue<PreseasonScheduledGame>(found);


            while (result.Count < 8)
            {
                var game = queue.Dequeue();

                // we haven't seen these teams before, easy add them
                if (!normalized.Contains(game.HomeTeam) && !normalized.Contains(game.AwayTeam))
                {
                    result.Add(game);
                    normalized.Add(game.HomeTeam);
                    normalized.Add(game.AwayTeam);
                    queue.RemoveFromQueue(game.HomeTeam);
                    queue.RemoveFromQueue(game.AwayTeam);
                }
                else
                {
                    queue.Enqueue(game);
                }
            }

            return result;
        }

        public static int GetGameNumber()
        {
            return monotronic++;
        }

        static int monotronic = 200;

        private static void TemporarilyRemoveFcsGameFromSchedule(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game)
        {
            var gameIndex = game.WeekIndex;
            var home = game.HomeTeam;
            var homeSchd = schedules[home];
            homeSchd[gameIndex] = null;
            homeSchd.AddUnscheduledGame(game, GetGameNumber());
        }

        private static void TemporarilyRemoveGameFromSchedule(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game)
        {
            var gameIndex = game.WeekIndex;
            var home = game.HomeTeam;
            var away = game.AwayTeam;
            var homeSchd = schedules[home];
            var awaySchd = schedules[away];
            homeSchd[gameIndex] = null;
            awaySchd[gameIndex] = null;

            if (gameIndex < 5 && FindCommonOpenWeek(homeSchd.FindOpenWeeksWithBuffer(), awaySchd.FindOpenWeeksWithBuffer(), true, out var week))
            {
                var gameNum = GetGameNumber();
                homeSchd.SetUnscheduleGame(game, week, gameNum);
                awaySchd.SetUnscheduleGame(game, week, gameNum);
            }
        }

        public static void TryMoveNonConfGamesEarly(Dictionary<int, TeamSchedule> schedules)
        {
            // remove all fcs games from schedules
            var fcs = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsFCSGame())).Distinct().OrderBy(g => g.WeekIndex).ToArray();
            foreach (var game in fcs)
            {
                TemporarilyRemoveFcsGameFromSchedule(schedules, game);
            }


            var nonConf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.Week > 4 && !g.IsLateSeasonRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame())).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();

            // non conf go earlier
            foreach (var game in nonConf)
            {
                var gameIndex = game.WeekIndex;
                var home = game.HomeTeam;
                var away = game.AwayTeam;
                var homeSchd = schedules[home];
                var awaySchd = schedules[away];

                if (FindCommonOpenWeek(homeSchd.FindOpenWeeks(), awaySchd.FindOpenWeeks(), out var opening))
                {
                    game.SetWeek(opening);
                    game.GameNumber = monotronic++;
                    homeSchd[opening] = game;
                    awaySchd[opening] = game;
                    homeSchd[gameIndex] = null;
                    awaySchd[gameIndex] = null;
                }
            }

            // put the fcs game back for each team, sec gets them late
            foreach (var game in fcs)
            {
                var currentGameWeek = game.WeekIndex;
                var home = game.HomeTeam;
                var homeSchd = schedules[home];
                var openWeek = (home.IsSECTeam() || home == 68) ? homeSchd.FindLastOpenWeekForFcs() : homeSchd.FindOpenWeeks().First();

                // set the game in the schedule
                game.SetWeek(openWeek);
                game.GameNumber = monotronic++;
                homeSchd[openWeek] = game;
                homeSchd[currentGameWeek] = null;
            }

            FcsGamesEarly(schedules);
        }

        /// <summary>
        /// all this function should do is remove all conference games from the and put non conf early in the schedule
        /// </summary>
        /// <param name="schedules"></param>
        /// <param name="confWeek"></param>
        public static void MoveNonConfGamesEarly(Dictionary<int, TeamSchedule> schedules, int confWeek = 13)
        {
            // remove all fcs games from schedules
            var fcs = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsFCSGame())).Distinct().OrderBy(g => g.WeekIndex).ToArray();
            foreach (var game in fcs)
            {
                TemporarilyRemoveFcsGameFromSchedule(schedules, game);
            }

            var conf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsConferenceGame() && !g.IsSecConfGame())).Distinct().OrderBy(g => g.WeekIndex).ToArray();
            var nonConf = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsLateSeasonRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame())).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();

            foreach (var psg in conf.Concat(nonConf))
            {
                TemporarilyRemoveGameFromSchedule(schedules, psg);
            }

            // reread these
            nonConf = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsLateSeasonRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame())).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();
            fcs = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsFCSGame())).Distinct().OrderBy(g => g.WeekIndex).ToArray();

            // non conf go earlier
            foreach (var game in nonConf)
            {
                var gameIndex = game.WeekIndex;
                var home = game.HomeTeam;
                var away = game.AwayTeam;
                var homeSchd = schedules[home];
                var awaySchd = schedules[away];

                if (FindCommonOpenWeek(homeSchd.FindOpenWeeks(), awaySchd.FindOpenWeeks(), out var opening))
                {
                    game.SetWeek(opening);
                    game.GameNumber = monotronic++;
                    homeSchd[opening] = game;
                    awaySchd[opening] = game;
                    homeSchd[gameIndex] = null;
                    awaySchd[gameIndex] = null;
                }
            }

            // put the fcs game back for each team, sec gets them late
            foreach (var game in fcs)
            {
                var home = game.HomeTeam;
                var homeSchd = schedules[home];
                var openWeek = (home.IsSECTeam() || home == 68) ? homeSchd.FindLastOpenWeekForFcs() : homeSchd.FindOpenWeeks().First();

                // set the game in the schedule
                game.SetWeek(openWeek);
                game.GameNumber = monotronic++;
                homeSchd[openWeek] = game;
            }

            FcsGamesEarly(schedules);
        }

        public static void ReplaceFcsOnlyGames(Dictionary<int, TeamSchedule> schedules)
        {
            // find the games with fcs home team
            var fcsGames = schedules.Values.SelectMany(games => games.Where(g => g != null && g.HomeTeam.IsFcsTeam() && g.AwayTeam.IsFcsTeam())).Distinct().ToArray();

            if (fcsGames.Length == 0)
            {
                return;
            }

            var stack = new Stack<PreseasonScheduledGame>(fcsGames);

            // should not remove more than 8 games, but only 1 per team
            var extraConfGames = FindExtraSunBeltGames(schedules)
                .Concat(FindExtraAccGames(schedules)
                .Concat(FindExtraBig12Games(schedules))
                );

            // p5-p5 games late in the season
            var replaceableGamesP5 = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame() && g.IsP5Game() && g.WeekIndex > 4)).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();
            var replaceableGamesAny = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame() && !g.IsP5Game() && g.WeekIndex > 4)).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();
            var replaceableGamesEarlyy = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame() && g.WeekIndex <= 4)).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();
            var oocGames = extraConfGames.Concat(replaceableGamesP5).Concat(replaceableGamesAny).Concat(replaceableGamesEarlyy);

            foreach (var game in oocGames)
            {
                // nothing in the stack, clear it all out
                if (!stack.Any())
                {
                    return;
                }

                var fcsGame = stack.Pop();
                var gameIndex = game.WeekIndex;

                // swap so the fcs team is always on the road
                var fcsTeam = fcsGame.AwayTeam;
                var fbsTeam = game.AwayTeam;
                var fcsWeek = fcsGame.WeekIndex;
                game.MaddenRecord["GATG"] = fcsTeam.ToString();
                game.AwayTeam = fcsTeam;

                fcsGame.MaddenRecord["GHTG"] = fbsTeam.ToString();
                fcsGame.HomeTeam = fbsTeam;

                var fbsTeamSchedule = schedules[fbsTeam];

                for (int i = 0; i < fbsTeamSchedule.Length; i++)
                {
                    if (fbsTeamSchedule[i] == null)
                    {
                        fcsGame.SetWeek(i);
                        schedules[fbsTeam][i] = fcsGame;
                        break;
                    }
                }

                schedules[fcsTeam][fcsWeek] = null;
                schedules[fbsTeam][gameIndex] = null;
            }
        }

        static List<int> fcsOpenings = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

        static List<int> FcsOpenings() => fcsOpenings;

        public static void ExtraConfGameSwap(Dictionary<int, TeamSchedule> schedules, Func<PreseasonScheduledGame, bool> g5Qualifier, Func<PreseasonScheduledGame, bool> confGameQualifier)
        {
            // give me all g5 games
            var gamesToReplace = schedules.Values.SelectMany(games => games.Where(g => g != null && g5Qualifier(g) && g.IsG5Game() && !g.IsFCSGame() && !g.IsRivalryGame())).GroupBy(g => g.WeekIndex).ToDictionary(g => g.Key, g => g.Distinct().ToList());

            // give me all extra games
            var conf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.MustReplace && confGameQualifier(g))).Distinct().ToList();

            for (int i = 0; i < conf.Count; i++)
            {
                var game = conf[i];

                List<PreseasonScheduledGame> potentialGames = null;

                if (gamesToReplace.TryGetValue(game.WeekIndex, out potentialGames) && potentialGames.Count > 0)
                {
                    var swapee = potentialGames[0];
                    potentialGames.RemoveAt(0);
                    ScheduleFixup.SwapTeams(game, swapee);
                    schedules[game.AwayTeam][game.WeekIndex] = game;
                    schedules[game.HomeTeam][game.WeekIndex] = game;
                    schedules[swapee.HomeTeam][swapee.WeekIndex] = swapee;
                    schedules[swapee.AwayTeam][swapee.WeekIndex] = swapee;
                    swapee.MustReplace = false;
                    game.MustReplace = false;
                }
            }
        }

        public static void SwapG5ForP5HomeTeam(Dictionary<int, TeamSchedule> schedules)
        {
            // give me all g5 fcs games
            var g5fcs = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsAmericanGame() && g.IsG5FCSGame())).Distinct().ToList();

            // give me all p5 games
            var gamesToReplace = schedules.Values
                            .SelectMany(games => games.Where(g => g != null && !g.IsRivalryGame() && g.IsP5Game())).GroupBy(g => g.WeekIndex).ToDictionary(g => g.Key, g => g.Distinct().ToList());

            if (gamesToReplace.Count == 0)
                return;

            for (int i = 0; i < g5fcs.Count; i++)
            {
                var game = g5fcs[i];
                List<PreseasonScheduledGame> potentialGames = null;

                if (gamesToReplace.TryGetValue(game.WeekIndex, out potentialGames) && potentialGames.Count > 0)
                {
                    for (int j = 0; j < potentialGames.Count; j++)
                    {
                        var swapee = potentialGames[j];

                        // we successfully swapped teams, we can break out of the loop
                        if (ScheduleFixup.SwapTeams(game, swapee))
                        {
                            potentialGames.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }


        public static void G5FCSSwap(Dictionary<int, TeamSchedule> schedules, bool anyP5Game = false)
        {
            // this code has never been executed, not sure what happens when it does
            return;
            // give me all g5 fcs games
            var g5fcs = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsAmericanGame() && g.IsG5Game() && g.IsFCSGame())).Distinct().ToList();

            if (g5fcs.Count < 3)
                return;

            bool exhausted = false;

            // give me all p5 ooc games
            Dictionary<int, List<PreseasonScheduledGame>> gamesToReplace = null;

            if (anyP5Game)
            {
                gamesToReplace = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsRivalryGame() && (g.IsExtraAccGame() || g.IsP5Game()))).GroupBy(g => g.WeekIndex).ToDictionary(g => g.Key, g => g.Distinct().ToList());
            }
            else
            {
                gamesToReplace = schedules.Values.SelectMany(games => games.Where(g => g != null && (g.MustReplace))).GroupBy(g => g.WeekIndex).ToDictionary(g => g.Key, g => g.Distinct().ToList());
            }

            // nothing to replace, proceed
            if (gamesToReplace.Count == 0)
            {
                return;
            }

            for (int i = 0; i < g5fcs.Count; i++)
            {
                var game = g5fcs[i];
                List<PreseasonScheduledGame> potentialGames = null;

                if (gamesToReplace.TryGetValue(game.WeekIndex, out potentialGames) && potentialGames.Count > 0)
                {
                    var swapee = potentialGames[0];
                    potentialGames.RemoveAt(0);
                    ScheduleFixup.SwapTeams(game, swapee);
                    schedules[game.AwayTeam][game.WeekIndex] = game;
                    schedules[game.HomeTeam][game.WeekIndex] = game;
                    schedules[swapee.HomeTeam][swapee.WeekIndex] = swapee;
                    swapee.MustReplace = false;
                    game.MustReplace = false;
                }
                else
                {
                    if (g5fcs.Count > 100)
                        break;

                    // move to a new week and append
                    if (game.WeekIndex == 14)
                    {
                        exhausted = true;
                    }

                    var open = schedules[game.HomeTeam].FindNextOpenWeek(game.WeekIndex);
                    schedules[game.HomeTeam].MoveFcsGame(game.WeekIndex, open);

                    if (game.HomeTeam.IsFcsTeam() && game.AwayTeam.IsFcsTeam())
                    {
                        continue;
                    }

                    g5fcs.Add(game);
                }
            }

            if (exhausted)
            {
                G5FCSSwap(schedules, true);
            }
        }

        static Random _random = new Random();

        /// <summary>
        /// Shuffle the array.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="array">Array to shuffle.</param>
        public static T[] Shuffle<T>(this T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                // Use Next on random instance with an argument.
                // ... The argument is an exclusive bound.
                //     So we will not go past the end of the array.
                int r = i + _random.Next(n - i);
                var t = array[r];
                array[r] = array[i];
                array[i] = t;
            }

            return array;
        }

        public static T[] CreateAndShuffle<T>(this T[] array)
        {
            var arr = new List<T>(array).ToArray();
            return Shuffle(arr);
        }

        public static void Fix(Dictionary<int, TeamSchedule> schedules, ConferenceLocks confLocks)
        {
            void Audit(int expectedGames)
            {
                // everyone should have 8 games 
                var audit = schedules.Where(kvp => !kvp.Key.IsFcsTeam() && kvp.Value.ScheduledGameCount != expectedGames).ToArray();
                if (audit.Any())
                {
                    Debug.WriteLine(audit.Select(kvp => $"{kvp.Key}:{kvp.Value.ScheduledGameCount}").ToArray().ToJson());
                }
            }

            var teams = TableUtility.TeamAndConferences.Keys.ToArray();

            // assign locks for everyone
            var allGames = schedules.Values.SelectMany(s => s).Where(g => g != null).Select(g => { g.CheckForLock(confLocks); return g; }).Distinct().ToList();
            var lockedGames = allGames.Where(g => g.LockedWeek.HasValue).ToList();
            var allConfGames = allGames.Where(g => g.IsConferenceGame() && !g.LockedWeek.HasValue).ToList();
            var allFcsGames = allGames.Where(g => g.IsFCSGame()).ToList();
            var allNonConGames = allGames.Where(g => !g.IsFCSGame() && !g.IsConferenceGame() && !g.LockedWeek.HasValue).ToList();
            var leftover = new List<PreseasonScheduledGame>();

            // remove all games , so we can lay it all back out
            foreach (var team in teams)
            {
                var schedule = schedules[team];

                for (int i = 0; i < schedule.Length; i++)
                {
                    schedule[i] = null;
                }
            }

            // add in all the locked games first
            foreach (var game in lockedGames)
            {
                if(!AssignGame(game, schedules))
                {
                    leftover.Add(game);
                }
            }

            // then conference games
            foreach (var game in allConfGames)
            {
                if (!TryAssignGameLate(schedules, game))
                {
                    leftover.Add(game);
                }
            }

            leftover = TryAssignLeftovers(leftover, schedules);
            Audit(8);

            // then non con
            foreach (var game in allNonConGames)
            {
                if (!TryAssignGameEarly(schedules, game))
                {
                    leftover.Add(game);
                }
            }

            // then leftover games
            leftover = TryAssignLeftovers(leftover, schedules);

            // finally fcs can go anywhere
            foreach (var game in allFcsGames)
            {
                TryAssignGameEarly(schedules, game);
            }

            Audit(12);
        }

        private static List<PreseasonScheduledGame> TryAssignLeftovers(List<PreseasonScheduledGame> games, Dictionary<int, TeamSchedule> schedules)
        {
            var result = new List<PreseasonScheduledGame>();

            // we're going to find it anyway
            foreach (var game in games)
            {
                if (FindCommonOpenWeek(schedules[game.HomeTeam].FindOpenWeeks(), schedules[game.AwayTeam].FindOpenWeeks(), true, out var week))
                {
                    game.SetWeek(week);
                    schedules[game.HomeTeam][week] = game;
                    schedules[game.AwayTeam][week] = game;
                }
                else
                {
                    result.Add(game);
                }
            }

            return result;
        }

        private static bool AssignToPlaceholder(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game)
        {
            if (game.AssignGame(schedules, 14))
            {
                return true;
            }

            if (game.AssignGame(schedules, 15))
            {
                return true;
            }

            return false;
        }

        public static bool TryAssignGameEarly(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game)
        {
            // find the latest available week for both teams
            for (int i = 0; i <= 13; i++)
            {
                if (game.AssignGame(schedules, i, true))
                {
                    return true;
                }
            }

            return AssignToPlaceholder(schedules, game);
        }

        public static bool TryAssignGameLate(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game)
        {
            // find the latest available week for both teams
            for (int i = 13; i >= 0; i--)
            {
                if (game.AssignGame(schedules, i, true))
                {
                    return true;
                }
            }

            return AssignToPlaceholder(schedules, game);
        }



        public static void SetHomeTeamForRoundRobin(Dictionary<int, TeamSchedule> schedules, Dictionary<int, int[]> homeGames)
        {
            foreach (var game in schedules.Values.SelectMany(s => s.Where(g => g != null && g.IsConferenceGame() && homeGames.ContainsKey(g.HomeTeam))))
            {
                game.SetHomeTeam(homeGames);
            }
        }

        private static HashSet<PreseasonScheduledGame> RejectedOnce = new HashSet<PreseasonScheduledGame>();

        public static bool AssignGame(this PreseasonScheduledGame game, Dictionary<int, TeamSchedule> schedules, int week)
            => game.AssignGame(schedules, week, false);

        public static bool AssignGame(this PreseasonScheduledGame game, Dictionary<int, TeamSchedule> schedules, int week, bool postFix)
        {
            var homeSchedule = schedules[game.HomeTeam];
            var awaySchedule = game.AwayTeam.IsFcsTeam() ? new TeamSchedule(true) : schedules[game.AwayTeam];

            // we don't want 3 or 4 away games in a row, try to mitigate that
            if (postFix && !RejectedOnce.Contains(game) && game.IsConferenceGame())
            {
                var awayTeam = game.AwayTeam;
                var leftAwayGames = 0;
                var rightAwayGames = 0;

                // not more than 2 away games
                for (int i = week + 1; i < awaySchedule.Length; i++)
                {
                    if (awaySchedule[i] == null) continue;

                    if (awaySchedule[i].AwayTeam == awayTeam)
                    {
                        rightAwayGames++;
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = week - 1; i >= 0; i--)
                {
                    if (awaySchedule[i] == null) continue;

                    if (awaySchedule[i].AwayTeam == awayTeam)
                    {
                        leftAwayGames++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (
                    (leftAwayGames > 0 && rightAwayGames > 0) ||
                    (leftAwayGames >= 2 || rightAwayGames >= 2))
                {
                    RejectedOnce.Add(game);

                    var conf = TableUtility.TeamAndConferences[awayTeam];
                    var findLate = true;// conf == TableUtility.Pac16Id || conf == TableUtility.Big10Id || conf == TableUtility.MACId || conf == TableUtility.SBCId || conf == TableUtility.CUSAId || conf == TableUtility.AmericanId || conf == TableUtility.MWCId;

                    var finalTry = FindCommonOpenWeek(homeSchedule.FindOpenWeeks(week), awaySchedule.FindOpenWeeks(week), findLate, out var nextOpen) ? nextOpen : week;
                    return AssignGame(game, schedules, finalTry);
                }

                // not more than 2 home games
                var homeTeam = game.HomeTeam;
                var leftHomeGames = 0;
                var rightHomeGames = 0;

                for (int i = week + 1; i < awaySchedule.Length; i++)
                {
                    if (homeSchedule[i] == null) continue;

                    if (homeSchedule[i].HomeTeam == homeTeam)
                    {
                        rightHomeGames++;
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = week - 1; i >= 0; i--)
                {
                    if (homeSchedule[i] == null) continue;

                    if (homeSchedule[i].HomeTeam == homeTeam)
                    {
                        leftHomeGames++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (
                    (leftHomeGames > 0 && rightHomeGames > 0) ||
                    (leftHomeGames >= 2 || rightHomeGames >= 2))
                {
                    RejectedOnce.Add(game);

                    var conf = TableUtility.TeamAndConferences[awayTeam];
                    var findLate = true;// conf == TableUtility.Pac16Id || conf == TableUtility.Big10Id || conf == TableUtility.MACId || conf == TableUtility.SBCId || conf == TableUtility.CUSAId || conf == TableUtility.AmericanId || conf == TableUtility.MWCId;

                    var finalTry = FindCommonOpenWeek(homeSchedule.FindOpenWeeks(week), awaySchedule.FindOpenWeeks(week), findLate, out var nextOpen) ? nextOpen : week;
                    return AssignGame(game, schedules, finalTry);
                }
            }

            if (awaySchedule[week] == null && homeSchedule[week] == null)
            {
                //var currentWeek = game.WeekIndex;
                game.SetWeek(week);
                homeSchedule[week] = game;
                awaySchedule[week] = game;
                return true;
            }

            // suppose one or both teams have a fcs game that week, we can move it easily
            if (homeSchedule[week] == null && awaySchedule[week] != null && awaySchedule[week].IsFCSGame())
            {
                var open = awaySchedule.FindNextOpenWeek(week);
                awaySchedule.MoveFcsGame(week, open);
                return game.AssignGame(schedules, week);
            }
            else if (awaySchedule[week] == null && homeSchedule[week] != null && homeSchedule[week].IsFCSGame())
            {
                var open = homeSchedule.FindNextOpenWeek(week);
                homeSchedule.MoveFcsGame(week, open);
                return game.AssignGame(schedules, week);
            }
            else if (homeSchedule[week] != null && homeSchedule[week].IsFCSGame() && awaySchedule[week] != null && awaySchedule[week].IsFCSGame())
            {
                var open = awaySchedule.FindNextOpenWeek(week);
                awaySchedule.MoveFcsGame(week, open);

                open = homeSchedule.FindNextOpenWeek(week);
                homeSchedule.MoveFcsGame(week, open);
                return game.AssignGame(schedules, week);
            }
            /*            else
                        {
                            // find the next common opening 
                            var awayOpen = awaySchedule.FindOpenWeeks();
                            var homeOpen = homeSchedule.FindOpenWeeks();
                            int openWeek;

                            if( FindCommonOpenWeek(awayOpen,homeOpen, out openWeek))
                            {
                                game.AssignGame()
                            }
                        }
                        */
            return false;
        }

        public static bool FindCommonOpenWeek(List<int> a, List<int> b, out int week) => FindCommonOpenWeek(a, b, false, out week);

        public static bool FindCommonOpenWeek(List<int> a, List<int> b, bool findLaterGames, out int week)
        {
            try
            {
                var intesection = a.Intersect(b);
                intesection = findLaterGames ? intesection.OrderByDescending(x => x) : intesection.OrderBy(x => x);

                var common = intesection.ToArray();

                if (common.Length >= 2 && common[0] == 14)
                {
                    week = common[1];
                    return true;
                }

                if (common.Length > 0)
                {
                    week = common[0];
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            week = -1;
            return false;
        }

        public static bool AssignGame(this PreseasonScheduledGame game, Dictionary<int, TeamSchedule> schedules)
        {
            var week = game.LockedWeek.Value % 15;
            return game.AssignGame(schedules, week);
        }

        public static long GetKey(this PreseasonScheduledGame game)
        {
            long first = 0;
            long second = 0;

            if (game.HomeTeam < game.AwayTeam)
            {
                first = game.HomeTeam;
                second = game.AwayTeam;
            }
            else
            {
                first = game.AwayTeam;
                second = game.HomeTeam;
            }

            return (((long)first) << 32) | second;
        }
    }
}
