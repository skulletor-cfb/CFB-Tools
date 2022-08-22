using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;

namespace EA_DB_Editor
{
    public enum CopyAction
    {
        Roster,
        Coach,
        HSAA
    }
    public static class RosterCopy
    {
        static FieldList lMappedFields = new FieldList();
        static List<MaddenTable> lMappedTables = new List<MaddenTable>();
        static List<View> lMappedViews = new List<View>();
        static MaddenDatabase maddenDB = null;
        static bool bConfigRead = false;
        static public bool mc02Recalc = false;

        public static T FromJson<T>(this string json)
        {
            var js = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)js.ReadObject(ms);
            }
        }

        public static void LoadSource(Form1 form, MaddenDatabase destination, CopyAction copyAction)
        {
            ContinuationData continuation = new ContinuationData();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"D:\OneDrive\ncaa";
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = ".*";
            openFileDialog.Filter = "(*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select MC02 file to open...";

            if (System.Windows.Forms.DialogResult.OK == openFileDialog.ShowDialog())
            {
                var maddenDB = new MaddenDatabase(openFileDialog.FileName);
                var tables = new List<string>(new[] { "CSKL", "COCH", "DCHT", "PLAY", "TEAM", "RCPT", "RCPR" });

                var sourceTablesForRoster = maddenDB.lTables.Where(t => tables.Contains(t.Table.TableName)).ToList();
                continuation.Teams = MaddenTable.FindMaddenTable(maddenDB.lTables, "TEAM").lRecords.Where(mr => mr["TGID"].ToInt32().IsValidTeam()).Select(mr => new TeamData(mr)).ToList();

                // walked each table and field and add in the mapped elements
                foreach (MaddenTable mt in sourceTablesForRoster)
                {
                    MaddenTable mtmapped = MaddenTable.FindTable(lMappedTables, mt.Table.TableName);
                    mt.Abbreviation = mt.Table.TableName;
                    if (mtmapped != null)
                        mt.Name = mtmapped.Name;

                    foreach (Field f in mt.lFields)
                    {
                        Field fmapped = lMappedFields.FindField(f.name);
                        f.Abbreviation = f.name;
                        if (fmapped != null)
                            f.Name = fmapped.Name;
                    }
                }

                if (copyAction == CopyAction.Coach)
                {
                    CopyCoachTable(continuation, FindTable(sourceTablesForRoster, "COCH"), FindTable(destination.lTables, "COCH")); 
                    CopyCoachSkillTable(continuation, FindTable(sourceTablesForRoster, "CSKL"), FindTable(destination.lTables, "CSKL"));

                    if (MessageBox.Show("Do you want to copy over Bowl Tie-ins, NCAA Records and School Records?", "Copy", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CopyTeamRecordData(maddenDB.lTables[159], destination.lTables[159]);
                        //CopyNCAARecordData(maddenDB.lTables[91], destination.lTables[91]);
                        CopyBowlData(maddenDB.lTables[129], destination.lTables[129]);
                        CopyStadiumData(MaddenTable.FindTable(maddenDB.lTables, "STAD"), MaddenTable.FindTable(destination.lTables, "STAD"));
                    }
                }
                else if (copyAction == CopyAction.Roster)
                {
                    CopyPlayers(continuation, FindTable(sourceTablesForRoster, "PLAY"), FindTable(sourceTablesForRoster, "DCHT"), FindTable(destination.lTables, "PLAY"), FindTable(destination.lTables, "DCHT"));


                    if (MessageBox.Show("Recruit ratings and playbook, etc", "Do you want to copy over team data?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        CopyTeamData(continuation, FindTable(sourceTablesForRoster, "TEAM"), FindTable(destination.lTables, "TEAM"));
                    }
                }
                else if (copyAction == CopyAction.HSAA)
                {
                    var source = FindTable(sourceTablesForRoster, "RCPT");
                    RecruitAllAmericans.GetRecruits(source, FindTable(sourceTablesForRoster, "RCPR"));
                    var dest = FindTable(destination.lTables, "PLAY");
                    CopyHSAARoster(source, dest);
                }

                form.PostProcessMaps();
                form.UpdateTableBoundViews();
            }

            SerializeToFile(continuation, "continuationfile.txt");
        }

        static void SerializeToFile(object o, string file)
        {
            using (var stream = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(o.GetType());
                ser.WriteObject(stream, o);
                stream.Position = 0;
                File.WriteAllText(file, Encoding.UTF8.GetString(stream.ToArray()));
            }
        }

        static void CopyTeamRecordData(MaddenTable source, MaddenTable destination)
        {
            var include = new string[] { "RCDH", "RCDV", "RCDO" };
            CopyData(
                source.CreateDictionary(CreateSchoolRecordKey, mr => mr["RCDY"].ToInt32() != 63),
                destination.CreateDictionary(CreateSchoolRecordKey, mr => mr["RCDY"].ToInt32() != 63),
                null,
                null,
                key => include.Contains(key),
                null
                );
        }

        static void CopyNCAARecordData(MaddenTable source, MaddenTable destination)
        {
            var include = new string[] { "RCDH", "RCDV", "RCDO" };
            CopyData(
                source.CreateDictionary(mr => mr["RCDI"].ToInt32(),
                mr =>
                {
                    var id = mr["RCDI"].ToInt32();
                    var allRecordsWithid = source.lRecords.Where(rec => rec["RCDI"].ToInt32() == id).ToArray();
                    if (allRecordsWithid.Length == 1)
                        return true;

                    var year = mr["RCDY"].ToInt32();
                    var ordered = allRecordsWithid.OrderByDescending(or => or["RCDY"].ToInt32()).ToArray();
                    return ordered[0]["RCDY"].ToInt32() == year;
                }),
                destination.CreateDictionary(mr => mr["RCDI"].ToInt32(), null),
                null,
                null,
                key => include.Contains(key),
                (s, d, k) => k != "RCDY" || d[k].Data.ToInt32() != 63);
        }

        static void CopyBowlData(MaddenTable source, MaddenTable destination)
        {
            var include = new[] { "BCI1", "BCR1", "BCI2", "BCR2", "BNME" };
            var dest = destination.CreateDictionary(mr => mr["BIDX"].ToInt32(), mr => mr.lEntries[12].Data.ToInt32() > 16);
            var sauce = source.CreateDictionary(mr => mr["BIDX"].ToInt32(), mr => mr.lEntries[12].Data.ToInt32() > 16);
            CopyData(
                sauce,
                dest,
                null,
                null,
                key => include.Contains(key));
        }

        static void CopyStadiumData(MaddenTable source, MaddenTable destination)
        {
            var include = new[] { "SNAM", "SCIT", "STAT", "STNN", "TDNA", "SCAP", "STAA", "FLID", "WCLC", "STOF", "STRY", "STYP"  };

#if false
            var sourceToDestinationMap = new Dictionary<int, int>();

            // for a straight map we just copy
            var ids = new int[] {3, 4, 97, 99, 31, 80, 175, 185, 251, 242, 258, 261, 262, 263, 264, 265, 266, 267, 163, 259, 268, 257, 258, 259, 261, 262, 263, 264, 265, 266, 267, 268, 274, 279, 245 };
            foreach(var id in ids)
            {
                sourceToDestinationMap[id] = id;
            }

            var destinationMap = sourceToDestinationMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);


            var sauce = source.CreateDictionary(mr => mr["SGID"].ToInt32(), mr => sourceToDestinationMap.ContainsKey(mr["SGID"].ToInt32()));
            var dest = destination.CreateDictionary(mr => mr["SGID"].ToInt32(), mr => destinationMap.ContainsKey(mr["SGID"].ToInt32()));
#else
            var sauce = source.CreateDictionary(mr => mr["SGID"].ToInt32(), mr=>true);
            var dest = destination.CreateDictionary(mr => mr["SGID"].ToInt32(), mr => true);
#endif

            foreach (var key in dest.Keys)
            {
#if false
                var sourceKey = destinationMap[key];
#else
                var sourceKey = key;
#endif
                var sourceRow = sauce[sourceKey];
                var destRow = dest[key];
                CopyRecordData(sourceRow, destRow, dataKey => include.Contains(dataKey));

                switch(key)
                {
                    case 278:
                        destRow["TDNA"].Data = "Southwest Classic";
                        break;

                    case 257:
                        destRow["TDNA"].Data = "Battle for the Iron Skillet";
                        break;

                    case 268:
                        // set the data for duke's mayo classic
                        destRow["FLID"].Data = "HSNESM";
                        break;

                    case 259:
                    case 266:
                    case 265:
                    case 264:
                        // set the data for camping world kickoff
                        destRow["FLID"].Data = "HSNEMD";
                        break;

                    default:
                        break;
                }
            }
        }

        static void CopyTeamData(ContinuationData continuation, MaddenTable teamSource, MaddenTable teamDestination)
        {
            Action<int, int, Dictionary<string, DBData>, Dictionary<string, DBData>> action = null;
            if (MessageBox.Show("W-L records and Championships will be set to 0", "Do you want to change Team W-L", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                action = SetTeamStatsToZero;
            }

            var columnsToCopy = new[] { "PCHV", "PPFV", "PAFV", "PTDV", "TMAR", "TDPB", "TOPB", "PCLV", "PAPV", "PCPV", "PPSV", "PTVV", "TPRX", "TPST", "TPSL", "TPSW", "TCRK", "TMRK" , "TCHS", "TRIV"};

            CopyTeamData(
                teamSource.CreateDictionary(mr => mr["TGID"].ToInt32(), mr => mr.IsValidTeam()),
                teamDestination.CreateDictionary(mr => mr["TGID"].ToInt32(), mr => mr.IsValidTeam()),
                action,
                new string[0],
                ck => columnsToCopy.Contains(ck));
        }

        static void SetTeamStatsToZero(int a, int b, Dictionary<string, DBData> c, Dictionary<string, DBData> team)
        {
            team["NUMC"].Data = "0";
            team["TCTI"].Data = "0";
            team["TCWI"].Data = "0";
            team["TCLO"].Data = "0";
            team["CBOL"].Data = "0";
            team["CBOT"].Data = "0";
            team["CBOW"].Data = "0";
            team["NCYN"].Data = "0";
            team["TCHW"].Data = "0";
            team["TCHL"].Data = "0";
            team["TCHT"].Data = "0";
            team["FCYR"].Data = "262";
            team["FNYR"].Data = "262";
        }

        /// <summary>
        /// For Players we don't copy PGID, TGID, POID = Player Id, Team Id , Player Original Id
        /// </summary>
        /// <param name="continuation"></param>
        /// <param name="playersSource"></param>
        /// <param name="depthChartSource"></param>
        /// <param name="playersDestination"></param>
        /// <param name="depthChartDestination"></param>
        static void CopyPlayers(ContinuationData continuation, MaddenTable playersSource, MaddenTable depthChartSource, MaddenTable playersDestination, MaddenTable depthChartDestination)
        {
            // first we group by team, so what we have is a Dictionary key'd by team id, and each bucket has a Dictionary keyed by PlayerId
            var sourceTeams = playersSource.lRecords.GroupBy(mr => mr["TGID"].ToInt32()).ToDictionary(group => group.Key, group => group.ToList().ToDictionary(mr => mr["PGID"].ToInt32()));
            var sourceDepthChart = depthChartSource.lRecords.GroupBy(mr => mr["TGID"].ToInt32()).ToDictionary(group => group.Key, group => group.ToList().ToDictionary(mr => new DepthChartKey(mr["PPOS"].ToInt32(), mr["ddep"].ToInt32())));
            var destTeams = playersDestination.lRecords.GroupBy(mr => mr["TGID"].ToInt32()).ToDictionary(group => group.Key, group => group.ToList().ToDictionary(mr => mr["PGID"].ToInt32()));
            var destDepthChart = depthChartDestination.lRecords.GroupBy(mr => mr["TGID"].ToInt32()).ToDictionary(group => group.Key, group => group.ToList().ToDictionary(mr => new DepthChartKey(mr["PPOS"].ToInt32(), mr["ddep"].ToInt32())));

            foreach (var key in destDepthChart.Keys)
            {
                // only copy over for valid teams
                if (key.IsValidTeam() == false)
                    continue;

#if false
                // uconn and nmsu are gone!
                if (key == 61 || key == 100)
                    continue;
#endif

                var sourceId = key.SourceKeyFromDesintation(out var value) ? value : key;

                CopyPlayersForTeam(sourceTeams[sourceId], sourceDepthChart[sourceId], destTeams[key], destDepthChart[key]);
            }
        }

        static void CopyHSAARoster(MaddenTable source, MaddenTable destination)
        {
            var recruitFields = source.lRecords.First().lEntries.Select(entry => entry.GetFieldName()).ToArray();
            var playerFields = destination.lRecords.First().lEntries.Select(entry => entry.GetFieldName()).ToArray();
            var intersection = recruitFields.Intersect(playerFields).Where(s => s != "TGID").ToArray();
            var list = string.Join(",", intersection);

            // ARMY GETS EAST
            CopyHSAARoster(RecruitAllAmericans.EastRoster.Players, destination.lRecords.Where(mr => mr["TGID"].ToInt32() == 8).OrderBy(mr => mr["PPOS"].ToInt32()).ToList(), intersection);

            // NAVY GETS WEST
            CopyHSAARoster(RecruitAllAmericans.WestRoster.Players, destination.lRecords.Where(mr => mr["TGID"].ToInt32() == 57).OrderBy(mr => mr["PPOS"].ToInt32()).ToList(), intersection);
        }

        static void CopyHSAARoster(List<MaddenRecord> source, List<MaddenRecord> destination, string[] keys)
        {
            for (int i = 0; i < source.Count; i++)
            {
                var s = source[i];
                var d = destination[i];
                foreach (var key in keys)
                {
                    d[key] = s[key];
                }
            }
        }

        /// <summary>
        /// Depth Chart has the following columns:  PGID, TGID, PPOS , ddep : Player Id , Team Id , Position , Depth Chart Position
        /// </summary>
        /// <param name="sourcePlayers"></param>
        /// <param name="sourceDepthChart"></param>
        /// <param name="destPlayers"></param>
        /// <param name="destDepthChart"></param>
        static void CopyPlayersForTeam(Dictionary<int, MaddenRecord> sourcePlayers, Dictionary<DepthChartKey, MaddenRecord> sourceDepthChart, Dictionary<int, MaddenRecord> destPlayers, Dictionary<DepthChartKey, MaddenRecord> destDepthChart)
        {
            var playerFieldExclusion = new[] { "PGID", "TGID", "POID" };

            // first copy players that are in the depth chart
            var destDepthChartKeys = destDepthChart.Keys.ToArray();
            foreach (var key in destDepthChartKeys)
            {
                // we found a match and a player to replace
                if (sourceDepthChart.ContainsKey(key))
                {
                    var sourcePlayerId = sourceDepthChart[key]["PGID"].ToInt32();
                    var destPlayerId = destDepthChart[key]["PGID"].ToInt32();

                    // remove the key from the depth chart dictionary to narrow down who needs a replacement
                    destDepthChart.Remove(key);
                    sourceDepthChart.Remove(key);

                    // copy over player if he hasn't been copied/removed already
                    if (sourcePlayers.ContainsKey(sourcePlayerId) && destPlayers.ContainsKey(destPlayerId))
                    {
                        CopyRecordData(
                            sourcePlayers[sourcePlayerId].lEntries.ToDictionary(dbd => dbd.GetFieldName()),
                            destPlayers[destPlayerId].lEntries.ToDictionary(dbd => dbd.GetFieldName()),
                            ck => !playerFieldExclusion.Contains(ck));

                        sourcePlayers.Remove(sourcePlayerId);
                        destPlayers.Remove(destPlayerId);
                    }
                }
            }

            MaddenRecord[] sourcePlayerArray, destPlayersArray;
            // now we look for players who haven't been copied over to copy over
            // this case is easy, will have extra players, just have a 1:1 map
            if (destPlayers.Count >= sourcePlayers.Count)
            {
                sourcePlayerArray = sourcePlayers.Values.ToArray();
                destPlayersArray = destPlayers.Values.Take(sourcePlayerArray.Length).ToArray();
            }
            else
            {
                // this is hard as we have to cut some players, try to cut the lowest rated
                destPlayersArray = destPlayers.Values.ToArray();
                sourcePlayerArray = sourcePlayers.Values.OrderByDescending(v => v["POVR"].ToInt32()).Take(destPlayersArray.Length).ToArray();
            }

            for (int i = 0; i < sourcePlayerArray.Length; i++)
            {
                CopyRecordData(
                    sourcePlayerArray[i].lEntries.ToDictionary(dbd => dbd.GetFieldName()),
                    destPlayersArray[i].lEntries.ToDictionary(dbd => dbd.GetFieldName()),
                    ck => !playerFieldExclusion.Contains(ck));
            }
        }

        static void CopyCoachSkillTable(ContinuationData continuation, MaddenTable source, MaddenTable destination)
        {
            var levelsData = File.ReadAllText("levels.txt");
            Dictionary<int, int> levelsAndXp = null;
            using (var stream = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(levelsData);
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                var ser = new DataContractJsonSerializer(typeof(Dictionary<int, int>));
                levelsAndXp = (Dictionary<int, int>)ser.ReadObject(stream);
            }


            var exclude = new[] { "CCID", "CFUC", "TOID", "BLCS", "NLVL" };
            var coachSkillKeys = File.ReadAllText("coachSkill.txt").Split(',').Where(s => !exclude.Contains(s)).ToArray();
            var sourceData = source.CreateDictionary(
                record => continuation.OldToNewCoachMap[CreateCoachSkillKey(record)], 
                record => continuation.OldToNewCoachMap.ContainsKey(CreateCoachSkillKey(record)),
                (a,b) =>
                {
                    if (a["CLVL"].Data.ToInt32() > b["CLVL"].Data.ToInt32())
                        return a;
                    return b;
                });
            var destinationData = destination.CreateDictionary(CreateCoachSkillKey, null);
            CopyData(
                 sourceData,
                 destinationData,
                 null,
                /*                 (a,b,s,d)=>
                                     {
                                         var key = s["CLVL"].Data.ToInt32();
                                         if (levelsAndXp.ContainsKey(key) == false)
                                         {
                                             key++;
                                         }
                                         s["CEXP"].Data = levelsAndXp[key].ToString();
                                     },*/
                 null,
                 key => coachSkillKeys.Contains(key)
                 );
        }

        static void CopyCoachTable(ContinuationData continuationData, MaddenTable source, MaddenTable destination)
        {
            var exclude = new[] { "CCID", "CLPS", "CFUC", "CSXP", "PTID", "TOID", "CLPS", "TGID" };
            var coachSkillKeys = File.ReadAllText("coachColumns.txt").Split(',').Where(s => !exclude.Contains(s)).ToArray();

            //CCID is coach ID
            //TGID is team id
            //COPS is position
            var destinationTable = destination.CreateDictionary(CreateCoachKey, mr => mr.IsValidTeam());
            var sourceTable = source.CreateDictionary(CreateCoachKey, mr => mr.IsValidTeam());

            foreach (var key in destinationTable.Keys)
            {
#if false
                // dont modify uncc/ccu, you dont have to do this for the next continuation
                if (key.Team == 61 || key.Team == 100)
                {
                    continue;
                }
#endif

                // source key for gaso/appst are different
                var sourceKey = key.Team.SourceKeyFromDesintation(out var value) ? new CoachKey { Team = value, Position = key.Position } : key;

                if (sourceTable.TryGetValue(sourceKey, out var sourceRow))
                {
                    var rowKey = sourceTable.Keys.First(myKey => myKey.Equals(sourceKey));

                    continuationData.CoachMapping.Add(
                        new CoachMapping
                        {
                            OldCoachId = rowKey.CoachId,
                            NewCoachId = key.CoachId,
                        });

                    CopyRecordData(sourceRow, destinationTable[key], fieldKey => coachSkillKeys.Contains(fieldKey));
                }
            }
        }

        static void CopyTeamData(
            Dictionary<int, Dictionary<string, DBData>> source, 
            Dictionary<int, Dictionary<string, DBData>> destination, 
            Action<int, int, Dictionary<string, DBData>, Dictionary<string, DBData>> action, 
            string[] dontReplaceKeys, 
            Func<string, bool> filter = null)
        {
            // for each key in the destination find data in the source
            foreach (var key in destination.Keys)
            {
                var sourceKey = key.SourceKeyFromDesintation(out var value) ? value : key;
                if (source.ContainsKey(sourceKey))
                {
                    // get the row and the key for the coach we want to replace
                    var rowKey = source.Keys.First(myKey => myKey.Equals(sourceKey));
                    var row = source[sourceKey];

                    if (action != null)
                    {
                        action(rowKey, sourceKey, row, destination[key]);
                    }

#if false
                    // dont copy nmsu or uconn
                    if (key == 100 || key == 61)
                        continue;
#endif

                    if (filter == null)
                        filter = columnKey => dontReplaceKeys.Contains(columnKey) == false;
                    
                    CopyRecordData(row, destination[key], filter);
                }
            }
        }

        static void CopyData<TableKey>(
            Dictionary<TableKey, Dictionary<string, DBData>> source, 
            Dictionary<TableKey, Dictionary<string, DBData>> destination, 
            Action<TableKey, TableKey, Dictionary<string, DBData>, Dictionary<string, DBData>> action, 
            string[] dontReplaceKeys, 
            Func<string, bool> filter = null, 
            Func<Dictionary<string, DBData>, Dictionary<string, DBData>, string, bool> editRowFilter = null)
        {
            // for each key in the destination find data in the source
            foreach (var key in destination.Keys)
            {
                if (source.ContainsKey(key))
                {
                    // get the row and the key for the coach we want to replace
                    var rowKey = source.Keys.First(myKey => myKey.Equals(key));
                    var row = source[key];

                    if (action != null)
                    {
                        action(rowKey, key, row, destination[key]);
                    }

                    if (filter == null)
                        filter = columnKey => dontReplaceKeys.Contains(columnKey) == false;

                    CopyRecordData(row, destination[key], filter);
                }
            }
        }

        static void CopyRecordData(Dictionary<string, DBData> source, Dictionary<string, DBData> destination, Func<string, bool> filter, Func<Dictionary<string, DBData>, Dictionary<string, DBData>, string, bool> editRowFilter = null)
        {
            foreach (var dataKey in destination.Keys)
            {
                if (source.ContainsKey(dataKey) && filter(dataKey) && (editRowFilter == null || editRowFilter(source, destination, dataKey)))
                {
                    destination[dataKey].Data = source[dataKey].Data;
                }
            }
        }

        static MaddenTable FindTable(List<MaddenTable> tables, string name)
        {
            return tables.Where(t => t.Table.TableName == name).SingleOrDefault();
        }

        static int CreateCoachSkillKey(MaddenRecord record)
        {
            return record["CCID"].ToInt32();
        }

        static SchoolRecordKey CreateSchoolRecordKey(MaddenRecord mr)
        {
            return new SchoolRecordKey
            {
                TeamId = mr["RCDM"].ToInt32(),
                RecordDescription = mr["RCDI"].ToInt32(),
                RecordType = mr["RCDT"].ToInt32()
            };
        }

        static CoachKey CreateCoachKey(MaddenRecord record)
        {
            return new CoachKey
            {
                CoachId = record["CCID"].ToInt32(),
                Position = record["COPS"].ToInt32(),
                Team = record["TGID"].ToInt32()
            };
        }

        static Dictionary<TableKey, Dictionary<string, DBData>> CreateDictionary<TableKey>(this MaddenTable table, Func<MaddenRecord, TableKey> keyCreator, Func<MaddenRecord, bool> recordFilter, Func<Dictionary<string, DBData>, Dictionary<string, DBData>, Dictionary<string, DBData>> resolver = null)
        {
            if (recordFilter == null)
                recordFilter = record => true;

            var tbl = new Dictionary<TableKey, Dictionary<string, DBData>>();

            foreach (var record in table.lRecords.Where(mr => recordFilter(mr)))
            {
                var newKey = keyCreator(record);
                var data = record.lEntries.ToDictionary(dbd => dbd.GetFieldName());

                if (tbl.ContainsKey(newKey) && resolver != null)
                {
                    tbl[newKey] = resolver(tbl[newKey], data);
                }
                else
                {
                    tbl.Add(newKey, data);
                }
            }

            /*            return table.lRecords.Where(mr => recordFilter(mr)).ToDictionary(
                            record => keyCreator(record),
                            mr => mr.lEntries.ToDictionary(dbd => dbd.GetFieldName())
                            );
                            */
            return tbl;
        }

        static bool IsValidTeam(this MaddenRecord record)
        {
            var teamId = record["TGID"].ToInt32();
            return teamId.IsValidTeam();
        }

        // private static HashSet<int> TeamBuilderTeams = new HashSet<int>(new[] { 901, 902 });


        public static bool SourceKeyFromDesintation( this int teamId, out int sourceId)
        {
            sourceId = 0;

#if false
            if (teamId == 34)
            {
                sourceId = 901;
                return true;
            }

            if (teamId == 181)
            {
                sourceId = 902;
                return true;
            }
#endif

            return false;
        }

        public static bool HasRedirect(this int teamId, out int redirectTo)
        {
            redirectTo = 0; 

            if (teamId == 901)
            {
                redirectTo = 34;
                return true;
            }

            if (teamId == 902)
            {
                redirectTo = 181;
                return true;
            }

            return false;
        }

        public static bool IsValidTeam(this int teamId)
        {
            // copy over team builder rosters uncomment the line below
            //return TeamBuilderTeams.Contains(teamId);
            //return TeamBuilderTeams.Contains(teamId) || teamId != 34 && teamId != 181 && teamId != 0 && teamId != 1023 && teamId != 300 && teamId < 600;

            // return TeamBuilderTeams.Contains(teamId) || (teamId != 0 && teamId != 1023 && teamId != 300 && teamId < 600);
            return (teamId != 0 && teamId != 1023 && teamId != 300 && teamId < 600); ;
        }

        public static bool IsFcsTeam(this int teamId)
        {
            return teamId >= 160 && teamId <= 164;
        }
    }

    [DataContract]
    public class TeamData
    {
        [DataMember]
        public Dictionary<string, string> Data { get; set; }
        public TeamData(MaddenRecord record)
        {
            this.Data = record.lEntries.ToDictionary(rc => rc.GetFieldName(), rc => rc.Data);
        }
    }

    [DataContract]
    public class ContinuationData
    {
        private Dictionary<int, int> newToOldCoachMap;
        private Dictionary<int, int> oldToNewCoachMap;

        public List<TeamData> Teams { get; set; }

        [DataMember]
        public List<CoachMapping> CoachMapping { get; set; }

        public ContinuationData()
        {
            this.CoachMapping = new List<CoachMapping>();
        }

        public Dictionary<int, int> NewToOldCoachMap
        {
            get
            {
                if (newToOldCoachMap == null)
                {
                    newToOldCoachMap = this.CoachMapping.ToDictionary(cm => cm.NewCoachId, cm => cm.OldCoachId);
                }

                return newToOldCoachMap;
            }
        }

        public Dictionary<int, int> OldToNewCoachMap
        {
            get
            {
                if (oldToNewCoachMap == null)
                {
                    oldToNewCoachMap = this.CoachMapping.ToDictionary(cm => cm.OldCoachId, cm => cm.NewCoachId);
                }

                return oldToNewCoachMap;
            }
        }
    }

    public class CoachMapping
    {
        public int OldCoachId { get; set; }
        public int NewCoachId { get; set; }
    }

    public class SchoolRecordKey
    {
        public int TeamId { get; set; }
        public int RecordType { get; set; }
        public int RecordDescription { get; set; }

        public override bool Equals(object obj)
        {
            var other= obj as SchoolRecordKey;
            return other != null && other.TeamId == this.TeamId && other.RecordDescription == this.RecordDescription && other.RecordType == this.RecordType;
        }

        public override int GetHashCode()
        {
            return this.RecordType | this.RecordDescription << 8 | this.TeamId << 16;
        }
    }

    public class CoachKey
    {
        public int CoachId { get; set; }
        public int Position { get; set; }
        public int Team { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as CoachKey;
            return other != null && other.Team == this.Team && other.Position == this.Position;
        }

        public override int GetHashCode()
        {
            return this.Position << 16 | this.Team;
        }
    }

    public class DepthChartKey
    {
        public DepthChartKey(int playerPosition, int depthChartPosition)
        {
            this.PlayerPosition = playerPosition;
            this.DepthChartPosition = depthChartPosition;
        }

        public int PlayerPosition { get; set; }
        public int DepthChartPosition { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as DepthChartKey;
            return other != null && other.PlayerPosition == this.PlayerPosition && other.DepthChartPosition == this.DepthChartPosition;
        }

        public override int GetHashCode()
        {
            return this.PlayerPosition << 16 | this.DepthChartPosition;
        }
    }
}