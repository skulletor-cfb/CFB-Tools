namespace CFB27.Data.Model
{
    public class CFBSeasonInfo : BaseRecord
    {
        public int CurrentSeasonYear { get; set; }
        public int BaseCalendarYear { get; set; }
        public int CurrentWeek { get; set; }
        public string CurrentWeekType { get; set; }
        public string CurrentStage { get; set; }
        public int RegularSeasonLastWeekScheduled { get; set; }
        public int RegularSeasonWeekConferenceChampionship { get; set; }
        public int PostSeasonNumWeeks { get; set; }
    }
}
