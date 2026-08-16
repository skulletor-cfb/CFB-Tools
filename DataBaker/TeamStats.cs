using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DataBaker.Contracts;

namespace DataBaker
{
    public class TeamStats
    {
        private static HashSet<int> tablesCached = new HashSet<int>(new[] { 1, 2, 3, 5 });
        public TeamStats(int teamId)
        {
            TeamId = teamId;
            Tables = new Dictionary<int, Dictionary<Player, Player>>();
        }

        public int TeamId { get; set; }
        public Dictionary<int, Dictionary<Player, Player>> Tables { get; set; }

        public void AddPlayer(Player player)
        {
            if (!tablesCached.Contains(player.TableIndex))
                return;

            // get a table
            Dictionary<Player, Player> table = null;

            if (Tables.TryGetValue(player.TableIndex, out table) == false)
            {
                table = new Dictionary<Player, Player>();
                Tables.Add(player.TableIndex, table);
            }

            Player existing = null;

            if (!table.TryGetValue(player, out existing))
            {
                table.Add(player, player);
            }
            else
            {
                existing.Merge(player);
            }
        }

        public List<TableRow> GetAllTimePassers(int top)
        {
            var table = Tables[1];
            return table.Values.OrderByDescending(p => p.Stat3).Take(top).Select(p => p.ToTableRow()).ToList();
        }

        public List<TableRow> GetAllTimeQBRushers(int top)
        {
            var table = Tables[2];
            return table.Values.Where(p => p.Position == "QB").OrderByDescending(p => p.Stat2).Take(top).Select(p => p.ToTableRow(true)).ToList();
        }

        public List<TableRow> GetAllTimeRushers(int top)
        {
            var table = Tables[2];
            return table.Values.Where(p => p.Position != "QB").OrderByDescending(p => p.Stat2).Take(top).Select(p => p.ToTableRow(true)).ToList();
        }

        public List<TableRow> GetAllTimeRec(int top)
        {
            var table = Tables[3];
            return table.Values.OrderByDescending(p => p.Stat1).Take(top).Select(p => p.ToTableRow()).ToList();
        }

        public List<TableRow> GetAllTimeTackles(int top)
        {
            var table = Tables[5];
            return table.Values.OrderByDescending(p => p.Stat1).Take(top).Select(p => p.ToTableRow(true, true)).ToList();
        }

        public List<TableRow> GetAllTimeSacks(int top)
        {
            var table = Tables[5];
            return table.Values.OrderByDescending(p => p.Stat3).Take(top).Select(p => p.ToTableRow(true, true)).ToList();
        }

        public List<TableRow> GetAllTimeInt(int top)
        {
            var table = Tables[5];
            return table.Values.OrderByDescending(p => p.Stat4).Take(top).Select(p => p.ToTableRow(true, true)).ToList();
        }
    }
}