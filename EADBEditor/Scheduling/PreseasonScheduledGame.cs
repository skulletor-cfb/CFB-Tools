using EA_DB_Editor.Scheduling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
                        IsBaylorTCU,
                        IsTexasTT,
                        IsKUKSU,
                        IsOSUOU,
                        IsNUCU,
                        IsNUOU,
                        // IsBSUCU,
                        // IsBSUTCU,
                        IsISUKSU,
                        IsTexasOU,
                        g => MatchTeams(13, g, 20, 38), // ISU-Cincy end the season when they play
                        g => MatchTeams(7,g,11,94), //BU-TT in week 7
                        g => MatchTeams(12, g, 39, 58), // neb-ku week 12
                        g => MatchTeams(13, g, 22, 72), // cu-ok st week 13
                    };
                }

                return lockChecks;
            }
        }

        public int? IsTexasOU(PreseasonScheduledGame game)
        {
            var week = game.WeekIndex;
            if (game.WeekIndex != 5 && game.WeekIndex != 6)
                week = DateTime.UtcNow.Second % 2 == 0 ? 5 : 6;

            return MatchTeams(week, game, 92, 71);
        }

        public int? IsTexasTT(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 94, 92);
        }

        public int? IsSMUHOU(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 33, 83);
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
            return MatchTeams(7, game, 58, 22);
        }

        public int? IsNUOU(PreseasonScheduledGame game)
        {
            return MatchTeams(13 + Is10TeamBig12Modifier, game, 58, 71);
        }

        public int? IsOSUOU(PreseasonScheduledGame game)
        {
            return MatchTeams(12 + Is10TeamBig12Modifier, game, 72, 71);
        }

    }

    public class AmericanLocks : ConferenceLocks
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
                        g=>MatchTeams(13, g, 8, 57), // army-navy
                        g=>MatchTeams(13, g, 64, 232), // nt-utsa
  //                      g=>MatchTeams(13, g, 33, 83), // hou-smu
                        g=>MatchTeams(13, g, 79, 97), // rice-tulsa
                        g=>MatchTeams(13, g, 25, 100), // charlotte-ecu
