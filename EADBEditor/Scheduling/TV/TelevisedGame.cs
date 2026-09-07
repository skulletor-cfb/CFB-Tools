using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.Scheduling
{
    public class TelevisedGame
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
        public bool IsAccGame => ConferenceOwner == TableUtility.ACCId;
        public bool IsHawaiiGame => HomeTeam == 32;
        public bool IsBig10Game => ConferenceOwner == TableUtility.Big10Id;
        public bool IsBig12Game => ConferenceOwner == TableUtility.Big12Id;
        public bool IsPac12Game => ConferenceOwner == TableUtility.Pac16Id;
        public bool IsNotreDameHomeGame => HomeTeam.IsIndependentND();
        public bool IsShamrockSeries => (IsNotreDameHomeGame || AwayTeam == TableUtility.NotreDameId) && GTOD == new TimeSlot(8, 7).GTOD;
        public bool IsNotreDameAtNavy => (HomeTeam == 57 && AwayTeam == TableUtility.NotreDameId);
        public bool BothTeamsRanked { get; }
        public bool IsArizonaGame => HomeTeam == 4 || HomeTeam == 5;
        public bool IsASUvAU => IsArizonaGame && (AwayTeam == 4 || AwayTeam == 5);
        public TelevisedGame(MaddenRecord mr, Dictionary<int, MaddenRecord> teams)
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
            ConferenceOwner = (IsNotreDameHomeGame || IsShamrockSeries || IsNotreDameAtNavy) ? TableUtility.NotreDameId : TableUtility.GameConferenceOwner(HomeTeam);
            Week = mr.GameWeek();
            Day = mr.GameDay();
            GTOD = mr.GTOD();
            IsConferenceGame = ConferenceOwner == TableUtility.GameConferenceOwner(AwayTeam);
            IsSecAccGame = (AwayTeam.IsSECTeam() && HomeTeam.IsAccTeam()) || (HomeTeam.IsSECTeam() && AwayTeam.IsAccTeam());
            IsP5Game = AwayTeam.IsP5OrND() && HomeTeam.IsP5OrND();
            HomeTeamIsP5 = HomeTeam.IsP5OrND();
            Score += IsP5Game ? -5 : 0;
            IsFCSGame = AwayTeam.IsFcsTeam();
            Score += IsFCSGame ? 100 : 0;

            if ((AwayTeam == 70 && HomeTeam == 51) || (AwayTeam == 51 && HomeTeam == 70))
            {
                Score += -100000;
            }

            BothTeamsRanked = (home.CoachPollRanking() <= 25 || home.MediaPollRanking() <= 25) && (away.CoachPollRanking() <= 25 || away.MediaPollRanking() <= 25) ;
        }

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
            Assigned = true;
            this.Record["GTOD"] = time.ToGTOD();
            this.Record["GDAT"] = time.Day.ToString();
            return this;
        }

        public void PreAssigned()
        {
            Assigned = true;
            Selected = true;
        }

        public TelevisedGame Deselect()
        {
            Selected = false;
            return this;
        }
    }
}