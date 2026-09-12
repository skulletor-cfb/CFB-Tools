using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EA_DB_Editor.Scheduling
{
    public class GameSelections
    {
        private Dictionary<int, List<TelevisedGame>> weeklySchedule = null;
        private List<TelevisedGame> games = new List<TelevisedGame>();

        public IEnumerable<TelevisedGame> Games => games;

        public void Select(TelevisedGame game)
        {
            games.Add(game.Select());
        }

        public void Select(IEnumerable<TelevisedGame> games)
        {
            foreach (var game in games)
            {
                Select(game);
            }
        }

        public Dictionary<int, List<TelevisedGame>> WeeklySchedule
        {
            get
            {
                if (weeklySchedule == null)
                {
                    weeklySchedule = this.games.GroupBy(g => g.Week).ToDictionary(g => g.Key, g => g.OrderBy(game => game.Score).ToList());
                }

                return weeklySchedule;
            }
        }

        public TelevisedGameQueue ToAssignmentQueue() => this.games.Where(g => !g.Assigned).ToQueue();

        public void ReturnInventory()
        {
            foreach (var game in games.Where(g => !g.Assigned))
            {
                game.Deselect();
            }
        }
    }

    public abstract class NetworkSchedule
    {
        public ChannelName Name { get; }

        protected GameSelections SelectedGames { get; } = new GameSelections();

        protected Dictionary<int, List<TelevisedGame>> WeeklySchedule => SelectedGames.WeeklySchedule;

        public virtual bool PreassignGame(TelevisedGame game)
        {
            return !game.Assigned;
        }

        public virtual void Offer(IEnumerable<TelevisedGame> games)
        {
            this.SelectedGames.Select(games);
        }

        public virtual void Offer(TelevisedGame game)
        {
            this.SelectedGames.Select(game);
        }

        protected ChannelSchedule Primary { get; }

        public StreamingSchedule Streaming { get; }

        protected NetworkSchedule(ChannelName name, StreamingProvider provider)
        {
            this.Name = name;
            this.Streaming = StreamingSchedule.Create(provider);
            this.Primary = ChannelSchedule.Create(name);
            TelevisionScheduler.Register(Primary);
        }

        public abstract NetworkSchedule AssignGames();

        public virtual void AssignStreaming()
        {
        }

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