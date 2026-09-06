using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling.TV
{
    public class FoxNetworks:NetworkSchedule
    {
        public static readonly FoxNetworks Instance = new FoxNetworks();

        public Dictionary<TimeSlot, TelevisedGame> FOX = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> FS1 = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> BTN = new Dictionary<TimeSlot, TelevisedGame>();
        private FoxNetworks() : base("FOX")
        {
        }

        public override NetworkSchedule AssignGames()
        {
            throw new NotImplementedException();
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // take the rest of the big 12 games
            this.SelectedGames.AddRange(televisedGames[TableUtility.Big12Id].Where(g => !g.Selected).Select(g => g.Select()));

            // take the rest of pac 12 games
            this.SelectedGames.AddRange(televisedGames[TableUtility.Pac16Id].Where(g => !g.Selected).Select(g => g.Select()));

            // every week get the top fox game
            var big10 = televisedGames[TableUtility.Big10Id].GetAvailableGamesByWeek();
            foreach (var kvp in big10)
            {
                this.SelectedGames.Add(kvp.Value[0].Select());
            }
        }
    }
}
