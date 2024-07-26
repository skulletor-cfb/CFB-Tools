using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

// set RVPF = 0 
// set STNA = EvenDivsNoProtectedRivalsStyle

namespace EA_DB_Editor
{

    public class FutureGame
    {
        public int AwayId { get; set; }
        public int HomeId { get; set; }
        public string Away { get; set; }
        public string Home { get; set; }

        public int GetOpponent(int teamId)
        {
            if (AwayId == teamId)
                return HomeId;

            return AwayId;
        }

        public static FutureGame FromXml(XElement el)
        {
            var game = new FutureGame();

            var a = el.Element("Away");
            var h = el.Element("Home");

            game.Away = (string)a;
            game.Home = (string)h;

            try
            {
                game.AwayId = (int)a.Attribute("awayId");
                game.HomeId = (int)a.Attribute("homeId");
            }
            catch { }

            if (game.AwayId == 0)
                game.AwayId = RecruitingFixup.TeamNames.Where(kvp => kvp.Value == game.Away).First().Key;

            if( game.HomeId ==0)
                game.HomeId = RecruitingFixup.TeamNames.Where(kvp => kvp.Value == game.Home).First().Key;


            return game; 
        }

        public XElement ToXml()
        {
            if (Away == null)
                Away = RecruitingFixup.TeamNames[AwayId];

            if (Home == null)
                Home = RecruitingFixup.TeamNames[HomeId];

            var el = new XElement("Game",
                new XElement("Away",
                    new XAttribute("awayId", this.AwayId),
                    this.Away),
                new XElement("Home",
                    new XAttribute("homeId", this.HomeId),
                    this.Home)
                    );

            return el;
        }
    }

    public static class ScheduleFixup
    {
        private static bool MatchTeams(int home, int away, int[] matchup)
        {
            return matchup.Contains(home) && matchup.Contains(away);
        }

        public static bool IsServiceAcademyGame(int homeTeam, int awayTeam)
        {
            var academies = new[] { 1, 8, 57 };
            return academies.Contains(homeTeam) && academies.Contains(awayTeam);
        }

        static bool OklahomaSchoolMatchBaylor(int a, int b)
        {
            if (a == 11)
            {
                return b == 71 || b == 72;
            }

            if (b == 11)
            {
                return a == 71 || a == 72;
            }

            return false;
        }

        static bool NUCUMatchKU(int a, int b)
        {
            if (a == 39)
            {
                return b == 58 || b == 22;
            }

            if (b == 39)
            {
                return a == 22 || a == 58;
            }

            return false;
        }

        public static bool IsIndependentBYU(this int byuId)
        {
            return byuId == 16 && RecruitingFixup.TeamAndConferences[byuId] == RecruitingFixup.IndId;
        }

        public static bool RanG5Fixup = false;

        public static (Dictionary<int, TeamSchedule> teamSchedule, MaddenRecord[] scheduleTable) FillSchedule(bool reorderNeedsToBeRun, bool dontShowMessageBox = false)
        {
            var scheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "SCHD").lRecords.Where(r => r["SEYR"].ToInt32() == 0).ToArray();

            var groups = scheduleTable.GroupBy(mr => mr["SEWN"].ToInt32()).ToDictionary(g => g.Key, g => g.ToArray().OrderBy(r => r["GTOD"].ToInt32()).ToArray());
            foreach (var week in groups)
            {
                int gameNum = 0;
                foreach (var g in week.Value)
                {
                    g["SGNM"] = gameNum.ToString();
                    gameNum++;
                }
            }

            Dictionary<int, TeamSchedule> teamSchedule = new Dictionary<int, TeamSchedule>();

            foreach (var mr in scheduleTable.OrderBy(mr => mr["SEWN"].ToInt32()))
            {
                var homeTeam = mr["GHTG"].ToInt32();
                var awayTeam = mr["GATG"].ToInt32();
                var gameNum = mr["SGNM"].ToInt32();
                var weekNum = mr["SEWN"].ToInt32();
                mr["GFFU"] = "1";
                mr["GMFX"] = "0";
                mr["SEWT"] = mr["SEWN"];

                // switch if G5 is hosting P5
                if (!RanG5Fixup && (awayTeam.IsP5() && !homeTeam.IsP5() && !homeTeam.IsServiceAcademy() && !IsRivalryGame(homeTeam, awayTeam) && !homeTeam.IsIndependentBYU()) ||
                    homeTeam.IsFcsTeam())
                {
                    var g5 = homeTeam;
                    var p5 = awayTeam;
                    homeTeam = p5;
                    awayTeam = g5;
                    mr["GATG"] = g5.ToString();
                    mr["GHTG"] = p5.ToString();
                }

                // if the stadium id is not zero, make sure it is correct
                if (mr["SGID"].ToInt32() != 0)
                {
                    mr["SGID"] = TeamStadiums.Value[mr["GHTG"].ToInt32()];
                }


                var game = new PreseasonScheduledGame
                {
                    HomeTeam = homeTeam,
                    AwayTeam = awayTeam,
                    GameNumber = gameNum,
                    WeekIndex = weekNum,
                    MaddenRecord = mr
                };

                // if this is an ACC Game we need to have our special ACC schedule
                if (game.IsConferenceGame() && RecruitingFixup.TeamAndConferences[homeTeam] == RecruitingFixup.ACCId && game.ShouldFixAccGame())
                {
                    game.SwapHomeAwayTeam(mr);
                }
                else if (false && game.IsConferenceGame() && RecruitingFixup.TeamAndConferences[homeTeam] == RecruitingFixup.AmericanId && game.ShouldFixSunBeltGame())
                {
                    game.SwapHomeAwayTeam(mr);
                }
                else if (false && game.IsConferenceGame() && RecruitingFixup.TeamAndConferences[homeTeam] == RecruitingFixup.Big12Id)
                {
                    // FINDME : BIG 12 HOME/AWAY FIXES
                    bool doSwitch = false;
                    // in even years OK ST @ OU , OU @ NEB , NEB @ CU
                    // in odd years OU @ OK ST , NEB @ OU , CU @ NEB                    
                    if (Form1.IsEvenYear.Value)
                    {
                        if (homeTeam == 72 && awayTeam == 71)
                        {
                            doSwitch = true;
                        }
                        else if (homeTeam == 71 && awayTeam == 58)
                        {
                            //   doSwitch = true;
                        }
                        else if (homeTeam == 58 && awayTeam == 22)
                        {
                            doSwitch = true;
                        }
                        else if ((homeTeam == 71 && awayTeam == 11) ||
                            (homeTeam == 11 && awayTeam == 72) ||
                            (homeTeam == 39 && awayTeam == 58) ||
                            (homeTeam == 22 && awayTeam == 39))
                        {
                            doSwitch = true;
                        }
                    }
                    else
                    {
                        if (awayTeam == 72 && homeTeam == 71)
                        {
                            doSwitch = true;
                        }
                        else if (awayTeam == 71 && homeTeam == 58)
                        {
                            //  doSwitch = true;
                        }
                        else if (awayTeam == 58 && homeTeam == 22)
                        {
                            doSwitch = true;
                        }
                        else if ((awayTeam == 71 && homeTeam == 11) ||
                            (awayTeam == 11 && homeTeam == 72) ||
                            (awayTeam == 39 && homeTeam == 58) ||
                            (awayTeam == 22 && homeTeam == 39))
                        {
                            doSwitch = true;
                        }
                    }

                    if (doSwitch)
                    {
                        var realHomeTeam = game.AwayTeam;
                        game.AwayTeam = game.HomeTeam;
                        game.HomeTeam = realHomeTeam;
                        mr["GATG"] = game.AwayTeam.ToString();
                        mr["GHTG"] = game.HomeTeam.ToString();
                    }
                }

                var newGameNum = ConfScheduleFixer.GetGameNumber();

                // home team
                if (teamSchedule.TryGetValue(homeTeam, out var homeSchedule) == false)
                {
                    homeSchedule = new TeamSchedule();
                    teamSchedule[homeTeam] = homeSchedule;
                }

                // away team
                if (teamSchedule.TryGetValue(awayTeam, out var awaySchedule) == false)
                {
                    awaySchedule = new TeamSchedule();
                    teamSchedule[awayTeam] = awaySchedule;
                }

                // we can't put them here, drop it to week 14
                if (homeSchedule[weekNum] != null || awaySchedule[weekNum] != null)
                {
                    if (!dontShowMessageBox)
                    {
                        MessageBox.Show(game.ToString());
                    }

                    homeSchedule.AddUnscheduledGame(game, newGameNum);

                    if (!awayTeam.IsFcsTeam())
                    {
                        awaySchedule.AddUnscheduledGame(game, newGameNum);
                    }
                }
                else
                {

                    homeSchedule[weekNum] = game;

                    if (!awayTeam.IsFcsTeam())
                    {
                        // don't track away games for fcs
                        awaySchedule[weekNum] = game;
                    }
                }


                if (reorderNeedsToBeRun)
                {
                    mr["GDAT"] = "5";
                }
            }

            return (teamSchedule, scheduleTable);
        }

