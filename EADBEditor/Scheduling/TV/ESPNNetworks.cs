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

        public Dictionary<TimeSlot, TelevisedGame> ESPNU = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> ACCN = new Dictionary<TimeSlot, TelevisedGame>();
        public Dictionary<TimeSlot, TelevisedGame> SECN = new Dictionary<TimeSlot, TelevisedGame>();
        private ESPNNetworks() : base("ESPN")
        {
        }

        public override void Report()
        {
            WriteReport("abc", ABC);
            WriteReport("espn", ESPN);
            WriteReport("espn2", ESPN2);
            WriteReport("espnu", ESPNU);
            WriteReport("accn", ACCN);
            WriteReport("secn", SECN);
        }

        public override NetworkSchedule AssignGames()
        {
            AssignMWCAfterDark();
            AssignSecGamesOfTheWeek();
            AssignP5ESPN();
            AssignABCNoon();
            AssignACCFriday();
            AssignMidMajorThursday();
            AssignACCNetwork(new[] { new TimeSlot(3, 30) });
            AssignSECNetwork(new[] { new TimeSlot(4, 15) });
            AssignP5ESPN_NoonGames();
            AssignACCNetwork(new[] { new TimeSlot(12, 0), new TimeSlot(7, 30), });
            AssignSECNetwork(new[] { new TimeSlot(12, 45), new TimeSlot(7, 45), });
            return this;
        }

        /// <summary>
        /// SECN gets 1245/415/745 games
        /// </summary>
        private void AssignSECNetwork(params TimeSlot[] slots)
        {

            for (int i = 0; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsSecGame).OrderBy(g => g.Score).ToQueue();

                var stack = new Stack<TimeSlot>(slots);

                while (queue.TryDequeueGame(out var game))
                {
                    if (!stack.TryPop(out var timeslot))
                    {
                        break;
                    }

                    SECN.AssignGame(game, new TimeSlot(timeslot.Hour, timeslot.Minute, i, timeslot.AM, timeslot.Day));
                }
            }
        }

        /// <summary>
        /// ACCN gets 12/330/730 games
        /// </summary>
        private void AssignACCNetwork(params TimeSlot[] slots)
        {

            for (int i = 0; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsAccGame).OrderBy(g => g.Score).ToQueue();
                var stack = new Stack<TimeSlot>(slots);

                while (queue.TryDequeueGame(out var game))
                {
                    if (!stack.TryPop(out var timeslot))
                    {
                        break;
                    }

                    ACCN.AssignGame(game, new TimeSlot(timeslot.Hour, timeslot.Minute, i, timeslot.AM, timeslot.Day));
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
                    if (queue.TryDequeueGame(out var game))
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

        // the best acc game left is one friday 8pm
        private void AssignACCFriday()
        {
            // we start the friday after labor day
            var fridayStarts = TelevisionScheduler.LaborDayWeek()+1;

            for (int i = fridayStarts; i <= 12; i++)
            {
                // look for conference game first
                var queue = this.WeeklySchedule[i]
                    .Where(g => g.IsAccGame && g.IsConferenceGame).ToQueue();

                // any acc hosted game
                queue.Enqueue(this.WeeklySchedule[i].Where(g => g.IsAccGame));

                if(queue.TryExhaustiveDequeue(out var game))
                {
                    ESPN.AssignGame(game, i, 8, 0, 4);
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

                if (queue.TryExhaustiveDequeue(out var game))
                {
                    ABC.AssignGame(game, i, 12, 0);
                }
            }

            // sec games first
            for (int i = 5; i <= 13; i++)
            {
                var queue = this.WeeklySchedule[i].Where(g => !g.Assigned && g.IsSecConferenceGame).OrderBy(g => g.Score).ToQueue();
                queue.Enqueue(this.WeeklySchedule[i].Where(g => !g.IsMWCGame));

                if (queue.TryExhaustiveDequeue(out var game))
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

                if (games.TryExhaustiveDequeue(out var game))
                {
                    ESPN.AssignGame(game, i, 12, 00);
                }

                if (games.TryExhaustiveDequeue(out game))
                {
                    ESPN2.AssignGame(game, i, 1, 0);
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

                if (games.TryExhaustiveDequeue(out var game))
                {
                    ESPN.AssignGame(game, i, 7, 0);
                }

                if (games.TryExhaustiveDequeue(out game))
                {
                    ESPN.AssignGame(game, i, 3, 30);
                }

                if (games.TryExhaustiveDequeue(out game))
                {
                    ESPN2.AssignGame(game, i, 4, 30);
                }

                if (games.TryExhaustiveDequeue(out game))
                {
                    ESPN2.AssignGame(game, i, 8, 30);
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

                if (secConferenceGames.TryExhaustiveDequeue(out var gotw))
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

                    if (secGames.TryExhaustiveDequeue(out var primetime))
                    {
                        ABC.AssignGame(primetime, secondarySlot);
                    }
                    else if (secConferenceGames.TryExhaustiveDequeue(out primetime))
                    {
                        ABC.AssignGame(primetime, secondarySlot);
                    }
                }
                else
                {
                    if (secGames.TryExhaustiveDequeue(out var primetime))
                    {
                        ABC.AssignGame(primetime, i, 7, 30);
                    }

                    if (secGames.TryExhaustiveDequeue(out gotw))
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
            this.SelectedGames.AddRange(big12Games.Take(4).Select(g => g.Select()));

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

            // all american, sun belt, cusa, mac games
            this.SelectedGames.AddRange(televisedGames.Values.SelectMany(g => g).Where(g => !g.Selected && !g.HomeTeamIsP5).Select(g => g.Select()));
        }
    }
}