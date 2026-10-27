using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CFB27.Data.Model
{
    /// <summary>
    /// Where a coach was during the season, and his record then, so the end-of-season save can be rebuilt as "the coaches
    /// who coached this season". Coach table rows stay put from week 0 through National Signing Day (they're reused only
    /// at the rollover to the next preseason), but by the end of the season:
    /// - a coach who retired has TeamIndex 255 (ContractStatus Retired); his career and at-school stats are still there
    ///   and final;
    /// - a coach who took another job is already at his new school, and the hire reset WinsAtCurrentSchool,
    ///   LossesAtCurrentSchool and SeasonsWithTeam; his career stats are final.
    /// </summary>
    public class CoachSeasonStart
    {
        public int Row { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TeamIndex { get; set; }
        public string Position { get; set; }
        public int CareerWins { get; set; }
        public int CareerLosses { get; set; }
        public int WinsAtCurrentSchool { get; set; }
        public int LossesAtCurrentSchool { get; set; }
        public int SeasonsWithTeam { get; set; }

        public static List<CoachSeasonStart> Capture(CFBTable<CFBCoach> coaches) =>
            coaches.Records.Where(c => !c.IsEmpty).Select(c => new CoachSeasonStart
            {
                Row = c.Row,
                FirstName = c.FirstName,
                LastName = c.LastName,
                TeamIndex = c.TeamIndex,
                Position = c.Position,
                CareerWins = c.CareerWins,
                CareerLosses = c.CareerLosses,
                WinsAtCurrentSchool = c.WinsAtCurrentSchool,
                LossesAtCurrentSchool = c.LossesAtCurrentSchool,
                SeasonsWithTeam = c.SeasonsWithTeam,
            }).ToList();

        public static void Write(CFBTable<CFBCoach> coaches, string file) =>
            File.WriteAllText(file, JsonConvert.SerializeObject(Capture(coaches), Formatting.Indented));

        public static List<CoachSeasonStart> Read(string file) =>
            JsonConvert.DeserializeObject<List<CoachSeasonStart>>(File.ReadAllText(file));

        /// <summary>
        /// Puts every coach back on the staff he coached the season with, keeping his final stats from the save. A coach
        /// whose at-school record was reset by a new job gets it rebuilt: the season-start record plus what he won and lost
        /// since (his career record now minus then), and one more season with the team. A coach the cache doesn't know
        /// (or a row now holding someone else) didn't coach the season, so he's taken off staff.
        /// </summary>
        public static void Restore(CFBTable<CFBCoach> coaches, IEnumerable<CoachSeasonStart> seasonStart)
        {
            var byRow = seasonStart.ToDictionary(s => s.Row);
            foreach (var coach in coaches.Records.Where(c => !c.IsEmpty))
            {
                if (!byRow.TryGetValue(coach.Row, out var start) || start.FirstName != coach.FirstName || start.LastName != coach.LastName)
                {
                    coach.TeamIndex = CFBPlayer.NoTeamIndex;
                    continue;
                }

                if (coach.SeasonsWithTeam <= start.SeasonsWithTeam)
                {
                    coach.WinsAtCurrentSchool = start.WinsAtCurrentSchool + (coach.CareerWins - start.CareerWins);
                    coach.LossesAtCurrentSchool = start.LossesAtCurrentSchool + (coach.CareerLosses - start.CareerLosses);
                    coach.SeasonsWithTeam = start.SeasonsWithTeam + 1;
                }

                coach.TeamIndex = start.TeamIndex;
                coach.Position = start.Position;
            }
        }
    }
}
