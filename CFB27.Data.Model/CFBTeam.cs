using Newtonsoft.Json;
using System.Collections.Generic;

namespace CFB27.Data.Model
{
    public class CFBTeam : BaseRecord
    {
        #region Properties
        public string DefaultPhilosophy { get; set; }
        public string DefenseActiveAbilitiesPlayers { get; set; }
        public string DefensiveCoordinator { get; set; }
        public string DefensiveGameplan { get; set; }
        public string DepthChart { get; set; }
        public string DepthChartConfig { get; set; }
        public string EquipmentTeamUpgradeStatusList { get; set; }
        public string FanHappinessTrackingTable { get; set; }
        public string FocusTrainingList { get; set; }
        public string FormationSubs { get; set; }
        public string FranchiseTaggedPlayer { get; set; }
        public string GameStatRecords { get; set; }
        public string GeneralManager { get; set; }
        public string HCContractGoal1 { get; set; }
        public string HCContractGoal2 { get; set; }
        public string HCContractGoal3 { get; set; }
        public string HeadCoach { get; set; }
        public string HeadScout { get; set; }
        public string HeadTrainer { get; set; }
        public string HistoryEntries { get; set; }
        public string HomeFieldAdvantage { get; set; }
        public string MarketedLegends { get; set; }
        public string MarketedPlayers { get; set; }
        public string MySchoolTrackingTable { get; set; }
        public string OCContractGoal1 { get; set; }
        public string OCContractGoal2 { get; set; }
        public string OCContractGoal3 { get; set; }
        public string OffenseActiveAbilitiesPlayers { get; set; }
        public string OffensiveCoordinator { get; set; }
        public string OffensiveGameplan { get; set; }
        public string Owner { get; set; }
        public string Philosophy { get; set; }
        public string PipelineInitialInfluence { get; set; }
        public string PlaycallHistory { get; set; }
        public string PlayerPersonnel { get; set; }
        public string PracticeSquad { get; set; }
        public string SupportStaffStatusList { get; set; }
        public string StaffPersonBlacklist { get; set; }
        public string Stadium { get; set; }
        public string SpecialTeamsCoach { get; set; }
        public string RecruitingBoard { get; set; }
        public string SelectedOffensiveDrill { get; set; }
        public string SelectedDefensiveDrill { get; set; }
        public string RelativeTempBuffsList { get; set; }
        public string RevenueTable { get; set; }
        public string Rival1TeamRef { get; set; }
        public string Rival2TeamRef { get; set; }
        public string SeasonStatRecords { get; set; }
        public string Scouts { get; set; }
        public string ScoutingFocus { get; set; }
        public string SchoolPipelineInfluenceList { get; set; }
        public string Rival3TeamRef { get; set; }
        public string Rivalries { get; set; }
        public string Roadmap { get; set; }
        public string Roster { get; set; }
        public string AbsoluteTempBuffsList { get; set; }
        public string ActiveStoryArcs { get; set; }
        public string WeekPopularityList { get; set; }
        public string WeeklyTrainingData { get; set; }
        public string AltStadium { get; set; }
        public string UserCharacter { get; set; }
        public string TradePlayerBlackList { get; set; }
        public string CareerStatRecords { get; set; }
        public string City { get; set; }
        public string TeamValueTrackingTable { get; set; }
        public string TeamUpgradeProgramStatus { get; set; }
        public string TeamTraditions { get; set; }
        public string TeamTendencyStats { get; set; }
        public string TeamSettingRef { get; set; }
        public string TeamSeriesHistory { get; set; }
        public string TeamSeasonStats { get; set; }
        public string CoachTalentEffects { get; set; }
        public string CommittedPlayers { get; set; }
        public string TeamHistoricalData { get; set; }
        public string TeamGameStatsRegSeason { get; set; }
        public string TeamGameStatsPreSeason { get; set; }
        public string DefaultOwner { get; set; }
        public string DCContractGoal3 { get; set; }
        public string DCContractGoal2 { get; set; }
        public string DCContractGoal1 { get; set; }
        public string CurrentPopularity { get; set; }
        public string TeamBuilderData { get; set; }
        public string ContractOfferBlacklist { get; set; }
        public string TeamFan_Family { get; set; }
        public string TeamFan_Hardcore { get; set; }
        public string TeamFan_New { get; set; }
        public string TeamFan_Pessimistic { get; set; }
        public string TeamFan_Optimistic { get; set; }
        public string TEAM_PREFIX_NAME { get; set; }
        public string Hashtag2 { get; set; }
        public string AssetName { get; set; }
        public int BlacklistUserIdLower { get; set; }
        public int BlacklistUserIdUpper { get; set; }
        public string UniformPrefix { get; set; }
        public string UniformAssetName { get; set; }
        public string ShortName { get; set; }
        public float ToughestPlacesScore { get; set; }
        public string DisplayName { get; set; }
        public float CFPPoll_CurrentPoints { get; set; }
        public string TEAM_AFL_DISPLAYNAME { get; set; }
        public string PrestigeDisplay { get; set; }
        public string TEAM_ALT_LOGO_ASSETNAME { get; set; }
        public int CurrentBalance { get; set; }
        public int ThisWeekStartBalance { get; set; }
        public string PaintedFanThirdWord { get; set; }
        public string PaintedFanSecondWord { get; set; }
        public string PaintedFanFirstWord { get; set; }
        public string Hashtag1 { get; set; }
        public string NickNameAlt { get; set; }
        public string TEAM_LOGO_SMALL_ALT_ASSETNAME { get; set; }
        public int LastWeekStartBalance { get; set; }
        public string TEAM_LOGO_ASSETNAME { get; set; }
        public string LongName { get; set; }
        public string Mascot_AssetName { get; set; }
        public string Motto { get; set; }
        public string TEAM_DBASSETNAME { get; set; }
        public string NickName { get; set; }
        public string HomeUniformShade { get; set; }
        public int LastWeekStadiumPaymentExpense { get; set; }
        public bool AllowsTripleOptionCoaches { get; set; }
        public int LastWeekPlayerMarketingExpense { get; set; }
        public bool TeamRegressionOccurred { get; set; }
        public int StartingBalance { get; set; }
        public string PendingFacilityUpgrade { get; set; }
        public int ThisWeekStadiumUpkeepExpense { get; set; }
        public string DesiredSpecialtyType { get; set; }
        public int ThisWeekPlayerBonusExpense { get; set; }
        public string PreferredSecSchemeType { get; set; }
        public int LastWeekAdvertisingExpense { get; set; }
        public string PreferredSchemeType { get; set; }
        public int LastWeekPlayerBonusExpense { get; set; }
        public string TeamBuilding { get; set; }
        public int StadiumPaymentWeekly { get; set; }
        public string ContentionPhase { get; set; }
        public int LastWeekPlayerSalariesExpense { get; set; }
        public string AwayUniformShade { get; set; }
        public bool IsDefDrillSim { get; set; }
        public int LastWeekStaffSalariesExpense { get; set; }
        public bool HasStrictCodeOfConduct { get; set; }
        public bool WasFacilityDowngraded { get; set; }
        public int Capital { get; set; }
        public bool IsTeamBuilder { get; set; }
        public bool HasPrideStickers { get; set; }
        public int LastWeekSharedRevenue { get; set; }
        public bool TEAM_LOCKED { get; set; }
        public bool FocusTainingComplete { get; set; }
        public int LastWeekStadiumUpkeepExpense { get; set; }
        public int SeasonConfPointsAgainst { get; set; }
        public int HighestGameAttendance { get; set; }
        public int SeasonConfPointsFor { get; set; }
        public int TEAM_SALARY { get; set; }
        public int SeasonDivPointsAgainst { get; set; }
        public int ScoutingPoints { get; set; }
        public int SeasonDivPointsFor { get; set; }
        public int AverageAttendance { get; set; }
        public int RolloverProgramPoints { get; set; }
        public int SalCapNextYearReserve { get; set; }
        public int FacilitiesProgramPointsSpent { get; set; }
        public int RolloverCap { get; set; }
        public int AccumulatedCoachContractGoalsPoints { get; set; }
        public int SalCapNextYearSalaryReserve { get; set; }
        public int CoachContractGoalsProgramPoints { get; set; }
        public int SalCapOffersReserve { get; set; }
        public int StaffProgramPointsSpent { get; set; }
        public int SalCapRookieReserve { get; set; }
        public int ProgramPointBudget { get; set; }
        public int SalCapRosterFillCount { get; set; }
        public int NILProgramPointsSpent { get; set; }
        public int SalCapRosterFillReserve { get; set; }
        public int ProgramTraditionsProgramPoints { get; set; }
        public int SalCapRosterReserve { get; set; }
        public int StadiumAtmosphereProgramPoints { get; set; }
        public int SalCapSpendingMoney { get; set; }
        public int ConferencePrestigeProgramPoints { get; set; }
        public int SalCapNextYearOfferReserve { get; set; }
        public int BrandExposureProgramPoints { get; set; }
        public int SalCapThisYearOfferReserve { get; set; }
        public int RecruitProgramPointsSpent { get; set; }
        public int SalCapNextYearCapRoom { get; set; }
        public bool WasFacilityUpgradedLastYear { get; set; }
        public int CoachesPoll_CurrentPoints { get; set; }
        public int SalCapCapRoom { get; set; }
        public bool IsHiringBonusAvailableHC { get; set; }
        public int MediaPoll_CurrentPoints { get; set; }
        public int NextYearCapPenalties { get; set; }
        public bool SharedStadiumSlot { get; set; }
        public int HomeTie { get; set; }
        public int ThisYearCapPenalties { get; set; }
        public bool EndOfSeasonProcessed { get; set; }
        public bool TEAM_ISAFL { get; set; }
        public int SalCapLowReplaceSalary { get; set; }
        public int RemainingProgramPoints { get; set; }
        public string PlayoffStatus { get; set; }
        public int SeasonLeagPointsAgainst { get; set; }
        public int FacilitiesRenewalCostReserved { get; set; }
        public int HomeWin { get; set; }
        public int CoachesPoll_NumVoters { get; set; }
        public int SeasonLeagPointsFor { get; set; }
        public int TEAM_LOGO_RIGHT_ANGLED { get; set; }
        public int DefensiveCoordinatorPointBudget { get; set; }
        public int MediaPoll_NumVoters { get; set; }
        public int TEAM_GROUP { get; set; }
        public int PresentationId { get; set; }
        public int CoachesPoll_FirstPlaceVotes { get; set; }
        public int ExpectedContractPoints_LastYear { get; set; }
        public int MediaPoll_FirstPlaceVotes { get; set; }
        public string NickNameCommentaryId { get; set; }
        public int ExpectedContractPoints_ThisYear { get; set; }
        public int OffensiveCoordinatorPointBudget { get; set; }
        public int HeadCoachProgramPointBudget { get; set; }
        public int TEAM_REPUTATION { get; set; }
        public int TEAM_ALT_LOGO { get; set; }
        public int TEAM_LOGO { get; set; }
        public int TEAM_DEFPLAYBOOK { get; set; }
        public int TEAM_LOGO_SMALL_ALT { get; set; }
        public int StaffPoints { get; set; }
        public int ExpectedContractPoints_TwoYearsAgo { get; set; }
        public int SeatingType4TicketPrice { get; set; }
        public int TEAM_ORIGID { get; set; }
        public int TEAM_ORDER { get; set; }
        public int SeatingType2TicketPrice { get; set; }
        public int SeatingType3TicketPrice { get; set; }
        public int TEAM_OFFPLAYBOOK { get; set; }
        public int YearStartOfFootballProgram { get; set; }
        public int SeatingType1TicketPrice { get; set; }
        public bool TEAM_HAS_SECONDARY_COLOR { get; set; }
        public bool TEAM_HASCHEERLEADERS { get; set; }
        public int DivisionLoss { get; set; }
        public int SeasonWinPct { get; set; }
        public int YearSchoolEstablished { get; set; }
        public string CurrentDefensiveScheme { get; set; }
        public int ConfWin { get; set; }
        public int ConfTie { get; set; }
        public int ConfLoss { get; set; }
        public string CurrentOffensiveScheme { get; set; }
        public int NonConfTie { get; set; }
        public int NonConfWin { get; set; }
        public int TEAM_LOGO_LEFT_ANGLED { get; set; }
        public string DefaultDefensiveScheme { get; set; }
        public int DivisionTie { get; set; }
        public int DivisionWin { get; set; }
        public int NonConfLoss { get; set; }
        public string DesiredSecArchetype { get; set; }
        public int TEAM_BACKGROUNDCOLORG { get; set; }
        public int TEAM_CROWDPALETTE { get; set; }
        public int TEAM_BACKGROUNDCOLORG2 { get; set; }
        public string DesiredTertArchetype { get; set; }
        public int TEAM_BACKGROUNDCOLORR { get; set; }
        public string TEAM_ENDPLAY_ANIM_VAL { get; set; }
        public int TEAM_BACKGROUNDCOLORR2 { get; set; }
        public int MediaPoll_HiddenCurrentRank { get; set; }
        public int MediaPoll_LastWeeksRank { get; set; }
        public int MediaPoll_StartOfSeasonRank { get; set; }
        public int TEAM_GOALPOST { get; set; }
        public int TEAM_LOGO_PRIMARYR { get; set; }
        public int TEAM_LOGO_PRIMARYG { get; set; }
        public int TEAM_LOGO_PRIMARYB { get; set; }
        public int MediaPoll_CurrentRank { get; set; }
        public int ForcedStreak { get; set; }
        public int TEAM_LOGO_SECONDARYR { get; set; }
        public int TEAM_LOGO_SECONDARYG { get; set; }
        public int TEAM_LOGO_SECONDARYB { get; set; }
        public int TEAM_LOGO_TRIM_PRIME_R { get; set; }
        public int TEAM_LOGO_TRIM_SEC_B { get; set; }
        public int TEAM_LOGO_TRIM_SEC_G { get; set; }
        public int TEAM_LOGO_TRIM_SEC_R { get; set; }
        public string TEAM_LOGO_SWAPPABLE_LIBRARY_PATH { get; set; }
        public int TEAM_LOGO_TRIM_PRIME_B { get; set; }
        public int TEAM_LOGO_TRIM_PRIME_G { get; set; }
        public int HighestAttendanceRank { get; set; }
        public int SeasonWinLossStreak { get; set; }
        public int ToughestPlacesRank { get; set; }
        public int WinLossStreakAgainstRankedTeams { get; set; }
        public string TEAM_LOGO_SMALL_ALT_SWAPPABLE_LIBRARY_PATH { get; set; }
        public int CoachesPoll_CurrentRank { get; set; }
        public int CFPPoll_LastWeeksRank { get; set; }
        public int CFPPoll_CurrentRank { get; set; }
        public int TopClassRank { get; set; }
        public int TeamIndex { get; set; }
        public int CoachesPoll_LastWeeksRank { get; set; }
        public int CoachesPoll_HiddenCurrentRank { get; set; }
        public int TeamRank { get; set; }
        public int PrestigeRank { get; set; }
        public int PrevSeasonDivStanding { get; set; }
        public int PrevSeasonLeagStanding { get; set; }
        public int PrevSeasonWinLossStreak { get; set; }
        public int TEAM_ALT_LOGO_PRIME_R { get; set; }
        public int CurSeasonConfStanding { get; set; }
        public int TEAM_ALT_LOGO_PRIME_G { get; set; }
        public int TEAM_ALT_LOGO_PRIME_B { get; set; }
        public int CurSeasonDivStanding { get; set; }
        public int TEAM_ALT_LOGO_SEC_R { get; set; }
        public int TEAM_ALT_LOGO_SEC_G { get; set; }
        public int TEAM_ALT_LOGO_SEC_B { get; set; }
        public int TEAM_ALT_LOGO_TRIM_PRIME_R { get; set; }
        public int TEAM_ALT_LOGO_TRIM_PRIME_G { get; set; }
        public int TEAM_ALT_LOGO_TRIM_PRIME_B { get; set; }
        public string TEAM_ALT_LOGO_SWAPPABLE_LIBRARY_PATH { get; set; }
        public int DefensiveRank { get; set; }
        public string TEAM_SHOE { get; set; }
        public string TEAM_TYPE { get; set; }
        public int CurSeasonLeagStanding { get; set; }
        public string DesiredPrimArchetype { get; set; }
        public int TEAM_ALT_LOGO_TRIM_SEC_B { get; set; }
        public int OffensiveRank { get; set; }
        public int TEAM_BACKGROUNDCOLORB2 { get; set; }
        public bool IsHiringBonusAvailableDC { get; set; }
        public int ActiveFreeAgentNegotiationCount { get; set; }
        public int TEAM_ALT_LOGO_TRIM_SEC_G { get; set; }
        public int TEAM_ALT_LOGO_TRIM_SEC_R { get; set; }
        public int TEAM_BACKGROUNDCOLORB { get; set; }
        public int TeamPrestige { get; set; }
        public int TransactionAndCutCount { get; set; }
        public int UserCoachExpressedInterestCount { get; set; }
        public int ActiveRosterSize { get; set; }
        public int HomeLoss { get; set; }
        public int TEAM_HOMEUNIFORMORDER { get; set; }
        public int TEAM_RATINGDB { get; set; }
        public int TEAM_RATINGST { get; set; }
        public int TEAM_RATINGRB { get; set; }
        public int DesiredTeamCaptains { get; set; }
        public int IRRemovalCount { get; set; }
        public int TEAM_RATINGOFF { get; set; }
        public int TEAM_RATINGLB { get; set; }
        public int TEAM_RATINGDL { get; set; }
        public int TEAM_RATINGDEF { get; set; }
        public string ProgramPointsStadiumAtmosphereGrade { get; set; }
        public int TeamHistory { get; set; }
        public int TEAM_RATINGTE { get; set; }
        public int TEAM_RATINGOVR { get; set; }
        public int TEAM_RATINGOL { get; set; }
        public string WeeklyDefenseMedal { get; set; }
        public int SalCapNextYearRosterSize { get; set; }
        public int SalCapNextYearOfferCount { get; set; }
        public int TEAM_RATINGQB { get; set; }
        public int TEAM_RATINGWR { get; set; }
        public string WeeklyOffenseMedal { get; set; }
        public int RoadLoss { get; set; }
        public int SalCapRookieCount { get; set; }
        public int RoadTie { get; set; }
        public int RoadWin { get; set; }
        public int TEAM_ROSTER_TYPE { get; set; }
        public int OverTimeWin { get; set; }
        public int StadiumPaymentYearsLeft { get; set; }
        public int SalCapThisYearOfferCount { get; set; }
        public int SalCapRosterSize { get; set; }
        public string ProgramPointsBrandExposureGrade { get; set; }
        public int OutDoorWin { get; set; }
        public int OverallPopularity { get; set; }
        public int OverTimeLoss { get; set; }
        public int OverTimeTie { get; set; }
        public string ProgramPointsBudgetGrade { get; set; }
        public int JumpBacksUsed { get; set; }
        public int TEAM_FLAGRESID { get; set; }
        public int OutDoorLoss { get; set; }
        public int OutDoorTie { get; set; }
        public string DefaultOffensiveScheme { get; set; }
        public int TEAM_PREVSEASLOSSES { get; set; }
        public int InDoorWin { get; set; }
        public int InDoorTie { get; set; }
        public int InDoorLoss { get; set; }
        public bool DesiresAlumni { get; set; }
        public bool IsReSigning { get; set; }
        public int TEAM_PREVSEASWINS { get; set; }
        public int TeamPrestigeBias { get; set; }
        public int PrevYearConfWins { get; set; }
        public int PrevYearConfLosses { get; set; }
        public int TEAM_PREVSEASTIES { get; set; }
        public int LastWeekConferenceStanding { get; set; }
        public int NumGamesScheduled { get; set; }
        public int TopClassConferenceRank { get; set; }
        public int NumRequiredConferenceGames { get; set; }
        public int LastSeasonTransfersSigned { get; set; }
        public int LastWeekCommittedRecruits { get; set; }
        public bool IsEntitlementAwardEnabled { get; set; }
        public string ADPrioritySecondary { get; set; }
        public string DCContractGoal3Status { get; set; }
        public string DCContractGoal2Status { get; set; }
        public string ADPriorityPrimary { get; set; }
        public string DCContractGoal1Status { get; set; }
        public string ADPriorityGuaranteed { get; set; }
        public string ProgramPointsProgramTraditionsGrade { get; set; }
        public string ProgramPointsConferencePrestigeGrade { get; set; }
        public int LastSeasonTransfersLost { get; set; }
        public bool IsOffDrillSim { get; set; }
        public bool OffensiveDrillComplete { get; set; }
        public int DIV_SLOTNUMBER { get; set; }
        public string PlayoffRoundReached { get; set; }
        public string OCContractGoal1Status { get; set; }
        public string OCContractGoal2Status { get; set; }
        public string OCContractGoal3Status { get; set; }
        public string HCContractGoal1Status { get; set; }
        public string HCContractGoal2Status { get; set; }
        public int CoachSeasonGoalLevelReached { get; set; }
        public string HCContractGoal3Status { get; set; }
        public string ADPriorityTertiary { get; set; }
        public bool TEAM_VISIBLEINQUICKSTART { get; set; }
        public bool IsHiringBonusAvailableOC { get; set; }
        public bool TEAM_VISIBLE { get; set; }
        public bool TEAM_THANKSGIVEHOME { get; set; }
        public bool TEAM_SHAREDSTADIUMSLOT { get; set; }
        public bool TEAM_SHAREDSTADIUMOWNER { get; set; }
        public bool IsMilitary { get; set; }
        public bool DefensiveDrillComplete { get; set; }
        public string LastSeasonPlayoffRoundReached { get; set; }
        public int FacilitiesLevel { get; set; }
        public string TeamApparel { get; set; }
        public string ADDemeanor { get; set; }
        #endregion

