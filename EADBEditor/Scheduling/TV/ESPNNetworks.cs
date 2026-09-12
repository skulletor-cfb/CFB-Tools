using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    /// <summary>
    /// ESPN, ABC, ESPN2, SEC Network, ACC Network, ESPNU, ESPN+
    /// </summary>
    public class ESPNNetworks : NetworkSchedule
    {
        private static int RedRiverWeek = -1;
        public static readonly ESPNNetworks Instance = new ESPNNetworks();

        public ChannelSchedule ABC =  ChannelSchedule.Create(ChannelName.ABC);
        public ChannelSchedule ESPN => this.Primary;
        public ChannelSchedule ESPN2 = ChannelSchedule.Create(ChannelName.ESPN2);

        public ChannelSchedule ESPNU = ChannelSchedule.Create(ChannelName.ESPNU);
        public ChannelSchedule ACCN = ChannelSchedule.Create(ChannelName.ACCNetwork);
        public ChannelSchedule SECN = ChannelSchedule.Create(ChannelName.SECNetwork);
        public StreamingSchedule ACCNX = StreamingSchedule.Create(StreamingProvider.ACCNX);

        private ESPNNetworks() : base(ChannelName.ESPN, StreamingProvider.ESPNPlus)
        {
        }

        public override void Report()
        {
            WriteReport(ABC);
            WriteReport(ESPN2);
            WriteReport(ESPNU);
            WriteReport(ACCN);
            WriteReport(SECN);
            WriteReport(ACCNX);
            base.Report();
        }

        public override NetworkSchedule AssignGames()
        {
            AssignPac12AfterDark();
            AssignSunBeltTuesday();
            AssignThanksgivingWeekend();
            AssignMACtion();
            AssignSecGamesOfTheWeek();
            AssignP5ESPN();
            AssignABCNoon();
            AssignACCFriday();
            AssignAmericanFriday();
            AssignMidMajorThursday();
            AssignACCNetwork(new Func<int, TimeSlot>[] { week => new TimeSlot(3, 30, week) });
            AssignSECNetwork(new Func<int, TimeSlot>[] { week => new TimeSlot(4, 15, week) });
            AssignP5ESPN_NoonGames();
            AssignACCNetwork(new Func<int, TimeSlot>[] { week => new TimeSlot(12, 0, week), week => new TimeSlot(7, 30, week), });
            AssignSECNetwork(new Func<int, TimeSlot>[] { week => new TimeSlot(12, 45, week), week => new TimeSlot(7, 45, week), });
            AssignESPNU();
            AssignMidMajorFriday();
            return this;
        }

        public override void AssignStreaming()
        {
            AssignAccNetworkExtra();
            AssignStreamingGames();
        }

        private void AssignAccNetworkExtra()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var week = kvp.Key;
                var queue = kvp.Value.Where(g => g.IsAccGame).ToQueue();

                var times = new[]
                {
                    new TimeSlot(12,0,week),
                    new TimeSlot(3,30,week),
                    new TimeSlot(4,0,week),
                }.Shuffle();

                var idx = 0;

                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    var slot = times[idx++ % times.Length];
                    ACCNX.AssignGame(game, slot);
                }
            }
        }

        private void AssignStreamingGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var week = kvp.Key;
                var games = kvp.Value;

                var times = new[]
                {
                    new TimeSlot(12,0,week),
                    new TimeSlot(12,30,week),
                    new TimeSlot(1,00,week),
                    new TimeSlot(1,30,week),
                    new TimeSlot(3,30,week),
                    new TimeSlot(4,0,week),
                    new TimeSlot(4,30,week),
                    new TimeSlot(7,0,week),
                    new TimeSlot(7,30,week),
                    new TimeSlot(8,0,week),
                    new TimeSlot(8,30,week),
                };

                var westernTimes = new[]
                {
                    new TimeSlot(3,30,week),
                    new TimeSlot(6,30, week),
                    new TimeSlot(10,30,week),
                };

                var widx = 0;
                var timeIdx = 0;

                var queue = games.Where(g => !g.Assigned).ToQueue();

                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (game.IsPac12Game)
                    {
                        var slot = westernTimes[widx++ % westernTimes.Length];

                        if(game.IsArizonaGame)
                        {
                            slot = new TimeSlot(9, 30, week);
                        }

                        Streaming.AssignGame(game, slot);
                    }
                    else
                    {
                        var slot = times[timeIdx++ % times.Length];
                        Streaming.AssignGame(game, slot);
                    }
                }
            }
        }

        /// <summary>
        /// find a matchup where they have a previous bye week
        /// </summary>
        private void AssignSunBeltTuesday()
        {
            var dict = new Dictionary<int, TelevisedGame[]>();
            var sunBeltGames = TelevisionScheduler.AllGames.Values.SelectMany(g => g.Games).Where(g => g.HomeTeam.IsSunBeltTeam() && g.AwayTeam.IsSunBeltTeam());

            // build the schedules
            foreach (var game in sunBeltGames)
            {
                if (!dict.TryGetValue(game.AwayTeam, out var awaySchedule))
                {
                    awaySchedule = dict[game.AwayTeam] = new TelevisedGame[14];
                }

                if (!dict.TryGetValue(game.HomeTeam, out var homeSchedule))
                {
                    homeSchedule = dict[game.HomeTeam] = new TelevisedGame[14];
                }

                awaySchedule[game.Week] = game;
                homeSchedule[game.Week] = game;
            }

            // walk backwards with week 12
            for (int i = 12; i > TelevisionScheduler.FirstWeekOfOctober(); i--)
            {
                var gamesThisWeek = sunBeltGames.Where(g => g.Week == i).ToList();
                var eligibleGames = gamesThisWeek.Where(
                    g =>
                    {
                        return dict[g.AwayTeam][i - 1] == null && dict[g.HomeTeam][i - 1] == null;
                    }).ToQueue();

                if (eligibleGames.TryDequeueGameForAssignment(out var game))
                {
                    ESPN.AssignGame(game, i, 8, 0, day: 1);
                    continue;
                }
            }
        }

        private void AssignThanksgivingWeekend()
        {
            AssignThanksgivingDay();
            AssignSECThanksgivingWeekend();

            // top sun belt game is at 3pm friday on ESPN+
            var game = this.WeeklySchedule[13].Where(g => !g.Assigned && g.IsSunBeltGame).ToQueue().Dequeue();
            Streaming.AssignGame(game, 13, 3, 0, 4);
        }

        private void AssignSECThanksgivingWeekend()
        {
            var queue = this.WeeklySchedule[13].Where(g => !g.Assigned && (g.IsSecGame || g.IsSecAccGame)).ToQueue();
            ABC.AssignGame(queue.Dequeue(), 13, 7, 30, day: 4);
            ABC.AssignGame(queue.Dequeue(), 13, 3, 30, day: 5);
            ABC.AssignGame(queue.Dequeue(), 13, 7, 30, day: 5);
            ABC.AssignGame(queue.Dequeue(), 13, 3, 30, day: 4);
            ABC.AssignGame(queue.Dequeue(), 13, 12, 0, day: 5);
            ABC.AssignGame(queue.Dequeue(), 13, 12, 0, day: 4);
        }

        private void AssignThanksgivingDay()
        {
            // start with thanksgiving egg bowl at 12pm
            var eggBowl = this.WeeklySchedule[13].Where(g => g.IsEggBowl).FirstOrDefault();
            var kuMizzou = this.WeeklySchedule[13].Where(g => g.IsCivilWar).FirstOrDefault();
            var texasTamu = this.WeeklySchedule[13].Where(g => g.IsTexasShowDown).FirstOrDefault();
            var texasTech = this.WeeklySchedule[13].Where(g => g.IsTexasTechGame).FirstOrDefault();
            var ksuku = this.WeeklySchedule[13].Where(g => g.CheckMatchup(39, 40)).FirstOrDefault();
            var smuhou = this.WeeklySchedule[13].Where(g => g.IsSMUHOU).FirstOrDefault();
            var tcubu = this.WeeklySchedule[13].Where(g => g.IsTCUBU).FirstOrDefault();
            var queue = new[] { texasTamu, kuMizzou, eggBowl }.Where(g => g != null).ToQueue();
            queue.Enqueue(this.WeeklySchedule[13].Where(g => g.IsAccGame && g.IsConferenceGame).Skip(2).First());
            queue.Enqueue(new[] { tcubu, smuhou, texasTech, ksuku }.Shuffle().Where(g => g != null));

            var arr = new[] { queue.Dequeue(), queue.Dequeue(), queue.Dequeue() }.OrderBy(g => g.Score).ToArray();

            ESPN.AssignGame(arr[0], 13, 8, 0, day: 3);
            ESPN.AssignGame(arr[1], 13, 4, 0, day: 3);
            ESPN.AssignGame(arr[2], 13, 12, 0, day: 3);
        }

        private void AssignMACtion()
        {
            TelevisedGame game = null;
            var firstWeekOfNovember = TelevisionScheduler.LastWeekOfOctober() + 1;
            for (int i = firstWeekOfNovember; i <= 12; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsMACGame).OrderBy(g => g.Score).ToQueue();

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    ESPN2.AssignGame(game, i, 7, 0, day: 1);
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    ESPN2.AssignGame(game, i, 7, 0, day: 2);
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    ESPNU.AssignGame(game, i, 7, 30, day: 1);
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    ESPNU.AssignGame(game, i, 7, 30, day: 2);
                }
            }

            // week 13 goes tuesday, friday, saturday
            var lastWeek = this.WeeklySchedule[13].Where(g => !g.Assigned && g.IsMACGame).OrderBy(g => g.Score).ToQueue();
            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                ESPN2.AssignGame(game, 13, 7, 0, day: 1);
            }

            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                ESPNU.AssignGame(game, 13, 12, 0, day: 4);
            }

            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                Streaming.AssignGame(game, 13, 12, 0, day: 5);
            }
        }

        /// <summary>
        /// 12pm, 3:30pm, 7pm, 1030pm
        /// </summary>
        private void AssignESPNU()
        {
            // first we're just going to assign the mid afternoon and evening games
            for (int i = 0; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned).OrderBy(g => g.Score).ToQueue();

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ESPNU.AssignGame(game, i, 4, 0);
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    ESPNU.AssignGame(game, i, 8, 0);
                }
            }

            // finally not MWC
            for (int i = 0; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && !g.IsMWCGame && !g.IsPac12Game).OrderBy(g => g.Score).ToQueue();

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ESPNU.AssignGame(game, i, 12, 0);
                }
            }
        }

        /// <summary>
        /// SECN gets 1245/415/745 games
        /// </summary>
        private void AssignSECNetwork(Func<int, TimeSlot>[] slots)
        {

            for (int i = 0; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsSecGame).OrderBy(g => g.Score).ToQueue();
                var stack = new Stack<Func<int, TimeSlot>>(slots);

                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (!stack.TryPop(out var timeslotFunc))
                    {
                        break;
                    }

                    var timeSlot = timeslotFunc(i);
                    SECN.AssignGame(game, timeSlot);
                }
            }
        }

        /// <summary>
        /// ACCN gets 12/330/730 games
        /// </summary>
        private void AssignACCNetwork( Func<int,TimeSlot>[] slots)
        {

            for (int i = 0; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsAccGame).OrderBy(g => g.Score).ToQueue();
                var stack = new Stack<Func<int, TimeSlot>>(slots);

                while (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (!stack.TryPop(out var timeslotFunc))
                    {
                        break;
                    }

                    var timeSlot = timeslotFunc(i);
                    ACCN.AssignGame(game, timeSlot);
                }
            }
        }

        /// <summary>
        /// ESPNU shows a mid major game no friday night
        /// </summary>
        private void AssignMidMajorFriday()
        {
            // we start the friday after labor day
            var fridayStarts = TelevisionScheduler.LaborDayWeek() + 1;

            for (int i = fridayStarts; i <= 12; i++)
            {
                // non AAC, and fall back to AAC
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && !g.HomeTeamIsP5 && !g.IsAmericanGame).OrderBy(g => g.Score).ToQueue();
                queue.Enqueue(this.WeeklySchedule[i].Where(g => !g.Assigned && !g.HomeTeamIsP5 && g.IsAmericanGame));

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ESPNU.AssignGame(game, new TimeSlot(7, 0, game.Week, day: 4));
                }
            }
        }

        /// <summary>
        /// mid major thursday games, not MWC at 730 on both ESPN/ESPN2
        /// </summary>
        private void AssignMidMajorThursday()
        {
            // we start the friday after labor day
            var fridayStarts = TelevisionScheduler.LaborDayWeek() + 1;

            for (int i = fridayStarts; i <= 12; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && !g.HomeTeamIsP5).OrderBy(g => g.Score).ToQueue();
                var games = new List<TelevisedGame>();
                bool mwcFound = false;

                // find 3 games, but at most 1 mwc game
                while (games.Count < 3)
                {
                    if (queue.TryDequeueGameForAssignment(out var game))
                    {
                        // no hawaii on thursday
                        if (game.IsHawaiiGame || (mwcFound && game.IsMWCGame))
                        {
                            continue;
                        }

                        games.Add(game);
                        mwcFound = mwcFound || game.IsMWCGame;
                    }
                }

                var espnuGame = games[2];
                var espnuTimeSlot = games[2].IsMWCGame ? new TimeSlot(8, 30, i, day: 3) : new TimeSlot(7, 0, i, day: 3);

                var espnGame = games[0].IsMWCGame ? games[1] : games[0];
                var espnTimeSlot = new TimeSlot(7, 30, i, day: 3);

                var espn2Game = games[0].IsMWCGame ? games[0] : games[1];
                var espn2Timeslot = espn2Game.IsMWCGame ? new TimeSlot(8, 30, i, day: 3) : new TimeSlot(8, 0, i, day: 3);

                ESPN.AssignGame(espnGame, espnTimeSlot);
                ESPNU.AssignGame(espnuGame, espnuTimeSlot);
                ESPN2.AssignGame(espn2Game, espn2Timeslot);
            }
        }

        /// <summary>
        /// the best American game of the week is played on Friday ESPN2, starting 3 weeks after labor day
        /// </summary>
        private void AssignAmericanFriday()
        {
            TelevisedGame game = null;
            // we start the friday after labor day
            var fridayStarts = TelevisionScheduler.LaborDayWeek() + 3;

            for (int i = fridayStarts; i <= 12; i++)
            {
                // look for conference game first
                var queue = this.WeeklySchedule[i].Where(g => g.IsAmericanGame && !g.Assigned).ToQueue();

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    ESPN2.AssignGame(game, i, 7, 30, day: 4);
                }
            }

            var blackFriday = this.WeeklySchedule[13].Where(g => g.IsAmericanGame && !g.Assigned).ToQueue();
            if (blackFriday.TryDequeueGameForAssignment(out game))
            {
                ESPN.AssignGame(game, 13, 12, 0, day: 4);
            }

            if (blackFriday.TryDequeueGameForAssignment(out game))
            {
                ESPN.AssignGame(game, 13, 3, 30, day: 4);
            }
        }

        // the best acc game left is one friday 8pm
        private void AssignACCFriday()
        {
            // we start the friday after labor day
            var fridayStarts = TelevisionScheduler.LaborDayWeek() + 1;

            // first week we play 4 friday night games, worst of the week
            var games = this.WeeklySchedule[fridayStarts].Where(g => g.IsAccGame && !g.Assigned && !g.IsConferenceGame).OrderByDescending(g => g.Score).ToQueue(4);
            var slotsAndChannel = new (ITelelvisionProvider provider, TimeSlot slot)[]
            {
                (ACCNX, new TimeSlot(7, 0, fridayStarts, day: 4)),
                (ACCN, new TimeSlot(7, 0, fridayStarts, day: 4)),
                (ESPNU, new TimeSlot(7, 0, fridayStarts, day: 4)),
                (ESPN2, new TimeSlot(7, 30, fridayStarts, day: 4)),
            }.Shuffle();

            foreach (var (provider, slot) in slotsAndChannel)
            {
                if (games.TryDequeueGameForAssignment(out var game))
                {
                    provider.AssignGame(game, slot);
                }
            }

            // 1 game the rest of the season
            for (int i = fridayStarts + 1; i <= 12; i++)
            {
                // look for conference game first
                var queue = this.WeeklySchedule[i]
                    .Where(g => g.IsAccGame && g.IsConferenceGame).ToQueue();

                // any acc hosted game
                queue.Enqueue(this.WeeklySchedule[i].Where(g => g.IsAccGame));

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ESPN.AssignGame(game, i, 8, 0, day: 4);
                }
            }
        }

        private void AssignABCNoon()
        {
            // first 5 weeks is the best game left over
            for (int i = 0; i < 5; i++)
            {
                if (i == RedRiverWeek)
                {
                    continue;
                }

                var queue = this.WeeklySchedule[i]
                    .Where(g => !g.Assigned && g.IntraConferenceP5 && !g.IsPac12Game).OrderBy(g => g.Score).ToQueue();

                queue.Enqueue(this.WeeklySchedule[i]
                    .Where(g => !g.Assigned && g.HomeTeamIsP5 && !g.IsPac12Game).OrderBy(g => g.Score));

                queue.Enqueue(this.WeeklySchedule[i]);

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ABC.AssignGame(game, i, 12, 0);
                }
            }

            // sec games first
            for (int i = 5; i <= 12; i++)
            {
                if (i == RedRiverWeek)
                {
                    continue;
                }

                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsSecConferenceGame).OrderBy(g => g.Score).ToQueue();
                queue.Enqueue(this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsAmericanGame));
                queue.Enqueue(this.WeeklySchedule[i].Where(g => !g.Assigned && !g.IsMWCGame));

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    if (game.IsMWCGame)
                    {
                        continue;
                    }

                    ABC.AssignGame(game, i, 12, 0);
                }
            }
        }

        /// <summary>
        /// best of the afternoon and 330 of the ESPN/ABC games
        /// </summary>
        private void AssignP5ESPN_NoonGames()
        {
            for (int i = 0; i <= 13; i++)
            {
                var games = this.WeeklySchedule[i]
                    .Where(g => !g.Assigned && g.HomeTeamIsP5 && !g.IsFCSGame && !g.IsPac12Game).OrderBy(g => g.Score).ToQueue();

                games.Enqueue(this.WeeklySchedule[i].Where(g => !g.IsFCSGame && !g.IsPac12Game));

                if (games.TryDequeueGameForAssignment(out var game))
                {
                    ESPN.AssignGame(game, i, 12, 0);
                }

                if (games.TryDequeueGameForAssignment(out game))
                {
                    ESPN2.AssignGame(game, i, 12, 0);
                }
            }
        }

        /// <summary>
        /// best of the afternoon and 330 of the ESPN/ABC games
        /// </summary>
        private void AssignP5ESPN()
        {
            for (int i = 0; i <= 13; i++)
            {
                var games = this.WeeklySchedule[i]
                    .Where(g => !g.Assigned && g.HomeTeamIsP5).OrderBy(g => g.Score).ToQueue();

                games.Enqueue(this.WeeklySchedule[i]);

                if (games.TryDequeueGameForAssignment(out var game))
                {
                    ESPN.AssignGame(game, i, 7, 0);
                }

                if (games.TryDequeueGameForAssignment(out game))
                {
                    ESPN.AssignGame(game, i, 3, 30);
                }

                if (games.TryDequeueGameForAssignment(out game))
                {
                    ESPN2.AssignGame(game, i, 3, 30);
                }

                if (games.TryDequeueGameForAssignment(out game))
                {
                    ESPN2.AssignGame(game, i, 7, 30);
                }
            }
        }

        /// <summary>
        /// ESPN2 airs one Pac 12 at 1030pm
        /// </summary>
        private void AssignPac12AfterDark()
        {
            for (int i = 0; i <= 13; i++)
            {
                var games = this.WeeklySchedule[i];
                var queue = games.Where(g => g.ConferenceOwner == TableUtility.Pac16Id && !g.IsFCSGame && g.AwayTeam != 68).OrderByDescending(g => g.IsSecConferenceGame).ToQueue();

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ESPN.AssignGame(game, i, 10, 30);
                }
            }
        }

        /// <summary>
        /// 3:30 pm the top ranked SEC game conference game if there is one.  if LSU, it should be 7:30
        /// 7:30 pm the top ranked SEC intraconference game, if none, 2nd best SEC conference game
        /// </summary>
        private void AssignSecGamesOfTheWeek()
        {
            for (int i = 0; i <= 12; i++)
            {
                // the RRR starts at 1230, shifting everything 30 minutes
                var primetimeSlot = i == RedRiverWeek ? new TimeSlot(8, 0, i) : new TimeSlot(7, 30, i);
                var afternoonSlot = i == RedRiverWeek ? new TimeSlot(4, 0, i) : new TimeSlot(3, 30, i);

                // top sec conference game
                var games = this.WeeklySchedule[i];
                var secGames = games.Where(g => !g.Assigned && !g.IsSecConferenceGame && ((g.ConferenceOwner == TableUtility.SECId && g.IsP5Game) || g.IsSecAccGame)).OrderBy(g => g.Score).ToQueue();

                // top one goes to 330 unless its LSU
                var secConferenceGames = games.Where(g => !g.Assigned && g.IsSecConferenceGame).OrderBy(g => g.Score).ToQueue();

                if (secConferenceGames.TryDequeueGameForAssignment(out var gotw))
                {
                    var secondarySlot = primetimeSlot;
                    if (gotw.HomeTeam == TableUtility.LSUId)
                    {
                        ABC.AssignGame(gotw, primetimeSlot);
                        secondarySlot = afternoonSlot;
                    }
                    else
                    {
                        ABC.AssignGame(gotw, afternoonSlot);
                    }

                    if (secGames.TryDequeueGameForAssignment(out var primetime))
                    {
                        ABC.AssignGame(primetime, secondarySlot);
                    }
                    else if (secConferenceGames.TryDequeueGameForAssignment(out primetime))
                    {
                        ABC.AssignGame(primetime, secondarySlot);
                    }
                }
                else
                {
                    if (secGames.TryDequeueGameForAssignment(out var primetime))
                    {
                        ABC.AssignGame(primetime, primetimeSlot);
                    }

                    if (secGames.TryDequeueGameForAssignment(out gotw))
                    {
                        ABC.AssignGame(gotw, afternoonSlot);
                    }
                }
            }
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            // Iron Bowl is locked into 330 Saturday
            if (game.CheckMatchup(3, 9))
            {
                return !ABC.PreassignGame(game, new TimeSlot(3, 30, week: game.Week), true);
            }

            // ABC buys the Texas-OU game
            if (game.CheckMatchup(71, 92))
            {
                RedRiverWeek = game.Week;
                return !ABC.PreassignGame(game, new TimeSlot(12, 30, week: game.Week), true);
            }

            // ABC buys navy-nd when it's in midseason and Navy is at home
            if(game.CheckMatchup(57, 68) && game.Week > 2 && game.HomeTeam == 57)
            {
                return !ABC.PreassignGame(game, new TimeSlot(12, 0, game.Week), true);
            }

            // labor day monday does not get assigned
            if (game.Week <= 2 && game.Day == 0)
            {
                return !ESPN.PreassignGame(game, new TimeSlot(game.GameTimeOfDay, game.Week, game.Day), false);
            }

            // rocky mountain showdown will get assigned manually
            if (game.CheckMatchup(22, 23))
            {
                // if saturday, go to espn2
                if (game.Day == 5)
                {
                    return !ESPNU.PreassignGame(game, new TimeSlot(3, 30, week: game.Week, day: game.Day), true);
                }
                else
                {
                    return !ESPN2.PreassignGame(game, new TimeSlot(game.GameTimeOfDay, week: game.Week, day: game.Day), true);
                }
            }

            // Sundays before labor day do not get assigned
            // same with thur/fri as those are hand crafted
            if (game.Week <= 1 && game.Day != 5)
            {
                return !ESPN.PreassignGame(game, new TimeSlot(game.GameTimeOfDay, game.Week, game.Day), false);
            }

            // Mayhem at MBS, Johnny Majors Classic do get the 8pm slot on ESPN
            if (game.GTOD == 1173 || game.GTOD == 1177)
            {
                return !ESPN.PreassignGame(game, new TimeSlot(7, 0, game.Week, day: game.Day), false);
            }

            // ESPNU gets the oyster bowl  Oyster Bowl
            if (game.GTOD == 1157)
            {
                var timeSlot = new TimeSlot(8, 0, game.Week, false, game.Day);
                return !ESPNU.PreassignGame(game, timeSlot, false);
            }

            return base.PreassignGame(game);
        }
    }
}