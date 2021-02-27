using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace EA_DB_Editor
{
    [DataContract]
    public class Conference
    {
        public static int[] PowerConferences = ConfigurationManager.AppSettings["Power5"].Split(',').Select(s => Convert.ToInt32(s)).ToArray();
        public static int[] PowerIndependents = GetPowerInd();

        public static int[] GetPowerInd()
        {
            var pi=ConfigurationManager.AppSettings["PowerInd"];
            return string.IsNullOrEmpty(pi)?new int[0]:pi.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
        }

        public static void ToJsonFile()
        {
            Conferences.Values.OrderBy(c => c.Id).ToArray().ToJsonFile(@".\Archive\Reports\conf");
        }

        public int TeamCount
        {
            get
            {
                return Team.Teams.Values.Where(t => t.ConferenceId == this.Id).Count();
            }
        }

        private static Dictionary<int, Conference> conferences;
        public static Dictionary<int, Conference> Conferences { get { return conferences; } }
        public static void Create(MaddenDatabase db)
        {
            if (conferences != null)
                return;

            conferences = new Dictionary<int, Conference>();

            for (int i = 0; i < db.lTables[134].Table.currecords; i++)
            {
                var conf = new Conference
                {
                    Id = db.lTables[134].lRecords[i].lEntries[0].Data.ToInt32(),
                    LeagueId = db.lTables[134].lRecords[i].lEntries[1].Data.ToInt32(),
                    Name = db.lTables[134].lRecords[i].lEntries[2].Data
                };

                conferences.Add(conf.Id, conf);
            }

            // now go thru the divisions
            var table = db.lTables[136];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var confId = table.lRecords[i].lEntries[0].Data.ToInt32();
                var division = new Division(table.lRecords[i]);
                conferences[confId].Divisions.Add(division);
            }

            ToJsonFile();
        }

        [DataMember]
        public int Id { get; set; }
        public int LeagueId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember(Name = "Divisions")]
        public Division[] ConferenceDivisions { get { return this.Divisions.ToArray(); } set { this.Divisions = new List<Division>(value); } }
        public List<Division> Divisions { get; set; }
        public Conference()
        {
            this.Divisions = new List<Division>();
        }
        public Division FindDivision(int id)
        {
            return Divisions.Where(d => d.Id == id).SingleOrDefault();
        }

    }

    [DataContract]
    public class Division
    {
        public Division(MaddenRecord record)
        {
            Id = record.lEntries[1].Data.ToInt32();
            Name = record.lEntries[2].Data;
            SubName = record.lEntries[3].Data;
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string SubName { get; set; }
    }


    public class City
    {
        public const string CitiesCsv = @".\Archive\HTML\cities.csv";
        private static Dictionary<int, City> cities = new Dictionary<int, City>();
        public static Dictionary<int, City> Cities { get { return cities; } }
        public static void Create()
        {
            if (cities.Keys.Count == 0)
            {
#if false
                    var cityData = PocketScout.getData("select * from Cities where City_ID > 0 order by City_ID ASC");
                    foreach( DataRow row in cityData.Rows)
                    {
                        Create(row["City_ID"].ToInt32(), row["City_Name"].ToString(), row["State"].ToString());
                    }
                    ToCSV();
#else
                //read from csv
                using (var reader = new StreamReader(File.OpenRead(CitiesCsv)))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        Create(values[0].ToInt32(), values[1], values[2]);
                    }
                }
#endif
            }
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public static void Create(int id, string name, string state)
        {
            Cities.Add(id, new City { Id = id, Name = name, State = state });
        }
        public static void ToCSV()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in Cities.Keys)
            {
                sb.AppendLine(string.Format("{0},{1},{2}", Cities[key].Id, Cities[key].Name, Cities[key].State));
            }
            Utility.WriteData(CitiesCsv, sb.ToString());
        }
    }


    [DataContract]
    public class GameScore
    {
        [DataMember]
        public ushort Time { get; set; }
        [DataMember]
        public int TeamId { get; set; }
        [DataMember]
        public ushort Quarter { get; set; }
        public ushort Points { get; set; }
        [DataMember(Name="Points")]
        public int ActualPoints { get; set; }
        [DataMember]
        public string Description { get; set; }
        public ushort ScoreType { get; set; }

        public void Parse()
        {
            switch (this.Points)
            {
                case 11:
                    ActualPoints = 6;
                    Description = "TD, XP Missed";
                    break;
                case 12:
                    ActualPoints = 6;
                    Description = "TD, 2pt Failed";
                    break;
                case 14:
                    ActualPoints = 7;
                    Description = "TD, XP";
                    break;
                case 15:
                    ActualPoints = 8;
                    Description = "TD, 2pt";
                    break;
                case 16:
                    if (ScoreType == 5)
                    {
                        ActualPoints = 3;
                        Description = "Field Goal";
                    }
                    else if (ScoreType == 10)
                    {
                        ActualPoints = 2;
                        Description = "Safety";
                    }
                    else if (ScoreType == 1 || ScoreType == 10)
                    {
                        ActualPoints = 6;
                        Description = "TD";
                    }
                    break;
                default:
                    break;
            }
        }
}

    [DataContract]
    public class TeamStat
    {        
        [DataMember()]
        public int RushYards { get; set; }
        
        [DataMember()]
        public int KRYards { get; set; }
        
        [DataMember()]
        public int PassCompletions { get; set; }
        
        [DataMember(EmitDefaultValue=false)]
        public int FirstDowns { get; set; }
        
        [DataMember()]
        public int ThirdDownAttempts { get; set; }
        
        [DataMember()]
        public int FourthDownAttempts { get; set; }
        
        [DataMember()]
        public int TeamId { get; set; }
        
        [DataMember()]
        public int TwoPointConversionAttempts { get; set; }
        
        [DataMember()]
        public int TwoPointConversions { get; set; }
        
        [DataMember()]
        public int Turnovers { get; set; }
        
        [DataMember()]
        public int PassAttempts { get; set; }
        
        [DataMember()]
        public int RushAttempts { get; set; }
        
        [DataMember()]
        public int ThirdDownConversions { get; set; }
        
        [DataMember()]
        public int FourthDownConversions { get; set; }
        
        [DataMember()]
        public int PuntYards { get; set; }
        
        [DataMember()]
        public int Penalties { get; set; }
        
        [DataMember()]
        public int RedZoneFG { get; set; }
        
        [DataMember()]
        public int IntThrown { get; set; }
        
        [DataMember()]
        public int FumblesLost { get; set; }
        
        [DataMember()]
        public int PassYards { get; set; }
        
        [DataMember()]
        public int PRYards { get; set; }
        
        [DataMember()]
        public int PassTD { get; set; }
        
        [DataMember()]
        public int RedZoneTD { get; set; }
        
        [DataMember()]
        public int TimeOfPossesion { get; set; }
        
        [DataMember()]
        public int RushTD { get; set; }
        
        [DataMember()]
        public int Punts { get; set; }
        
        [DataMember()]
        public int PenaltyYards { get; set; }
        
        [DataMember()]
        public int TotalYards { get; set; }
        
        [DataMember()]
        public int OffensiveYards { get; set; }
        
        [DataMember()]
        public int RedZoneVisits { get; set; }
        
        [DataMember()]
        public int Tssa { get; set; }
        
        [DataMember()]
        public int Tsta { get; set; }
        
        [DataMember()]
        public int Tsdi { get; set; }
        
        [DataMember()]
        public int RedZoneFGAllowed { get; set; }
        
        [DataMember()]
        public int InterceptionsByDefense { get; set; }
        
        [DataMember()]
        public int Sacks { get; set; }
        
        [DataMember()]
        public int PassYardsAllowed { get; set; }
        
        [DataMember()]
        public int OpponentsInRedZone { get; set; }
        
        [DataMember()]
        public int FumblesRecovered { get; set; }
        
        [DataMember()]
        public int RedZoneTDAllowed { get; set; }
        
        [DataMember()]
        public int RushingYardsAllowed { get; set; }
        
        [DataMember()]
        public int SpecialTeamYards { get; set; }

        [DataMember()]
        public int DefensiveYards { get { return RushingYardsAllowed + PassYardsAllowed; } set { } }
        public Team Team { get { return Team.Teams[this.TeamId]; } }
    }

    [DataContract]
    public class Stadium
    {
        public static Dictionary<int, Stadium> Stadiums { get; set; }
        public static void Create(MaddenDatabase db)
        {
            if (Stadiums != null)
                return;

            Stadiums = new Dictionary<int, Stadium>();
            var table = db.lTables[163];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var stadium = new Stadium
                {
                    Id = record.GetInt(40),
                    Capacity = record.GetInt(63),
                    Name = record.GetData(56)
                };

                Stadiums.Add(stadium.Id, stadium);
            }

            Stadiums[1023] = new Stadium();

        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int Capacity { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    public class ConferenceChampion
    {
        public static List<ConferenceChampion> ConferenceChampions;
        public static void Create(MaddenDatabase db)
        {
            Conference.Create(db);

            if (ConferenceChampions != null)
                return;

            ConferenceChampions = new List<ConferenceChampion>();
            var table = db.lTables[20];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                ConferenceChampions.Add(new ConferenceChampion
                {
                    ConferenceId = record.GetInt(0),
                    TeamId = record.GetInt(1).GetRealTeamId(),
                    Year = record.GetInt(2) + ContinuationData.ContinuationYear
                });
            }

            if (ContinuationData.UsingContinuationData && ContinuationData.Instance != null)
            {
                ConferenceChampions.AddRange(ContinuationData.Instance.ConferenceChamps);
            }
        }

        public static List<ConferenceChampion> FromCsvFile(string file)
        {
            var lines = file.ReadAllLines().Skip(1).Select(line => line.Split(','));
            return lines.Select(
                line =>
                new ConferenceChampion
                {
                    Year = line[0].ToInt32(),
                    ConferenceId = line[4].ToInt32(),
                    TeamId = line[2].ToInt32()
                }).ToList();
        }

        public static string GetConferenceChampionship(int teamId, int year)
        {
            var confChamp = ConferenceChampions.Where(bc => bc.TeamId == teamId && bc.Year == year).SingleOrDefault();
            if (confChamp != null)
            {
                // we are conference champion return the conference name
                return Conference.Conferences[confChamp.ConferenceId].Name + " Champions";
            }

            return null;
        }

        public static IEnumerable<ConferenceChampion> GetTeamConferenceChampionships(int teamId)
        {
            return ConferenceChampions.Where(bc => bc.TeamId == teamId).OrderByDescending(bc => bc.Year);
        }

        public static IEnumerable<ConferenceChampion> GetConferenceChampions(int confId)
        {
            return ConferenceChampions.Where(bc => bc.ConferenceId == confId).OrderByDescending(bc => bc.Year);
        }

        public static void ToCCFile(string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,ConfId,TeamId");
            foreach (var item in ConferenceChampions.OrderBy(cc => cc.TeamId).ThenByDescending(cc => cc.Year))
            {
                sb.AppendLine(string.Format("{0},{1},{2}", item.Year, item.ConferenceId, item.TeamId));
            }

            Utility.WriteData(@".\Archive\Reports\" + file, sb.ToString());
        }

        public static void ToCsvFile(IEnumerable<ConferenceChampion> list, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,ConfId");
            foreach (var item in list)
            {
                sb.AppendLine(string.Format("{0},{1}", item.Year, item.ConferenceId));
            }

            Utility.WriteData(@".\Archive\Reports\" + file, sb.ToString());
        }

        public int ConferenceId { get; set; }
        public int TeamId { get; set; }
        public int Year { get; set; }
        public Conference Conference { get { return Conference.Conferences[this.ConferenceId]; } }
        public Team Team {
            get {
                if( !Team.Teams.ContainsKey(this.TeamId))
                {
                    return null;
                }
                return Team.Teams[this.TeamId];
            }
        }
    }

    public class BowlChampion
    {
        public const int NationalChampionshipGameId = 39; 
        public static int DynastyFileYear
        {
            get
            {
                return CurrentYear - ContinuationData.ContinuationYear;
            }
        }
        public static int CurrentYear { get; set; }
        public static List<BowlChampion> BowlChampions;
        public static void Create(MaddenDatabase db)
        {
            if (BowlChampions != null)
                return;

            ContinuationData.Create(db);

            BowlChampions = new List<BowlChampion>();
            var table = db.lTables[0];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var bc = new BowlChampion
                {
                    TeamId = record.GetInt(0),
                    Year = record.GetInt(1) + ContinuationData.ContinuationYear,
                    BowlId = record.GetInt(2)
                };

                if (Bowl.BowlIdOverrides.ContainsKey(bc.BowlId) && Bowl.BowlIdOverrides[bc.BowlId].Item2 <= bc.Year)
                    bc.BowlId = Bowl.BowlIdOverrides[bc.BowlId].Item1;


                BowlChampions.Add(bc);

                CurrentYear = Math.Max(CurrentYear, bc.Year);
            }

            if (ContinuationData.UsingContinuationData && ContinuationData.Instance != null)
            {
                BowlChampions.AddRange(ContinuationData.Instance.BowlChamps);

                // first year of a dynasty
                if (CurrentYear == 0)
                {
                    CurrentYear = ContinuationData.Instance.Year;
                    return; 
                }
            }

            // if we haven't played the bowls, we need to increment the current year by 1, aka the assumption that CurrentYear is correct is only true at the end of the season
            if (ScheduledGame.IsSeasonOver(db) == false)
            {
                CurrentYear = CurrentYear + 1;
            }
        }

        public static bool IsNationalChampionshipYear(int teamId, int year)
        {
            return BowlChampions.Where(bc => bc.Year == year && bc.TeamId == teamId && bc.BowlId == NationalChampionshipGameId).Any();
        }

        public static string[] GetBowlChampionships(int teamId, int year)
        {
            return BowlChampions.Where(bc => bc.Year == year && bc.TeamId == teamId && bc.BowlId != 39).Select(bc => Bowl.FindById(bc.BowlId).Name).ToArray();
        }

        public static IEnumerable<BowlChampion> GetTeamBowlChampionships(int teamId)
        {
            return BowlChampions.Where(bc => bc.TeamId == teamId).OrderByDescending(bc => bc.Year);
        }

        public static IEnumerable<BowlChampion> GetBowlChampions(int bowlId)
        {
            return BowlChampions.Where(bc => bc.BowlId == bowlId).OrderByDescending(bc => bc.Year);
        }


        public static void ToBCFile(string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,BowlId,TeamId");
            foreach (var item in BowlChampions.OrderBy(bc=>bc.TeamId).ThenByDescending(bc=> bc.Year))
            {
                sb.AppendLine(string.Format("{0},{1},{2}", item.Year, item.BowlId,item.TeamId));
            }

            Utility.WriteData(@".\Archive\Reports\" + file, sb.ToString());
        }

        public static void ToCsvFile(IEnumerable<BowlChampion> list, string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Year,BowlId");
            foreach (var item in list)
            {
                sb.AppendLine(string.Format("{0},{1}", item.Year, item.BowlId));
            }

            Utility.WriteData(@".\Archive\Reports\" + file, sb.ToString());
        }

        public static List<BowlChampion> GetBowlChampions()
        {
            List<BowlChampion> bowls = new List<BowlChampion>();
            var order = Bowl.PlayoffBowlOrder;
            for (int i = 0; i < order.Length; i++)
            {
                bowls.AddRange(GetBowlChampions(order[i]));
            }

            foreach (var bowl in Bowl.Bowls.Where( kvp => kvp.Value.Week > 16).OrderByDescending(kvp => kvp.Value.Id))
            {
                if (order.Contains(bowl.Value.Id) == false)
                {
                    bowls.AddRange(GetBowlChampions(bowl.Value.Id));
                }
            }

            return bowls;
        }

        public static List<BowlChampion> FromCsvFile(string file)
        {
            var lines = file.ReadAllLines().Skip(1).Select(line => line.Split(','));
            return lines.Select(
                line =>
                new BowlChampion
                {
                    Year = line[0].ToInt32(),
                    BowlId = line[2].ToInt32(),
                    TeamId = line[4].ToInt32()
                }).ToList();
        }

        public int TeamId { get; set; }
        public int Year { get; set; }
        public int BowlId { get; set; }
    }

    public class Award
    {
        public static Dictionary<int, List<Award>> Awards { get; set; }
        public static void Create(MaddenDatabase db)
        {
            if (Awards != null)
                return;

            Awards = new Dictionary<int, List<Award>>();

            var table = db.lTables[71];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var awardID = record.GetInt(3);

                List<Award> list;
                if (!Awards.TryGetValue(awardID, out list))
                {
                    list = new List<Award>();
                    Awards[awardID] = list;
                }

                list.Add(new Award
                {
                    Id = awardID,
                    PlayerId = record.GetInt(0),
                    Rank = record.GetInt(1),
                    Year = record.GetInt(2) + ContinuationData.ContinuationYear
                });
            }
        }

       
        public int PlayerId { get; set; }
        public int Id { get; set; }
        public int Year { get; set; }
        public int Rank { get; set; }
        public Player Player {get {return PlayerDB.Players[this.PlayerId];}         }

        static Dictionary<int, string> AwardNames;

        public static string GetAwardName(int awardId)
        {
            if (AwardNames == null)
            {
                AwardNames = new Dictionary<int, string>();

                using (var fs = File.OpenRead(@".\Archive\HTML\award_names.xml"))
                {
                    var xml = XElement.Load(fs, LoadOptions.None);
                    foreach (var an in xml.Elements("Award_Names"))
                    {
                        var id = (int)an.Element("Award_ID");
                        var name = (string)an.Element("Award_x0020_Name");
                        AwardNames.Add(id, name);
                    }
                }
            }

            return AwardNames[awardId];
        }
    }

    public class SchoolRecord
    {
        public static Dictionary<int, List<Record>> SchoolRecords { get; set; }

        public static bool LoadFromBaseSchoolRecords()
        {
            TeamRecord.Create();

            if (TeamRecord.TeamRecords == null)
                return false;

            SchoolRecords = new Dictionary<int, List<Record>>();

            foreach( var r in TeamRecord.TeamRecords)
            {
                List<Record> records;
                var teamId = r.TeamId;

                if (SchoolRecords.TryGetValue(teamId, out records) == false)
                {
                    records = new List<Record>();
                    SchoolRecords.Add(teamId, records);
                }

                var sr = new Record
                {
                    Type = r.Type,
                    Description = r.Key,
                    Holder = r.UIHolderValue,
                    Value = r.Value,
                    Opponent = r.Opponent,
                    Year = r.RealYear
                };

                records.Add(sr);
            }

            return true; 
        }
        
        public static void Create(MaddenDatabase db, bool recreateUsingRecordsFile = false)
        {
            if (recreateUsingRecordsFile && LoadFromBaseSchoolRecords())
                return;

            if (SchoolRecords != null || LoadFromBaseSchoolRecords())
                return;

            SchoolRecords = new Dictionary<int, List<Record>>();

            var table = db.lTables[159];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var teamId = record.GetInt(7);

                List<Record> records;
                if (SchoolRecords.TryGetValue(teamId, out records) == false)
                {
                    records = new List<Record>();
                    SchoolRecords.Add(teamId, records);
                }

                var sr = new Record
                                {
                                    Type = record.GetInt(12),
                                    Description = record.GetInt(4),
                                    Holder = record.GetData(3),
                                    Value = record.GetInt(14),
                                    Opponent = record.GetData(10),
                                    Year = record.GetInt(15)
                                };

                // fix the year of the holder
                if (ContinuationData.UsingContinuationData && !string.IsNullOrWhiteSpace(record["RCDE"]))
                {
                    var holderYear = sr.Holder.Substring(0,4).ToInt32();
                    var newYear = holderYear + ContinuationData.ContinuationYear;
                    sr.Holder = sr.Holder.Replace(holderYear.ToString(), newYear.ToString());
                }

                records.Add(sr);
            }
        }
    }

    [DataContract]
    public class NcaaRecord
    {
        [DataMember]
        public List<Record> Records { get; set; }

        public static List<Record> AllTimeRecords { get; set; }

        public static void Create(MaddenDatabase db)
        {
            if (AllTimeRecords != null)
                return;

            AllTimeRecords = new List<Record>();
            var table = db.lTables[91];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var row = table.lRecords[i];

                var record = new Record
                {
                    Description = row.GetInt(4),
                    Holder = row.GetData(3),
                    Value = row.GetInt(13),
                    Opponent = row.GetData(9),
                    Year = row.GetInt(14)
                };

                Reconcile(record);
                AllTimeRecords.Add(record);
            }
        }

        public static void ToJson()
        {
            NcaaRecord nr = new NcaaRecord
            {
                Records = AllTimeRecords
            };

            nr.ToJsonFile(@".\archive\reports\ncaarecords");
        }

        public static void Reconcile(Record record)
        {
            if (ContinuationData.Instance != null)
            {
                var continuationRecord = ContinuationData.Instance.NcaaRecords.Records.Where(rec => rec.Description == record.Description).FirstOrDefault();
                if (continuationRecord.Year != 63 && record.Year == 63)
                {
                    record.Year = continuationRecord.Year;
                }
                else if (record.Year != 63)
                {
                    record.Year += ContinuationData.ContinuationYear;
                }
            }
        }
    }

    public class Record
    {
        public int Type { get; set; }
        public int Description { get; set; }
        public int Value { get; set; }
        public string Holder { get; set; }
        public int Year { get; set; }
        public string Opponent { get; set; }
    }

    public enum TeamRecordKeys
    {
        RushingYds = 0 ,
        RushingTD = 1 , 
        PassYds = 2 , 
        PassTD = 3 , 
        Receptions = 4 , 
        RecYds = 5 , 
        RecTD = 6 , 
        Sacks = 7 , 
        INT = 8
    }


    [DataContract]
    public class TeamRecord
    {
        public const string BaseSchoolRecordsFile = @".\archive\BaseSchoolRecords.txt";
        public static TeamRecord[] TeamRecords;
        public static Dictionary<int, List<TeamRecord>> TeamRecordDict;

        public static void Commit(MaddenDatabase db)
        {
            // pull in the latest records from the table for career/season
            var table = db.lTables[159].lRecords.Where(mr => mr["RCDY"].ToInt32() == BowlChampion.DynastyFileYear && mr["RCDT"].ToInt32() != 0).ToArray();
            for (int i = 0; i < table.Length; i++)
            {
                var record = table[i];
                var teamId = record.GetInt(7).GetRealTeamId();
                var holder = record["RCDH"].Substring(5);

                SetNewRecord((TeamRecordKeys)record["RCDI"].ToInt32(), record["RCDV"].ToInt32(), PlayerDB.Find(teamId, holder[0], holder.Substring(2)), null, Int32.MaxValue, record["RCDT"].ToInt32());
            }

            TeamRecords.ToJsonFile(BaseSchoolRecordsFile, true, false);
        }

        public static void Create()
        {
            if (File.Exists(BaseSchoolRecordsFile))
            {
                TeamRecords = BaseSchoolRecordsFile.FromJsonFile<TeamRecord[]>(true);

                if (TeamRecords != null)
                {
                    TeamRecordDict = TeamRecords.GroupBy(tr => tr.TeamId).ToDictionary(g => g.Key, g => g.ToList());
                    return;
                }
            }

            TeamRecordDict = new Dictionary<int, List<TeamRecord>>();
        }

        public static void SetNewRecord(TeamRecordKeys key, int value, Player player, ScheduledGame game, int maxValue=1023, int recordType=0)
        {
            if (player == null)
                return; 

            if (value > 0 && value < maxValue)
            {
                var record = GetRecordForStat(player.TeamId, key, recordType);

                if (record != null && record.Value < value)
                {
                    record.PreviousRecordHolder = record.RealYear + " " + record.Holder;
                    record.Holder = player.Name;
                    record.Value = value;

                    if (game == null)
                        record.Opponent = string.Empty;
                    else
                    {
                        record.Week = game.Week;
                        record.Opponent = game.AwayTeamId == player.TeamId ? TeamNames[game.HomeTeamId] : TeamNames[game.AwayTeamId];
                    }
                                        
                    record.DynastyYear = BowlChampion.CurrentYear;
                    record.RealYear = BowlChampion.CurrentYear + Utility.StartingYear;
                }
            }
        }

        private static Dictionary<int, string> teamNames;

        public static Dictionary<int, string> TeamNames
        {
            get
            {
                if (teamNames == null || teamNames.Count == 0)
                {
                    try
                    {
                        teamNames = Form1.MainForm.maddenDB.lTables[167].lRecords.ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record["TDNA"]);
                    }
                    catch
                    {
                        teamNames = new Dictionary<int, string>();
                    }
                }

                return teamNames;
            }
        }

        public static bool IsNewRecord(int teamId, TeamRecordKeys key , int value, int recordType)
        {
            var record = GetRecordForStat(teamId, key,  recordType);

            if (record != null)
            {
                return record.Value < value;
            }

            return false; 
        }

        public static TeamRecord GetRecordForStat(int teamId, TeamRecordKeys stat, int recordType  )
        {
            List<TeamRecord> records;

            if( TeamRecordDict.TryGetValue( teamId , out records))
            {
                return records.Where(r => r.Key == (int)stat  && r.Type == recordType).FirstOrDefault();
            }

            return null; 
        }

        /*
         * RBKS table
            RCDM - record team
            RCDI - record key e.g. INTs/REC/ETC
            RCDT - Record Type .  0=game, 1 = season , 2 = career
            RCDH = Record Holder
            RCDV = Record Value
            RCDO = Record Opponent
            RCDY = Record Year
            RCDE = Previous Holder
         * */
        [DataMember]
        public int TeamId { get; set; }

        //INTs/REC/ETC        
        [DataMember]
        public int Key { get; set; }

        //0=game, 1 = season , 2 = career       
        [DataMember]
        public int Type { get; set; }

        /// <summary>
        /// 2068 M.McDaniel
        /// </summary>
        [DataMember]
        public string Holder { get; set; }

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public string Opponent { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int DynastyYear { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int RealYear { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PreviousRecordHolder { get; set; }

        [DataMember]
        public int Week { get; set; }

        public string UIHolderValue
        {
            get
            {
                if( Holder[1]== ' ')
                {
                    string lastName = Holder.Substring(Holder.IndexOf(' ') + 1);
                    return string.Format("{0} {1}.{2}", RealYear, Holder[0], lastName);
                }

                return RealYear + " " + Holder;
            }
        }

        public string RecordHolderValue
        {
            get
            {
                string lastName = Holder.Substring(Holder.IndexOf(' ') + 1);
                return string.Format("{0} {1}.{2}", RealYear, Holder[0], lastName);
            }
        }

        static char[] delimiter = new char[] { ' ', '.' };

        public override int GetHashCode()
        {
            return CreateKey(this.TeamId, this.Key, this.Type);
        }

        public override bool Equals(object obj)
        {
            var other = obj as TeamRecord;
            return other != null &&
                other.TeamId == this.TeamId &&
                other.Key == this.Key &&
                other.Type == this.Type;
        }

        public static int CreateKey(int id, int key, int type)
        {
            return type << 24 | key << 16 | id;
        }
    }
}
