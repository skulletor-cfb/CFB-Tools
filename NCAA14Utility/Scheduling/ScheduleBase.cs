using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCAA14Utility.Scheduling
{
    public abstract class ScheduleBase
    {
        #region constants
        // ACC (16)

        public const int BC = 13; // Boston College

        public const int Clemson = 21; // Clemson

        public const int Duke = 27; // Duke

        public const int FSU = 38; // Florida State

        public const int GT = 43; // Georgia Tech

        public const int UL = 60; // Louisville

        public const int UMD = 63; // Maryland

        public const int Miami = 65; // Miami

        public const int NCSU = 75; // NC State

        public const int UNC = 80; // North Carolina

        public const int Pitt = 95; // Pittsburgh

        public const int SU = 108; // Syracuse

        public const int UVA = 133; // Virginia

        public const int VT = 134; // Virginia Tech

        public const int WF = 135; // Wake Forest

        public const int WVU = 138; // West Virginia



        // American (12)

        public const int Charlotte = 19; // Charlotte

        public const int ECU = 29; // East Carolina

        public const int Houston = 45; // Houston

        public const int Memphis = 64; // Memphis

        public const int Rice = 97; // Rice

        public const int SMU = 103; // SMU

        public const int Temple = 110; // Temple

        public const int Tulane = 118; // Tulane

        public const int Tulsa = 119; // Tulsa

        public const int UAB = 120; // UAB

        public const int UCF = 121; // UCF

        public const int USF = 127; // USF



        // Big 12 (12)

        public const int Baylor = 11; // Baylor

        public const int Cincy = 20; // Cincinnati

        public const int Colorado = 23; // Colorado

        public const int ISU = 49; // Iowa State

        public const int KU = 52; // Kansas

        public const int KSU = 53; // Kansas State

        public const int Nebraska = 76; // Nebraska

        public const int Oklahoma = 88; // Oklahoma

        public const int OkSt = 89; // Oklahoma State

        public const int TCU = 109; // TCU

        public const int Texas = 112; // Texas

        public const int TT = 115; // Texas Tech



        // Big Ten (12)

        public const int Illinois = 46; // Illinois

        public const int Indiana = 47; // Indiana

        public const int Iowa = 48; // Iowa

        public const int Michigan = 67; // Michigan

        public const int MichiganSt = 68; // Michigan State

        public const int Minnesota = 70; // Minnesota

        public const int Northwestern = 84; // Northwestern

        public const int OhioSt = 87; // Ohio State

        public const int PennSt = 94; // Penn State

        public const int Purdue = 96; // Purdue

        public const int Rutgers = 98; // Rutgers

        public const int Wisconsin = 141; // Wisconsin



        // CUSA (9)

        public const int Army = 8; // Army

        public const int FAU = 37; // FLA Atlantic

        public const int LT = 59; // Louisiana Tech

        public const int MTSU = 69; // MTSU

        public const int Navy = 74; // Navy

        public const int NT = 82; // North Texas

        public const int UTEP = 130; // UTEP

        public const int UTSA = 131; // UTSA

        public const int WKU = 139; // W. Kentucky



        // Independent (13)

        public const int Delaware = 26; // Delaware

        public const int FIU = 35; // FIU

        public const int JaxSt = 50; // Jax State

        public const int KennesawSt = 54; // Kennesaw St.

        public const int Liberty = 57; // Liberty

        public const int UMass = 124; // Massachusetts

        public const int MissouriSt = 73; // Missouri State

        public const int NDSU = 81; // NDSU

        public const int NMSU = 79; // New Mexico St.

        public const int NotreDame = 85; // Notre Dame

        public const int SacSt = 99; // Sac State

        public const int SHSU = 100; // Sam Houston

        public const int UConn = 25; // UConn



        // MAC (12)

        public const int Akron = 1; // Akron

        public const int BallSt = 10; // Ball State

        public const int BGSU = 14; // Bowling Green

        public const int Buffalo = 15; // Buffalo

        public const int CMU = 18; // C. Michigan

        public const int EMU = 28; // E. Michigan

        public const int KentSt = 55; // Kent State

        public const int MiamiOH = 66; // Miami (OH)

        public const int NIU = 83; // NIU

        public const int Ohio = 86; // Ohio

        public const int Toledo = 116; // Toledo

        public const int WMU = 140; // W. Michigan



        // MW (12)

        public const int AF = 0; // Air Force

        public const int BSU = 12; // Boise State

        public const int CSU = 24; // Colorado State

        public const int FresnoSt = 39; // Fresno State

        public const int Hawaii = 44; // Hawai'i

        public const int Nevada = 77; // Nevada

        public const int UNM = 78; // New Mexico

        public const int SDSU = 101; // San Diego St.

        public const int SJSU = 102; // San Jose State

        public const int UNLV = 125; // UNLV

        public const int UtahSt = 129; // Utah State

        public const int Wyoming = 142; // Wyoming



        // Pac-12 (12)

        public const int Arizona = 4; // Arizona

        public const int ASU = 5; // Arizona State

        public const int BYU = 16; // BYU

        public const int Cal = 17; // California

        public const int Oregon = 92; // Oregon

        public const int OrSt = 93; // Oregon State

        public const int Stanford = 107; // Stanford

        public const int UCLA = 122; // UCLA

        public const int USC = 126; // USC

        public const int Utah = 128; // Utah

        public const int Washington = 136; // Washington

        public const int WSU = 137; // Washington St.



        // SEC (14)

        public const int Alabama = 2; // Alabama

        public const int Arkansas = 6; // Arkansas

        public const int Auburn = 9; // Auburn

        public const int UF = 36; // Florida

        public const int UGA = 40; // Georgia

        public const int UK = 56; // Kentucky

        public const int LSU = 61; // LSU

        public const int MissSt = 71; // Mississippi St

        public const int Missouri = 72; // Missouri

        public const int OleMiss = 91; // Ole Miss

        public const int SCAR = 105; // South Carolina

        public const int Tennessee = 111; // Tennessee

        public const int TAMU = 113; // Texas A&M

        public const int Vandy = 132; // Vanderbilt



        // Sun Belt (14)

        public const int AppSt = 3; // App St.

        public const int ArkansasSt = 7; // Arkansas State

        public const int CCU = 22; // C. Carolina

        public const int GASO = 41; // Ga Southern

        public const int GSU = 42; // Georgia State

        public const int JMU = 51; // James Madison

        public const int ULL = 58; // Louisiana

        public const int Marshall = 62; // Marshall

        public const int ODU = 90; // Old Dominion

        public const int USA = 104; // South Alabama

        public const int USM = 106; // Southern Miss

        public const int TexasSt = 114; // Texas State

        public const int Troy = 117; // Troy

        public const int ULM = 123; // UL Monroe



        // FCS / no conference (5)

        public const int FCSEast = 30; // FCS East

        public const int FCSMidwest = 31; // FCS Midwest

        public const int FCSNorthwest = 32; // FCS Northwest

        public const int FCSSoutheast = 33; // FCS Southeast

        public const int FCSWest = 34; // FCS West
        #endregion

        public Dictionary<int, HashSet<int>> ConferenceSchedule { get; private set; }
        public Dictionary<int, int[]> ScenarioForSeason { get; private set; }
        protected abstract Func<Dictionary<int, int[]>>[] Creators { get; }

        protected virtual bool Initialized { get; set; }
        protected int Year { get; }
        protected int TeamCount { get; }
        protected int ConferenceId { get; }
        protected string ConferenceName { get; }
        protected abstract int InitialYear { get; }

        protected ScheduleBase(int year, int teamCount, int conferenceId, string conferenceName)
        {
            this.Year = year;
            this.TeamCount = teamCount;
            this.ConferenceId = conferenceId;
            this.ConferenceName = conferenceName;
        }

        public void Init()
        {
            if (!this.Initialized)
            {
                this.Initialized = true;
                var idx = (this.Year - this.InitialYear) % Creators.Length;
                this.ScenarioForSeason = Creators[idx]();
                this.ScenarioForSeason.Verify(this.TeamCount, this.ConferenceId, this.ConferenceName);
                this.ConferenceSchedule = this.ScenarioForSeason.BuildHashSet();
            }
        }

        public bool IsEvenYear => this.Year % 2 == 0;

        /// <summary>
        /// Whether a game must be played in a particular week: true with that week in <paramref name="expectedWeek"/>
        /// (its current week when it just mustn't move), false when it can go anywhere.
        /// </summary>
        public bool IsLocked(int team1, int team2, int currentWeek, out int expectedWeek) =>
            this.IsGameLocked(team1, team2, currentWeek, out expectedWeek);

        /// <summary>The locks every conference shares (<see cref="IsSharedLock"/>); a conference adds its own by overriding.</summary>
        protected virtual bool IsGameLocked(int team1, int team2, int currentWeek, out int expectedWeek) =>
            IsSharedLock(team1, team2, this.IsEvenYear, currentWeek, out expectedWeek);

        /// <summary>Rivalries that never move from the week the game schedules them.</summary>
        private static readonly (int, int)[] FixedRivalries =
        {
            (Navy, NotreDame), (Army, AF), (AF, Navy),
            (FSU, UF), (UGA, GT), (Clemson, SCAR), (UL, UK),
        };

        public const int ArmyNavyWeek = 13;

        /// <summary>
        /// Army-Navy in week 13 and the fixed rivalries where they are, plus Notre Dame's: it travels to Stanford (even
        /// years) or USC (odd years) in week 13 and hosts the other in week 7.
        /// </summary>
        public static bool IsSharedLock(int team1, int team2, bool evenYear, int currentWeek, out int expectedWeek)
        {
            bool Is(int a, int b) => (team1 == a && team2 == b) || (team1 == b && team2 == a);

            expectedWeek = currentWeek;
            if (Is(Army, Navy))
            {
                expectedWeek = ArmyNavyWeek;
                return true;
            }

            if (FixedRivalries.Any(r => Is(r.Item1, r.Item2)))
            {
                return true;
            }

            if (Is(NotreDame, Stanford) || Is(NotreDame, USC))
            {
                bool roadGame = Is(NotreDame, evenYear ? Stanford : USC);
                expectedWeek = roadGame ? NotreDameRoadRivalryWeek : NotreDameHomeRivalryWeek;
                return true;
            }

            expectedWeek = -1;
            return false;
        }

        /// <summary>The host of a Notre Dame-Stanford or Notre Dame-USC game for the year, or -1 for any other game.</summary>
        public static int NotreDameRivalryHost(int team1, int team2, bool evenYear)
        {
            int other = team1 == NotreDame ? team2 : team2 == NotreDame ? team1 : -1;
            if (other != Stanford && other != USC)
            {
                return -1;
            }

            // the road game is the one played in week 13
            return other == (evenYear ? Stanford : USC) ? other : NotreDame;
        }

        public const int NotreDameRoadRivalryWeek = 13;
        public const int NotreDameHomeRivalryWeek = 7;
    }
}