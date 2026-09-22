using EA_DB_Editor.CAPGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

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
    public static class RecruitReader
    {
        public static Dictionary<int, List<MaddenRecord>> OrganizeRecruits()
        {
            var recruits = new Dictionary<int, MaddenRecord>();
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");

            foreach (var recruit in recruitTable.lRecords)
            {
                recruits[recruit["PRSI"].ToInt32()] = recruit;
            }

            var recruitPitchTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPR");
            var result = new Dictionary<int, List<MaddenRecord>>();

            foreach (var committedRecruit in recruitPitchTable.lRecords.Where(mr => mr["PTCM"] != "1023"))
            {
                var team = committedRecruit["PTCM"].ToInt32();

                if (!result.TryGetValue(team, out var recruitList))
                {
                    recruitList = result[team] = new List<MaddenRecord>();
                }

                recruitList.Add(recruits[committedRecruit["PRSI"].ToInt32()]);
            }

            return result;
        }
    }

    public static class RecruitingFixup
    {
        // by default we make 3 CAPs
        public static HashSet<int> DontChange = new HashSet<int> { 0, 1, 2 };
        public static string[] PlayerSkills = new[] { "PSPD", "PSTR", "PAGI", "PACC", "PAWR", "PBTK", "PTRK", "PESV", "PBCV", "PSAR", "PSMV", "PJMV", "PCAR", "PCTH", "SPCT", "TRAF", "PRTR", "PJMP", "PTHP", "PTHA", "PTAK", "PHIT", "PPMV", "PFMV", "PBSH", "PPRS", "PPRC", "PMCV", "PZCV", "PYRS", "RELS", "PPBK", "PRBK", "PIBL", "PKRT", "PSTA", "PINJ" };

        public static void Register(int id)
        {
            DontChange.Add(id);
        }

        public static bool TrainJucoPlayer(this Dictionary<string, string> dict, HashSet<string> rosteredPlayers)
        {
            // double check that the player isn't on a roster, these three dimensions should be enough to find them
            var firstName = dict["PFNA"];
            var lastName = dict["PLNA"];
            var year = dict["PYEA"].ToInt32();
            if (rosteredPlayers.Contains(CreateLookupKey(firstName, lastName, year)))
            {
                return false;
            }

            var set = new HashSet<string>(PlayerSkills);

            // add a modifier to unscouted OVR of 1 to 5 because JUCOs have some game tape
            dict["RCOV"] = (dict["RCOV"].ToInt32() + (TableUtility.Rand100() % 5) + 1).ToString();

            foreach (var kvp in dict)
            {
                // increment the player year
                if (kvp.Key == "PYEA")
                {
                    dict[kvp.Key] = (kvp.Value.ToInt32() + 1).ToString();
                }
                else if (set.Contains(kvp.Key))
                {
                    // add 1-3 points for offseason progression
                    var mod = 1 + TableUtility.Rand100() % 3;
                    var newValue = Math.Min(99, kvp.Value.ToInt32() + mod);
                    dict[kvp.Key] = newValue.ToString();
                }
                else
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return true;
        }

        private static string CreateLookupKey(string first, string last, int year) => $"{first}|{last}|{year}";

        private static int PosititionGroup(this Dictionary<string, string> dict) => dict["RPGP"].ToInt32();

        public static void RegisterJucoRecruits(JucoRecruits jucos)
        {
            // we need to create a hashset of Player NAmes/Year for lookup
            var playerTable = TableUtility.FindTable("PLAY");
            var playerLookup = new HashSet<string>(playerTable.lRecords.Select(mr => CreateLookupKey(mr.FirstName(), mr.LastName(), mr.PYEA())));

            // we get a position group to queue dictionary
            var recruitPitchTable = TableUtility.FindTable("RCPR");
            var recruitTable = TableUtility.FindTable("RCPT");
            var worstRecruits = recruitTable.lRecords
                .Where(mr => mr.POVR() < 60)
                .GroupBy(mr => mr.PositionGroup())
                .ToDictionary(g => g.Key, g => new Queue<MaddenRecord>(g.OrderBy(mr => mr.POVR())));

            // jucos gotta train
            jucos.Recruits.ForEach(r =>
            {
                // a player might have made it onto a roster as a walk on
                if(!r.TrainJucoPlayer(playerLookup))
                {
                    return;
                }

                var group = r.PosititionGroup();

                // can't find a replacement, sometimes guys just flame out
                if (!worstRecruits.TryGetValue(group, out var canBeReplaced) || canBeReplaced.Count ==0)
                {
                    worstRecruits.Remove(group);
                    return;
                }

                var mr = canBeReplaced.Dequeue();

                foreach(var kvp in r)
                {
                    mr[kvp.Key] = kvp.Value;

                    if (mr.RecruitRank() < 400)
                    {
                        mr["RCRK"] = (TableUtility.Rand100() + 400).ToString();
                    }
                }

                Register(mr.RecruitId());

                // find schools for the recruit
                var state = mr.State();
                var schools = JucoConfStateAssignments.Value[state].CreateAndShuffle().Take(10).ToArray();
                var recruit = TransferPortal.FindRecruit(recruitPitchTable, mr.RecruitId());

                // now apply the schools to the recruit pitch table
                // set PT01 = PT10
                for (int i = 1; i <= schools.Length; i++)
                {
                    var key = i == 10 ? "PT10" : "PT0" + i.ToString();
                    recruit[key] = schools[i - 1].ToString();
                }
            });
        }

        const int P5Cutoff = 300;

        public static Random RAND = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0));
        public static void Fix(bool fixPoints)
        {
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");
            var recruitPitchTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPR");

            foreach (var record in recruitTable.lRecords)
            {
                Fixup(recruitPitchTable, recruitTable, record, fixPoints);
            }
        }

        public static bool PreseasonFixupRun { get; set; }

        public static int[] Pop(this Stack<int> stack, int count)
        {
            var result = new List<int>(count);

            for (int i = 0; i < count; i++)
            {
                result.Add(stack.Pop());
            }

            return result.ToArray();
        }

        public static List<MaddenRecord> PreseasonFixup()
        {
            var recruitTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPT");

            // find the last 5 guys in texas and cali
            int texasId = 42;
            int caliId = 4;
            int floridaId = 8;
            const int louisianaId = 17;
            var positionGroups = new int[] { 1, 2, 3, 4, 0, 1, 5, 6, 7, 0, 8, 0, 9, 10, 11, 12, 13, 14 };
            var recruitPitchTable = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "RCPR");
            // select 25 random positions:  5 from Texas, 15 from SEC country, 5 elsewhere, 5 from florida
            var positionsToLookFor = new Stack<int>();

            for (int i = 0; i < 100; i++)
            {
                positionsToLookFor.Push(positionGroups[RAND.Next(0, positionGroups.Length)]);
            }



