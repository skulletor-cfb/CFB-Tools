using EA_DB_Editor.CAPGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/*
Recruit Id = PRSI
Recruit Rank = RCRK
recruit Position Rank = RCPR
Star rating = rccb
Recruit Position = RPGP
Player State = STAT
Player Tendency = PTEN
qb=0,hb=1,wr=3,te=4,t=5,g=6,c=7,de=8,dt=9,olb=10,mlb=11,cb=12,fs=13,ss=14,ath=18

speed = PSPD
Strength  = PSTR
Agility = PAGI
acc - PACC
Awareness = PAWR
Break Tackle = PBTK
Trucking = PTRK
Elusiveness = PESV 
Ball Carrier Vision = PBCV
Stiff Arm = PSAR
Spin Move = PSMV
Juke = PJMV
Carry = PCAR
Catch = PCTH
Spec Catch = SPCT
Catch in Traffic = TRAF
Route running = PRTR
Jump = PJMP
Throw Power = PTHP
Throw Acc = PTHA
Tackle - PTAK
Hit Power = PHIT
Power Move = PPMV
Finesse Move = PFMV
Block Shedding - PBSH
Pursuit = PPRS
Play Rec = PPRC
Man Cov = PMCV
Zone Cov = PZCV
Press = PYRS
Release = RELS
Pass Block = PPBK
Pass Blk Foot - PPBF
Pass Block Str = PPBS
Run Block = PRBK
Run Block Foot = PBFW
Run Block Str = PRBS
Impact Blocking = PIBL
Kick Ret = PKRT
stamina - PSTA
Injury - PINJ
*/
namespace EA_DB_Editor
{
    public static class RecruitingFixup
    {
        // should be -1 if we haven't added any CAPs
        public static int DontChange =5;
        const int P5Cutoff = 300;

