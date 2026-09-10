using EA_DB_Editor.Scheduling.TV;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    public abstract class NetworkSchedule
    {
        private Dictionary<int, List<TelevisedGame>> weeklySchedule = null;
        public ChannelName Name { get; }

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
        public virtual void SubLicense(TelevisedGame game, TimeSlot slot)
        {
            Primary.AssignGame(game, slot);
        }

        public virtual bool PreassignGame(TelevisedGame game)
        {
            return !game.Assigned;
        }


        protected ChannelSchedule Primary { get; }

        public StreamingSchedule Streaming { get; }

        protected NetworkSchedule(ChannelName name, StreamingProvider provider)
        {
            this.Name = name;
            this.Streaming = new StreamingSchedule(provider);
            this.Primary = new ChannelSchedule(name);
            TelevisionScheduler.Register(Primary);
        }

        public abstract void SelectGames(Dictionary<int, List<TelevisedGame>> televisedGames);

        public abstract NetworkSchedule AssignGames();

        public virtual void Report()
        {
            WriteReport(Primary);
            WriteReport(Streaming);
        }

        protected void WriteReport(ChannelSchedule network)
        {
            var report = network.WriteReport();
            File.WriteAllText($"{network.Name}-tv-debug.log", report);
        }

        protected void WriteReport(StreamingSchedule streaming)
        {
            var report = streaming.WriteReport();
            File.WriteAllText($"{streaming.Provider}-tv-debug.log", report);
        }
    }
}