#if false // you want the last guy in the list to be here
            var l = new List<int>(positionsToLookFor);
            l.Add(0);
            positionsToLookFor = l.ToArray();
#endif

            var caliPosition = positionsToLookFor.Pop(5);
            var texPosition = positionsToLookFor.Pop(9);
            var flPosition = positionsToLookFor.Pop(12);
            var secPosition = positionsToLookFor.Pop(20);
            var nationalPosition = positionsToLookFor.Pop(10);
            var hawaiiPosition = positionsToLookFor.Pop(3);
            var laPosition = positionsToLookFor.Pop(8);

            // get the lowest ranked freshman at the position we have selected
            var caliRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && caliPosition.Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == caliId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var texasRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && texPosition.Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == texasId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var floridaRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && flPosition.Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == floridaId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var secRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && secPosition.Contains(r["RPGP"].ToInt32()) && SECStates.Contains(r["STAT"].ToInt32())).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var nationalRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && nationalPosition.Contains(r["RPGP"].ToInt32()) && SECStates.Concat(new[] { texasId, caliId }).Contains(r["STAT"].ToInt32()) == false).OrderByDescending(r => r["RCRK"].ToInt32()).Take(500).ToArray();
            var athRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && r["RPGP"].ToInt32() == 18).OrderByDescending(r => r["RCRK"].ToInt32()).Take(3).ToArray();
            var hawaiiRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && hawaiiPosition.Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == 10).OrderByDescending(r => r["RCRK"].ToInt32()).Take(2000).ToArray();
            var laRecruits = recruitTable.lRecords.Where(r => r["PYEA"].ToInt32() == 0 && flPosition.Contains(r["RPGP"].ToInt32()) && r["STAT"].ToInt32() == louisianaId).OrderByDescending(r => r["RCRK"].ToInt32()).Take(1000).ToArray();

            List<MaddenRecord> records = new List<MaddenRecord>();
            FillList(records, caliRecruits, caliPosition);
            FillList(records, texasRecruits, texPosition);
            FillList(records, floridaRecruits, flPosition);
            FillList(records, secRecruits, secPosition);
            FillList(records, nationalRecruits, nationalPosition);
            FillList(records, hawaiiRecruits, hawaiiPosition);
            FillList(records, laRecruits, laPosition);

            // ATH recruits get a 5-20 adder to each stat
            foreach (var ath in athRecruits)
            {
                foreach (var rating in Player.RatingMap.Keys)
                {
                    var curr = ath[rating].ToInt32();
                    curr += RAND.Next(5, 21);
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
            var recruit = TransferPortal.FindRecruit(pitchTable, recruitInfo["PRSI"].ToInt32());
            var state = recruitInfo["STAT"].ToInt32();
            var pitches = new[] { 1, 3, 4, 5, 6, 8, 9, 12, 13 };
            var selected = new List<int>();
            while (selected.Count < 3)
            {
                var p = pitches[RAND.Next(0, pitches.Length)];
                if (selected.Contains(p) == false)
                    selected.Add(p);
            }

            if (pitches.Contains(recruitInfo["IMP1"].ToInt32()) == false) recruitInfo["IMP1"] = selected[0].ToString();
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


        static Lazy<Dictionary<int, int[]>> ConfStateAssignments = new Lazy<Dictionary<int, int[]>>(TableUtility.CreateConferenceAssignmentsForStates, true);
        static Lazy<Dictionary<int, int[]>> JucoConfStateAssignments = new Lazy<Dictionary<int, int[]>>(TableUtility.CreateConferenceAssignmentsForJucos, true);

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

            if (DontChange.Contains(recruitId)) { return; }

            List<int> teams = new List<int>();
            List<int> subs = new List<int>();

            // we want to make sure the top 750 commit just to P5 schools
            if (recruitRank < 1500)
            {
                // PT01-PT10 are the top teams
                // PTCM is the committed team
                var recruit = TransferPortal.FindRecruit(pitchTable, recruitId);

                if (!DontChange.Contains(recruitId))
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
                        if (teamId.IsP5() || OnTheirOwn.Contains(teamId))
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
                        if (teamId.IsP5() == false && !OnTheirOwn.Contains(teamId) && ShouldReplaceForAAC(recruitRank, teamId))
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
                else if (!recruit["PTCM"].ToInt32().IsP5() && recruitRank <= P5Cutoff && !OnTheirOwn.Contains(recruit["PTCM"].ToInt32()))
                {
                    // mark the top 250 recruits that committed to non P5 schools
                    recruit["PTCM"] = "9999";
                }
                else if (recruitRank < 500 && !recruit["PTCM"].ToInt32().IsP5() && !OnTheirOwn.Contains(recruit["PTCM"].ToInt32()) && !DontFoolWith.Contains(recruit["PTCM"].ToInt32()))
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

            return !DontFoolWith.Contains(teamId);
        }

        static HashSet<int> changedFaces = new HashSet<int>();

        public static void ChangeRecruitFace(MaddenTable recruitTable, int recruitId)
        {
            if (!changedFaces.Add(recruitId)) return;

            foreach (var recruit in recruitTable.lRecords.Where(r => !RecruitingFixup.DontChange.Contains(r.RecruitId())))
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
        static int[] linePlayers = { 5, 6, 7, 8, 9, 10, 11, 12 };


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


        public static Dictionary<int, int> TeamAndDivision
        {
            get
            {
                if (teamAndDivisions == null || teamAndDivisions.Count == 0)
                {
                    try
                    {
                        teamAndDivisions = Form1.MainForm.maddenDB.lTables[167].lRecords
                            .Where(mr => mr.lEntries[40].Data.ToInt32() != 611 && mr.lEntries[37].ToInt32() != 30 && mr.lEntries[40].Data.ToInt32() != 300)
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

        public static int SunBeltTeams => TableUtility.TeamAndConferences.Values.Count(v => v == TableUtility.SBCId);

        public static int AccTeams
        {
            get
            {
                if (accTeams.HasValue == false)
                {
                    accTeams = TableUtility.TeamAndConferences.Values.Count(v => v == TableUtility.ACCId);
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

        private static Dictionary<int, int> prestigeMap;
        public static Dictionary<int, int> PrestigeMap
        {
            get
            {
                if (prestigeMap == null)
                {
                    var table = MaddenTable.FindMaddenTable(Form1.MainForm.maddenDB.lTables, "TEAM");
                    prestigeMap = table.lRecords.ToDictionary(mr => mr["TGID"].ToInt32(), mr => mr["TPRX"].ToInt32());
                }

                return prestigeMap;
            }
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


        static string[] academies = { "1", "8", "57" };
        public static int[] OnTheirOwn = TeamsOnTheirOwn();
#if true
        public static int[] DontFoolWith = new int[0];// American.ToArray();
#else
        public static int[] DontFoolWith = TableUtility.American.ToArray();
#endif

        public static int[] TeamsOnTheirOwn()
        {
            return ScheduleFixup.FindUserControllerTeams(true)
                //.Concat(new[] {  })
                .Where(team => !team.IsP5OrND()).ToArray();
        }

        public static bool IsServiceAcademy(this int teamId)
        {
            return academies.Contains(teamId.ToString());
        }


        public static bool IsSECTeam(this int teamId)
        {
            return TableUtility.SEC.Contains(teamId);
        }

        public static bool IsPac12Team(this int teamId) => TableUtility.Pac12.Contains(teamId);

        public static bool IsSunBeltTeam(this int teamId) => TableUtility.SBC.Contains(teamId);

        public static bool IsAccTeam(this int teamId)
        {
            return TableUtility.ACC.Contains(teamId) && teamId != 68;
        }

        public static bool IsBig12Team(this int teamId)
        {
            return TableUtility.Big12.Contains(teamId) && teamId != 68;
        }

        public static bool IsBig10Team(this int teamId)
        {
            return TableUtility.Big10.Contains(teamId) && teamId != 68;
        }

        public static bool IsAmericanTeam(this int teamId)
        {
            return TableUtility.American.Contains(teamId);
        }

        public static bool HasWeek14Games(this int teamId)
        {
            var conf = TableUtility.TeamAndConferences[teamId];
            var count = TableUtility.TeamAndConferences.Count(kvp => kvp.Value == conf && kvp.Key != 68);
            return count < 12;
        }

        public static bool TooManyFcsGameCheck(this int teamId, int fcsGAmes)
        {
            var conf = TableUtility.TeamAndConferences[teamId];
            var count = TableUtility.TeamAndConferences.Count(kvp => kvp.Value == conf && kvp.Key != 68);

            if (count <= 6)
            {
                return fcsGAmes > 2;
            }

            return fcsGAmes > 1;
        }

        public static bool ConferenceGameCountCheck(this int teamId, int current)
        {
            var conf = TableUtility.TeamAndConferences[teamId];
            var count = TableUtility.TeamAndConferences.Count(kvp => kvp.Value == conf && kvp.Key != 68);
            var expected = 0;

            if (conf == TableUtility.IndId) return true;

            if (conf == TableUtility.Big12Id)
            {
                return current == 8;
            }

            if (conf == TableUtility.CUSAId && count == 4)
            {
                return current == 6;
            }

            if (conf == TableUtility.Big12Id && count == 16)
                expected = 9;

            if (conf == TableUtility.SECId && current == 10)
                return true;

            if (count == 12 && conf == TableUtility.Pac16Id)
                return current == 8;

            if (count == 12 && conf == TableUtility.Big10Id)
                return current == 8;

            if (count == 16 && conf == TableUtility.ACCId)
                expected = 8;
            else if (count == 16 && conf == TableUtility.AmericanId)
                expected = 8;
            else if (count == 16 && conf == TableUtility.SBCId)
                expected = 8;
            else if (count >= 16)
                expected = 9;
            else if (count == 14 || count == 12 || count == 11 || count == 13 || count == 15)
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

        public static bool ConferenceHomeGameCount(this TeamSchedule schedule, int teamId)
        {
            var conf = TableUtility.TeamAndConferences[teamId];
            var confGames = schedule.Count(g => g != null && g.HomeTeam == teamId && TableUtility.TeamAndConferences[g.AwayTeam] == conf);

            if (conf == TableUtility.CUSAId)
            {
                return confGames == 3;
            }

            if (conf == TableUtility.Big10Id || conf == TableUtility.Pac16Id)
            {
                return confGames == 4;
            }
            if (conf == TableUtility.Big12Id)
            {
                return confGames == 4;
            }

            if (conf == TableUtility.Pac16Id) return true;
            if (conf == TableUtility.Big12Id && (TableUtility.Big12.Length == 16 || TableUtility.Big12.Length == 10)) return true;
            //if (conf == TableUtility.AmericanId && TableUtility.American.Length == 16) return true;
            if (conf == TableUtility.AmericanId && TableUtility.American.Length == 10) return true;
            if (conf == TableUtility.IndId) return true;
            if (conf == TableUtility.MACId && TableUtility.MAC.Length == 16) return true;
            if (conf == TableUtility.MWCId /*&& MWC.Length == 10*/) return true;

            //if (conf == ACCId && AccTeams > 14)
            //    return true;


            if (conf == TableUtility.CUSAId && TableUtility.CUSA.Length == 5 && confGames == 2) return true;

            if (conf == TableUtility.CUSAId && TableUtility.CUSA.Length == 7 && confGames == 3) return true;

            if (conf == TableUtility.CUSAId && TableUtility.CUSA.Length == 4 && (confGames == 2 || confGames == 1)) return true;

            if (conf == TableUtility.CUSAId && TableUtility.CUSA.Length == 6 && (confGames == 2 || confGames == 3)) return true;

            return confGames == 4;
        }

        public static bool TeamsInSameConference(int t1, int t2)
        {
            return TableUtility.TeamAndConferences[t1] == TableUtility.TeamAndConferences[t2];
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
    }
}