using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
{
    public class Peacock : NetworkSchedule
    {
        public static readonly Peacock Instance = new Peacock();
        private Peacock() : base(ChannelName.Peacock, StreamingProvider.Peacock)
        {
        }

        public override NetworkSchedule AssignGames()
        {
            // week 1 game goes at 8pm thursday
            Streaming.AssignGame(this.WeeklySchedule[0].First(), 0, 8, 0, 3);

            // the rest at either 1230pm or 4pm
            var queue = this.SelectedGames.ToAssignmentQueue();
            while (queue.TryDequeueGameForAssignment(out var game))
            {
                var timeslot = Guid.NewGuid().ToByteArray().Last() % 2 == 0 ? new TimeSlot(12, 30, game.Week) : new TimeSlot(4, 0, game.Week);
                Streaming.AssignGame(game, timeslot);
            }

            return this;
        }
    }

    public class NBCNetwork : NetworkSchedule
    {
        public static readonly NBCNetwork Instance = new NBCNetwork();
        private NBCNetwork() : base(ChannelName.NBC, StreamingProvider.None)
        {
        }

        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var nd = kvp.Value.Where(g => g.IsNotreDameHomeGame).FirstOrDefault();
                var b10 = kvp.Value.Where(g => !g.IsNotreDameHomeGame).FirstOrDefault();

                // nbc plays the last game on friday
                if (kvp.Key == 13)
                {
                    Primary.AssignGame(b10, kvp.Key, 7, 30, day: 4);
                    b10 = null;
                }

                // no nd game, b10 it is
                if (nd == null)
                {
                    Primary.AssignGame(b10, kvp.Key, 8, 0);
                    continue;
                }

                // nd plays at night if it's a premier game
                if (nd.BothTeamsRanked)
                {
                    Primary.AssignGame(nd, kvp.Key, 8, 0);
                    continue;
                }

                // nd is early, big 10 late
                Primary.AssignGame(nd, kvp.Key, 3, 30);
                Primary.AssignGame(b10, kvp.Key, 8, 00);
            }

            // we might need to put games back into the pool
            this.SelectedGames.ReturnInventory();
            return this;
        }

        public override bool PreassignGame(TelevisedGame game)
        {
            // Shamrock Series
            if (game.GTOD == 1207)
            {
                return !Primary.PreassignGame(game, new TimeSlot(8, 0, game.Week, day: game.Day), false);
            }

            // the rest of the notre dame home games come to NBC.  Stan vs ND and USC vs ND are always at 330.  
            if ((game.CheckMatchup(68, 87) && game.IsNotreDameHomeGame) || (game.CheckMatchup(68, 102) && game.IsNotreDameHomeGame) || (game.IsNotreDameHomeGame && game.AwayTeam.IsG5()))
            {
                return !Primary.PreassignGame(game, new TimeSlot(3, 30, game.Week), true);
            }

            // ND vs P5 goes at night
            if (game.IsP5Game && game.IsNotreDameHomeGame)
            {
                return !Primary.PreassignGame(game, new TimeSlot(8, 0, game.Week), true);
            }

            // first big 10 game observed gets assigned to black friday
            if (game.Week == 13 && game.IsBig10Game)
            {
                return !Primary.PreassignGame(game, new TimeSlot(7, 30, game.Week, day: 4), true);
            }

            // otherwise a big 10 game goes at night
            if (game.IsBig10Game)
            {
                return !Primary.PreassignGame(game, new TimeSlot(8, 0, game.Week), true);
            }

            return base.PreassignGame(game);
        }
    }
}