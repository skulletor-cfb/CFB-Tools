using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CFB27.Data.Model
{

    public class CFBSeasonGame : BaseRecord
    {
        public string AwayPlayerStatCache { get; set; }
        public string Stadium { get; set; }
        public string AwayTeam { get; set; }
        public string AwayTeamStatCache { get; set; }
        public string BowlGame { get; set; }
        public string GameGoal { get; set; }
        public string ScoringSummaries { get; set; }
        public string GameSetup { get; set; }
        public string HomePlayerStatCache { get; set; }
        public string HomeTeam { get; set; }
        public string HomeTeamStatCache { get; set; }
        public string InjuryCache { get; set; }
        public double PrecipitationIntensity { get; set; }
        public int AwayRequestId { get; set; }
        public int GameSessionId { get; set; }
        public int HomeRequestId { get; set; }
        public int InitialMomentum { get; set; }
        public string AwayTeamStatus { get; set; }
        public int TimeOfDay { get; set; }
        public int Attendance { get; set; }
        public string StripeOutType { get; set; }
        public int NumberTimesPlayed { get; set; }
        public string WeatherIconId { get; set; }
        public int GameOfTheWeekScore { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int SeasonGameNum { get; set; }
        public int Temperature { get; set; }
        public string SeasonWeekType { get; set; }
        public int HomeScoreOT { get; set; }
        public int AwayScoreOT { get; set; }
        public int AwayScoreQuarter1 { get; set; }
        public int AwayScoreQuarter2 { get; set; }
        public string GameStatus { get; set; }
        public int HomeScoreQuarter4 { get; set; }
        public int HomeScoreQuarter3 { get; set; }
        public int HomeScoreQuarter2 { get; set; }
        public int HomeScoreQuarter1 { get; set; }
        public int WindSpeed { get; set; }
        public int SeasonYear { get; set; }
        public int AwayScoreQuarter3 { get; set; }
        public int AwayScoreQuarter4 { get; set; }
        public int PrecipitationChance { get; set; }
        public string StripeOutOverrideColors { get; set; }
        public string BroadcastNetwork { get; set; }
        public string DayOfWeek { get; set; }
        public int GameDateMonth { get; set; }
        public string Weather { get; set; }
        public int QuarterLengthMins { get; set; }
        public int SeasonWeek { get; set; }
        public int GameDateDay { get; set; }
        public bool IsOvertimeGame { get; set; }
        public bool IsKickoffGame { get; set; }
        public bool IsGameOfTheWeek { get; set; }
        public bool IsChallengeGame { get; set; }
        public bool IsPractice { get; set; }
        public bool IsRematch { get; set; }
        public bool IsSimmed { get; set; }
        public bool IsWorstOfTheWeek { get; set; }
        public bool ThanksgivingFlag { get; set; }
        public bool NewYearsFlag { get; set; }
        public bool HasBeenPublished { get; set; }
        public bool ChristmasFlag { get; set; }
        public string HomeTeamStatus { get; set; }
        public string PlayMomentType { get; set; }
        public string ForceWin { get; set; }
        public string CloudCover { get; set; }
        public string Precipitation { get; set; }
        public string Wind { get; set; }

        [JsonIgnore]
        public bool IsBowlGame => !string.Equals(this.BowlGame, NoRefString);

        [JsonIgnore]
        public int BowlId => this.BowlGame.ToRowId();
    }
}