        public static Dictionary<int, TeamSchedule> ReadSchedule(bool runG5Fix = false)
        {
            ACCPodSchedule.Init();
            Big12Schedule.Init();
            Pac12Schedule.Init();
            AmericanSchedule.Init();
            MWCSchedule.Init();
            MACSchedule.Init();
            Big10Schedule.Init();
            CUSASchedule.Init();
            SunBeltSchedule.Init();

            var (teamSchedule, scheduleTable) = FillSchedule(!RanReorder);

            // fcs-fcs games need to find teams to set them to
            ConfScheduleFixer.ReplaceFcsOnlyGames(teamSchedule);

            if (!RanReorder)
            {
                // try to put non conference games earlier in the season
                (teamSchedule, scheduleTable) = FillSchedule(false, true);
                ConfScheduleFixer.MoveNonConfGamesEarly(teamSchedule);

                /*
                (teamSchedule, scheduleTable) = FillSchedule(false, true);
                ConfScheduleFixer.MoveNonConfGamesEarly(teamSchedule);
                ConfScheduleFixer.FcsGamesEarly(teamSchedule);*/

                (teamSchedule, scheduleTable) = FillSchedule(false, true);
                ACCPodSchedule.ProcessACCSchedule(teamSchedule);
                Big12Schedule.ProcessBig12Schedule(teamSchedule);
                Pac12Schedule.ProcessPac12Schedule(teamSchedule);
                Big10Schedule.ProcessBig10Schedule(teamSchedule);
                AmericanSchedule.ProcessAmericanSchedule(teamSchedule);
                MWCSchedule.ProcessMWCSchedule(teamSchedule);
                MACSchedule.ProcessMACSchedule(teamSchedule);
                CUSASchedule.ProcessCUSASchedule(teamSchedule);
                SunBeltSchedule.ProcessSunbeltSchedule(teamSchedule);

                (teamSchedule, scheduleTable) = FillSchedule(false, true);
                ConfScheduleFixer.MoveNonConfGamesEarly(teamSchedule);

                //ConfScheduleFixer.G5FCSSwap(teamSchedule);
                //(teamSchedule, scheduleTable) = FillSchedule(false, true);


                // move aerlier in the year to ensure more chance of replacement
                // ConfScheduleFixer.MoveReplaceableGames(teamSchedule, g => true);

                /*
                // do power conf first
                ConfScheduleFixer.ExtraConfGameSwap(teamSchedule, g => true, g => !g.IsAmericanGame());

                // now do american
                ConfScheduleFixer.ExtraConfGameSwap(teamSchedule, g => !g.IsAmericanGame(), g => g.IsAmericanGame());

                // now cross cut 
                ConfScheduleFixer.ExtraConfGameSwap(teamSchedule);
                */

                //ConfScheduleFixer.G5FCSSwap(teamSchedule);
                (teamSchedule, scheduleTable) = FillSchedule(false, true);
                ConfScheduleFixer.SecFix(teamSchedule);
                ConfScheduleFixer.AccFix(teamSchedule);
                ConfScheduleFixer.Big10Fix(teamSchedule);
                ConfScheduleFixer.Big12Fix(teamSchedule);
                ConfScheduleFixer.Pac12Fix(teamSchedule);
                ConfScheduleFixer.AmericanFix(teamSchedule);
                ConfScheduleFixer.SunBeltFix(teamSchedule);
                ConfScheduleFixer.CUSAFix(teamSchedule);
                ConfScheduleFixer.MWCFix(teamSchedule);
                ConfScheduleFixer.MACFix(teamSchedule);

                (teamSchedule, scheduleTable) = FillSchedule(false, true);

                /*
                // move aerlier in the year to ensure more chance of replacement
                for (int i = 0; i < 5; i++)
                {
                    ConfScheduleFixer.MoveReplaceableGames(teamSchedule, g => true);
                    (teamSchedule, scheduleTable) = FillSchedule(false, true);
                }*/

                // try to put non conference games earlier in the season
                //ConfScheduleFixer.MoveNonConfGamesEarly(teamSchedule, 4);
                //(teamSchedule, scheduleTable) = FillSchedule(false, true);

                ConfScheduleFixer.SwapG5ForP5HomeTeam(teamSchedule);
                (teamSchedule, scheduleTable) = FillSchedule(false, true);

                //ConfScheduleFixer.MoveNonConfGamesEarly(teamSchedule, 4);
                //(teamSchedule, scheduleTable) = FillSchedule(false, true);

                ConfScheduleFixer.FcsGamesEarly(teamSchedule);
                (teamSchedule, scheduleTable) = FillSchedule(false, true);
            }

            RanReorder = true;
            bool fix = runG5Fix;
            var sb = new StringBuilder();
            FCSGames = 0;
            var G5Matchups = 0;
            var P5Matchups = 0;
            var P5vsG5 = 0;
            var allGames = scheduleTable.Length;

            for (int j = 0; j < 5; j++)
            {
                List<int> needMorePower5GAmes = new List<int>();
                List<int> needMoreG5Games = new List<int>();

                if (j == 4) fix = false;

                sb = new StringBuilder();
                sb.AppendLine("Name,,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,,Notes");
                sb.AppendLine(",,,,,,,,,,,,,,,,,,");


                foreach (var tsch in teamSchedule.OrderBy(kvp => RecruitingFixup.TeamAndConferences[kvp.Key]).ThenBy(kvp => RecruitingFixup.TeamNames[kvp.Key]))
                {
                    var prefix = string.Empty;

                    if (tsch.Value[0] == null)
                    {
                        if (tsch.Value[1] != null)
                        {
                            if (teamSchedule.TryGetValue(tsch.Value[1].OpponentId(tsch.Key), out var oppSch) && oppSch[0] == null)
                            {
                                prefix = "R: ";
                            }
                        }
                    }

                    if (tsch.Key.IsFcsTeam())
                        continue;

                    string notes = string.Empty;

                    var homeGames = tsch.Value.Where(g => g != null && g.HomeTeam == tsch.Key).Count();
                    var ooc = tsch.Value.Where(g => g != null && !g.IsConferenceGame()).ToArray();
                    var p5Opp = ooc.Count(g => g.IsP5Game());
                    var fcsOpp = ooc.Count(g => g.AwayTeam.IsFcsTeam());
                    var g5Opp = ooc.Count(g => g.IsG5Game());
                    FCSGames += fcsOpp;

                    if (tsch.Key.IsP5())
                    {
                        P5Matchups += p5Opp;
                        P5vsG5 += (ooc.Length - p5Opp -fcsOpp);
                    }

                    if (tsch.Key.IsG5())
                    {
                        G5Matchups += g5Opp;
                        //P5vsG5 += (ooc.Length - g5Opp);
                    }

                    if(homeGames < 5)
                    {
                        //notes += $"{homeGames} home games.  ";
                    }

                    if (tsch.Value.Count(g => g != null) != 12)
                        notes += "Wrong game count.  ";

                    if (p5Opp == 0 && tsch.Key.IsP5())
                    {
                        notes += "No P5 Opponents.  ";
                    }
                    else if (p5Opp > 1 && tsch.Key.IsP5())
                    {
                        notes += p5Opp + " P5 Opponents.  ";
                    }
                    else if (g5Opp > 0 && tsch.Key.IsP5() == false)
                    {
                        // notes += g5Opp + " G5 Opponents.  ";
                    }

                    if (ooc.Length != ooc.Select(g => g.OpponentId(tsch.Key)).Distinct().Count())
                    {
                        if (ooc.Count(g => g.OpponentId(tsch.Key).IsFcsTeam()) <= 1)
                        {
                            notes += "Duplicate opponents.   ";
                        }
                    }

                    if (tsch.Key.TooManyFcsGameCheck(fcsOpp))
                    {
                        notes += "Multiple FCS opponents.  ";
                    }
                    else if(fcsOpp==0 && tsch.Key.IsP5())
                    {
                        notes += "No FCS.  ";
                    }

                    if (fcsOpp >= 1 && tsch.Key.IsG5() && RecruitingFixup.TeamAndConferences[tsch.Key] != RecruitingFixup.AmericanId && !RanG5Fixup)
                    {
                        notes += "G5-FCS MATCHUP.  ";
                    }

                    if (!tsch.Key.ConferenceGameCountCheck(tsch.Value.Count(g => g != null && g.IsConferenceGame())))
                    {
                        notes += "Wrong conference game count ";
                    }

                    if (!tsch.Value.ConferenceHomeGameCount(tsch.Key))
                    {
                        notes += "Wrong conference home game count ";
                    }

                    for (int i = 0; i < tsch.Value.Length; i++)
                    {
                        if (tsch.Value[i] != null && tsch.Value[i].MustReplace)
                        {
                            notes += string.Format("Must Replace week={0} ", i);
                        }
                    }

                    // let AAC be
                    var p5OppForG5 = ooc.Count(g => g.IsP5GameAnyOpponent());

                    if ((p5OppForG5 > ooc.Length.OutOfConferenceG5GamesGoal()) && tsch.Key.IsG5())
                    {
                        notes += string.Format(",G5 with {0} P5 Opponents.  ", p5OppForG5);
                        needMoreG5Games.Add(tsch.Key);
                    }
                    else if ((p5OppForG5 < ooc.Length.OutOfConferenceG5GamesGoal()) && tsch.Key.IsG5())
                    {
                        if (tsch.Key.IsIndependentG5())
                        {
                            //     needMorePower5GAmes.Add(tsch.Key);
                        }

                        notes += string.Format(",G5 with not enough {0} P5 Opponents.  ", p5OppForG5);

                        if (!tsch.Key.IsAmericanTeam())
                        {
                            needMorePower5GAmes.Add(tsch.Key);
                        }
                    }
                    else
                    {
                        notes += ",";
                    }

                    sb.AppendLine(string.Format("{0},,{1},,{2}", RecruitingFixup.TeamNames[tsch.Key], string.Join(",", tsch.Value.Take(TeamSchedule.ScheduleLimit).Select((ts, idx) => ts == null ? string.Empty : string.Format("{0}{1}", idx == 1 ? prefix : string.Empty, ts.Opponent(tsch.Key)))), notes));
                }

                needMoreG5Games = Randomize(needMoreG5Games);

                if (fix)
                {
                    // try to trade teams with too many p5 with not enough
                    foreach (var needP5 in needMorePower5GAmes)
                    {
                        var tsch = teamSchedule[needP5];
                        var ooc = tsch.Where(g => g != null && !g.IsConferenceGame() && g.IsG5Game() && !g.IsRivalryGame()).ToArray();
                        var p5OppForG5 = ooc.Count(g => g.IsP5GameAnyOpponent());
                        AttemptFix(needP5, teamSchedule, ooc, needMoreG5Games, ooc.Length.OutOfConferenceG5GamesGoal() - p5OppForG5);
                    }
                }

                if (!fix)
                    break;
            }

            sb.AppendLine(FCSGames + " FCS GAMES");
            sb.AppendLine(G5Matchups / 2 + " Group 5 GAMES");
            sb.AppendLine(P5Matchups / 2 + " Power 5 GAMES");
            sb.AppendLine(P5vsG5 + " P5 vs G5 GAMES");
            sb.Append(allGames + " total games");

            try
            {
                File.WriteAllText(@"schedule.csv", sb.ToString());
            }
            catch { }

            /*
            var groups = scheduleTable.GroupBy(mr => mr["SEWN"].ToInt32()).ToDictionary(g => g.Key, g => g.ToArray().OrderBy(r => r["GTOD"].ToInt32()).ToArray());
            foreach (var week in groups)
            {
                int gameNum = 0;
                foreach (var g in week.Value)
                {
                    g["SGNM"] = gameNum.ToString();
                    gameNum++;
                }
            }
            */
                                   
            return teamSchedule;
        }


