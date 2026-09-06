using EA_DB_Editor.Scheduling.TV;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EA_DB_Editor.Scheduling
{
    /// <summary>
    /// ESPN, ABC, ESPN2, SEC Network, ACC Network, ESPNU, ESPN+
    /// </summary>
    public class ESPNNetworks : NetworkSchedule
    {
        public static readonly ESPNNetworks Instance = new ESPNNetworks();

        public Dictionary<TimeSlot, TelevisedGame> ABC = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> ESPN = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> ESPN2 = new Dictionary<TimeSlot, TelevisedGame>();
        private ESPNNetworks() : base("ESPN")
        {
            //build ABC, they have 12, 3:30, 7:30 each saturday
            for (int i = 0; i <= 13; i++)
            {
                var noon = new TimeSlot(12, 0, i);
                var secGotw = new TimeSlot(3, 30, i);
                var espnPrimetime = new TimeSlot(7, 00, i);
                var primetime = new TimeSlot(7, 30, i);
                var eightPM = new TimeSlot(8, 00, i);
                var afterDark = new TimeSlot(10, 30, i);
                ABC.Add(noon, null);
                ABC.Add(secGotw, null);
                ABC.Add(primetime, null);
                ESPN.Add(noon, null);
                ESPN.Add(secGotw, null);
                ESPN.Add(espnPrimetime, null);
                ESPN.Add(afterDark, null);
                ESPN2.Add(noon, null);
                ESPN2.Add(secGotw, null);
                ESPN2.Add(eightPM, null);
            }
        }

        public override void Report()
        {
            WriteReport("abc", ABC);
            WriteReport("espn", ESPN);
            WriteReport("espn2", ESPN2);
        }

        public override NetworkSchedule AssignGames()
        {
            AssignMWCAfterDark();
            AssignSecGamesOfTheWeek();
            AssignP5ESPN();
            return this;
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

                if (games.TryDequeue(out var game))
                {
                    ESPN.AssignGame(game, i, 7, 0);
                }

                if (games.TryDequeue(out game))
                {
                    ESPN2.AssignGame(game, i, 8, 0);
                }

                if (games.TryDequeue(out game))
                {
                    ESPN.AssignGame(game, i, 3, 30);
                }

                if (games.TryDequeue(out game))
                {
                    ESPN2.AssignGame(game, i, 3, 30);
                }
            }
        }

        /// <summary>
        /// ESPN airs one MWC at 1030pm
        /// </summary>
        private void AssignMWCAfterDark()
        {
            for (int i = 0; i <= 13; i++)
            {
                var games = this.WeeklySchedule[i];
                var mwc = games.Where(g => !g.Assigned && g.ConferenceOwner == TableUtility.MWCId).OrderBy(g => g.Score).FirstOrDefault();

                if (mwc != null)
                {
                    ESPN.AssignGame(mwc, i, 10, 30);
                }
            }
        }

        /// <summary>
        /// 3:30 pm the top ranked SEC game conference game if there is one.  if LSU, it should be 7:30
        /// 7:30 pm the top ranked SEC intraconference game, if none, 2nd best SEC conference game
        /// </summary>
        private void AssignSecGamesOfTheWeek()
        {
            for (int i = 0; i <= 13; i++)
            {
                // top sec conference game
                var games = this.WeeklySchedule[i];
                var secGames = games.Where(g => !g.Assigned && !g.IsSecConferenceGame && ((g.ConferenceOwner == TableUtility.SECId && g.IsP5Game) || g.IsSecAccGame)).OrderBy(g => g.Score).ToQueue();

                // top one goes to 330 unless its LSU
                var secConferenceGames = games.Where(g => !g.Assigned && g.IsSecConferenceGame).OrderBy(g => g.Score).ToQueue();

                if (secConferenceGames.TryDequeue(out var gotw))
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

                    if (secGames.TryDequeue(out var primetime))
                    {
                        ABC.AssignGame(primetime, secondarySlot);
                    }
                    else if (secConferenceGames.TryDequeue(out primetime))
                    {
                        ABC.AssignGame(primetime, secondarySlot);
                    }
                }
                else
                {
                    if (secGames.TryDequeue(out var primetime))
                    {
                        ABC.AssignGame(primetime, i, 7, 30);
                    }

                    if (secGames.TryDequeue(out gotw))
                    {
                        ABC.AssignGame(gotw, i, 3, 30);
                    }
                }
            }
        }

        public override void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames)
        {
            // take all sec games
            this.SelectedGames.AddRange(televisedGames[TableUtility.SECId].Select(g => g.Select()));

            // take the unselected acc games
            this.SelectedGames.AddRange(televisedGames[TableUtility.ACCId].Where(g => !g.Selected).Select(g => g.Select()));

            // for big 12 espn gets half in the cadnce 0-3, 5, 7, 9, 11, 13, 15, 17, 19 ...
            var big12Games = televisedGames[TableUtility.Big12Id];
            this.SelectedGames.AddRange(big12Games.Take(4).Select(g=>g.Select()));

            // remove the first 4 and last 4 from big 12 games and assign half
            var big12OnESPN = big12Games.Skip(4).Take(big12Games.Count - 8).ToArray();
            for (int i = 1; i < big12OnESPN.Length; i += 2)
            {
                this.SelectedGames.Add(big12OnESPN[i].Select());
            }

            // espn takes the top MWC game for the 10:30pm slot
            var mwcGames = televisedGames[TableUtility.MWCId].GetAvailableGamesByWeek();
            foreach (var kvp in mwcGames)
            {
                this.SelectedGames.Add(kvp.Value[0].Select());
            }
        }
    }
}