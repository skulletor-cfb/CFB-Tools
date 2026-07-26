using DataBaker.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace DataBaker
{
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
            var sw = Stopwatch.StartNew();
            var seasonsToRun = Helper.Seasons.Season.Where(s => !s.Loaded).ToArray();
            Task<bool>[][] loads = new Task<bool>[seasonsToRun.Length][];
            var latestSeason = seasonsToRun.Last();


            Parallel.ForEach(seasonsToRun,
                s =>
                {
                    s.Loaded = s.ReadFromFile("tsch.csv", Season.scheduleKey) &&
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
        }
    }
}