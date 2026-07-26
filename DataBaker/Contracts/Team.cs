using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DataBaker.Contracts
{

    [JsonObject]
    public class DefensiveRankings
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Overall")]
        public int Overall { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Passing")]
        public int Passing { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Rushing")]
        public int Rushing { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Turnovers")]
        public int Turnovers { get; set; }
    }

    [JsonObject]
    public class DraftHistory
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Round1")]
        public int Round1 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Round2")]
        public int Round2 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Round3")]
        public int Round3 { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RoundLater")]
        public int RoundLater { get; set; }
    }

    [JsonObject]
    public class MediaCoverage
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Content")]
        public string Content { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "GameNumber")]
        public int GameNumber { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Headline")]
        public string Headline { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "PlayerId")]
        public int PlayerId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "PlayerName")]
        public string PlayerName { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamId")]
        public int TeamId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Week")]
        public int Week { get; set; }
    }

    [JsonObject]
    public class OffensiveRankings
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Overall")]
        public int Overall { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Passing")]
        public int Passing { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "PassingTD")]
        public int PassingTD { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Rushing")]
        public int Rushing { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RushingTD")]
        public int RushingTD { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Turnovers")]
        public int Turnovers { get; set; }
    }

    [JsonObject]
    public class Team
    {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AllTimeLoss")]
        public int AllTimeLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AllTimeTie")]
        public int AllTimeTie { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AllTimeWin")]
        public int AllTimeWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Article")]
        public object Article { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "AverageAttendance")]
        public int AverageAttendance { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BCSPrevious")]
        public int BCSPrevious { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BCSRank")]
        public int BCSRank { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BowlLoss")]
        public int BowlLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BowlTie")]
        public int BowlTie { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BowlWin")]
        public int BowlWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "BowlWinsThisYear")]
        public string BowlWinsThisYear { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachesPollFirstPlaceVotes")]
        public int CoachesPollFirstPlaceVotes { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachesPollPoints")]
        public int CoachesPollPoints { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachesPollPrevious")]
        public int CoachesPollPrevious { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "CoachesPollRank")]
        public int CoachesPollRank { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "ConferenceId")]
        public int ConferenceId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "ConferenceLoss")]
        public int ConferenceLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "ConferenceOrDivisionChampionship")]
        public string ConferenceOrDivisionChampionship { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "ConferenceTitles")]
        public int ConferenceTitles { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "ConferenceWin")]
        public int ConferenceWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DefensiveCoordinator")]
        public Coach DefensiveCoordinator { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DefensiveRankings")]
        public DefensiveRankings DefensiveRankings { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DivisionId")]
        public int DivisionId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DivisionLoss")]
        public int DivisionLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DivisionWin")]
        public int DivisionWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "DraftHistory")]
        public DraftHistory DraftHistory { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HeadCoach")]
        public Coach HeadCoach { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HomeLoss")]
        public int HomeLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HomeStreak")]
        public string HomeStreak { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HomeStreakRaw")]
        public int HomeStreakRaw { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HomeTie")]
        public int HomeTie { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "HomeWin")]
        public int HomeWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Id")]
        public int Id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "IsNationalChampion")]
        public bool IsNationalChampion { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "LastConferenceChampionshipYear")]
        public int LastConferenceChampionshipYear { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "LastNationalChampionshipYear")]
        public int LastNationalChampionshipYear { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Loss")]
        public int Loss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Mascot")]
        public string Mascot { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "MediaCoverage")]
        public MediaCoverage[] MediaCoverage { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "MediaPollFirstPlaceVotes")]
        public int MediaPollFirstPlaceVotes { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "MediaPollPoints")]
        public int MediaPollPoints { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "MediaPollPrevious")]
        public int MediaPollPrevious { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "MediaPollRank")]
        public int MediaPollRank { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "NationalTitles")]
        public int NationalTitles { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "OffPlayBookId")]
        public int OffPlayBookId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "OffensiveCoordinator")]
        public Coach OffensiveCoordinator { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "OffensiveRankings")]
        public OffensiveRankings OffensiveRankings { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "PlayoffStatus")]
        public int PlayoffStatus { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "PriorSeasonLoss")]
        public int PriorSeasonLoss { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "PriorSeasonWin")]
        public int PriorSeasonWin { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RecordAttendance")]
        public int RecordAttendance { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RecruitClassRank")]
        public int RecruitClassRank { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "RecruitClassRating")]
        public int RecruitClassRating { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "StadiumCapacity")]
        public int StadiumCapacity { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "StadiumId")]
        public int StadiumId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Streak")]
        public int Streak { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingDB")]
        public int TeamRatingDB { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingDEF")]
        public int TeamRatingDEF { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingDL")]
        public int TeamRatingDL { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingLB")]
        public int TeamRatingLB { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingOFF")]
        public int TeamRatingOFF { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingOL")]
        public int TeamRatingOL { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingOVR")]
        public int TeamRatingOVR { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingQB")]
        public int TeamRatingQB { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingRB")]
        public int TeamRatingRB { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingST")]
        public int TeamRatingST { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "TeamRatingWR")]
        public int TeamRatingWR { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Required = Required.Default, PropertyName = "Win")]
        public int Win { get; set; }

        public int Year { get; set; }

        public bool IsValidTeam { get { return this.Id < 160 || this.Id > 165; } }

        public static Team Generate(Team t, int year)
        {
            t.Year = year;
            return t;
        }

        public List<Coach> CoachingStaff
        {
            get
            {
                if (staff == null)
                {
                    staff = new List<Coach>();

                    if (this.HeadCoach != null)
                    {
                        HeadCoach.TeamId = this.Id;
                        staff.Add(HeadCoach);
                    }
                    else
                    {
                        staff.Add(null);
                    }

                    if (this.OffensiveCoordinator != null)
                    {
                        this.OffensiveCoordinator.TeamId = this.Id;
                        staff.Add(this.OffensiveCoordinator);
                    }
                    else
                    {
                        staff.Add(null);
                    }

                    if (this.DefensiveCoordinator != null)
                    {
                        this.DefensiveCoordinator.TeamId = this.Id;
                        staff.Add(DefensiveCoordinator);
                    }
                    else
                    {
                        staff.Add(null);
                    }
                }

                return staff;
            }
        }

        private List<Coach> staff;
    }
}