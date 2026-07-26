using DataBaker.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace DataBaker
{
    public class PlayoffDescriptor
    {
        public string Error { get; set; }
        public int Apperances { get; set; }
        public int Last { get; set; }
    }

    public interface ILogger
    {
        void WriteLine(object s);
    }

    public class ConsoleLogger : ILogger
    {
        public void WriteLine(object s)
        {
            Console.WriteLine(s);
        }
    }

    public static class Oven
    {
        private static Season[] seasons;

        public static void Bake(ILogger logger)
        {
            if (!Directory.Exists(Helper.BakedPath))
            {
                Directory.CreateDirectory(Helper.BakedPath);
            }

            var sw = Stopwatch.StartNew();
            var seasonsToRun = Helper.Seasons.Season.Where(s => !s.Loaded).ToArray();
            Task<bool>[][] loads = new Task<bool>[seasonsToRun.Length][];
            var latestSeason = seasonsToRun.Last();


            Parallel.ForEach(seasonsToRun,
                s =>
                {
                    s.Loaded = s.ReadTeamScheduleFile() &&
                    s.ReadFromFile("aaac.csv", Season.AllAmericanKey) &&
                    s.ReadFromFile("awards.csv", Season.AwardsKey) &&
                    s.ReadFromFile("team", Season.TeamKey);
                    logger.WriteLine(s.Year);
                });

            sw.Stop();

            logger.WriteLine("ReadPlayoffHistory");
            Helper.ReadFromPlayoffHistory();

            var status = "Started in " + sw.Elapsed;
            logger.WriteLine(status);
            seasons = Helper.Seasons.Season.Where(s => s.Loaded).ToArray();

            // bake playoff apperances
            PlayoffAppearances().Bake("playoffs");
        }

        /// <summary>
        /// will write to a file prefix.key.txt
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="prefix"></param>
        private static void Bake<T>(this Dictionary<int, T> dict, string prefix)
        {
            foreach (var kvp in dict)
            {
                var fileName = $"{prefix}.{kvp.Key}.txt";
                var file = Path.Combine(Helper.BakedPath, fileName);
                File.WriteAllText(file, JsonConvert.SerializeObject(kvp.Value));
            }
        }

        private static Dictionary<int, PlayoffDescriptor> PlayoffAppearances()
        {
            var result = new Dictionary<int, PlayoffDescriptor>();

            foreach (var teamId in Team.TeamIds)
            {
                var dict = new PlayoffDescriptor();

                try
                {
                    var appearances = new List<int>();
                    foreach (var s in seasons)
                    {
                        if (!s.Schedule.ContainsKey(teamId))
                            continue;

                        s.ReadTeamScheduleFile();
                        var games = s.Schedule[teamId].Where(g => g.IsPlayoffBowl()).ToArray();
                        appearances.AddRange(games.Select(pg => pg.Year).Distinct());
                    }

                    dict.Apperances = appearances.Count;
                    dict.Last = appearances.Max();
                }
                catch (Exception ex)
                {
                    dict.Error = ex.ToString();
                }

                result[teamId] = dict;
            }

            return result;
        }
    }
}