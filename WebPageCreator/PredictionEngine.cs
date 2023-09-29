using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    public class PredictedConferenceChamp
    {
        public string ConferenceName { get; set; }
        public int ConferenceId { get; set; }
        public string TeamName { get; set; }
        public int TeamId { get; set; }
    }

    [DataContract]
    public class ConferencePrediction
    {
        [DataMember]
        public int ConferenceId { get; set; }

        [DataMember]
        public string ConferenceName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DivisionPrediction DivisionA { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DivisionPrediction DivisionB { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PredictedTeamFinish[] Teams { get; set; }
    }

    [DataContract]
    public class DivisionPrediction
    {
        [DataMember]
        public int DivisionId { get; set; }

        [DataMember]
        public string DivisionName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PredictedTeamFinish[] Teams { get; set; }
    }

    [DataContract]
    public class PredictedTeamFinish
    {
        [DataMember]
        public int TeamId { get; set; }

        [DataMember]
        public string Team { get; set; }

        [DataMember]
        public int Win { get; set; }

        [DataMember]
        public int Loss { get; set; }

        [DataMember]
        public int ConferenceWin { get; set; }

        [DataMember]
        public int ConferenceLoss { get; set; }

        public PredictedTeamFinish()
        {
        }

        public PredictedTeamFinish(Team team)
        {
            this.TeamId = team.Id;
            this.Team = team.Name;
            this.Win = team.PredictedWin.Value;
            this.Loss = team.PredictedLoss.Value;
            this.ConferenceWin = team.PredictedConferenceWin.Value;
            this.ConferenceLoss = team.PredictedConferenceLoss.Value;
        }
    }

    public class PredictionEngine
    {
        public static Dictionary<int, Team> ConferenceChampionPrediction = new Dictionary<int, Team>();
        public static Dictionary<int, ConferencePrediction> ConferenceStandings = new Dictionary<int, ConferencePrediction>();

        public static DivisionPrediction CreateDivisionPrediction(Team predictedWinner, ConferencePrediction conference, Team[] confTeams)
        {
            var order = confTeams.Where(t => t.Id != predictedWinner.Id && t.DivisionId == predictedWinner.DivisionId).Select(t => new PredictedTeamFinish(t)).ToList();
            order.Insert(0, new PredictedTeamFinish(predictedWinner));
            return new DivisionPrediction
            {
                DivisionId = predictedWinner.DivisionId,
                DivisionName = Conference.Conferences[conference.ConferenceId].Divisions.Where(d => d.Id == predictedWinner.DivisionId).First().Name,
                Teams = order.ToArray()
            };
        }

        private static Dictionary<int, (int start, int end)[]> NoDivisionConference = new Dictionary<int, (int start, int end)[]>()
        {
            {1, new[]{(2472, int.MaxValue) } }, // big 10
            {2, new[]{(2470, 2475), (2478, 2479), (2486, int.MaxValue) } }, // big 12
            {13, new[]{ (2462, 2467) } }, // Sun Belt
            {0, new []{(2477, 2478 )} }, // acc
            {3, new []{(2478, int.MaxValue )} }, // american
            {9, new []{(2478, int.MaxValue )} }, // mountain west
            {10, new []{(2483, int.MaxValue )} }, // pac-12
        };

        public static bool ConferenceHasNoDivisions(int confId)
        {
            if (NoDivisionConference.TryGetValue(confId, out var years))
            {
                foreach (var year in years)
                {
                    if (Form1.CalendarYear >= year.start && Form1.CalendarYear <= year.end)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void Create(MaddenDatabase db)
        {
            //we need to group teams by conference
            var conferenceGrouping = Team.Teams.Values.Where(t => t.ConferenceId!=17 && !t.Id.TeamNoLongerFBS()).GroupBy(team => team.ConferenceId).ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.PredictedConferenceWin.Value).ThenByDescending(t => t.PredictedDivisionWin.Value).ToArray());
            foreach (var key in conferenceGrouping.Keys.Where(k => k < 17))
            {
                var conf = conferenceGrouping[key];
                var conferencePrediction = new ConferencePrediction { ConferenceId = key, ConferenceName = Conference.Conferences[key].Name };
                ConferenceStandings.Add(key, conferencePrediction);

                // do we have a conference with less than 12 teams have a ccg?
                // either the Sun Belt or CUSA will have a CCG with 11 team conference
                if (conf.First().ConferenceId == 13 && conf.Count() == 11 ||
                    ConferenceHasNoDivisions(conf.First().ConferenceId)
                    )
                {
                    var top2 = FindTopTwo(conf);
                    var home = top2[0];
                    var away = top2[1];

                    // now we have to create a fake CCG
                    ScheduledGame sg = new ScheduledGame
                    {
                        HomeTeamId = home.Id,
                        AwayTeamId = away.Id,
                        Week = 15, // week 15 makes sure we don't collide with the actual schedule
                        GameNumber = key,
                        GameSite = Conference.Conferences[key].Name + " Championship",
                        IsNeutralSite = true
                    };

                    ScheduledGame.Schedule.Add(sg.Key, sg);
                    CreatePostSeasonGame(home, away, sg.Week, sg.GameNumber, sg);
                    CreatePostSeasonGame(away, home, sg.Week, sg.GameNumber, sg);
                    var ccgPrediction = PredictedGame.Predict(sg.Week, sg.GameNumber);
                    ConferenceChampionPrediction.Add(key, ccgPrediction.Winner);

                    // Shoud already be sorted properly
                    var order = conf.Where(t => t.Id != home.Id && t.Id != away.Id).Select(t => new PredictedTeamFinish(t)).ToList();
                    order.Insert(0, new PredictedTeamFinish(ccgPrediction.Winner));
                    order.Insert(1, new PredictedTeamFinish(ccgPrediction.Loser));
                    conferencePrediction.Teams = order.ToArray();
                }
                // this means we don't have divisions
                else if (conf.First().DivisionId == 30)
                {
                    var confChamp = FindDivisionOrConferenceWinner(conf);
                    ConferenceChampionPrediction.Add(key, confChamp);

                    // Shoud already be sorted properly
                    var order = conf.Where(t => t.Id != confChamp.Id).Select(t => new PredictedTeamFinish(t)).ToList();
                    order.Insert(0, new PredictedTeamFinish(confChamp));
                    conferencePrediction.Teams = order.ToArray();
                }
                else
                {
                    // we have divisions, so division record then head to head.  
                    var divisionGroups = conf.GroupBy(t => t.DivisionId).ToDictionary(g => g.Key, g => g.OrderByDescending(t => t.PredictedWin.Value).ThenByDescending(t => t.PredictedDivisionWin.Value).ToArray());
                    var div1Winner = FindDivisionOrConferenceWinner(divisionGroups.Values.First());
                    var div2Winner = FindDivisionOrConferenceWinner(divisionGroups.Values.Last());

                    // now we have to create a fake CCG
                    ScheduledGame sg = new ScheduledGame
                    {
                        HomeTeamId = div1Winner.Id,
                        AwayTeamId = div2Winner.Id,
                        Week = 15, // week 15 makes sure we don't collide with the actual schedule
                        GameNumber = key,
                        GameSite = Conference.Conferences[key].Name + " Championship",
                        IsNeutralSite = true
                    };

                    ScheduledGame.Schedule.Add(sg.Key, sg);
                    CreatePostSeasonGame(div1Winner, div2Winner, sg.Week, sg.GameNumber, sg);
                    CreatePostSeasonGame(div2Winner, div1Winner, sg.Week, sg.GameNumber, sg);
                    ConferenceChampionPrediction.Add(key, PredictedGame.Predict(sg.Week, sg.GameNumber).Winner);

                    // now that we have a CCG done, now set predictions
                    conferencePrediction.DivisionA = CreateDivisionPrediction(div1Winner, conferencePrediction, conf);
                    conferencePrediction.DivisionB = CreateDivisionPrediction(div2Winner, conferencePrediction, conf);
                }
            }

            ConferenceChampionPrediction.Where(kvp=>kvp.Key < 17).Select( kvp =>  new PredictedConferenceChamp
            {
                ConferenceId = kvp.Key,
                ConferenceName = Conference.Conferences[kvp.Key].Name,
                TeamId = kvp.Value.Id,
                TeamName = kvp.Value.Name
            }).OrderBy(pcc => pcc.ConferenceId).ToArray().ToJsonFile(@".\Archive\Reports\PredictedCC");

            CreateRanking();

            CreateStandings();
        }

        public static void CreateRanking()
        {
           // Team.Teams.Values.OrderByDescending(t => t.PredictedWin).ThenByDescending();
        }

        public static void CreateStandings()
        {
            PredictionEngine.ConferenceStandings.Values.ToJsonFile(@".\Archive\Reports\PredictedStandings");
        }

        public static void CreatePostSeasonGame(Team a, Team b,int week,int game, ScheduledGame sg)
        {
            Game g = new Game
            {
                OpponentId = b.Id,
                Week=week,
                GameNumber = game ,
                IsHomeGame  = true ,
                TeamId = a.Id
            };

            var list = new List<Game>();
            list.Add(g);
            TeamSchedule.TeamSchedules[a.Id].Add(week, list);
        }

        /// <summary>
        /// when having a ccg with less than 10 teams, find the top 2
        /// </summary>
        /// <param name="teams"></param>
        /// <returns></returns>
        public static Team[] FindTopTwo(Team[] teams)
        {
            return teams.Take(2).ToArray();
        }

        // Finds the winner of a division or a conference
        public static Team FindDivisionOrConferenceWinner(Team[] teams)
        {
            var first = teams.First();
            
            // find any teams with the same # of conference wins
            var others = teams.Skip(1).Where(t => t.PredictedConferenceWin.Value == first.PredictedConferenceWin.Value).ToArray();

            // no one else had the same number of wins, so it's easy first place is the champ
            if (others.Length == 0)
                return first;

            List<Team> tieBreakerTeams = new List<Team>(others);
            tieBreakerTeams.Add(first);
            
            // no divisions, so we look at head to head
            if( first.DivisionId == 30)
            {
                return DetermineHeadToHeadWinner(tieBreakerTeams);
            }
            else
            {
                // first see how division wins stack up
                var sortByDivisionWin = tieBreakerTeams.OrderByDescending(t => t.PredictedDivisionWin.Value).ToArray();
                var allTeamsWithTopWin = sortByDivisionWin.Where(t => t.PredictedDivisionWin.Value == sortByDivisionWin.First().PredictedDivisionWin.Value).ToList();
                return DetermineHeadToHeadWinner(allTeamsWithTopWin);
            }
        }

        public static Team DetermineHeadToHeadWinner(List<Team> teams)
        {
            // easy head to head winner, wins the conference/division
            if (teams.Count == 2)
            {
                var schedule = TeamSchedule.TeamSchedules[teams[0].Id];
                var game = schedule.FlattenedListOfGames.Where(g => g.OpponentId == teams[1].Id).SingleOrDefault();

                // certain divisions/conference sizes means everyone doesnt play each other, just pick the team with the most overall wins
                if (game == null)
                {
                    if (teams[0].PredictedWin.Value > teams[1].PredictedWin.Value)
                    {
                        return teams[0];
                    }
                    else
                    {
                        return teams[1];
                    }
                }
                else
                {
                    // pick the head to head winner
                    return PredictedGame.FindGame(game).Winner;
                }
            }
            else
            {
                // we just pick the team with the most wins
                return teams.OrderByDescending(t => t.PredictedWin.Value).First();
            }
        }

        public static int Rand(int inclusiveLowerBound, int exclusiveUpperBound)
        {
            var bytes = Guid.NewGuid().ToByteArray().Take(2).ToArray();
            var rand = bytes[0] << 8 | bytes[1];
            var diff = exclusiveUpperBound - inclusiveLowerBound;
            return inclusiveLowerBound + (rand % diff);
        }
    }

    public class PredictedGame
    {
        public static Dictionary<string, PredictedGame> Predictions = new Dictionary<string, PredictedGame>();

        public static PredictedGame FindGame(Game game)
        {
            return Predictions[game.ScheduledGame.Key];
        }

        public static PredictedGame Predict(int week, int game)
        {
            var key = week + "-" + game;
            PredictedGame gamePrediction = null;

            if (!Predictions.TryGetValue(key, out gamePrediction))
            {
                gamePrediction = new PredictedGame(ScheduledGame.Schedule[key]);
                Predictions.Add(key, gamePrediction);
            }

            return gamePrediction;
        }

        public PredictedGame(ScheduledGame game)
        {
            this.Game = game;
            this.CalculateWinner();
        }

        public void SetWinner(Team winner, Team loser)
        {
            if (Math.Abs(this.Spread) <= 66)
            {
                winner.SwingWin += 1;
                loser.SwingLoss += 1;
            }

            // set predicted won/loss
            winner.PredictedWin += 1;
            loser.PredictedLoss += 1;

            // set predicted conference won/loss
            if (winner.ConferenceId == loser.ConferenceId)
            {
                winner.PredictedConferenceWin += 1;
                loser.PredictedConferenceLoss += 1;

                if (winner.DivisionId == loser.DivisionId)
                {
                    winner.PredictedDivisionWin += 1;
                    loser.PredictedDivisionLoss += 1;
                }
            }
        }

        public static Team SetupTeam(Team team)
        {
            if (!team.PredictedWin.HasValue)
            {
                team.PredictedConferenceLoss = 0;
                team.PredictedConferenceWin = 0;
                team.PredictedDivisionLoss = 0;
                team.PredictedDivisionWin = 0;
                team.PredictedLoss = 0;
                team.PredictedWin = 0;
                team.SwingWin = 0;
                team.SwingLoss = 0;
            }

            return team;
        }

        public void CalculateWinner()
        {
            var home = SetupTeam(this.Game.HomeTeam);
            var away = SetupTeam(this.Game.AwayTeam);
            var homePts = CalculateBasePoints(home, away);
            var awayPts = CalculateBasePoints(away, home);
            if (homePts > awayPts)
            {
                this.Spread = (int)(10 * (homePts - awayPts)) / 2;
                this.Winner = home;
                this.Loser = away;
                this.SetWinner(home, away);
            }
            else
            {
                this.Spread = (int)(10 * (awayPts - homePts)) / 2;
                this.Winner = away;
                this.Loser = home;
                this.SetWinner(away, home);
            }

            var singleDigit = this.Spread % 10;
            // normalize the spread
            if (singleDigit >= 3 && singleDigit <= 7)
            {
                this.Spread = this.Spread - singleDigit + 5;
            }
            else if (singleDigit < 3)
            {
                this.Spread -= singleDigit;
            }
            else
            {
                this.Spread = this.Spread - singleDigit + 10;
            }
        }

        public double CalculateBasePoints(Team team, Team opp)
        {
            // Default equation values
            double skill = 0.6;
            double lines = .35;
            double specialteams = .05;

            // Playbook Boost: This provides a boost to teams using Army, Navy or GT playbooks
            // as the in game sim engine gives teams using these books a boost.
            if (team.OffPlayBookId == 7 || team.OffPlayBookId == 30 || team.OffPlayBookId == 59)
            { 
                skill = .65; 
                lines = .4; 
                specialteams = .05; 
            }

            // QB Boost: Give a slight boost to teams with a QB rating of 99
            if (team.TeamRatingQB == 99)
            {
                skill = skill + .05;
            }

            if (team.TeamRatingOFF >= 98 )
            {
                skill = skill + .1;
                lines = lines + .1;
            }

            return (CalculateOffensiveSkillRating(team) - CalculateDefensiveBack7(opp)) * skill
                + (((double)team.TeamRatingOL - opp.TeamRatingDL)) * lines 
                + (((double)team.TeamRatingST - opp.TeamRatingST)) * specialteams;
        }

        public double CalculateOffensiveSkillRating(Team team)
        {
            return ((double)team.TeamRatingQB + team.TeamRatingWR + team.TeamRatingRB) / 3;
        }

        public double CalculateDefensiveBack7(Team team)
        {
            return ((double)team.TeamRatingLB + team.TeamRatingDB) / 2;
        }

        public ScheduledGame Game { get; set; }
        public Team Winner { get; set; }

        public Team Loser { get; set; }

        public int Spread { get; set; }
    }
}
