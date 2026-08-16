using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DataBaker.Contracts;
namespace DataBaker
{
    public static class RuntimeCache
    {
        public static Dictionary<int, Season> SeasonsDict { get; set; }
        public static ConcurrentDictionary<string, bool> processedStats { get; set; }
        public static ConcurrentDictionary<int, TeamStats> CachedTeamStats { get; set; }
        public static void Clear()
        {
            SeasonsDict = null;
            processedStats = null;
            CachedTeamStats = null;
        }

        public static TValue GetDictionaryValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            TValue result = default(TValue);
            dict.TryGetValue(key, out result);
            return result;
        }

        public static void ProcessSeasons(this Seasons seasons)
        {
            // easy hasn't been created yet
            if (SeasonsDict == null)
            {
                SeasonsDict = seasons.Season.ToDictionary(s => s.Year);
            }
            else if (seasons.Season.Count != SeasonsDict.Count)
            {
                foreach (var s in seasons.Season.Where(season => SeasonsDict.ContainsKey(season.Year) == false))
                {
                    SeasonsDict.Add(s.Year, s);
                }
            }
        }

        public static TeamStats ProcessTeamStat(this Season s, int teamId)
        {
            if (processedStats == null) processedStats = new ConcurrentDictionary<string, bool>();
            if (CachedTeamStats == null) CachedTeamStats = new ConcurrentDictionary<int, TeamStats>();

            var key = string.Format("{0}-{1}", s.Year, teamId);
            var file = string.Format("team{0}pstat.csv", teamId);

            // already processed
            if (processedStats.ContainsKey(key))
                return CachedTeamStats[teamId];

            var fileText = s.ReadFromFile(file);

            if (fileText == null) return null;

            var csv = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1);

            TeamStats ts = null;
            if (CachedTeamStats.TryGetValue(teamId, out ts) == false)
            {
                ts = new TeamStats(teamId);
                CachedTeamStats.TryAdd(teamId, ts);
            }

            foreach (var line in csv)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var player = Player.Generate(line.CsvSplit(), teamId, s.Year);
                ts.AddPlayer(player);
            }

            return ts;
        }
    }
}