        public static Random RAND = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0));
        public static void Fix(bool fixPoints)
        {
            Check();
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");
            var recruitPitchTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPR");

            foreach (var record in recruitTable.lRecords)
            {
                Fixup(recruitPitchTable, recruitTable, record, fixPoints);
            }
        }

        public static bool PreseasonFixupRun { get; set; }

        public static List<MaddenRecord> PreseasonFixup()
        {
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");

            // find the last 5 guys in texas and cali
            int texasId = 42;
            int caliId = 4;
            int floridaId = 8;
            var positionGroups = new int[] { 1, 2, 3, 4, 0, 1, 5, 6, 7, 0, 8, 0, 9, 10, 11, 12, 13, 14 };
            var recruitPitchTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPR");
            // select 25 random positions:  5 from Texas, 15 from SEC country, 5 elsewhere, 5 from florida
            int[] positionsToLookFor = new int[41];
            for (int i = 0; i < positionsToLookFor.Length; i++)
            {
                positionsToLookFor[i] = positionGroups[RAND.Next(0, positionGroups.Length)];
            }

#if false // you want the last guy in the list to be here
            var l = new List<int>(positionsToLookFor);
            l.Add(0);
            positionsToLookFor = l.ToArray();
#endif

            var nationalTake = positionsToLookFor.Length - 31;
            // get the lowest ranked freshman at the position we have selected
            var caliRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && positionsToLookFor.Take(5).Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == caliId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var texasRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && positionsToLookFor.Skip(5).Take(5).Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == texasId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var floridaRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && positionsToLookFor.Skip(10).Take(5).Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == floridaId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var secRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && positionsToLookFor.Skip(15).Take(20).Contains(r["RPGP"].ToInt32()) && SECStates.Contains(r["STAT"].ToInt32())).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var nationalRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && positionsToLookFor.Skip(35).Take(nationalTake).Contains(r["RPGP"].ToInt32()) && SECStates.Concat(new[] { texasId, caliId }).Contains(r["STAT"].ToInt32()) == false).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var athRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && r["RPGP"].ToInt32() == 18).OrderByDescending(r => r["RCRK"].ToInt32()).Take(3).ToArray();
            MaddenRecord[] hawaiiRecruits = new MaddenRecord[0];

            if ((DateTime.UtcNow.Ticks % 5) > 1)
            {
                hawaiiRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && positionsToLookFor.Skip(positionsToLookFor.Length - 1).Take(1).Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == 10).OrderByDescending(r => r["RCRK"].ToInt32()).Take(2000).ToArray();
            }

            List<MaddenRecord> records = new List<MaddenRecord>();
            FillList(records, caliRecruits, positionsToLookFor.Take(5).ToArray());
            FillList(records, texasRecruits, positionsToLookFor.Skip(5).Take(5).ToArray());
            FillList(records, floridaRecruits, positionsToLookFor.Skip(10).Take(5).ToArray());
            FillList(records, secRecruits, positionsToLookFor.Skip(15).Take(20).ToArray());
            FillList(records, nationalRecruits, positionsToLookFor.Skip(35).Take(nationalTake).ToArray());
            FillList(records, hawaiiRecruits, positionsToLookFor.Skip(positionsToLookFor.Length - 1).Take(1).ToArray());

            // ATH recruits get a 5-20 adder to each stat
            foreach (var ath in athRecruits)
            {
                foreach (var rating in Player.RatingMap.Keys)
                {
                    var curr = ath[rating].ToInt32();
                    curr+=RAND.Next(5, 21);
                    if (curr > 99)
                        curr = 99;
                    ath[rating] = curr.ToString();
                    SetStatesForRecruit(recruitPitchTable, ath);
                }
            }

            // we have identified our players fix each one up
            for (int i = 0; i < records.Count; i++)
            {
                var player = records[i];
                // we have our new recruit
                var cap = CAPGen.CAPGen.GeneratePlayer(player["RPGP"].ToInt32(), player["PTEN"].ToInt32());
                cap.CreatePlayer();

                foreach (var key in Player.RatingMap.Keys)
                {
                    player[key] = cap.GetRating(key, player[key].ToInt32()).ToString();
                    if (cap.TendencyOverride != null)
                        player["PTEN"] = cap.TendencyOverride;
                    SetStatesForRecruit(recruitPitchTable, player);
                }
            }

            // foreach player we will compare RCOV to other players RCOV and move him up the charts
            PreseasonFixupRun = true;
            records.AddRange(athRecruits);
            return records;
        }

        static void FillList(List<MaddenRecord> records, MaddenRecord[] playersToSearch, int[] positionsToSearch)
        {
            List<MaddenRecord> players = new List<MaddenRecord>(playersToSearch);
            for (int i = 0; i < positionsToSearch.Length; i++)
            {
                int playerToRemove = -1;
                for (int j = 0; j < players.Count; j++)
                {
                    if (positionsToSearch[i] == players[j]["RPGP"].ToInt32())
                    {
                        playerToRemove = j;
                        records.Add(players[j]);
                        break;
                    }
                }

                if (playerToRemove >= 0 && playerToRemove < players.Count)
                {
                    players.RemoveAt(playerToRemove);
                }
            }
        }

        static int ToInt32(this string s)
        {
            return Convert.ToInt32(s);
        }

        static void SetStatesForRecruit(MaddenTable pitchTable, MaddenRecord recruitInfo)
        {
            var recruit = FindRecruit(pitchTable, recruitInfo["PRSI"].ToInt32());
            var state = recruitInfo["STAT"].ToInt32();
            var pitches = new[] { 1, 3, 4, 5, 6, 8, 9, 12, 13 };
            var selected = new List<int>();
            while (selected.Count < 3)
            {
                var p = pitches[RAND.Next(0, pitches.Length)];
                if (selected.Contains(p) == false)
                    selected.Add(p);
            }

            if(pitches.Contains(recruitInfo["IMP1"].ToInt32())==false)recruitInfo["IMP1"] = selected[0].ToString();
            if (pitches.Contains(recruitInfo["IMP2"].ToInt32()) == false) recruitInfo["IMP2"] = selected[1].ToString();
            if (pitches.Contains(recruitInfo["IMP3"].ToInt32()) == false) recruitInfo["IMP3"] = selected[2].ToString();

            // get the states
            var top5 = new int[10];
            for (int i = 0; i < top5.Length;)
            {
                var newState = ConfStateAssignments.Value[state][RAND.Next(0, ConfStateAssignments.Value[state].Length)];
                if (top5.Contains(newState) == false)
                {
                    top5[i] = newState;
                    i++;
                }
            }

            //set PT01 = PT10
            for (int i = 1; i <= top5.Length; i++)
            {
                var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                recruit[key] = top5[i - 1].ToString();
            }
        }

        //static Dictionary<int, int[]> StateAssignments = CreateStateAssignments();

        // florida gets a little extra weight
        static int[] SECStates = new int[] { 0, 3, 8, 9, 16, 17, 24, 23, 39, 41, 42 };

        const int NotreDameId = 68;
        const int BYUId = 16;
        const int CincyId = 20;
        const int UCFId = 18;

        static Lazy<Dictionary<int, int[]>> ConfStateAssignments = new Lazy<Dictionary<int, int[]>>(CreateConferenceAssignmentsForStates, true);

        static Dictionary<int, int[]> CreateConferenceAssignmentsForStates()
        {
            int[] allConf = new int[] { ACCId, Big12Id, Big10Id, Pac16Id, SECId, NotreDameId };
            #region state stuff
            var dict = new Dictionary<int, int[]>();
            dict.Add(0, new int[] { SECId }); //AL
            dict.Add(1,allConf); //AK
            dict.Add(2, new int[] { Pac16Id, BYUId }); //AZ
            dict.Add(3, new int[] { SECId }); //AR
            dict.Add(4, new int[] { Pac16Id , NotreDameId, Pac16Id, NotreDameId,BYUId }); //CA
            dict.Add(5, new int[] { TeamAndConferences[22], NotreDameId, BYUId }); //CO
            dict.Add(6, new int[] { ACCId,Big10Id , NotreDameId}); //CT
            dict.Add(7, new int[] { ACCId, Big10Id ,NotreDameId}); //DE
            dict.Add(8, new int[] { SECId,ACCId , SECId, ACCId, UCFId }); //FL
            dict.Add(9, new int[] { SECId, ACCId }); //GA
            dict.Add(10, new int[] { Pac16Id, NotreDameId  }); //HI
            dict.Add(11, new int[] { Pac16Id, BYUId, Big12Id }); //ID
            dict.Add(12, new int[] { Big10Id,NotreDameId }); //IL
            dict.Add(13, new int[] { Big10Id, NotreDameId }); //IN
            dict.Add(14, new int[] { Big10Id, NotreDameId,Big12Id }); //IA
            dict.Add(15, new int[] { Big12Id }); //KS
            dict.Add(16, new int[] { TeamAndConferences[44], SECId}); //KY
            dict.Add(17, new int[] { SECId }); //LA
            dict.Add(18, new int[] { ACCId,Big10Id,NotreDameId }); //ME
            dict.Add(19, new int[] {  ACCId,  NotreDameId }); //MD
            dict.Add(20, new int[] { ACCId, NotreDameId }); //MA
            dict.Add(21, new int[] { Big10Id,NotreDameId, CincyId }); //MI
            dict.Add(22, new int[] { Big10Id,NotreDameId }); //MN
            dict.Add(23, new int[] { SECId }); //MS
            dict.Add(24, new int[] { SECId,Big12Id}); //MO
            dict.Add(25, new int[] { Pac16Id,NotreDameId}); //MT
            dict.Add(26, new int[] { Big12Id }); //NE
            dict.Add(27, new int[] { Pac16Id,NotreDameId, BYUId }); //NV
            dict.Add(28, new int[] { ACCId,Big10Id,NotreDameId }); //NH
            dict.Add(29, new int[] { ACCId, Big10Id, NotreDameId }); //NJ
            dict.Add(30, new int[] { Pac16Id,Big12Id,NotreDameId }); //NM
            dict.Add(31, new int[] { ACCId, Big10Id, NotreDameId }); //NY
            dict.Add(32, new int[] { ACCId }); //NC
            dict.Add(33, new int[] { Pac16Id,NotreDameId, BYUId }); //ND
            dict.Add(34, new int[] { Big10Id,NotreDameId, Big10Id, NotreDameId, Big10Id, NotreDameId, CincyId }); //OH
            dict.Add(35, new int[] { Big12Id }); //OK
            dict.Add(36, new int[] { Pac16Id }); //OR
            dict.Add(37, new int[] { ACCId,Big10Id,NotreDameId, CincyId }); //PA
            dict.Add(38, new int[] { ACCId, Big10Id, NotreDameId }); //RI
            dict.Add(39, new int[] { ACCId,SECId }); //SC
            dict.Add(40, new int[] { Pac16Id,Big12Id,NotreDameId, BYUId }); //SD
            dict.Add(41, new int[] { SECId , SECId}); //TN
            dict.Add(42, new int[] { Big12Id,SECId,NotreDameId }); //TX
            dict.Add(43, new int[] { Pac16Id, NotreDameId , BYUId }); //UT
            dict.Add(44, new int[] { ACCId, Big10Id, NotreDameId }); //VT
            dict.Add(45, new int[] { ACCId,  NotreDameId }); //VA
            dict.Add(46, new int[] { Pac16Id }); //WA
            dict.Add(47, new int[] {ACCId }); //WV
            dict.Add(49, allConf); //WY
            dict.Add(48, new int[] { Big10Id,NotreDameId }); //WI
            dict.Add(50, allConf); //CN
            dict.Add(51, allConf); //DC
            #endregion

            var teams  = new Dictionary<int,int[]>();
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
                        default:
                            break; 
                    }
                }

                teams[kvp.Key] = allTeams.ToArray();
            }

            return teams;
        }

