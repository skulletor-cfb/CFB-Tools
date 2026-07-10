using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
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
                        //g => MatchTeams(13, g, 20, 38), // ISU-Cincy end the season when they play
                        g => MatchTeams(7,g,11,94), //BU-TT in week 7
                        g => MatchTeams(12, g, 39, 58), // neb-ku week 12
                        g => MatchTeams(4, g, 83, 89), //TCU-SMU week 4
                        g => MatchTeams(13, g, 33, 83), // smu-hou week 13
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
                        g=>MatchTeams(13, g, 79, 97), // rice-tulsa
                        g=>MatchTeams(13, g, 25, 100), // charlotte-ecu
                        g=>MatchTeams(13, g, 48, 98), // memphis-uab
  //                      g=>MatchTeams(13, g, 85, 96), // usm-tulane
    //                    g=>MatchTeams(12, g, 85, 98), // usm-uab

      //                  g=>MatchTeams(12, g, 90, 232), // utsa-temple
        //                g=>MatchTeams(6, g, 48, 85), // usm-memphis
                        g=>MatchTeams(7, g, 18, 97), // ucf-tulsa
                        g=>MatchTeams(6, g, 18, 25), // ucf-ecu
          //              g=>MatchTeams(7, g, 25, 85), // usm-ecu
                        g=>MatchTeams(6, g, 79, 83), // rice-smu
                        g=>MatchTeams(6, g, 33, 97), // hou-tulsa
                        g=>MatchTeams(6, g, 79, 96), // tulane-rice
                        g=>MatchTeams(8, g, 33, 79), // hou-rice
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
                game => MatchTeams(13, game, 218, 232), //nt-UTSA
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
}
