using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
{
    public enum Network
    {
        ABC,
        ESPN,
        ESPN2,
        SECNetwork,
        FOX,
        CBS,
        NBC,
        BTN,
        FS1,
        ACCNetwork,
        CW,
        CBSSN,

        Peacock, //streaming
        Fox1, //streaming
        ESPN_Plus, //streaming
    }

    public abstract class NetworkSchedule
    {
        public string Name { get; }

        protected List<InSeasonGame> Games { get; } = new List<InSeasonGame>();

        protected Dictionary<int, List<InSeasonGame>> Schedule { get; set; }

        protected NetworkSchedule(string name)
        {
            this.Name = name;
        }

        public abstract void SelectGames(Dictionary<int, List<InSeasonGame>> games);

        public abstract void AssignGames();
    }

    /// <summary>
    /// ESPN, ABC, ESPN2, SEC Network, ACC Network, ESPNU, ESPN+
    /// </summary>
    public class ESPNNetworks : NetworkSchedule
    {
        public static readonly ESPNNetworks Instance = new ESPNNetworks();

        public Dictionary<TimeSlot,InSeasonGame> ABC = new Dictionary<TimeSlot,InSeasonGame>();
        public Dictionary<TimeSlot, InSeasonGame> ESPN = new Dictionary<TimeSlot, InSeasonGame>();
        public Dictionary<TimeSlot, InSeasonGame> ESPN2 = new Dictionary<TimeSlot, InSeasonGame>();
        private ESPNNetworks() : base("ESPN")
        {
            //build ABC, they have 12, 3:30, 7:30 each saturday
            for (int i = 0; i <= 13; i++)
            {
                var noon = new TimeSlot(12, 0, i);
                var secGotw = new TimeSlot(3, 30, i);
                var espnPrimetime = new TimeSlot(7, 00, i);
                var primetime = new TimeSlot(7, 30, i);
                var eightPM = new TimeSlot(8, 00, i);
                var afterDark = new TimeSlot(10, 30, i);
                ABC.Add(noon, null);
                ABC.Add(secGotw, null);
                ABC.Add(primetime, null);
                ESPN.Add(noon, null);
                ESPN.Add(secGotw, null);
                ESPN.Add(espnPrimetime, null);
                ESPN.Add(afterDark, null);
                ESPN2.Add(noon, null);
                ESPN2.Add(secGotw, null);
                ESPN2.Add(eightPM, null);
            }
        }

        public void Report()
        {
            WriteReport("abc", ABC);
            WriteReport("espn", ESPN);
            WriteReport("espn2", ESPN2);
        }

        private void WriteReport(string file, Dictionary<TimeSlot, InSeasonGame> network) 
        {
            var sb = new StringBuilder();
            foreach (var kvp in network.OrderBy(k => k.Key.Week).ThenBy(k => k.Key.GTOD))
            {
                sb.AppendLine($"{kvp.Key.GTOD}: {kvp.Value?.AwayTeam} at {kvp.Value?.HomeTeam}");
            }

            File.WriteAllText($"{file}-tv-debug.log", sb.ToString());
        }

        public override void AssignGames()
        {
            AssignMWCAfterDark();
            AssignSecGamesOfTheWeek();
            AssignP5ESPN();
            Report();
        }

        /// <summary>
        /// best of the afternoon and 330 of the ESPN/ABC games
        /// </summary>
        private void AssignP5ESPN()
        {
            for (int i = 0; i <= 13; i++)
            {
                var games = this.Schedule[i]
                    .Where(g => !g.Assigned && g.HomeTeamIsP5).OrderBy(g => g.Score).ToQueue();

                if (games.TryDequeue(out var game))
                {
                    ESPN.AssignGame(i, 7, 0, game);
                }

                if (games.TryDequeue(out  game))
                {
                    ESPN2.AssignGame(i, 8, 0, game);
                }

                if (games.TryDequeue(out  game))
                {
                    ESPN.AssignGame(i, 3, 30, game);
                }

                if (games.TryDequeue(out game))
                {
                    ESPN2.AssignGame(i, 3, 30, game);
                }
            }
        }

        /// <summary>
        /// ESPN airs one MWC at 1030pm
        /// </summary>
        private void AssignMWCAfterDark()
        {
            for (int i = 0; i <= 13; i++)
            {
                var games = this.Schedule[i];
                var mwc = games.Where(g => !g.Assigned && g.ConferenceOwner == TableUtility.MWCId).OrderBy(g => g.Score).FirstOrDefault();

                if (mwc != null)
                {
                    ESPN.AssignGame(i, 10, 30, mwc);
                }
            }
        }

        /// <summary>
        /// 3:30 pm the top ranked SEC game conference game if there is one.  if LSU, it should be 7:30
        /// 7:30 pm the top ranked SEC intraconference game, if none, 2nd best SEC conference game
        /// </summary>
        private void AssignSecGamesOfTheWeek()
        {
            for (int i = 0; i <= 13; i++)
            {
                // top sec conference game
                var games = this.Schedule[i];
                var secGames = games.Where(g => !g.Assigned && !g.IsSecConferenceGame && ((g.ConferenceOwner == TableUtility.SECId && g.IsP5Game) || g.IsSecAccGame)).OrderBy(g => g.Score).ToQueue();

                // top one goes to 330 unless its LSU
                var secConferenceGames = games.Where(g => !g.Assigned && g.IsSecConferenceGame ).OrderBy(g => g.Score).ToQueue();

                if (secConferenceGames.TryDequeue(out var gotw))
                {
                    var secondarySlot = new TimeSlot(7, 30, i);
                    if (gotw.HomeTeam == TableUtility.LSUId)
                    {
                        ABC.AssignGame(i, 7, 30, gotw);
                        secondarySlot = new TimeSlot(3, 30, i);
                    }
                    else
                    {
                        ABC.AssignGame(i, 3, 30, gotw);
                    }

                    if (secGames.TryDequeue(out var primetime))
                    {
                        ABC.AssignGame(secondarySlot, primetime);
                    }
                    else if (secConferenceGames.TryDequeue(out primetime))
                    {
                        ABC.AssignGame(secondarySlot, primetime);
                    }
                }
                else
                {
                    if (secGames.TryDequeue(out var primetime))
                    {
                        ABC.AssignGame(i, 7, 30, primetime);
                    }

                    if (secGames.TryDequeue(out gotw))
                    {
                        ABC.AssignGame(i, 3, 30, gotw);
                    }
                }
            }
        }

        public override void SelectGames(Dictionary<int, List<InSeasonGame>> televisedGames)
        {
            // take all sec games
            this.Games.AddRange(televisedGames[TableUtility.SECId]);
            televisedGames[TableUtility.SECId].Clear();

            // for the acc game #4 each week belongs to the CW
            var accGames = televisedGames[TableUtility.ACCId].GroupBy(g => g.Week).ToDictionary(g => g.Key, g => g.OrderBy(game => game.Score).ToList());
            var accGamesLeftOver = new List<InSeasonGame>();
            foreach (var kvp in accGames)
            {
                if (kvp.Value.Count >= 4)
                {
                    accGamesLeftOver.Add(kvp.Value[3]);
                    kvp.Value.RemoveAt(3);
                }
                else
                {
                    accGamesLeftOver.Add(kvp.Value.Last());
                    kvp.Value.RemoveAt(kvp.Value.Count - 1);
                }
            }
            televisedGames[TableUtility.ACCId] = accGamesLeftOver;

            // remaining in the dictionary is all for ESPN
            this.Games.AddRange(accGames.Values.SelectMany(g => g));

            // for big 12 espn gets 0-3, 5, 7, 9, 11, 13, 15, 17, 19
            var big12Games = televisedGames[TableUtility.Big12Id];
            this.Games.AddRange(big12Games.Take(4));

            // remove the first 4 from big 12 games
            big12Games = big12Games.Skip(4).ToList();
            for (int i = 1; i < 16; i += 2)
            {
                this.Games.Add(big12Games[i]);
                big12Games.RemoveAt(i);
            }

            televisedGames[TableUtility.Big12Id] = big12Games;

            // espn takes the top MWC game for the 10:30pm slot
            var mwcGames = televisedGames[TableUtility.MWCId].GroupBy(g => g.Week).ToDictionary(g => g.Key, g => g.OrderBy(game => game.Score).ToList());
            foreach (var kvp in mwcGames)
            {
                this.Games.Add(kvp.Value[0]);
                kvp.Value.RemoveAt(0);
            }

            televisedGames[TableUtility.MWCId] = mwcGames.Values.SelectMany(g => g).ToList();

            Schedule = this.Games.GroupBy(g => g.Week).ToDictionary(g => g.Key, g => g.OrderBy(game => game.Score).ToList());
        }
    }

    public class TimeSlot
    {
        public static readonly TimeSlot ShamrockSeries = new TimeSlot(8, 7);// 807pm
        public static readonly TimeSlot MayhemAtMBS = new TimeSlot(7, 33); // 733pm
        public static readonly TimeSlot OysterBowl = new TimeSlot(7, 17); //717 pm
        public static readonly TimeSlot JohnnyMajorsClassic = new TimeSlot(7, 37); //737pm

        public int Day { get; }
        public int Hour { get; }
        public int Minute { get; }
        public bool AM { get; }
        public int? Week { get; }
        public TimeSlot(int hour, int minute, int? week = null, bool am = false, int day = 5)
        {
            Hour = hour;
            Minute = minute;
            AM = am;
            Day  = day;
            Week = week;
        }

        public override bool Equals(object obj)
        {
            return obj is TimeSlot other &&
                this.Hour == other.Hour &&
                this.Minute == other.Minute &&
                this.AM == other.AM &&
                this.Day == other.Day &&
                this.Week == other.Week;                
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var arr = new int[] { this.Hour, this.Minute, this.Day, this.AM ? 101 : 103, this.Week ?? 113 };
                var code = 23;

                foreach (var item in arr)
                {
                    code = code * 17 + item;
                }

                return code;
            }
        }

        public int GTOD
        {
            get
            {
                if (AM || Hour == 12)
                {
                    return (60 * Hour) + Minute;
                }

                return (60 * (12 + Hour)) + Minute;
            }
        }

        public string ToGTOD()
        {
            var hourMod = AM ? Hour : (12 + Hour);
            var result = hourMod * 60 + Minute;
            return result.ToString();
        }
    }

    public static class TelevisionScheduler
    {
        public static void FixTelevisionSchedule()
        {
            var team = TableUtility.FindTable("TEAM").lRecords.ToDictionary(mr => mr.TeamId());
            var games = TableUtility.FindTable("SCHD").lRecords
                .Select(mr => new InSeasonGame(mr, team))
                .Where(g => g.GameNeedsAssignment())
                .GroupBy(g => g.ConferenceOwner)
                .ToDictionary(g => g.Key, g => g.ToList());

            ESPNNetworks.Instance.SelectGames(games);
            ESPNNetworks.Instance.AssignGames();
        }

        public static bool GameNeedsAssignment(this InSeasonGame game)
        {
            // labor day monday does not get assigned
            if (game.Week <= 2 && game.Day == 0)
            {
                return game.Assigned = true;
            }

            // Sundays before labor day do not get assigned
            // same with thur/fri as those are hand crafted
            if (game.Week <= 1 && game.Day != 5)
            {
                return game.Assigned = true;
            }

            // rocky mountain showdown will get assigned
            if ((game.HomeTeam == 22 && game.AwayTeam == 23) || (game.HomeTeam == 23 && game.AwayTeam == 22))
            {
                return game.Assigned = true;
            }

            return !game.Assigned;
        }

        public static void AssignGame(this Dictionary<TimeSlot, InSeasonGame> schedule, int week, int hour, int minute, InSeasonGame game)
        {
            schedule.AssignGame(new TimeSlot(hour, minute, week), game);
        }

        public static void AssignGame(this Dictionary<TimeSlot, InSeasonGame> schedule, TimeSlot timeslot, InSeasonGame game)
        {
            schedule[timeslot] = game.Assign();
        }
    }

    public class InSeasonGame
    {
        public int Score { get; }

        public int ConferenceOwner { get; }

        public int Week { get; }

        public int Day { get; }

        public int GTOD { get; }
        public int AwayTeam { get; }
        public int HomeTeam { get; }
        public bool IsConferenceGame { get; }
        public MaddenRecord Record { get; }
        public bool IsSecAccGame { get; }
        public bool IsSecConferenceGame => IsConferenceGame && ConferenceOwner == TableUtility.SECId;
        public bool IsP5Game { get; }
        public bool Assigned { get;  set; }
        public bool HomeTeamIsP5 { get; }
        public InSeasonGame(MaddenRecord mr, Dictionary<int, MaddenRecord> teams)
        {
            Record = mr;
            HomeTeam = mr.GetHomeTeam();
            AwayTeam = mr.GetAwayTeam();
            var away = teams[AwayTeam];
            var home = teams[HomeTeam];
            var score = home.CoachPollRanking() + home.MediaPollRanking() + away.CoachPollRanking() + away.MediaPollRanking();
            score /= 2;
            score += ScheduleFixup.IsRivalryGame(AwayTeam, HomeTeam) ? -10 : 0;
            score += TableUtility.TeamAndConferences.TeamsInSameConference(AwayTeam, HomeTeam) ? -5 : 0;
            Score = score;
            ConferenceOwner = HomeTeam.IsIndependentND() ? TableUtility.NotreDameId : TableUtility.GameConferenceOwner(HomeTeam);
            Week = mr.GameWeek();
            Day = mr.GameDay();
            GTOD = mr.GTOD();
            IsConferenceGame = ConferenceOwner == TableUtility.GameConferenceOwner(AwayTeam);
            IsSecAccGame = (AwayTeam.IsSECTeam() && HomeTeam.IsAccTeam()) || (HomeTeam.IsSECTeam() && AwayTeam.IsAccTeam());
            IsP5Game = AwayTeam.IsP5OrND() && HomeTeam.IsP5OrND();
            HomeTeamIsP5 = HomeTeam.IsP5OrND();
            Score += IsP5Game ? -5 : 0;
        }

        public override bool Equals(object obj)
        {
            return obj is InSeasonGame other &&
                Week == other.Week &&
                AwayTeam == other.AwayTeam &&
                HomeTeam == other.HomeTeam;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public InSeasonGame Assign()
        {
            Assigned = true;
            return this;
        }
    }
}