#if false
        static Dictionary<int, int[]> CreateStateAssignments()
        {
        #region state stuff
            var dict = new Dictionary<int, int[]>();
            dict.Add(0, new int[] { 3, 3, 3, 3, 3, 8, 8, 8, 45, 30, 27 }); //AL
            dict.Add(1, new int[] { 110, 111, 75, 75, 12 }); //AK
            dict.Add(2, new int[] { 4, 4, 4, 4, 5, 5, 5, 5, 92, 102, 99, 87, 17 }); //AZ
            dict.Add(3, new int[] { 6, 6, 6, 6, 45, 45, 92, 71, 56, 6 }); //AR
            dict.Add(4, new int[] { 102, 99, 87, 17, 102, 99, 87, 17, 102, 99, 22, 87, 17, 102, 99, 22, 87, 17, 12, 4, 5, 58, 58, 58, 68, 68, 74, 75, 111, 110 }); //CA
            dict.Add(5, new int[] { 22,22,22, 22, 22, 22,22,22,22,16,39,40,38,72,71,58,  103 }); //CO
            dict.Add(6, new int[] { 68, 68, 68, 76, 77, 70 }); //CT
            dict.Add(7, new int[] { 68, 76, 77, 70, 80 }); //DE
            dict.Add(8, new int[] { 27, 28, 49, 27, 28, 49, 27, 28, 49, 3, 30, 91, 9, 44 }); //FL
            dict.Add(9, new int[] { 30, 30, 30, 30, 30, 30, 91, 44, 27, 28, 49, 31 }); //GA
            dict.Add(10, new int[] { 102, 102, 102, 87, 16, 99 }); //HI
            dict.Add(11, new int[] { 12, 110, 111, 74, 75, 103, 104 }); //ID
            dict.Add(12, new int[] { 68, 35, 37, 114, 68, 58 }); //IL
            dict.Add(13, new int[] { 68, 68, 68, 51, 52, 70, 35 }); //IN
            dict.Add(14, new int[] { 37, 38, 58, 39, 40 }); //IA
            dict.Add(15, new int[] { 39, 40, 58, 71, 72, 22, }); //KS
            dict.Add(16, new int[] { 44, 44, 44, 42, 112, 70, 91 ,48,42}); //KY
            dict.Add(17, new int[] { 45, 45, 45, 45, 45, 45, 45, 55, 73, 6, 56, 92, 3, 8 }); //LA
            dict.Add(18, new int[] { 13, 80, 76, 77, 68 }); //ME
            dict.Add(19, new int[] { 47, 76, 77, 80, 68, 112, 107, 108 }); //MD
            dict.Add(20, new int[] { 13, 77, 76, 68, 68, 77, 76 ,80}); //MA
            dict.Add(21, new int[] { 51, 51, 52, 70, 68, 76 }); //MI
            dict.Add(22, new int[] { 54, 114, 70, 35, 37, 51, 52, 58 }); //MN
            dict.Add(23, new int[] { 91, 73, 55, 3, 9, 45, 6 }); //MS
            dict.Add(24, new int[] { 6, 56, 45, 3, 84, 27, 58, 92, 93, 91 }); //MO
            dict.Add(25, new int[] { 12, 16, 110, 111, 103, 54 }); //MT
            dict.Add(26, new int[] { 58 }); //NE
            dict.Add(27, new int[] { 59, 101, 4, 5, 102, 87, 99, 17, 103, 104, 16, 74, 75, 110, 111, 22, 23 }); //NV
            dict.Add(28, new int[] { 68, 76, 77, 80, 88 }); //NH
            dict.Add(29, new int[] { 68, 76, 77, 80, 88 }); //NJ
            dict.Add(30, new int[] { 92, 93, 4, 5, 22, 23, 103, 104 }); //NM
            dict.Add(31, new int[] { 80, 88, 76, 77, 68 }); //NY
            dict.Add(32, new int[] { 84, 30, 62, 63, 45, 30, 108, 44, 91, 112, 3, 9 }); //NC
            dict.Add(33, new int[] { 54, 58, 37, 22, 12 }); //ND
            dict.Add(34, new int[] { 70, 70, 70, 70, 70, 70, 70, 70, 70, 51, 51, 51, 52, 76, 44,  68 }); //OH
            dict.Add(35, new int[] { 71, 71, 71, 71, 71, 71, 92, 92, 93, 72, 72, 58, 58 }); //OK
            dict.Add(36, new int[] { 75, 74, 102, 87, 17, 99, 110, 111, 12 }); //OR
            dict.Add(37, new int[] { 76, 77, 68, 70, 76, 77, 68, 70, 112, 80, 88, 47 }); //PA
            dict.Add(38, new int[] { 68, 13, 80, 88, 76, 77 }); //RI
            dict.Add(39, new int[] { 84, 84, 84, 84, 84, 84, 84, 84, 84, 84, 91, 3, 30, 31, 27, 28, 49, 21, 21, 21 }); //SC
            dict.Add(40, new int[] { 54, 58, 37, 22, 12 }); //SD
            dict.Add(41, new int[] { 91, 91, 91, 91, 91, 91, 91, 28, 91, 49, 91, 91, 6, 91, 42, 44, 3, 3, 3, 9, 84, 27 }); //TN
            dict.Add(42, new int[] { 92, 92, 92, 92, 92, 92, 71, 71, 71, 71, 71, 71, 45, 45, 93, 93, 93, 6, 6, 72, 58, 58, 58, 58, 11, 89, 94, 33}); //TX
            dict.Add(43, new int[] { 103, 104, 22, 102, 74, 75, 16, 99, 87, 17 }); //UT
            dict.Add(44, new int[] { 68, 76, 77, 47, 80, 88 }); //VT
            dict.Add(45, new int[] { 108, 108, 108, 108, 107, 112, 47, 42, 44, 70, 76, 77, 62, 63, 84, 91 }); //VA
            dict.Add(46, new int[] { 110, 111, 12, 74, 75, 16, 102, 87, 17, 99, 22 }); //WA
            dict.Add(47, new int[] { 112, 112, 112, 108, 107, 76, 70, 42, 44, 77 }); //WV
            dict.Add(49, new int[] { 12, 22, 23, 103, 104, 110, 111, 74, 75, 58 }); //WY
            dict.Add(48, new int[] { 114, 114, 114, 68, 35, 54, 37 }); //WI
            dict.Add(50, new int[] { 3, 9, 70, 51, 27, 28, 49, 45, 92, 71, 84, 30, 91, 76 }); //CN
            dict.Add(51, new int[] { 47, 108, 107, 76, 77, 112, 27, 28, 49, 30, 91, 3, 9, 51, 68 }); //DC
            return dict;
        #endregion
        }
