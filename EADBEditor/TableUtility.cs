using System.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace EA_DB_Editor
{
    [Serializable]
    public class UpsetAlert
    {
        public string Headline { get; set; }

        public int Game { get; set; }

        public string Scenario { get; set; }

        public bool Upset { get; set; }
    }

    public static class TableUtility
    {
        public static MaddenTable FindTable(string name)
        {
            return MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, name);
        }

        public static HashSet<int> FindUserTeams()
        {
            var sttm = FindTable("STTM");
            var result = new HashSet<int>();

            foreach (var mr in sttm.lRecords)
            {
                if (mr["CFUC"].ToInt32() == 1)
                {
                    result.Add(mr["TGID"].ToInt32());
                }
            }

            return result;
        }

        public static int Prestige(this MaddenRecord mr)
        {
            return mr["TPRX"].ToInt32();
        }
        public static string TeamName(this MaddenRecord mr)
        {
            return mr["TLNA"];
        }

        public static int MediaPollRanking(this MaddenRecord mr)
        {
            return mr["TMRK"].ToInt32();
        }

        public static int TeamRating(this MaddenRecord mr)
        {
            return mr["TROV"].ToInt32();
        }

        public static int WinPct(this MaddenRecord mr)
        {
            var win = mr.Wins();
            var loss = mr.Losses();

            if ((win + loss) == 0) return -1;

            return (win * 1000) / (win + loss);
        }   

        public static int Wins(this MaddenRecord mr)
        {
            return mr["TSWI"].ToInt32();
        }

        public static int Losses(this MaddenRecord mr)
        {
            return mr["TSLO"].ToInt32();
        }

        public static int AwayScore(this MaddenRecord mr)
        {
            return mr["GASC"].ToInt32();
        }

        public static int HomeScore(this MaddenRecord mr)
        {
            return mr["GHSC"].ToInt32();
        }

        public static int TeamId(this MaddenRecord mr)
        {
            return mr["TGID"].ToInt32();
        }

        public static int GameNumber(this MaddenRecord mr)
        {
            return mr["SGNM"].ToInt32();
        }

        public static string DisplayName(this Dictionary<int, MaddenRecord> dict, int tgid)
        {
            var team = dict[tgid];
            var ranking = team.MediaPollRanking();
            var rankingField = ranking <= 25 ? $"#{ranking} " : string.Empty;
            return $"{rankingField}{team.TeamName()}";
        }

        public static List<UpsetAlert> ReportBadUpsets()
        {
            var result = new List<UpsetAlert>();

            var sgin = FindTable("SGIN");
            var team = FindTable("TEAM").lRecords.ToDictionary(mr => mr.TeamId(), mr => mr);

            foreach (var mr in sgin.lRecords)
            {
                var (needsFixing, winningTeam, losingTeam, reason) = mr.EvaluateGameForStudioUpdate(team);
                result.Add(new UpsetAlert
                {
                    Headline = $"{team.DisplayName(winningTeam)} defeats {team.DisplayName(losingTeam)}",
                    Game = mr.GameNumber(),
                    Scenario = reason,
                    Upset = needsFixing,
                });
            }

            return result;
        }

        public static (bool needsFixing, int winningTeam, int losingTeam, string reason) EvaluateGameForStudioUpdate(this MaddenRecord mr, Dictionary<int, MaddenRecord> teams)
        {
            var away = mr.GetAwayTeam();
            var home = mr.GetHomeTeam();
            var awayScore = mr.AwayScore();
            var homeScore = mr.HomeScore();
            var winner = 0;
            var loser = 0;
            bool homeLost = false;

            if (awayScore > homeScore)
            {
                winner = away;
                loser = home;
                homeLost = true;
            }
            else
            {
                winner = home;
                loser = away;
            }

            // g5 beating p5 warrants a look
            if (winner.IsG5() && loser.IsP5OrND())
            {
                return (true, winner, loser, "G5");
            }

            // big overall gap
            var winnerRating = teams[winner].TeamRating();
            var loserRating = teams[loser].TeamRating();
            if ((loserRating - winnerRating) >= 10)
            {
                return (true, winner, loser, $"OVR: {winnerRating} :: {loserRating}");
            }

            // unranked beats ranked team
            var winnerRank = teams[winner].MediaPollRanking();
            var loserRank = teams[loser].MediaPollRanking();

            if (winnerRank > 25 && loserRank <= 25)
            {
                return (true, winner, loser, $"Unranked: {winnerRating} :: {loserRating}");
            }

            if (homeLost && ((loserRating - winnerRating) > 5))
            {
                return (true, winner, loser, $"HomeLoss: {winnerRating} :: {loserRating}");
            }

            var winnerRecord = teams[winner].WinPct();
            var loserRecord = teams[loser].WinPct();

            if (loserRecord > 650 && winnerRecord < 500)
            {
                return (true, winner, loser, $"WinPct: {winnerRecord} :: {loserRecord}");
            }

            return (false, winner, loser, string.Empty);
        }

        public static int GetHomeTeam(this MaddenRecord mr)
        {
            return mr["GHTG"].ToInt32();
        }

        public static int GetAwayTeam(this MaddenRecord mr)
        {
            return mr["GATG"].ToInt32();
        }

        public static void FixSgin(string upsetsFile)
        {
            var json = File.ReadAllText(upsetsFile);
            var upsets = JsonConvert.DeserializeObject<List<UpsetAlert>>(json);
            var hash = new HashSet<int>(upsets.Select(u => u.Game));

            var sgin = FindTable("SGIN");

            foreach (var mr in sgin.lRecords)
            {
                if (hash.Contains(mr.GameNumber()))
                {
                    mr["SGNM"] = "127";
                }
            }
        }

        public static void SetupForStudioUpdates()
        {
            var schd = TableUtility.FindTable("SCHD");
            var userTeams = TableUtility.FindUserTeams();

            foreach (var mr in schd.lRecords)
            {
                var away = mr.GetAwayTeam();
                var home = mr.GetHomeTeam();

                // don't change anything about the games vs fcs teams
                if (away.IsFcsTeam())
                {
                    mr["GFFU"] = "1";
                    mr["GFHU"] = "1";
                    mr["GMFX"] = "0";
                    continue;
                }

                // user team should not have a studio update
                if (userTeams.Contains(home) || userTeams.Contains(away))
                {
                    mr["GFFU"] = "1";
                    mr["GFHU"] = "1";
                    mr["GMFX"] = "1";
                }
                else
                {
                    mr["GFFU"] = "0";
                    mr["GFHU"] = "0";
                    mr["GMFX"] = "1";
                }
            }
        }

        public static bool IsFcsTeam(this int teamId)
        {
            return teamId >= 160 && teamId <= 164;
        }

        #region conference utilities
        public const int ACCId = 0;
        public const int AmericanId = 3;
        public const int Big12Id = 2;
        public const int Big16Id = 200;
        public const int Big10Id = 1;
        public const int CUSAId = 4;
        public const int MACId = 7;
        public const int MWCId = 9;
        public const int Pac16Id = 10;
        public const int SECId = 11;
        public const int SBCId = 13;
        public const int IndId = 5;

        public static bool IsP5OrND(this int teamId)
        {
            return teamId.IsP5() || teamId == 68;
        }

        /// <summary>
        /// Whether or not ateam is in the Power5
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static bool IsP5(this int teamId)
        {
            return ACC.Contains(teamId) || Big10.Contains(teamId) || Big12.Contains(teamId) || Pac12.Contains(teamId) || SEC.Contains(teamId);
        }

        public static bool IsG5(this int teamId)
        {
            return American.Contains(teamId) || MWC.Contains(teamId) || MAC.Contains(teamId) || SBC.Contains(teamId) || CUSA.Contains(teamId) || teamId == 57 || teamId == 8 || teamId == 1;
        }

        public static bool IsIndependentG5(this int teamId)
        {
            return TeamAndConferences[teamId] == IndId && teamId != 16 && teamId != 68;
        }


        public static int[] ACCConfTeams { get { return TeamAndConferences.Where(kvp => kvp.Value == ACCId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] ACC { get { return TeamAndConferences.Where(kvp => kvp.Value == ACCId).Select(kvp => kvp.Key).Concat(new[] { 68 }).Distinct().ToArray(); } }

        public static int[] Big10 { get { return TeamAndConferences.Where(kvp => kvp.Value == Big10Id).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] Big12 { get { return TeamAndConferences.Where(kvp => kvp.Value == Big12Id).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] Pac12 { get { return TeamAndConferences.Where(kvp => kvp.Value == Pac16Id).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] SEC { get { return TeamAndConferences.Where(kvp => kvp.Value == SECId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] American { get { return TeamAndConferences.Where(kvp => kvp.Value == AmericanId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] MAC { get { return TeamAndConferences.Where(kvp => kvp.Value == MACId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] CUSA { get { return TeamAndConferences.Where(kvp => kvp.Value == CUSAId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] SBC { get { return TeamAndConferences.Where(kvp => kvp.Value == SBCId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] MWC { get { return TeamAndConferences.Where(kvp => kvp.Value == MWCId).Select(kvp => kvp.Key).ToArray(); } }

        private static Dictionary<int, int> teamAndConferences;

        public static Dictionary<int, int> TeamAndConferences
        {
            get
            {
                if (teamAndConferences == null || teamAndConferences.Count == 0)
                {
                    try
                    {
                        teamAndConferences = Form1.MainForm.maddenDB.lTables[167].lRecords
                            .Where(mr => mr.lEntries[40].Data.ToInt32() != 611 && mr.lEntries[40].Data.ToInt32() != 300)
                            .ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record.lEntries[36].Data.ToInt32());
                    }
                    catch
                    {
                        teamAndConferences = new Dictionary<int, int>();
                    }
                }

                return teamAndConferences;
            }
        }
        public const int NotreDameId = 68;
        public const int BYUId = 16;
        public const int CincyId = 20;
        public const int UCFId = 18;
        public const int USFId = 144;
        public const int LSUId = 45;

        public static Dictionary<int, int[]> CreateConferenceAssignmentsForStates()
        {
            int[] allConf = new int[] { ACCId, Big12Id, Big10Id, Pac16Id, SECId, NotreDameId };
            #region state stuff
            var dict = new Dictionary<int, int[]>();
            dict.Add(0, new int[] { SECId }); //AL
            dict.Add(1, allConf); //AK
            dict.Add(2, new int[] { Pac16Id, BYUId }); //AZ
            dict.Add(3, new int[] { SECId }); //AR
            dict.Add(4, new int[] { Pac16Id, NotreDameId, Pac16Id, NotreDameId, BYUId }); //CA
            dict.Add(5, new int[] { TeamAndConferences[22], NotreDameId, BYUId }); //CO
            dict.Add(6, new int[] { ACCId, Big10Id, NotreDameId }); //CT
            dict.Add(7, new int[] { ACCId, Big10Id, NotreDameId }); //DE
            dict.Add(8, new int[] { SECId, ACCId, SECId, ACCId, UCFId, USFId }); //FL
            dict.Add(9, new int[] { SECId, ACCId }); //GA
            dict.Add(10, new int[] { Pac16Id, NotreDameId }); //HI
            dict.Add(11, new int[] { Pac16Id, BYUId }); //ID
            dict.Add(12, new int[] { Big10Id, NotreDameId }); //IL
            dict.Add(13, new int[] { Big10Id, NotreDameId }); //IN
            dict.Add(14, new int[] { Big10Id, NotreDameId, Big12Id }); //IA
            dict.Add(15, new int[] { Big12Id }); //KS
            dict.Add(16, new int[] { TeamAndConferences[44], SECId }); //KY
            dict.Add(17, new int[] { SECId, LSUId }); //LA
            dict.Add(18, new int[] { ACCId, Big10Id, NotreDameId }); //ME
            dict.Add(19, new int[] { ACCId, NotreDameId }); //MD
            dict.Add(20, new int[] { ACCId, NotreDameId }); //MA
            dict.Add(21, new int[] { Big10Id, NotreDameId, CincyId }); //MI
            dict.Add(22, new int[] { Big10Id, NotreDameId }); //MN
            dict.Add(23, new int[] { SECId }); //MS
            dict.Add(24, new int[] { SECId, Big12Id }); //MO
            dict.Add(25, new int[] { Pac16Id, NotreDameId }); //MT
            dict.Add(26, new int[] { Big12Id }); //NE
            dict.Add(27, new int[] { Pac16Id, NotreDameId, BYUId }); //NV
            dict.Add(28, new int[] { ACCId, Big10Id, NotreDameId }); //NH
            dict.Add(29, new int[] { ACCId, Big10Id, NotreDameId }); //NJ
            dict.Add(30, new int[] { Pac16Id, Big12Id, NotreDameId }); //NM
            dict.Add(31, new int[] { ACCId, Big10Id, NotreDameId }); //NY
            dict.Add(32, new int[] { ACCId }); //NC
            dict.Add(33, new int[] { Pac16Id, NotreDameId, BYUId }); //ND
            dict.Add(34, new int[] { Big10Id, NotreDameId, Big10Id, NotreDameId, Big10Id, NotreDameId, CincyId }); //OH
            dict.Add(35, new int[] { Big12Id }); //OK
            dict.Add(36, new int[] { Pac16Id }); //OR
            dict.Add(37, new int[] { ACCId, Big10Id, NotreDameId, CincyId }); //PA
            dict.Add(38, new int[] { ACCId, Big10Id, NotreDameId }); //RI
            dict.Add(39, new int[] { ACCId, SECId }); //SC
            dict.Add(40, new int[] { Pac16Id, Big12Id, NotreDameId, BYUId }); //SD
            dict.Add(41, new int[] { SECId, SECId }); //TN
            dict.Add(42, new int[] { Big12Id, SECId, NotreDameId }); //TX
            dict.Add(43, new int[] { Pac16Id, NotreDameId, BYUId }); //UT
            dict.Add(44, new int[] { ACCId, Big10Id, NotreDameId }); //VT
            dict.Add(45, new int[] { ACCId, NotreDameId }); //VA
            dict.Add(46, new int[] { Pac16Id }); //WA
            dict.Add(47, new int[] { ACCId }); //WV
            dict.Add(49, allConf); //WY
            dict.Add(48, new int[] { Big10Id, NotreDameId }); //WI
            dict.Add(50, allConf); //CN
            dict.Add(51, allConf); //DC
            #endregion

            var teams = new Dictionary<int, int[]>();
            SetWeightedArrays();

            foreach (var kvp in dict)
            {
                List<int> allTeams = new List<int>();

                foreach (var conf in kvp.Value)
                {
                    switch (conf)
                    {
                        case ACCId:
                            allTeams.AddRange(WeightedACC.Where(t => t != 68));
                            break;
                        case Big10Id:
                            allTeams.AddRange(WeightedBig10);
                            break;
                        case Big12Id:
                            allTeams.AddRange(WeightedBig12);
                            break;
                        case Pac16Id:
                            allTeams.AddRange(WeightedPac16);
                            break;
                        case SECId:
                            allTeams.AddRange(WeightedSEC);
                            break;
                        case Big16Id:
                            allTeams.AddRange(WeightedBig16);
                            break;
                        case NotreDameId:
                            allTeams.AddRange(WeightedND);
                            break;
                        case BYUId:
                            allTeams.AddRange(WeightedBYU);
                            break;
                        case CincyId:
                            allTeams.AddRange(WeightedCincy);
                            break;
                        case UCFId:
                            allTeams.AddRange(WeightedUCF);
                            break;
                        case USFId:
                            allTeams.AddRange(WeightedUSF);
                            break;
                        case LSUId:
                            allTeams.AddRange(WeightedLSU);
                            break;
                        default:
                            break;
                    }
                }

                teams[kvp.Key] = allTeams.ToArray();
            }

            return teams;
        }

        static List<int> WeightedACC = null;
        static List<int> WeightedBig10 = null;
        static List<int> WeightedBig12 = null;
        static List<int> WeightedPac16 = null;
        static List<int> WeightedSEC = null;
        static List<int> WeightedBYU = null;
        static List<int> WeightedND = null;
        static List<int> WeightedBig16 = null;
        static List<int> WeightedCincy = null;
        static List<int> WeightedUCF = null;
        static List<int> WeightedUSF = null;
        static List<int> WeightedLSU = null;

        static void SetWeightedArrays()
        {
            if (WeightedACC != null)
                return;

            WeightedACC = CreateWeightedList(ACCConfTeams);
            WeightedBig10 = CreateWeightedList(Big10);
            WeightedBig12 = CreateWeightedList(Big12);
            WeightedSEC = CreateWeightedList(SEC);
            WeightedPac16 = CreateWeightedList(Pac12);
            WeightedND = CreateWeightedList(new[] { 68 });
            WeightedBig16 = CreateWeightedList(Big12);

            // Independent BYU gets to recruit
            WeightedBYU = TeamAndConferences[16] == IndId ? CreateWeightedList(new[] { BYUId }) : new List<int>();
            WeightedLSU = CreateWeightedList(new[] { LSUId });

            if (CincyId.IsP5())
            {
                WeightedCincy = CreateWeightedList(new[] { CincyId });
            }
            else
            {
                WeightedCincy = CreateWeightedList(new int[0]);
            }

            if (UCFId.IsP5())
            {
                WeightedUCF = CreateWeightedList(new[] { UCFId });
            }
            else
            {
                WeightedUCF = CreateWeightedList(new int[0]);
            }

            if (USFId.IsP5())
            {
                WeightedUSF = CreateWeightedList(new[] { USFId });
            }
            else
            {
                WeightedUSF = CreateWeightedList(Array.Empty<int>());
            }
        }

        static List<int> CreateWeightedList(int[] teams, int modifier = 1)
        {
            var list = new List<int>();
            foreach (var team in teams)
            {
                // var weight = PrestigeMap[team]/modifier;
                var weight = PrestigeMap[team] * PrestigeMap[team];
                for (int i = 0; i < weight; i++)
                {
                    list.Add(team);
                }
            }

            return list;
        }

        private static Dictionary<int, int> prestigeMap;
        public static Dictionary<int, int> PrestigeMap
        {
            get
            {
                if (prestigeMap == null)
                {
                    var table = FindTable("TEAM");
                    prestigeMap = table.lRecords.ToDictionary(mr => mr.TeamId(), mr => mr.Prestige());
                }

                return prestigeMap;
            }
        }

        public static bool IsTeamInPower5(this Dictionary<int, int> teams, int team)
        {
            if (team != 1023)
            {
                var conf = teams[team];
                if (conf == ACCId || conf == Big12Id || conf == Big10Id || conf == SECId || conf == Pac16Id)
                    return true;
            }

            return false;
        }

        #endregion
    }
}