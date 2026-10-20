using Newtonsoft.Json;
using System.Collections.Generic;

namespace NCAA14Utility
{
    public class ScheduleNode
    {
        public int Week { get; set; }
        public int Day { get; set; }
        public int OpponentId { get; set; }
        public string Opponent { get; set; }
        public bool IsConferenceGame { get; set; }
        public bool IsP5Game { get; set; }
        public bool IsG5Game { get; set; }
        public bool IsLaborDayGame { get; set; }
        public bool IsHomeGame { get; set; }
    }

    public class TeamSeasonSchedule
    {
        private Dictionary<int, ScheduleNode> scheduleNodesByWeek;
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamAbbreviation { get; set; }
        public List<ScheduleNode> ScheduleNodes { get; set; } = new List<ScheduleNode>();

        [JsonIgnore]
        public Dictionary<int, ScheduleNode> ScheduleNodesByWeek
        {
            get
            {
                if(scheduleNodesByWeek == null)
                {
                    scheduleNodesByWeek = new Dictionary<int, ScheduleNode>();
                    foreach (var node in ScheduleNodes)
                    {
                        scheduleNodesByWeek[node.Week] = node;
                    }
                }

                return scheduleNodesByWeek;
            }
        }

        public List<string> Notes { get; set; } = new List<string>();
        public string G5Note { get; set; } = string.Empty;
        public int ConferenceId { get; set; }
    }

    public class ScheduleModel
    {
        public List<TeamSeasonSchedule> TeamSchedules { get; set; } = new List<TeamSeasonSchedule>();
        public string FCSGames { get; set; }
        public string G5Games { get; set; }
        public string P5Games { get; set; }
        public string P5G5Games { get; set; }
        public string TotalGames { get; set; }
    }
}