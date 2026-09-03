using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public static class TableUtility
    {
        public static MaddenTable FindTable(string name)
        {
            return MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, name);
        }

        public static HashSet<int> FindUserTeams()
        {
            var sttm = FindTable("STTM");
            var result = new HashSet<int>();

            foreach (var mr in sttm.lRecords)
            {
                if (mr["CFUC"].ToInt32() == 1)
                {
                    result.Add(mr["TGID"].ToInt32());
                }
            }

            return result;
        }

        public static void SetupForStudioUpdates()
        {
            var schd = TableUtility.FindTable("SCHD");
            var userTeams = TableUtility.FindUserTeams();

            foreach (var mr in schd.lRecords)
            {
                var away = mr["GATG"].ToInt32();
                var home = mr["GHTG"].ToInt32();

                // don't change anything about the games vs fcs teams
                if (away.IsFcsTeam())
                {
                    continue;
                }

                // user team should not have a studio update
                if (userTeams.Contains(home) || userTeams.Contains(away))
                {
                    mr["GFFU"] = "1";
                    mr["GFHU"] = "0";
                    mr["GMFX"] = "0";
                }
                else
                {
                    mr["GFFU"] = "0";
                    mr["GFHU"] = "0";
                    mr["GMFX"] = "1";
                }
            }
        }
    }
}
