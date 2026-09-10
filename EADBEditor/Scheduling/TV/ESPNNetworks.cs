using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    /// <summary>
    /// ESPN, ABC, ESPN2, SEC Network, ACC Network, ESPNU, ESPN+
    /// </summary>
    public class ESPNNetworks : NetworkSchedule
    {
        public static readonly ESPNNetworks Instance = new ESPNNetworks();

        public ChannelSchedule ABC =  ChannelSchedule.Create(ChannelName.ABC);
        public ChannelSchedule ESPN => this.Primary;
        public ChannelSchedule ESPN2 = ChannelSchedule.Create(ChannelName.ESPN2);

        public ChannelSchedule ESPNU = ChannelSchedule.Create(ChannelName.ESPNU);
        public ChannelSchedule ACCN = ChannelSchedule.Create(ChannelName.ACCNetwork);
        public ChannelSchedule SECN = ChannelSchedule.Create(ChannelName.SECNetwork);

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
            base.Report();
        }

        public override NetworkSchedule AssignGames()
        {
            AssignSunBeltTuesday();
            AssignThanksgivingWeekend();
            AssignMACtion();
            AssignAfterDark();
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
            AssignStreamingGames();
            return this;
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
                    if (game.IsMWCGame)
                    {
                        var slot = westernTimes[widx++ % westernTimes.Length];
                        if (game.IsHawaiiGame)
                        {
                            slot = new TimeSlot(11, 30, week);
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
            var sunBeltGames = TelevisionScheduler.AllGames.Values.SelectMany(g => g).Where(g => g.HomeTeam.IsSunBeltTeam() || g.AwayTeam.IsSunBeltTeam());

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

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    CBSSportsNetwork.Instance.SubLicense(game, new TimeSlot(8, 0, i, day: 1));
                }

                if (queue.TryDequeueGameForAssignment(out game))
                {
                    CBSSportsNetwork.Instance.SubLicense(game, new TimeSlot(8, 0, i, day: 2));
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
                CBSSportsNetwork.Instance.SubLicense(game, new TimeSlot(12, 0, 13, day: 5));
            }

            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                CBSSportsNetwork.Instance.SubLicense(game, new TimeSlot(12, 30, 13, day: 4));
            }

            if (lastWeek.TryDequeueGameForAssignment(out game))
            {
                CBSSportsNetwork.Instance.SubLicense(game, new TimeSlot(7, 30, 13, day: 1));
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
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && !g.IsMWCGame).OrderBy(g => g.Score).ToQueue();

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
                        if (mwcFound && game.IsMWCGame)
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

            for (int i = fridayStarts; i <= 12; i++)
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
                var queue = this.WeeklySchedule[i]
                    .Where(g => !g.Assigned && g.IntraConferenceP5).OrderBy(g => g.Score).ToQueue();

                queue.Enqueue(this.WeeklySchedule[i]
                    .Where(g => !g.Assigned && g.HomeTeamIsP5).OrderBy(g => g.Score));

                queue.Enqueue(this.WeeklySchedule[i]);

                if (queue.TryDequeueGameForAssignment(out var game))
                {
                    ABC.AssignGame(game, i, 12, 0);
                }
            }

            // sec games first
            for (int i = 5; i <= 12; i++)
            {
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
                    .Where(g => !g.Assigned && g.HomeTeamIsP5 && !g.IsFCSGame).OrderBy(g => g.Score).ToQueue();

                games.Enqueue(this.WeeklySchedule[i].Where(g => !g.IsFCSGame));

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
        /// ESPN2 airs one MWC at 1030pm
        /// </summary>
        private void AssignAfterDark()
        {
            var stack = new Stack<ChannelSchedule>();
            stack.Push(ESPN2);
            stack.Push(ESPN);

            for (int i = 0; i <= 13; i++)
            {
                var games = this.WeeklySchedule[i];
                var mwc = games.Where(g => !g.Assigned && (g.ConferenceOwner == TableUtility.MWCId || g.ConferenceOwner == TableUtility.Pac16Id)).OrderByDescending(g => g.ConferenceOwner).ThenBy(g => g.Score).ToQueue();

                if (mwc.TryDequeueGameForAssignment(out var game) && stack.TryPop(out var channel))
                {
                    channel.AssignGame(game, i, 10, 30);
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
                // top sec conference game
                var games = this.WeeklySchedule[i];
                var secGames = games.Where(g => !g.Assigned && !g.IsSecConferenceGame && ((g.ConferenceOwner == TableUtility.SECId && g.IsP5Game) || g.IsSecAccGame)).OrderBy(g => g.Score).ToQueue();

                // top one goes to 330 unless its LSU
                var secConferenceGames = games.Where(g => !g.Assigned && g.IsSecConferenceGame).OrderBy(g => g.Score).ToQueue();

                if (secConferenceGames.TryDequeueGameForAssignment(out var gotw))
                {
                    var secondarySlot = new TimeSlot(7, 30, i);
                    if (gotw.HomeTeam == TableUtility.LSUId)
                    {
                        ABC.AssignGame(gotw, i, 7, 30);
                        secondarySlot = new TimeSlot(3, 30, i);
                    }
                    else
                    {
                        ABC.AssignGame(gotw, i, 3, 30);
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
                        ABC.AssignGame(primetime, i, 7, 30);
                    }

                    if (secGames.TryDequeueGameForAssignment(out gotw))
                    {
                        ABC.AssignGame(gotw, i, 3, 30);
                    }
                }
            }
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // take all sec games
            this.SelectedGames.Select(televisedGames[TableUtility.SECId]);

            // take the unselected acc games
            this.SelectedGames.Select(televisedGames[TableUtility.ACCId].Where(g => !g.Selected));

            // for big 12 espn gets half in the cadnce 0-3, 5, 7, 9, 11, 13, 15, 17, 19 ...
            var big12Games = televisedGames[TableUtility.Big12Id];
            this.SelectedGames.Select(big12Games.Take(4));

            // remove the first 4 and last 4 from big 12 games and assign half
            var big12OnESPN = big12Games.Skip(4).Take(big12Games.Count - 8).ToArray();
            for (int i = 1; i < big12OnESPN.Length; i += 2)
            {
                this.SelectedGames.Select(big12OnESPN[i]);
            }

            // espn takes the top MWC game for the 10:30pm slot
            var mwcGames = televisedGames[TableUtility.MWCId].GetAvailableGamesByWeek();
            foreach (var kvp in mwcGames)
            {
                this.SelectedGames.Select(kvp.Value[0]);
            }

            // all american, sun belt, cusa, mac games
            this.SelectedGames.Select(televisedGames.Values.SelectMany(g => g).Where(g => !g.Selected && !g.HomeTeamIsP5));

            // every week get the 3rd best pac12 game
            var pac12 = televisedGames[TableUtility.Pac16Id].GetAvailableGamesByWeek();
            foreach (var kvp in pac12)
            {
                if (kvp.Value.Count >= 3)
                {
                    this.SelectedGames.Select(kvp.Value[2]);
                }
            }
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            // labor day monday does not get assigned
            if (game.Week <= 2 && game.Day == 0)
            {
                ESPN.PreassignGame(game, new TimeSlot(game.GameTimeOfDay, game.Week, game.Day));
                return false;
            }

            // rocky mountain showdown will get assigned manually
            if (game.CheckMatchup(22, 23))
            {
                // if saturday, go to espn2
                if (game.Day == 5)
                {
                    ESPNU.PreassignGame(game, new TimeSlot(3, 30, week: game.Week, day: game.Day));
                }
                else
                {
                    ESPN2.PreassignGame(game, new TimeSlot(game.GameTimeOfDay, week: game.Week, day: game.Day));
                }

                return false;
            }

            // Sundays before labor day do not get assigned
            // same with thur/fri as those are hand crafted
            if (game.Week <= 1 && game.Day != 5)
            {
                ESPN.PreassignGame(game, new TimeSlot(game.GameTimeOfDay, game.Week, game.Day));
                return false;
            }

            // Mayhem at MBS, Johnny Majors Classic do get the 8pm slot on ESPN
            if (game.GTOD == 1173 || game.GTOD == 1177)
            {
                ESPN.PreassignGame(game, new TimeSlot(7, 0, game.Week, day: game.Day));
                return false;
            }

            // ESPNU gets the oyster bowl  Oyster Bowl
            if (game.GTOD == 1157)
            {
                var timeSlot = new TimeSlot(8, 0, game.Week, false, game.Day);
                ESPNU.PreassignGame(game, timeSlot);
                return false;
            }

            return base.PreassignGame(game);
        }
    }
}