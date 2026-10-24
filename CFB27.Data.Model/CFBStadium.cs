using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CFB27.Data.Model
{
    /// <summary>
    /// A stadium from the game's own stadium table. Saves only hold an asset reference per stadium (Team.Stadium,
    /// SeasonGame.Stadium), not names or capacities, so the table is extracted from the game install once
    /// (<see cref="ExtractFromGame"/>) and embedded as Resources\stadiums.csv. Records are keyed by Code, which
    /// matches Team.AssetName (IOWAST = Iowa State's Jack Trice Stadium).
    /// </summary>
    public class CFBStadium
    {
        private const string ResourceName = "CFB27.Data.Model.Resources.stadiums.csv";

        public string Code { get; set; }
        public string School { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int Capacity { get; set; }

        /// <summary>
        /// Approximate position from the game table (degrees, north and east positive; west longitudes are negative).
        /// Null when the game has none: 0,0, or the 25N 55E placeholder the generic stadiums and Dublin share. Several
        /// neutral sites are only whole-degree or copied from another stadium (Lucas Oil has Ford Field's, MetLife
        /// has Jacksonville's), so treat them as rough.
        /// </summary>
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>
        /// Stadium id used by the web page creator: the save's 32-bit stadium asset reference
        /// (e.g. 0x80654C94 for Jack Trice), so team and game stadiums share one key.
        /// </summary>
        public static int ToStadiumId(string reference)
        {
            return unchecked((int)Convert.ToUInt32(reference, 2));
        }

        private static List<CFBStadium> embedded;
        private static Dictionary<string, CFBStadium> embeddedByCode;

        /// <summary>The embedded stadium table, in game order.</summary>
        public static IReadOnlyList<CFBStadium> Embedded
        {
            get
            {
                if (embedded == null)
                {
                    using (var stream = typeof(CFBStadium).Assembly.GetManifestResourceStream(ResourceName))
                    using (var reader = new StreamReader(stream))
                    {
                        embedded = ReadCsv(reader);
                    }
                }

                return embedded;
            }
        }

        /// <summary>Stadium by Code, ignoring case (the table has Kenn for Team.AssetName KENN). First one wins for the few repeated codes.</summary>
        public static CFBStadium FindByCode(string code)
        {
            if (embeddedByCode == null)
            {
                var byCode = new Dictionary<string, CFBStadium>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in Embedded.Where(s => !byCode.ContainsKey(s.Code)))
                {
                    byCode.Add(s.Code, s);
                }

                embeddedByCode = byCode;
            }

            return code != null && embeddedByCode.TryGetValue(code, out var stadium) ? stadium : null;
        }

        /// <summary>
        /// A team's home stadium. Most stadium codes are the team's AssetName; a few teams added later use other codes
        /// (JSU, MISTXX, Dela, CLARENCE) and match on the stadium's Abbreviation instead (JVST, MOST, DEL, HAW).
        /// </summary>
        public static CFBStadium FindHome(string teamAssetName, string teamShortName)
        {
            return FindByCode(teamAssetName)
                ?? Embedded.FirstOrDefault(s => string.Equals(s.Abbreviation, teamAssetName, StringComparison.OrdinalIgnoreCase))
                ?? Embedded.FirstOrDefault(s => string.Equals(s.Abbreviation, teamShortName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Stadium by name, ignoring case; first one wins (Lincoln Financial Field is both Temple's and the Army-Navy site).</summary>
        public static CFBStadium FindByName(string name)
        {
            return Embedded.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Stadium references that no team uses as its home (bowl, title game, kickoff and other neutral sites), mapped to
        /// the stadium's name in the game table. The references are asset ids, so this list is cross-referenced with
        /// MyDynasty's stadium table and with how the saves use them. References not listed resolve to no stadium:
        /// MyDynasty also knows 0x80654C4D Aloha Stadium, 0x80654C9E Kibbie Dome and 0x8095C154 College Football Blitz
        /// Stadium, which aren't in the game's stadium table; 0x80DA11F2 (Arizona State-Kansas) is unknown.
        /// </summary>
        public static readonly IReadOnlyDictionary<uint, string> NeutralSites = new Dictionary<uint, string>
        {
            { 0x806385B3, "Bank of America Stadium" },         // Duke's Mayo Bowl, ACC title game
            { 0x806385C1, "Cotton Bowl" },                     // Oklahoma-Texas
            { 0x806385CF, "Caesars Superdome" },               // Sugar Bowl, New Orleans Bowl
            { 0x806385D0, "AT&T Stadium" },                    // Cotton Bowl (CFP), Big 12 title game
            { 0x80638607, "Hard Rock Stadium" },               // Orange Bowl
            { 0x80638637, "Mercedes-Benz Stadium" },           // Peach Bowl, SEC title game, Aflac Kickoff
            { 0x80639DC0, "Nissan Stadium" },                  // Music City Bowl, Music City Kickoff
            { 0x80639DCE, "Reliant Stadium" },                 // Texas Bowl (NRG Stadium in MyDynasty)
            { 0x80639DD6, "College Practice Field" },
            { 0x80639DE5, "SoFi Stadium" },
            { 0x80639DE7, "State Farm Stadium" },              // Fiesta Bowl
            { 0x80639E01, "EverBank Stadium" },                // Gator Bowl, Florida-Georgia
            { 0x8063B3C6, "M&T Bank Stadium" },                // Army-Navy rotation
            { 0x80654C81, "Ford Field" },                      // MAC title game
            { 0x80654C91, "Independence Bowl Stadium" },       // Independence Bowl
            { 0x80654CA7, "Lincoln Financial Field" },         // Army-Navy rotation (Temple's home is a separate reference)
            { 0x80654CAD, "Lucas Oil Stadium" },               // Big Ten title game
            { 0x80654CB9, "Metlife Stadium" },                 // Army-Navy
            { 0x80654CD4, "Rentschler Field" },
            { 0x80654CDE, "Rose Bowl" },                       // Rose Bowl (CFP)
            { 0x80654C4D, "Aloha Stadium" },                   // Team Builder stadium list; not in the game's stadium table
            { 0x806A46F0, "Xbox Bowl Stadium" },
            { 0x806A46F3, "Exploria Stadium" },
            { 0x806A46FD, "Frisco Bowl Stadium" },
            { 0x806A4710, "Salute to Veterans Bowl Stadium" },
            { 0x806A4711, "Holiday Bowl Stadium" },
            { 0x806A4712, "Rate Bowl Stadium" },
            { 0x806A4713, "Fenway Bowl Stadium" },
            { 0x806A4714, "City Stadium" },
            { 0x806A4715, "Camping World Stadium" },           // Citrus Bowl, Pop-Tarts Bowl, Camping World Kickoff
            { 0x8095C153, "Dublin Stadium" },                  // Aer Lingus Classic
            { 0x8095C1A7, "Generic Stadium 4" },
            { 0x80A62A1C, "High School Stadium" },
            { 0x80B4451A, "High School Stadium 01" },
            { 0x80B4451B, "High School Stadium 02" },
            { 0x80B4451C, "High School Stadium 03" },
            { 0x80B4451D, "Horseshoe stadium" },
            { 0x80B4451E, "Oval Stadium" },
            { 0x80B4451F, "Side stadium" },
            { 0x80B7A946, "Gillette Stadium" },                // Notre Dame-Navy
            { 0x80C7E4D7, "Lambeau Field" },                   // Wisconsin-Notre Dame
            { 0x80DA11F2, "Wembley Stadium" },                 // Arizona State-Kansas (seen in game, DYNASTY-TEST)
        };

        /// <summary>
        /// Generic, practice and high-school stadium assets in <see cref="NeutralSites"/>. They aren't real game venues
        /// (a dynasty game set at Oval Stadium crashed the game), so schedule editors shouldn't offer them.
        /// </summary>
        public static readonly ISet<uint> NonGameSites = new HashSet<uint>
        {
            0x80639DD6, // College Practice Field
            0x8095C1A7, // Generic Stadium 4
            0x80A62A1C, // High School Stadium
            0x80B4451A, // High School Stadium 01
            0x80B4451B, // High School Stadium 02
            0x80B4451C, // High School Stadium 03
            0x80B4451D, // Horseshoe stadium
            0x80B4451E, // Oval Stadium
            0x80B4451F, // Side stadium
        };

        /// <summary>The neutral site a save stadium reference points to, or null when it isn't in <see cref="NeutralSites"/>.</summary>
        public static CFBStadium FindNeutralSite(string reference)
        {
            return NeutralSites.TryGetValue(Convert.ToUInt32(reference, 2), out var name) ? FindByName(name) : null;
        }

        #region csv
        private static readonly string[] CsvHeader = { "Code", "School", "Name", "Abbreviation", "Capacity", "Latitude", "Longitude" };

        public static void WriteCsv(IEnumerable<CFBStadium> stadiums, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", CsvHeader));
            foreach (var s in stadiums)
            {
                sb.AppendLine(string.Join(",", Quote(s.Code), Quote(s.School), Quote(s.Name), Quote(s.Abbreviation), s.Capacity,
                    FormatCoordinate(s.Latitude), FormatCoordinate(s.Longitude)));
            }

            File.WriteAllText(path, sb.ToString());
        }

        public static List<CFBStadium> ReadCsv(TextReader reader)
        {
            var stadiums = new List<CFBStadium>();
            reader.ReadLine(); // header
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                var parts = SplitCsv(line);
                stadiums.Add(new CFBStadium
                {
                    Code = parts[0],
                    School = parts[1],
                    Name = parts[2],
                    Abbreviation = parts[3],
                    Capacity = int.Parse(parts[4]),
                    Latitude = parts.Count > 5 ? ParseCoordinate(parts[5]) : null,
                    Longitude = parts.Count > 6 ? ParseCoordinate(parts[6]) : null,
                });
            }

            return stadiums;
        }

        private static string FormatCoordinate(double? value) =>
            value?.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        private static double? ParseCoordinate(string value) =>
            double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : (double?)null;

        private static string Quote(string value)
        {
            value = value ?? string.Empty;
            return value.IndexOfAny(new[] { ',', '"' }) >= 0 ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }

        private static List<string> SplitCsv(string line)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else if (c == '"') { inQuotes = false; }
                    else { current.Append(c); }
                }
                else if (c == '"') { inQuotes = true; }
                else if (c == ',') { parts.Add(current.ToString()); current.Clear(); }
                else { current.Append(c); }
            }

            parts.Add(current.ToString());
            return parts;
        }
        #endregion

        #region extraction
        // The table is a run of fixed 216-byte records inside an uncompressed CAS asset (the same kind of
        // asset the FTX schema ships in): code[32] school[40] index[2] name[40] abbreviation[42] capacity(u32 BE), then 56 bytes
        // that end with the NEXT record's longitude and latitude (f32 BE at +204/+208). So a record's position sits in the
        // 56 bytes before it, and the first record's position comes from the bytes just before the table.
        // CAS stores assets in 64 KB blocks, each preceded by an 8-byte header: [u32be size][u8 0][u8 0x70|0x71][u16].
        private const int RecordSize = 216;
        private const int BlockSize = 0x10000;
        private const int BlockHeaderSize = 8;

        // first record: AIRFOR / Air Force
        private static readonly byte[] FirstRecordMarker =
            Encoding.ASCII.GetBytes("AIRFOR").Concat(new byte[26]).Concat(Encoding.ASCII.GetBytes("Air Force")).ToArray();

        /// <summary>
        /// Reads the stadium table out of the game install. gameDataDir is the game's Data folder, e.g.
        /// C:\Program Files (x86)\Steam\steamapps\common\College Football 27\Data. Returns null if it isn't found.
        /// </summary>
        public static List<CFBStadium> ExtractFromGame(string gameDataDir)
        {
            var layout = Path.Combine(gameDataDir, "Win32", "superbundlelayout");
            foreach (var cas in Directory.GetFiles(layout, "*.cas", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                var offset = Find(cas, FirstRecordMarker);
                if (offset >= 0)
                {
                    return ReadTable(cas, offset);
                }
            }

            return null;
        }

        private static long Find(string path, byte[] pattern)
        {
            const int chunk = 1 << 24;
            var buffer = new byte[chunk + pattern.Length];
            using (var fs = File.OpenRead(path))
            {
                long position = 0;
                while (true)
                {
                    fs.Position = position;
                    int read = fs.Read(buffer, 0, buffer.Length);
                    if (read < pattern.Length)
                    {
                        return -1;
                    }

                    for (int i = 0; i <= read - pattern.Length; i++)
                    {
                        if (buffer[i] != pattern[0])
                        {
                            continue;
                        }

                        int j = 1;
                        while (j < pattern.Length && buffer[i + j] == pattern[j]) j++;
                        if (j == pattern.Length)
                        {
                            return position + i;
                        }
                    }

                    position += chunk;
                }
            }
        }

        private static List<CFBStadium> ReadTable(string casPath, long recordOffset)
        {
            using (var fs = File.OpenRead(casPath))
            {
                // the block header is within one block of the record
                long header = -1;
                var window = new byte[BlockSize + BlockHeaderSize];
                long windowStart = Math.Max(0, recordOffset - window.Length);
                fs.Position = windowStart;
                int read = fs.Read(window, 0, (int)(recordOffset - windowStart));
                for (int i = read - BlockHeaderSize; i >= 0; i--)
                {
                    // the record's block is a full one; requiring that keeps record bytes from passing as a header
                    if (IsBlockHeader(window, i) && window[i + 1] == 1 && window[i + 2] == 0 && window[i + 3] == 0)
                    {
                        header = windowStart + i;
                        break;
                    }
                }

                if (header < 0)
                {
                    throw new InvalidDataException("CAS block header for the stadium table was not found.");
                }

                // join the payloads of this block and the next few so records crossing a block boundary stay whole
                var stream = new MemoryStream();
                long blockAt = header;
                for (int b = 0; b < 8; b++)
                {
                    fs.Position = blockAt;
                    var h = new byte[BlockHeaderSize];
                    if (fs.Read(h, 0, BlockHeaderSize) < BlockHeaderSize || !IsBlockHeader(h, 0))
                    {
                        break;
                    }

                    int size = (h[0] << 24) | (h[1] << 16) | (h[2] << 8) | h[3];
                    var payload = new byte[size];
                    int got = fs.Read(payload, 0, size);
                    stream.Write(payload, 0, got);
                    blockAt += BlockHeaderSize + size;
                }

                var data = stream.ToArray();
                var stadiums = new List<CFBStadium>();
                for (long p = recordOffset - (header + BlockHeaderSize); p + RecordSize <= data.Length; p += RecordSize)
                {
                    var stadium = ParseRecord(data, (int)p);
                    if (stadium == null)
                    {
                        break;
                    }

                    stadiums.Add(stadium);
                }

                return stadiums;
            }
        }

        private static bool IsBlockHeader(byte[] b, int i)
        {
            int size = (b[i] << 24) | (b[i + 1] << 16) | (b[i + 2] << 8) | b[i + 3];
            return size > 0 && size <= BlockSize && b[i + 4] == 0 && (b[i + 5] == 0x70 || b[i + 5] == 0x71);
        }

        private static CFBStadium ParseRecord(byte[] data, int p)
        {
            var code = ReadString(data, p, 32);
            var name = ReadString(data, p + 74, 40);
            // codes are usually short ids (IOWAST, saints) but a few are names ("Holiday Bowl Stadium")
            if (code.Length == 0 || name.Length == 0 || code.Any(c => c < ' ' || c > '~'))
            {
                return null;
            }

            var stadium = new CFBStadium
            {
                Code = code,
                School = ReadString(data, p + 32, 40),
                Name = name,
                Abbreviation = ReadString(data, p + 114, 42),
                Capacity = (data[p + 156] << 24) | (data[p + 157] << 16) | (data[p + 158] << 8) | data[p + 159],
            };

            if (p >= 56)
            {
                ReadPosition(data, p - 56 + 44, stadium);
            }

            return stadium;
        }

        /// <summary>
        /// Longitude then latitude. The original stadiums store west longitudes as positive numbers; the schools added
        /// later (App State, Charlotte, Delaware...) use negative ones, so every longitude is turned into a west (negative)
        /// one. Wembley's 1.0 becomes 1W, close to London's 0.1W.
        /// </summary>
        private static void ReadPosition(byte[] data, int offset, CFBStadium stadium)
        {
            float lon = ReadFloatBigEndian(data, offset);
            float lat = ReadFloatBigEndian(data, offset + 4);
            bool none = (lon == 0 && lat == 0) || (lon == 55 && lat == 25);
            if (none || float.IsNaN(lon) || float.IsNaN(lat))
            {
                return;
            }

            stadium.Latitude = Math.Round(lat, 3);
            stadium.Longitude = -Math.Round(Math.Abs((double)lon), 3);
        }

        private static float ReadFloatBigEndian(byte[] data, int offset)
        {
            var bytes = new[] { data[offset + 3], data[offset + 2], data[offset + 1], data[offset] };
            return BitConverter.ToSingle(bytes, 0);
        }

        private static string ReadString(byte[] data, int offset, int length)
        {
            // Windows-1252 text; a few names end in a trademark sign (Lambeau Field™, Bank of America Stadium®), which is dropped
            var sb = new StringBuilder();
            for (int i = offset; i < offset + length && data[i] != 0; i++)
            {
                byte b = data[i];
                if (b == 0x99 || b == 0xAE || b == 0xA9)
                {
                    continue;
                }

                sb.Append((char)b);
            }

            return sb.ToString().Trim();
        }
        #endregion
    }
}
