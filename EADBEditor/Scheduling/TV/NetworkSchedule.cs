using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace EA_DB_Editor.Scheduling
{
    public abstract class NetworkSchedule
    {
        public string Name { get; }

        protected List<TelevisedGame> SelectedGames { get; } = new List<TelevisedGame>();

        protected Dictionary<int, List<TelevisedGame>> WeeklySchedule { get; set; }

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
            foreach (var kvp in network.OrderBy(k => k.Key.Week).ThenBy(k => k.Key.GTOD))
            {
                sb.AppendLine($"{kvp.Key.Week}-{kvp.Key.GTOD}: {kvp.Value?.AwayTeam} at {kvp.Value?.HomeTeam}");
            }

            File.WriteAllText($"{file}-tv-debug.log", sb.ToString());
        }

    }
}