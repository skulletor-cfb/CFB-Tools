using DataBaker.Contracts;
using System.Collections.Concurrent;
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
        public ConcurrentDictionary<string, bool> processedStats { get; set; }
        public ConcurrentDictionary<int, TeamStats> CachedTeamStats { get; set; }
    }
}