#nullable disable
using System.Text.Json.Serialization;
using MaddenFranchise.Dynasty;
using NCAA14Utility.Scheduling;

namespace ScheduleMaker.TV
{
    public class TelevisedGame
    {
        public DateTime Date => this.GetDate();

        public int Score { get; }

        /// <summary>The home team's conference id; Notre Dame's home games (and Notre Dame at Navy) are owned by <see cref="ScheduleBase.NotreDame"/>.</summary>
        public int ConferenceOwner { get; }

        public int Week { get; }

        /// <summary>The day the game had going in: 0 Monday ... 5 Saturday ... 6 Sunday (<see cref="TimeSlot.Days"/>).</summary>
        public int Day { get; }

        /// <summary>A week 0-1 game off Saturday (a hand-set opener): it keeps its day and kickoff (<see cref="OwnSlot"/>).</summary>
        public bool KeepsDay => Week <= 1 && Day != 5;

        /// <summary>The day and kickoff the game had going in.</summary>
        public TimeSlot OwnSlot => new TimeSlot(GameTimeOfDay, Week, Day);

        /// <summary>Whether the game may take that slot: a hand-set opener only takes its own day and kickoff.</summary>
        public bool CanPlayOn(TimeSlot slot) => !KeepsDay || (slot.Day == Day && slot.GTOD == OwnSlot.GTOD);

        /// <summary>The save's kickoff in minutes after midnight, or -1 for a range such as NIGHT_FIRST.</summary>
        public int GTOD { get; }
        public GameTimeOfDay GameTimeOfDay { get; }
        public int AwayTeam { get; }
        public int HomeTeam { get; }
        public bool IsConferenceGame { get; }
        [JsonIgnore]
        public ScheduledGame Record { get; }
        public bool IsSecAccGame { get; }
        public bool IsSecConferenceGame => IsConferenceGame && IsSecGame;
        public bool IsP5Game { get; }
        public bool HomeTeamIsP5 { get; }
        public bool IsFCSGame { get; }
        /// <summary>
        /// selected means a network has taken it
        /// </summary>
        public bool Selected { get; private set; }
        /// <summary>
        /// assigned means it's in a network/timeslot
        /// </summary>
        public bool Assigned { get; private set; }
        public bool IsSecGame => ConferenceOwner == ScheduleHelpers.SEC;
        public bool IsAccGame => ConferenceOwner == ScheduleHelpers.ACC;
        public bool IsHawaiiGame => HomeTeam == ScheduleBase.Hawaii;
        public bool IsArmyGame => HomeTeam == ScheduleBase.Army;
        public bool IsNavyGame => HomeTeam == ScheduleBase.Navy;
        public bool IsBig10Game => ConferenceOwner == ScheduleHelpers.Big10;
        public bool IsBig12Game => ConferenceOwner == ScheduleHelpers.Big12;
        public bool IsPac12Game => ConferenceOwner == ScheduleHelpers.Pac12;
        public bool IsMACGame => ConferenceOwner == ScheduleHelpers.MAC;
        public bool IsCUSAGame => ConferenceOwner == ScheduleHelpers.CUSA;
        public bool IsNotreDameHomeGame => HomeTeam.IsIndependentND();
        public bool IsNotreDameAtNavy => (HomeTeam == ScheduleBase.Navy && AwayTeam == ScheduleBase.NotreDame);
        public bool IsNotreDameGame => HomeTeam == ScheduleBase.NotreDame || AwayTeam == ScheduleBase.NotreDame;
        public bool BothTeamsRanked { get; }
        public bool IsArizonaGame => HomeTeam == ScheduleBase.Arizona || HomeTeam == ScheduleBase.ASU;
        public bool IsASUvAU => IsArizonaGame && (AwayTeam == ScheduleBase.Arizona || AwayTeam == ScheduleBase.ASU);
        public bool IsAirForce => HomeTeam == ScheduleBase.AF;
        public bool IsMilitaryHomeGame => IsAirForce || HomeTeam == ScheduleBase.Army || HomeTeam == ScheduleBase.Navy;
        public bool AwayTeamIsP5 => AwayTeam.IsP5OrND();
        public bool IntraConferenceP5 => AwayTeamIsP5 && HomeTeamIsP5 && !TvTeams.TeamsInSameConference(AwayTeam, HomeTeam);
        public bool IsMWCGame => ConferenceOwner == ScheduleHelpers.MWC;
        public bool IsAmericanGame => ConferenceOwner == ScheduleHelpers.American;
        public bool IsSunBeltGame => ConferenceOwner == ScheduleHelpers.SBC;
        public bool IsEggBowl => CheckMatchup(ScheduleBase.MissSt, ScheduleBase.OleMiss);
        public bool IsTexasShowDown => CheckMatchup(ScheduleBase.Texas, ScheduleBase.TAMU);
        public bool IsCivilWar => CheckMatchup(ScheduleBase.KU, ScheduleBase.Missouri);
        public bool IsTexasTechGame => CheckMatchup(ScheduleBase.Texas, ScheduleBase.TT);
        public bool IsTCUBU => CheckMatchup(ScheduleBase.Baylor, ScheduleBase.TCU);
        public bool IsSMUHOU => CheckMatchup(ScheduleBase.Houston, ScheduleBase.SMU);
        /// <summary>A neutral-site or kickoff game.</summary>
        public bool IsNeutralSite { get; }
        public bool IsKickoffGame { get; }
        /// <summary>Rivalry.Name when the two teams are rivals.</summary>
        public string Rivalry { get; }
        public string HomeName { get; }
        public string AwayName { get; }
        public int AwayRank { get; }
        public int HomeRank { get; }
        public string HomeConference { get; }
        public string AwayConference { get; }
        public bool IsP5G5Game => (HomeTeam.IsP5OrND() && AwayTeam.IsG5()) || (AwayTeam.IsP5OrND() && HomeTeam.IsG5());
        public TelevisedGame(ScheduledGame game, PreseasonSchedule schedule, IReadOnlyDictionary<(int, int), string> rivalries)
        {
            Record = game;
            HomeTeam = game.HomeTeamRow;
            AwayTeam = game.AwayTeamRow;
            var away = schedule.TeamTable.Records[AwayTeam];
            var home = schedule.TeamTable.Records[HomeTeam];
            AwayName = schedule.TeamName(AwayTeam);
            HomeName = schedule.TeamName(HomeTeam);
            AwayRank = MediaPollRanking(away);
            HomeRank = MediaPollRanking(home);
            var score = CoachPollRanking(home) + HomeRank + CoachPollRanking(away) + AwayRank;
            score /= 2;
            Rivalry = rivalries.TryGetValue((Math.Min(AwayTeam, HomeTeam), Math.Max(AwayTeam, HomeTeam)), out var rivalry) ? rivalry : null;
            score += Rivalry != null ? -10 : 0;
            score += TvTeams.TeamsInSameConference(AwayTeam, HomeTeam) ? -5 : 0;
            // more prestigious game should rank higher; NCAA 14's prestige ran 1-6, CFB 27's runs 0-10 (6/10 of NCAA 14's 10/15)
            score -= ((Prestige(home) + Prestige(away)) * 2) / 5;

            ConferenceOwner = (IsNotreDameHomeGame || IsNotreDameAtNavy) ? ScheduleBase.NotreDame : TvTeams.GameConferenceOwner(HomeTeam, AwayTeam);
            AwayConference = AwayTeam.IsFcsTeam() ? "FCS" : schedule.ConferenceOf(AwayTeam);
            HomeConference = HomeTeam.IsFcsTeam() ? "FCS" : schedule.ConferenceOf(HomeTeam);
            Week = game.Week;
            Day = Array.IndexOf(TimeSlot.Days, game.DayOfWeek) is var day && day >= 0 ? day : 5;
            GTOD = TelevisionScheduler.Minutes(game.TimeSlot);
            GameTimeOfDay = new GameTimeOfDay(Math.Max(GTOD, 0));
            IsConferenceGame = TvTeams.TeamsInSameConference(AwayTeam, HomeTeam);
            IsSecAccGame = (AwayTeam.IsSECTeam() && HomeTeam.IsAccTeam()) || (HomeTeam.IsSECTeam() && AwayTeam.IsAccTeam());
            IsP5Game = AwayTeam.IsP5OrND() && HomeTeam.IsP5OrND();
            HomeTeamIsP5 = HomeTeam.IsP5OrND();
            IsFCSGame = AwayTeam.IsFcsTeam() || HomeTeam.IsFcsTeam();
            IsNeutralSite = game.StadiumId != 0;
            IsKickoffGame = game.IsKickoffGame;

            score += IsP5G5Game ? 3 : 0;
            score += IsFCSGame ? 1000 : 0;
            score += IsP5Game ? -5 : 0;
            Score = score;

            BothTeamsRanked = (CoachPollRanking(home) <= 25 || MediaPollRanking(home) <= 25) && (CoachPollRanking(away) <= 25 || MediaPollRanking(away) <= 25);
        }

