using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor.Scheduling.TV
{
    public class CBSNetwork : NetworkSchedule
    {
        public static readonly CBSNetwork Instance = new CBSNetwork();
        private CBSNetwork() : base("CBS")
        {
        }

        public override NetworkSchedule AssignGames()
        {
            var armyNavy = this.SelectedGames.Where(g => g.IsArmyNavy).First();
            Primary.AssignGame(armyNavy, new TimeSlot(3, 30, armyNavy.Week));

            var afArmy = this.SelectedGames.Where(g => g.IsArmyAirForce).First();
            Primary.AssignGame(afArmy, new TimeSlot(7, 30, afArmy.Week));

            var afNavy = this.SelectedGames.Where(g => g.IsAirForceNavy).First();
            Primary.AssignGame(afNavy, new TimeSlot(12, 30, afNavy.Week));

            foreach (var kvp in this.WeeklySchedule)
            {
                var queue = kvp.Value.ToQueue();

                while (queue.TryDequeueGame(out var game))
                {
                    if (game.IsBig10Game)
                    {
                        var slot = new TimeSlot(3, 30, week: kvp.Key);

                        if (!Primary.ContainsKey(slot))
                        {
                            Primary.AssignGame(game, slot);
                        }
                    }
                    else
                    {
                        var slot = new TimeSlot( 7, 30, week: kvp.Key);

                        if (!Primary.ContainsKey(slot))
                        {
                            Primary.AssignGame(game, slot);
                        }
                    }
                }
            }

            // we might need to put games back into the pool
            this.SelectedGames.ReturnInventory();
            return this;
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // cbs takes the military academy games
            this.SelectedGames.AddRange(televisedGames.Values.SelectMany(g => g).Where(g => g.IsMilitaryAcademyGame).Select(g => g.Select()));

            // every week get the third best Big 10 game
            var big10 = televisedGames[TableUtility.Big10Id].GetAvailableGamesByWeek();
            foreach (var kvp in big10)
            {
                if (kvp.Value.Count > 2)
                {
                    this.SelectedGames.Add(kvp.Value[2].Select());
                }
            }

            // every week get the best pac12 game
            var pac12 = televisedGames[TableUtility.Pac16Id].GetAvailableGamesByWeek();
            foreach (var kvp in pac12)
            {
                this.SelectedGames.Add(kvp.Value[0].Select());
            }
        }
    }
}