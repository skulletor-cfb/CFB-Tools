using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class NationalSchedule
    {
        public NationalSchedule() { }

        public static NationalSchedule Create(IEnumerable<MaddenRecord> records, Dictionary<int, string> teamAbb, Dictionary<int, string> teams)
        {
            var games = new List<GameRecord>();

            foreach(var record in records)
            {
                games.Add(GameRecord.Create(record));
            }

            return new NationalSchedule
            {
                Games = games,
                TeamMap = teams,
                TeamAbbreviations = teamAbb,
            };
        }

        public List<GameRecord> Games { get; set; }
        public Dictionary<int, string> TeamMap { get; set; }
        public Dictionary<int, string> TeamAbbreviations { get; set; }
    }

    public class GameRecord
    {
        public GameRecord() { }

        public static GameRecord Create(MaddenRecord record)
        {
            return new GameRecord
            {
                GATG = record["GATG"],
                GHTG = record["GHTG"],
                GTOD = record["GTOD"],
                SGNM = record["SGNM"],
                SEWN = record["SEWN"],
                SEWT = record["SEWT"],
                GDAT = record["GDAT"],
                GFFU = record["GFFU"],
                GMFX = record["GMFX"],
            };
        }

        public string GATG { get; set; }
        public string GHTG { get; set; }
        public string GTOD { get; set; }
        public string SGNM { get; set; }
        public string SEWN { get; set; }
        public string SEWT { get; set; }
        public string GDAT { get; set; }
        public string GFFU { get; set; }
        public string GMFX { get; set; }
    }
}