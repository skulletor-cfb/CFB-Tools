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

        public static readonly Dictionary<int, int> TeamIdToOldIdMap = new Dictionary<int, int>
        {
            { 0, 1 },     // Air Force
            { 1, 2 },     // Akron
            { 2, 3 },     // Alabama
            { 3, 901 },   // App St.
            { 4, 4 },     // Arizona
            { 5, 5 },     // Arizona State
            { 6, 6 },     // Arkansas
            { 7, 7 },     // Arkansas State
            { 8, 8 },     // Army
            { 9, 9 },     // Auburn
            { 10, 10 },   // Ball State
            { 11, 11 },   // Baylor
            { 12, 12 },   // Boise State
            { 13, 13 },   // Boston College
            { 14, 14 },   // Bowling Green
            { 15, 15 },   // Buffalo
            { 16, 16 },   // BYU
            { 17, 17 },   // California
            { 18, 19 },   // C. Michigan
            { 19, 904 },  // Charlotte
            { 20, 20 },   // Cincinnati
            { 21, 21 },   // Clemson
            { 22, 903 },  // C. Carolina
            { 23, 22 },   // Colorado
            { 24, 23 },   // Colorado State
            { 25, 100 },  // UConn
            { 26, 906 },  // Delaware
            { 27, 24 },   // Duke
            { 28, 26 },   // E. Michigan
            { 29, 25 },   // East Carolina
            { 30, 160 },    // FCS East
            { 31, 163 },    // FCS Midwest
            { 32, 162 },    // FCS Northwest
            { 33, 164 },    // FCS Southeast
            { 34, 161 },    // FCS West
            { 35, 230 },  // FIU
            { 36, 27 },   // Florida
            { 37, 229 },  // FLA Atlantic
            { 38, 28 },   // Florida State
            { 39, 29 },   // Fresno State
            { 40, 30 },   // Georgia
            { 41, 902 },  // Ga Southern
            { 42, 233 },  // Georgia State
            { 43, 31 },   // Georgia Tech
            { 44, 32 },   // Hawai'i
            { 45, 33 },   // Houston
            { 46, 35 },   // Illinois
            { 47, 36 },   // Indiana
            { 48, 37 },   // Iowa
            { 49, 38 },   // Iowa State
            { 50, 907 },  // Jax State
            { 51, 905 },  // James Madison
            { 52, 39 },   // Kansas
            { 53, 40 },   // Kansas State
            { 54, 908 },  // Kennesaw St.
            { 55, 41 },   // Kent State
            { 56, 42 },   // Kentucky
            { 57, 909 },  // Liberty
            { 58, 86 },   // Louisiana
            { 59, 43 },   // Louisiana Tech
            { 60, 44 },   // Louisville
            { 61, 45 },   // LSU
            { 62, 46 },   // Marshall
            { 63, 47 },   // Maryland
            { 64, 48 },   // Memphis
            { 65, 49 },   // Miami
            { 66, 50 },   // Miami (OH)
            { 67, 51 },   // Michigan
            { 68, 52 },   // Michigan State
            { 69, 53 },   // Middle Tenn
            { 70, 54 },   // Minnesota
            { 71, 55 },   // Mississippi St
            { 72, 56 },   // Missouri
            { 73, 910 },  // Missouri State
            { 74, 57 },   // Navy
            { 75, 63 },   // NC State
            { 76, 58 },   // Nebraska
            { 77, 59 },   // Nevada
            { 78, 60 },   // New Mexico
            { 79, 61 },   // New Mexico St.
            { 80, 62 },   // North Carolina
            { 81, 911 },  // NDSU
            { 82, 64 },   // North Texas
            { 83, 66 },   // NIU
            { 84, 67 },   // Northwestern
            { 85, 68 },   // Notre Dame
            { 86, 69 },   // Ohio
            { 87, 70 },   // Ohio State
            { 88, 71 },   // Oklahoma
            { 89, 72 },   // Oklahoma State
            { 90, 234 },  // Old Dominion
            { 91, 73 },   // Ole Miss
            { 92, 74 },   // Oregon
            { 93, 75 },   // Oregon State
            { 94, 76 },   // Penn State
            { 95, 77 },   // Pittsburgh
            { 96, 78 },   // Purdue
            { 97, 79 },   // Rice
            { 98, 80 },   // Rutgers
            { 99, 912 },  // Sac State
            { 100, 913 }, // Sam Houston
            { 101, 81 },  // San Diego St.
            { 102, 82 },  // San Jose State
            { 103, 83 },  // SMU
            { 104, 235 }, // South Alabama
            { 105, 84 },  // South Carolina
            { 106, 85 },  // Southern Miss
            { 107, 87 },  // Stanford
            { 108, 88 },  // Syracuse
            { 109, 89 },  // TCU
            { 110, 90 },  // Temple
            { 111, 91 },  // Tennessee
            { 112, 92 },  // Texas
            { 113, 93 },  // Texas A&M
            { 114, 218 }, // Texas State
            { 115, 94 },   // Texas Tech
            { 116, 95 },   // Toledo
            { 117, 143 },   // Troy
            { 118, 96 },   // Tulane
            { 119, 97 },   // Tulsa
            { 120, 98 },   // UAB
            { 121, 18 },   // UCF
            { 122, 99 },   // UCLA
            { 123, 65 },   // UL Monroe
            { 124, 181 },   // UMass
            { 125, 101 },   // UNLV
            { 126, 102 },   // USC
            { 127, 144 },   // USF
            { 128, 103 },   // Utah
            { 129, 104 },   // Utah State
            { 130, 105 },   // UTEP
            { 131, 232 },   // UTSA
            { 132, 106 },   // Vanderbilt
            { 133, 107 },   // Virginia
            { 134, 108 },   // Virginia Tech
            { 135, 109 },   // Wake Forest
            { 136, 110 },   // Washington
            { 137, 111 },   // Washington St.
            { 138, 112 },   // West Virginia
            { 139, 211 },   // W. Kentucky
            { 140, 113 },   // W. Michigan
            { 141, 114 },   // Wisconsin
            { 142, 115 },    // Wyoming
        };

        public static readonly int[] TeamIds = TeamIdToOldIdMap.Select(kvp => kvp.Value).Where(v => v < 160 || v > 164).ToArray();
    }
}