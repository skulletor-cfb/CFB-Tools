using EA_DB_Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RefreshRunner
{
    class Program
    {
        static string PreSeasonFile = "DYNASTY-Y{0}";
        static string EndOfSeason = "DYNASTY-Q5";
        static string EndOfRecruting = "DYNASTY-Q7";
        static string ArchiveDir = @"e:\NCAA_2014\Archive";
        static string EndOfSeasonArchiveDir = @"D:\OneDrive\ncaa\EndOfSeasonArchive";
        static string SeasonsFile = ArchiveDir + @"\seasons";
        static string ReportsDir = @".\archive\reports";
        static int StartContinuation = 17; // this is 2067, after this we need to use the continuation file
        static int StartContinuation2 = 75; //this is 2125 , after this we need continuation 2
        static int StartContinuation3 = 133; //this is 2183 , after this we need continuation 3
        static int StartContinuation4 = 147; //this is 2197 , after this we need continuation 4
        static int StartContinuation5 = 205; //this is 2255 , after this we need continuation 5
        static int StartContinuation6 = 263; //this is 2313 , after this we need continuation 6
        static int StartContinuation7 = 321; // this is 2371, after this we need continuation 7
        static int ContinuationConfig1 = 55; //this is 2068-2125
        static int ContinuationConfig2 = 113; //this is 2126-2183
        static int ContinuationConfig3 = 171; //this is 2184-2197
        static int ContinuationConfig4 = 185; //this is 2198-2255
        static int ContinuationConfig5 = 243; //this is 2256-2313
        static int ContinuationConfig6 = 301; //this is 2314-2371
        static int ContinuationConfig7 = 359; //this is 2372-2429
        static string ContinationDir1 = @"e:\NCAA_2014\Archive\2067_Miami_HC_(15-0)_Week_EOY";
        static string ContinationDir2 = @"e:\NCAA_2014\Archive\2125_Season";
        static string ContinationDir3 = @"e:\NCAA_2014\Archive\2183_Season";
        static string ContinationDir4 = @"e:\NCAA_2014\Archive\2197_Season";
        static string ContinationDir5 = @"e:\NCAA_2014\Archive\2255_Season";
        static string ContinationDir6 = @"e:\NCAA_2014\Archive\2313_Season";
        static string ContinationDir7 = @"e:\NCAA_2014\Archive\2371_Season";
        static string CF1 = @"e:\NCAA_2014\Archive\Continuation68to125\continuationfile.txt";
        static string CF2 = @"e:\NCAA_2014\Archive\Continuation126to183\continuationfile.txt";
        static string CF3 = @"e:\NCAA_2014\Archive\Continuation184to197\continuationfile.txt";
        static string CF4 = @"e:\NCAA_2014\Archive\Continuation198to255\continuationfile.txt";
        static string CF5 = @"e:\NCAA_2014\Archive\Continuation256to313\continuationfile.txt";
        static string CF6 = @"e:\NCAA_2014\Archive\Continuation314to371\continuationfile.txt";
        static string CF7 = @"e:\NCAA_2014\Archive\continuationfile.txt";
        static int SetNeutralSiteYear = 156; // the year 2206 in dynasty before this we need
        const string OldNeutralSiteValue = "271,272,276,273,{150;150},{275;277;186},{147;169;153;168},182,184,183";
        const string OldStadiumNickNameOverride = "Stadium=271,BeforeWeek=2,NickName=Cowboys Classic;Stadium=184,BeforeWeek=3,NickName=Rocky Mountain Showdown;Stadium=276,BeforeWeek=3,NickName=Chicago Kickoff;Stadium=183,BeforeWeek=16,NickName=World's Largest Outdoor Cocktail Party;Stadium=182,BeforeWeek=16,NickName=Red River Shootout;Stadium=150,BeforeWeek=2,NickName=Fiesta Kickoff;Stadium=275,BeforeWeek=2,NickName=Atlantic Kickoff;Stadium=186,BeforeWeek=2,NickName=Atlantic Kickoff;Stadium=277,BeforeWeek=2,NickName=Atlantic Kickoff;Stadium=147,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=169,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=153,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=168,BeforeWeek=2,NickName=Sunshine State Kickoff;";

        // year 2207 to 2216
        static Tuple<int, int> NeutralSitesPhase2 = new Tuple<int, int>(157, 166);
        const string Phase2NeutralSitevalue = "271,272,276,273,184,183,182,{277;277},{150;150},{147;169;153;168},{186;186},{275;249;144;185;180;176;251;165;173;145;253;254;160;164;159},{162;4343976}";
        const string Phase2Override = "Stadium=271,BeforeWeek=2,NickName=Cowboys Classic;Stadium=184,BeforeWeek=3,NickName=Rocky Mountain Showdown;Stadium=276,BeforeWeek=3,NickName=Chicago Kickoff;Stadium=183,BeforeWeek=16,NickName=World's Largest Outdoor Cocktail Party;Stadium=182,BeforeWeek=10,NickName=Red River Showdown;Stadium=150,BeforeWeek=2,NickName=Fiesta Kickoff;Stadium=186,BeforeWeek=2,NickName=Belk College Kickoff;Stadium=147,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=169,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=153,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=168,BeforeWeek=2,NickName=Sunshine State Kickoff;Stadium=275,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=277,BeforeWeek=2,NickName=The Kickoff Classic;Stadium=144,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=185,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=180,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=176,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=251,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=165,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=173,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=145,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=253,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=254,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=160,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=164,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=159,BeforeWeek=2,NickName=Pigskin Kickoff Classic;Stadium=162,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=249,BeforeWeek=2,NickName=Pigskin Kickoff Classic;";

        // 2217 to 2289
        static Tuple<int, int> NeutralSitesPhase3 = new Tuple<int, int>(167, 239);
        const string Phase3Override = "RivalryGame=83-89,Stadium=279,BeforeWeek=16,NickName=Iron Skillet Battle;Stadium=278,BeforeWeek=4,NickName=Cowboys Showdown; RivalryGame=11-94,Stadium=279,BeforeWeek=16,NickName=Texas Shootout; RivalryGame=6-93,Stadium=279,BeforeWeek=16,NickName=Southwest Classic;Stadium=271,BeforeWeek=2,NickName=Cowboys Classic;Stadium=184,BeforeWeek=2,NickName=Rocky Mountain Showdown;Stadium=276,BeforeWeek=3,NickName=Chicago Kickoff;Stadium=183,BeforeWeek=16,NickName=World's Largest Outdoor Cocktail Party;Stadium=182,BeforeWeek=16,NickName=Red River Shootout;Stadium=150,BeforeWeek=2,NickName=Fiesta Kickoff;Stadium=186,BeforeWeek=2,NickName=Belk College Kickoff;Stadium=147,BeforeWeek=2,NickName=Orlando Kickoff;Stadium=169,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=153,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=168,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=275,BeforeWeek=2,NickName=Pigskin Classic;Stadium=277,BeforeWeek=2,NickName=The Kickoff Classic;Stadium=144,BeforeWeek=2,NickName=Pigskin Classic;Stadium=185,BeforeWeek=2,NickName=Pigskin Classic;Stadium=180,BeforeWeek=2,NickName=Pigskin Classic;Stadium=176,BeforeWeek=2,NickName=Pigskin Classic;Stadium=251,BeforeWeek=2,NickName=Pigskin Classic;Stadium=165,BeforeWeek=2,NickName=Pigskin Classic;Stadium=173,BeforeWeek=2,NickName=Pigskin Classic;Stadium=145,BeforeWeek=2,NickName=Pigskin Classic;Stadium=253,BeforeWeek=2,NickName=Pigskin Classic;Stadium=254,BeforeWeek=2,NickName=Pigskin Classic;Stadium=160,BeforeWeek=2,NickName=Pigskin Classic;Stadium=164,BeforeWeek=2,NickName=Pigskin Classic;Stadium=159,BeforeWeek=2,NickName=Pigskin Classic;Stadium=162,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=249,BeforeWeek=2,NickName=Pigskin Classic;";
        const string Phase3NeutralSiteValue = "278,279,271,272,276,273,184,183,182,{147;147},{277;277},{150;150},{162;169;153;168},{186;186},{275;249;144;185;180;176;251;165;173;145;253;254;160;164;159}";

        // 2290 to 2348
        static Tuple<int, int> NeutralSitesPhase4 = new Tuple<int, int>(240, 298);
        const string Phase4Override = "Stadium=272,BeforeWeek=2,NickName=Texas Kickoff;RivalryGame=33-79,Stadium=272,BeforeWeek=16,NickName=Bayou Bucket Classic;RivalryGame=83-89,Stadium=279,BeforeWeek=16,NickName=Iron Skillet Battle;Stadium=278,BeforeWeek=4,NickName=Cowboys Showdown;RivalryGame=11-94,Stadium=279,BeforeWeek=16,NickName=Texas Shootout;RivalryGame=6-93,Stadium=279,BeforeWeek=16,NickName=Southwest Classic;Stadium=271,BeforeWeek=2,NickName=Cowboys Classic;Stadium=184,BeforeWeek=4,NickName=Rocky Mountain Showdown;Stadium=276,BeforeWeek=3,NickName=Windy City Classic;Stadium=183,BeforeWeek=16,NickName=World's Largest Outdoor Cocktail Party;Stadium=182,BeforeWeek=16,NickName=Red River Shootout;Stadium=150,BeforeWeek=2,NickName=Cactus Kickoff Classic;Stadium=186,BeforeWeek=2,NickName=Belk College Kickoff;Stadium=147,BeforeWeek=2,NickName=Orlando Kickoff;Stadium=169,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=153,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=168,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=275,BeforeWeek=3,NickName=Pigskin Classic;Stadium=277,BeforeWeek=2,NickName=The Kickoff Classic;Stadium=144,BeforeWeek=2,NickName=Pigskin Classic;Stadium=185,BeforeWeek=2,NickName=Pigskin Classic;Stadium=180,BeforeWeek=2,NickName=Pigskin Classic;Stadium=176,BeforeWeek=2,NickName=Pigskin Classic;Stadium=251,BeforeWeek=2,NickName=Pigskin Classic;Stadium=165,BeforeWeek=3,NickName=Pigskin Classic;Stadium=173,BeforeWeek=2,NickName=Pigskin Classic;Stadium=145,BeforeWeek=2,NickName=Pigskin Classic;Stadium=253,BeforeWeek=2,NickName=Pigskin Classic;Stadium=254,BeforeWeek=2,NickName=Pigskin Classic;Stadium=160,BeforeWeek=2,NickName=Pigskin Classic;Stadium=164,BeforeWeek=2,NickName=Pigskin Classic;Stadium=159,BeforeWeek=2,NickName=Pigskin Classic;Stadium=162,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=249,BeforeWeek=2,NickName=Kickoff in the Capital;Stadium=158,BeforeWeek=2,NickName=Pigskin Classic;Stadium=262,BeforeWeek=8,NickName=Pigskin Classic;Stadium=263,BeforeWeek=3,NickName=Patriot Bowl;Stadium=261,BeforeWeek=2,NickName=Aer Lingus College Football Classic;";
        const string Phase4NeutralSiteValue = "278,279,271,272,276,273,184,183,182,{147;147},{277;277},{150;150},{162;169;153;168},{186;186},{275;249;144;185;180;176;251;165;173;145;253;254;160;164;159}";

        // 2349 to 2356
        static Tuple<int, int> NeutralSitesPhase5 = new Tuple<int, int>(241, 306);
        const string Phase5Override = "Stadium=272,BeforeWeek=2,NickName=Texas Kickoff;RivalryGame=33-79,Stadium=272,BeforeWeek=16,NickName=Bayou Bucket Classic;RivalryGame=83-89,Stadium=279,BeforeWeek=16,NickName=Iron Skillet Battle;Stadium=278,BeforeWeek=4,NickName=Cowboys Showdown;RivalryGame=11-94,Stadium=279,BeforeWeek=16,NickName=Texas Shootout;RivalryGame=6-93,Stadium=279,BeforeWeek=16,NickName=Southwest Classic;Stadium=271,BeforeWeek=2,NickName=Cowboys Classic;Stadium=184,BeforeWeek=4,NickName=Rocky Mountain Showdown;Stadium=276,BeforeWeek=3,NickName=Windy City Classic;Stadium=183,BeforeWeek=16,NickName=World's Largest Outdoor Cocktail Party;Stadium=182,BeforeWeek=16,NickName=Red River Shootout;Stadium=150,BeforeWeek=2,NickName=Cactus Kickoff Classic;Stadium=186,BeforeWeek=2,NickName=Duke's Mayo Classic;Stadium=147,BeforeWeek=2,NickName=Orlando Kickoff;Stadium=169,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=153,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=168,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=275,BeforeWeek=3,NickName=Pigskin Classic;Stadium=277,BeforeWeek=2,NickName=The Kickoff Classic;Stadium=144,BeforeWeek=2,NickName=Pigskin Classic;Stadium=185,BeforeWeek=2,NickName=Pigskin Classic;Stadium=180,BeforeWeek=2,NickName=Pigskin Classic;Stadium=176,BeforeWeek=2,NickName=Pigskin Classic;Stadium=251,BeforeWeek=2,NickName=Pigskin Classic;Stadium=165,BeforeWeek=3,NickName=Pigskin Classic;Stadium=173,BeforeWeek=2,NickName=Pigskin Classic;Stadium=145,BeforeWeek=2,NickName=Pigskin Classic;Stadium=253,BeforeWeek=2,NickName=Pigskin Classic;Stadium=254,BeforeWeek=2,NickName=Pigskin Classic;Stadium=160,BeforeWeek=2,NickName=Pigskin Classic;Stadium=164,BeforeWeek=2,NickName=Pigskin Classic;Stadium=159,BeforeWeek=2,NickName=Pigskin Classic;Stadium=162,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=249,BeforeWeek=2,NickName=Kickoff in the Capital;Stadium=158,BeforeWeek=2,NickName=Pigskin Classic;Stadium=262,BeforeWeek=8,NickName=Pigskin Classic;Stadium=263,BeforeWeek=3,NickName=Patriot Bowl;Stadium=261,BeforeWeek=2,NickName=Aer Lingus College Football Classic;";
        const string Phase5NeutralSiteValue = Phase4NeutralSiteValue;

        //2357 to 2360
        static Tuple<int, int> NeutralSitesPhase6 = new Tuple<int, int>(307, 310);
        const string Phase6Override = Phase5Override;
        const string Phase6NeutralSiteValue = "262,261,263,278,279,271,272,276,273,184,183,182,{147;147},{277;277},{150;150},{162;169;153;168},{186;186},{275;158;144;185;180;176;251;165;173;145;253;254;160;164;159},{249;249}";

        //2361 to 2371
        static Tuple<int, int> NeutralSitesPhase7 = new Tuple<int, int>(311, 321);
        const string Phase7Override = "Stadium=272,BeforeWeek=2,NickName=Texas Kickoff;RivalryGame=33-79,Stadium=272,BeforeWeek=16,NickName=Bayou Bucket Classic;RivalryGame=83-89,Stadium=279,BeforeWeek=16,NickName=Iron Skillet Battle;Stadium=278,BeforeWeek=4,NickName=Cowboys Showdown;RivalryGame=11-94,Stadium=279,BeforeWeek=16,NickName=Texas Shootout;RivalryGame=6-93,Stadium=279,BeforeWeek=16,NickName=Southwest Classic;Stadium=271,BeforeWeek=2,NickName=Cowboys Classic;Stadium=184,BeforeWeek=4,NickName=Rocky Mountain Showdown;Stadium=276,BeforeWeek=3,NickName=Windy City Classic;Stadium=183,BeforeWeek=16,NickName=World's Largest Outdoor Cocktail Party;Stadium=182,BeforeWeek=16,NickName=Red River Shootout;Stadium=258,BeforeWeek=2,NickName=Cactus Kickoff Classic;Stadium=268,BeforeWeek=3,NickName=Duke's Mayo Classic;Stadium=267,BeforeWeek=3,NickName=Orlando Kickoff;Stadium=259,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=264,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=265,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=275,BeforeWeek=3,NickName=Pigskin Classic;Stadium=277,BeforeWeek=2,NickName=The Kickoff Classic;Stadium=144,BeforeWeek=2,NickName=Pigskin Classic;Stadium=185,BeforeWeek=2,NickName=Pigskin Classic;Stadium=180,BeforeWeek=2,NickName=Pigskin Classic;Stadium=176,BeforeWeek=2,NickName=Pigskin Classic;Stadium=251,BeforeWeek=2,NickName=Pigskin Classic;Stadium=165,BeforeWeek=3,NickName=Pigskin Classic;Stadium=173,BeforeWeek=2,NickName=Pigskin Classic;Stadium=145,BeforeWeek=2,NickName=Pigskin Classic;Stadium=253,BeforeWeek=2,NickName=Pigskin Classic;Stadium=254,BeforeWeek=2,NickName=Pigskin Classic;Stadium=160,BeforeWeek=2,NickName=Pigskin Classic;Stadium=164,BeforeWeek=2,NickName=Pigskin Classic;Stadium=159,BeforeWeek=2,NickName=Pigskin Classic;Stadium=266,BeforeWeek=2,NickName=Camping World Kickoff;Stadium=242,BeforeWeek=2,NickName=Kickoff in the Capital;Stadium=158,BeforeWeek=2,NickName=Pigskin Classic;Stadium=262,BeforeWeek=8,NickName=Pigskin Classic;Stadium=263,BeforeWeek=3,NickName=Patriot Bowl;Stadium=261,BeforeWeek=2,NickName=Aer Lingus College Football Classic;";
        const string Phase7NeutralSiteValue = "{268;268},{258;258},262,261,263,278,279,271,272,276,273,184,183,182,{267;267},{277;277},{150;150},{264;265;266;259},{186;186},{275;158;144;185;180;176;251;165;173;145;253;254;160;164;159},{242;242}";

        //2372 to ???
        static Tuple<int, int> NeutralSitesPhase8 = new Tuple<int, int>(322, 999);
        const string Phase8Override = "fill in from app.config";
        const string Phase8NeutralSiteValue = "fill in from app.config";


        static string GetPreseasonFileName(string dir)
        {
            var suffix = dir.Replace(Path.GetDirectoryName(dir) + "\\EndOfY", string.Empty);
            return string.Format(PreSeasonFile, suffix);
        }

        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "records")
            {
                Create();
                return;
            }

            if( args.Length == 1 && args[0] =="coaches")
            {
                ReadCoachData();
                return;
            }

            bool last = false;
            bool preseasonOnly = false;
            int? singleYear = null;
            if (args != null)
            {
                last = args.Any(s => string.Equals(s, "last", StringComparison.OrdinalIgnoreCase));
                preseasonOnly = args.Any(s => string.Equals(s, "pre", StringComparison.OrdinalIgnoreCase));
                int sy;

                if (args != null && args.Length > 0 && Int32.TryParse(args[0], out sy))
                {
                    singleYear = sy;
                }
            }

            if (args.Length == 2 && args[1] == "clean")
            {
                DirectoryCopy(@"D:\vso\Web Page Creator\creator\archive", Path.Combine(Environment.CurrentDirectory, "archive"), true);
            }

            string years = File.ReadAllText(SeasonsFile);
            var seasons = years.FromJson<Seasons>();
            var dirs = Directory.GetDirectories(EndOfSeasonArchiveDir).Where( d => d.Contains("EndOfY")).OrderBy(d => Convert.ToInt32(d.Substring(d.IndexOf("EndOfY") + 6))).ToArray();
            // SeasonsList has 1:1 Map with Directories
            var reportDestinations = seasons.SeasonList.Select(s => ArchiveDir + "\\" + s.Directory.Substring(s.Directory.LastIndexOf('/') + 1)).ToArray();
            if (reportDestinations.Select(rd => Directory.Exists(rd)).All(b => b) == false)
                throw new Exception("error1");

            //            dirs = new[] { dirs.Last() };
            //          reportDestinations = new[] { reportDestinations.Last() };

            var year = last ? dirs.Length - 1 : 0;
            for (int i = year; i < dirs.Length; i++)
            {
                // we specified a specific year
                if (singleYear.HasValue && !dirs[i].Contains("Y" + singleYear.Value))
                {
                    continue;
                }

                year = i;

                bool useContinuation = i > StartContinuation;
                string continuationDir = null;
                int? continuationYear = null;
                string cf = null;

                if (useContinuation)
                {
                    if (i <= StartContinuation2)
                    {
                        continuationDir = ContinationDir1;
                        continuationYear = ContinuationConfig1;
                        cf = CF1;
                    }
                    else if (i <= StartContinuation3)
                    {
                        continuationDir = ContinationDir2;
                        continuationYear = ContinuationConfig2;
                        cf = CF2;
                    }
                    else if( i <= StartContinuation4)
                    {
                        continuationDir = ContinationDir3;
                        continuationYear = ContinuationConfig3;
                        cf = CF3;
                    }
                    else if (i <= StartContinuation5)
                    {
                        continuationDir = ContinationDir4;
                        continuationYear = ContinuationConfig4;
                        cf = CF4;
                    }
                    else if( i<=StartContinuation6)
                    {
                        continuationDir = ContinationDir5;
                        continuationYear = ContinuationConfig5;
                        cf = CF5;
                    }
                    else if(i<=StartContinuation7)
                    {
                        continuationDir = ContinationDir6;
                        continuationYear = ContinuationConfig6;
                        cf = CF6;
                    }
                    else
                    {
                        continuationDir = ContinationDir7;
                        continuationYear = ContinuationConfig7;
                        cf = CF7;
                    }
                }
                Console.WriteLine("Going from {0} to {1}", dirs[i], reportDestinations[i]);
                Console.WriteLine();

                //clear files first
                //Directory.Delete(reportDestinations[i],true);
                //Directory.CreateDirectory(reportDestinations[i]);

                // do preseason
                AppDomain appDomain = null;
                Form1 form = null;
                string dynastyFile = null;

                dynastyFile = Path.Combine(dirs[i], GetPreseasonFileName(dirs[i]));
                if (File.Exists(dynastyFile))
                {
                    appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                    form = CreateForm(appDomain);
                    form.SetContinuation(useContinuation, continuationDir, continuationYear, cf);
                    SetKickoffGameOverrides(year, form);
                    form.OpenDynastyFile(dynastyFile);
                    form.CreatePreseasonMagazine(false);
                    // AppDomain.Unload(appDomain);

                    // copy preseason files
                    foreach (var file in Directory.GetFiles(ReportsDir))
                    {
                        File.Copy(file, Path.Combine(reportDestinations[i], Path.GetFileName(file)), true);
                    }
                }

                dynastyFile = Path.Combine(dirs[i], EndOfSeason);
                if (!preseasonOnly && File.Exists(dynastyFile))
                {
                    // do regular season
                    appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                    form = CreateForm(appDomain);
                    form.SetContinuation(useContinuation, continuationDir, continuationYear, cf);
                    SetKickoffGameOverrides(year, form);
                    form.OpenDynastyFile(dynastyFile);
                    form.CreateReports(false);
                    // AppDomain.Unload(appDomain);

                    // do recruit updates
                    appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                    form = CreateForm(appDomain);
                    form.SetContinuation(useContinuation, continuationDir, continuationYear, cf);
                    SetKickoffGameOverrides(year, form);
                    form.OpenDynastyFile(Path.Combine(dirs[i], EndOfRecruting));
                    form.UpdateRecruitingReports(false);

                    // copy all files
                    Parallel.ForEach(Directory.GetFiles(ReportsDir), file => File.Copy(file, Path.Combine(reportDestinations[i], Path.GetFileName(file)), true));

                    /*
                    foreach (var file in Directory.GetFiles(ReportsDir))
                    {
                        File.Copy(file, Path.Combine(reportDestinations[i], Path.GetFileName(file)), true);
                    }*/

                    File.Copy(Path.Combine(ReportsDir + "\\..", "bowlrecords"), Path.Combine(ArchiveDir, "bowlrecords"), true);
                    // AppDomain.Unload(appDomain);
                }
            }
        }

        static void ReadCoachData()
        {
            var preDir = Path.Combine(EndOfSeasonArchiveDir, "Pre2050");
            var foundCoaches = new Dictionary<string, Coach>();

            for (int i = 49; i >= 20; i--)
            {
                var file = Path.Combine(preDir, "DYNASTY-Y" + i);
                var appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                var form = CreateForm(appDomain);

                var records = File.ReadAllLines(@"e:\NCAA_2014\Archive\2246_Season\thr.csv").Skip(1)
                    .Select(line =>
                    {
                    var split = line.Split(',');
                    return new { TeamId = Convert.ToInt32(split[2]), Year = Convert.ToInt32(split[0]) + 2013, Win = split[1].Split('-')[0].ToInt32(), Loss = split[1].Split('-')[1].ToInt32() };
                    })
                    .ToDictionary(a => Tuple.Create(a.TeamId, a.Year));

                var bowlChamps = File.ReadAllLines(@"e:\NCAA_2014\Archive\2246_Season\bowlchamps.csv").Skip(1)
                    .Select(line =>
                    {
                        var split = line.Split(',');
                        return new { TeamId = Convert.ToInt32(split[4]), Year = Convert.ToInt32(split[0]) + 2013, BowlId = split[2].ToInt32() };
                    })
                    .GroupBy(a => Tuple.Create(a.TeamId, a.Year))
                    .ToDictionary(g => g.Key, g => g.ToArray());

                var ccChamps = File.ReadAllLines(@"e:\NCAA_2014\Archive\2246_Season\cc.csv").Skip(1)
                    .Select(line =>
                    {
                        var split = line.Split(',');
                        return new { TeamId = Convert.ToInt32(split[2]), Year = Convert.ToInt32(split[0]) + 2013, ConfId = split[4].ToInt32() };
                    })
                    .ToDictionary(a => Tuple.Create(a.TeamId, a.Year));

                form.OpenDynastyFile(file);
               var json= form.CookCoaches();
                var coaches = json.FromJson<Dictionary<string, Coach>>();
                foreach (var c in coaches)
                {
                    if (!foundCoaches.ContainsKey(c.Key))
                    {
                        foundCoaches.Add(c.Key, c.Value);
                        var coach = c.Value;

                        if (coach.Position == 0)
                        {
                            var key =Tuple.Create(2013 + i, coach.TeamId);
                            var record = records[key];
                            coach.CareerWin += record.Win;
                            coach.CareerLoss += record.Loss;
                            coach.BowlWins += bowlChamps.ContainsKey(key) ? 1 : 0;
                            coach.CoachBowlLoss += bowlChamps.ContainsKey(key) ? 1 : 0;
                        }
                    }
                }
                return;
            }
        }


        static void SetKickoffGameOverrides(int year, Form1 form)
        {
            if (year <= SetNeutralSiteYear)
            {
                form.NeutralSiteGamesForRecords(OldNeutralSiteValue, OldStadiumNickNameOverride);
            }
            else
            {
                if (year >= NeutralSitesPhase2.Item1 && year <= NeutralSitesPhase2.Item2)
                {
                    form.NeutralSiteGamesForRecords(Phase2NeutralSitevalue, Phase2Override);
                }
                else if (year >= NeutralSitesPhase3.Item1 && year <= NeutralSitesPhase3.Item2)
                {
                    form.NeutralSiteGamesForRecords(Phase3NeutralSiteValue, Phase3Override);
                }
                else if (year >= NeutralSitesPhase4.Item1 && year <= NeutralSitesPhase4.Item2)
                {
                    form.NeutralSiteGamesForRecords(Phase4NeutralSiteValue, Phase4Override);
                }
                else if (year >= NeutralSitesPhase5.Item1 && year <= NeutralSitesPhase5.Item2)
                {
                    form.NeutralSiteGamesForRecords(Phase5NeutralSiteValue, Phase5Override);
                }
                else if (year >= NeutralSitesPhase6.Item1 && year <= NeutralSitesPhase6.Item2)
                {
                    form.NeutralSiteGamesForRecords(Phase6NeutralSiteValue, Phase6Override);
                }
                else if (year >= NeutralSitesPhase7.Item1 && year <= NeutralSitesPhase7.Item2)
                {
                    form.NeutralSiteGamesForRecords(Phase7NeutralSiteValue, Phase7Override);
                }
                else if (year >= NeutralSitesPhase8.Item1 && year <= NeutralSitesPhase8.Item2)
                {
                    // use app.config until phase 9
                }

                // starting in the same year the poinsetta bowl became the Pinstripe Bowl
                form.SetBowlIdOverrides(dict => dict[15] = new Tuple<int, int>(7015, 194));

                // Mobile Alabama Bowl became the LA BOWL in 2357
                form.SetBowlIdOverrides(dict => dict[0] = new Tuple<int, int>(8000, 344));
            }
        }

        static Form1 CreateForm(AppDomain appDomain)
        {
            var form = (Form1)appDomain.CreateInstanceAndUnwrap(typeof(Form1).Assembly.FullName, typeof(Form1).FullName);
            form.SetArchiveLocation(@"e:\NCAA_2014\archive");
            return form;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void Create()
        {
            var lines = File.ReadAllLines(@"D:\OneDrive\ncaa\Default NCAA Files\SchoolRecords.csv")
                .Skip(2)
                .Select(s => s.Split(','))
                .Select(parts => new TeamRecord
                {
                    TeamId = Convert.ToInt32(parts[7]),
                    Key = Convert.ToInt32(parts[4]),
                    Type = Convert.ToInt32(parts[12]),
                    Holder = parts[3],
                    Value = Convert.ToInt32(parts[14]),
                    Opponent = parts[10],
                    DynastyYear = Convert.ToInt32(parts[15]),
                    Week = Convert.ToInt32(parts[9])
                }).ToArray();

            lines.ToJsonFile(@"D:\OneDrive\ncaa\Default NCAA Files\BaseSchoolRecords.txt", true, false);
        }
    }
}