        static int OutOfConferenceG5GamesGoal(this int oocGames)
        {
            return 2;
            if (oocGames >= 6)
            {
                return 2;
            }
            else if (oocGames == 3)
            {
                return 2;
            }

            // goal is to not exceed 10 G5 games
            return oocGames / 2;
        }
    

        static List<int> Randomize(List<int> list)
        {
            Random rand = new Random();
            var arr = new int[list.Count];
            for (int i = 0; i < arr.Length; i++)
            {
                var idx = rand.Next(0, list.Count);
                arr[i] = list[idx];
                list.RemoveAt(idx);
            }

            return new List<int>(arr);
        }

        static void AttemptFix(int teamId, Dictionary<int, TeamSchedule> teamSchedule, PreseasonScheduledGame[] ooc, List<int> potentialFixees, int gamesNeeded)
        {
            if (ooc.Any(g =>  RecruitingFixup.OnTheirOwn.Contains(g.HomeTeam) || RecruitingFixup.OnTheirOwn.Contains(g.AwayTeam))) return;

            foreach (var potentialMatch in potentialFixees)
            {
                // get the OOC games where the opponents are not in my conference
                var matchOOC = teamSchedule[potentialMatch].Where(g => g != null && !g.IsConferenceGame()).ToArray();
                var p5Count = matchOOC.Count(g => g.IsP5GameAnyOpponent());
                var p5ToGive = p5Count - matchOOC.Length.OutOfConferenceG5GamesGoal();
                matchOOC = teamSchedule[potentialMatch].Where(g => g != null && !g.IsConferenceGame() && !g.IsRivalryGame() && g.IsP5GameAnyOpponent() && !InTheSameConference(g.OpponentId(potentialMatch), teamId)).ToArray();

                // see if we intersect weeks
                var intersect = matchOOC.Where(g => ooc.Select(ocg => ocg.Week).Contains(g.Week)).ToArray();

                if (intersect.Length > 0)
                {
                    var g1 = intersect.OrderBy( g => g.Week).First();
                    var g2 = ooc.Where(g => g.Week == g1.Week).First();
                    if (SwapForP5(g1, g2, teamId))
                    {
                        teamSchedule[g1.HomeTeam][g1.WeekIndex] = g1;
                        teamSchedule[g2.HomeTeam][g2.WeekIndex] = g2;

                        if (!g1.AwayTeam.IsFcsTeam())
                            teamSchedule[g1.AwayTeam][g1.WeekIndex] = g1;

                        if (!g2.AwayTeam.IsFcsTeam())
                            teamSchedule[g2.AwayTeam][g2.WeekIndex] = g2;

                        p5ToGive--;

                        if (p5ToGive == 0 && !potentialMatch.IsIndependentG5())
                        {
                            potentialFixees.Remove(potentialMatch);
                        }

                        return;
                    }
                }
            }
        }

        static bool InTheSameConference(int a, int b)
        {
            return RecruitingFixup.TeamAndConferences[a] == RecruitingFixup.TeamAndConferences[b];
        }

        static int FCSGames = 0;

        private static bool? _ranReorder = null;
        private static bool RanReorder
        {
            get
            {
                if (_ranReorder.HasValue == false)
                {
                    var result = MessageBox.Show("Did you already run preseason schedule fix?", "", MessageBoxButtons.YesNo);
                    _ranReorder = result == DialogResult.Yes;
                }

                return _ranReorder.Value;
            }
            set
            {
                _ranReorder = value;
            }
        }

        public static List<int> FindUserControllerTeams(bool remove)
        {
            try
            {
                var sttm = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "STTM");

                if(sttm == null)
                {
                    return new List<int>();
                }

                var list =  sttm.lRecords.Where(mr => mr["CFUC"].ToInt32() == 1).Select(mr => mr["TGID"].ToInt32()).ToList();

                if (remove)
                {
                    list.Remove(8);
                    list.Remove(57);
                    list.Remove(1);
                }
                return list;
            }
            catch { }

