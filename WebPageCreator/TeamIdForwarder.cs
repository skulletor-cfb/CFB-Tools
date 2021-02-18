using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace EA_DB_Editor
{
    public static class TeamIdForwarder
    {
        public static Dictionary<int, int> TeamIdFwd = ConfigurationManager.AppSettings["TeamIdFwd"].Split(',').ToDictionary(s => s.Split('=').First().ToInt32(), s => s.Split('=')[1].ToInt32());

        public static int GetRealTeamId(this int id)
        {
            return TeamIdFwd.TryGetValue(id, out var value) ? value : id;
        }

        public static bool TeamNoLongerFBS(this int id) => TeamIdFwd.ContainsKey(id);
    }
}