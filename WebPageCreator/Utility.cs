using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EA_DB_Editor
{
    public enum DefensivePlaybook
    {
        DP_43 = 0,
        DP_34 = 1,
        DP_335 = 2,
        DP_425 = 3,
        Multi = 4,
        Multi34 = 5,
        Multi43 = 6
    }
    public enum ShowTeamRank
    {
        None,
        Top25,
        All
    }

    public enum TeamRankDisplay
    {
        BCS,
        AP,
        Coach
    }

    public static class Utility
    {
        public static bool ZipFiles = ConfigurationManager.AppSettings["ZipFiles"].ToBool();
        public static ShowTeamRank ShowTeamRank = GetAppConfigValueEnum("ShowTeamRank", ShowTeamRank.All);
        public static TeamRankDisplay TeamRankDisplay = GetAppConfigValueEnum("TeamRankPoll", TeamRankDisplay.BCS);
        public static int PlayoffTeamCount = ConfigurationManager.AppSettings["PlayoffTeamCount"].ToInt32();

        public static string LoadTeamTemplate(string file, bool isPreseason)
        {
            var content = File.ReadAllText(@".\archive\html\" + file);
            var links = File.ReadAllText(Path.Combine(@".\archive\html", isPreseason ? "PreseasonTeamPageLinkTemplate.txt" : "TeamPageLinkTemplate.txt"));
            var scheduleTemplate = File.ReadAllText(Path.Combine(@".\archive\html", isPreseason ? "PreseasonScheduleTemplate.txt" : "ScheduleTemplate.txt"));
            content = content.Replace("{LINK_TEMPLATE}", links);
            content = content.Replace("{SCHEDULE_TEMPLATE}", scheduleTemplate);
            return content;
        }

        public static bool ToBool(this string val)
        {
            bool b;

            if (bool.TryParse(val, out b))
            {
                return b;
            }

            return false;
        }

        public static void ToCsvFile<T>(this IEnumerable<T> items, string[] keys, Func<T, string> lines, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(",", keys));
            foreach (var item in items)
            {
                sb.AppendLine(lines(item));
            }

            var filePath = @".\archive\reports\" + file;

            if (ZipFiles)
            {
                Compression.Instance.WriteToFile(filePath, sb.ToString());
            }
            else
            {
                File.WriteAllText(filePath, sb.ToString());
            }
        }

        public static string ReadCsv(string file)
        {
            if (!File.Exists(file))
                return null;

            if (ZipFiles)
            {
                return Compression.Instance.ReadFromFile(file);
            }

            return File.ReadAllText(file);
        }

        static T GetAppConfigValueEnum<T>(string key, T defaultValue)
        {
            try
            {
                var rank = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(rank) ? defaultValue : (T)Enum.Parse(typeof(T), rank);
            }
            catch { }
            return defaultValue;
        }

        static int? startingYear;
        public static int StartingYear
        {
            get
            {
                if (startingYear.HasValue == false)
                {
                    startingYear = Convert.ToInt32(ConfigurationManager.AppSettings["StartingYear"]);
                }
                return startingYear.Value;
            }
        }

        public static string ToJson<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
                js.WriteObject(ms, obj);

                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }
        }



        public static void ToJsonFile<T>(this T obj, string file, bool dontZip = false, bool removeExtension = true)
        {
            if (removeExtension && file.EndsWith(".txt"))
            {
                file = file.Substring(0, file.Length - 4);
            }

            string json = null;

            using (var ms = new MemoryStream())
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
                js.WriteObject(ms, obj);

                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    json = sr.ReadToEnd();
                }
            }

            if (ZipFiles && !dontZip)
            {
                Compression.Instance.WriteToFile(file, json);
            }
            else
            {
                File.WriteAllText(file, json);
            }
        }

        public static void WriteData(string file, string data, bool skipCompression=false)
        {
            if (ZipFiles && !skipCompression)
            {
                Compression.Instance.WriteToFile(file, data);
            }
            else
            {
                File.WriteAllText(file, data);
            }
        }

        public static string[] ReadAllLines(this string file)
        {
            if (ZipFiles)
            {
                return Compression.Instance.ReadAllLines(file);
            }
            else
            {
                return File.ReadAllLines(file);
            }
        }

        public static string ReadData(this string file)
        {
            if (ZipFiles)
            {
                return Compression.Instance.ReadFromFile(file);
            }
            else
            {
                return File.ReadAllText(file);
            }
        }

        public static T FromJsonFile<T>(this string file, bool dontZip = false)
        {
            string json = null;

            if (ZipFiles && !dontZip)
            {
                json = Compression.Instance.ReadFromFile(file);
            }
            else
            {
                json = File.ReadAllText(file);
            }

            return json.FromJson<T>();
        }

        public static T FromJson<T>(this string json)
        {
            var js = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)js.ReadObject(ms);
            }
        }

        public static ushort GetUShort(this MaddenRecord record, int idx)
        {
            try
            {
                return Convert.ToUInt16(record.lEntries[idx].Data);
            }
            catch (OverflowException)
            {
                var uint32 = Convert.ToUInt32(record.lEntries[idx].Data);
                return (ushort)(uint.MaxValue - uint32);
            }
        }

        public static int GetSignedInt(this MaddenRecord record, int idx, int negativeBoundary)
        {
            var value = record.GetInt(idx);
            return value.GetSignedInt(negativeBoundary);
        }

        public static int GetSignedInt(this int value, int negativeBoundary)
        {
            // e.g if 2048, look for a value greater than 1024, this means we have a negative number
            if (value > negativeBoundary - 100)
            {
                value = value - negativeBoundary;
            }

            return value;
        }

        public static int GetInt(this MaddenRecord record, int idx)
        {
            return record.lEntries[idx].Data.ToInt32();
        }

        public static string GetData(this MaddenRecord record, int idx)
        {
            return record.lEntries[idx].Data;
        }

        public static bool IsValidTeam(this int teamId)
        {
            return teamId != 0 && (teamId < 236 || (teamId >= 901 && teamId <= 912));
        }

        public static bool IsFCS(this int teamId)
        {
            return teamId >= 160 && teamId <= 165;
        }

        public static string ToWinningOrLosingStreak(this int streak)
        {
            // 255 is a -1 streak
            if (streak > 0 && streak < 100)
            {
                return string.Format("Won {0}", streak);
            }
            else if (streak > 100 && streak < 256)
            {
                return string.Format("Lost {0}", 256 - streak);
            }

            return string.Empty;
        }

        public static string ToJob(this int type)
        {
            string Type = "";
            if (type == 0) { Type = "Head Coach"; }
            if (type == 1) { Type = "Off Coord"; }
            if (type == 2) { Type = "Def Coord"; }
            return Type;
        }

        public static string ToPositionAbbreviation(this int type)
        {
            string Type = "";
            if (type == 0) { Type = "HC"; }
            if (type == 1) { Type = "OC"; }
            if (type == 2) { Type = "DC"; }
            return Type;
        }

        public static string ToTeamName(this int id)
        {
            return PocketScout.TeamNameLookup(id);
        }
        public static int ToInt32(this object obj)
        {
            return Convert.ToInt32(obj);
        }

        public static string ToPositionName(this object obj)
        {
            return obj.ToInt32().ToPositionName();
        }

        public static bool IsOffensivePosition(this int position)
        {
            return position < 10;
        }

        public static bool IsDefensivePosition(this int position)
        {
            return position >= 10 && position <= 18;
        }

        public static bool IsSTPosition(this int position)
        {
            return position == 19 || position == 20;
        }

        public static int[][] GetPositionGroups()
        {
            return new int[][] { 
                new int[]{0,1}, //QB
                new int[]{1,2}, //HB
                new int[]{2,1}, //FB
                new int[]{3,3}, //WR
                new int[]{4,2}, //TE
                new int[]{5,1}, //LT
                new int[]{9,1}, //RT
                new int[]{6,1}, //LG
                new int[]{8,1,}, //RG
                new int[]{7,1},  //C
                new int[]{10,11,2}, //DE
                new int[]{12,2}, //DT
                new int[]{13,15,2}, //OLB
                new int[]{14,2}, //MLB
                new int[]{16,3}, //CB
                new int[]{17,1}, //FS
                new int[]{18,1}, //SS
                new int[]{19,20,2}, //ST
            };
        }

        public static string ToPositionName(this int POS)
        {
            String POS_NAME = "";
            if (POS == 0) { POS_NAME = "QB"; }
            if (POS == 1) { POS_NAME = "HB"; }
            if (POS == 2) { POS_NAME = "FB"; }
            if (POS == 3) { POS_NAME = "WR"; }
            if (POS == 4) { POS_NAME = "TE"; }
            if (POS == 5) { POS_NAME = "LT"; }
            if (POS == 6) { POS_NAME = "LG"; }
            if (POS == 7) { POS_NAME = "C"; }
            if (POS == 8) { POS_NAME = "RG"; }
            if (POS == 9) { POS_NAME = "RT"; }
            if (POS == 10) { POS_NAME = "LE"; }
            if (POS == 11) { POS_NAME = "RE"; }
            if (POS == 12) { POS_NAME = "DT"; }
            if (POS == 13) { POS_NAME = "LOLB"; }
            if (POS == 14) { POS_NAME = "MLB"; }
            if (POS == 15) { POS_NAME = "ROLB"; }
            if (POS == 16) { POS_NAME = "CB"; }
            if (POS == 17) { POS_NAME = "FS"; }
            if (POS == 18) { POS_NAME = "SS"; }
            if (POS == 19) { POS_NAME = "K"; }
            if (POS == 20) { POS_NAME = "P"; }
            if (POS == 21) { POS_NAME = "KR"; }
            if (POS == 22) { POS_NAME = "PR"; }
            if (POS == 1000) { POS_NAME = "RET"; }
            return POS_NAME;
        }

        public static void AddJs(TextWriter tw)
        {
            tw.Write("<script src=\"../HTML/utility.js\"></script>");
            tw.Write("<script src=\"../jquery.min.js\"></script>");
        }

        public static void WriteNavBarAndHeader(TextWriter tw, string title, string loadFunc)
        {
            WriteNavBarAndHeader(tw, title, loadFunc, string.Empty);
        }

        public static void WriteNavBarAndHeader(TextWriter tw, string title, string loadFunc, string arg)
        {
            arg = arg == null ? string.Empty : arg;
            tw.Write(string.Format("<html><head><title>{0}</title><link rel=stylesheet type=text/css href=../HTML/styles.css>", title));
            Utility.AddJs(tw);
            tw.Write("</head>");
            loadFunc = string.IsNullOrEmpty(loadFunc) ? string.Empty : "onload=\"" + loadFunc + "(" + arg + ")\"";
            tw.Write(string.Format("<body {0} topmargin=0 leftmargin=0 marginheight=70 marginwidth=0>", loadFunc));
            tw.WriteLine(NavBar);
        }

        private static string NavBar
        {
            get
            {
                try
                {
                    var nav = File.ReadAllText("./Archive/HTML/Nav_Bar");
                    var confs = Conference.Conferences.Values.Where(c => c.Id != 17 && Team.Teams.Values.Any(t => t.ConferenceId == c.Id)).OrderBy(c => c.Name);
                    var sb = new StringBuilder();
                    foreach (var cc in confs)
                    {
                        var first = string.Empty;
                        if (cc == confs.Last())
                        {
                            first = "class='last'";
                        }
                        sb.AppendLine(string.Format("<li {0}><a href='Standings.html#{1}'><span>{2}</span></a></li>", first, cc.Name.Replace(" ", string.Empty), cc.Name));
                    }

                    return string.Format(nav, sb.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }
        }

        public static int CalculateWinPct(int win, int loss)
        {
            if ((win + loss) == 0)
                return 0;

            var pct = 10000 * win / (win + loss);
            var round = pct % 10 >= 5 ? 1 : 0;
            return round + pct / 10;
        }

        public static V GetValueOrDefault<T, V>(this Dictionary<T, V> dict, T key, V defaultValue)
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        public static string CalculateYear(this Player player, int modifier)
        {
            return player.CalculateYear(modifier, false);
        }

        public static string CalculateYear(this Player player, int modifier, bool omitRedshirtDesignation)
        {
            if (modifier < 0)
                modifier = 0;

            var isRedShirt = player.IsRedShirt;
            string Year = "";
            switch (player.Year - modifier)
            {
                case 0:
                    Year = "Fr";
                    break;
                case 1:
                    Year = "So";
                    break;
                case 2:
                    Year = "Jr";
                    break;
                case 3:
                    Year = "Sr";
                    break;
                case -1:
                    // we got a player who redshirted after his freshman year
                    Year = "Fr";
                    isRedShirt = false;
                    break;
                default:
                    throw new Exception("Not a valid year");
            }

            if (omitRedshirtDesignation)
                return Year;

            return isRedShirt ? "Rs-" + Year : Year;
        }

        public static string CalculateHgt(this int heigth)
        {
            string Hgt = "";
            if (heigth == 65) { Hgt = "5'5"; }
            if (heigth == 66) { Hgt = "5'6"; }
            if (heigth == 67) { Hgt = "5'7"; }
            if (heigth == 68) { Hgt = "5'8"; }
            if (heigth == 69) { Hgt = "5'9"; }
            if (heigth == 70) { Hgt = "5'10"; }
            if (heigth == 71) { Hgt = "5'11"; }
            if (heigth == 72) { Hgt = "6'0"; }
            if (heigth == 73) { Hgt = "6'1"; }
            if (heigth == 74) { Hgt = "6'2"; }
            if (heigth == 75) { Hgt = "6'3"; }
            if (heigth == 76) { Hgt = "6'4"; }
            if (heigth == 77) { Hgt = "6'5"; }
            if (heigth == 78) { Hgt = "6'6"; }
            if (heigth == 79) { Hgt = "6'7"; }
            if (heigth == 80) { Hgt = "6'8"; }
            if (heigth == 81) { Hgt = "6'9"; }
            if (heigth == 82) { Hgt = "6'10"; }
            if (heigth == 83) { Hgt = "6'11"; }
            if (heigth == 84) { Hgt = "7'0"; }
            if (heigth == 85) { Hgt = "7'1"; }
            return Hgt;
        }

    }
}