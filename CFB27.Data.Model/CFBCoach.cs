using MaddenFranchise;
using System;

namespace CFB27.Data.Model
{
    public class CFBCoach : BaseRecord
    {
        /// <summary>
        /// Reads the coach plus the career totals from its CareerCoachStats row (wins, bowls, titles live there, not on Coach).
        /// </summary>
        public static CFBCoach Transform(FranchiseFile dynasty, FranchiseFileRecord record)
        {
            var coach = new CFBCoach
            {
                Row = record.Index,
                IsEmpty = record.IsEmpty,
            };

            if (record.IsEmpty)
            {
                return coach;
            }

            coach.FirstName = record["FirstName"] as string;
            coach.LastName = record["LastName"] as string;
            coach.TeamIndex = Convert.ToInt32(record["TeamIndex"]);
            coach.Position = record["Position"] as string;
            coach.Age = Convert.ToInt32(record["Age"]);
            coach.YearsCoaching = Convert.ToInt32(record["YearsCoaching"]);
            coach.SeasonsWithTeam = Convert.ToInt32(record["SeasonsWithTeam"]);
            coach.ContractLength = Convert.ToInt32(record["ContractLength"]);
            coach.ContractYearsRemaining = Convert.ToInt32(record["ContractYearsRemaining"]);
            coach.Level = Convert.ToInt32(record["Level"]);
            coach.ExperiencePoints = Convert.ToInt32(record["ExperiencePoints"]);
            coach.CareerLongWinStreak = Convert.ToInt32(record["CareerLongWinStreak"]);
            coach.CoachPrestige = record["CoachPrestige"] as string;
            coach.AlmaMater = Convert.ToInt32(record["AlmaMater"]);
            coach.Portrait = Convert.ToInt32(record["Portrait"]);
            coach.OffensivePlaybook = CFBPlaybook.Find(record["OffensivePlaybook"] as string);
            coach.DefensivePlaybook = CFBPlaybook.Find(record["DefensivePlaybook"] as string);

            var career = dynasty.Follow(record.GetReferenceDataByKey("CareerStats").Value);
            if (career != null)
            {
                coach.CareerWins = Convert.ToInt32(career["Wins"]);
                coach.CareerLosses = Convert.ToInt32(career["Losses"]);
                coach.WinsAtCurrentSchool = Convert.ToInt32(career["WinsAtCurrentSchool"]);
                coach.LossesAtCurrentSchool = Convert.ToInt32(career["LossesAtCurrentSchool"]);
                coach.BowlWins = Convert.ToInt32(career["BowlWins"]);
                coach.BowlLosses = Convert.ToInt32(career["BowlLosses"]);
                coach.ConfChampWins = Convert.ToInt32(career["ConfChampWins"]);
                coach.NCWins = Convert.ToInt32(career["NCWins"]);
                coach.Top25Wins = Convert.ToInt32(career["Top25Wins"]);
                coach.Top25Losses = Convert.ToInt32(career["Top25Losses"]);
                coach.RivalWins = Convert.ToInt32(career["RivalWins"]);
                coach.RivalLosses = Convert.ToInt32(career["RivalLosses"]);
            }

            return coach;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TeamIndex { get; set; }

        /// <summary>CoachPosition enum name: HeadCoach, OffensiveCoordinator or DefensiveCoordinator</summary>
        public string Position { get; set; }

        public int Age { get; set; }
        public int YearsCoaching { get; set; }

        /// <summary>includes the current season (1 in a coach's first year)</summary>
        public int SeasonsWithTeam { get; set; }

        public int ContractLength { get; set; }
        public int ContractYearsRemaining { get; set; }
        public int Level { get; set; }
        public int ExperiencePoints { get; set; }
        public int CareerLongWinStreak { get; set; }

        /// <summary>LetterGrade enum name, e.g. Bminus</summary>
        public string CoachPrestige { get; set; }

        /// <summary>
        /// Team.TEAM_ORIGID minus <see cref="AlmaMaterOrigIdOffset"/> (Deion Sanders 32 = Florida State 1132), not a TeamIndex.
        /// Coaches from non-FBS schools appear to get their current school (Ryan Day = Ohio State).
        /// </summary>
        public int AlmaMater { get; set; }

        public const int AlmaMaterOrigIdOffset = 1100;

        public int Portrait { get; set; }

        /// <summary>The coach's playbooks (asset references in the save), or null when he has none.</summary>
        public CFBPlaybook OffensivePlaybook { get; set; }
        public CFBPlaybook DefensivePlaybook { get; set; }

        // CareerCoachStats
        public int CareerWins { get; set; }
        public int CareerLosses { get; set; }
        public int WinsAtCurrentSchool { get; set; }
        public int LossesAtCurrentSchool { get; set; }
        public int BowlWins { get; set; }
        public int BowlLosses { get; set; }
        public int ConfChampWins { get; set; }
        public int NCWins { get; set; }
        public int Top25Wins { get; set; }
        public int Top25Losses { get; set; }
        public int RivalWins { get; set; }
        public int RivalLosses { get; set; }

        public bool IsOnTeam => !this.IsEmpty && this.TeamIndex != CFBPlayer.NoTeamIndex;

        /// <summary>NCAA 14 coach position: 0 head coach, 1 offensive coordinator, 2 defensive coordinator, -1 anything else</summary>
        public int PositionId
        {
            get
            {
                switch (this.Position)
                {
                    case "HeadCoach": return 0;
                    case "OffensiveCoordinator": return 1;
                    case "DefensiveCoordinator": return 2;
                    default: return -1;
                }
            }
        }

        public int TeamId => CFBTeam.TeamIndexToOldIdMap[this.TeamIndex];
    }
}
