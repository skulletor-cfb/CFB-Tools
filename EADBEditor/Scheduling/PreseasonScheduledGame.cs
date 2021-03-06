﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    #region Conference Locks
    public abstract class ConferenceLocks
    {
        protected static int? MatchTeams(int lockedWeek, PreseasonScheduledGame game, params int[] teams)
        {
            return teams.Contains(game.HomeTeam) && teams.Contains(game.AwayTeam) ? lockedWeek : (int?)null;
        }

        public virtual int? CheckWeekLock(PreseasonScheduledGame game)
        {
            return LockChecks.Select(f => f(game)).Where(lw => lw.HasValue).SingleOrDefault();
        }

        protected abstract Func<PreseasonScheduledGame, int?>[] LockChecks { get; }
    }

    public class Big10Locks : ConferenceLocks
    {
        private Func<PreseasonScheduledGame, int?>[] lockChecks;
        protected override Func<PreseasonScheduledGame, int?>[] LockChecks
        {
            get
            {
                if (lockChecks == null)
                {
                    lockChecks = new Func<PreseasonScheduledGame, int?>[]
                    {
                        IsIllOSU,
                        IsIllNU,
                        IsPurdueIU,
                        IsUMOSU,
                        IsUMPSU,
                        IsPSUMSU,
                        IsUMMSU,
                        IsOSUMSU,
                        IsIowaMinn,
                        IsPSUOSU,
                        IsPSURU,
                        IsWiscMinn,
                        IsMichMinn,
                    };
                }

                return lockChecks;
            }
        }
        public int? IsWiscMinn(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 114, 54);
        }
        public int? IsPSURU(PreseasonScheduledGame game)
        {
            return MatchTeams(12, game, 76, 80);
        }
        public int? IsPSUOSU(PreseasonScheduledGame game)
        {
            return MatchTeams(6, game, 76, 70);
        }

        public int? IsIowaMinn(PreseasonScheduledGame game)
        {
            return MatchTeams(5, game, 37, 54);
        }

        public int? IsMichMinn(PreseasonScheduledGame game)
        {
            return MatchTeams(10, game, 51, 54);
        }

        public int? IsOSUMSU(PreseasonScheduledGame game)
        {
            return MatchTeams(12, game, 70, 52);
        }
        public int? IsPSUMSU(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 76, 52);
        }

        public int? IsUMMSU(PreseasonScheduledGame game)
        {
            return MatchTeams(9, game, 51, 52);
        }

        public int? IsUMPSU(PreseasonScheduledGame game)
        {
            return MatchTeams(11, game, 51, 76);
        }

        public int? IsUMOSU(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 51, 70);
        }

        public int? IsPurdueIU(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 36, 78);
        }

        public int? IsIllNU(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 35, 67);
        }


        public int? IsIllOSU(PreseasonScheduledGame game)
        {
            return MatchTeams(8, game, 35, 70);
        }

    }

    public class Big12Locks : ConferenceLocks
    {
        static int Is10TeamBig12Modifier { get { return RecruitingFixup.Big12.Length == 10 ? 1 : 0; } }
        private Func<PreseasonScheduledGame, int?>[] lockChecks;
        protected override Func<PreseasonScheduledGame, int?>[] LockChecks
        {
            get
            {
                if (lockChecks == null)
                {
                    lockChecks = new Func<PreseasonScheduledGame, int?>[]
                    {
                        IsOSUOU,
                        IsNUOU,
                        IsNUCU,
                        IsBSUCU,
                        IsBSUTCU,
                        IsBaylorTCU,
                        IsISUKSU,
                        IsKUKSU,
                        IsTexasTT,
                        IsTexasOU,
                        g => MatchTeams(13,g,81,16), //sdsu - byu in week 13
                        g => MatchTeams(13,g,112,44), //WVU-UL in week 13
                        g => MatchTeams(7,g,11,94), //BU-TT in week 7
                        IsTexasSMU,
                        g=> MatchTeams(4, g, 83,89), //TCU-SMU doesn't move
                    };
                }

                return lockChecks;
            }
        }

        public int? IsTexasSMU(PreseasonScheduledGame game)
        {
            if (game.HomeTeam == 83 && game.AwayTeam == 92)
            {
                return 8;
            }

            return null;
        }

        public int? IsTexasOU(PreseasonScheduledGame game)
        {
            return MatchTeams(game.WeekIndex, game, 92, 71);
        }

        public int? IsTexasTT(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 94, 92);
        }


        public int? IsKUKSU(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 39, 40);
        }

        public int? IsISUKSU(PreseasonScheduledGame game)
        {
            return MatchTeams(12 + Is10TeamBig12Modifier, game, 38, 40);
        }


        public int? IsBaylorTCU(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 89, 11);
        }

        public int? IsBSUTCU(PreseasonScheduledGame game)
        {
            return MatchTeams(7, game, 89, 12);
        }

        public int? IsBSUCU(PreseasonScheduledGame game)
        {
            return MatchTeams(11, game, 22, 12);
        }

        public int? IsNUCU(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 58, 22);
        }

        public int? IsNUOU(PreseasonScheduledGame game)
        {
            return MatchTeams(12 + Is10TeamBig12Modifier, game, 58, 71);
        }

        public int? IsOSUOU(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 72, 71);
        }

    }

    public class AmericanLocks : ConferenceLocks
    {
        //static int Is10TeamConf { get { return RecruitingFixup.American.Length == 10 ? 1 : 0; } }
        static int Is10TeamConf => 0;

        private Func<PreseasonScheduledGame, int?>[] lockChecks;
        protected override Func<PreseasonScheduledGame, int?>[] LockChecks
        {
            get
            {
                if (lockChecks == null)
                {
                    lockChecks = new Func<PreseasonScheduledGame, int?>[]
                    {
                        g=>MatchTeams(13+Is10TeamConf, g, 144, 18),
                        g=>MatchTeams(13+Is10TeamConf, g, 20, 90),
                        g=>MatchTeams(13+Is10TeamConf, g, 25, 46),
                        g=>MatchTeams(13+Is10TeamConf, g, 33, 97),
                        g=>MatchTeams(13+Is10TeamConf, g, 83, 79),
                        g=>MatchTeams(13+Is10TeamConf, g, 48, 96),
                        g=>MatchTeams(12+Is10TeamConf, g, 33, 83),
                        g=>MatchTeams(8, g, 33, 79),
                        g=>MatchTeams(7, g, 18, 25),
                    };
                }

                return lockChecks;
            }
        }
    }

    public class Pac12Locks : ConferenceLocks
    {
        public override int? CheckWeekLock(PreseasonScheduledGame game)
        {
            return base.CheckWeekLock(game);
        }

        private Func<PreseasonScheduledGame, int?>[] lockChecks;
        protected override Func<PreseasonScheduledGame, int?>[] LockChecks
        {
            get
            {
                if (lockChecks == null)
                {
                    lockChecks = new Func<PreseasonScheduledGame, int?>[]
                    {
                        g=>MatchTeams(13, g, 110, 111),
                      g=>  MatchTeams(13, g, 103, 16),
                        g=>MatchTeams(13, g, 74, 75),
                        g=>MatchTeams(13, g, 4, 5),
                        IsUscUCLA,
                        IsStanfordCal,
                        g => MatchTeams(13,g,22,103),
                        g => MatchTeams(g.Week,g,111,75),
                        g => MatchTeams(g.Week,g,110,74),
                        g => MatchTeams(g.Week,g,110,75),
                        g => MatchTeams(g.Week,g,111,74),
                        g => MatchTeams(g.Week,g,99,17),
                        g => MatchTeams(g.Week,g,102,17),
                    };
                }

                return lockChecks;
            }
        }
        public int? IsUscUCLA(PreseasonScheduledGame game)
        {
            var week = Form1.IsEvenYear.Value ? 13 : 12;
            return MatchTeams(week, game, 99, 102);
        }

        public int? IsStanfordCal(PreseasonScheduledGame game)
        {
            var week = Form1.IsEvenYear.Value ? 12 : 13;
            return MatchTeams(week, game, 17, 87);
        }
    }

    public class SunBeltLocks : ConferenceLocks
    {
        private Func<PreseasonScheduledGame, int?>[] lockChecks;

        protected override Func<PreseasonScheduledGame, int?>[] LockChecks => lockChecks ?? (lockChecks = CreateChecks());

        Func<PreseasonScheduledGame, int?>[] CreateChecks()
        {
            return new Func<PreseasonScheduledGame, int?>[]
            {
                game=> MatchTeams(13,game,8,57), //army-navy
                game=> MatchTeams(13,game,85,98), //uab-usm
                game=> MatchTeams(13,game,64,7), //nt-ark st
                game=> MatchTeams(13,game,232,218), //tex st-utsa
                game=> MatchTeams(13,game,65,86), //ull-ulm
                game=> MatchTeams(9,game,43,86), //lt-ull
                game=> MatchTeams(12,game,43,65), //lt-ulm
                game=> MatchTeams(7,game,43,85), //lt-usm
                game=> MatchTeams(7,game,65,7), //ulm-ark st
//                game=> MatchTeams(13,game,211,53), //wku-mtsu
 //               game=> MatchTeams(13,game,85,98), //usm-uab
   //             game=> MatchTeams(8,game,53,64), //mtsu-nt
            };
        }
    }
    public class MWCLocks : ConferenceLocks
    {
        static int Is10TeamConf { get { return RecruitingFixup.MWC.Length < 12 ? 0 : -1; } }
        private Func<PreseasonScheduledGame, int?>[] lockChecks;

        protected override Func<PreseasonScheduledGame, int?>[] LockChecks => lockChecks ?? (lockChecks = CreateChecks());

        Func<PreseasonScheduledGame, int?>[] CreateChecks()
        {
            return new Func<PreseasonScheduledGame, int?>[]
            {
//                game=> MatchTeams(game.WeekIndex,game,61,60), //nmsu-unm
                game=> MatchTeams(game.WeekIndex,game,59,101), //nev-unlv
                game=> MatchTeams(game.WeekIndex,game,1,23), //af-csu
                game=> MatchTeams(game.WeekIndex,game,115,23), //wyoming-csu
//                game=> MatchTeams(13,game,29,82), //SJSu-fs
            };
        }
    }


    public class CUSALocks : ConferenceLocks
    {

        private Func<PreseasonScheduledGame, int?>[] lockChecks;

        protected override Func<PreseasonScheduledGame, int?>[] LockChecks => lockChecks ?? (lockChecks = CreateChecks());

        Func<PreseasonScheduledGame, int?>[] CreateChecks()
        {
            return new Func<PreseasonScheduledGame, int?>[]
            {
                game=> MatchTeams(6,game,34,181), //app st-gaso
                game=> MatchTeams(13,game,233,181), //ga st-gaso
                game=> MatchTeams(13,game,100,234), //clt-odu
                game=> MatchTeams(13,game,34,61), //appst-ccu

                game=> MatchTeams(13,game,229,230), //fiu-fau
                game=> MatchTeams(13,game,211,53), //wku-mtsu
                game=> MatchTeams(13,game,235,143), //troy-usa

                game=> MatchTeams(8,game,53,143), //mtsu-troy
                game=> MatchTeams(6,game,229,143), //troy-fau
                game=> MatchTeams(7,game,901,234), //appst-odu
                game=> MatchTeams(7,game,61,100), //ccu-clt
                game=> MatchTeams(8,game,61,181), //ccu-gaso
            };
        }
    }

    public class AccLocks : ConferenceLocks
    {
        private Func<PreseasonScheduledGame, int?>[] lockChecks;
        protected override Func<PreseasonScheduledGame, int?>[] LockChecks
        {
            get
            {
                if (lockChecks == null)
                {
                    lockChecks = new Func<PreseasonScheduledGame, int?>[]
                    {
                        IsFSUatMiami,
                        IsMiamiatFSU,
                        IsMiamiBC,
                        IsMiamiVT,
                        IsBCVT,
                        IsSyracusePitt,
                        IsClemsonFSU,
                        IsUMDWVU,
                        IsFSUUMD,
                        IsNCSUWake,
                        IsUNCUVA,
                        IsUNCDuke,
                        IsPittWVU,
                        IsVTUVA,
                        IsUNCNCSU
                    };
                }

                return lockChecks;
            }
        }

        public int? IsFSUatMiami(PreseasonScheduledGame game)
        {
            return game.HomeTeam == 49 && game.AwayTeam == 28 ? 6 : default(int?);
        }

        public int? IsMiamiatFSU(PreseasonScheduledGame game)
        {
            //return game.HomeTeam == 28 && game.AwayTeam == 49 ? 9 : default(int?);
            return game.HomeTeam == 28 && game.AwayTeam == 49 ? 6 : default(int?);
        }

        public int? IsMiamiBC(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 49, 13);
        }

        public int? IsMiamiVT(PreseasonScheduledGame game)
        {
            return MatchTeams(7, game, 49, 108);
        }

        public int? IsBCVT(PreseasonScheduledGame game)
        {
            return MatchTeams(12, game, 13, 108);
        }

        public int? IsSyracusePitt(PreseasonScheduledGame game)
        {
            return MatchTeams(12, game, 88, 77);
        }

        public int? IsClemsonFSU(PreseasonScheduledGame game)
        {
            return MatchTeams(12, game, 21, 28);
        }

        public int? IsUMDWVU(PreseasonScheduledGame game)
        {
            return MatchTeams(3, game, 47, 112);
        }

        public int? IsFSUUMD(PreseasonScheduledGame game)
        {
            return MatchTeams(5, game, 47, 28);
        }

        public int? IsNCSUWake(PreseasonScheduledGame game)
        {
            return MatchTeams(8, game, 63, 109);
        }

        public int? IsUNCUVA(PreseasonScheduledGame game)
        {
            return MatchTeams(12, game, 62, 107);
        }

        public int? IsUNCDuke(PreseasonScheduledGame game)
        {
            return MatchTeams(8, game, 62, 24);
        }

        public int? IsPittWVU(PreseasonScheduledGame game)
        {
            var value = MatchTeams(13, game, 77, 112);
            return value;
        }

        public int? IsVTUVA(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 107, 108);
        }

        public int? IsUNCNCSU(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 62, 63);
        }

    }
    #endregion

    public class SecLocks : ConferenceLocks
    {
        private Func<PreseasonScheduledGame, int?>[] lockChecks;
        protected override Func<PreseasonScheduledGame, int?>[] LockChecks
        {
            get
            {
                if (lockChecks == null)
                {
                    lockChecks = new Func<PreseasonScheduledGame, int?>[]
                    {
                        IsSecConfGame
                    };
                }

                return lockChecks;
            }
        }

        public int? IsTennVandyGame(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 106, 91);
        }


        public int? IsSecConfGame(PreseasonScheduledGame game)
        {
            var isUkTenn = MatchTeams(1000, game, 91, 42);
            if (isUkTenn.HasValue)
            {
                return 0;
            }

            var isTennvandy = IsTennVandyGame(game);

            return isTennvandy.HasValue ? isTennvandy.Value : game.WeekIndex;
        }
    }

    public static class ConfScheduleFixer
    {
        public static void ExtraConfGameSwap(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            // give me all extra games for a given week
            var conf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.MustReplace)).Distinct().GroupBy(g => g.WeekIndex).ToDictionary(g => g.Key, g => g.ToArray());

            foreach (var week in conf.Keys)
            {
                var games = conf[week];

                for (int i = 0; i < games.Length; i++)
                {
                    var game = games[i];

                    if (!game.MustReplace)
                        continue;

                    for (int j = i + 1; j < games.Length; j++)
                    {
                        var swapee = games[j];

                        if (swapee.MustReplace && RecruitingFixup.TeamAndConferences[game.HomeTeam] != RecruitingFixup.TeamAndConferences[swapee.HomeTeam])
                        {
                            ScheduleFixup.SwapTeams(game, swapee);
                            schedules[game.AwayTeam][game.WeekIndex] = game;
                            schedules[game.HomeTeam][game.WeekIndex] = game;
                            schedules[swapee.HomeTeam][swapee.WeekIndex] = swapee;
                            schedules[swapee.AwayTeam][swapee.WeekIndex] = swapee;
                            swapee.MustReplace = false;
                            game.MustReplace = false;
                            break;
                        }
                    }
                }
            }
        }

        public static void MoveReplaceableGames(Dictionary<int, PreseasonScheduledGame[]> schedules, Func<PreseasonScheduledGame, bool> confGameQualifier)
        {
            // give me all extra games
            var conf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.MustReplace && confGameQualifier(g))).Distinct().ToList();

            foreach (var game in conf)
            {
                if (game.Week < 6)
                    continue;

                var homeOpenings = FindOpenWeeks(schedules[game.HomeTeam]);
                var awayOpenings = game.AwayTeam.IsFcsTeam() ? FcsOpenings() : FindOpenWeeks(schedules[game.AwayTeam]);
                int week = 0;

                if (FindCommonOpenWeek(awayOpenings, homeOpenings, out week))
                {
                    AssignGame(game, schedules, week);
                }
            }
        }

        /// <summary>
        /// put fcs games the earliest possible
        /// </summary>
        /// <param name="schedules"></param>
        public static void FcsGamesEarly(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            var fcsGames = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsFCSGame()));

            foreach (var game in fcsGames)
            {
                var team = game.HomeTeam.IsFcsTeam() ? game.AwayTeam : game.HomeTeam;
                var currentGameWeek = game.WeekIndex;
                var teamSchedule = schedules[team];
                var openWeeks = teamSchedule.FindOpenWeeks();
                var week = team.IsSECTeam() ? openWeeks.Last() : openWeeks.First();

                game.SetWeek(week);
                teamSchedule[week] = game;
                teamSchedule[currentGameWeek] = null;
            }
        }

        public static void ReplaceFcsOnlyGames(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            // find the games with fcs home team
            var fcsGames = schedules.Values.SelectMany(games => games.Where(g => g != null && g.HomeTeam.IsFcsTeam() && g.AwayTeam.IsFcsTeam())).Distinct().ToArray();

            if(fcsGames.Length == 0)
            {
                return;
            }

            var stack = new Stack<PreseasonScheduledGame>(fcsGames);
            var oocGames = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame()/* && g.IsP5Game()*/)).OrderByDescending(g => g.WeekIndex).Distinct().ToArray();

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

        static List<int> fcsOpenings = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

        static List<int> FcsOpenings() => fcsOpenings;

        public static void ExtraConfGameSwap(Dictionary<int, PreseasonScheduledGame[]> schedules, Func<PreseasonScheduledGame, bool> g5Qualifier, Func<PreseasonScheduledGame, bool> confGameQualifier)
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


        public static void G5FCSSwap(Dictionary<int, PreseasonScheduledGame[]> schedules, bool anyP5Game = false)
        {
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
                gamesToReplace = schedules.Values.SelectMany(games => games.Where(g => g != null && g.MustReplace)).GroupBy(g => g.WeekIndex).ToDictionary(g => g.Key, g => g.Distinct().ToList());
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

                    var open = FindNextOpenWeek(schedules[game.HomeTeam], game.WeekIndex);
                    MoveFcsGame(schedules[game.HomeTeam], game.WeekIndex, open);

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

        public static void Fix(Dictionary<int, PreseasonScheduledGame[]> schedules, ConferenceLocks confLocks, int confId, Action<Dictionary<int, PreseasonScheduledGame[]>> special = null)
        {
            var teams = RecruitingFixup.TeamAndConferences.Where(kvp => kvp.Value == confId).Select(kvp => kvp.Key).ToArray();
            var allConfGames = new Dictionary<long, PreseasonScheduledGame>();

            // remove all games and put them into a dictionary
            foreach (var team in teams)
            {
                var schedule = schedules[team];

                for (int i = 0; i < schedule.Length; i++)
                {
                    if (schedule[i] == null)
                        continue;

                    var currentGame = schedule[i];

                    // now see if the game is already in the right week
                    currentGame.CheckForLock(confLocks);

                    if (currentGame.IsConferenceGame() && !currentGame.IsExtraAccGame() && !currentGame.MustReplace)
                    {
                        if (!allConfGames.ContainsKey(currentGame.GetKey()))
                        {
                            allConfGames.Add(currentGame.GetKey(), currentGame);
                        }

                        schedule[i] = null;
                    }

                    if (currentGame.LockedWeek.HasValue && currentGame.LockedWeek.Value == i)
                    {
                        allConfGames.Remove(currentGame.GetKey());
                        schedule[i] = currentGame;
                        continue;
                    }
                    else if (currentGame.WeekIndex > 5 && !currentGame.IsConferenceGame() && !currentGame.IsRivalryGame())
                    {
                        var limit = currentGame.WeekIndex;
                        // this is easy, just move it to the earliest spot
                        if (currentGame.IsFCSGame())
                        {
                            for (int j = 0; j < limit; j++)
                            {
                                if (schedule[j] == null)
                                {
                                    currentGame.SetWeek(j);
                                    schedule[j] = currentGame;
                                    schedule[i] = null;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // move it to the first available spot
                            var opponent = currentGame.OpponentId(team);

                            if (schedules.TryGetValue(opponent, out var oppSchedule))
                            {
                                for (int j = 0; j < limit; j++)
                                {
                                    if (schedule[j] == null && oppSchedule[j] == null)
                                    {
                                        currentGame.SetWeek(j);
                                        schedule[j] = currentGame;
                                        oppSchedule[j] = currentGame;
                                        oppSchedule[i] = null;
                                        schedule[i] = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (special != null)
            {
                special(schedules);
            }

            // add in all the locked games first
            var lockedGames = allConfGames.Values.Where(g => g.LockedWeek.HasValue).ToArray();
            foreach (var game in lockedGames)
            {
                if (AssignGame(game, schedules))
                {
                    allConfGames.Remove(game.GetKey());
                }
            }

            var gamesLeft = allConfGames.Values.ToList();
            List<PreseasonScheduledGame> filled = new List<PreseasonScheduledGame>();

            while (gamesLeft.Count > 0)
            {
                // get a game
                var index = PlayerHelper.Rand() % gamesLeft.Count;
                var game = gamesLeft[index];
                gamesLeft.RemoveAt(index);
                bool success = false;

                // find the latest available week for both teams
                for (int i = 13; i >= 0; i--)
                {
                    if (game.AssignGame(schedules, i, true))
                    {
                        filled.Add(game);
                        success = true;
                        break;
                    }
                }

                if (!success && game.AssignGame(schedules, 14))
                {
                    filled.Add(game);
                    success = true;
                }

                if (success)
                {
                    allConfGames.Remove(game.GetKey());
                }
                else
                {
                    foreach (var filledGame in filled)
                    {
                        schedules[filledGame.AwayTeam][filledGame.WeekIndex] = null;
                        schedules[filledGame.HomeTeam][filledGame.WeekIndex] = null;
                    }

                    gamesLeft.AddRange(filled);
                    gamesLeft.Add(game);
                    filled.Clear();
                }
            }
        }

        public static void SunBeltFix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new SunBeltLocks(), RecruitingFixup.SBCId);
        }

        public static void MWCFix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new MWCLocks(), RecruitingFixup.MWCId);
        }

        public static void CUSAFix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new CUSALocks(), RecruitingFixup.CUSAId);

            /*
            var dict = new Dictionary<int, int[]>()
            {
                { 7, new[]{86,65,232,98} },
                {43 , new[]{7,86,218,98} },
                {64 , new[]{7,43,86,218} },
                {86 , new[]{65,232,98,85} },
                { 65, new[]{43,64,218,85} },
                {232 , new[]{43,64,65,98} },
                { 218, new[]{7,86,232,85} },
                {98 , new[]{64,65,218,85} },
                {85 , new[]{7,43,64,232} },
            };

            SetHomeTeamForRoundRobin(schedules, dict);*/
        }

        public static void SetHomeTeamForRoundRobin(Dictionary<int, PreseasonScheduledGame[]> schedules, Dictionary<int, int[]> homeGames)
        {
            foreach (var game in schedules.Values.SelectMany(s => s.Where(g => g != null && g.IsConferenceGame() && homeGames.ContainsKey(g.HomeTeam))))
            {
                game.SetHomeTeam(homeGames);
            }
        }

        public static void AmericanFix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new AmericanLocks(), RecruitingFixup.AmericanId);
        }

        public static void Big10Fix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new Big10Locks(), RecruitingFixup.Big10Id);
        }

        public static void Big12Fix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new Big12Locks(), RecruitingFixup.Big12Id);
        }

        public static void SecFix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new SecLocks(), RecruitingFixup.SECId);
        }

        public static void AccFix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            var accLocks = new AccLocks();

            Fix(
                schedules,
                accLocks,
                RecruitingFixup.ACCId,
                s =>
                {
                    // find UL/UK game and set it to week 13
                    var game = s[44].Where(g => g != null && g.OpponentId(44) == 42).FirstOrDefault();
                    if (game != null)
                    {
                        game.AssignGame(s, 13);
                    }
                });
        }

        public static void Pac12Fix(Dictionary<int, PreseasonScheduledGame[]> schedules)
        {
            Fix(schedules, new Pac12Locks(), RecruitingFixup.Pac16Id);
        }

        private static HashSet<PreseasonScheduledGame> RejectedOnce = new HashSet<PreseasonScheduledGame>();

        public static bool AssignGame(this PreseasonScheduledGame game, Dictionary<int, PreseasonScheduledGame[]> schedules, int week, bool postFix = false)
        {
            var homeSchedule = schedules[game.HomeTeam];
            var awaySchedule = game.AwayTeam.IsFcsTeam() ? schedules[homeSchedule.Length] : schedules[game.AwayTeam];

            // we don't want 3 or 4 away games in a row, try to mitigate that
            if (postFix && !RejectedOnce.Contains(game) && game.IsConferenceGame())
            {
                var awayTeam = game.AwayTeam;
                var leftAwayGames = 0;
                var rightAwayGames = 0;

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

                    var conf = RecruitingFixup.TeamAndConferences[awayTeam];
                    var findLate = conf == RecruitingFixup.Pac16Id || conf == RecruitingFixup.Big10Id || conf == RecruitingFixup.MACId || conf == RecruitingFixup.SBCId || conf == RecruitingFixup.CUSAId || conf == RecruitingFixup.AmericanId;

                    var finalTry = FindCommonOpenWeek(homeSchedule.FindOpenWeeks(week), awaySchedule.FindOpenWeeks(week), findLate, out var nextOpen) ? nextOpen : week;
                    return AssignGame(game, schedules, finalTry);
                }
            }

            if (awaySchedule[week] == null && homeSchedule[week] == null)
            {
                game.SetWeek(week);
                homeSchedule[week] = game;
                awaySchedule[week] = game;
                return true;
            }

            // suppose one or both teams have a fcs game that week, we can move it easily
            if (homeSchedule[week] == null && awaySchedule[week] != null && awaySchedule[week].IsFCSGame())
            {
                var open = FindNextOpenWeek(awaySchedule, week);
                MoveFcsGame(awaySchedule, week, open);
                return game.AssignGame(schedules, week);
            }
            else if (awaySchedule[week] == null && homeSchedule[week] != null && homeSchedule[week].IsFCSGame())
            {
                var open = FindNextOpenWeek(homeSchedule, week);
                MoveFcsGame(homeSchedule, week, open);
                return game.AssignGame(schedules, week);
            }
            else if (homeSchedule[week] != null && homeSchedule[week].IsFCSGame() && awaySchedule[week] != null && awaySchedule[week].IsFCSGame())
            {
                var open = FindNextOpenWeek(awaySchedule, week);
                MoveFcsGame(awaySchedule, week, open);

                open = FindNextOpenWeek(homeSchedule, week);
                MoveFcsGame(homeSchedule, week, open);
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

        public static List<int> FindOpenWeeks(this PreseasonScheduledGame[] games, int? butNot = null)
        {
            var list = new List<int>();

            for (int i = 0; i < games.Length; i++)
            {
                if (games[i] == null)
                {
                    if (butNot == null || butNot.Value != i)
                        list.Add(i);
                }
            }

            return list;
        }

        public static void MoveFcsGame(PreseasonScheduledGame[] schedule, int fromWeek, int toWeek)
        {
            schedule[fromWeek].SetWeek(toWeek);
            schedule[toWeek] = schedule[fromWeek];
            schedule[fromWeek] = null;
        }

        public static int FindNextOpenWeek(PreseasonScheduledGame[] schedule, int notThisWeek)
        {

            for (int i = notThisWeek + 1; i < schedule.Length; i++)
            {
                if (schedule[i] == null)
                    return i;
            }

            for (int i = 0; i < notThisWeek; i++)
            {
                if (schedule[i] == null)
                    return i;
            }

            return -1;
        }

        public static bool AssignGame(this PreseasonScheduledGame game, Dictionary<int, PreseasonScheduledGame[]> schedules)
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


    public class PreseasonScheduledGame
    {
        public int HomeTeam { get; set; }
        public int AwayTeam { get; set; }
        public int Week { get { return WeekIndex + 1; } }
        public int WeekIndex { get; set; }
        public int GameNumber { get; set; }
        public MaddenRecord MaddenRecord { get; set; }

        private bool mustReplace = false;

        public bool MustReplace
        {
            get
            {
                return mustReplace;
            }

            set
            {
                mustReplace = value;
            }
        }

        public int? LockedWeek { get; private set; }

        public void CheckForLock(ConferenceLocks locks)
        {
            this.LockedWeek = locks.CheckWeekLock(this);
        }

        public void SetWeek(int week)
        {
            this.WeekIndex = week;
            MaddenRecord["SEWN"] = week.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0}-{1} :  {2} at {3}", WeekIndex, GameNumber, AwayTeam, HomeTeam);
        }

        public bool IsConferenceGame()
        {
            return RecruitingFixup.TeamAndConferences[AwayTeam] == RecruitingFixup.TeamAndConferences[HomeTeam] &&
                RecruitingFixup.TeamAndConferences[HomeTeam] != 17;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PreseasonScheduledGame;

            return other != null && other.Week == this.Week && other.GameNumber == this.GameNumber;
        }

        public override int GetHashCode()
        {
            return (this.Week << 14) | this.GameNumber;
        }

        public bool IsFCSGame()
        {
            return !IsConferenceGame() && AwayTeam.IsFcsTeam();
        }

        public bool IsP5FCSGame()
        {
            return HomeTeam.IsP5() && HomeTeam != 68 && AwayTeam.IsFcsTeam();
        }

        public bool IsP5Game()
        {
            return !IsConferenceGame() && AwayTeam.IsP5OrND() && HomeTeam.IsP5OrND();
        }

        public bool IsP5GameAnyOpponent()
        {
            return !IsConferenceGame() && (AwayTeam.IsP5() || HomeTeam.IsP5());
        }

        public bool IsRivalryGame()
        {
            return ScheduleFixup.IsRivalryGame(HomeTeam, AwayTeam);
        }

        public bool IsG5Game()
        {
            return !IsConferenceGame() && !AwayTeam.IsP5OrND() && !HomeTeam.IsP5OrND();
        }

        public bool IsAmericanGame()
        {
            return AwayTeam.IsAmericanTeam() || HomeTeam.IsAmericanTeam();
        }

        public bool IsServiceAcademyGame()
        {
            return ScheduleFixup.IsServiceAcademyGame(HomeTeam, AwayTeam);
        }

        public bool IsAccOrSecGame()
        {
            return this.HomeTeam.IsSECTeam() || this.HomeTeam.IsAccTeam() || this.AwayTeam.IsSECTeam() || this.AwayTeam.IsAccTeam();
        }

        public int OpponentId(int teamId)
        {
            if (HomeTeam == teamId)
                return AwayTeam;

            return HomeTeam;
        }

        public string Opponent(int teamId)
        {
            string team = null;
            if (HomeTeam == teamId)
                team = "vs " + RecruitingFixup.TeamAbbreviations[AwayTeam];
            if (AwayTeam == teamId)
                team = "at " + RecruitingFixup.TeamAbbreviations[HomeTeam];

            if (AwayTeam.IsFcsTeam())
            {
                team = team.ToLower();
            }

            if (this.IsG5Game())
            {
                team = team.Replace("at", "G5-at").Replace("vs", "G5-vs");
            }

            // team = string.Format("({0})  {1}", this.WeekIndex, team);

            if (team != null)
                return IsConferenceGame() ? team.ToLower() : team.ToUpper();

            throw new InvalidOperationException();
        }

        public bool IsNotreDameGame()
        {
            return HomeTeam == 68 || AwayTeam == 68;
        }

        public bool IsExtraPac12Game()
        {
            return Pac12Schedule.Pac12ConferenceSchedule != null &&
                Pac12Schedule.Pac12ConferenceSchedule.ContainsKey(this.HomeTeam) &&
                Pac12Schedule.Pac12ConferenceSchedule.ContainsKey(this.AwayTeam) &&
                !Pac12Schedule.Pac12ConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam);
        }

        public bool IsExtraBig12Game()
        {
            return Big12Schedule.Big12ConferenceSchedule != null &&
                Big12Schedule.Big12ConferenceSchedule.ContainsKey(this.HomeTeam) &&
                Big12Schedule.Big12ConferenceSchedule.ContainsKey(this.AwayTeam) &&
                !Big12Schedule.Big12ConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam);
        }

        public bool IsExtraAccGame()
        {
            if (ACCPodSchedule.ACCConferenceSchedule != null &&
                ACCPodSchedule.ACCConferenceSchedule.ContainsKey(this.HomeTeam) &&
                ACCPodSchedule.ACCConferenceSchedule.ContainsKey(this.AwayTeam) &&
                RecruitingFixup.AccTeams == 16)
            {
                return !ACCPodSchedule.ACCConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam);
            }
            else if (RecruitingFixup.AccTeams == 14 &&
                Acc14OpponentDict.ContainsKey(this.HomeTeam) &&
                Acc14OpponentDict.ContainsKey(this.AwayTeam))
            {
                // 14th row of CNFR table for each conference controls 8 or 9 game schedule, but the game fails to schedule properly
                //return false;             
                return !Acc14OpponentDict[this.HomeTeam].Contains(this.AwayTeam);
            }

            return false;
        }

        public bool ShouldFixAccGame()
        {
            if (RecruitingFixup.AccTeams == 14)
            {
                var isEvenYear = Form1.IsEvenYear.Value;

                if (isEvenYear)
                {
                    // it's an even year
                    return !AccEvenYearHosting[this.HomeTeam].Contains(this.AwayTeam);
                }
                else
                {
                    // it's an odd year
                    return AccEvenYearHosting[this.HomeTeam].Contains(this.AwayTeam);
                }
            }
            else
            {
                var isOddYear = !Form1.IsEvenYear.Value;

                if (isOddYear)
                {
                    return !ACCPodSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
                }
                else
                {
                    return ACCPodSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
                }
            }
        }

        public static Dictionary<int, int[]> AccEvenYearHosting = CreateAccDict();

        public static Dictionary<int, int[]> CreateAccDict()
        {
            var dict = new Dictionary<int, int[]>();

#if true
            // atlantic
            dict.Add(47, new[] { 31, 21, 109, 107 });
            dict.Add(21, new[] { 28, 77, 109, 13 });
            dict.Add(28, new[] { 31, 47, 109, 49 });
            dict.Add(31, new[] { 63, 21, 77, 108 });
            dict.Add(63, new[] { 47, 21, 28, 62 });
            dict.Add(77, new[] { 47, 28, 63, 88 });
            dict.Add(109, new[] { 31, 63, 77, 24 });

            // coastal
            dict.Add(108, new[] { 13, 107, 62, 21, 28, 47, 63, 77, 109 });
            dict.Add(13, new[] { 49, 107, 24, 47, 28, 31, 63, 77, 109 });
            dict.Add(49, new[] { 108, 88, 62, 21, 47, 31, 63, 77, 109 });
            dict.Add(107, new[] { 88, 49, 24, 21, 28, 31, 63, 77, 109 });
            dict.Add(62, new[] { 107, 13, 24, 21, 28, 31, 47, 77, 109 });
            dict.Add(88, new[] { 13, 108, 62, 21, 28, 31, 63, 47, 109 });
            dict.Add(24, new[] { 49, 88, 108, 21, 28, 31, 63, 77, 47 });
#else
            // atlantic
            dict.Add(47, new[] { 13, 21, 109, 107 });
            dict.Add(21, new[] { 28, 88, 109, 31 });
            dict.Add(28, new[] { 13, 47, 109, 49 });
            dict.Add(13, new[] { 63, 21, 88, 108 });
            dict.Add(63, new[] { 47, 21, 28, 62 });
            dict.Add(88, new[] { 47, 28, 63, 77 });
            dict.Add(109, new[] { 13, 63, 88, 24 });

            // coastal
            dict.Add(108, new[] { 31, 107, 62, 21, 28, 47, 63, 88, 109 });
            dict.Add(31, new[] { 49, 107, 24, 47, 28, 13, 63, 88, 109 });
            dict.Add(49, new[] { 108, 77, 62, 21, 47, 13, 63, 88, 109 });
            dict.Add(107, new[] { 77, 49, 24, 21, 28, 13, 63, 88, 109 });
            dict.Add(62, new[] { 107, 31, 24, 21, 28, 13, 47, 88, 109 });
            dict.Add(77, new[] { 31, 108, 62, 21, 28, 13, 63, 47, 109 });
            dict.Add(24, new[] { 49, 77, 108, 21, 28, 13, 63, 88, 47 });
#endif
            return dict;
        }

        public static Dictionary<int, HashSet<int>> acc14Dict;
        public static Dictionary<int, HashSet<int>> Acc14OpponentDict
        {
            get
            {
                if (acc14Dict == null)
                {
                    acc14Dict = Acc14TeamCreators[(Form1.DynastyYear - 2201) % Acc14TeamCreators.Length]();
                }

                return acc14Dict;
            }
        }
        public static Func<Dictionary<int, HashSet<int>>>[] Acc14TeamCreators = new Func<Dictionary<int, HashSet<int>>>[] { CreateAccA, CreateAccA, null, null, null, null, null, null, null, null, null, null };


        public static Dictionary<int, HashSet<int>> CreateAccA()
        {
            var dict = new Dictionary<int, HashSet<int>>();
            // atlantic
            var atlantic = new[] { 47, 21, 28, 31, 63, 77, 109 }.ToList();
            var coastal = new[] { 108, 13, 49, 107, 62, 88, 24 }.ToList();
            atlantic.ForEach(team => dict.Add(team, new HashSet<int>(atlantic.Where(t => t != team))));
            coastal.ForEach(team => dict.Add(team, new HashSet<int>(coastal.Where(t => t != team))));

            dict[13].AddTeams(31, 21);
            dict[24].AddTeams(109, 28);
            dict[49].AddTeams(28, 47);
            dict[62].AddTeams(21, 63);
            dict[88].AddTeams(109, 77);
            dict[107].AddTeams(63, 47);
            dict[108].AddTeams(31, 77);

            dict[31].AddTeams(13, 108);
            dict[21].AddTeams(13, 62);
            dict[109].AddTeams(24, 88);
            dict[28].AddTeams(24, 49);
            dict[47].AddTeams(49, 107);
            dict[63].AddTeams(62, 107);
            dict[77].AddTeams(88, 108);

            return dict;
        }

    }

    public static class Ext
    {
        public static void AddTeams(this HashSet<int> set, params int[] teams)
        {
            foreach (var team in teams)
                set.Add(team);
        }
    }
}