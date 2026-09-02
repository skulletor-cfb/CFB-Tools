using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DataBaker.Contracts
{
    [JsonObject]
    public class Seasons
    {
        [DataMember(Name = "Season")]
        public List<Season> Season { get; set; }
    }

    [JsonObject]
    public class Season
    {
        public const string scheduleKey = "schedule";
        public const string AwardsKey = "awards";
        public const string AllAmericanKey = "aa";
        public const string BowlChampKey = "bc";
        public const string ConfChampKey = "cc";
        public const string CoachKey = "coach";
        public const string TeamKey = "team";

        private HashSet<string> keysFilled;

        public static HashSet<int> ClassicGames = new HashSet<int>()
    {
        24721827, /*Allstate Crossbar Classic*/
        24907791, /*Johnny Majors Classic*/
        247568807, /*Shamrock Series*/
        473234717, /*Oyster Bowl*/
        24732831, /*Erik Simpson CFB Classic*/
        247431733, /*Mayhem at MBS*/
        24861407, /*Eddie Robinson Classic*/
            /*Atlanta Gridiron Classic */ 263263,
            262262, /*Kansas City Classic*/
            267762, /*Union Jack Classic*/
            270270270, /*CFB Brasil*/
            165165165, /*Music City Kickoff*/
    };

        public static HashSet<int> kickoffGames = new HashSet<int>(
            new[] {
            /* arrowhead pigskin classic */ 262,
            /* aer lingus*/ 261,
            /* patriot bowl*/ 263,
            /* Kickoff in the Capital */249*249,
            /* Cowboys Showdown */ 278,
            /* The Kickoff Classic */ 277 * 277,
            /* Cowboys Kickoff */ 273,
            /* Chick Fil A Kickoff */ 271,
            /* Texas Kickoff */ 272,
            /* Windy City Classic */ 276,
            /* Cactus Kickoff Classic */ 150 * 150,
            /* Atlantic Kickoff */ 275 * 277 * 186,
            /*Sunshine State Kickoff*/ 147 * 169 * 153 * 168,
            /* Belk College Kickoff */ 186 * 186,
            /* Pigskin Classic */ 71041024,
            /* Rocky Mountain Showdown */ 184 ,
            /* Orlando Kickoff */ 147 * 147, 
            /*Camping World Kickoff*/ 162*4343976,
            /*Louisiana kickoff*/ 77841,
            /*Vegas Kickoff Classic*/ 274*274,
            /*Mile High Classic */ 250,
            });
        public Season()
        {
        }

        [JsonProperty]
        public string Directory { get; set; }

        [JsonProperty]
        public int Year { get; set; }

        [JsonProperty]
        public Dictionary<int, List<PlayedGame>> Schedule { get; set; }
        [JsonProperty]
        public Dictionary<int, List<Award>> Awards { get; set; }
        [JsonProperty]
        public Dictionary<int, List<AllAmerican>> AllAmericans { get; set; }
        [JsonProperty]
        public Dictionary<int, List<BowlChamp>> BowlChampions { get; set; }
        [JsonProperty]
        public Dictionary<int, List<ConferenceChamp>> ConferenceChampions { get; set; }
        [JsonProperty]
        public Dictionary<int, List<PlayedGame>> BowlResults { get; set; }

        [JsonProperty]
        public List<PlayedGame> PlayoffGames { get; set; }

        [JsonProperty]
        public List<PlayedGame> NY6Games { get; set; }

        [JsonProperty]
        public List<PlayedGame> KickOffGames { get; set; }

        [JsonProperty]
        public Dictionary<int, Team> Teams { get; set; }

        [JsonProperty]
        public Dictionary<CoachKey, Coach> Coaches { get; set; }

        [JsonIgnore]
        public string SeasonPath { get { return Directory.Substring(10); } }

        [JsonProperty]
        public HashSet<string> KeysFilled
        {
            get
            {
                if (keysFilled == null)
                    keysFilled = new HashSet<string>();

                return keysFilled;
            }

            set
            {
                if (value != null)
                    keysFilled = value;
            }
        }

        [JsonIgnore]
        public bool Loaded { get; set; }

        private static List<PlayedGame> GetPlayoffGamesInOrder(PlayedGame[] games, int[] gameOrder)
        {
            var list = new List<PlayedGame>();

            foreach (var gameId in gameOrder)
            {
                for (int i = 0; i < games.Length; i++)
                {
                    if (games[i].BowlId == gameId)
                    {
                        list.Add(games[i]);
                        break;
                    }
                }
            }

            return list;
        }

        public void Parse(string key, string data)
        {
            if (KeysFilled == null)
                KeysFilled = new HashSet<string>();

            if (KeysFilled.Contains(key))
                return;

            var csv = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

            switch (key)
            {
                case scheduleKey:
                    int[] gameOrder = null;
                    var games = csv.Select(l => PlayedGame.Generate(l.CsvSplit(), this.Year)).Where(pg => pg != null).ToArray();
                    var bowlsAndKickoff = games.Where(g => g.WonGame && (g.Week >= 16 || (g.BowlId.HasValue && ClassicGames.Contains(g.BowlId.Value)) || (g.Week <= 2 && g.BowlId.HasValue && kickoffGames.Contains(g.BowlId.Value)))).ToArray();
                    this.BowlResults = bowlsAndKickoff.GroupBy(g => g.BowlId.Value).ToDictionary(g => g.Key, g => g.ToList());
                    this.Schedule = games.GroupBy(pg => pg.TeamId).ToDictionary(g => g.Key, g => g.ToList());
                    this.PlayoffGames = GetPlayoffGamesInOrder(games.Where(g => g.WonGame && g.IsPlayoffBowl(out gameOrder)).ToArray(), gameOrder);
                    this.KickOffGames = games.Where(g => g.WonGame && g.IsKickoff).OrderBy(g => g.BowlId.Value).ToList();
                    this.NY6Games = games.Where(g => g.WonGame && g.IsNY6Bowl()).OrderByDescending(g => g.BowlId.Value).ToList();
                    break;
                case AwardsKey:
                    this.Awards = csv.Select(l => Award.Generate(l.CsvSplit(), this.Year)).Where(aw => !string.IsNullOrEmpty(aw.AwardName)).GroupBy(pg => pg.TeamId).ToDictionary(g => g.Key, g => g.ToList());
                    break;
                case AllAmericanKey:
                    this.AllAmericans = csv.Select(l => AllAmerican.Generate(l.CsvSplit(), this.Year)).Where(aa => aa.ConfId == AllAmerican.AllAmericanTeamConfId).GroupBy(pg => pg.TeamId).ToDictionary(g => g.Key, g => g.ToList());
                    break;
                case BowlChampKey:
                    this.BowlChampions = csv.Select(l => BowlChamp.Generate(l.CsvSplit(), this.Year)).OrderByDescending(bc => bc.DynastyYear).GroupBy(pg => pg.TeamId).ToDictionary(g => g.Key, g => g.ToList());
                    break;
                case ConfChampKey:
                    this.ConferenceChampions = csv.Select(l => ConferenceChamp.Generate(l.CsvSplit(), this.Year)).OrderByDescending(bc => bc.DynastyYear).GroupBy(pg => pg.TeamId).ToDictionary(g => g.Key, g => g.ToList());
                    break;
                case CoachKey:
                case TeamKey:
                    this.Teams = data.FromJson<List<Team>>().ToDictionary(t => t.Id, t => Team.Generate(t, this.Year));
                    this.Coaches = new Dictionary<CoachKey, Coach>();

                    foreach (var coach in this.Teams.Values.Where(t => t.IsValidTeam).SelectMany(t => t.CoachingStaff).Where(c => c != null))
                    {
                        if (string.IsNullOrWhiteSpace(coach.Name)) continue;
                        var coachKey = new CoachKey(coach.Id, coach.Name);
                        this.Coaches.Add(coachKey, coach);
                    }

                    KeysFilled.Add(key == CoachKey ? TeamKey : CoachKey);
                    break;
                default:
                    return;
            }

            KeysFilled.Add(key);
        }
    }
}
