using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling.TV
{
    public class FoxNetworks : NetworkSchedule
    {
        public static readonly FoxNetworks Instance = new FoxNetworks();

        public Dictionary<TimeSlot, TelevisedGame> FOX = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> FS1 = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> BTN = new Dictionary<TimeSlot, TelevisedGame>();
        public List<(TimeSlot time, TelevisedGame game)> Streaming = new List<(TimeSlot time, TelevisedGame game)>();

        private FoxNetworks() : base("FOX")
        {
        }

        public override void Report()
        {
            WriteReport("FOX", FOX);
            WriteReport("FS1", FS1);
            WriteReport("BTN", BTN);
            WriteReport("FoxOne", Streaming);
        }

        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var games = kvp.Value;
                AssignFoxGames(kvp.Key, games);
                AssignFS1Games(kvp.Key, games);
                AssignBTNGames(kvp.Key, games);
                AssignStreamingGames(kvp.Key, games);
            }

            return this;
        }


        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // take the rest of the big 12 games
            this.SelectedGames.AddRange(televisedGames[TableUtility.Big12Id].Where(g => !g.Selected).Select(g => g.Select()));

            // take the rest of pac 12 games
            this.SelectedGames.AddRange(televisedGames[TableUtility.Pac16Id].Where(g => !g.Selected).Select(g => g.Select()));

            // take the rest of the big 10 games
            this.SelectedGames.AddRange(televisedGames[TableUtility.Big10Id].Where(g => !g.Selected).Select(g => g.Select()));
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

            var queue = games.Where(g => !g.Assigned && !g.IsPac12Game).Concat(games.Where(g => !g.Assigned && g.IsPac12Game)).ToQueue();

            while (queue.TryDequeueGame(out var game))
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
            // fs1 shows the best non p12 game at noon
            var bigNoon = games.Where(g => !g.Assigned && !g.IsPac12Game).FirstOrDefault();
            FS1.AssignGame(bigNoon, week, 12, 0);

            // fs1 shows the best of the rest at primetime
            var primetime = games.Where(g => !g.Assigned).FirstOrDefault();
            FS1.AssignGame(primetime, week, 7, 15);

            // what's left can go to 4pm
            var afternoon = games.Where(g => !g.Assigned).FirstOrDefault();
            FS1.AssignGame(primetime, week, 3, 30);

            // finally we get pac 12 after dark
            var afterDark = games.Where(g => !g.Assigned && g.IsPac12Game).FirstOrDefault();
            FS1.AssignGame(afterDark, week, 10, 30);
        }

        private void AssignBTNGames(int week, List<TelevisedGame> games)
        {
            var btn = games.Where(g => !g.Assigned && g.IsBig10Game).ToQueue();
            var stack = new Stack<TimeSlot>(
            new[]{
                new TimeSlot(12,0,week),
                new TimeSlot(7,15,week),
                new TimeSlot(3,30,week),
                new TimeSlot(7,0,week,day:4),
            });

            while (btn.TryDequeueGame(out var game))
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
            var bigNoon = games.Where(g => g.IsBig10Game).Concat(games.Where(g => g.IsBig12Game)).FirstOrDefault();
            FOX.AssignGame(bigNoon, week, 12, 0);

            // best available big 12 game goes to 3:30pm, fall back to pac 12
            var bg12Afternoon = games.Where(g => g.IsBig12Game && !g.Assigned).Concat(games.Where(g => g.IsPac12Game)).FirstOrDefault();
            FOX.AssignGame(bg12Afternoon, week, 3, 30);

            // in september there's no playoff baseball so we get a late game, lead with pac 12 and fallback to big 12
            if (!week.IsOctober())
            {
                var primetimeGame = games.Where(g => g.IsPac12Game && !g.Assigned).Concat(games.Where(g => g.IsBig10Game && !g.Assigned)).Concat(games.Where(g => g.IsBig12Game && !g.Assigned)).FirstOrDefault();
                FOX.AssignGame(primetimeGame, week, 7, 30);
            }

            // fox friday , in september it's the best of the remaining big12/big 10/pac 12 games at 830pm
            if (week.IsAugustSeptember() && week > 0)
            {
                var friday = this.SelectedGames.Where(g => !g.Assigned).OrderBy(g => g.Score).FirstOrDefault();
                FOX.AssignGame(friday, week, 8, 30, 4);
            }

            // fox friday the rest of the year is big 10/pac12
            if (!week.IsAugustSeptember())
            {
                var friday = this.SelectedGames.Where(g => !g.Assigned && !g.IsBig12Game).OrderBy(g => g.Score).FirstOrDefault();
                FOX.AssignGame(friday, week, 8, 30, 4);
            }
        }
    }
}