//                        g=>MatchTeams(13, g, 18, 144), // ucf-usf
                        g=>MatchTeams(13, g, 48, 98), // memphis-uab
  //                      g=>MatchTeams(13, g, 85, 96), // usm-tulane
    //                    g=>MatchTeams(12, g, 85, 98), // usm-uab

      //                  g=>MatchTeams(12, g, 90, 232), // utsa-temple
        //                g=>MatchTeams(6, g, 48, 85), // usm-memphis
                       // g=>MatchTeams(7, g, 18, 97), // ucf-tulsa
                       // g=>MatchTeams(6, g, 18, 25), // ucf-ecu
          //              g=>MatchTeams(7, g, 25, 85), // usm-ecu
                      //  g=>MatchTeams(6, g, 79, 83), // rice-smu
                       // g=>MatchTeams(6, g, 33, 97), // hou-tulsa
                        g=>MatchTeams(6, g, 79, 96), // tulane-rice
                       // g=>MatchTeams(8, g, 33, 79), // hou-rice
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
                game => MatchTeams( 13, game, 34,181), //gaso-app st
                game => MatchTeams(13, game, 143, 235), //usa-troy
                game=> MatchTeams(13, game, 230, 234), // odu-jmu
                game=> MatchTeams(13,game,65,86), //ull-ulm
                game=> MatchTeams(13,game,7,64), //ark st - nt



                game=> MatchTeams(7, game, 46, 234), // odu - marshall
                game=> MatchTeams(12,game,7,218), //tsu-ark st
                game => MatchTeams(7, game, 61, 181), // coastal- gaso
                game=> MatchTeams(8, game, 181, 233), // gsu-gaso
                game=> MatchTeams(8,game,34,61), //ccu-app st
                game => MatchTeams(7, game , 34, 46), // marsh-app st
                game => MatchTeams(6, game, 34, 234), // app st - odu
                game=> MatchTeams(7,game,7,65), //ulm-ark st
                game=> MatchTeams(6,game,64,232), //nt-utsa

                /*
                game=> MatchTeams(13,game,85,98), //usm-uab
                game=> MatchTeams(13,game,7,64), //nt-ark st
                game=> MatchTeams(13,game,218,232), //tsu-utsa
                game=> MatchTeams(6,game,43,86), //lt-ull
                game=> MatchTeams(7,game,43,85), //lt-usm
                game=> MatchTeams(12,game,43,65), //lt-ulm
                 */
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

    public class MACLocks : ConferenceLocks
    {
        private Func<PreseasonScheduledGame, int?>[] lockChecks;

        protected override Func<PreseasonScheduledGame, int?>[] LockChecks => lockChecks ?? (lockChecks = CreateChecks());

        Func<PreseasonScheduledGame, int?>[] CreateChecks()
        {
            var list = new List<Func<PreseasonScheduledGame, int?>>
            {
                game=> MatchTeams(13,game,50,69), //miami-ohio
                game=> MatchTeams(13,game,10,66), //ball st-niu
            };

            var seed = Guid.NewGuid().ToByteArray().First() % 3;

            switch (seed)
            {
                case 0:
                    list.Add(game => MatchTeams(13, game, 19, 113)); //cmu-wmu
                    list.Add(game => MatchTeams(13, game, 14, 41)); //bgsu-kent st
                    break;

                case 1:
                    list.Add(game => MatchTeams(13, game, 19, 26)); //cmu-emu
                    list.Add(game => MatchTeams(13, game, 2, 41)); //akron-kent st
                    break;

                case 2:
                    list.Add(game => MatchTeams(13, game, 26, 113)); //emu-wmu
                    list.Add(game => MatchTeams(13, game, 14, 95)); //bgsu-toledo
                    break;

                default:
                    break;
            }

            return list.ToArray();
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
                game => MatchTeams(7, game, 53, 64), //mtsu-nt
                game => MatchTeams(13, game, 53, 211), //wku-mtsu
                game=> MatchTeams(13,game,8,57), //army-navy
                game => MatchTeams(12, game, 64, 232), //nt-UTSA
                //game=> MatchTeams(13,game,85,98), //usm-uab
                //game => MatchTeams(6, game, 43, 85), //lt-usm

#if false
                game => MatchTeams(6, game, 53, 143), //mtsu-troy
                game => MatchTeams(7, game, 143, 229), //fau-troy
                game => MatchTeams(8, game , 46, 211), // marsh-wku
                game => MatchTeams(6, game, 143 ,233), //fau-gsu
                game => MatchTeams(7, game, 143 ,233), //fau-gsu
                game => MatchTeams(6, game, 143 ,229), //fau-troy
                game => MatchTeams(8, game, 46, 211), //wku-marsh
                game => MatchTeams( 8, game, 34,181), //gaso-app st
                game => MatchTeams(7, game , 34, 46), // marsh-app st
                game => MatchTeams(7, game, 61, 181), // coastal- gaso
                game => MatchTeams(6, game, 34, 234), // app st - odu
                game=> MatchTeams(7,game,53,143), //mtsu-troy
                game => MatchTeams(13, game, 143, 235), //usa-troy
                game => MatchTeams(13, game, 229, 230), //fau-fiu
                game=> MatchTeams(13,game,34,61), //ccu-app st
                game=> MatchTeams(13, game, 181, 233), // gsu-gaso
                game=> MatchTeams(13, game, 46, 234), // odu - marshall
#endif
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
                        IsUNCNCSU,
                        IsClemsonGT,
                        IsWFDuke,
                        IsWVUVT,
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

        public int? IsClemsonGT(PreseasonScheduledGame game)
        {
            return MatchTeams(6, game, 21, 31);
        }

        public int? IsClemsonFSU(PreseasonScheduledGame game)
        {

            if (game.HomeTeam == 28 && game.AwayTeam == 21)
                return 8;
            if (game.HomeTeam == 21 && game.AwayTeam == 28)
                return 12;
            return default;
        }

        public int? IsUMDWVU(PreseasonScheduledGame game)
        {
            // set to week 3 if this is ever neutral again
            return MatchTeams(game.Week, game, 47, 112);
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

        public int? IsWFDuke(PreseasonScheduledGame game)
        {
            return MatchTeams(13, game, 24, 109);
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

        public int? IsWVUVT(PreseasonScheduledGame game)
        {
            return MatchTeams(7, game, 108, 112);
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
        public static void ExtraConfGameSwap(Dictionary<int, TeamSchedule> schedules)
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

        public static void MoveReplaceableGames(Dictionary<int, TeamSchedule> schedules, Func<PreseasonScheduledGame, bool> confGameQualifier)
        {
            // give me all extra games
            var conf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsExtraConferenceGame() && confGameQualifier(g))).Distinct().ToList();

            foreach (var game in conf)
            {
                if (game.Week < 4)
                    continue;

                var homeOpenings = schedules[game.HomeTeam].FindOpenWeeks();
                var awayOpenings = game.AwayTeam.IsFcsTeam() ? FcsOpenings() : schedules[game.AwayTeam].FindOpenWeeks();
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

                if(team.IsSECTeam() && firstTime)
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

        private static bool TryPop(this Stack<int> stack, out int value)
        {
            try
            {
                if (stack.Count == 0)
                {
                    value = 0;
                    return false;
                }

                value = stack.Pop();
                return true;
            }
            catch
            {
                value = 0;
            }

            return false;
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
#if false
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

        private static void TemporarilyRemoveGameFromSchedule(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame game)
        {
            var gameIndex = game.WeekIndex;
            var home = game.HomeTeam;
            var homeSchd = schedules[home];
            homeSchd[gameIndex] = null;

            if (gameIndex < 5)
            {
                homeSchd.AddUnscheduledGame(game, GetGameNumber());
            }

            if (schedules.TryGetValue(game.AwayTeam, out var awaySchedule))
            {
                awaySchedule[gameIndex] = null;

                if (!game.AwayTeam.IsFcsTeam() && gameIndex < 5)
                {
                    awaySchedule.AddUnscheduledGame(game, GetGameNumber());
                }
            }
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
                TemporarilyRemoveGameFromSchedule(schedules, game);
            }

            var conf = schedules.Values.SelectMany(games => games.Where(g => g != null && g.IsConferenceGame() && !g.IsSecConfGame())).Distinct().OrderBy(g => g.WeekIndex).ToArray();
            var nonConf = schedules.Values.SelectMany(games => games.Where(g => g != null && !g.IsLateSeasonRivalryGame() && !g.IsConferenceGame() && !g.IsFCSGame())).Distinct().OrderByDescending(g => g.WeekIndex).ToArray();
            var secNonConf = nonConf.Where(g => g.IsSECNonConferenceGame());
            var theRest = nonConf.Where(g => !g.IsSECNonConferenceGame());

            foreach (var psg in conf.Concat(nonConf))
            {
                TemporarilyRemoveGameFromSchedule(schedules, psg);
            }

            // non conf go earlier
            foreach (var game in secNonConf.Concat(theRest))
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
                    //homeSchd[gameIndex] = null;
                    //awaySchd[gameIndex] = null;
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

        public static List<PreseasonScheduledGame> FindExtraP5Games(Dictionary<int, TeamSchedule> schedules)
        {
            // find b12 games such that we have a unique number of teams
            var foundTeams = new HashSet<int>();
            var b12 = FindExtraBig12Games(schedules);
            var b12Match = new List<PreseasonScheduledGame>();
            var accMatch = new List<PreseasonScheduledGame>();
            var leftovers = new List<PreseasonScheduledGame>();

            // we get no more than 7 games
            foreach (var g in b12)
            {
                if (schedules[g.HomeTeam][0] != null || schedules[g.AwayTeam][0] != null)
                {
                    leftovers.Add(g);
                    continue;
                }

                if (foundTeams.Add(g.HomeTeam) && foundTeams.Add(g.AwayTeam))
                {
                    b12Match.Add(g);
                }
                else
                {
                    leftovers.Add(g);
                }
            }

            var accGames = FindExtraAccGames(schedules);

            foreach (var g in accGames)
            {
                if (schedules[g.HomeTeam][0] != null || schedules[g.AwayTeam][0] != null)
                {
                    leftovers.Add(g);
                    continue;
                }
                else
                {
                    accMatch.Add(g);
                }
            }

            var diff = Math.Abs(b12Match.Count - accMatch.Count);

            if (accMatch.Count < b12Match.Count)
            {
                leftovers.AddRange(b12Match.Take(diff));
                b12Match = b12Match.Skip(diff).ToList();
            }
            else if (b12Match.Count < accMatch.Count)
            {
                leftovers.AddRange(accMatch.Take(diff));
                accMatch = accMatch.Skip(diff).ToList();
            }


            MessageBox.Show("Big12Match is: " + b12Match.Count, "Leftovers count is: " + leftovers.Count);

            for (int i = 0; i < accMatch.Count; i++)
            {
                // null them out first
                schedules[accMatch[i].HomeTeam][accMatch[i].WeekIndex] = null;
                schedules[accMatch[i].AwayTeam][accMatch[i].WeekIndex] = null;
                schedules[b12Match[i].HomeTeam][b12Match[i].WeekIndex] = null;
                schedules[b12Match[i].AwayTeam][b12Match[i].WeekIndex] = null;

                ScheduleFixup.SwapTeams(accMatch[i], b12Match[i]);
                accMatch[i].SetWeek(0);
                b12Match[i].SetWeek(0);

                // set them in the team schedule now
                schedules[accMatch[i].HomeTeam][accMatch[i].WeekIndex] = accMatch[i];
                schedules[accMatch[i].AwayTeam][accMatch[i].WeekIndex] = accMatch[i];
                schedules[b12Match[i].HomeTeam][b12Match[i].WeekIndex] = b12Match[i];
                schedules[b12Match[i].AwayTeam][b12Match[i].WeekIndex] = b12Match[i];
            }

            return leftovers;
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
                .Concat(FindExtraAccGames(schedules));

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

                    var open = schedules[game.HomeTeam].FindNextOpenWeek( game.WeekIndex);
                    schedules[game.HomeTeam].MoveFcsGame( game.WeekIndex, open);

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
        static void Shuffle<T>(this T[] array)
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
        }

        public static void Fix(Dictionary<int, TeamSchedule> schedules, ConferenceLocks confLocks, int confId, Action<Dictionary<int, TeamSchedule>> special = null)
        {
            var teams = RecruitingFixup.TeamAndConferences.Where(kvp => kvp.Value == confId).Select(kvp => kvp.Key).ToArray();
            Shuffle(teams);
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

                    if (currentGame.IsConferenceGame() && !currentGame.IsExtraConferenceGame() && !currentGame.MustReplace)
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

        public static void SunBeltFix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new SunBeltLocks(), RecruitingFixup.SBCId);
        }

        public static void MWCFix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new MWCLocks(), RecruitingFixup.MWCId);
        }

        public static void MACFix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new MACLocks(), RecruitingFixup.MACId);
        }

        public static void CUSAFix(Dictionary<int, TeamSchedule> schedules)
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

        public static void SetHomeTeamForRoundRobin(Dictionary<int, TeamSchedule> schedules, Dictionary<int, int[]> homeGames)
        {
            foreach (var game in schedules.Values.SelectMany(s => s.Where(g => g != null && g.IsConferenceGame() && homeGames.ContainsKey(g.HomeTeam))))
            {
                game.SetHomeTeam(homeGames);
            }
        }

        public static void AmericanFix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new AmericanLocks(), RecruitingFixup.AmericanId);
        }

        public static void Big10Fix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new Big10Locks(), RecruitingFixup.Big10Id);
        }

        public static void Big12Fix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new Big12Locks(), RecruitingFixup.Big12Id);
        }

        public static void SecFix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new SecLocks(), RecruitingFixup.SECId);
        }

        public static void AccFix(Dictionary<int, TeamSchedule> schedules)
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

        public static void Pac12Fix(Dictionary<int, TeamSchedule> schedules)
        {
            Fix(schedules, new Pac12Locks(), RecruitingFixup.Pac16Id);
        }

        private static HashSet<PreseasonScheduledGame> RejectedOnce = new HashSet<PreseasonScheduledGame>();

        public static bool AssignGame(this PreseasonScheduledGame game, Dictionary<int, TeamSchedule> schedules, int week, bool postFix = false)
        {
            var homeSchedule = schedules[game.HomeTeam];
            var awaySchedule = game.AwayTeam.IsFcsTeam() ? new TeamSchedule(true) : schedules[game.AwayTeam];

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
                    var findLate = conf == RecruitingFixup.Pac16Id || conf == RecruitingFixup.Big10Id || conf == RecruitingFixup.MACId || conf == RecruitingFixup.SBCId || conf == RecruitingFixup.CUSAId || conf == RecruitingFixup.AmericanId || conf == RecruitingFixup.MWCId;

                    var finalTry = FindCommonOpenWeek(homeSchedule.FindOpenWeeks(week), awaySchedule.FindOpenWeeks(week), findLate, out var nextOpen) ? nextOpen : week;
                    return AssignGame(game, schedules, finalTry);
                }
            }

            if (awaySchedule[week] == null && homeSchedule[week] == null)
            {
                // var currentWeek = game.WeekIndex;
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

        public int? ConferenceGameId()
        {
            if (!this.IsConferenceGame())
            {
                return null;
            }

            return RecruitingFixup.TeamAndConferences[HomeTeam];
        }

        public bool IsAmericanConferenceGame()
        {
            var conf = this.ConferenceGameId();
            return conf.HasValue && conf.Value == RecruitingFixup.AmericanId;
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

        public bool IsG5FCSGame()
        {
            return HomeTeam.IsG5() && HomeTeam != 68 && AwayTeam.IsFcsTeam();
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

        public bool IsSECNonConferenceGame()
        {
            return (this.HomeTeam.IsSECTeam() || this.AwayTeam.IsSECTeam());
        }

        public bool IsLateSeasonRivalryGame()
        {
            if (!IsRivalryGame())
            {
                return false;
            }

            var rivalry = new Rivalry(HomeTeam, AwayTeam);
            return lateSeasonRivals.Contains(rivalry);
        }

        private static readonly HashSet<Rivalry> lateSeasonRivals = new HashSet<Rivalry>()
        {
            new Rivalry(68,102),
            new Rivalry(68, 87),
            new Rivalry(68, 57),
            new Rivalry(27,28),
            new Rivalry(42,44),
            new Rivalry(21,84),
            new Rivalry(30,31),
            new Rivalry(1,57),
            new Rivalry(1, 8),
            new Rivalry(8,57),
            new Rivalry(18,144),
            new Rivalry(37, 38),
            new Rivalry(20, 50),
            new Rivalry(22, 23),
            new Rivalry(83,89),
            new Rivalry(103, 104),
        };

        public bool IsG5Game()
        {
            return !IsConferenceGame() && !AwayTeam.IsP5OrND() && !HomeTeam.IsP5OrND() && !AwayTeam.IsFcsTeam() && !HomeTeam.IsFcsTeam();
        }

        public bool IsAmericanGame()
        {
            return AwayTeam.IsAmericanTeam() || HomeTeam.IsAmericanTeam();
        }

        public bool IsServiceAcademyGame()
        {
            return ScheduleFixup.IsServiceAcademyGame(HomeTeam, AwayTeam);
        }

        public bool IsAccConfGame()
        {
            return this.HomeTeam.IsAccTeam() && this.AwayTeam.IsAccTeam();
        }

        public bool IsBig12ConfGame()
        {
            return this.HomeTeam.IsBig12Team() && this.AwayTeam.IsBig12Team();
        }

        public bool IsAccOrSecOrBig12Game()
        {
            return this.HomeTeam.IsSECTeam() || this.HomeTeam.IsAccTeam() || this.AwayTeam.IsSECTeam() || this.AwayTeam.IsAccTeam() || this.AwayTeam.IsBig12Team() || this.HomeTeam.IsBig12Team();
        }

        public bool IsAccOrSecConfGame()
        {
            return this.IsSecConfGame() || this.IsAccConfGame();
        }

        public bool IsSecConfGame()
        {
            return this.HomeTeam.IsSECTeam() && this.AwayTeam.IsSECTeam();
        }

        public bool IsSunBeltGame()
        {
            return this.HomeTeam.IsSunBeltTeam() && this.AwayTeam.IsSunBeltTeam();
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

        public bool IsExtraSunbeltGame()
        {
            if (SunBeltSchedule.SunbeltConferenceSchedule != null &&
                SunBeltSchedule.SunbeltConferenceSchedule.ContainsKey(this.HomeTeam) &&
                SunBeltSchedule.SunbeltConferenceSchedule.ContainsKey(this.AwayTeam) &&
                RecruitingFixup.SunBeltTeams == 16)
            {

                // if either team is supposed to host the other, then it's not an extra game!
                return !(SunBeltSchedule.SunbeltConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam) ||
                    SunBeltSchedule.SunbeltConferenceSchedule[this.AwayTeam].Contains(this.HomeTeam));
            }

            return false;
        }

        public bool IsCrossDivisionGame()
        {
            return SunBeltSchedule.CrossDivision(this.HomeTeam, this.AwayTeam);
        }

        public bool IsExtraAccGame()
        {
            return false;
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

        public bool IsExtraConferenceGame() => IsExtraAccGame() || IsExtraSunbeltGame();

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

        public bool ShouldFixSunBeltGame()
        {
#if true
            return false;
#else
            if(RecruitingFixup.SunBeltTeams < 16)
            {
                return false;
            }

            var isOddYear = !Form1.IsEvenYear.Value;

            if (isOddYear)
            {
                return !SunBeltSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
            }
            else
            {
                return SunBeltSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
            }
#endif
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