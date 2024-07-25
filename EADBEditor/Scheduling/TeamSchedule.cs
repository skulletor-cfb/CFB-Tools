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
        private readonly List<PreseasonScheduledGame> games = new List<PreseasonScheduledGame>(15);

        private readonly List<PreseasonScheduledGame> unscheduledGames = new List<PreseasonScheduledGame>();

        private readonly bool isFcsTeam;

        public  TeamSchedule(bool isFcsTeam = false)
        {
            this.isFcsTeam = isFcsTeam;
        }

        public int Length => this.games.Count;

        public PreseasonScheduledGame this[int index]
        {
            get => this.games[index];
            set
            {
                if (this.isFcsTeam) return;

                if (index > 14)
                {
                    throw new InvalidOperationException($"Unable to set week to {index}");
                }

                this.games[index] = value;
            }
        }

        public IEnumerator<PreseasonScheduledGame> GetEnumerator() => this.games.GetEnumerator();

        public List<int> FindOpenWeeks(int? butNot = null)
        {
            var list = new List<int>();

            for (int i = 0; i < games.Count; i++)
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

            for (int i = notThisWeek + 1; i < games.Count; i++)
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

        IEnumerator IEnumerable.GetEnumerator() => this.games.GetEnumerator();
    }
}