#endif

        static void Fixup(MaddenTable pitchTable, MaddenTable recruitTable, MaddenRecord recruitInfo, bool fixPoints)
        {
            int recruitId = 0;
            int recruitRank = 0;
            bool ptChanged = false;

            foreach (var entry in recruitInfo.lEntries)
            {
                if (entry.field.Abbreviation == "PRSI")
                {
                    recruitId = Int32.Parse(entry.Data);
                }
                else if (entry.field.Abbreviation == "RCRK")
                {
                    recruitRank = Int32.Parse(entry.Data);
                }
            }

            if(recruitId < DontChange) { return; }

            List<int> teams = new List<int>();
            List<int> subs = new List<int>();

            // we want to make sure the top 750 commit just to P5 schools
            if (recruitRank < 1500)
            {
                // PT01-PT10 are the top teams
                // PTCM is the committed team
                var recruit = FindRecruit(pitchTable, recruitId);

                if (recruitId > DontChange)
                {
                    ChangeRecruitFace(recruitTable, recruitId);
                }

                // for whatever reason this recruit hasn't been recruited, this is 4* and 5*
                if (Int32.Parse(recruit["PS01"]) < 5000 && recruitRank < 350 && fixPoints)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        var key = i == 10 ? "PS10" : "PS0" + i.ToString();
                        recruit[key] = "9999";
                    }
                }


                // recruit has not committed
                if (recruit["PTCM"] == "1023" && (recruitRank < 1000))// || (IsInterestedInServiceAcademy(recruit) && recruitRank < 1500)))
                {
                    // first get the teams
                    for (int i = 1; i <= 10; i++)
                    {
                        var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                        var teamId = Int32.Parse(recruit[key]);
                        if (IsP5(teamId) || OnTheirOwn.Contains(teamId))
                        {
                            teams.Add(teamId);
                        }
                        else if (DontFoolWith.Contains(teamId) && recruitRank > P5Cutoff)
                        {
                            teams.Add(teamId);
                        }
                    }

                    // now get replacements for them
                    for (int i = 1; i <= 10; i++)
                    {
                        var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                        var teamId = Int32.Parse(recruit[key]);
                        if (IsP5(teamId) == false && !OnTheirOwn.Contains(teamId) && ShouldReplaceForAAC(recruitRank, teamId))
                        {
                            ptChanged = true;
                            subs.Add(GetReplacement(teamId, teams, recruit["RCCB"].ToInt32(), recruitInfo["STAT"].ToInt32()));
                        }
                    }

                    teams.AddRange(subs);

                    // write the teams back
                    if (ptChanged)
                    {
                        for (int i = 1; i <= 10; i++)
                        {
                            var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                            recruit[key] = teams[i - 1].ToString();
                        }
                    }
                }
                else if (!IsP5(Int32.Parse(recruit["PTCM"])) && recruitRank <= P5Cutoff && !OnTheirOwn.Contains(recruit["PTCM"].ToInt32()))
                {
                    // mark the top 250 recruits that committed to non P5 schools
                    recruit["PTCM"] = "9999";
                }
                else if (recruitRank < 500 && !IsP5(Int32.Parse(recruit["PTCM"])) && !OnTheirOwn.Contains(recruit["PTCM"].ToInt32()) && !DontFoolWith.Contains(recruit["PTCM"].ToInt32()))
                {
                    // mark the top 500 recruits that committed to non P5 schools, may go AAC
                    recruit["PTCM"] = "9999";
                }
            }
        }

        static bool ShouldReplaceForAAC(int rank, int teamId)
        {
            if (rank <= P5Cutoff)
                return true;

            return  !DontFoolWith.Contains(teamId);
        }

        static HashSet<int> changedFaces = new HashSet<int>();

        public static void ChangeRecruitFace(MaddenTable recruitTable, int recruitId)
        {
            if (!changedFaces.Add(recruitId)) return;

            foreach (var recruit in recruitTable.lRecords.Where(r => r["PRSI"].ToInt32() > RecruitingFixup.DontChange))
            {
                foreach (var entry in recruit.lEntries)
                {
                    if (entry.field.Abbreviation == "PRSI")
                    {
                        if (Int32.Parse(entry.Data) == recruitId)
                        {
                            int position = Int32.Parse(recruit["PPOS"]);
                            var face = Int32.Parse(recruit["PGHE"]);

                            if (face < 100)
                            {
                                // line players players have 35% of changing
                                if (linePlayers.Contains(position) && RAND.Next(0, 100) < 35)
                                {
                                    recruit["PGHE"] = RAND.Next(160, 240).ToString();
                                }
                                else if ((position == 1 || position == 16) && RAND.Next(0, 100) < 98)
                                {
                                    // CB/HB have a 98% of changing
                                    recruit["PGHE"] = RAND.Next(160, 221).ToString();
                                }
                                else if (position == 3 && RAND.Next(0, 10) < 5)
                                {
                                    // WR have a 50% of changing
                                    recruit["PGHE"] = RAND.Next(160, 221).ToString();
                                }
                                else if (skillPlayers.Contains(position) && RAND.Next(0, 100) < 60)
                                {
                                    recruit["PGHE"] = RAND.Next(160, 221).ToString();
                                }
                                else if (recruit["RATH"] == "1" && skillPlayers.Contains(position))
                                {
                                    recruit["PGHE"] = RAND.Next(160, 240).ToString();
                                }
                            }

                            return;
                        }
                    }
                }
            }
        }

        static int[] skillPlayers = { 1, 13, 14, 15, 16, 17, 18 };
        static int[] linePlayers = {  5, 6, 7, 8, 9, 10, 11, 12 };

        static MaddenRecord FindRecruit(MaddenTable pitchTable, int recruitId)
        {
            foreach (var recruit in pitchTable.lRecords)
            {
                foreach (var entry in recruit.lEntries)
                {
                    if (entry.field.Abbreviation == "PRSI")
                    {
                        if (Int32.Parse(entry.Data) == recruitId)
                        {
                            return recruit;
                        }
                    }
                }
            }

            throw new Exception("bad data");
        }

        public static bool IsIndependentG5(this int teamId)
        {
            return TeamAndConferences[teamId] == IndId && teamId != 16 && teamId != 68 ;
        }

       public const int ACCId = 0;
        public const int AmericanId = 3;
    public    const int Big12Id = 2;
        public const int Big16Id = 200;
    public    const int Big10Id = 1;
        public const int CUSAId = 4;
        public const int MACId = 7;
        public const int MWCId = 9;
        public const int Pac16Id = 10;
        public  const int SECId = 11;
        public const int SBCId = 13;
        public const int IndId = 5;
        private static Dictionary<int, int> teamAndConferences;
        private static Dictionary<int, int> teamAndDivisions;
        private static Dictionary<int, string> teamNames;

        public static string ToConferenceName(this string id)
        {
            return id.ToInt32().ToConferenceName();
        }

        public static string ToConferenceName(this int id)
        {
            return id == 17 ?
                "At Large" :
                ConferenceNames[id];
        }

        public static Dictionary<int, string> ConferenceNames = new Dictionary<int, string>()
        {
            {0,"ACC" },
            {3,"American" },
            {2,"Big 12" },
            {1,"Big 10" },
            {4,"CUSA" },
            {7,"MAC" },
            {9,"MWC" },
            {10,"Pac 12" },
            {11,"SEC" },
            {13,"Sun Belt" },
            {14,"WAC" },
            {5,"FBS IND" },
        };

        public static Dictionary<int, int> TeamAndConferences
        {
            get
            {
                if (teamAndConferences == null || teamAndConferences.Count == 0)
                {
                    try
                    {
                        teamAndConferences = Form1.MainForm.maddenDB.lTables[167].lRecords
                            .Where( mr=> mr.lEntries[40].Data.ToInt32()!=611&& mr.lEntries[40].Data.ToInt32() != 300)
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

        public static Dictionary<int, int> TeamAndDivision
        {
            get
            {
                if (teamAndDivisions == null || teamAndDivisions.Count == 0)
                {
                    try
                    {
                        teamAndDivisions = Form1.MainForm.maddenDB.lTables[167].lRecords
                            .Where(mr => mr.lEntries[40].Data.ToInt32() != 611 && mr.lEntries[37].ToInt32()!=30&& mr.lEntries[40].Data.ToInt32() != 300)
                            .ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record.lEntries[37].Data.ToInt32());
                    }
                    catch
                    {
                        teamAndDivisions = new Dictionary<int, int>();
                    }
                }

                return teamAndDivisions;
            }
        }

        private static int? accTeams = null;

        public static int SunBeltTeams => TeamAndConferences.Values.Count(v => v == SBCId);

        public static int AccTeams
        {
            get
            {
                if (accTeams.HasValue == false)
                {
                    accTeams = TeamAndConferences.Values.Count(v => v == ACCId);
                }

                return accTeams.Value;
            }
        }

        static Dictionary<int, string> teamAbbreviations;
        public static Dictionary<int, string> TeamAbbreviations
        {
            get
            {
                if (teamAbbreviations == null || teamAbbreviations.Count == 0)
                {
                    try
                    {
                        teamAbbreviations = Form1.MainForm.maddenDB.lTables[167].lRecords.ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record["TSNA"]);
                    }
                    catch
                    {
                        teamAbbreviations = new Dictionary<int, string>();
                    }
                }

                return teamAbbreviations;
            }
        }


        public static Dictionary<int, string> TeamNames
        {
            get
            {
                if (teamNames == null || teamNames.Count == 0)
                {
                    try
                    {
                        teamNames = Form1.MainForm.maddenDB.lTables[167].lRecords.ToDictionary(mr => mr.lEntries[40].Data.ToInt32(), record => record["TDNA"]);
                    }
                    catch
                    {
                        teamNames = new Dictionary<int, string>();
                    }
                }

                return teamNames;
            }
        }


        public static bool TeamsInSameConference(this Dictionary<int, int> teams, int a, int b)
        {
            return teams[b] == teams[a];
        }

        public static bool TeamsEligbleForReplacement(this Dictionary<int, int> teams, int home, int away)
        {
            if (ScheduleFixup.IsNotreDameGame(home, away) || OnTheirOwn.Contains(home) || OnTheirOwn.Contains(away))
                return false;

            if (teams.IsTeamInPower5(home) == false && teams.IsTeamInPower5(away) == false && away.IsValidTeam() && home.IsValidTeam())
            {
                return !teams.TeamsInSameConference(home, away);
            }

            return false;
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

        static Dictionary<int, int> PrestigeMap;
        public static int[] ACCConfTeams { get { return TeamAndConferences.Where(kvp => kvp.Value == ACCId).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] ACC { get { return TeamAndConferences.Where(kvp => kvp.Value == ACCId).Select(kvp => kvp.Key).Concat(new[] { 68 }).Distinct().ToArray(); } }

        public static int[] Big10 { get { return TeamAndConferences.Where(kvp => kvp.Value == Big10Id).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] Big12 { get { return TeamAndConferences.Where(kvp => kvp.Value == Big12Id).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] Pac12 { get { return TeamAndConferences.Where(kvp => kvp.Value == Pac16Id).Select(kvp => kvp.Key).ToArray(); } }
        public static int[] SEC { get { return TeamAndConferences.Where(kvp => kvp.Value == SECId).Select(kvp => kvp.Key).ToArray(); } }
     public   static int[] American { get { return TeamAndConferences.Where(kvp => kvp.Value == AmericanId).Select(kvp => kvp.Key).ToArray(); } }
     public   static int[] MAC { get { return TeamAndConferences.Where(kvp => kvp.Value == MACId).Select(kvp => kvp.Key).ToArray(); } }
     public   static int[] CUSA { get { return TeamAndConferences.Where(kvp => kvp.Value == CUSAId).Select(kvp => kvp.Key).ToArray(); } }
      public  static int[] SBC { get { return TeamAndConferences.Where(kvp => kvp.Value == SBCId).Select(kvp => kvp.Key).ToArray(); } }
     public   static int[] MWC { get { return TeamAndConferences.Where(kvp => kvp.Value == MWCId).Select(kvp => kvp.Key).ToArray(); } }

        static string[] academies = { "1", "8", "57" };
        public static int[] OnTheirOwn = TeamsOnTheirOwn();
#if false
        public static int[] DontFoolWith = new int[0];// American.ToArray();
#else
        public static int[] DontFoolWith = American.ToArray();
#endif
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

        public static int[] TeamsOnTheirOwn()
        {
            return ScheduleFixup.FindUserControllerTeams(true)
                //.Concat(new[] {  })
                .Where(team => !team.IsP5OrND()).ToArray();
        }

        public static bool IsServiceAcademy(this int teamId)
        {
            return  academies.Contains(teamId.ToString());
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
            return American.Contains(teamId) || MWC.Contains(teamId) || MAC.Contains(teamId) || SBC.Contains(teamId) || CUSA.Contains(teamId) || teamId == 57 || teamId == 8;
        }

        public static bool IsSECTeam(this int teamId)
        {
            return SEC.Contains(teamId);
        }

        public static bool IsSunBeltTeam(this int teamId) => SBC.Contains(teamId);

        public static bool IsAccTeam(this int teamId)
        {
            return ACC.Contains(teamId) && teamId != 68;
        }

        public static bool IsAmericanTeam(this int teamId)
        {
            return American.Contains(teamId);
        }

        public static bool ConferenceGameCountCheck(this int teamId, int current)
        {
            var conf = TeamAndConferences[teamId];
            var count = TeamAndConferences.Count(kvp => kvp.Value == conf && kvp.Key != 68);
            var expected = 0;

            if (conf == IndId) return true;

            if (conf == SECId && current == 10)
                return true;

            if (conf == CUSAId && current != 4)
                return false;
            else if (CUSAId == conf)
                return true;

            if (count == 12 && conf == Pac16Id)
                return current == 8;

            if (count == 16 && conf == ACCId)
                expected = 8;
            else if (count == 16 && conf == AmericanId)
                expected = 8;
            else if (count == 16 && conf == SBCId)
                expected = 8;
            else if (count >= 16)
                expected = 9;
            else if (count == 14 || count == 12 || count == 11 || count == 13)
                expected = 8;
            else if (count == 10)
                expected = 9;
            else if (count == 8)
                expected = 7;
            else if (count == 7)
                expected = 6;
            else if (count >= 4)
                expected = count - 1;

            return current == expected;
        }

        public static bool ConferenceHomeGameCount(this PreseasonScheduledGame[] schedule, int teamId)
        {
            var conf = TeamAndConferences[teamId];

            if (conf == Pac16Id) return true;
            if (conf == Big12Id && (Big12.Length == 16|| Big12.Length == 10)) return true;
            //if (conf == AmericanId && American.Length == 16) return true;
            if (conf == AmericanId && American.Length == 10) return true;
            if (conf == IndId) return true;
            if (conf == SBCId || conf==CUSAId) return true;
            if (conf == MACId && MAC.Length == 16) return true;
            if (conf == MWCId /*&& MWC.Length == 10*/) return true;
            
            //if (conf == ACCId && AccTeams > 14)
            //    return true;

            var confGames = schedule.Count(g => g != null && g.HomeTeam == teamId && teamAndConferences[g.AwayTeam] == conf);
            return confGames == 4;
        }

        public static bool TeamsInSameConference(int t1, int t2)
        {
            return TeamAndConferences[t1] == TeamAndConferences[t2];
        }

        public static bool IsP5OrND(this int teamId)
        {
            return teamId.IsP5() || teamId == 68;
        }

        static void SetWeightedArrays()
        {
            if (WeightedACC != null)
                return;

            var table = Form1.MainForm.maddenDB.lTables.Where(t => t.Abbreviation == "TEAM").First();
            PrestigeMap = table.lRecords.ToDictionary(mr => mr["TGID"].ToInt32(), mr => mr["TPRX"].ToInt32());
            WeightedACC = CreateWeightedList(ACCConfTeams);
            WeightedBig10 = CreateWeightedList(Big10);
            WeightedBig12 = CreateWeightedList(Big12);
            WeightedSEC = CreateWeightedList(SEC);
            WeightedPac16 = CreateWeightedList(Pac12);
            WeightedND = CreateWeightedList(new[] { 68 });
            WeightedBig16 = CreateWeightedList(Big12);

            // Independent BYU gets to recruit
            WeightedBYU = TeamAndConferences[16] == IndId ? CreateWeightedList(new[] { 16 }) : new List<int>();
#if false // when cincy/ucf are in teh big12
            WeightedUCF = CreateWeightedList(new[] { 18 });
            WeightedCincy = CreateWeightedList(new[] { 20 });
#else
            WeightedUCF = CreateWeightedList(new int[0]);
            WeightedCincy = CreateWeightedList(new int[0]);
#endif
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

        static int SelectFromConferences(List<int> first, List<int> second, int recruitRating)
        {
            var a = recruitRating > 3 ? first : first.Distinct().ToList();
            var b = recruitRating > 3 ? second : second.Distinct().ToList();
            var idx = RAND.Next(0, a.Count + b.Count);
            return idx < a.Count ? a[idx] : b[idx - a.Count];
        }

        static int SelectFromConferences(List<int> first,List<int> second, List<int> third, int recruitRating)
        {
            var all = first.Concat(second).Concat(third).ToList();
            all = recruitRating > 3 ? all : all.Distinct().ToList();
            var idx = RAND.Next(0, all.Count);
            return all[idx];
        }

        static int GetReplacement(int teamId, List<int> teams, int recruitRating, int state)
        {
            var dict = ConfStateAssignments.Value;
            var teamsToChooseFrom = dict[state];
            var idx = RAND.Next(0, teamsToChooseFrom.Length);
            var result = teamsToChooseFrom[idx];

            if (result == 0)
                throw new Exception("bad data");

            // if it already is in the list, get another
            if (teams.Contains(result))
            {
                return GetReplacement(teamId, teams, recruitRating, state);
            }

            return result;
        }

#if false
        static int GetReplacement(int teamId, List<int> teams, int recruitRating)
        {
            SetWeightedArrays();

            int result = 0;

#if true
            int conf = teamId == 1023 ? RAND.Next(0, 5) : -1;

            if (American.Contains(teamId) || conf == 0)
            {
                result = SelectFromConferences(WeightedACC, WeightedBig12, recruitRating);
            }
            else if (MAC.Contains(teamId) || conf == 1)
            {
                result = SelectFromConferences(WeightedPac16, WeightedBig10,WeightedACC, recruitRating);
            }
            else if (CUSA.Contains(teamId) || conf == 2)
            {
                result = SelectFromConferences(WeightedSEC, WeightedBig12, recruitRating);
            }
            else if (SBC.Contains(teamId) || conf == 3)
            {
                result = SelectFromConferences(WeightedSEC,WeightedACC, recruitRating);
            }
            else if (MWC.Contains(teamId) || conf == 4)
            {
                result = SelectFromConferences(WeightedPac16, WeightedBig10, WeightedBig12, recruitRating);
            }
#else
            int conf = (teamId == 1023||teamId==57||teamId==8) ? RAND.Next(1, 5) : -1;
            // each G4 conference has a 3 part weight.  Each P6 conf gets 2 parts
            
            if (MAC.Contains(teamId) || conf == 1)
            {
                // MAC shares with Big 10, ACC, American
                result = SelectFromConferences(WeightedACC, WeightedBig10,WeightAmerican, recruitRating);
            }
            else if (CUSA.Contains(teamId) || conf == 2)
            {
                // Big 12 gets 2 parts CUSA, SEC 1
                result = SelectFromConferences(WeightedBig12,WeightedSEC, WeightedBig12, recruitRating);
            }
            else if (SBC.Contains(teamId) || conf == 3)
            {
                // Sunbelt shares with SEC, ACC, Big 10
                result = SelectFromConferences(WeightedSEC, WeightedACC, WeightedBig10, recruitRating);
            }
            else if (MWC.Contains(teamId) || conf == 4)
            {
                // Pac gets 2 parts MWC, American 1
                result = SelectFromConferences(WeightAmerican, WeightedPac16, WeightedPac16, recruitRating);
            }
#endif
            if (result == 0)
                throw new Exception("bad data");

            // if it already is in the list, get another
            if (teams.Contains(result))
            {
                return GetReplacement(teamId, teams, recruitRating);
            }

            return result;
        }
#endif

        static void Check()
        {
            /*
            if (ACC.Length != 16)
            {
                throw new Exception("ACC");
            }
            if (SEC.Length != 14)
            {
                throw new Exception("SEC");
            }
            if (Pac12.Length != 14)
            {
                throw new Exception("Pac16");
            }
            if (Big12.Length != 12)
            {
                throw new Exception("Big16");
            }
            if (Big10.Length != 13)
            {
                throw new Exception("Big10");
            }
            if ((ACC.Length + Big10.Length + Big12.Length + Pac12.Length + SEC.Length + American.Length + MAC.Length + CUSA.Length + SBC.Length + MWC.Length + OnTheirOwn.Length) != 126)
            {
                throw new Exception("bad data");
            }*/
        }
    }
}