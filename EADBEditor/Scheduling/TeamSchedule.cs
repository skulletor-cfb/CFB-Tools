using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class TeamSchedule : IEnumerable<PreseasonScheduledGame>
    {
        public const int ScheduleLimit = 15;
        private readonly PreseasonScheduledGame[] games = new PreseasonScheduledGame[ScheduleLimit * 2];
        private readonly bool isFcsTeam;

        private int gameOverflow = 14;

        public TeamSchedule(bool isFcsTeam = false, bool hasWeek14Games = false)
        {
            this.isFcsTeam = isFcsTeam;
            this.gameOverflow = hasWeek14Games ? 15 : 14;
            
        }

        public int Length => this.games.Length;

        public PreseasonScheduledGame this[int index]
        {
            get => this.games[index];
            set
            {
                if (this.isFcsTeam) return;

                this.games[index] = value;
            }
        }

        public PreseasonScheduledGame[] GetAllConferenceGames()
        {
            return this.games.Where(g => g?.IsConferenceGame() ?? false).Distinct().ToArray();
        }

        public void AddUnscheduledGame(PreseasonScheduledGame game, int gameNumber)
        {
            game.SetWeek(gameOverflow++);
            game.GameNumber = gameNumber;
            games[game.WeekIndex] = game;
        }

        public List<int> FindOpenWeeks(int? butNot = null)
        {
            var list = new List<int>();

            for (int i = 0; i < ScheduleLimit; i++)
            {
                if (games[i] == null)
                {
                    if (butNot == null || butNot.Value != i)
                        list.Add(i);
                }
            }

            return list;
        }

        public int FindLastOpenWeekForFcs()
        {
            return FindOpenWeeks(14).Last();
        }

        public int FindNextOpenWeek(int notThisWeek)
        {

            for (int i = notThisWeek + 1; i < ScheduleLimit; i++)
            {
                if (games[i] == null)
                    return i;
            }

            for (int i = 0; i < notThisWeek; i++)
            {
                if (games[i] == null)
                    return i;
            }

            return -1;
        }

        public void MoveFcsGame(int fromWeek, int toWeek)
        {
            games[fromWeek].SetWeek(toWeek);
            games[toWeek] = games[fromWeek];
            games[fromWeek] = null;
        }

        public IEnumerator<PreseasonScheduledGame> GetEnumerator() => ((IEnumerable<PreseasonScheduledGame>)this.games).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.games.GetEnumerator();
    }
}