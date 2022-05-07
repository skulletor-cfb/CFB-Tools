using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

/*
 * Team

Most Points in Quarter
Most Points in Half

 

Most Combined Offense
Most Combined Passing
Most Combined Rushing
 

            //Team Rushing TD
            //Team Passing TD
            //Team TD
            //Team FG 

Team Sacks
Team INT


 

            //Most Points 
 

Most Points by Kicker

Most FG

Most FG Att

Longest FG

Most XP Att

Most XP Made

Most Punts

Longest Punt

Highest Punt Avg

 

Return Game

 

Most Punt Returns
Most KR
*/

namespace EA_DB_Editor
{
    #region Comparer
    [DataContract]
    public class TeamMap
    {
        [DataMember]
        public int TeamId { get; set; }

        [DataMember]
        public string School { get; set; }

        [DataMember]
        public string Mascot { get; set; }
    }

    public class PlayerRecordComparer<T> : IEqualityComparer<T> where T : PlayerRecord
    {
        public bool Equals(T x, T y)
        {
            return x.PlayerId == y.PlayerId && x.Year == y.Year;
        }

        public int GetHashCode(T obj)
        {
            return obj.Year << 16 | obj.PlayerId;
        }
    }

    public static class RecordComparer
    {
        public static PlayerRecordComparer<PlayerStatRecord> PlayerComparer = new PlayerRecordComparer<PlayerStatRecord>();
        public static RecordComparer<BiggestWin> BiggestWinComparer = new RecordComparer<BiggestWin>();
        public static RecordComparer<PointsRecord> PointsComparer = new PointsRecordsComparer();
        public static RecordComparer<CombinedPointsRecord> CombinedPointsComparer = new RecordComparer<CombinedPointsRecord>();
        public static RecordComparer<YardageRecord> MostYardageComparer = new RecordComparer<YardageRecord>();

        public static List<PlayerStatRecord> OrderAndTrim(this List<PlayerStatRecord> stats)
        {
            return stats.OrderAndTrim(PlayerComparer, p => p.Value, true);
        }

        public static List<T> OrderAndTrim<T>(this List<T> list, IEqualityComparer<T> comparer, Func<T, int> fx, bool greaterThanZeroOnly = false)
        {
            var ordered = list.Distinct(comparer)
                .Where(r => !greaterThanZeroOnly || fx(r) > 0)
                .OrderByDescending(p => fx(p)).ToList();
            return ordered.Trim(comparer, fx);
        }

        public static List<T> Trim<T>(this List<T> ordered, IEqualityComparer<T> comparer, Func<T, int> fx)
        {
            var takenValues = 1;
            for (int i = 1; i < ordered.Count; i++)
            {
                if (fx(ordered[i]) != fx(ordered[i - 1]) &&
                    takenValues >= IndividualBowlRecords.BowlHistoryLookback)
                {
                    break;
                }

                takenValues++;
            }

            return ordered.Take(takenValues).ToList();
        }
    }

    public class RecordComparer<T> : IEqualityComparer<T> where T : GameDescriptor
    {
        public virtual bool Equals(T x, T y)
        {
            return x.Year == y.Year && x.By == y.By && x.Against == y.Against;
        }

        public int GetHashCode(T obj)
        {
            return obj.Year;
        }
    }

    public class YardageRecordsComparer : RecordComparer<YardageRecord>
    {
        public override bool Equals(YardageRecord x, YardageRecord y)
        {
            return base.Equals(x, y) &&
                x.Yards == y.Yards;
        }
    }

    public class PointsRecordsComparer : RecordComparer<PointsRecord>
    {
        public override bool Equals(PointsRecord x, PointsRecord y)
        {
            return base.Equals(x, y) &&
                x.Points == y.Points;
        }
    }
    #endregion
    #region Record classes
    [DataContract]
    public abstract class GameDescriptor
    {
        protected GameDescriptor(ScheduledGame game)
        {
            GameId = game.Key;
            Year = BowlChampion.CurrentYear;
        }

