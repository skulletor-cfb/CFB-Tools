using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling.TV
{
    public class NBCNetwork:NetworkSchedule
    {
        public static readonly NBCNetwork Instance = new NBCNetwork();
        private NBCNetwork() : base("NBC")
        {
        }

        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule)
            {
                var nd = kvp.Value.Where(g => g.IsNotreDameHomeGame).FirstOrDefault();
                var b10 = kvp.Value.Where(g => !g.IsNotreDameHomeGame).FirstOrDefault();

                // no nd game, b10 it is
                if (nd == null)
                {
                    Primary.AssignGame(b10, kvp.Key, 8, 0);
                    continue;
                }

                // shamrock series is primetime, no big 10 game
                if (nd.IsShamrockSeries)
                {
                    Primary.AssignGame(nd, kvp.Key, 8, 7);
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

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // every week get the 2nd best Big 10 game
            var big10 = televisedGames[TableUtility.Big10Id].GetAvailableGamesByWeek();
            foreach (var kvp in big10)
            {
                if (kvp.Value.Count > 1)
                {
                    this.SelectedGames.Add(kvp.Value[1].Select());
                }
            }

            // all the notre dame games
            this.SelectedGames.AddRange(televisedGames[TableUtility.NotreDameId].Select(g => g.Select()));
        }
    }
}
