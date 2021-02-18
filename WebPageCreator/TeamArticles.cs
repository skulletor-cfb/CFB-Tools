using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace EA_DB_Editor
{
    public class TeamArticles
    {
        public static void Create()
        {
            var articles = Articles.Deserialize();
            foreach (var team in Team.Teams.Where(kvp => kvp.Key.IsValidTeam()))
            {
                if (team.Key >= 160 && team.Key <= 165)
                    continue;

                articles.AssignArticle(team.Value);
            }
        }
    }

    [Serializable, XmlRoot(ElementName = "Articles")]
    public class Articles
    {
        public static Random RAND = new Random();
        public static Articles Deserialize()
        {
            var serializer = new XmlSerializer(typeof(Articles));
            using (var reader = new StreamReader(@".\archive\html\articles.xml"))
            {
                return (Articles)serializer.Deserialize(reader);
            }
        }

        [XmlElement]
        public Article[] Article { get; set; }

        public void AssignArticle(Team team)
        {
            var eligbleArticles = this.Article.Where(a => a.Conditions.TeamSatisfiesConditions(team)).ToArray();
            if (eligbleArticles.Length > 0)
            {
                var n = RAND.Next(0, eligbleArticles.Length);
                eligbleArticles[n].WriteArticle(team);
            }

            team.Article += "<BR/><BR/>" +
                string.Format("Prediction: {0}-{1}<BR/>", team.PredictedWin.Value, team.PredictedLoss.Value) +
                string.Format("Best case scenario:  {0}-{1}<BR/>Worst case scenario:  {2}-{3}", team.PredictedWin.Value + team.SwingLoss.Value, team.PredictedLoss.Value - team.SwingLoss.Value, team.PredictedWin.Value - team.SwingWin.Value, team.PredictedLoss.Value + team.SwingWin.Value);

        }
    }

    [Serializable]
    public class Article
    {
        [XmlElement]
        public string Blurb { get; set; }

        [XmlElement]
        public Aspects Aspects { get; set; }

        [XmlElement]
        public Conditions Conditions { get; set; }

        public void WriteArticle(Team team)
        {
            List<string> aspects = new List<string>();
            aspects.Add(string.Empty);
            aspects.AddRange(this.Aspects.Aspect.Select(a => this.Aspects.GetAspectForTeam(a, team)));
            try
            {
                team.Article = string.Format(this.Blurb, aspects.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    [Serializable]
    public class Aspects
    {
        [XmlElement]
        public string[] Aspect { get; set; }

        public string GetAspectForTeam(string aspect, Team team)
        {
            try
            {
                switch (aspect.ToUpper())
                {
                    case "NICKNAME":
                        return team.Mascot;
                    case "COACH":
                        return team.HeadCoach.Name;
                    case "SCHOOL":
                        return team.Name;
                    case "CURRENTYEAR":
                        return (BowlChampion.CurrentYear + Utility.StartingYear).ToString();
                    case "CONFERENCE":
                        return team.Conference.Name;
                    case "BESTNONCONFOPPONENT":
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam() && g.Opponent.ConferenceId != team.ConferenceId).OrderBy(g => g.Opponent.CoachesPollRank).First().Opponent.Name;
                    case "BESTNONCONFOPPONENT2":
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam() && g.Opponent.ConferenceId != team.ConferenceId).OrderBy(g => g.Opponent.CoachesPollRank).Skip(1).First().Opponent.Name;
                    case "BESTCONFOPPONENT":
                        // make this distinct, because a Conf Champ Game means you can have a rematch 
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam() && g.Opponent.ConferenceId == team.ConferenceId).OrderBy(g => g.Opponent.CoachesPollRank).Select(g => g.Opponent).Distinct().First().Name;
                    case "BESTCONFOPPONENT2":
                        // make this distinct, because a Conf Champ Game means you can have a rematch 
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam() && g.Opponent.ConferenceId == team.ConferenceId).OrderBy(g => g.Opponent.CoachesPollRank).Select(g => g.Opponent).Distinct().Skip(1).First().Name;
                    case "BESTOPPONENT":
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam()).OrderBy(g => g.Opponent.CoachesPollRank).First().Opponent.Name;
                    case "WORSTOPPONENT":
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam()).OrderByDescending(g => g.Opponent.CoachesPollRank).First().Opponent.Name;
                    case "WORSTOPPONENT2":
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam()).OrderByDescending(g => g.Opponent.CoachesPollRank).Skip(1).First().Opponent.Name;
                    case "WORSTOPPONENT3":
                        return TeamSchedule.TeamSchedules[team.Id].FlattenedListOfGames.Where(g => g.OpponentId.IsValidTeam()).OrderByDescending(g => g.Opponent.CoachesPollRank).Skip(2).First().Opponent.Name;
                    case "LASTYEAR":
                        return (Conditions.CurrentYear - 1).ToString();
                    case "TWOYEARSAGO":
                        return (Conditions.CurrentYear - 2).ToString();
                    case "NEXTYEAR":
                        return (Conditions.CurrentYear + 1).ToString();
                    default:
                        return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    [Serializable]
    public class Conditions
    {
        public static int CurrentYear { get { return BowlChampion.CurrentYear + Utility.StartingYear; } }

        [XmlElement]
        public MinWin MinWin { get; set; }

        [XmlElement]
        public MaxWin MaxWin { get; set; }

        [XmlElement]
        public MaxConferenceWin MaxConferenceWin { get; set; }

        [XmlElement]
        public MinPreseasonRank MinPreseasonRank { get; set; }

        [XmlElement]
        public MaxPreseasonRank MaxPreseasonRank { get; set; }

        [XmlElement]
        public IsIndependent IsIndependent { get; set; }

        [XmlElement]
        public IsPowerConferenceTeam IsPowerConferenceTeam { get; set; }

        [XmlElement]
        public OnTheRise OnTheRise { get; set; }

        [XmlElement]
        public ImprovedLastYear ImprovedLastYear { get; set; }

        [XmlElement]
        public DownLastYear DownLastYear { get; set; }

        [XmlElement]
        public LastSeasonLossMin LastSeasonLossMin { get; set; }

        [XmlElement]
        public CoachRatingMax CoachRatingMax { get; set; }

        [XmlElement]
        public LostInPlayoffsLastYear LostInPlayoffsLastYear { get; set; }

        [XmlElement]
        public NationalChampionLastYear NationalChampionLastYear { get; set; }

        [XmlElement]
        public SwingLossMin SwingLossMin { get; set; }

        [XmlElement]
        public WorstCaseWinMax WorstCaseWinMax { get; set; }

        [XmlElement]
        public LosingRecordOver5Years LosingRecordOver5Years { get; set; }

        [XmlElement]
        public ConferenceChampionLastYear ConferenceChampionLastYear { get; set; }

                public bool TeamSatisfiesConditions(Team team)
        {
            return MinWin.Eval(team) &&
                MaxWin.Eval(team) &&
                MaxConferenceWin.Eval(team) &&
                MinPreseasonRank.Eval(team) &&
                MaxPreseasonRank.Eval(team) &&
                IsIndependent.Eval(team) &&
                IsPowerConferenceTeam.Eval(team) &&
                OnTheRise.Eval(team) &&
                ImprovedLastYear.Eval(team) &&
                DownLastYear.Eval(team) &&
                LastSeasonLossMin.Eval(team) &&
                CoachRatingMax.Eval(team) &&
                LostInPlayoffsLastYear.Eval(team) &&
                NationalChampionLastYear.Eval(team) &&
                SwingLossMin.Eval(team) &&
                WorstCaseWinMax.Eval(team) &&
                LosingRecordOver5Years.Eval(team) &&
                ConferenceChampionLastYear.Eval(team)
                ;
        }
    }
    #region Condtion classes
    public static class ConditionHelper
    {
        public static bool Eval(this Condition condition, Team team)
        {
            // because all possible conditions are AND compared, not having a condition active returns true
            // if the condition is not null, then Evaluate must return true
            return condition == null || condition.Evaluate(team);
        }
    }

    [Serializable]
    public abstract class Condition
    {
        [XmlText]
        public string Value { get; set; }
        public abstract bool Evaluate(Team team);
        public bool ToBool()
        {
            return string.Equals(this.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Serializable]
    public class MinWin : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.PredictedWin.Value >= this.Value.ToInt32();
        }
    }

    [Serializable]
    public class MaxWin : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.PredictedWin.Value <= this.Value.ToInt32();
        }
    }

    [Serializable]
    public class MaxConferenceWin : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.PredictedConferenceWin.Value <= this.Value.ToInt32();
        }
    }

    [Serializable]
    public class MinPreseasonRank : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.PredictedConferenceWin.Value <= this.Value.ToInt32();
        }
    }

    [Serializable]
    public class MaxPreseasonRank : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.PredictedConferenceWin.Value >= this.Value.ToInt32();
        }
    }

    [Serializable]
    public class IsIndependent : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.IsIndependent == this.ToBool();
        }
    }

    [Serializable]
    public class IsPowerConferenceTeam : Condition
    {
        public override bool Evaluate(Team team)
        {
            return Conference.PowerConferences.Contains(team.ConferenceId);
        }
    }

    [Serializable]
    public class OnTheRise : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.OnTheRise == this.ToBool();
        }
    }

    [Serializable]
    public class ImprovedLastYear : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.ImprovedLastYear == this.ToBool();
        }
    }

    [Serializable]
    public class DownLastYear : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.DownLastYear == this.ToBool();
        }
    }

    [Serializable]
    public class LastSeasonLossMin : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.PriorSeasonLoss >= Value.ToInt32();
        }
    }

    [Serializable]
    public class CoachRatingMax : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.HeadCoach.Rating <= Value.ToInt32();
        }
    }

    [Serializable]
    public class LostInPlayoffsLastYear : Condition
    {
        public override bool Evaluate(Team team)
        {
            return team.LostInPlayoffLastYear == this.ToBool();
        }
    }

    [Serializable]
    public class NationalChampionLastYear : Condition
    {
        public override bool Evaluate(Team team)
        {
            return this.ToBool() &&team.LastConferenceChampionshipYear.HasValue&& team.LastNationalChampionshipYear == (Conditions.CurrentYear - 1);
        }
    }

    [Serializable]
    public class ConferenceChampionLastYear : Condition
    {
        public override bool Evaluate(Team team)
        {
            return this.ToBool() &&team.LastConferenceChampionshipYear.HasValue&& team.LastConferenceChampionshipYear == (Conditions.CurrentYear - 1);
        }
    }

    [Serializable]
    public class SwingLossMin : Condition
    {
        public override bool Evaluate(Team team)
        {
            return this.Value.ToInt32() <= team.SwingLoss.Value;
        }
    }

    [Serializable]
    public class WorstCaseWinMax : Condition
    {
        public override bool Evaluate(Team team)
        {
            return (team.PredictedWin.Value - team.SwingWin.Value) <= this.Value.ToInt32();
        }
    }

    [Serializable]
    public class LosingRecordOver5Years : Condition
    {
        public override bool Evaluate(Team team)
        {
            var win = team.Last5Years.Sum(r => r.Win);
            var loss = team.Last5Years.Sum(r => r.Loss);
            return this.ToBool() && loss > win;
        }
    }
    #endregion
}