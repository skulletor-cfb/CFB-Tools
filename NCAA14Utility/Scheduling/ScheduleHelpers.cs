using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCAA14Utility.Scheduling
{
    public static class ScheduleHelpers
    {
        // classic (NCAA 14) conference ids, as CFBConference.ClassicIds and EADBEditor's TableUtility use them
        public const int ACC= 0;
        public const int Big10= 1;
        public const int Big12= 2;
        public const int American= 3;
        public const int CUSA= 4;
        public const int Independent= 5;
        public const int MAC= 7;
        public const int MWC= 9;
        public const int Pac12= 10;
        public const int SEC= 11;
        public const int SBC= 13;

        /// <summary>
        /// CFB 27 Team table row (the ScheduleBase constants) -> classic conference id for the season being scheduled.
        /// Membership can change from season to season, so it comes from the save: set it from
        /// CFBConference.ReadTeamConferences(dynasty) when a save is opened. Empty until then.
        /// </summary>
        public static Dictionary<int, int> TeamAndConferences { get; set; } = new Dictionary<int, int>();

        public static Dictionary<int, HashSet<int>> BuildHashSet(this Dictionary<int, int[]> dict)
        {
            var conf = new Dictionary<int, HashSet<int>>();

            foreach (var kvp in dict)
            {
                HashSet<int> curr = null;
                HashSet<int> temp = null;

                if (!conf.TryGetValue(kvp.Key, out curr))
                {
                    curr = new HashSet<int>();
                    conf[kvp.Key] = curr;
                }

                foreach (var opp in kvp.Value)
                {
                    curr.Add(opp);

                    if (!conf.TryGetValue(opp, out temp))
                    {
                        temp = new HashSet<int>();
                        conf[opp] = temp;
                    }

                    temp.Add(kvp.Key);
                }
            }

            return conf;
        }

        public static Dictionary<int, int[]> Verify(this Dictionary<int, int[]> result, int teamLength, int confId, string confName, bool verifyMembership = true, int expectedGames = 4, int ifNotExpectedThen = 5)
        {
            // verify integrity
            var teams = result.Values.SelectMany(i => i).GroupBy(i => i).Select(g => new { Team = g.Key, Count = g.Count() }).ToArray();

            if (teams.Length != teamLength)
                throw new Exception("Wrong number of teams");

            if (teams.Length == 4 || teams.Length == 6)
                return result;

            var badSchedule = teams.Where(t => t.Count != expectedGames).ToArray();

            if (badSchedule.Any())
            {
                foreach (var schd in badSchedule)
                {
                    if (schd.Count != ifNotExpectedThen)
                    {
                        throw new Exception("You fucked up: " + string.Join(",", badSchedule.Select(t => t.Team.ToString())));
                    }
                }
            }

            if (verifyMembership)
            {
                badSchedule = teams.Where(t => TeamAndConferences[t.Team] != confId).ToArray();
                if (badSchedule.Any())
                {
                    throw new Exception("Team not in " + confName + ": " + string.Join(",", badSchedule.Select(t => t.Team.ToString())));
                }
            }

            return result;
        }

        public static KeyValuePair<int, int[]> Create(this int team, params int[] values)
        {
            return new KeyValuePair<int, int[]>(team, values);
        }

        public static void Create(this Dictionary<int, int[]> dict, int team, params int[] values)
        {
            var kvp = team.Create(values);
            dict[kvp.Key] = kvp.Value;
        }

        public static Dictionary<int, int[]> Create(this IEnumerable<KeyValuePair<int, int[]>> values) => values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
