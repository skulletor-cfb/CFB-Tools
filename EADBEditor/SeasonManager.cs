using EA_DB_Editor.Scheduling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    internal static class SeasonManager
    {
        public static void CreateNewSeason()
        {
            int seasonYear = Form1.DynastyYear;
            var archiveName = @"D:\NCAA_2014\Archive";
            var destDirName = Path.Combine(archiveName, $"{seasonYear}_Season");
            var SeasonsFile = Path.Combine(archiveName, "Seasons");
            var year = Form1.DynastyYear.ToString();
            bool existingSeason = false;
            var sourceDir = Path.GetFullPath(@".\");
            var scheduleFile = Path.Combine(sourceDir, TelevisionScheduler.TVScheduleFile);

            if (string.IsNullOrEmpty(year) == false && (destDirName.EndsWith("EOY") || destDirName.EndsWith("Season")))
            {
                var seasonsFile = SeasonsFile;
                Seasons seasons = Seasons.FromFile(seasonsFile);

                var seasonAlreadyExisting = seasons.SeasonList.Where(season => season.Year == seasonYear).SingleOrDefault();
                if (seasonAlreadyExisting != null)
                {
                    destDirName = seasonAlreadyExisting.Directory.Replace('/', '\\');
                    existingSeason = true;
                }
                else
                {
                    seasons.AddYearAndWriteToFile(seasonsFile, seasonYear, destDirName);
                }
            }

            //if the destination directory doesnt exist create it
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            File.Copy(scheduleFile, Path.Combine(destDirName, TelevisionScheduler.TVScheduleFile), true);
        }


        public static Seasons FromFile(string seasonsFile)
        {
            if (File.Exists(seasonsFile) && !string.IsNullOrWhiteSpace(File.ReadAllText(seasonsFile)))
            {
                string years = File.ReadAllText(seasonsFile);
                return years.FromJson<Seasons>();
            }
            else
            {
                return new Seasons();
            }
        }
    }

    [DataContract]
    public class Seasons
    {
        public static string ArchiveLocation = "./Archive";
        public static string SeasonsFile { get { return Path.Combine(ArchiveLocation, "Seasons"); } }
        private static Seasons instance;

        List<Season> seasonList;

        [DataMember(Name = "Season")]
        public List<Season> SeasonList
        {
            get
            {
                return seasonList;
            }
            set
            {
                // let's make sure we only have 1 season in the list at any one time
                seasonList = value.Distinct(Season.EqualityComparer).OrderBy(s => s.Year).ToList();
            }
        }


        public Seasons()
        {
            SeasonList = new List<Season>();
        }

        public void AddYearAndWriteToFile(string file, int year, string dir)
        {
            this.SeasonList.Add(new Season { Year = Convert.ToInt32(year), Directory = dir });
            this.ToJsonFile(file);
        }

        public static Seasons FromFile(string seasonsFile)
        {
            if (File.Exists(seasonsFile) && !string.IsNullOrWhiteSpace(File.ReadAllText(seasonsFile)))
            {
                string years = File.ReadAllText(seasonsFile);
                return years.FromJson<Seasons>();
            }
            else
            {
                return new Seasons();
            }
        }

        public static Seasons Instance
        {
            get
            {
                if (instance == null)
                    instance = FromFile(SeasonsFile);
                return instance;
            }
        }
    }
    [DataContract]
    public class Season : IEqualityComparer<Season>
    {
        public static Season EqualityComparer = new Season();

        [DataMember]
        public string Directory { get; set; }
        [DataMember]
        public int Year { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Season;
            if (other == null)
                return false;

            return this.Year == other.Year;
        }

        public bool Equals(Season x, Season y)
        {
            if (x == null && y == null)
                return true;

            return x != null && y != null && x.Year == y.Year;
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(this);
        }

        public int GetHashCode(Season obj)
        {
            return obj.Directory.GetHashCode();
        }
    }
}