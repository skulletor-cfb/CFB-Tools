using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    public class FoxNetworks : NetworkSchedule
    {
        public static readonly FoxNetworks Instance = new FoxNetworks();

        public ChannelSchedule FOX => this.Primary;
        public ChannelSchedule FS1 = ChannelSchedule.Create(ChannelName.FoxSports1);
        public ChannelSchedule BTN = ChannelSchedule.Create(ChannelName.BigTenNetwork);

        private FoxNetworks() : base(ChannelName.FOX, StreamingProvider.FoxOne)
        {
        }

        public override void Report()
        {
            WriteReport(FOX);
            WriteReport(BTN);
            WriteReport(FS1);
            WriteReport(Streaming);
        }

        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var games = kvp.Value;
                AssignFoxGames(kvp.Key, games);
                AssignBTNGames(kvp.Key, games);
                AssignFS1Games(kvp.Key, games);
                AssignStreamingGames(kvp.Key, games);
            }

            return this;
        }


        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // take the rest of the big 12 games
            this.SelectedGames.Select(televisedGames[TableUtility.Big12Id].Where(g => !g.Selected));

            // take the rest of pac 12 games
            this.SelectedGames.Select(televisedGames[TableUtility.Pac16Id].Where(g => !g.Selected));

            // take the rest of the big 10 games
            this.SelectedGames.Select(televisedGames[TableUtility.Big10Id].Where(g => !g.Selected));
        }

        private void AssignStreamingGames(int week, List<TelevisedGame> games)
        {
            var times = new[]
            {
                new TimeSlot(12,0,week),
                new TimeSlot(1,0,week),
                new TimeSlot(3,30,week),
                new TimeSlot(4,0,week),
                new TimeSlot(4,30,week),
                new TimeSlot(7,30,week),
                new TimeSlot(8,0,week),
            };

            var p12Times = new[]
            {
                new TimeSlot(3,30,week),
                new TimeSlot(6,30, week),
                new TimeSlot(10,30,week),
            };

            var p12idx = 0;
            var timeIdx = 0;

            var queue = games.Where(g => !g.Assigned && !g.IsPac12Game).ToQueue();
            queue.Enqueue(games.Where(g => !g.Assigned && g.IsPac12Game));

            while (queue.TryDequeueGameForAssignment(out var game))
            {
                if (game.IsPac12Game)
                {
                    if (game.IsArizonaGame)
                    {
                        if (week.IsAugustSeptember())
                        {
                            Streaming.AssignGame(game, week, 9, 30);
                        }
                        else if (game.IsASUvAU)
                        {
                            Streaming.AssignGame(game, week, 9, 00);
                        }
                        else
                        {
                            var early = (Guid.NewGuid().ToByteArray().First() & 0x1) == 0;
                            if (early)
                            {
                                Streaming.AssignGame(game, week, 6, 00);
                            }
                            else
                            {
                                Streaming.AssignGame(game, week, 9, 30);
                            }
                        }
                    }
                    else if (game.IsFCSGame)
                    {
                        var slot = p12Times[p12idx++ % 2];
                        Streaming.AssignGame(game, slot);
                    }
                    else
                    {
                        var slot = p12Times[p12idx++ % p12Times.Length];
                        Streaming.AssignGame(game, slot);
                    }
                }
                else
                {
                    var slot = times[timeIdx++ % times.Length];
                    Streaming.AssignGame(game, slot);
                }
            }
        }

        private void AssignFS1Games(int week, List<TelevisedGame> games)
        {
            // definitely get pac 12 after dark
            var afterDark = games.Where(g => !g.Assigned && g.IsPac12Game).FirstOrDefault();
            FS1.AssignGame(afterDark, week, 10, 30);

            // fs1 shows the best non p12 game at noon
            var bigNoon = games.Where(g => !g.Assigned && !g.IsPac12Game).FirstOrDefault();
            FS1.AssignGame(bigNoon, week, 12, 0);

            // fs1 shows the best of the rest at primetime
            var primetime = games.Where(g => !g.Assigned).FirstOrDefault();
            FS1.AssignGame(primetime, week, 7, 15);

            // what's left can go to 4pm
            var afternoon = games.Where(g => !g.Assigned).FirstOrDefault();
            FS1.AssignGame(afternoon, week, 3, 30);
        }

        private void AssignBTNGames(int week, List<TelevisedGame> games)
        {
            var btn = games.Where(g => !g.Assigned && g.IsBig10Game).ToQueue();

            // BTN Friday is only for the start of the season
            IEnumerable<TimeSlot> slots = new[]{
                new TimeSlot(7,0,week,day:4),
                new TimeSlot(3,30,week),
                new TimeSlot(7,15,week),
                new TimeSlot(12,0,week),
            }.Skip(week.IsAugustSeptember() ? 0 : 1);

            var stack = new Stack<TimeSlot>(slots);

            while (btn.TryDequeueGameForAssignment(out var game))
            {
                if (!stack.TryPop(out var timeslot))
                {
                    break;
                }

                BTN.AssignGame(game, timeslot);
            }
        }

        private void AssignFoxGames(int week, List<TelevisedGame> games)
        {
            // top big 10 goes to FOX big noon, if none is available fall back to big 12
            var queue = games.Where(g => g.IsBig10Game).ToQueue();

            // we know that the last week we have 6 games, plenty to go around
            if (week <= 12)
            {
                queue.Enqueue(games.Where(g => g.IsBig12Game));
                if (queue.TryDequeueGameForAssignment(out var bigNoon))
                {
                    FOX.AssignGame(bigNoon, week, 12, 0);
                }
            }
            else if (week == 13)  // last week of the season, fox will sublicense big 10 games on friday
            {
                // 330pm b12 game to CBS Friday
                queue = games.Where(g => g.IsBig12Game && !g.Assigned).ToQueue();
                CBSNetwork.Instance.SubLicense(queue.Dequeue(), new TimeSlot(3, 30, week: week, day: 4));

                // 9pm p12 game, Notre Dame won't play friday
                queue = games.Where(g => g.IsPac12Game && !g.Assigned).ToQueue();
                while (true)
                {
                    var game = queue.Dequeue();
                    if (game.AwayTeam == 68)
                    {
                        continue;
                    }

                    FOX.AssignGame(game, week, 9, 0, day: 4);
                    break;
                }
            }

            // best available big 12 game goes to 3:30pm, fall back to pac 12
            queue = games.Where(g => g.IsBig12Game && !g.Assigned).ToQueue();
            queue.Enqueue(games.Where(g => g.IsPac12Game));

            if (queue.TryDequeueGameForAssignment(out var bg12Afternoon))
            {
                FOX.AssignGame(bg12Afternoon, week, 3, 30);
            }

            var worldSeriesEnd = TelevisionScheduler.LastWeekOfOctober();
            var worldSeriesStart = worldSeriesEnd - 1;

            // FOX doesn't broadcast night games during world series week
            if (week != worldSeriesEnd && week != worldSeriesStart)
            {
                queue = games.Where(g => g.IsPac12Game && !g.Assigned).ToQueue();
                queue.Enqueue(games.Where(g => g.IsBig12Game && !g.Assigned));
                queue.Enqueue(games.Where(g => g.IsBig10Game && !g.Assigned));

                if (queue.TryDequeueGameForAssignment(out var primetimeGame))
                {
                    FOX.AssignGame(primetimeGame, week, 7, 30);
                }
            }

            // fox friday , in september it's the best of the remaining big12/big 10/pac 12 games at 830pm
            if (week.IsAugustSeptember() && week > 0)
            {
                var friday = games.Where(g => !g.Assigned && !g.IsBig10Game).OrderBy(g => g.Score).FirstOrDefault();
                FOX.AssignGame(friday, week, 8, 0, day: 4);

				var btn= games.Where(g => !g.Assigned && g.IsBig10Game).OrderByDescending(g => g.Score).FirstOrDefault();
				BTN.AssignGame(btn, week, 7, 0, day: 4);
			}

			// fox friday the rest of the year is big 10/pac12
			if (!week.IsAugustSeptember() && week != worldSeriesEnd && week != worldSeriesStart && week != 13)
            {
                var friday = games.Where(g => !g.Assigned && !g.IsBig12Game).OrderBy(g => g.Score).ToQueue();

                if (friday.TryDequeueGameForAssignment(out var game))
                {
                    FOX.AssignGame(game, week, 8, 30, day: 4);
                }
            }
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            // the game belongs at 12pm saturday no matter what
            if (game.CheckMatchup(51, 70))
            {
                FOX.PreassignGame(game, new TimeSlot(12, 0, week: game.Week, day: game.Day), true);
                return false;
            }

            return base.PreassignGame(game);
        }
    }
}