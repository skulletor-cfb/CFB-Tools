using DataBaker.Contracts;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;

namespace DataBaker
{
    public static class Helper
    {
        public const string SeasonsKey = "seasonsFile";
        public static readonly string appPath = @"D:\NCAA_2014\Archive";
        private static Seasons seasons = null;

        public static Seasons Seasons
        {
            get
            {
                if (seasons == null)
                {
                    seasons = ReadSeasonsFile();
                }

                return seasons;
            }
            set
            {
                seasons = value;
            }
        }

        static Seasons ReadSeasonsFile()
        {
            var file = Path.Combine(appPath, "seasons");
            seasons = File.ReadAllText(file).FromJson<Seasons>();
            seasons.Season = seasons.Season.OrderBy(s => s.Year).ToList();
            RuntimeCache.ProcessSeasons(seasons);
            return seasons;
        }

        public static string ToDisplayRank(this int rank)
        {
            return rank > 25 ? "-" : "#" + rank;
        }

        public static string MakeBold<T>(this T o)
        {
            return "<b>" + o.ToString() + "</b>";
        }

        public static string MakeWinningTeamBold(this string s)
        {
            return "<b><font size=2>" + s + "</font></b>";
        }

        public static bool ReadFromFile(this Season s, string file, string key)
        {
            file = Path.Combine(appPath, s.SeasonPath, file);

            if (s.KeysFilled.Contains(key))
                return true;

            if (File.Exists(file) == false)
                return false;

            var data = s.ReadFromFile(file);
            s.Parse(key, data);
            return true;
        }

        public static string ReadFromFile(this Season s, string file)
        {
            if (!File.Exists(file))
            {
                file = Path.Combine(appPath, s.SeasonPath, file);
            }

            if (!File.Exists(file))
                return null;

            if (UsingGZip)
            {
                return File.ReadAllText(file).FromBase64().UnzipIt();
            }

            return File.ReadAllText(file);
        }

        public static void ReadFromFile(this Season s, string key, string file, Action<string> assign)
        {
            file = Path.Combine(appPath, s.SeasonPath, file);
            assign(File.ReadAllText(file).FromBase64().UnzipIt());
        }

        public static byte[] FromBase64(this string b)
        {
            return Convert.FromBase64String(b);
        }

        public static Stream ToJson<T>(this T obj)
        {
            var js = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream();
            js.WriteObject(ms, obj);
            return ms;
        }

