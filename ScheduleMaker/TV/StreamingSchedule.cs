#nullable disable
namespace ScheduleMaker.TV
{
    public enum StreamingProvider
    {
        None,
        ESPNPlus,
        FoxOne,
        ParamountPlus,
        Peacock,
        MWPlus,
        ACCNX,
    }

    public class StreamingSchedule : ITelelvisionProvider
    {
        public StreamingProvider Provider { get; }
        private List<(TimeSlot time, TelevisedGame game)> schedule = new List<(TimeSlot time, TelevisedGame game)>();
        public void AssignGame(TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        private StreamingSchedule(StreamingProvider provider)
        {
            this.Provider = provider;
        }

        public IEnumerable<(TimeSlot time, TelevisedGame game)> Games => this.schedule;

        public static StreamingSchedule Create(StreamingProvider provider)
        {
            return new StreamingSchedule(provider).Register();
        }

        public bool AssignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null || game.Assigned || !game.CanPlayOn(timeslot)) return false;
            schedule.Add((timeslot, game.Assign(timeslot)));
            return true;
        }

        /// <summary>Takes the game at the time it already has (nothing is written).</summary>
        public bool PreassignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game.Assigned || !game.CanPlayOn(timeslot)) return false;
            schedule.Add((timeslot, game.PreAssign()));
            return true;
        }
    }
}
