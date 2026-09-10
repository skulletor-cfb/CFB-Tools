using EA_DB_Editor.Scheduling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EA_DB_Editor
{

    public class PreseasonScheduledGame
    {
        public int HomeTeam { get; set; }
        public int AwayTeam { get; set; }
        public int Week { get { return WeekIndex + 1; } }
        public int WeekIndex { get; set; }
        public int GameNumber { get; set; }
        public MaddenRecord MaddenRecord { get; set; }

        private bool mustReplace = false;

        public bool MustReplace
        {
            get
            {
                return mustReplace;
            }

            set
            {
                mustReplace = value;
            }
        }

        public int? LockedWeek { get; private set; }

        public void CheckForLock(ConferenceLocks locks)
        {
            this.LockedWeek = locks.CheckWeekLock(this);
        }

        public void SetWeek(int week)
        {
            this.WeekIndex = week;
            MaddenRecord["SEWN"] = week.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0}-{1} :  {2} at {3}", WeekIndex, GameNumber, AwayTeam, HomeTeam);
        }

        public bool IsConferenceGame()
        {
            return TableUtility.TeamAndConferences[AwayTeam] == TableUtility.TeamAndConferences[HomeTeam] &&
                TableUtility.TeamAndConferences[HomeTeam] != 17;
        }

        public int? ConferenceGameId()
        {
            if (!this.IsConferenceGame())
            {
                return null;
            }

            return TableUtility.TeamAndConferences[HomeTeam];
        }

        public bool IsAmericanConferenceGame()
        {
            var conf = this.ConferenceGameId();
            return conf.HasValue && conf.Value == TableUtility.AmericanId;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PreseasonScheduledGame;

            return other != null && other.Week == this.Week && other.GameNumber == this.GameNumber;
        }

        public override int GetHashCode()
        {
            return (this.Week << 14) | this.GameNumber;
        }

        public bool IsFCSGame()
        {
            return !IsConferenceGame() && AwayTeam.IsFcsTeam();
        }

        public bool IsP5FCSGame()
        {
            return HomeTeam.IsP5() && HomeTeam != 68 && AwayTeam.IsFcsTeam();
        }

        public bool IsG5FCSGame()
        {
            return HomeTeam.IsG5() && HomeTeam != 68 && AwayTeam.IsFcsTeam();
        }

        public bool IsP5Game()
        {
            return !IsConferenceGame() && AwayTeam.IsP5OrND() && HomeTeam.IsP5OrND();
        }

        public bool IsP5GameAnyOpponent()
        {
            return !IsConferenceGame() && (AwayTeam.IsP5() || HomeTeam.IsP5());
        }

        public bool IsRivalryGame()
        {
            return ScheduleFixup.IsRivalryGame(HomeTeam, AwayTeam);
        }

        public bool IsSECNonConferenceGame()
        {
            return (this.HomeTeam.IsSECTeam() || this.AwayTeam.IsSECTeam());
        }

        public bool IsLateSeasonRivalryGame()
        {
            if (!IsRivalryGame())
            {
                return false;
            }

            var rivalry = new Rivalry(HomeTeam, AwayTeam);
            return lateSeasonRivals.Contains(rivalry);
        }

        private static readonly HashSet<Rivalry> lateSeasonRivals = new HashSet<Rivalry>()
        {
            new Rivalry(68,102),
            new Rivalry(68, 87),
            new Rivalry(68, 57),
            new Rivalry(27,28),
            new Rivalry(42,44),
            new Rivalry(21,84),
            new Rivalry(30,31),
            new Rivalry(1,57),
            new Rivalry(1, 8),
            new Rivalry(8,57),
            new Rivalry(18,144),
            new Rivalry(37, 38),
            new Rivalry(20, 50),
            new Rivalry(22, 23),
            new Rivalry(83,89),
            new Rivalry(103, 104),
        };

        public bool IsG5Game()
        {
            return !IsConferenceGame() && !AwayTeam.IsP5OrND() && !HomeTeam.IsP5OrND() && !AwayTeam.IsFcsTeam() && !HomeTeam.IsFcsTeam();
        }

        public bool IsAmericanGame()
        {
            return AwayTeam.IsAmericanTeam() || HomeTeam.IsAmericanTeam();
        }

        public bool IsServiceAcademyGame()
        {
            return ScheduleFixup.IsServiceAcademyGame(HomeTeam, AwayTeam);
        }

        public bool IsAccConfGame()
        {
            return this.HomeTeam.IsAccTeam() && this.AwayTeam.IsAccTeam();
        }

        public bool IsBig12ConfGame()
        {
            return this.HomeTeam.IsBig12Team() && this.AwayTeam.IsBig12Team();
        }

        public bool IsAccOrSecOrBig12Game()
        {
            return this.HomeTeam.IsSECTeam() || this.HomeTeam.IsAccTeam() || this.AwayTeam.IsSECTeam() || this.AwayTeam.IsAccTeam() || this.AwayTeam.IsBig12Team() || this.HomeTeam.IsBig12Team();
        }

        public bool IsAccOrSecConfGame()
        {
            return this.IsSecConfGame() || this.IsAccConfGame();
        }

        public bool IsSecConfGame()
        {
            return this.HomeTeam.IsSECTeam() && this.AwayTeam.IsSECTeam();
        }

        public bool IsSunBeltGame()
        {
            return this.HomeTeam.IsSunBeltTeam() && this.AwayTeam.IsSunBeltTeam();
        }

        public int OpponentId(int teamId)
        {
            if (HomeTeam == teamId)
                return AwayTeam;

            return HomeTeam;
        }

        public string Opponent(int teamId)
        {
            string team = null;
            if (HomeTeam == teamId)
                team = "vs " + RecruitingFixup.TeamAbbreviations[AwayTeam];
            if (AwayTeam == teamId)
                team = "at " + RecruitingFixup.TeamAbbreviations[HomeTeam];

            if (AwayTeam.IsFcsTeam())
            {
                team = team.ToLower();
            }

            if (this.IsG5Game())
            {
                team = team.Replace("at", "G5-at").Replace("vs", "G5-vs");
            }

            // team = string.Format("({0})  {1}", this.WeekIndex, team);

            if (team != null)
                return IsConferenceGame() ? team.ToLower() : team.ToUpper();

            throw new InvalidOperationException();
        }

        public bool IsNotreDameGame()
        {
            return HomeTeam == 68 || AwayTeam == 68;
        }

        public bool IsExtraPac12Game()
        {
            return Pac12Schedule.Pac12ConferenceSchedule != null &&
                Pac12Schedule.Pac12ConferenceSchedule.ContainsKey(this.HomeTeam) &&
                Pac12Schedule.Pac12ConferenceSchedule.ContainsKey(this.AwayTeam) &&
                !Pac12Schedule.Pac12ConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam);
        }

        public bool IsExtraBig12Game()
        {
            return Big12Schedule.Big12ConferenceSchedule != null &&
                Big12Schedule.Big12ConferenceSchedule.ContainsKey(this.HomeTeam) &&
                Big12Schedule.Big12ConferenceSchedule.ContainsKey(this.AwayTeam) &&
                !Big12Schedule.Big12ConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam);
        }

        public bool IsExtraSunbeltGame()
        {
            if (SunBeltSchedule.SunbeltConferenceSchedule != null &&
                SunBeltSchedule.SunbeltConferenceSchedule.ContainsKey(this.HomeTeam) &&
                SunBeltSchedule.SunbeltConferenceSchedule.ContainsKey(this.AwayTeam) &&
                RecruitingFixup.SunBeltTeams == 16)
            {

                // if either team is supposed to host the other, then it's not an extra game!
                return !(SunBeltSchedule.SunbeltConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam) ||
                    SunBeltSchedule.SunbeltConferenceSchedule[this.AwayTeam].Contains(this.HomeTeam));
            }

            return false;
        }

        public bool IsCrossDivisionGame()
        {
            return SunBeltSchedule.CrossDivision(this.HomeTeam, this.AwayTeam);
        }

        public bool IsExtraAccGame()
        {
            return false;
            if (ACCPodSchedule.ACCConferenceSchedule != null &&
                ACCPodSchedule.ACCConferenceSchedule.ContainsKey(this.HomeTeam) &&
                ACCPodSchedule.ACCConferenceSchedule.ContainsKey(this.AwayTeam) &&
                RecruitingFixup.AccTeams == 16)
            {
                return !ACCPodSchedule.ACCConferenceSchedule[this.HomeTeam].Contains(this.AwayTeam);
            }
            else if (RecruitingFixup.AccTeams == 14 &&
                Acc14OpponentDict.ContainsKey(this.HomeTeam) &&
                Acc14OpponentDict.ContainsKey(this.AwayTeam))
            {
                // 14th row of CNFR table for each conference controls 8 or 9 game schedule, but the game fails to schedule properly
                //return false;             
                return !Acc14OpponentDict[this.HomeTeam].Contains(this.AwayTeam);
            }

            return false;
        }

        public bool IsExtraConferenceGame() => IsExtraAccGame() || IsExtraSunbeltGame();

        public bool ShouldFixAccGame()
        {
            if (RecruitingFixup.AccTeams == 14)
            {
                var isEvenYear = Form1.IsEvenYear.Value;

                if (isEvenYear)
                {
                    // it's an even year
                    return !AccEvenYearHosting[this.HomeTeam].Contains(this.AwayTeam);
                }
                else
                {
                    // it's an odd year
                    return AccEvenYearHosting[this.HomeTeam].Contains(this.AwayTeam);
                }
            }
            else
            {
                var isOddYear = !Form1.IsEvenYear.Value;

                if (isOddYear)
                {
                    return !ACCPodSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
                }
                else
                {
                    return ACCPodSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
                }
            }
        }

        public bool ShouldFixSunBeltGame()
        {
#if true
            return false;
#else
            if(RecruitingFixup.SunBeltTeams < 16)
            {
                return false;
            }

            var isOddYear = !Form1.IsEvenYear.Value;

            if (isOddYear)
            {
                return !SunBeltSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
            }
            else
            {
                return SunBeltSchedule.ScenarioForSeason[this.HomeTeam].Contains(this.AwayTeam);
            }
#endif
        }

        public static Dictionary<int, int[]> AccEvenYearHosting = CreateAccDict();

        public static Dictionary<int, int[]> CreateAccDict()
        {
            var dict = new Dictionary<int, int[]>();

#if true
            // atlantic
            dict.Add(47, new[] { 31, 21, 109, 107 });
            dict.Add(21, new[] { 28, 77, 109, 13 });
            dict.Add(28, new[] { 31, 47, 109, 49 });
            dict.Add(31, new[] { 63, 21, 77, 108 });
            dict.Add(63, new[] { 47, 21, 28, 62 });
            dict.Add(77, new[] { 47, 28, 63, 88 });
            dict.Add(109, new[] { 31, 63, 77, 24 });

            // coastal
            dict.Add(108, new[] { 13, 107, 62, 21, 28, 47, 63, 77, 109 });
            dict.Add(13, new[] { 49, 107, 24, 47, 28, 31, 63, 77, 109 });
            dict.Add(49, new[] { 108, 88, 62, 21, 47, 31, 63, 77, 109 });
            dict.Add(107, new[] { 88, 49, 24, 21, 28, 31, 63, 77, 109 });
            dict.Add(62, new[] { 107, 13, 24, 21, 28, 31, 47, 77, 109 });
            dict.Add(88, new[] { 13, 108, 62, 21, 28, 31, 63, 47, 109 });
            dict.Add(24, new[] { 49, 88, 108, 21, 28, 31, 63, 77, 47 });
#else
            // atlantic
            dict.Add(47, new[] { 13, 21, 109, 107 });
            dict.Add(21, new[] { 28, 88, 109, 31 });
            dict.Add(28, new[] { 13, 47, 109, 49 });
            dict.Add(13, new[] { 63, 21, 88, 108 });
            dict.Add(63, new[] { 47, 21, 28, 62 });
            dict.Add(88, new[] { 47, 28, 63, 77 });
            dict.Add(109, new[] { 13, 63, 88, 24 });

            // coastal
            dict.Add(108, new[] { 31, 107, 62, 21, 28, 47, 63, 88, 109 });
            dict.Add(31, new[] { 49, 107, 24, 47, 28, 13, 63, 88, 109 });
            dict.Add(49, new[] { 108, 77, 62, 21, 47, 13, 63, 88, 109 });
            dict.Add(107, new[] { 77, 49, 24, 21, 28, 13, 63, 88, 109 });
            dict.Add(62, new[] { 107, 31, 24, 21, 28, 13, 47, 88, 109 });
            dict.Add(77, new[] { 31, 108, 62, 21, 28, 13, 63, 47, 109 });
            dict.Add(24, new[] { 49, 77, 108, 21, 28, 13, 63, 88, 47 });
#endif
            return dict;
        }

        public static Dictionary<int, HashSet<int>> acc14Dict;
        public static Dictionary<int, HashSet<int>> Acc14OpponentDict
        {
            get
            {
                if (acc14Dict == null)
                {
                    acc14Dict = Acc14TeamCreators[(Form1.DynastyYear - 2201) % Acc14TeamCreators.Length]();
                }

                return acc14Dict;
            }
        }
        public static Func<Dictionary<int, HashSet<int>>>[] Acc14TeamCreators = new Func<Dictionary<int, HashSet<int>>>[] { CreateAccA, CreateAccA, null, null, null, null, null, null, null, null, null, null };


        public static Dictionary<int, HashSet<int>> CreateAccA()
        {
            var dict = new Dictionary<int, HashSet<int>>();
            // atlantic
            var atlantic = new[] { 47, 21, 28, 31, 63, 77, 109 }.ToList();
            var coastal = new[] { 108, 13, 49, 107, 62, 88, 24 }.ToList();
            atlantic.ForEach(team => dict.Add(team, new HashSet<int>(atlantic.Where(t => t != team))));
            coastal.ForEach(team => dict.Add(team, new HashSet<int>(coastal.Where(t => t != team))));

            dict[13].AddTeams(31, 21);
            dict[24].AddTeams(109, 28);
            dict[49].AddTeams(28, 47);
            dict[62].AddTeams(21, 63);
            dict[88].AddTeams(109, 77);
            dict[107].AddTeams(63, 47);
            dict[108].AddTeams(31, 77);

            dict[31].AddTeams(13, 108);
            dict[21].AddTeams(13, 62);
            dict[109].AddTeams(24, 88);
            dict[28].AddTeams(24, 49);
            dict[47].AddTeams(49, 107);
            dict[63].AddTeams(62, 107);
            dict[77].AddTeams(88, 108);

            return dict;
        }

    }

    public static class Ext
    {
        public static void AddTeams(this HashSet<int> set, params int[] teams)
        {
            foreach (var team in teams)
                set.Add(team);
        }
    }
}