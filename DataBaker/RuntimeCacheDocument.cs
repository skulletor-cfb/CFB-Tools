using System;
using System.Collections.Generic;
using System.Text;
using DataBaker.Contracts;
namespace DataBaker
{
    public class RuntimeCacheDocument
    {
        public static RuntimeCacheDocument CreateFromCache()
        {
            return new RuntimeCacheDocument
            {
                SeasonsDict = RuntimeCache.SeasonsDict,
                processedStats = RuntimeCache.processedStats,
                CachedTeamStats = RuntimeCache.CachedTeamStats
            };
        }

        public static void FromCache(RuntimeCacheDocument doc)
        {
            RuntimeCache.SeasonsDict = doc.SeasonsDict;
            RuntimeCache.processedStats = doc.processedStats;
            RuntimeCache.CachedTeamStats = doc.CachedTeamStats;
        }

        public Dictionary<int, Season> SeasonsDict { get; set; }
        public HashSet<string> processedStats { get; set; }
        public Dictionary<int, TeamStats> CachedTeamStats { get; set; }
    }
}