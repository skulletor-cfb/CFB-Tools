#nullable disable
namespace ScheduleMaker.TV
{
    public enum ChannelName
    {
        ABC,
        ACCNetwork,
        BigTenNetwork,
        CBSSportsNetwork,
        CBS,
        CW,
        ESPN2,
        ESPN,
        ESPNU,
        FOX,
        FoxSports1,
        NBC,
        SECNetwork,
        TheMW,
        Peacock,
    }

    public interface ITelelvisionProvider
    {
        bool AssignGame(TelevisedGame game, TimeSlot timeslot);
    }

    public class ChannelSchedule : ITelelvisionProvider
    {
        private Dictionary<TimeSlot, TelevisedGame> schedule = new Dictionary<TimeSlot, TelevisedGame>();

        public ChannelName Name { get; }
        private ChannelSchedule(ChannelName name)
        {
            Name = name;
        }

        public static ChannelSchedule Create(ChannelName name)
        {
            var schedule = new ChannelSchedule(name);
            return schedule.Register();
        }

        public bool IsTimeslotAvailable(TimeSlot ts) => schedule.ContainsKey(ts) == false;

        public IEnumerable<(TimeSlot time, TelevisedGame game)> Games => this.schedule.Select(kvp => (kvp.Key, kvp.Value));

        public bool AssignGame(TelevisedGame game, int week, int hour, int minute, int day = 5)
        {
            return AssignGame(game, new TimeSlot(hour, minute, week, day: day));
        }

        /// <summary>
        /// will try to assign the game
        /// </summary>
        public bool AssignGame(TelevisedGame game, TimeSlot timeslot)
        {
            if (game == null || schedule.ContainsKey(timeslot) || game.Assigned || !game.CanPlayOn(timeslot))
            {
                return false;
            }

            schedule[timeslot] = game.Assign(timeslot);
            return true;
        }

        public bool PreassignGame(TelevisedGame game, TimeSlot timeslot, bool setGameTime)
        {
            if (schedule.ContainsKey(timeslot) || !game.CanPlayOn(timeslot))
            {
                return false;
            }

            schedule[timeslot] = game.PreAssign(setGameTime ? timeslot : null);
            return game.Assigned;
        }
    }
}
