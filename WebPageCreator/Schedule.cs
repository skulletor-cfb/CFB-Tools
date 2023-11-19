using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EA_DB_Editor
{
    [DataContract]
    public class TopGames
    {
        [DataMember]
        public TopGame[] NonConferenceGames { get; set; }

        [DataMember]
        public TopGame[] ConferenceGames { get; set; }

        [DataMember]
        public TopGame[] KickoffGames { get; set; }
    }
    [DataContract]
    public class TopGame
    {
        [DataMember(EmitDefaultValue = false)]
        public bool IsNeutral { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SiteName { get; set; }

        public bool IsConferenceGame { get; set; }

        [DataMember]
        public int HomeTeamId { get; set; }

        [DataMember]
        public int AwayTeamId { get; set; }

        [DataMember]
        public string HomeTeam { get; set; }

        [DataMember]
        public string AwayTeam { get; set; }

        [DataMember]
        public int HomeRank { get; set; }

        [DataMember]
        public int AwayRank { get; set; }

        [DataMember]
        public int Week { get; set; }

        [DataMember]
        public int Day { get; set; }

        [DataMember]
        public string Time { get; set; }

        public int Score { get; set; }
    }

    public class NeutralSiteComparerer : IEqualityComparer<NeutralSiteGame>
    {
        public bool Equals(NeutralSiteGame x, NeutralSiteGame y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(NeutralSiteGame obj)
        {
            return obj.GetHashCode();
        }
    }


    public class NeutralSiteGame
    {
        private int? gameIdOverride;
        public int[] Games { get; set; }
        public int Id
        {
            get
            {
                return this.GetHashCode();
            }
        }

        public static NeutralSiteGame Create(string group, Dictionary<int,int> fwd)
        {
            int[] g = null;
            if (group.StartsWith("{"))
            {
                g = group.Substring(1, group.Length - 2).Split(';').Select(i => Convert.ToInt32(i)).OrderBy(i=>i).ToArray();
            }
            else
            {
                g = new[] { Convert.ToInt32(group) };
            }

            var game= new NeutralSiteGame { Games = g };

            if (fwd.ContainsKey(game.Id))
            {
                game.gameIdOverride = fwd[game.Id];
            }

            return game; 
        }

        public override bool Equals(object obj)
        {
            var other = obj as NeutralSiteGame;
            return other.Games.Length == this.Games.Length &&
                other.Games.SequenceEqual(this.Games);
        }

        public override int GetHashCode()
        {
            if (this.gameIdOverride.HasValue)
                return this.gameIdOverride.Value;

            unchecked
            {
                var hc = Games[0];

                for (int i = 1; i < Games.Length; i++)
                {
                    hc *= Games[i];
                }

                return hc;
            }
        }

        public bool Contains(int siteId)
        {
            return this.Games.Contains(siteId);
        }
    }

    [DataContract]
    public class ScheduledGame
    {
        public static Dictionary<int, int> GameIdFwder = ConfigurationManager.AppSettings["GameIdFwd"].Split(',').ToDictionary(s => s.Split('=').First().ToInt32(), s => s.Split('=')[1].ToInt32());
        public static HashSet<NeutralSiteGame> KickOffGames = new HashSet<NeutralSiteGame>(ConfigurationManager.AppSettings["NeutralSiteGamesForRecords"].Split(',').Select(i => NeutralSiteGame.Create(i, GameIdFwder)));
        public static Dictionary<string, ScheduledGame> Schedule;

        public static bool IsSeasonOver(MaddenDatabase db)
        {
            // check to see if the season is still going on
            // BUGBUG if the NCG has been played, but the week hasn't advanced this info is incorrect, but that's probably ok
            var record = db.lTables[161].lRecords.OrderByDescending(mr => mr.lEntries[12].Data.ToInt32()).Take(1).First();

            // if the score is 0-0 the season is not over
            return !(record.lEntries[1].Data.ToInt32() == 0 && record.lEntries[2].Data.ToInt32() == 0);
        }

        #region opening week helper
        public static bool IsRivalryGame(int homeTeam, int awayTeam)
        {
            var rivalries = new List<int[]>();

            // su-temple
            rivalries.Add(new[] { 88, 90 });


            // ru-uconn
            rivalries.Add(new[] { 80, 100 });

            // gt-gsu
            rivalries.Add(new[] { 31, 233 });

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
            rivalries.Add(new[] { 60, 61 });

            // GaSo-App St
            rivalries.Add(new[] { 901, 902 });

            // Tulane-USM
            rivalries.Add(new[] { 96, 85 });

            // UTEP-NMSU
            rivalries.Add(new[] { 61, 105 });

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
            rivalries.Add(new[] { 230, 229 });

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
            rivalries.Add(new[] { 100, 88 });

            // usm-ecu
            rivalries.Add(new[] { 25, 85 });

            // marshall-ecu
            rivalries.Add(new[] { 25, 46 });

            // fau-trou
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

            // ecu-ncsu
            rivalries.Add(new[] { 25, 63 });

            // ecu-duke
            rivalries.Add(new[] { 25, 24 });

            // ecu-unc
            rivalries.Add(new[] { 25, 62 });

            // ecu-wf
            rivalries.Add(new[] { 25, 109 });

            // odu-gs
            rivalries.Add(new[] { 234, 901 });

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

            // army st-navy
            rivalries.Add(new[] { 8, 57 });

            // army st-ru
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

            // GS - GSU
            rivalries.Add(new[] { 233, 902 });

            // GS - App St
            rivalries.Add(new[] { 901, 902 });



            return rivalries.Where(r => (r[0] == homeTeam && r[1] == awayTeam) || (r[1] == homeTeam && r[0] == awayTeam)).Any();
        }

        public static bool IsNotreDameGame(int homeTeam, int awayTeam)
        {
            return homeTeam == 68 || awayTeam == 68;
        }
        #endregion
        public static void CreateOpeningWeek(MaddenDatabase db, bool isPreseason)
        {
            var power5 = new HashSet<int>(new[] { 0, 1, 2, 10, 11 });
            List<TopGame> games = new List<TopGame>();

            Create(db, isPreseason);
            foreach (var game in Schedule.Values.Where(g => g.HomeTeamId != 1023 && g.Week <= 2).OrderBy(g => g.Week).ThenBy(g => g.GameDay).ThenBy(g => g.TimeOfDay))
            {
                // non neutral site games 
                if (!game.IsClassicGame)
                {
                    if (game.Week > 0)
                    {
                        if ((game.Week == 1 && game.GameDay != 0) || game.Week > 1)
                        {
                            continue;
                        }
                    }

                    // not a p5 game and not a rivalry game
                    if ((!power5.Contains(game.HomeTeam.ConferenceId) || !power5.Contains(game.AwayTeam.ConferenceId)) && !IsRivalryGame(game.HomeTeamId, game.AwayTeamId) && !IsNotreDameGame(game.HomeTeamId, game.AwayTeamId))
                    {
                        continue;
                    }
                }

                var time = TimeSpan.FromMinutes(game.TimeOfDay);
                var ampm = time.Hours > 12 ? "PM" : "AM";
                var hr = time.Hours > 12 ? time.Hours - 12 : time.Hours;
                var tod = string.Format("{0}:{1} {2}", hr, time.Minutes < 10 ? "0" + time.Minutes : time.Minutes.ToString(), ampm);

                var tg = new TopGame
                {
                    SiteName = game.GameSite,
                    HomeRank = game.HomeTeam.MediaPollRank,
                    AwayRank = game.AwayTeam.MediaPollRank,
                    HomeTeam = game.HomeTeam.Name,
                    AwayTeam = game.AwayTeam.Name,
                    HomeTeamId = game.HomeTeamId.GetRealTeamId(),
                    AwayTeamId = game.AwayTeamId.GetRealTeamId(),
                    IsConferenceGame = game.HomeTeam.ConferenceId == game.AwayTeam.ConferenceId,
                    IsNeutral = game.IsClassicGame,
                    Week = game.Week + 1,
                    Day = game.GameDay,
                    Time = tod,
                };

                games.Add(tg);
            }

            var topGames = new TopGames
            {
                KickoffGames = games.ToArray()
            };

            topGames.ToJsonFile(@".\Archive\Reports\kickoffweek");
            PocketScout.OpeningWeek();
        }

        public static void CreateTopGames(MaddenDatabase db, bool isPreseason)
        {
            List<TopGame> games = new List<TopGame>();

            Create(db, isPreseason);
            foreach (var game in Schedule.Values)
            {
                if (game.HomeTeamId == 1023)
                    continue;

                var tg = new TopGame
                {
                    SiteName = game.GameSite,
                    HomeRank = game.HomeTeam.MediaPollRank,
                    AwayRank = game.AwayTeam.MediaPollRank,
                    HomeTeam = game.HomeTeam.Name,
                    AwayTeam = game.AwayTeam.Name,
                    HomeTeamId = game.HomeTeamId.GetRealTeamId(),
                    AwayTeamId = game.AwayTeamId.GetRealTeamId(),
                    IsConferenceGame = game.HomeTeam.ConferenceId == game.AwayTeam.ConferenceId,
                    IsNeutral = game.IsClassicGame,
                    Week = game.Week + 1,
                };

                // score is the compositve ranks of the teams / 2.  If they are main rivals we subtract 5 points
                tg.Score = game.HomeTeam.MediaPollRank + game.HomeTeam.CoachesPollRank + game.AwayTeam.CoachesPollRank + game.AwayTeam.MediaPollRank;
                tg.Score /= 2;
                tg.Score += (game.HomeTeam.MainRival == game.AwayTeam.MainRival ? -10 : 0);
                tg.Score += (game.HomeTeam.DivisionId == game.AwayTeam.DivisionId && game.HomeTeam.DivisionId != 30) ? -5 : 0;

                games.Add(tg);
            }

            var topGames = new TopGames
            {
                NonConferenceGames = games.Where(g => g.IsConferenceGame == false && g.Week < 16).OrderBy(g => g.Score).Take(15).ToArray(),
                ConferenceGames = games.Where(g => g.IsConferenceGame == true && g.Week < 16).OrderBy(g => g.Score).Take(25).ToArray(),
            };

            topGames.ToJsonFile(@".\Archive\Reports\topgames");
            PocketScout.TopGames();
        }

        public static string StadiumNickNameOverrides = ConfigurationManager.AppSettings["StadiumNickNameOverrides"];

        public static Dictionary<int, (int id, string name)> ClassicGames = new Dictionary<int, (int id, string name)>
        {

        };

        public static Func<ScheduledGame, bool>[] ClassicGameEvaluators = new Func<ScheduledGame, bool>[]       
        {
            IsAllstateCrossbarClassic,
            IsJohnnyMajorsClassic,
            IsShamrockSeries,
            IsOysterBowl,
            IsErikSimpsonCFBClassic,
            IsMayhemAtMBS,
            IsEddieRobinsonClassic,
        };

        public static string SiteIdSuffix(ScheduledGame g)
        {
            return SiteIdSuffix(g.StadiumId);
        }

        public static string SiteIdSuffix(int siteId)
        {
            var stadiumTable = Form1.MainForm.maddenDB.lTables.Where(tbl => tbl.Abbreviation == "STAD").SingleOrDefault();
            var stadium = stadiumTable.lRecords.Where(record => record["SGID"].ToInt32() == siteId).SingleOrDefault();
            return stadium.lEntries[56].Data;
        }

        private static bool IsPigskinClassic(ScheduledGame g)
        {
            const int id = 71041024;

            if(g.SiteId == id)
            {
                g.GameSite += " " + SiteIdSuffix(g);
            }

            return false;
        }

        private static bool IsMayhemAtMBS(ScheduledGame g)
        {
            const int id = 247431733;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 19) + 33;
            if (g.TimeOfDay == tod && g.HomeTeamId == 31)
            {
                g.SiteId = id;
                g.GameSite = "Mayhem at MBS";
                return true;
            }

            return false;
        }

        private static bool IsOysterBowl(ScheduledGame g)
        {
            const int id = 473234717;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 19) + 17;
            if (g.TimeOfDay == tod && g.HomeTeamId == 234)
            {
                g.SiteId = id;
                g.GameSite = "Oyster Bowl";
                return true;
            }

            return false;
        }

        private static bool IsShamrockSeries(ScheduledGame g)
        {
            const int id = 247568807;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 20) + 7;
            if (g.TimeOfDay == tod && (g.HomeTeamId == 68 || g.AwayTeamId==68) && g.Week > 3)
            {
                g.GameSite = $"Shamrock Series ({SiteIdSuffix(g)})";
                g.SiteId = id;
                return true;
            }
            
            return false;
        }

        private static bool IsJohnnyMajorsClassic(ScheduledGame g)
        {
            const int id = 24907791;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 19) + 37;
            var teams = new HashSet<int>() { 77, 91 };
            if (g.TimeOfDay == tod && teams.Contains(g.HomeTeamId) && teams.Contains(g.AwayTeamId))
            {
                g.GameSite = $"Johnny Majors Classic ({SiteIdSuffix(g)})";
                g.SiteId = id;
                return true;
            }

            return false;
        }

        private static bool IsEddieRobinsonClassic(ScheduledGame g)
        {
            const int id = 24861407;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 16) + 7;
            if (g.TimeOfDay == tod && g.Week == 0)
            {
                g.GameSite = $"Eddie Robinson Classic ({SiteIdSuffix(g)})";
                g.SiteId = id;
                return true;
            }

            return false;
        }

        private static bool IsErikSimpsonCFBClassic(ScheduledGame g)
        {
            const int id = 24732831;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 20) + 31;
            if (g.TimeOfDay == tod && g.Week == 2)
            {
                g.GameSite = $"Erik Simpson College Football Classic ({SiteIdSuffix(g)})";
                g.SiteId = id;
                return true;
            }

            return false;
        }


        private static bool IsAllstateCrossbarClassic(ScheduledGame g)
        {
            const int id = 24721827;
            KickOffGames.Add(new NeutralSiteGame { Games = new[] { id } });
            var tod = (60 * 20) + 27;
            if (g.TimeOfDay == tod && g.Week == 1)
            {
                g.GameSite = $"Allstate Crossbar Classic ({SiteIdSuffix(g)})";
                g.SiteId = id;
                return true;
            }

            return false;
        }

        public int StadiumId { get; set; }

        public static void Create(MaddenDatabase db, bool isPreseason)
        {
            if (Schedule != null)
                return;

            SchoolRecord.Create(db);
            Schedule = new Dictionary<string, ScheduledGame>();
            var table = db.lTables[161];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var game = new ScheduledGame
                {
                    PostSeason = table.lRecords[i].lEntries[0].Data.ToInt32(),
                    AwayScore = table.lRecords[i].lEntries[1].Data.ToInt32(),
                    HomeScore = table.lRecords[i].lEntries[2].Data.ToInt32(),
                    AwayTeamId = table.lRecords[i].lEntries[6].Data.ToInt32().GetRealTeamId(),
                    HomeTeamId = table.lRecords[i].lEntries[7].Data.ToInt32().GetRealTeamId(),
                    DynastySeason = table.lRecords[i].lEntries[8].Data.ToInt32(),
                    GameNumber = table.lRecords[i].lEntries[11].Data.ToInt32(),
                    Week = table.lRecords[i].lEntries[12].Data.ToInt32(),
                    Year = table.lRecords[i].lEntries[13].Data.ToInt32(),
                    WentToOvertime = table.lRecords[i].lEntries[15].Data.ToInt32(),
                    GameDay = table.lRecords[i]["GDAT"].ToInt32(),
                    TimeOfDay = table.lRecords[i]["GTOD"].ToInt32(),
                    StadiumId = table.lRecords[i]["SGID"].ToInt32(),
                };

                // stadium id
                var stadiumId =table.lRecords[i].lEntries[3].Data.ToInt32();

                // 1023 stadium id means it's probably a game that hasn't been filled in like a bowl or a Conf Champ Game
                if (stadiumId == 1023)
                    continue;

                // find the STAD table and the stadium
                var stadiumTable = db.lTables.Where(tbl => tbl.Abbreviation == "STAD").SingleOrDefault();
                var stadium = stadiumTable.lRecords.Where(record => record["SGID"].ToInt32() == stadiumId).SingleOrDefault();
                game.GameSite = stadium.lEntries[56].Data;

                TeamSchedule homeTeamSchedule;
                TeamSchedule awayTeamSchedule;

                // check to see if we have a neutral site game only for regular season games
                // don't get to set an overrides for a week with more than 1 game
                if (TeamSchedule.TeamSchedules.TryGetValue(game.HomeTeamId, out homeTeamSchedule) &&
                    TeamSchedule.TeamSchedules.TryGetValue(game.AwayTeamId, out awayTeamSchedule) &&
                    game.Week < 16 &&
                    homeTeamSchedule[game.Week].Count == 1 &&
                    awayTeamSchedule[game.Week].Count == 1)
                {
                    // both teams are marked as home means its a neutral site game
                    if (ClassicGameEvaluators.Any(e => e(game)))
                    {
                        game.IsClassicGame = true;
                    }
                    else if (homeTeamSchedule[game.Week][0].IsHomeGame && awayTeamSchedule[game.Week][0].IsHomeGame)
                    {
                        game.IsNeutralSite = true;
                        // check to see if we have an override
                        var overrides = StadiumNickNameOverrides;
                        //we have an override, a set of comma delimited settings seperated by semi colon
                        if (overrides != null)
                        {
                            var sections = overrides.Split(';').Where(section => string.IsNullOrWhiteSpace(section) == false).ToArray();
                            var overridenNickNames = sections.Select(s => s.Split(',').ToDictionary(str => str.Split('=')[0], right => right.Split('=')[1])).ToList();
                            var currentNicknames = overridenNickNames.Where(s => s["Stadium"] == stadiumId.ToString() && game.Week < s["BeforeWeek"].ToInt32()).ToList();

                            Dictionary<string, string> stadiumOverride = currentNicknames.Count == 1 ? currentNicknames[0] : null;

                            if (stadiumOverride == null && currentNicknames.Count > 1)
                            {
                                stadiumOverride = currentNicknames.Where(s => s.TryGetValue("RivalryGame", out var value) && value.Contains(Math.Min(game.HomeTeamId, game.AwayTeamId).ToString() + "-" + Math.Max(game.HomeTeamId, game.AwayTeamId).ToString())).FirstOrDefault();
                            }

                            if (stadiumOverride != null)
                            {
                                NeutralSiteGame koGame = null;
                                game.GameSite = stadiumOverride["NickName"];
                                koGame = KickOffGames.Where(nsg => nsg.Contains(stadiumId)).FirstOrDefault();

                                if (koGame != null)
                                {
                                    game.SiteId = koGame.Id;
                                    const int PigskinClassicKickoff = 71041024;

                                    if (koGame.Id == PigskinClassicKickoff)
                                    {
                                        game.GameSite += $" ({ScheduledGame.SiteIdSuffix(stadiumId)})";
                                    }
                                }
                                else
                                {
                                    game.SiteId = stadiumId;
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(stadium["STNN"]) == false)
                            {
                                game.GameSite = stadium["STNN"];
                                game.SiteId = stadiumId;
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(stadium["STNN"]) == false)
                        {
                            game.GameSite = stadium["STNN"];
                            game.SiteId = stadiumId;
                        }
                    }
                }

                Schedule.Add(game.Key, game);
            }

            // do the scoring summary for each game now
            table = db.lTables[6];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(5);
                var weekNumber = record.GetInt(6);
                var game = Schedule[CreateKey(weekNumber, gameNumber)];

                // add the item to the list
                game.Scores.Add(new GameScore
                {
                    TeamId = record.GetInt(0),
                    Time = record.GetUShort(4),
                    Quarter = record.GetUShort(8),
                    Points = record.GetUShort(9),
                    ScoreType = record.GetUShort(13)
                });

                game.Scores.Last().Parse();
            }

            // box score data for each game
            table = db.lTables[7];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(1);
                var weekNumber = record.GetInt(2);
                var teamId = record.GetInt(0).GetRealTeamId();
                var game = Schedule[CreateKey(weekNumber, gameNumber)];

                //create the box score
                var boxScore = new TeamStat
                {
                    TeamId = teamId,
                    TwoPointConversionAttempts = record.GetInt(3),
                    Turnovers = record.GetInt(4),
                    PassAttempts = record.GetInt(5),
                    RushAttempts = record.GetInt(6),
                    TwoPointConversions = record.GetInt(7),
                    ThirdDownConversions = record.GetInt(8),
                    FourthDownConversions = record.GetInt(9),
                    PuntYards = record.GetInt(14),
                    Penalties = record.GetInt(15),
                    RedZoneFG = record.GetInt(16),
                    IntThrown = record.GetInt(17),
                    PassCompletions = record.GetInt(10),
                    FirstDowns = record.GetInt(11),
                    ThirdDownAttempts = record.GetInt(12),
                    FourthDownAttempts = record.GetInt(13),
                    FumblesLost = record.GetInt(18),
                    PassYards = record.GetInt(19),
                    KRYards = record.GetInt(20),
                    RushTD = record.GetInt(26),
                    OffensiveYards = record.GetInt(30),
                    PRYards = record.GetInt(22),
                    PassTD = record.GetInt(23),
                    RedZoneTD = record.GetInt(24),
                    TimeOfPossesion = record.GetInt(25),
                    Punts = record.GetInt(27),
                    PenaltyYards = record.GetInt(28),
                    TotalYards = record.GetInt(29),
                    RedZoneVisits = record.GetInt(31)
                };

                boxScore.RushYards = (int)((short)record.GetInt(21));

                if (teamId == game.HomeTeamId)
                {
                    game.HomeTeamBoxScore = boxScore;
                }
                else
                {
                    game.AwayTeamBoxScore = boxScore;
                }
            }

            // offensive stats
            table = db.lTables[4];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(2);
                var weekNumber = record.GetInt(3);
                var playerId = record.GetInt(0);
                var game = Schedule[CreateKey(weekNumber, gameNumber)];

                // create the offensive stats for the player
                var player = new PlayerStats { PlayerId = playerId };

                var passingYards = record.GetSignedInt(8, 4096);
                var rushingYards = record.GetSignedInt(10, 2048);
                var rececptions = record.GetInt(6);
                var passTD = record.GetInt(13);
                var rushTD = record.GetInt(15);
                var recTD = record.GetInt(14);
                var receivingYrds = record.GetSignedInt(9, 2048);

                // gaya
                player[PlayerStats.PassingYards] = passingYards;

                // gaat
                player[PlayerStats.PassAttempts] = record.GetInt(22);

                // gacm
                player[PlayerStats.Completions] = record.GetInt(17);

                // gatd
                player[PlayerStats.PassingTD] = passTD;

                // gain
                player[PlayerStats.IntThrown] = record.GetInt(18);

                // guat
                player[PlayerStats.RushAttempts] = record.GetInt(23);

                // guya
                player[PlayerStats.RushingYards] = rushingYards;

                // gutd
                player[PlayerStats.RushingTD] = rushTD;

                // gctd
                player[PlayerStats.ReceivingTD] = recTD;

                // gcca
                player[PlayerStats.Receptions] = rececptions;

                // gcya
                player[PlayerStats.ReceivingYards] = receivingYrds;

                player[PlayerStats.LongestPass] = record["galN"].ToInt32();
                player[PlayerStats.LongestReception] = record["gcrL"].ToInt32();
                player[PlayerStats.LongestRush] = record["gulN"].ToInt32();

                player.GameKey = game.Key;
                game.GamePlayerStats.Add(playerId, player);
                PlayerStats.OffensiveGamePerformances.Add(player);

                if (player.Player != null)
                {
                    TeamRecord.SetNewRecord(TeamRecordKeys.PassTD, passTD, player.Player,game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RushingTD, rushTD, player.Player,game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RecTD, recTD, player.Player,game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.PassYds, passingYards, player.Player,game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RushingYds, rushingYards, player.Player,game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.RecYds, receivingYrds, player.Player,game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.Receptions, rececptions, player.Player, game);
                }
            }

            // defensive stats
            table = db.lTables[1];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record.GetInt(1);
                var weekNumber = record.GetInt(2);
                var playerId = record.GetInt(0);
                var game = Schedule[CreateKey(weekNumber, gameNumber)];

                // make sure the player is in the dictionary
                PlayerStats player;
                if (!game.GamePlayerStats.TryGetValue(playerId, out player))
                {
                    player = new PlayerStats { PlayerId = playerId };
                    player.GameKey = game.Key;
                    game.GamePlayerStats.Add(playerId, player);
                }

                var sacks = record.GetInt(8);
                var halfSacks = record.GetInt(13);
                var ints = record.GetInt(11);

                // add the defensive stats
                // gdta
                player[PlayerStats.Tackles] = record.GetInt(5);

                // gdpd
                player[PlayerStats.PassDeflections] = record.GetInt(6);

                //glff
                player[PlayerStats.ForcedFumble] = record.GetInt(7);

                // glsk
                player[PlayerStats.Sacks] = sacks;

                // gdtl
                player[PlayerStats.TackleForLoss] = record.GetInt(10);

                // gsin
                player[PlayerStats.Interceptions] = ints;

                //glfr
                player[PlayerStats.FumbleRec] = record.GetInt(12);

                // glhs
                player[PlayerStats.HalfSacks] = halfSacks;

                // gdht
                player[PlayerStats.AssistedTackles] = record.GetInt(15);

                player[PlayerStats.LongIntRet] = record["gslR"].ToInt32();
                player[PlayerStats.IntRetYds] = record["gsiy"].ToInt32().GetSignedInt(512);
                player[PlayerStats.FumRecYds] = record["glfy"].ToInt32().GetSignedInt(512);
                player[PlayerStats.IntReturnedForTD] = record["gsit"].ToInt32();
                player[PlayerStats.FumblesReturnedForTD] = record["glft"].ToInt32();

                if (player.Player != null)
                {
                    var totalsacks = sacks + (halfSacks > 0 ? (1 + halfSacks / 2) : 0);

                    TeamRecord.SetNewRecord(TeamRecordKeys.Sacks, totalsacks, player.Player, game);
                    TeamRecord.SetNewRecord(TeamRecordKeys.INT, ints, player.Player, game);
                }
            }

            // return starts
            table = db.lTables[3];
            for (int i = 0; i < table.Table.currecords; i++)
            {
                var record = table.lRecords[i];
                var gameNumber = record["SGNM"].ToInt32();
                var weekNumber = record["SEWN"].ToInt32();
                var playerId = record["PGID"].ToInt32();
                var game = Schedule[CreateKey(weekNumber, gameNumber)];

                // return game stats
                PlayerStats player;
                if (!game.GamePlayerStats.TryGetValue(playerId, out player))
                {
                    player = new PlayerStats { PlayerId = playerId };
                    player.GameKey = game.Key;
                    game.GamePlayerStats.Add(playerId, player);
                }

                player[PlayerStats.KickReturns] = record["grka"].ToInt32();
                player[PlayerStats.KRTD] = record["grkt"].ToInt32();
                player[PlayerStats.KRYds] = record.GetSignedInt(9, 2048);
                player[PlayerStats.LongestKR] = record["grkL"].ToInt32();

                player[PlayerStats.PuntReturns] = record["grpa"].ToInt32();
                player[PlayerStats.PRTD] = record["grpt"].ToInt32();
                player[PlayerStats.PRYds] = record.GetSignedInt(10, 2048);
                player[PlayerStats.LongestPR] = record["grpL"].ToInt32();
            }
        }

        public int PostSeason { get; set; }
        [DataMember]
        public string GameSite { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsNeutralSite { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsClassicGame { get; set; }

        [DataMember]
        public int AwayScore { get; set; }
        [DataMember]
        public int HomeScore { get; set; }
        [DataMember]
        public int AwayTeamId { get; set; }
        [DataMember]
        public int HomeTeamId { get; set; }
        [DataMember]
        public string HomeTeamName { get { return HomeTeam.Name; } set { } }
        [DataMember]
        public string AwayTeamName { get { return AwayTeam.Name; } set { } }
        [DataMember]
        public string HomeTeamMascot { get { return HomeTeam.Mascot; } set { } }
        [DataMember]
        public string AwayTeamMascot { get { return AwayTeam.Mascot; } set { } }

        public bool GameHasRecordBook => IsNeutralSite || IsClassicGame;

        public Team WinningTeam
        {
            get
            {
                return this.AwayScore > this.HomeScore ? this.AwayTeam : this.HomeTeam;
            }
        }
        public Team LosingTeam
        {
            get
            {
                return this.AwayScore > this.HomeScore ? this.HomeTeam : this.AwayTeam;
            }
        }
        public Team AwayTeam { get { return Team.Teams[AwayTeamId]; } }
        public Team HomeTeam { get { return Team.Teams[HomeTeamId]; } }
        public int DynastySeason { get; set; }
        public int GameNumber { get; set; }
        public int Week { get; set; }
        public int Year { get; set; }
        public int WentToOvertime { get; set; }
        public string Key { get { return CreateKey(this.Week, this.GameNumber); } }
        [DataMember]
        public List<GameScore> Scores { get; set; }
        [DataMember]
        public TeamStat HomeTeamBoxScore { get; set; }
        [DataMember]
        public TeamStat AwayTeamBoxScore { get; set; }
        [DataMember]
        public string StatsTable { get; set; }

        [DataMember]
        public int GameDay { get; set; }

        [DataMember]
        public int TimeOfDay { get; set; }
        public Dictionary<int, PlayerStats> GamePlayerStats { get; set; }

        public ScheduledGame()
        {
            this.Scores = new List<GameScore>();
            this.GamePlayerStats = new Dictionary<int, PlayerStats>();
        }

        private static string CreateKey(int week, int num)
        {
            return string.Format("{0}-{1}", week, num);
        }

        public void ToJson()
        {
            var a = this.Scores.Where(s => s.Quarter <= 4).OrderBy(s => s.Quarter).ThenByDescending(s => s.Time).ToArray();
            var b = this.Scores.Where(s => s.Quarter > 4).OrderBy(s => s.Quarter).ThenBy(s => s.Time).ToArray();
            this.Scores = new List<GameScore>();
            this.Scores.AddRange(a);
            this.Scores.AddRange(b);
            var fileName = @".\Archive\Reports\box" + this.Key;
            this.StatsTable = this.CreateStatsTable(null);
            //            this.ToJsonFile(fileName);
            //          this.CreateStatsTable(fileName);
        }

        public void WriteToTable(StringBuilder sb, PlayerStats p, string[] keys, int tableIndex)
        {
            sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                p.Player.Number, p.Player.Name, p.Player.CalculateYear(0), p.Player.PositionName, p.Player.Height.CalculateHgt(), p.Player.Weight,
                p.GetIntStringValue(keys[0]), p.GetIntStringValue(keys[1]), p.GetIntStringValue(keys[2]), p.GetIntStringValue(keys[3]),
                p.GetIntStringValue(keys[4]), p.GetIntStringValue(keys[5]), p.GetIntStringValue(keys[6]), p.Player.TeamId, tableIndex));
        }

        public string CreateStatsTable(string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("No,Name,PlayerClass,Position,Height,Weight,Stat1,Stat2,Stat3,Stat4,Stat5,Stat6,Stat7,TeamId,TableId");

            var passStats = GamePlayerStats.Values.Where(p => p.GetIntValue(PlayerStats.PassAttempts) > 0 && PlayerDB.Players.ContainsKey(p.PlayerId)).OrderByDescending(p => p.Player.TeamId) //sort by team
                .ThenByDescending(p => p.GetIntValue(PlayerStats.PassingYards))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.PassAttempts));// get passers
            foreach (var p in passStats)
            {
                // home team tables are 0,1,2,3 and road team are 4,5,6,7
                var idx = p.Player.TeamId == this.HomeTeamId ? 0 : 4;
                WriteToTable(sb, p, new string[] { PlayerStats.Completions, PlayerStats.PassAttempts, PlayerStats.PassingYards, PlayerStats.PassingTD, PlayerStats.IntThrown, null, null }, idx);
            }

            var rushStats = GamePlayerStats.Values.Where(p => p.GetIntValue(PlayerStats.RushAttempts) > 0 && PlayerDB.Players.ContainsKey(p.PlayerId)).OrderByDescending(p => p.Player.TeamId) //sort by team
                .ThenByDescending(p => p.GetIntValue(PlayerStats.RushingYards))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.RushAttempts)); // get rushers
            foreach (var rs in rushStats)
            {
                var idx = rs.Player.TeamId == this.HomeTeamId ? 1 : 5;
                WriteToTable(sb, rs, new string[] { PlayerStats.RushAttempts, PlayerStats.RushingYards, PlayerStats.RushingTD, null, null, null, null }, idx);
            }

            var recStats = GamePlayerStats.Values.Where(p => p.GetIntValue(PlayerStats.Receptions) > 0 && PlayerDB.Players.ContainsKey(p.PlayerId)).OrderByDescending(p => p.Player.TeamId) //sort by team
                .ThenByDescending(p => p.GetIntValue(PlayerStats.Receptions))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.ReceivingYards)); // get receptions

            foreach (var rec in recStats)
            {
                var idx = rec.Player.TeamId == this.HomeTeamId ? 2 : 6;
                WriteToTable(sb, rec, new string[] { PlayerStats.Receptions, PlayerStats.ReceivingYards, PlayerStats.ReceivingTD, null, null, null, null }, idx);
            }

            var defStats = GamePlayerStats.Values
                .Where(p => PlayerDB.Players.ContainsKey(p.PlayerId) && (p.GetIntValue(PlayerStats.Tackles) > 0 || p.GetIntValue(PlayerStats.Interceptions) > 0 || p.GetIntValue(PlayerStats.Sacks) > 0 ||
                    p.GetIntValue(PlayerStats.HalfSacks) > 0 || p.GetIntValue(PlayerStats.PassDeflections) > 0 || p.GetIntValue(PlayerStats.ForcedFumble) > 0 ||
                    p.GetIntValue(PlayerStats.AssistedTackles) > 0))
                .OrderByDescending(p => p.Player.TeamId)
                .ThenByDescending(p => p.GetIntValue(PlayerStats.Tackles) + p.GetIntValue(PlayerStats.AssistedTackles))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.TackleForLoss))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.Sacks) + p.GetIntValue(PlayerStats.HalfSacks))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.Interceptions))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.PassDeflections))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.ForcedFumble))
                .ThenByDescending(p => p.GetIntValue(PlayerStats.FumbleRec));

            foreach (var d in defStats)
            {
                var idx = d.Player.TeamId == this.HomeTeamId ? 3 : 7;
                d[PlayerStats.TotalTackles] = d.GetIntValue(PlayerStats.Tackles) + d.GetIntValue(PlayerStats.AssistedTackles);
                d["RealSacks"] = d.GetIntValue(PlayerStats.Sacks) * 10 + d.GetIntValue(PlayerStats.HalfSacks) * 5;
                WriteToTable(sb, d, new string[] { PlayerStats.TotalTackles, PlayerStats.TackleForLoss, "RealSacks", PlayerStats.Interceptions, PlayerStats.PassDeflections, PlayerStats.ForcedFumble, PlayerStats.FumbleRec }, idx);
            }


            return sb.ToString();
            File.AppendAllText(file, sb.ToString());
            //File.WriteAllText(@".\archive\reports\box" + this.Key + ".csv", sb.ToString());
        }

        public bool IsPlayoffGame(int highestRank)
        {
            return this.HomeTeam.BCSPrevious <= highestRank && this.AwayTeam.BCSPrevious <= highestRank && this.HomeTeam.BCSPrevious >= 1 && this.AwayTeam.BCSPrevious >= 1;
        }
    }

    [DataContract]
    public class WeeklySchedule
    {
        private List<ScheduledGame> games = new List<ScheduledGame>();

        [DataMember]
        public ScheduledGame[] Games { get; set; }

        public void Add(ScheduledGame game)
        {
            games.Add(game);
        }

        public void Prepare()
        {
            var ordered = games.OrderBy(g => g.GameNumber).ToArray();
            this.Games = new ScheduledGame[ordered.Last().GameNumber + 1];
            for (int i = 0; i < ordered.Length; i++)
            {
                if (ordered[i] == null)
                    continue;

                this.Games[ordered[i].GameNumber] = ordered[i];
            }
        }
    }
}