        [JsonIgnore]
        public int TeamId => TeamIdToOldIdMap[this.TeamIndex];

        [JsonIgnore]
        public CFBTeamHistoricalData HistoricalData { get; set; }

        [JsonIgnore]
        public int Win => ConfWin + NonConfWin;

        [JsonIgnore]
        public int Loss => ConfLoss + NonConfLoss;

        /// <summary>
        /// ids in CFB27 mapped to the classic ids
        /// </summary>
        /// <summary>
        /// ids in CFB27 mapped to the classic ids
        /// </summary>
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
    }


    public class CFBTeamHistoricalData : BaseRecord
    {
        public string TeamSeriesHistory { get; set; }
        public int LongestHomeWinStreak { get; set; }
        public int Wins { get; set; }
        public int WeeksRankedTop25InMediaPoll { get; set; }
        public int AllAmericans1stAnd2nd { get; set; }
        public int HomeWins { get; set; }
        public int HomeLosses { get; set; }
        public int RivalryLosses { get; set; }
        public int PlayersDrafted { get; set; }
        public int Losses { get; set; }
        public int Top10RecruitingClasses { get; set; }
        public int CurrentHomeWinStreak { get; set; }
        public int RivalryWins { get; set; }
        public int NY6BowlsWon { get; set; }
        public int TopRecruitingClasses { get; set; }
        public int Top5RecruitingClasses { get; set; }
        public int Top25RecruitingClasses { get; set; }
        public int CFPSMade { get; set; }
        public int CFPSWon { get; set; }
        public int HeismanWinners { get; set; }
        public int NY6BowlsMade { get; set; }
        public int NationalChampionshipsMade { get; set; }
        public int NationalChampionshipsWon { get; set; }
        public int Ties { get; set; }
        public int BowlsMade { get; set; }
        public int BowlsWon { get; set; }
        public int ConferenceChampionshipsMade { get; set; }
        public int ConferenceChampionshipsWon { get; set; }
        public int HomeTies { get; set; }
    }
}
