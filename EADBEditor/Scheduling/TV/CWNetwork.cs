using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling.TV
{
    public class CWNetwork : NetworkSchedule
    {
        private class WeekSchedule
        {
            public TimeSlot Early { get; set; }

            public TimeSlot Afternoon { get; set; }
            public TimeSlot Evening { get; set; }
            public TimeSlot Late { get; set; }
            public TimeSlot FridayNight { get; set; }
        }

        public static readonly CWNetwork Instance = new CWNetwork();
        private CWNetwork() : base("CW")
        {
        }

        /// <summary>
        /// mwc plays 
        /// hawaii game: 1230, 400, 730, 1130
        /// otherwise: 1200, 330, 7, 1030
        /// </summary>
        /// <returns></returns>
        public override NetworkSchedule AssignGames()
        {
            foreach (var kvp in this.WeeklySchedule.OrderBy(kvp => kvp.Key))
            {

                var games = kvp.Value.OrderBy(g => g.Score).ToArray();
                var queue = games.Where(g => !g.IsHawaiiGame).ToQueue();
                var hawaiiGame = games.Where(g => g.IsHawaiiGame).FirstOrDefault();
                var gamesThisWeek = games.Length;
                var week = kvp.Key;

                // default slots
                var schedule = new WeekSchedule();
                var afternoon = new TimeSlot(3, 30, week: week);
                var early = new TimeSlot(12, 0, week: week);
                var evening = new TimeSlot(7, 0, week: week);
                var late = new TimeSlot(10, 30, week: week);

                if (hawaiiGame != null)
                {
                    schedule.Late = new TimeSlot(11, 30, week: week);
                    Primary.AssignGame(hawaiiGame, schedule.Late);
                    afternoon = new TimeSlot(4, 0, week: week);
                    early = new TimeSlot(12, 30, week: week);
                    evening = new TimeSlot(7, 30, week: week);
                }

                while (queue.TryDequeue(out var game))
                {
                    // acc will be played either at 4pm or 12:30pm
                    if (game.IsAccGame)
                    {
                        if (gamesThisWeek <= 3)
                        {
                            schedule.Afternoon = afternoon;
                            Primary.AssignGame(game, schedule.Afternoon);
                        }
                        else
                        {
                            schedule.Early = early;
                            Primary.AssignGame(game, schedule.Early);
                        }

                        continue;
                    }

                    // premier game, so it picks first
                    if (schedule.Evening == null)
                    {
                        schedule.Evening = evening;
                        Primary.AssignGame(game, schedule.Evening);
                        continue;
                    }

                    if (schedule.Late == null)
                    {
                        schedule.Late = late;
                        Primary.AssignGame(game, schedule.Late);
                        continue;
                    }

                    if (schedule.Afternoon == null)
                    {
                        schedule.Afternoon = afternoon;
                        Primary.AssignGame(game, afternoon);
                        continue;
                    }

                    // friday night is the last game
                    Primary.AssignGame(game, new TimeSlot(9, 0, week: week, day: 4));
                }
            }

            return this;
        }

        /// <summary>
        /// gets 2 MWC games and worst ACC non fcs game
        /// </summary>
        /// <param name="games"></param>
        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // take the unselected acc games, 1 per week
            var accGames = televisedGames[TableUtility.ACCId].GetAvailableGamesByWeek(selector: g => !g.Selected && !g.IsFCSGame, orderFunc: g => -g.Score);
            foreach (var kvp in accGames)
            {
                var game = kvp.Value.Take(1).FirstOrDefault();

                if (game != null)
                {
                    this.SelectedGames.Add(game.Select());
                }
            }

            // take the 2nd best and 3rd mwc games of the week
            var mwcGames = televisedGames[TableUtility.MWCId].GetAvailableGamesByWeek();
            foreach (var kvp in mwcGames)
            {
                // espn takes the top game
                var mwc = kvp.Value.Skip(1).ToArray();

                if (mwc.Length > 0)
                {
                    this.SelectedGames.Add(mwc[0].Select());
                }

                if (mwc.Length > 1)
                {
                    this.SelectedGames.Add(mwc[1].Select());
                }

                // once we get into conf play, take a third
                if (mwc.Length > 2 && kvp.Key > 5)
                {
                    this.SelectedGames.Add(mwc[2].Select());
                }

                // later in the season we go friday
                if (mwc.Length > 3 && kvp.Key > 5)
                {
                    this.SelectedGames.Add(mwc[3].Select());
                }
            }
        }
    }
}