        // FCS teams aren't in the polls; NCAA 14 ranked every FBS team, as CFB 27 does (1-138)
        private static int MediaPollRanking(MaddenFranchise.FranchiseFileRecord team) => Rank(team, "MediaPoll_CurrentRank");
        private static int CoachPollRanking(MaddenFranchise.FranchiseFileRecord team) => Rank(team, "CoachesPoll_CurrentRank");
        private static int Rank(MaddenFranchise.FranchiseFileRecord team, string field) => Convert.ToInt32(team[field]) is var rank and > 0 ? rank : 200;
        private static int Prestige(MaddenFranchise.FranchiseFileRecord team) => Convert.ToInt32(team["TeamPrestige"]);

        public override string ToString() => $"{Week}: {AwayName} at {HomeName}";
        public override bool Equals(object obj)
        {
            return obj is TelevisedGame other &&
                Week == other.Week &&
                AwayTeam == other.AwayTeam &&
                HomeTeam == other.HomeTeam;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public TelevisedGame Select()
        {
            Selected = true;
            return this;
        }

        public TelevisedGame Assign(TimeSlot time)
        {
            return PreAssign(time);
        }

        /// <summary>Takes the game; with a time, also moves it to that day and the nearest CFB 27 kickoff.</summary>
        public TelevisedGame PreAssign(TimeSlot time = null)
        {
            if (time != null)
            {
                this.Record.TimeSlot = time.ToGameTimeRange();
                this.Record.DayOfWeek = time.DayOfWeek;
            }

            Assigned = true;
            Selected = true;
            return this;
        }

        public TelevisedGame Deselect()
        {
            Selected = false;
            return this;
        }

        public bool CheckMatchup(int a, int b)
        {
            return (HomeTeam == a && AwayTeam == b) || (HomeTeam == b && AwayTeam == a);
        }
    }
}
