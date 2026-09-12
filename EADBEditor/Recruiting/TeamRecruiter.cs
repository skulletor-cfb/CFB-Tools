
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    internal enum TeamFilter
    {
        None,
        P5,
        G5,
        P5G5,
        G5P5,
    }

    internal class TeamRecruiter
    {
        public int TeamId { get; }

        public int Prestige { get; }

        private static Dictionary<int, TeamRecruiter> teamRecruiters = TableUtility.PrestigeMap.Where(kvp => !kvp.Key.IsFcsTeam()).ToDictionary(kvp => kvp.Key, kvp => new TeamRecruiter(kvp.Key, kvp.Value));
        private static TeamRecruiter[] PowerRecruiters = teamRecruiters.Where(kvp => kvp.Key.IsP5OrND()).Select(kvp => kvp.Value).ToArray();
        private static TeamRecruiter[] G5Recruiters = teamRecruiters.Where(kvp => kvp.Key.IsG5()).Select(kvp => kvp.Value).ToArray();
        private static TeamRecruiter[] EliteRecruiters = teamRecruiters.Values.Where( tr => TableUtility.PrestigeMap[tr.TeamId] >= 5).ToArray();
        private static TeamRecruiter[] SelectRecruiters = teamRecruiters.Values.Where(tr => TableUtility.PrestigeMap[tr.TeamId] >= 3).ToArray();
        private static TeamRecruiter[] PlayerRecruiters = teamRecruiters.Values.Where(tr => TableUtility.PrestigeMap[tr.TeamId] >= 1).ToArray();

        public TeamRecruiter(int teamId, int prestige)
        {
            TeamId = teamId;
            Prestige = prestige;
        }

        /// <summary>
        /// returns a team a player might be interested in
        /// </summary>
        /// <param name="player"></param>
        public static int ResearchTeam(TransferCandidate player, TeamFilter desiredTeam, int mod = 0)
        {
            TeamRecruiter[] teamsToLookAt = null;
            var compromiseMeter = player.OVR - mod;

            if (compromiseMeter >= 90)
            {
                teamsToLookAt = EliteRecruiters;
            }
            else if (compromiseMeter >= 80)
            {
                teamsToLookAt = SelectRecruiters;
            }
            else
            {
                teamsToLookAt = PlayerRecruiters;
            }

            var result = teamsToLookAt[teamsToLookAt.Length.RAND()].TeamId;

            if (desiredTeam == TeamFilter.P5 && !result.IsP5OrND())
            {
                return ResearchTeam(player, desiredTeam, mod);
            }

            return result;
        }
    }
}
