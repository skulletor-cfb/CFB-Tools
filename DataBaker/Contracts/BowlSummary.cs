using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace DataBaker.Contracts
{
    public static class PastPlayoffHistory
    {
        public static int[] Years;
        public static Dictionary<int, Dictionary<int, BowlSummary>> years = new Dictionary<int, Dictionary<int, BowlSummary>>();
        private static Dictionary<int, List<BowlSummary>> teams = new Dictionary<int, List<BowlSummary>>();

        public static List<BowlSummary> GetBowlsForTeam(int teamId)
        {
            List<BowlSummary> result = null;

            if (!teams.TryGetValue(teamId, out result))
                result = new List<BowlSummary>();

            return result;
        }

        public static void Clear()
        {
            teams.Clear();
            years.Clear();
        }

        public static bool ParseYear(XElement year)
        {
            int yr = (int)year.Attribute("id");
            var bowlTeams = year.Element("Teams").Elements().Select(t => new BowlTeam { Rank = (int)t.Attribute("rank"), Id = (int)t.Attribute("id"), Name = (string)t }).ToDictionary(t => t.Id);
            var bowls = year.Elements("Bowl").Select(n => new BowlSummary(yr, n, bowlTeams)).OrderByDescending(b => b.Id).ToArray();
            years[yr] = bowls.ToDictionary(bowl => bowl.Id);

            foreach (var bowl in bowls)
            {
                AddTeamBowl(bowl, bowl.WinningTeam);
                AddTeamBowl(bowl, bowl.LosingTeam);
            }

            return true;
        }

        static void AddTeamBowl(BowlSummary bowl, BowlTeam team)
        {
            List<BowlSummary> list;
            if (!teams.TryGetValue(team.Id, out list))
            {
                list = teams[team.Id] = new List<BowlSummary>();
            }

            list.Add(bowl);
        }
    }

    public class BowlTeam
    {
        private string name;
        public int Rank { get; set; }
        public int Id { get; set; }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                var rank = Rank > 25 ? string.Empty : "#" + Rank + " ";
                if (string.IsNullOrEmpty(value))
                {
                    var team = Helper.Seasons.Season.First().Teams[this.Id];
                    name = string.Format("{0}{1}", rank, team.Name);
                }
                else if (value[2] == '-' || value[1] == '-')
                {
                    var team = Helper.Seasons.Season.First().Teams[this.Id];
                    name = string.Format("{0}{1} ({2})", rank, team.Name, value);
                }
                else
                {
                    name = string.Format("{0}{1}", rank, value);
                }
            }
        }
    }

    public class BowlSummary : IPlayedGame
    {
        public BowlSummary(int year, XElement bowl, Dictionary<int, BowlTeam> teams)
        {
            Year = year;
            Id = (int)bowl.Attribute("id");
            Winner = (int)bowl.Attribute("winner");
            Loser = (int)bowl.Attribute("loser");
            Score = (string)bowl.Attribute("score");
            WinningTeam = teams[Winner];
            LosingTeam = teams[Loser];

            var split = Score.Split('-');
            WinningScore = split[0];
            LosingScore = split[1];

            switch (Id)
            {
                case 12:
                    Name = "Peach Bowl";
                    break;
                case 26:
                    Name = "Fiesta Bowl";
                    break;
                case 25:
                    Name = "Rose Bowl";
                    break;
                case 27:
                    Name = "Sugar Bowl";
                    break;
                case 28:
                    Name = "Orange Bowl";
                    break;
                case 17:
                    Name = "Cotton Bowl";
                    break;
                case 39:
                    Name = "National Championship";
                    break;
                case 0:
                    Name = "GoDaddy.com Bowl";
                    break;
                case 1:
                    Name = "Las Vegas Bowl";
                    break;
                case 2:
                    Name = "Armed Forces Bowl";
                    break;
                case 3:
                    Name = "San Francisco Bowl";
                    break;
                case 4:
                    Name = "Detroit Bowl";
                    break;
                case 5:
                    Name = "Texas Bowl";
                    break;
                case 6:
                    Name = "Famous Idaho Potato Bowl";
                    break;
                case 7:
                    Name = "Cactus Bowl";
                    break;
                case 8:
                    Name = "Tangerine Bowl";
                    break;
                case 9:
                    Name = "Music City Bowl";
                    break;
                case 10:
                    Name = "Liberty Bowl";
                    break;
                case 11:
                    Name = "Sun Bowl";
                    break;
                case 13:
                    Name = "Holiday Bowl";
                    break;
                case 14:
                    Name = "Alamo Bowl";
                    break;
                case 15:
                    Name = "Poinsettia Bowl";
                    break;
                case 16:
                    Name = "Independence Bowl";
                    break;
                case 18:
                    Name = "Outback Bowl";
                    break;
                case 19:
                    Name = "Gator Bowl";
                    break;
                case 20:
                    Name = "Citrus Bowl";
                    break;
                case 21:
                    Name = "Birmingham Bowl";
                    break;
                case 22:
                    Name = "New Mexico Bowl";
                    break;
                case 23:
                    Name = "St. Petersburg Bowl";
                    break;
                case 24:
                    Name = "Military Bowl";
                    break;
                case 29:
                    Name = "New Orleans Bowl";
                    break;
                case 30:
                    Name = "Hawai'i Bowl";
                    break;
                case 31:
                    Name = "Belk Bowl";
                    break;
                case 32:
                    Name = "Heart of Dallas Bowl";
                    break;
                default:
                    throw new InvalidOperationException("Unknown bowl");
            }
        }

        public string WinningScore { get; set; }
        public string LosingScore { get; set; }
        public int Year { get; set; }
        public int Id { get; set; }
        public int Winner { get; set; }
        public int Loser { get; set; }
        public string Score { get; set; }
        public BowlTeam WinningTeam { get; set; }
        public BowlTeam LosingTeam { get; set; }
        public string Name { get; set; }

        public int Week
        {
            get
            {
                return 0;
            }

            set
            {
            }
        }

        public BowlTeam this[int index]
        {
            get
            {
                if (index == Winner)
                    return WinningTeam;
                else return LosingTeam;
            }
        }

        public bool IsWinningTeam(int teamId)
        {
            return Winner == teamId;
        }

        public BowlTeam GetOpponent(int teamId)
        {
            if (teamId == Winner)
                return LosingTeam;
            return WinningTeam;
        }

        public int GetOpponentId(int teamId)
        {
            if (teamId == Winner)
                return Loser;
            return Winner;
        }

        public string GetTeamScore(int teamId)
        {
            string a = null, b = null;

            if (IsWinningTeam(teamId))
            {
                a = WinningScore;
                b = LosingScore;
            }
            else
            {
                a = LosingScore;
                b = WinningScore;
            }

            return string.Format("{0}-{1}", a, b);
        }
    }
}