        public virtual void SetBy(ScheduledGame game, int by)
        {
            this.By = by;
            this.Against = game.AwayTeamId == by ? game.HomeTeamId : game.AwayTeamId;
            this.SetScore(game);
        }

        public void SetScore(ScheduledGame game)
        {
            this.Score = string.Format("{0}-{1}",
                this.By == game.HomeTeamId ? game.HomeScore : game.AwayScore,
                this.Against == game.HomeTeamId ? game.HomeScore : game.AwayScore);
        }

        [DataMember]
        public string GameId { get; set; }

        [DataMember]
        public int By { get; set; }

        [DataMember]
        public int Against { get; set; }

        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public string Score { get; set; }
    }

    [DataContract]
    public class BiggestWin : GameDescriptor
    {
        public BiggestWin(ScheduledGame game)
            : base(game)
        {
            this.PointDiff = Math.Abs(game.HomeScore - game.AwayScore);
            this.SetBy(game, game.WinningTeam.Id);
        }

        [DataMember]
        public int PointDiff { get; set; }
    }

    [DataContract]
    public class PointsRecord : GameDescriptor
    {
        public PointsRecord(ScheduledGame game,bool homeTeam)
            : base(game)
        {
            if (homeTeam)
            {
                this.Points = game.HomeScore;
                this.SetBy(game, game.HomeTeamId);
            }
            else
            {
                this.Points = game.AwayScore;
                this.SetBy(game, game.AwayTeamId);
            }
        }

        [DataMember]
        public int Points { get; set; }
    }

    [DataContract]
    public class CombinedPointsRecord : GameDescriptor
    {
        public CombinedPointsRecord(ScheduledGame game)
            : base(game)
        {
            this.Points = game.HomeScore + game.AwayScore;
            this.SetBy(game, game.WinningTeam.Id);
        }

        [DataMember]
        public int Points { get; set; }
    }
    #endregion

    [DataContract]
    public class BowlRecords
    {
        public static readonly string BowlRecordsFile = Path.Combine(@".\archive", "bowlrecords");
        public static BowlRecords Instance { get; set; }

        private Dictionary<int, IndividualBowlRecords> recordsDict;

        public Dictionary<int, IndividualBowlRecords> RecordsDict
        {
            get
            {
                if (recordsDict == null)
                {
                    recordsDict = Records.ToDictionary(r => r.BowlId);
                }

                return recordsDict;
            }
        }

        [DataMember]
        public List<IndividualBowlRecords> Records { get; set; }

        [DataMember]
        public List<TeamMap> Teams { get; set; }

        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            if (isPreseason)
                return;

            // make sure we have our data structure
            EnsureInstanceExists();

            foreach (var bowl in Bowl.Bowls.Values.Where(b => b.ScheduleGame.Week >= 16))
            {
                if (Instance.RecordsDict.ContainsKey(bowl.Id) == false)
                {
                    Instance.RecordsDict[bowl.Id] = new IndividualBowlRecords(bowl.Id, bowl.Name);
                    Instance.Records.Add(Instance.RecordsDict[bowl.Id]);
                }

                Instance.RecordsDict[bowl.Id].Prepare(bowl);
            }

            // kick off games
            foreach (var ng in ScheduledGame.KickOffGames)
            {
                if (Instance.RecordsDict.ContainsKey(ng.Id) == false)
                {
                    Instance.RecordsDict[ng.Id] = new IndividualBowlRecords(ng.Id);
                    Instance.Records.Add(Instance.RecordsDict[ng.Id]);
                }

                foreach (var gamePlayed in ScheduledGame.Schedule.Values.Where(sg => sg.Week < 3 && ng.Id == sg.SiteId))
                {
                    Instance.RecordsDict[ng.Id].Prepare(gamePlayed);
                }
            }

            // write it out to the file
            Instance.ToJsonFile(BowlRecordsFile);
        }