            return new List<int>();
        }

        public static void FixPreseasonSchedule()
        {
            bool G5GameThatCanBeReplace(PreseasonScheduledGame game)
            {
                return game.IsG5Game() || game.IsExtraConferenceGame();
            }

            bool NotConferenceGame(PreseasonScheduledGame game)
            {
                return !game.IsConferenceGame() || game.IsExtraConferenceGame(); 
            }

            // Pitt-WVU , OU-NEB are week 14
            var schedules = ReadSchedule();

            var gamesThatCanBeReplaced = 
                schedules
                .Where(kvp => kvp.Value.Where(v => !kvp.Key.IsP5OrND() && v != null)
                .Select(v => G5GameThatCanBeReplace(v)).Count() > 0)
                .SelectMany(kvp => kvp.Value.Where(v => v != null && G5GameThatCanBeReplace(v) && !v.IsServiceAcademyGame() && NotConferenceGame(v) && !v.IsRivalryGame()))
                .OrderBy(g => g.Week).Distinct().ToList();

            var teams = schedules.Keys.Where(k => k.IsP5());

            // find user controlled teams and lock them
            var ucTeams = FindUserControllerTeams(false);
            ucTeams.ForEach(t => lockedTeamSchedules.Add(t));
            var futureGames = new List<FutureGame>();


            foreach (var team in teams.Where(t => !lockedTeamSchedules.Contains(t)).OrderByDescending(t => schedules[t].Count(g => g != null && g.IsP5Game())))
            {
                schedules = ReadSchedule();
                gamesThatCanBeReplaced = schedules
                    .Where(kvp => kvp.Value.Where(v => !kvp.Key.IsP5OrND() && v != null)
                    .Select(v => G5GameThatCanBeReplace(v)).Count() > 0)
                    .SelectMany(kvp => kvp.Value.Where(v => v != null && G5GameThatCanBeReplace(v) && !v.IsServiceAcademyGame() && NotConferenceGame(v) && !v.IsRivalryGame()))
                    .OrderBy(g => g.Week).Distinct().ToList();
                var gamesToAdd = CleanSchedule(team, schedules, new List<FutureGame>(), gamesThatCanBeReplaced);
                futureGames.AddRange(gamesToAdd);
            }

            schedules = ReadSchedule();

            // now we need to look into teams with > 2 P5 OOC and 0 OOC
            var moreThan2P5OOC = teams.Where(t => schedules[t].Count(g => g != null && !g.IsConferenceGame() && g.IsP5Game()) >= 2).ToList();
            var noP5OOC = teams.Where(t => schedules[t].Count(g => g != null && !g.IsConferenceGame() && g.IsP5Game()) == 0).ToArray();
            var eligbleGames = schedules.Where(kvp => moreThan2P5OOC.Contains(kvp.Key)).SelectMany(kvp => kvp.Value.Where(g => g != null && g.IsP5Game() && !g.IsNotreDameGame() && !g.IsConferenceGame() && !g.IsRivalryGame())).ToList();

            foreach (var team in noP5OOC)
            {
                foreach (var game in schedules[team].Where(g => g != null && !g.IsConferenceGame() && !g.IsRivalryGame()))
                {
                    var potentialSwap = eligbleGames.Pop(game);

                    if (potentialSwap == null)
                        continue;

                    if (ucTeams.Contains(potentialSwap.HomeTeam) || ucTeams.Contains(potentialSwap.AwayTeam))
                        continue;

                    var home = potentialSwap.HomeTeam;
                    var away = potentialSwap.AwayTeam;
                    if (SwapTeams(game, potentialSwap, moreThan2P5OOC))
                    {
                        // was it a future game?
                        var futureGameToRemove = futureGames.Where(fg => fg.HomeId == away && fg.AwayId == home).FirstOrDefault();

                        // remove the future game
                        if (futureGameToRemove != null)
                        {
                            futureGames.Remove(futureGameToRemove);
                        }
                        else
                        {
                            // put it back on
                            futureGames.Insert(0, new FutureGame { AwayId = away, HomeId = home });
                        }

                        // put a Future Game on
                        var gameToAdd = new[] { game, potentialSwap }.First(g => g.IsP5Game());
                        futureGames.Insert(0, new FutureGame { AwayId = gameToAdd.HomeTeam, HomeId = gameToAdd.AwayTeam });

                        break;
                    }
                }
            }

            // now teams with no FCS games
            var noFCS = teams.Where(t => t != 68 && t.IsP5() && schedules[t].Count(g => g != null && !g.IsConferenceGame() && g.IsFCSGame()) == 0).ToList();
            eligbleGames = schedules.Where(kvp =>!kvp.Key.IsFcsTeam()&& !noFCS.Contains(kvp.Key)).SelectMany(kvp => kvp.Value.Where(g => g != null && !g.IsP5FCSGame()&& g.IsFCSGame()&& !g.IsNotreDameGame())).ToList();
            foreach (var team in noFCS)
            {
                foreach (var game in schedules[team].Where(g => g != null && !g.IsConferenceGame() && !g.IsRivalryGame() && !g.IsP5Game()))
                {
                    var potentialSwap = eligbleGames.Pop(game);

                    if (potentialSwap == null)
                        continue;

                    var home = potentialSwap.HomeTeam;
                    var away = potentialSwap.AwayTeam;
                    if (SwapTeams(game, potentialSwap))
                    {
                        break;
                    }
                }
            }

            schedules = ReadSchedule();

            /*
            ConfScheduleFixer.AccFix(schedules);
            ConfScheduleFixer.Big10Fix(schedules);
            ConfScheduleFixer.Big12Fix(schedules);
            schedules = ReadSchedule();
            */

            MessageBox.Show("Scheduling finished");
        }

        static HashSet<int> lockedTeamSchedules = new HashSet<int>();
        static HashSet<int> lockedGames = new HashSet<int>();

        static void LockGame(int awayId, int homeId)
        {
            var n = new[] { awayId, homeId }.OrderBy(i => i).ToArray();
            lockedGames.Add((n[0] << 16) | n[1]);
        }

        static bool IsGameLocked(int awayId, int homeId)
        {
            var n = new[] { awayId, homeId }.OrderBy(i => i).ToArray();
            var hash = (n[0] << 16) | n[1];
            return lockedTeamSchedules.Contains(awayId)||lockedTeamSchedules.Contains(homeId)|| lockedGames.Contains(hash);
        }

        static List<FutureGame> CleanSchedule(int team, Dictionary<int, TeamSchedule> schedules, List<FutureGame> futureGames, List<PreseasonScheduledGame> replacements)
        {
            // give me all my OOC games
            var schedule = schedules[team];
            var games = schedule.Where(g => g != null && !g.IsConferenceGame()).ToArray();
            var name = RecruitingFixup.TeamNames[team];

            // games to playback
            var futureScheduledGames = futureGames.Where(g => g.AwayId == team || g.HomeId == team).ToArray();
            var gamesToAdd = new List<FutureGame>();

            var p5GamesAllowed = FindGamesAllowed(games);

            // try to schedule our future game
            foreach (var futureScheduledGame in futureScheduledGames)
            {
                if (p5GamesAllowed == 0)
                    break; 

                foreach (var game in games.Where(g => !IsGameLocked(g.AwayTeam, g.HomeTeam)))
                {
                    var opp = futureScheduledGame.GetOpponent(team);

                    // schedule is not locked
                    if (lockedTeamSchedules.Contains(opp) == false)
                    {
                        // we have slots to fill
                        var oppSchedule = schedules[opp].Where(g => g != null && !g.IsConferenceGame()).ToArray();

                        // opponent has slots to give 
                        if (oppSchedule.Count(os => IsGameLocked(os.AwayTeam, os.HomeTeam)) == 0)
                        {
                            // we have a matching week
                            var oppGame = oppSchedule.Where(g => g.WeekIndex == game.WeekIndex).FirstOrDefault();

                            if (oppGame != null && !IsGameLocked(oppGame.AwayTeam, oppGame.HomeTeam) && !game.Equals(oppGame))
                            {
                                // swap teams, find the new game with the team, and Lock it
                                if (SwapTeams(game, oppGame, futureScheduledGame))
                                {
                                    var newGame = new[] { game, oppGame }.Where(g => g.AwayTeam == team || g.HomeTeam == team).Single();
                                    SetupGame(newGame, futureScheduledGame);
                                    LockGame(newGame.AwayTeam, newGame.HomeTeam);
                                    futureGames.Remove(futureScheduledGame);
                                    p5GamesAllowed--;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // clean up just that extrac acc game
            foreach (var extraAcc in schedule.Where(g => g != null && (g.IsExtraConferenceGame() || g.MustReplace)))
            {
                var replacement = replacements.Pop(extraAcc, true);

                if (replacement != null)
                {
                    SwapTeams(extraAcc, replacement);
                }
            }

            // finally these are the games I need to replace
            foreach (var oocGame in games.Where(g => !IsGameLocked(g.AwayTeam,g.HomeTeam) ))
            {
                if (p5GamesAllowed > 0 && oocGame.IsP5Game())
                {
                    p5GamesAllowed--;
                    gamesToAdd.Add(new FutureGame
                    {
                        HomeId = oocGame.AwayTeam,
                        AwayId = oocGame.HomeTeam,
                        Away = RecruitingFixup.TeamNames[oocGame.HomeTeam],
                        Home = RecruitingFixup.TeamNames[oocGame.AwayTeam],
                    });

                    LockGame(oocGame.AwayTeam, oocGame.HomeTeam);
                }
                else if( oocGame.IsP5Game())
                {
                    var replacement = replacements.Pop(oocGame, true);

                    if (replacement != null)
                    {
                        SwapTeams(oocGame, replacement);
                    }
                    else
                    {
                        p5GamesAllowed--;

                        gamesToAdd.Add(new FutureGame
                        {
                            HomeId = oocGame.AwayTeam,
                            AwayId = oocGame.HomeTeam,
                            Away = RecruitingFixup.TeamNames[oocGame.HomeTeam],
                            Home = RecruitingFixup.TeamNames[oocGame.AwayTeam],
                        });

                        LockGame(oocGame.AwayTeam, oocGame.HomeTeam);
                    }
                }
            }

            if (p5GamesAllowed <= 0)
            {
                lockedTeamSchedules.Add(team);
            }

            return gamesToAdd;
        }

        static void SetupGame(PreseasonScheduledGame game, FutureGame futureGame)
        {
            // we have it right
            if (game.AwayTeam == futureGame.AwayId)
            {
                return;
            }

            // otherwise, swap teams
            var realHome = futureGame.HomeId;
            game.MaddenRecord["GATG"] = futureGame.AwayId.ToString();
            game.MaddenRecord["GHTG"] = futureGame.HomeId.ToString();
            game.HomeTeam = futureGame.HomeId;
            game.AwayTeam = futureGame.AwayId;
        }

        static int FindGamesAllowed(PreseasonScheduledGame[] games)
        {
            // number of P5 games are ND/Rivalry
            var rivalOrND = games.Where(g => g.IsNotreDameGame() || g.IsRivalryGame() ).ToArray();

            // Lock rivalry games
            games.Where(g => g.IsRivalryGame()).ToList().ForEach(g => LockGame(g.AwayTeam, g.HomeTeam));

            // number of P5 games are not ND/Rival
            var ooc = games.Where(g => !g.IsNotreDameGame() && !g.IsRivalryGame() && !IsGameLocked(g.AwayTeam, g.HomeTeam)).ToArray();

            // we typically allow just 1 P5 game, but if we have a rival or are playing ND, allow a 2nd one
            int p5GamesAllowed = 1;

            switch (rivalOrND.Length)
            {
                case 4:
                case 3:
                case 2:
                    p5GamesAllowed = 0;
                    break;
                case 1:
                case 0:
                    p5GamesAllowed = 1;
                    break;
                default:
                    break;
            }

            p5GamesAllowed -= games.Count(g => !g.IsRivalryGame() && IsGameLocked(g.AwayTeam, g.HomeTeam));
            return Math.Max(p5GamesAllowed, 0);
        }

#if false
        static void CleanSchedule(Dictionary<int, TeamSchedule> schedules)
        {
            // get all OOC games needed to be replaced, skip the first one
            var gamesInNeedOfReplacement = schedules.Where(kvp => kvp.Value.Where(v => v != null).Select(v => v.IsP5Game()).Count() > 1).SelectMany(kvp => kvp.Value.Where(v => v != null && !v.IsRivalryGame() && v.IsP5Game() && !v.IsConferenceGame() && !v.IsNotreDameGame()).Skip(1)).OrderBy(g => g.Week).Distinct().ToList();
            var gamesThatCanBeReplaced = schedules.Where(kvp => kvp.Value.Where(v => !kvp.Key.IsP5OrND() && v != null).Select(v => v.IsG5Game()).Count() > 0).SelectMany(kvp => kvp.Value.Where(v => v != null && v.IsG5Game() && !v.IsConferenceGame())).OrderBy(g => g.Week).Distinct().ToList();
            //var p5NoOOC = schedules.Where(kvp => kvp.Key.IsP5() && kvp.Value.Count(g => g != null && g.IsP5Game()) == 0).SelectMany(kvp => kvp.Value.Where(sg => sg != null && !sg.IsConferenceGame())).Distinct().ToList();
            //gamesThatCanBeReplaced.InsertRange(0, p5NoOOC);

            foreach (var game in gamesInNeedOfReplacement)
            {
                if (IsRivalryGame(game.HomeTeam, game.AwayTeam))
                {
                    continue;
                }

                var swap = gamesThatCanBeReplaced.Pop(game);

                if (swap == null)
                    continue;

                if (!BothTeamsNeedReplacement(schedules, game))
                {
                    gamesThatCanBeReplaced.Insert(0, swap);
                    continue;
                }

                SwapTeams(schedules, game, swap);
            }
        }
#endif

        static bool SwapTeams(PreseasonScheduledGame g1, PreseasonScheduledGame g2, List<int> teamsThatNeedG5)
        {
            // likely 3 P5 teams, 1 G5
            var teams = new[] { g2.HomeTeam, g1.HomeTeam, g2.AwayTeam, g1.AwayTeam }.ToList();
            var g5 = teams.First(t => !t.IsP5OrND());
            var p5ForG5 = teams.First(t => teamsThatNeedG5.Contains(t));
            teams.Remove(p5ForG5);
            teams.Remove(g5);

            if (RecruitingFixup.TeamsInSameConference(teams.First(), teams.Last()))
                return false;

            g1.MaddenRecord["GHTG"] = p5ForG5.ToString();
            g1.MaddenRecord["GATG"] = g5.ToString();
            g1.HomeTeam = p5ForG5;
            g1.AwayTeam = g5;

            var a = teams.First();
            var h = teams.Last();
            g2.MaddenRecord["GHTG"] = h.ToString();
            g2.MaddenRecord["GATG"] = a.ToString();
            g2.HomeTeam = h;
            g2.AwayTeam = a;
            return true; 
        }

    public    static bool SwapTeams(PreseasonScheduledGame g1, PreseasonScheduledGame g2)
        {
            if (RecruitingFixup.TeamsInSameConference(g1.HomeTeam, g2.AwayTeam) || RecruitingFixup.TeamsInSameConference(g2.HomeTeam, g1.AwayTeam))
                return false;

            var a1 = g1.AwayTeam;
            var a2 = g2.AwayTeam;
            g1.MaddenRecord["GATG"] = a2.ToString();
            g2.MaddenRecord["GATG"] = a1.ToString();
            g1.AwayTeam = a2;
            g2.AwayTeam = a1;
            return true; 
        }

        public static bool SwapForP5(PreseasonScheduledGame g1, PreseasonScheduledGame g2, int teamInNeedOfP5)
        {
            PreseasonScheduledGame allG5 = null;
            PreseasonScheduledGame g5p5 = null;
            int g5ToG5 = 0;
            int p5 = 0;
            int g5ToG5Two = 0;

            if (g1.IsG5Game())
            {
                allG5 = g1;
                g5p5 = g2;
            }
            else
            {
                allG5 = g2;
                g5p5 = g1;
            }

            if (allG5.AwayTeam == teamInNeedOfP5)
            {
                g5ToG5 = allG5.HomeTeam;
            }
            else
            {
                g5ToG5 = allG5.AwayTeam;
            }

            if (g5p5.HomeTeam.IsP5())
            {
                p5 = g5p5.HomeTeam;
                g5ToG5Two = g5p5.AwayTeam;
            }
            else
            {
                p5 = g5p5.AwayTeam;
                g5ToG5Two = g5p5.HomeTeam;
            }

            if (InTheSameConference(g5ToG5, g5ToG5Two))
                return false;

            var game = g5p5;
            game.MaddenRecord["GATG"] = teamInNeedOfP5.ToString();
            game.MaddenRecord["GHTG"] = p5.ToString();
            game.AwayTeam = teamInNeedOfP5;
            game.HomeTeam = p5;

            game = allG5;
            game.MaddenRecord["GATG"] = g5ToG5.ToString();
            game.MaddenRecord["GHTG"] = g5ToG5Two.ToString();
            game.AwayTeam = g5ToG5;
            game.HomeTeam = g5ToG5Two;
            return true;
        }

        static bool SwapTeams(PreseasonScheduledGame g1, PreseasonScheduledGame g2, FutureGame fg)
        {
            var teams = new[] { g2.HomeTeam, g1.HomeTeam, g2.AwayTeam, g1.AwayTeam }.ToList();    


            int g1H = 0, g2H = 0, g1A = 0, g2A = 0;


            if (g1.HomeTeam == fg.HomeId || g1.AwayTeam == fg.HomeId)
            {
                g1H = fg.HomeId;
                g1A = fg.AwayId;
                teams.Remove(g1H);
                teams.Remove(g1A);
                g2A = teams.Last();
                g2H = teams.First();
            }
            else if (g2.HomeTeam == fg.HomeId || g2.AwayTeam == fg.HomeId)
            {
                g2H = fg.HomeId;
                g2A = fg.AwayId;
                teams.Remove(g2H);
                teams.Remove(g2A);
                g1A = teams.Last();
                g1H = teams.First();
            }

            if( g1H.IsFcsTeam())
            {
                var temp = g1H;
                g1H = g1A;
                g1A = temp;
            }

            if (g2H.IsFcsTeam())
            {
                var temp = g2H;
                g2H = g2A;
                g2A = temp;
            }

            if (RecruitingFixup.TeamsInSameConference(teams[0],teams[1]))
                return false;


            g1.MaddenRecord["GATG"] = g1A.ToString();
            g1.MaddenRecord["GHTG"] = g1H.ToString();
            g1.AwayTeam =g1A;
            g1.HomeTeam = g1H;

            g2.MaddenRecord["GATG"] = g2A.ToString();
            g2.MaddenRecord["GHTG"] = g2H.ToString();
            g2.AwayTeam = g2A;
            g2.HomeTeam = g2H;
            return true ; 
        }

        static void SwapTeams(Dictionary<int, TeamSchedule> schedules, PreseasonScheduledGame g1, PreseasonScheduledGame g2)
        {
            var a1 = g1.AwayTeam;
            var a2 = g2.AwayTeam;

            SwapTeams(g1, g2);

            if (schedules.ContainsKey(a1))
            {
                schedules[a1][g2.WeekIndex] = g2;
            }

            if (schedules.ContainsKey(a2))
            {
                schedules[a2][g1.WeekIndex] = g1;
            }
        }

        public static Dictionary<int, string> StadiumsForTeams()
        {
            var teams = Form1.MainForm.maddenDB.lTables[167];
            return teams.lRecords.ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record["SGID"]);
        }

        public static Lazy<Dictionary<int, string>> TeamStadiums = new Lazy<Dictionary<int, string>>(StadiumsForTeams, true);

        public static void SetNeutralSiteLogos()
        {
            var nesg = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "NESG");

            var dict = new Dictionary<int, (int logo, int beforeWeek)>
            {
                [181] = (11, 14),
                [182] = (59, 14),
                [183] = (26, 14),
                [184] = (24, 14),
                [268] = (54, 8),
                [264] = (93, 5),
                [265] = (93, 5),
                [266] = (93, 5),
                [259] = (93, 5),
                [271] = (97, 5),
                [272] = (96, 5),
                [273] = (95, 5),
            };

            foreach(var mr in nesg.lRecords)
            {
                var sgid = mr["SGID"].ToInt32();
                var sewn = mr["SEWN"].ToInt32();

                if(dict.ContainsKey(sgid))
                {
                    var (logo, beforeWeek) = dict[sgid];

                    if(sewn < beforeWeek)
                    {
                        mr["RLID"] = logo.ToString();
                    }
                }
            }
        }

        public static void SetSunBeltChampionship()
        {
            MessageBox.Show("setting sunbelt ccg");
            // games in week 14 should be moved to week 16
            var schedule = Form1.MainForm.maddenDB.lTables[161];
            var teamSchedules = Form1.MainForm.maddenDB.lTables[113];

            var games = schedule.lRecords.Where(mr => mr["SEWN"].ToInt32() == 14).OrderBy(mr => mr["SGNM"].ToInt32()).ToArray();
            var set = new HashSet<int>(games.Select(mr => mr["SGNM"].ToInt32()));
            //var teamGames = teamSchedules.lRecords.Where(mr => mr["SEWN"].ToInt32() == 14 && set.Contains(mr["SGNM"].ToInt32())).OrderBy(mr => mr["SGNM"].ToInt32()).ToArray();

            // sun belt
            var bowlTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "BOWL");
            var gameNum = "43";
            var sbcChamp = bowlTable.lRecords.Single(mr => mr["BIDX"].ToInt32() == 42);
            sbcChamp["SGNM"] = gameNum;
            var game = games[0];
            //var teamGame1 = teamGames[0];
            //var teamGame2 = teamGames[1];
            const string ccgWeek = "16";
            game["GDAT"] = "4";
            game["GTOD"] = "1200";
            game["SEWN"] = ccgWeek;
            game["SEWT"] = ccgWeek;
            game["SGNM"] = gameNum;
            game["GHTG"] = "1023";
            game["GATG"] = "1023";

            /*
            teamGame1["SEWN"] = ccgWeek;
            teamGame2["SEWN"] = ccgWeek;

            teamGame1["SGNM"] = gameNum;
            teamGame2["SGNM"] = gameNum;

            teamGame1["THOA"] = "1";
            teamGame2["THOA"] = "1";*/
        }

        public static void FixSchedule()
        {
            Form1.LookForSchedules();
            var teamsInNeedOfReplacement = Form1.TeamOOC.Where(kvp => kvp.Value.Power5Opponents > 1).ToDictionary(kvp => kvp.Key, kvp => kvp);
            var schedule = Form1.MainForm.maddenDB.lTables[161];
            var teamSchedules = Form1.MainForm.maddenDB.lTables[113];
            var teams = Form1.MainForm.maddenDB.lTables[167];
            Dictionary<int, int> TeamAndConferences = teams.lRecords.ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record.lEntries[36].Data.ToInt32());
            var tc = TeamAndConferences;
            var teamsThatCanBeReplaced = schedule.lRecords.Where(g => tc.TeamsEligbleForReplacement(g.lEntries[7].Data.ToInt32(), g.lEntries[6].Data.ToInt32())).Select(g =>
                new
                {
                    GameNumber = g.lEntries[11].Data.ToInt32(),
                    Week = g.lEntries[12].Data.ToInt32(),
                    Home = g.lEntries[7].Data.ToInt32(),
                    Away = g.lEntries[6].Data.ToInt32()
                }).GroupBy(a => a.Week).ToDictionary(grouping => grouping.Key, group => group.ToList());

            for (int i = schedule.Table.currecords - 1; i >= 0; i--)
            {
                var gameRecord = schedule.lRecords[i];
                var homeTeam = schedule.lRecords[i].lEntries[7].Data.ToInt32();
                var awayTeam = schedule.lRecords[i].lEntries[6].Data.ToInt32();
                var gameNum = schedule.lRecords[i].lEntries[11].Data.ToInt32();
                var week = schedule.lRecords[i].lEntries[12].Data.ToInt32();
                var query = new Dictionary<string, string>();
                MaddenRecord teamScheduleRecord = null;
                query["SGNM"] = gameNum.ToString();
                query["SEWN"] = week.ToString();

                // turns off rece davis feature
                gameRecord["GFHU"] = "1";
                gameRecord["GFFU"] = "1";
                gameRecord["GMFX"] = "0";

                if (week > 13)
                    continue;

                // dont fix navy-nd
                if (IsNotreDameGame(homeTeam, awayTeam))
                    continue;

                // teams on their own shouldn't have their schedule messed with.  army and navy home games don't get messed with either
                if (RecruitingFixup.OnTheirOwn.Contains(homeTeam) || RecruitingFixup.OnTheirOwn.Contains(awayTeam) || (!IsServiceAcademyGame(homeTeam, awayTeam) && (homeTeam == 8 || homeTeam == 57)))
                    continue;

                // army-navy play at 181
                if (MatchTeams(homeTeam, awayTeam, new[] { 8, 57 }))
                {
                    gameRecord["SGID"] = "181";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // check for Mayhem at MBS
                else if (homeTeam == 31 && gameRecord["GTOD"].ToInt32() == ((60 * 19) + 33))
                {
                    gameRecord["SGID"] = "170";
                }
                // cu-csu play at 184
                else if (MatchTeams(homeTeam, awayTeam, new[] { 22, 23 }))
                {
                    gameRecord["SGID"] = "184";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // UF-UGA play at 183
                else if (MatchTeams(homeTeam, awayTeam, new[] { 27, 30 }))
                {
                    gameRecord["SGID"] = "183";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // ou-tx play at 182
                else if (MatchTeams(homeTeam, awayTeam, new[] { 71, 92 }))
                {
                    gameRecord["SGID"] = "182";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // wvu-umd play at 275
                // wvu-umd are now home-home, except every 5 years they should play at M&T
                else if (false && MatchTeams(homeTeam, awayTeam, new[] { 112, 47 }) && Form1.DynastyYear % 5 == 0)
                {
                    gameRecord["SGID"] = "275";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // tamu-ark play at 279 - southwest classic
                else if (MatchTeams(homeTeam, awayTeam, new[] { 6, 93 }))
                {
                    gameRecord["SGID"] = "278";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // baylor-tt play at 279 - Texas Shootout
                else if (false && MatchTeams(homeTeam, awayTeam, new[] { 11, 94 }))
                {
                    gameRecord["SGID"] = "279";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }
                // smu - texas play neutral site when SMU is at home
                else if (false && homeTeam == 83 && awayTeam == 92)
                {
                    gameRecord["SGID"] = "279";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }

                // smu-tcu play at 279 when smu is in big 12 on friday night
                else if (MatchTeams(homeTeam, awayTeam, new[] { 83, 89 }))
                {
                    gameRecord["SGID"] = "257";
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "1200";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }

                // houston-rice play at 272 on friday night
                else if (MatchTeams(homeTeam, awayTeam, new[] { 33, 79 }))
                {
                    gameRecord["SGID"] = "272";
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "1200";
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                }

                // colorado-nebraska play on black friday primetime
                else if (false && MatchTeams(homeTeam, awayTeam, new[] { 22, 58 }) && week == 13)
                {
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "1200";
                }

                // ou-nebraska play on black friday primetime
                else if (MatchTeams(homeTeam, awayTeam, new[] { 58, 71 }) && week == 13)
                {
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "1200";
                }

                // uva-vt play on black friday early
                else if (MatchTeams(homeTeam, awayTeam, new[] { 107, 108 }))
                {
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "720";
                }

                // wsu-uw play on black friday 4pm PST
                else if (MatchTeams(homeTeam, awayTeam, new[] { 110, 111 }))
                {
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "1140";
                }

                // unc-ncsu play on black friday 4pm EST
                else if (MatchTeams(homeTeam, awayTeam, new[] { 110, 111 }))
                {
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "960";
                }

                // usf-ucf play at 8pm too
                else if (MatchTeams(homeTeam, awayTeam, new[] { 18, 144 }) && week == 13)
                {
                    gameRecord["GDAT"] = "4";
                    gameRecord["GTOD"] = "1200";
                }

                // egg bowl is on thursday 430
                else if (MatchTeams(homeTeam, awayTeam, new[] { 55, 73 }))
                {
                    gameRecord["GDAT"] = "3";
                    gameRecord["GTOD"] = "990";
                }

                // red river shootout is at 330 (230 CT)
                else if (MatchTeams(homeTeam, awayTeam, new[] { 71, 92 }))
                {
                    gameRecord["GTOD"] = "930";
                }

#if false
                else if (!TeamAndConferences.IsTeamInPower5(homeTeam) && TeamAndConferences.IsTeamInPower5(awayTeam) && IsNotreDameGame(homeTeam, awayTeam) == false && IsRivalryGame(homeTeam, awayTeam) == false)
                {
                    //find stadium id then set the game record to the id
                    var teamQuery = new Dictionary<string, string>();
                    teamQuery["TGID"] = awayTeam.ToString();
                    var teamRecord = MaddenTable.Query(Form1.MainForm.maddenDB.lTables, "TEAM", teamQuery).SingleOrDefault();
                    gameRecord["SGID"] = teamRecord["SGID"];

                    // set the home team to away
                    query.Add("TGID", homeTeam.ToString());
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "0";

                    // set the away team to home
                    query["TGID"] = awayTeam.ToString();
                    teamScheduleRecord = MaddenTable.Query(teamSchedules, query).Single();
                    teamScheduleRecord["THOA"] = "1";
                    gameRecord["GATG"] = homeTeam.ToString();
                    gameRecord["GHTG"] = awayTeam.ToString();
                }
                else if (IsGame(homeTeam, awayTeam) && tc.TeamsInSameConference(homeTeam, awayTeam) == false && tc.IsTeamInPower5(homeTeam) && tc.IsTeamInPower5(awayTeam) && IsNotreDameGame(homeTeam, awayTeam) == false && IsRivalryGame(homeTeam, awayTeam) == false &&
                    IsNeutralSiteGame(gameRecord, teamSchedules, homeTeam, awayTeam) == false && (teamsInNeedOfReplacement.InNeedofReplaceAndContains(homeTeam) || teamsInNeedOfReplacement.InNeedofReplaceAndContains(awayTeam)) &&
                    teamsThatCanBeReplaced.ContainsKey(week) && teamsThatCanBeReplaced[week].Count > 0)
                {

                    var matchup = teamsThatCanBeReplaced[week].Pop();

                    while (matchup != null && IsRivalryGame(matchup.Home, matchup.Away))
                    {
                        matchup = teamsThatCanBeReplaced[week].Pop();
                    }

                    if (matchup != null)
                    {
                        if (matchup.Away.IsFcsTeam())
                        {
                            // swap home teams
                            SwapHomeTeams(gameRecord, FindGame(schedule, matchup.GameNumber, matchup.Week), matchup.Home, matchup.Away);
                            teamsInNeedOfReplacement.AdjustOOCValue(homeTeam,awayTeam);
                            teamsInNeedOfReplacement.AdjustOOCValue(awayTeam,homeTeam);
                        }
                        else 
                        {
                            // the current home team gets the away team popped out
                            ModifyTeamSchedule(gameRecord, awayTeam, matchup.Away);

                            var otherGame = FindGame(schedule, matchup.GameNumber, matchup.Week);
                            ModifyTeamSchedule(otherGame, matchup.Away, awayTeam);
                            teamsInNeedOfReplacement.AdjustOOCValue(homeTeam,awayTeam);
                            teamsInNeedOfReplacement.AdjustOOCValue(awayTeam,homeTeam);
                        }
                    }
                }
#endif
            }
        }

        static bool IsGame(int a, int b)
        {
            return a != 1023 && b != 1023;
        }

        static MaddenRecord FindGame(MaddenTable table, int game, int week)
        {
            var query = new Dictionary<string, string>();
            query["SGNM"] = game.ToString();
            query["SEWN"] = week.ToString();
            return MaddenTable.Query(table, query).SingleOrDefault();
        }

        static void SwapHomeTeams(MaddenRecord p5Game, MaddenRecord fcsGame, int fcsHost, int fcsTeam)
        {
            var teamQuery = new Dictionary<string, string>();
            teamQuery["TGID"] = fcsHost.ToString();

            // get the Team record of fcs host
            var teamHostingFcsTeam = MaddenTable.Query(Form1.MainForm.maddenDB.lTables, "TEAM", teamQuery).SingleOrDefault();

            // get the record of the game to replace host
            teamQuery["TGID"] = p5Game["GHTG"];
            var p5Host = MaddenTable.Query(Form1.MainForm.maddenDB.lTables, "TEAM", teamQuery).SingleOrDefault();

            // swap home teams and stadiums
            p5Game["GHTG"] = fcsHost.ToString();
            p5Game["SGID"] = teamHostingFcsTeam["SGID"];
            fcsGame["GHTG"] = p5Host["TGID"].ToString();
            fcsGame["SGID"] = p5Host["SGID"];

            // get the team schedule for 3 teams involved
            var teamScheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TSCH");

            // get the game # and week #
            var gameNum = p5Game["SGNM"];
            var weekNum = p5Game["SEWN"];

            var p5Away = teamScheduleTable.lRecords.Where(mr => mr["SGNM"] == gameNum && mr["SEWN"] == weekNum && mr["TGID"] == p5Game["GATG"]).SingleOrDefault();
            var p5Home = teamScheduleTable.lRecords.Where(mr => mr["SGNM"] == gameNum && mr["SEWN"] == weekNum && mr["TGID"] == p5Host["TGID"]).SingleOrDefault();
            var fcsHostHome = teamScheduleTable.lRecords.Where(mr => mr["SGNM"] == fcsGame["SGNM"] && mr["SEWN"] == fcsGame["SEWN"] && mr["TGID"] == fcsHost.ToString()).SingleOrDefault();

            // new home team gets the fcs game opp and number
            p5Home["OGID"] = fcsTeam.ToString();
            p5Home["SGNM"] = fcsGame["SGNM"];

            // gcshost gets the p5 game opp and number
            p5Away["OGID"] = fcsHost.ToString();
            fcsHostHome["OGID"] = p5Away["TGID"];
            fcsHostHome["SGNM"] = p5Away["SGNM"];
        }

        static void ModifyTeamSchedule(MaddenRecord mr, int previousValue, int newValue)
        {
            // get the team schedule
            var teamScheduleTable = MaddenTable.FindTable(Form1.MainForm.maddenDB.lTables, "TSCH");
            mr["GATG"] = newValue.ToString();

            // get the game # and week #
            var gameNum = mr["SGNM"];
            var weekNum = mr["SEWN"];

            var query = new Dictionary<string, string>();
            query["SGNM"] = gameNum;
            query["SEWN"] = weekNum;
            query["TGID"] = previousValue.ToString();

            var teamScheduleRecord = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
            query.Remove("TGID");
            query["OGID"] = previousValue.ToString();
            var oppRecord = MaddenTable.Query(teamScheduleTable, query).SingleOrDefault();
            teamScheduleRecord["TGID"] = newValue.ToString();
            oppRecord["OGID"] = newValue.ToString(); ;
        }

        static bool IsNeutralSiteGame(MaddenRecord gameRecord, MaddenTable teamSchedules, int home, int away)
        {
            var gameNum = gameRecord.lEntries[11].Data.ToInt32();
            var week = gameRecord.lEntries[12].Data.ToInt32();
            var query = new Dictionary<string, string>();
            query["SGNM"] = gameNum.ToString();
            query["SEWN"] = week.ToString();

            // find home team record
            query["TGID"] = home.ToString();
            var htGame = MaddenTable.Query(teamSchedules, query).Single();

            // set the away team to home
            query["TGID"] = away.ToString();
            var atGame = MaddenTable.Query(teamSchedules, query).Single();

            return atGame["THOA"] == "1" && htGame["THOA"] == "1";
        }

        public static bool IsRivalryGame(int homeTeam, int awayTeam)
        {
            var rivalries = new List<int[]>();

            // odu - marshall
            rivalries.Add(new[] { 234, 46 });

            // odu - ecu
            rivalries.Add(new[] { 234, 25 });

            // odu - app st
            rivalries.Add(new[] { 234, 901 });

            // su-temple
            rivalries.Add(new[] { 88, 90 });


            // ru-uconn
            // rivalries.Add(new[] { 80, 100 });

            // gt-gsu
            rivalries.Add(new[] {31, 233 });

            // ole miss-usm
            rivalries.Add(new[] { 73, 85 });

            // miss st-usm
            rivalries.Add(new[] { 55, 85 });

            // ole miss-memphis
            rivalries.Add(new[] { 73, 48 });

            // Marshall-ohio
            rivalries.Add(new[] { 46, 69 });

            // UMD-Navy state
            rivalries.Add(new[] { 47, 57 });

            // Utah-Utah state
            rivalries.Add(new[] { 103, 104 });

            // byu-Utah state
            rivalries.Add(new[] { 16, 104 });

            // Utah-BYU
            rivalries.Add(new[] { 103, 16 });

            // TCU-BYU
            rivalries.Add(new[] { 89, 16 });

            // TCU-BSU
            rivalries.Add(new[] { 89, 12 });

            // CU-CSU
            rivalries.Add(new[] { 23, 22 });

            // WYO-CSU
            rivalries.Add(new[] { 23, 115 });

            // Texas - TAMU
            rivalries.Add(new[] { 92, 93 });

            // TCU-SMU
            rivalries.Add(new[] { 83, 89 });

            // HOU-SMU
            rivalries.Add(new[] { 33, 89 });

            // Pitt - WVU
            rivalries.Add(new[] { 112, 77 });

            // Ark - Texas
            rivalries.Add(new[] { 92, 6 });

            // Baylor - TAMU
            rivalries.Add(new[] { 11, 93 });

            // Clemson -SCAR
            rivalries.Add(new[] { 21, 84 });

            // Colorado - Nebraska
            rivalries.Add(new[] { 22, 58 });

            // UF-FSU
            rivalries.Add(new[] { 27, 28 });

            // UF-Miami
            rivalries.Add(new[] { 27, 49 });

            // Illinois - Mizzou
            rivalries.Add(new[] { 35, 56 });

            // Iowa - ISU
            rivalries.Add(new[] { 37, 38 });

            // Mizzou - ISU
            rivalries.Add(new[] { 56, 38 });

            // Mizzou - Kansas
            rivalries.Add(new[] { 56, 39 });

            // UL - UK
            rivalries.Add(new[] { 42, 44 });

            // UGA - GT
            rivalries.Add(new[] { 30, 31 });

            // Maryland - WVU
            rivalries.Add(new[] { 47, 112 });

            // Pitt - WVU
            rivalries.Add(new[] { 77, 112 });

            // Mizzou - Nebraska
            rivalries.Add(new[] { 56, 58 });

            // Mizzou - OU
            rivalries.Add(new[] { 56, 71 });

            // PSU - Pitt
            rivalries.Add(new[] { 77, 76 });

            // PSU - WVU
            rivalries.Add(new[] { 112, 76 });

            // SU - WVU
            rivalries.Add(new[] { 112, 88 });

            // TAMU  - Texas Tech
            rivalries.Add(new[] { 94, 93 });

            // WVU - VT
            rivalries.Add(new[] { 108, 112 });

            // WVU - Marshall            
            rivalries.Add(new[] { 46, 112 });

            // army-af
            rivalries.Add(new[] { 8, 1 });

            // army-navy
            rivalries.Add(new[] { 8, 57 });

            // af-navy
            rivalries.Add(new[] { 1, 57 });

            // usc-nd
            rivalries.Add(new[] { 102, 68 });

            // stanford-nd
            rivalries.Add(new[] { 87, 68 });

            // cincy-miami u
            rivalries.Add(new[] { 50, 20 });

            // cincy-Louisville
            rivalries.Add(new[] { 44, 20 });

            // cincy-wvu
            rivalries.Add(new[] { 112, 20 });

            // houston - tulsa
            rivalries.Add(new[] { 33, 97 });

            // houston - rice
            rivalries.Add(new[] { 33, 79 });

            // USF - UCF
            rivalries.Add(new[] { 18, 144 });

            // lsu - tulane
            rivalries.Add(new[] { 45, 96 });

            // Pitt - Marshall            
            rivalries.Add(new[] { 46, 77 });

            // Pitt - Cincy            
            rivalries.Add(new[] { 20, 77 });

            // PSU - Cincy            
            rivalries.Add(new[] { 20, 76 });

            // NMSU - UNM
            // rivalries.Add(new[] { 60, 61 });

            // GaSo-App St
            rivalries.Add(new[] { 901, 902 });

            // Tulane-USM
            rivalries.Add(new[] { 96, 85 });

            // UTEP-NMSU
            // rivalries.Add(new[] { 61, 105 });

            // SDSU-FS
            rivalries.Add(new[] { 81, 29 });

            // SDSU-SJSU
            rivalries.Add(new[] { 81, 82 });

            // ECU-MARSH
            rivalries.Add(new[] { 25, 46 });

            // BYU-BSU
            rivalries.Add(new[] { 16, 12 });

            // UAB-USM
            rivalries.Add(new[] { 85, 98 });

            // LT-USM
            rivalries.Add(new[] { 85, 43 });

            // tulane-USM
            rivalries.Add(new[] { 85, 96 });

            // memphis-USM
            rivalries.Add(new[] { 85, 48 });

            // Arizona-UNM
            rivalries.Add(new[] { 4, 60 });

            // utep-tulsa
            rivalries.Add(new[] { 97, 105 });

            // wku-mtsu
            rivalries.Add(new[] { 211, 53 });

            // wku-marsh
            rivalries.Add(new[] { 46, 211 });

            // utsa-tex st
            rivalries.Add(new[] { 218, 232 });

            // troy-mtsu
            rivalries.Add(new[] { 143, 53 });

            // nt-mtsu
            rivalries.Add(new[] { 64, 53 });

            // af-csu
            rivalries.Add(new[] { 1, 23 });

            // af-wyoming
            rivalries.Add(new[] { 1, 115 });

            // af-hawaii
            rivalries.Add(new[] { 1, 32 });

            // BSU-WSU
            rivalries.Add(new[] { 12, 111 });

            // fau-fiu
            // rivalries.Add(new[] { 230, 229 });

            // army-ru
            rivalries.Add(new[] { 8, 80 });

            // navy-ru
            rivalries.Add(new[] { 57, 80 });

            // tcu-bsu
            rivalries.Add(new[] { 12, 89 });

            // buffalo-su
            rivalries.Add(new[] { 15, 88 });

            // buffalo-temple
            rivalries.Add(new[] { 15, 90 });

            // byu-hawaai
            rivalries.Add(new[] { 16, 32 });

            // byu-sdsu
            rivalries.Add(new[] { 16, 81 });

            // uconn-su
            //rivalries.Add(new[] { 100, 88 });

            // uconn-buffalo
            //rivalries.Add(new[] { 100, 15 });

            // uconn-bc
            //rivalries.Add(new[] { 100, 13 });

            // uconn-psu
            //rivalries.Add(new[] { 100, 76 });

            // uconn-temple
            //rivalries.Add(new[] { 100, 90 });

            // uconn-wvu
            //rivalries.Add(new[] { 100, 112 });

            // uconn-duke
            //rivalries.Add(new[] { 100, 24 });

            // uconn-indy
            //rivalries.Add(new[] { 100, 36 });

            // uconn-unc
            //rivalries.Add(new[] { 100, 62 });

            // uconn-ul
            //rivalries.Add(new[] { 100, 44 });

            // uconn-uk
            //rivalries.Add(new[] { 100, 42 });

            // usm-ecu
            rivalries.Add(new[] { 25, 85 });

            // marshall-ecu
            rivalries.Add(new[] { 25, 46 });

            // fau-troy
            rivalries.Add(new[] { 143, 229 });

            // fs-lt
            rivalries.Add(new[] { 43, 29 });

            // ulm-lt
            rivalries.Add(new[] { 43, 65 });

            // ull-lt
            rivalries.Add(new[] { 43, 86 });

            // marshall-ohio
            rivalries.Add(new[] { 69, 46 });

            // memphis-ul
            rivalries.Add(new[] { 48, 44 });

            // nt-smu
            rivalries.Add(new[] { 64, 83 });

            // odu-vt
            rivalries.Add(new[] { 234, 108 });

            // odu-uva
            rivalries.Add(new[] { 234, 107 });

            // sjsu-stan
            rivalries.Add(new[] { 82, 87 });

            // temple-psu
            rivalries.Add(new[] { 90, 76 });

            // temple-pitt
            rivalries.Add(new[] { 90, 77 });

            // tulsa-ok st
            rivalries.Add(new[] { 97, 72 });

            // memphis-uab
            rivalries.Add(new[] { 48, 98 });

            // utep-tulsa
            rivalries.Add(new[] { 105, 97 });

            // ecu-marshall
            rivalries.Add(new[] { 25, 46 });

            // ecu-usm
            rivalries.Add(new[] { 25, 85 });

            // ecu-ncsu
            rivalries.Add(new[] { 25, 63 });

            // ecu-duke
            rivalries.Add(new[] { 25, 24 });

            // ecu-unc
            rivalries.Add(new[] { 25, 62 });

            // ecu-wf
            rivalries.Add(new[] { 25, 109 });

            // odu-gs
            rivalries.Add(new[] { 234,901 });

            // app st-gs
            rivalries.Add(new[] { 901, 902 });

            // gs - gsu
            rivalries.Add(new[] { 233, 902 });

            // af - army
            rivalries.Add(new[] { 1, 8 });

            // af - csu
            rivalries.Add(new[] { 1, 23 });

            // af - hawaii
            rivalries.Add(new[] { 1, 32 });

            // af - navy
            rivalries.Add(new[] { 1, 57 });

            // akron - kent st
            rivalries.Add(new[] { 2, 41 });

            // alabama-auburn
            rivalries.Add(new[] { 3, 9 });

            // alabama-uf
            rivalries.Add(new[] { 3, 27 });

            // alabama-gt
            rivalries.Add(new[] { 3, 31 });

            // alabama-lsu
            rivalries.Add(new[] { 3, 45 });

            // alabama-miss st
            rivalries.Add(new[] { 3, 55 });

            // alabama-ole miss
            rivalries.Add(new[] { 3, 73 });

            // alabama-tenn
            rivalries.Add(new[] { 3, 91 });

            // au-asu
            rivalries.Add(new[] { 4, 5 });

            // au-unm
            rivalries.Add(new[] { 4, 60 });

            // ark-ark st
            rivalries.Add(new[] { 6, 7 });

            // ark-lsu
            rivalries.Add(new[] { 6, 45 });

            // ark-ole miss
            rivalries.Add(new[] { 6, 73 });

            // ark-texas
            rivalries.Add(new[] { 6, 92 });

            // ark-tamu
            rivalries.Add(new[] { 6, 93 });

            // ark st-nt
            rivalries.Add(new[] { 7, 64 });

            // ark st-ulm
            rivalries.Add(new[] { 7, 65 });

            // army-navy
            rivalries.Add(new[] { 8, 57 });

            // army-ru
            rivalries.Add(new[] { 8, 80 });

            // auburn next

            //baylor-smu
            rivalries.Add(new[] { 11, 83 });

            //baylor-rice
            rivalries.Add(new[] { 11, 79 });

            //baylor-tamu
            rivalries.Add(new[] { 11, 93 });

            // boise st - nevada
            rivalries.Add(new[] { 12, 59 });

            // boise st - fs
            rivalries.Add(new[] { 12, 29 });

            // boise st - hawaii
            rivalries.Add(new[] { 12, 32 });

            // smu - rice
            rivalries.Add(new[] { 79, 83 });

            // smu - houston
            rivalries.Add(new[] { 33, 83 });

            // smu - nt
            rivalries.Add(new[] { 64, 83 });

            // smu - navy
            rivalries.Add(new[] { 57, 83 });

            // nd - navy
            rivalries.Add(new[] { 57, 68 });

            // GS - GSU
            rivalries.Add(new[] { 233, 902 });

            // GS - App St
            rivalries.Add(new[] { 901, 902 });

            // buffalo - su
            rivalries.Add(new[] { 15, 88 });

            // buffalo - temple
            rivalries.Add(new[] { 15, 90 });

            // fs - lt
            rivalries.Add(new[] { 29, 43 });

            // lt - ulm
            rivalries.Add(new[] { 43, 65 });

            // lt - ull
            rivalries.Add(new[] { 43, 86 });

            // marshall - ohio
            rivalries.Add(new[] { 46, 69 });

            // memphis - uab
            rivalries.Add(new[] { 48, 98 });

            // mtsu - troy
            rivalries.Add(new[] { 53, 143 });

            // usm - tulane
            rivalries.Add(new[] { 85, 96 });

            // unm-utep
            rivalries.Add(new[] { 60, 105 });

            // ecu-appst
            rivalries.Add(new[] { 25, 901 });

            // ul-wku
            rivalries.Add(new[] { 44, 211 });

            // uk-wku
            rivalries.Add(new[] { 42, 211 });

            return rivalries.Where(r => (r[0]==homeTeam && r[1]==awayTeam)||(r[1] == homeTeam && r[0] == awayTeam)).Any();
        }

        public static bool IsNotreDameGame(int homeTeam, int awayTeam)
        {
            return homeTeam == 68 || awayTeam == 68;
        }

        public static int ToInt32(this object obj)
        {
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
            }

            return 0;
        }

        public static T Pop<T>(this List<T> list)
        {
            if (list.Count == 0)
                return default(T);

            var first = list.First();
            list.RemoveAt(0);
            return first;
        }

        public static PreseasonScheduledGame Pop(this List<PreseasonScheduledGame> list, PreseasonScheduledGame game, bool lookForExtraConfGame)
        {
            if(lookForExtraConfGame)
            {
                var weekIndex = game.WeekIndex;

                var first = list.FirstOrDefault(g => g.WeekIndex == weekIndex && g.IsExtraConferenceGame() && !RecruitingFixup.TeamsInSameConference(game.HomeTeam, g.AwayTeam) && !RecruitingFixup.TeamsInSameConference(g.HomeTeam, game.AwayTeam));

                if (first != null)
                {
                    list.Remove(first);
                    return first;
                }
            }

            return list.Pop(game);
        }

        public static PreseasonScheduledGame Pop(this List<PreseasonScheduledGame> list, PreseasonScheduledGame game)
        {
            var weekIndex = game.WeekIndex;

            var first = list.FirstOrDefault(g => g.WeekIndex == weekIndex && !RecruitingFixup.TeamsInSameConference(game.HomeTeam, g.AwayTeam) && !RecruitingFixup.TeamsInSameConference(g.HomeTeam, game.AwayTeam));

            if (first != null)
                list.Remove(first);

            return first;
        }

        static bool InNeedofReplaceAndContains(this Dictionary<int, KeyValuePair<int, TeamOOC>> dict, int key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key].Value.CurrentPower5 > 1;
            }

            return false;
        }

        static void AdjustOOCValue(this Dictionary<int, KeyValuePair<int, TeamOOC>> dict, int key, int removeTeamId)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Value.CurrentPower5--;
                dict[key].Value.Opponents.Remove(removeTeamId);
            }
        }
    }

    public class TeamOOC
    {
        public bool HasFcsGame { get; set; }
        public int CurrentPower5 { get; set; }
        public TeamOOC()
        {
            this.Opponents = new List<int>();
        }
        public int TeamId { get; set; }
        public List<int> Opponents { get; set; }
        public void AddGame(int opp)
        {
            if (this.Opponents.Contains(opp) == false)
            {
                this.Opponents.Add(opp);

                if (RecruitingFixup.IsP5(opp))
                    this.CurrentPower5++;
            }
        }

        public int Power5Opponents
        {
            get
            {
                return this.Opponents.Where(opp => RecruitingFixup.IsP5(opp)).Count();
            }
        }
    }
}