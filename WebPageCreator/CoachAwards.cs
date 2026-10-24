using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace EA_DB_Editor
{
    /// <summary>A coach's All-Americans, Heisman winners, top 25 recruiting classes and coach of the year awards.</summary>
    [DataContract]
    public class CoachAwardCount
    {
        /// <summary>The coach id the site uses (the NCAA 14 id for a coach carried over by the continuation file).</summary>
        [DataMember]
        public int CoachId { get; set; }

        [DataMember]
        public int AllAmericans { get; set; }

        [DataMember]
        public int HeismanWinners { get; set; }

        [DataMember]
        public int Top25Classes { get; set; }

        [DataMember]
        public int CoachOfYearAwards { get; set; }
    }

    [DataContract]
    public class CoachAwardSeason
    {
        /// <summary>The save's season (CurrentSeasonYear).</summary>
        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public List<CoachAwardCount> Coaches { get; set; }
    }

    /// <summary>
    /// CFB 27 keeps no per-coach All-American, Heisman, top 25 class or coach of the year counts, so, like bowlrecords, they're
    /// kept in a side file: each completed season's credits, keyed by season so a re-run replaces that season. A coach's totals
    /// are his NCAA 14 counts (the continuation file) plus every season recorded since.
    /// </summary>
    [DataContract]
    public class CoachAwardHistory
    {
        public static readonly string CoachAwardsFile = Path.Combine(@".\archive", "coachawards");

        [DataMember]
        public List<CoachAwardSeason> Seasons { get; set; }

        /// <summary>
        /// Fills in the coaches' award counts. At the end of a season (not the preseason magazine) that season's credits are
        /// recorded first. NCAA 14 saves have their own counts, so this only applies to CFB 27.
        /// </summary>
        public static void Apply(IDataEngine dataEngine, Dictionary<string, Coach> coaches, bool isPreseason)
        {
            if (!(dataEngine is CFB27DataEngine cfb27))
            {
                return;
            }

            var history = File.Exists(CoachAwardsFile) ? CoachAwardsFile.FromJsonFile<CoachAwardHistory>() : new CoachAwardHistory();
            history.Seasons = history.Seasons ?? new List<CoachAwardSeason>();

            if (!isPreseason && dataEngine.IsSeasonOver())
            {
                var year = cfb27.CurrentSeasonYear;
                history.Seasons.RemoveAll(s => s.Year == year);
                history.Seasons.Add(new CoachAwardSeason { Year = year, Coaches = cfb27.ReadSeasonCoachAwards() });
                history.Seasons.Sort((a, b) => a.Year.CompareTo(b.Year));
                history.ToJsonFile(CoachAwardsFile);
            }

            var seeds = ContinuationData.Instance?.CoachAwards ?? new List<CoachAwardCount>();
            var totals = seeds.Concat(history.Seasons.SelectMany(s => s.Coaches))
                .GroupBy(c => c.CoachId)
                .ToDictionary(g => g.Key, g => new CoachAwardCount
                {
                    CoachId = g.Key,
                    AllAmericans = g.Sum(c => c.AllAmericans),
                    HeismanWinners = g.Sum(c => c.HeismanWinners),
                    Top25Classes = g.Sum(c => c.Top25Classes),
                    CoachOfYearAwards = g.Sum(c => c.CoachOfYearAwards),
                });

            foreach (var coach in coaches.Values)
            {
                if (totals.TryGetValue(coach.Id, out var total))
                {
                    coach.AllAmericans = total.AllAmericans;
                    coach.HeismanWinners = total.HeismanWinners;
                    coach.Top25Classes = total.Top25Classes;
                    coach.CoachOfYearAwards = total.CoachOfYearAwards;
                }
            }
        }
    }
}