        public static T FromJson<T>(this string json)
        {
            var js = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)js.ReadObject(ms);
            }
        }

        public static bool UsingGZip = false;

        public static string UnzipIt(this byte[] data)
        {
            if (UsingGZip)
            {
                List<byte> result = new List<byte>();
                using (var ms = new MemoryStream(data))
                {
                    using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[4096];
                        int read = 0;

                        do
                        {
                            read = gz.Read(bytes, 0, bytes.Length);
                            result.AddRange(bytes.Take(read));
                        } while (read > 0);
                    }
                }

                return Encoding.UTF8.GetString(result.ToArray());
            }

            return Encoding.UTF8.GetString(data);
        }

        public static byte[] ZipItGood(this string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            using (var ms = new MemoryStream())
            {
                using (var gz = new GZipStream(ms, CompressionMode.Compress))
                {
                    gz.Write(bytes, 0, bytes.Length);
                    gz.Close();
                    return ms.ToArray();
                }
            }
        }

        public static T[] FromCsv<T>(this string csv, Func<string[], T> generator, Func<T, bool> filter = null)
        {
            var select = csv.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(l => generator(l.Split(',')));

            if (filter != null)
            {
                return select.Where(t => filter(t)).ToArray();
            }

            return select.ToArray();
        }

        public static int ToInt(this string s)
        {
            int i = 0;

            if (!int.TryParse(s, out i))
                return 0;

            return i;
        }

        public static int? ToNullableInt(this string s)
        {
            int i = 0;

            if (int.TryParse(s, out i))
                return i;

            return null;
        }

        public static string[] CsvSplit(this string s)
        {
            return s.Split(',');
        }

        public static bool IsNY6Bowl(this PlayedGame game)
        {
            return game.BowlId.HasValue && NY6Bowls.Contains(game.BowlId.Value);
        }

        const int cfp12Start = 2542;

        public static bool IsPlayoffBowl(this PlayedGame game)
        {
            int[] order;
            return IsPlayoffBowl(game, out order);
        }

        public static bool IsPlayoffBowl(this PlayedGame game, out int[] gameOrder)
        {
            var year = game.Year;
            int[] games = null;

            if (game.Year >= cfp12Start)
            {
                var mod = (game.Year - cfp12Start) % 3;
                games = Spots[mod + cfp12Start];
            }
            // 1 == rose/sugar, 2 = orange/cotton, 0 = peach/fiesta
            else if (!Spots.TryGetValue(year, out games))
            {
                games = Spots[year % 3];
            }

            gameOrder = games;
            return game.BowlId.HasValue && games.Contains(game.BowlId.Value);
        }

        public static bool IsPlayoffBowl(this int bowlId, int year)
        {
            int[] games = null;

            if (year <= 2013)
            {
                if (Spots.TryGetValue(year, out games))
                    return bowlId == games[0];
                else
                    return false;
            }

            if (year >= cfp12Start)
            {
                var mod = (year - cfp12Start) % 3;
                games = Spots[mod + cfp12Start];
            }

            // 1 == rose/sugar, 2 = orange/cotton, 0 = peach/fiesta
            else if (!Spots.TryGetValue(year, out games))
                games = Spots[year % 3];

            return games.Contains(bowlId);
        }

        static HashSet<int> NY6Bowls = new HashSet<int>(new[] { 17, 12, 28, 26, 25, 27 });

        // in my dynasty 2067 had the playoffs in Cotton/Peach and 2069 had it in Orange/Fiesta
        static Dictionary<int, int[]> Spots = new Dictionary<int, int[]>()
    {
        {2543, new[] { 39, 28, 17, 25, 27, 12, 26, 987050, 987049, 987048, 987047 }},
       {2544,new[] { 39, 12, 26, 25, 27, 28, 17, 987050, 987049, 987048, 987047 } },
        {2542, new[] { 39, 25, 27, 28, 17, 12, 26, 987050, 987049, 987048, 987047  }},
        {2067,  new[] { 39, 17, 12 }},
        {2069 ,   new[] { 39, 28, 26 }},
        {0, new[] { 39, 12, 26 }},
        { 1,new[] { 39, 25, 27 } },
        {2, new[] { 39, 28, 17 }},
        {2013,new[] {39 } },
        {2012,new[] {39 } },
        {2011,new[] {39 } },
        {2010,new[] {39 } },
        {2009,new[] {39 } },
        {2008,new[] {39 } },
        {2007,new[] {39 } },
        {2006,new[] {39 } },
        {2005,new[] {25 } },
        {2004,new[] {28 } },
        {2003,new[] {27 } },
        {2002,new[] {26 } },
        {2001,new[] {25 } },
        {2000,new[] {28 } },
        {1999,new[] {27 } },
        {1998,new[] {26 } },
        {1997,new[] {28 } },
        {1996,new[] {27 } },
        {1995,new[] {26 } },
        {1994,new[] {28 } },
        {1993,new[] {28 } },
        {1992,new[] {27 } },
    };

        public static void ReadFromPlayoffHistory()
        {
            PastPlayoffHistory.Clear();
            var file = Path.Combine(appPath, "PlayoffHistory.xml");
            var xml = XElement.Load(file);
            var arr = xml.Elements("Year").Select(node => PastPlayoffHistory.ParseYear(node)).ToArray();
            PastPlayoffHistory.Years = PastPlayoffHistory.years.OrderByDescending(kvp => kvp.Key).Select(kvp => kvp.Key).ToArray();
        }

    }
}