        public static void EnsureInstanceExists()
        {
            if (Instance != null)
                return;

            // create the bowl record file if it doesn't exist, otherwise read it
            if (File.Exists(BowlRecordsFile))
            {
                BowlRecords.Instance = BowlRecordsFile.FromJsonFile<BowlRecords>();
            }
            else if (Bowl.Bowls != null && Bowl.Bowls.Count > 0 && Team.Teams != null && Team.Teams.Count > 0)
            {
                BowlRecords.Instance = new BowlRecords
                {
                    Records = Bowl.Bowls.Values.Where(b => b.Week >= 16).Select(b => new IndividualBowlRecords(b.Id, b.Name)).ToList(),
                    Teams = Team.Teams.Values.Select(t => new TeamMap
                    {
                        TeamId = t.Id.GetRealTeamId(),
                        School = t.Name,
                        Mascot = t.Mascot
                    }).ToList()
                };
            }
        }
    }

    [DataContract]
    public class IndividualBowlRecords
    {
        public static int BowlHistoryLookback = Convert.ToInt32(ConfigurationManager.AppSettings["BowlHistoryLookback"]);
        public IndividualBowlRecords(int id,string name=null)
        {
            this.BowlId = id;
            this.EnsureInstantiatedLists();
            this.Name = name; 
        }

        [DataMember]
        public int BowlId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<BiggestWin> BiggestWins { get; set; }

        [DataMember]
        public List<PointsRecord> MostPoints { get; set; }

        [DataMember]
        public List<CombinedPointsRecord> MostCombinedPoints { get; set; }

        [DataMember]
        public List<CombinedPointsRecord> FewestCombinedPoints { get; set; }

        [DataMember]
        public List<YardageRecord> MostOffensiveYards { get; set; }

        [DataMember]
        public List<YardageRecord> MostPassingYards { get; set; }

        [DataMember]
        public List<YardageRecord> MostRushingYards { get; set; }

        [DataMember]
        public List<PointsRecord> LeastPoints { get; set; }

        [DataMember]
        public List<YardageRecord> LeastOffensiveYards { get; set; }

        [DataMember]
        public List<YardageRecord> LeastPassingYards { get; set; }

        [DataMember]
        public List<YardageRecord> LeastRushingYards { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerTotalOffense { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerOffensiveTD { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRushingAtt { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRushingYdsNonQB { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRushingYdsQB { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRushingYPC { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRushingTD { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerPassAtt { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerCompletions { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerPassingYards { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerPassingTD { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerCompletionPct { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerYardsPerPass { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerReceptions { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRecYds { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerRecTD { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerYardsPerRec { get; set; }

        [DataMember]
        public List<PlayerStatRecord> AllPurposeYards { get; set; }

        [DataMember]
        public List<PlayerStatRecord> AllPurposeTD { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerTackles { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerTFL { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerPassDef { get; set; }


        [DataMember]
        public List<PlayerStatRecord> PlayerSacks { get; set; }

        [DataMember]
        public List<PlayerStatRecord> PlayerINT { get; set; }


        [DataMember]
        public List<PlayerStatRecord> MostPRYards { get; set; }
        [DataMember]
        public List<PlayerStatRecord> MostPRTD { get; set; }
        [DataMember]
        public List<PlayerStatRecord> BestPRAvg { get; set; }
        [DataMember]
        public List<PlayerStatRecord> LongestPR { get; set; }

        [DataMember]
        public List<PlayerStatRecord> MostKRYards { get; set; }
        
        [DataMember]
        public List<PlayerStatRecord> MostKRTD { get; set; }
        
        [DataMember]
        public List<PlayerStatRecord> BestKRAvg { get; set; }
        
        [DataMember]
        public List<PlayerStatRecord> LongestKR { get; set; }

        [DataMember]
        public List<PlayerStatRecord> LongestPass { get; set; }

        [DataMember]
        public List<PlayerStatRecord> LongestRush { get; set; }

        [DataMember]
        public List<PlayerStatRecord> LongestRec { get; set; }

        [DataMember]
        public List<PlayerStatRecord> LongestIntReturn { get; set; }

        [DataMember]
        public List<PlayerStatRecord> MostIntTD { get; set; }

        [DataMember]
        public List<PlayerStatRecord> MostIntReturnYards { get; set; }

        [DataMember]
        public List<PlayerStatRecord> MostFumbleRecYds { get; set; }

        [DataMember]
        public List<PlayerStatRecord> MostFumbleRecTD { get; set; }

        private List<PlayerStatRecord> Evaluate(List<PlayerStatRecord> list, ScheduledGame game, params string[] stats)
        {
            return Evaluate(list, game, null, stats);
        }

        private List<PlayerStatRecord> Evaluate(List<PlayerStatRecord> list, ScheduledGame game, Func<PlayerStats, int> func, Func<IEnumerable<PlayerStats>, IEnumerable<PlayerStats>> filter = null)
        {
            var ps = game.GamePlayerStats.Values as IEnumerable<PlayerStats>;

            if (filter != null)
            {
                ps = filter(ps);
            }

            list.AddRange(PlayerStatRecord.CreateFunctionValue(game, ps, func));
            return list.OrderAndTrim();
        }

        private List<PlayerStatRecord> Evaluate(List<PlayerStatRecord> list, ScheduledGame game, Func<IEnumerable<PlayerStats>, IEnumerable<PlayerStats>> filter, params string[] stats)
        {
            var playerStats = game.GamePlayerStats.Values.ToArray();

            if (filter != null)
            {
                playerStats = filter(playerStats).ToArray();
            }

            list.AddRange(PlayerStatRecord.CreateAggregate(game, playerStats, stats));
            return list.OrderAndTrim();
        }

        public void Prepare(Bowl bowl)
        {
            Prepare(bowl.ScheduleGame);
        }

        public void Prepare(ScheduledGame game)
        {
            if (game == null)
                return;

            if (this.Name == null && game.IsNeutralSite)
            {
                this.Name = game.GameSite;
            }

            this.EnsureInstantiatedLists();

            #region Team Records
            this.BiggestWins.Add(new BiggestWin(game));
            this.MostPoints.Add(new PointsRecord(game,true));
            this.MostPoints.Add(new PointsRecord(game,false));
            this.MostCombinedPoints.Add(new CombinedPointsRecord(game));
            this.FewestCombinedPoints.Add(new CombinedPointsRecord(game));

            try
            {
                this.MostOffensiveYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.OffensiveYards, game.AwayTeamBoxScore.OffensiveYards, true));
            }
            catch
            {
                Console.WriteLine(game.HomeTeamBoxScore == null ? "home wtf":"home ok");
                Console.WriteLine(game.AwayTeamBoxScore == null ? "away wtf" : "away ok");
                Console.WriteLine(game.Week + " " + game.GameNumber);
            }
            this.MostPassingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.PassYards, game.AwayTeamBoxScore.PassYards, true));
            this.MostRushingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.RushYards, game.AwayTeamBoxScore.RushYards, true));
            this.MostOffensiveYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.OffensiveYards, game.AwayTeamBoxScore.OffensiveYards,false));
            this.MostPassingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.PassYards, game.AwayTeamBoxScore.PassYards, false));
            this.MostRushingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.RushYards, game.AwayTeamBoxScore.RushYards, false));
            this.LeastPoints.Add(new PointsRecord(game, true));
            this.LeastPoints.Add(new PointsRecord(game, false));
            this.LeastOffensiveYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.OffensiveYards, game.AwayTeamBoxScore.OffensiveYards, true));
            this.LeastPassingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.PassYards, game.AwayTeamBoxScore.PassYards, true));
            this.LeastRushingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.RushYards, game.AwayTeamBoxScore.RushYards, true));
            this.LeastOffensiveYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.OffensiveYards, game.AwayTeamBoxScore.OffensiveYards, false));
            this.LeastPassingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.PassYards, game.AwayTeamBoxScore.PassYards, false));
            this.LeastRushingYards.Add(new YardageRecord(game, game.HomeTeamBoxScore.RushYards, game.AwayTeamBoxScore.RushYards, false));

            // reorder things
            this.BiggestWins = this.BiggestWins.Distinct(RecordComparer.BiggestWinComparer).ToList().OrderAndTrim(RecordComparer.BiggestWinComparer, g => g.PointDiff).ToList();
            this.MostPoints = this.MostPoints.Distinct(RecordComparer.PointsComparer).ToList().OrderAndTrim(RecordComparer.PointsComparer,g => g.Points).ToList();
            this.MostCombinedPoints = this.MostCombinedPoints.Distinct(RecordComparer.CombinedPointsComparer).ToList().OrderAndTrim(RecordComparer.CombinedPointsComparer, g => g.Points).ToList();
            this.FewestCombinedPoints = this.FewestCombinedPoints.Distinct(RecordComparer.CombinedPointsComparer).OrderBy( r => r.Points).ToList().Trim(RecordComparer.CombinedPointsComparer, g => g.Points).ToList();

            // Total Offense
            this.MostOffensiveYards = this.MostOffensiveYards.Distinct(RecordComparer.MostYardageComparer).ToList().OrderAndTrim(RecordComparer.MostYardageComparer,g => g.Yards).ToList();

            //Passing Yards
            this.MostPassingYards = this.MostPassingYards.Distinct(RecordComparer.MostYardageComparer).ToList().OrderAndTrim(RecordComparer.MostYardageComparer, g => g.Yards).ToList();

            //Rushing Yards
            this.MostRushingYards = this.MostRushingYards.Distinct(RecordComparer.MostYardageComparer).ToList().OrderAndTrim(RecordComparer.MostYardageComparer, g => g.Yards).ToList();

            //Fewest Points Allowed
            this.LeastPoints = this.LeastPoints.Distinct(RecordComparer.PointsComparer).OrderBy(p => p.Points).ToList().Trim(RecordComparer.PointsComparer, p => p.Points).ToList();

            //Fewest Offense Yards Allowed
            this.LeastOffensiveYards = this.LeastOffensiveYards.Distinct(RecordComparer.MostYardageComparer).OrderBy(p => p.Yards).ToList().Trim(RecordComparer.MostYardageComparer, p => p.Yards).ToList();

            //Fewest Rushing Yards
            this.LeastRushingYards = this.LeastRushingYards.Distinct(RecordComparer.MostYardageComparer).OrderBy(p => p.Yards).ToList().Trim(RecordComparer.MostYardageComparer, p => p.Yards).ToList();

            //Fewest Passing Yards
            this.LeastPassingYards = this.LeastPassingYards.Distinct(RecordComparer.MostYardageComparer).OrderBy(p => p.Yards).ToList().Trim(RecordComparer.MostYardageComparer, p => p.Yards).ToList();
            #endregion
            #region Individual offensive records
            //Most Total Yards Offense (Passing  + Rushing + Receiving)
            this.PlayerTotalOffense = Evaluate(this.PlayerTotalOffense, game, PlayerStats.PassingYards, PlayerStats.RushingYards, PlayerStats.ReceivingYards);

            //Most TD (Passing + Rushing + Recieving)
            this.PlayerOffensiveTD = Evaluate(this.PlayerOffensiveTD, game, PlayerStats.PassingTD, PlayerStats.RushingTD, PlayerStats.ReceivingTD);

            //Most Rusing Attempt
            this.PlayerRushingAtt = Evaluate(this.PlayerRushingAtt, game, PlayerStats.RushAttempts);

            //Most Rushing Yards non-QB
            this.PlayerRushingYdsNonQB = Evaluate(this.PlayerRushingYdsNonQB, game, input => input.Where(p => p.Player.Position != 0), PlayerStats.RushingYards);

            //Most Rushing Yards by QB
            this.PlayerRushingYdsQB = Evaluate(this.PlayerRushingYdsQB, game, input => input.Where(p => p.Player.Position == 0), PlayerStats.RushingYards);

            //Most Rushing Yards Per Carry (min 5 attempt)
            this.PlayerRushingYPC = Evaluate(this.PlayerRushingYPC, game, ps => PlayerStatRecord.CalculateAverage(ps, 10, PlayerStats.RushingYards, PlayerStats.RushAttempts), p => p.Where(s => s.GetIntValue(PlayerStats.RushAttempts) >= 5));

            //Most Rushing TD
            this.PlayerRushingTD = Evaluate(this.PlayerRushingTD, game, PlayerStats.RushingTD);

            //Most Pass Attempt
            this.PlayerPassAtt = Evaluate(this.PlayerPassAtt, game, PlayerStats.PassAttempts);

            //Most Completions
            this.PlayerCompletions = Evaluate(this.PlayerCompletions, game, PlayerStats.Completions);

            //Most Passing Yards
            this.PlayerPassingYards = Evaluate(this.PlayerPassingYards, game, PlayerStats.PassingYards);

            //Most TD Pass
            this.PlayerPassingTD = Evaluate(this.PlayerPassingTD, game, PlayerStats.PassingTD);

            //Highest Completion % (min 15 att)
            this.PlayerCompletionPct = Evaluate(this.PlayerCompletionPct, game, ps => PlayerStatRecord.CalculateAverage(ps, 100, PlayerStats.Completions, PlayerStats.PassAttempts), p => p.Where(s => s.GetIntValue(PlayerStats.PassAttempts) >= 15));

            //Passing Yards Per Att (min 15 att)
            this.PlayerYardsPerPass = Evaluate(this.PlayerYardsPerPass, game, ps => PlayerStatRecord.CalculateAverage(ps, 10, PlayerStats.PassingYards, PlayerStats.PassAttempts), p => p.Where(s => s.GetIntValue(PlayerStats.PassAttempts) >= 15));

            //Most Receptions
            this.PlayerReceptions = Evaluate(this.PlayerReceptions, game, PlayerStats.Receptions);

            //Most Receiving YArds
            this.PlayerRecYds = Evaluate(this.PlayerRecYds, game, PlayerStats.ReceivingYards);

            //Most Rec TD
            this.PlayerRecTD = Evaluate(this.PlayerRecTD, game, PlayerStats.ReceivingTD);

            //Highest Yards Per Catch (min 3 receptions)
            this.PlayerYardsPerRec = Evaluate(this.PlayerYardsPerRec, game, ps => PlayerStatRecord.CalculateAverage(ps, 10, PlayerStats.ReceivingYards, PlayerStats.Receptions), p => p.Where(s => s.GetIntValue(PlayerStats.Receptions) >= 3));

            //ALl Purpose Yards = Offensive Yards + KR + PR yards + DEF YDS
            this.AllPurposeYards = Evaluate(this.AllPurposeYards, game, PlayerStats.RushingYards, PlayerStats.ReceivingYards, PlayerStats.KRYds, PlayerStats.PRYds, PlayerStats.IntRetYds, PlayerStats.FumRecYds);

            //Total TD = OFF TD + KR TD + PR TD + DEF TD
            this.AllPurposeTD = Evaluate(this.AllPurposeTD, game, PlayerStats.KRTD, PlayerStats.PRTD, PlayerStats.RushingTD, PlayerStats.ReceivingTD, PlayerStats.DefTD, PlayerStats.IntReturnedForTD, PlayerStats.FumblesReturnedForTD);

            // LONG PASS
            this.LongestPass = Evaluate(this.LongestPass, game, PlayerStats.LongestPass);

            // LONG Rush
            this.LongestRush = Evaluate(this.LongestRush, game, PlayerStats.LongestRush);

            // LONG REC
            this.LongestRec = Evaluate(this.LongestRec, game, PlayerStats.LongestReception);
            #endregion
            #region Defensive records
            //Most Tackles
            this.PlayerTackles = Evaluate(this.PlayerTackles, game, PlayerStats.TotalTackles);

            //Most TFL
            this.PlayerTFL = Evaluate(this.PlayerTFL, game, PlayerStats.TackleForLoss);

            //Most Pass Defended
            this.PlayerPassDef = Evaluate(this.PlayerPassDef, game, PlayerStats.PassDeflections);

            //Most Sacks
            this.PlayerSacks = Evaluate(this.PlayerSacks, game, PlayerStats.Sacks);

            //Most INT
            this.PlayerINT = Evaluate(this.PlayerINT, game, PlayerStats.Interceptions);

            // LONG INT RET
            this.LongestIntReturn = Evaluate(this.LongestIntReturn, game, PlayerStats.LongIntRet);

            // Most INT TD
            this.MostIntTD = Evaluate(this.MostIntTD, game, p => p.Where(s => s.GetIntValue(PlayerStats.IntReturnedForTD) > 0), PlayerStats.IntReturnedForTD);

            // Most INT yds
            this.MostIntReturnYards = Evaluate(this.MostIntReturnYards, game, PlayerStats.IntRetYds);

            // most fumble recovery yards
            this.MostFumbleRecYds = Evaluate(this.MostFumbleRecYds, game, PlayerStats.FumRecYds);

            // most fumble ret TD
            this.MostFumbleRecTD = Evaluate(this.MostFumbleRecTD, game, PlayerStats.FumblesReturnedForTD);
            #endregion
            #region Return records
            //Most PR Yards
            this.MostPRYards = Evaluate(this.MostPRYards, game, PlayerStats.PRYds);

            //Highest PR Average
            this.BestPRAvg = Evaluate(this.BestPRAvg, game, ps => PlayerStatRecord.CalculateAverage(ps, 10, PlayerStats.PRYds, PlayerStats.PuntReturns));

            // LONG PR
            this.LongestPR = Evaluate(this.LongestPR, game, PlayerStats.LongestPR);

            // PR TD
            this.MostPRTD = Evaluate(this.MostPRTD, game, p => p.Where(s => s.GetIntValue(PlayerStats.PRTD) > 0), PlayerStats.PRTD);

            //Most KR Yards
            this.MostKRYards = Evaluate(this.MostKRYards, game, PlayerStats.KRYds);

            //Highest KR Average
            this.BestKRAvg = Evaluate(this.BestKRAvg, game, ps => PlayerStatRecord.CalculateAverage(ps, 10, PlayerStats.KRYds, PlayerStats.KickReturns));

            // LONG KR
            this.LongestKR = Evaluate(this.LongestKR, game, PlayerStats.LongestKR);

            // KR TD
            this.MostKRTD = Evaluate(this.MostKRTD, game, p => p.Where(s => s.GetIntValue(PlayerStats.KRTD) > 0), PlayerStats.KRTD);
            #endregion
        }

        private void EnsureInstantiatedLists()
        {
#if false
            if (this.PlayerRushingTD == null) { this.PlayerRushingTD = new List<PlayerStatRecord>(); }
            if (this.PlayerRushingYPC == null) { this.PlayerRushingYPC = new List<PlayerStatRecord>(); }
            if (this.PlayerRushingYdsNonQB == null) { this.PlayerRushingYdsNonQB = new List<PlayerStatRecord>(); }
            if (this.PlayerRushingYdsQB == null) { this.PlayerRushingYdsQB = new List<PlayerStatRecord>(); }
            if (this.FewestCombinedPoints == null) { this.FewestCombinedPoints = new List<CombinedPointsRecord>(); }
            if (this.MostCombinedPoints == null) { this.MostCombinedPoints = new List<CombinedPointsRecord>(); }
            if (this.BiggestWins == null) { this.BiggestWins = new List<BiggestWin>(); }
            if (this.MostPoints == null) { this.MostPoints = new List<PointsRecord>(); }
            if (this.MostOffensiveYards == null) { this.MostOffensiveYards = new List<YardageRecord>(); }
            if (this.MostPassingYards == null) { this.MostPassingYards = new List<YardageRecord>(); }
            if (this.MostRushingYards == null) { this.MostRushingYards = new List<YardageRecord>(); }
            if (this.LeastPoints == null) { this.LeastPoints = new List<PointsRecord>(); }
            if (this.LeastOffensiveYards == null) { this.LeastOffensiveYards = new List<YardageRecord>(); }
            if (this.LeastPassingYards == null) { this.LeastPassingYards = new List<YardageRecord>(); }
            if (this.LeastRushingYards == null) { this.LeastRushingYards = new List<YardageRecord>(); }
            if (this.PlayerTotalOffense == null) { this.PlayerTotalOffense = new List<PlayerStatRecord>(); }
            if (this.PlayerOffensiveTD == null) { this.PlayerOffensiveTD = new List<PlayerStatRecord>(); }
            if (this.PlayerRushingAtt == null) { this.PlayerRushingAtt = new List<PlayerStatRecord>(); }
#endif

            // use reflection for all lists
            var type = this.GetType();
            var props = type.GetProperties();

            foreach (var prop in props.Skip(1))
            {
                if (prop.GetValue(this, null) == null && prop.PropertyType!=typeof(string))
                {
                    prop.SetValue(this, prop.PropertyType.GetConstructor(Type.EmptyTypes).Invoke(null), null);
                }
            }
        }
    }

    [DataContract]
    public abstract class PlayerRecord : GameDescriptor
    {
        protected PlayerRecord(ScheduledGame game, Player player)
            : base(game)
        {
            this.By = player.TeamId;
            this.Against = this.By == game.HomeTeamId ? game.AwayTeamId : game.HomeTeamId;
            this.Player = player.Name;
            this.Position = player.PositionName;
            this.Number = player.Number;
            this.PlayerId = player.Id;
            this.SetScore(game);
        }

        [DataMember]
        public string Player { get; set; }

        [DataMember]
        public string Position { get; set; }

        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public int PlayerId { get; set; }
    }

    [DataContract]
    public class PlayerStatRecord : PlayerRecord
    {
        public static int CalculateAverage(PlayerStats stat, int mult,string num, params string[] den)
        {
            var denomintor = den.Sum(d => stat.GetIntValue(d));
            return denomintor == 0 ? 0 : (mult * stat.GetIntValue(num)) / denomintor;
        }

        public static PlayerStatRecord[] CreateFunctionValue(ScheduledGame game, IEnumerable<PlayerStats> stats, Func<PlayerStats, int> func)
        {
            return stats.Select(
                s =>
                    new PlayerStatRecord(game, s.Player)
                    {
                        Value = func(s)
                    })
                    .Distinct(RecordComparer.PlayerComparer)
                    .OrderByDescending(p => p.Value)
                    .Take(IndividualBowlRecords.BowlHistoryLookback)
                    .ToArray();
        }

        public static PlayerStatRecord[] CreateAggregate(ScheduledGame game, IEnumerable<PlayerStats> stats, params string[] categories)
        {
            return stats.Select(
                s =>
                    new PlayerStatRecord(game, s.Player)
                    {
                        Value = categories.Sum(statName => s.GetIntValue(statName))
                    })
                    .Distinct(RecordComparer.PlayerComparer)
                    .OrderByDescending(p => p.Value)
                    .Take(IndividualBowlRecords.BowlHistoryLookback)
                    .ToArray();
        }

        public PlayerStatRecord(ScheduledGame game, Player player)
            : base(game, player)
        {
        }

        [DataMember]
        public int Value { get; set; }
    }

    [DataContract]
    public class YardageRecord : GameDescriptor
    {
        public YardageRecord(ScheduledGame game, int homeYards, int awayYards, bool homeTeam)
            : base(game)
        {
            if (homeTeam)
            {
                this.By = game.HomeTeamId;
                this.Against = game.AwayTeamId;
                this.Yards = homeYards;
            }
            else
            {
                this.By = game.AwayTeamId;
                this.Against = game.HomeTeamId;
                this.Yards = awayYards;
            }

            this.SetScore(game);
        }

        [DataMember]
        public int Yards { get; set; }
    }
}