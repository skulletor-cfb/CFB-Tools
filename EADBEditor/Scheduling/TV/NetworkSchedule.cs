using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace EA_DB_Editor.Scheduling
{
    public abstract class NetworkSchedule
    {
        private Dictionary<int, List<TelevisedGame>> weeklySchedule = null;
        public string Name { get; }

        protected List<TelevisedGame> SelectedGames { get; } = new List<TelevisedGame>();

        protected Dictionary<int, List<TelevisedGame>> WeeklySchedule
        {
            get
            {
                if (weeklySchedule == null)
                {
                    weeklySchedule = this.SelectedGames.GroupBy(g => g.Week).ToDictionary(g => g.Key, g => g.OrderBy(game => game.Score).ToList());
                }

                return weeklySchedule;
            }
        }

        protected Dictionary<TimeSlot, TelevisedGame> Primary = new Dictionary<TimeSlot, TelevisedGame>();

        protected NetworkSchedule(string name)
        {
            this.Name = name;
        }

        public abstract void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames);

        public abstract NetworkSchedule AssignGames();

        public virtual void Report()
        {
            WriteReport(this.Name, Primary);
        }

        protected void WriteReport(string file, Dictionary<TimeSlot, TelevisedGame> network)
        {
            var sb = new StringBuilder();
            foreach (var kvp in network.OrderBy(k => k.Key.Week).ThenBy(k => k.Key.Day).ThenBy(k => k.Key.GTOD))
            {
                sb.AppendLine($"{kvp.Key.ToString()} - {kvp.Value?.AwayTeam} at {kvp.Value?.HomeTeam}");
            }

            File.WriteAllText($"{file}-tv-debug.log", sb.ToString());
        }

        protected void WriteReport(string file, List<(TimeSlot time, TelevisedGame game)> streaming)
        {
            var sb = new StringBuilder();

            foreach (var (time, game) in streaming.OrderBy(g => g.time.Week).ThenBy(g => g.time.Day).ThenBy(g => g.time.GTOD))
            {
                sb.AppendLine($"{time} - {game.AwayTeam} at {game.HomeTeam}");
            }

            File.WriteAllText($"{file}-tv-debug.log", sb.ToString());
        }
    }
}