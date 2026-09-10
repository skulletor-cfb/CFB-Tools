using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
{
    public class NBCNetwork:NetworkSchedule
    {
        public static readonly NBCNetwork Instance = new NBCNetwork();
        private NBCNetwork() : base(ChannelName.NBC, StreamingProvider.Peacock)
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
            if (game.GTOD == 1207 )
            {
                Primary.PreassignGame(game, new TimeSlot(8, 0, game.Week, day: game.Day));
                return false;
            }
            return base.PreassignGame(game);
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // every week get the 2nd best Big 10 game
            var big10 = televisedGames[TableUtility.Big10Id].GetAvailableGamesByWeek();
            foreach (var kvp in big10)
            {
                if (kvp.Value.Count > 1)
                {
                    this.SelectedGames.Select(kvp.Value[1]);
                }
            }

            // all the notre dame games
            this.SelectedGames.Select(televisedGames[TableUtility.NotreDameId]);
        }
    }
}
