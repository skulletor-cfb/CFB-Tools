using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public static class ScheduleUtility
    {
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

        public static Dictionary<int, int[]> Verify(this Dictionary<int, int[]> result, int teamLength, int confId, string confName, bool verifyMembership = true, int expectedGames = 4)
        {
            // verify integrity
            var teams = result.Values.SelectMany(i => i).GroupBy(i => i).Select(g => new { Team = g.Key, Count = g.Count() }).ToArray();

            if (teams.Length != teamLength)
                throw new Exception("Wrong number of teams");

            if (teams.Length == 4)
                return result;

            var badSchedule = teams.Where(t => t.Count != expectedGames).ToArray();

            if (badSchedule.Any())
            {
                throw new Exception("You fucked up: " + string.Join(",", badSchedule.Select(t => t.Team.ToString())));
            }

            if (verifyMembership)
            {
                badSchedule = teams.Where(t => RecruitingFixup.TeamAndConferences[t.Team] != confId).ToArray();
                if (badSchedule.Any())
                {
                    throw new Exception("Team not in " + confName + ": " + string.Join(",", badSchedule.Select(t => t.Team.ToString())));
                }
            }

            return result;
        }
    }
}
