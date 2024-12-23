using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
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
            this.ToJsonFile(file, true);
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

        public static int CurrentYear
        {
            get
            {
                return BowlChampion.CurrentYear + Utility.StartingYear;
            }
        }

        public static Season LastYear
        {
            get
            {
                var lastYear = CurrentYear - 1;
                return Instance.SeasonList.Where(s => s.Year == lastYear).FirstOrDefault();
            }
        }

        public static string LastYearDirectory
        {
            get
            {
                return Path.Combine(Seasons.ArchiveLocation, Seasons.LastYear.Directory.Replace("./Archive", "."));
            }
        }

        public static string DirectoryForYear(int year)
        {
            var season = Instance.SeasonList.Where( s=>s.Year == year).FirstOrDefault();
            if( season == null)
                return null ;

            return Path.Combine(Seasons.ArchiveLocation, season.Directory.Replace("./Archive", "."));
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
