using System.Text.RegularExpressions;
using CFB27.Data.Model;
using MaddenFranchise;
using NCAA14Utility;

namespace ScheduleMaker
{
    /// <summary>
    /// Ports the NCAA 14 rosters (the PLAY table dumped to CSV) onto a CFB 27 preseason save. NCAA 14 teams have at most
    /// 70 players and CFB 27 teams 85, so on each team the best NCAA 14 player at a position replaces the best CFB 27
    /// player there, the second best the second best, and so on. NCAA 14 players left without a same-position slot take
    /// over the lowest-rated remaining CFB 27 player (same position group first) and change position. The 15+ CFB 27
    /// players not replaced stay on the team.
    ///
    /// A replaced player gets the NCAA 14 name, jersey, height, weight, class, redshirt, ratings and hometown, and a generic
    /// head of the same skin tone (NCAA 14 PGHE) in place of the CFB 27 player's face; the CFB 27 archetype, development
    /// trait, abilities, body, gear and stats stay as they were.
    /// </summary>
    internal sealed class Ncaa14RosterImporter
    {
        /// <summary>NCAA 14 team ids whose CFB 27 team has a different classic id (the same aliases the schedule import uses).</summary>
        private static readonly Dictionary<int, int> OldIdAliases = new Dictionary<int, int>
        {
            { 34, 901 },  // App State
            { 61, 903 },  // Coastal Carolina
            { 100, 904 }, // Charlotte
            { 181, 902 }, // Georgia Southern
            { 230, 905 }, // James Madison
        };

        /// <summary>NCAA 14's unattached players.</summary>
        private const int NoNcaaTeam = 1023;

        /// <summary>PositionE / NCAA 14 PPOS (the same 0-20 order in both games).</summary>
        private static readonly string[] Positions =
            { "QB", "HB", "FB", "WR", "TE", "LT", "LG", "C", "RG", "RT", "LE", "RE", "DT", "LOLB", "MLB", "ROLB", "CB", "FS", "SS", "K", "P" };

        /// <summary>Position groups an overflow player may take a slot from first (backs, line, edges, linebackers, safeties).</summary>
        private static readonly int[][] PositionGroups =
        {
            new[] { 1, 2 }, new[] { 5, 6, 7, 8, 9 }, new[] { 10, 11 }, new[] { 13, 14, 15 }, new[] { 17, 18 },
        };

        /// <summary>NCAA 14 rating column → the CFB 27 ratings it fills.</summary>
        private static readonly (string Ncaa, string[] Cfb)[] RatingMap =
        {
            ("PSPD", new[] { "SpeedRating" }),
            ("PSTR", new[] { "StrengthRating" }),
            ("PAGI", new[] { "AgilityRating", "ChangeOfDirectionRating" }),
            ("PACC", new[] { "AccelerationRating" }),
            ("PJMP", new[] { "JumpingRating" }),
            ("PAWR", new[] { "AwarenessRating" }),
            ("PSTA", new[] { "StaminaRating" }),
            ("PINJ", new[] { "InjuryRating" }),
            ("PCAR", new[] { "CarryingRating" }),
            ("PCTH", new[] { "CatchingRating" }),
            ("TRAF", new[] { "CatchInTrafficRating" }),
            ("SPCT", new[] { "SpectacularCatchRating" }),
            ("PRTR", new[] { "ShortRouteRunningRating", "MediumRouteRunningRating", "DeepRouteRunningRating" }),
            ("RELS", new[] { "ReleaseRating" }),
            ("PJMV", new[] { "JukeMoveRating" }),
            ("PSMV", new[] { "SpinMoveRating" }),
            ("PSAR", new[] { "StiffArmRating" }),
            ("PBCV", new[] { "BCVisionRating" }),
            ("PTRK", new[] { "TruckingRating" }),
            ("PBTK", new[] { "BreakTackleRating" }),
            ("PTHP", new[] { "ThrowPowerRating" }),
            ("PTHA", new[]
            {
                "ThrowAccuracyRating", "ThrowAccuracyShortRating", "ThrowAccuracyMidRating", "ThrowAccuracyDeepRating",
                "ThrowOnTheRunRating", "ThrowUnderPressureRating", "PlayActionRating",
            }),
            ("PPBK", new[] { "PassBlockRating" }),
            ("PPBF", new[] { "PassBlockFinesseRating" }),
            ("PPBS", new[] { "PassBlockPowerRating" }),
            ("PRBK", new[] { "RunBlockRating" }),
            ("PBFW", new[] { "RunBlockFinesseRating" }),
            ("PRBS", new[] { "RunBlockPowerRating" }),
            ("PIBL", new[] { "ImpactBlockingRating" }),
            ("PPRC", new[] { "PlayRecognitionRating" }),
            ("PPRS", new[] { "PursuitRating" }),
            ("PBSH", new[] { "BlockSheddingRating" }),
            ("PPMV", new[] { "PowerMovesRating" }),
            ("PFMV", new[] { "FinesseMovesRating" }),
            ("PTAK", new[] { "TackleRating" }),
            ("PHIT", new[] { "HitPowerRating" }),
            ("PMCV", new[] { "ManCoverageRating" }),
            ("PZCV", new[] { "ZoneCoverageRating" }),
            ("PYRS", new[] { "PressRating" }),
            ("PKPR", new[] { "KickPowerRating" }),
            ("PKAC", new[] { "KickAccuracyRating" }),
            ("PKRT", new[] { "KickReturnRating" }),
            ("POVR", new[] { "OverallRating" }),
        };

        /// <summary>
        /// cities.csv blocks: 256 ids per state, alphabetical, the same numbering as CFB 27's StateName (0 Alabama ... 49
        /// Wyoming). Block 50 is Canada (NonUS) and 51 is DC, which CFB 27 files under Maryland.
        /// </summary>
        private const int CityBlockSize = 256;
        private const int CanadaBlock = 50;
        private const int DcBlock = 51;
        private const int MarylandState = 19;

        private const int NoTeamIndex = 255;

        /// <summary>NCAA 14 PGHE (skin tone): 1-99 light, 100-159 medium, 160+ dark.</summary>
        private static int SkinGroup(int pghe) => pghe < 100 ? 0 : pghe < 160 ? 1 : 2;

        /// <summary>CFB 27 generic head skin digit (1-8): 1-3 light, 4-5 medium, 6-8 dark.</summary>
        private static int SkinGroupOfHead(int skin) => skin <= 3 ? 0 : skin <= 5 ? 1 : 2;

        /// <summary>A CFB 27 generic player head: Generic_&lt;portrait&gt;_P_T####_[DHMT]_&lt;skin 1-8&gt;_&lt;variant&gt;.</summary>
        private static readonly Regex GenericPlayerHead = new Regex(@"^Generic_(\d+)_P_T\d+_[DHMT]_([1-8])_");

        private readonly FranchiseFileTable players;
        private readonly FranchiseFileTable teams;
        private readonly Dictionary<int, (string Town, int Block)> cities;
        private readonly Dictionary<string, string> stateNames;

        // hometown lookups built from the CFB 27 players before anything changes
        private readonly Dictionary<(string Town, string State), (string Pipeline, string NearestCity)> hometowns =
            new Dictionary<(string, string), (string, string)>();
        private readonly Dictionary<string, (string Pipeline, string NearestCity)> stateDefaults = new Dictionary<string, (string, string)>();
        private readonly Dictionary<string, string> archetypeByPosition;

        /// <summary>
        /// Head recipes worn by CFB 27 players with generic heads, by skin group, one entry per wearer so common heads come up
        /// more often. The portrait is the head's number and PLYR_GENERICHEAD is carried from the same wearer.
        /// </summary>
        private readonly List<(string Head, int Portrait, string GenericHead)>[] headsBySkin =
            { new List<(string, int, string)>(), new List<(string, int, string)>(), new List<(string, int, string)>() };

        private readonly List<string> report = new List<string>();
        private int ported, converted, renumbered, hometownsMatched, hometownsByState, facesChanged;

        private Ncaa14RosterImporter(FranchiseFile dynasty, string citiesPath)
        {
            players = dynasty.GetPrimaryTable("Player", "FirstName");
            players.ReadRecords();
            teams = dynasty.GetPrimaryTable("Team", "DisplayName");
            teams.ReadRecords();
            cities = ReadCities(citiesPath);

            stateNames = players.OffsetTable.First(o => o.Name == "PLYR_HOME_STATE").Enum!.Members
                .Where(m => !m.Name.EndsWith("_") && m.Name != "INVALID")
                .GroupBy(m => m.Value)
                .ToDictionary(g => g.Key.ToString(), g => g.First().Name);

            var rostered = players.Records.Where(r => !r.IsEmpty).ToList();
            foreach (var town in rostered.GroupBy(r => ((r["PLYR_HOME_TOWN"] as string ?? string.Empty).ToLowerInvariant(), r["PLYR_HOME_STATE"] as string ?? string.Empty)))
            {
                hometowns[town.Key] = MostCommonHometown(town);
            }

            foreach (var state in rostered.GroupBy(r => r["PLYR_HOME_STATE"] as string ?? string.Empty))
            {
                stateDefaults[state.Key] = MostCommonHometown(state);
            }

            foreach (var r in rostered)
            {
                var head = r["GenericHeadAssetName"] as string ?? string.Empty;
                var match = GenericPlayerHead.Match(head);
                if (match.Success)
                {
                    headsBySkin[SkinGroupOfHead(int.Parse(match.Groups[2].Value))]
                        .Add((head, int.Parse(match.Groups[1].Value), r["PLYR_GENERICHEAD"] as string ?? string.Empty));
                }
            }

            // a converted player needs an archetype that fits the new position
            archetypeByPosition = rostered
                .Where(r => Convert.ToInt32(r["TeamIndex"]) != NoTeamIndex)
                .GroupBy(r => r["Position"] as string ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.GroupBy(r => r["PlayerType"] as string).OrderByDescending(t => t.Count()).First().Key ?? string.Empty);
        }

        /// <summary>Ports every mapped team. Returns a report: a summary line, then anything skipped.</summary>
        public static List<string> Import(FranchiseFile dynasty, string playCsvPath, string citiesPath) =>
            new Ncaa14RosterImporter(dynasty, citiesPath).Run(playCsvPath);

        private static (string Pipeline, string NearestCity) MostCommonHometown(IEnumerable<FranchiseFileRecord> group) =>
            group.GroupBy(r => (r["HomePipeline"] as string ?? string.Empty, r["NearestCity"] as string ?? string.Empty))
                .OrderByDescending(g => g.Count())
                .First().Key;

        private List<string> Run(string playCsvPath)
        {
            var ncaaPlayers = ReadCsv(playCsvPath).Where(p => p.Int("TGID") != NoNcaaTeam).ToList();
            var teamIndexByOldId = new Dictionary<int, int>();
            var map = CFBTeam.OldTeamIdToTeamRowMap;
            foreach (var oldId in ncaaPlayers.Select(p => p.Int("TGID")).Distinct())
            {
                int cfbId = OldIdAliases.TryGetValue(oldId, out var alias) ? alias : oldId;
                if (!map.TryGetValue(cfbId, out var row))
                {
                    report.Add($"NCAA 14 team {oldId} has no CFB 27 team; its players were skipped.");
                    continue;
                }

                teamIndexByOldId[oldId] = Convert.ToInt32(teams.Records[row]["TeamIndex"]);
            }

            var rosters = players.Records
                .Where(r => !r.IsEmpty && Convert.ToInt32(r["TeamIndex"]) != NoTeamIndex)
                .GroupBy(r => Convert.ToInt32(r["TeamIndex"]))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var team in ncaaPlayers.GroupBy(p => p.Int("TGID")))
            {
                if (teamIndexByOldId.TryGetValue(team.Key, out var teamIndex) && rosters.TryGetValue(teamIndex, out var roster))
                {
                    PortTeam(team.ToList(), roster);
                }
            }

            report.Insert(0,
                $"Ported {ported} players onto {teamIndexByOldId.Count} teams ({converted} changed position to fit). " +
                $"Hometowns: {hometownsMatched} matched a CFB 27 town, {hometownsByState} used their state's usual pipeline. " +
                $"{renumbered} remaining CFB 27 players got new jersey numbers. {facesChanged} ported players got a generic head matching their NCAA 14 skin tone.");
            return report;
        }

        private void PortTeam(List<NcaaPlayer> ncaa, List<FranchiseFileRecord> roster)
        {
            var assignments = new List<(NcaaPlayer Ncaa, FranchiseFileRecord Slot)>();
            var open = roster.ToList();
            var overflow = new List<NcaaPlayer>();

            // best to best within each position
            for (int position = 0; position < Positions.Length; position++)
            {
                var mine = ncaa.Where(p => p.Int("PPOS") == position).OrderByDescending(p => p.Int("POVR")).ThenByDescending(p => p.Int("PAWR")).ToList();
                var theirs = open.Where(r => r["Position"] as string == Positions[position]).OrderByDescending(r => Convert.ToInt32(r["OverallRating"])).ToList();
                for (int i = 0; i < mine.Count; i++)
                {
                    if (i < theirs.Count)
                    {
                        assignments.Add((mine[i], theirs[i]));
                        open.Remove(theirs[i]);
                    }
                    else
                    {
                        overflow.Add(mine[i]);
                    }
                }
            }

            // the rest take the lowest-rated player left in their position group, else from the position with the most left
            foreach (var player in overflow.OrderByDescending(p => p.Int("POVR")))
            {
                int position = player.Int("PPOS");
                var group = PositionGroups.FirstOrDefault(g => g.Contains(position)) ?? new[] { position };
                var candidates = open.Where(r => group.Any(g => r["Position"] as string == Positions[g])).ToList();
                if (candidates.Count == 0)
                {
                    var deepest = open.GroupBy(r => r["Position"] as string).OrderByDescending(g => g.Count()).FirstOrDefault();
                    candidates = deepest?.ToList() ?? new List<FranchiseFileRecord>();
                }

                var slot = candidates.OrderBy(r => Convert.ToInt32(r["OverallRating"])).FirstOrDefault();
                if (slot == null)
                {
                    report.Add($"No roster slot left for {player.Name} ({Positions[position]}, team {player.Int("TGID")}).");
                    continue;
                }

                open.Remove(slot);
                assignments.Add((player, slot));
            }

            foreach (var (player, slot) in assignments)
            {
                Write(player, slot);
            }

            RenumberLeftovers(assignments.Select(a => a.Slot).ToList(), open);
        }

        private void Write(NcaaPlayer p, FranchiseFileRecord r)
        {
            var position = Positions[p.Int("PPOS")];
            if (r["Position"] as string != position)
            {
                r["Position"] = position;
                if (archetypeByPosition.TryGetValue(position, out var archetype) && archetype.Length > 0)
                {
                    r["PlayerType"] = archetype;
                }

                converted++;
            }

            r["FirstName"] = p["PFNA"];
            r["LastName"] = p["PLNA"];
            r["JerseyNum"] = Math.Clamp(p.Int("PJEN"), 0, 99);
            r["Height"] = Math.Clamp(p.Int("PHGT"), 0, 127);
            r["Weight"] = Math.Clamp(p.Int("PWGT"), 0, 255); // pounds - 160 in both games
            r["SchoolYear"] = new[] { "Freshman", "Sophomore", "Junior", "Senior" }[Math.Clamp(p.Int("PYEA"), 0, 3)];
            r["RedshirtStatus"] = p.Int("PRSD") == 2 ? "Previous" : "Eligible";

            foreach (var (ncaa, cfb) in RatingMap)
            {
                var value = Math.Clamp(p.Int(ncaa), 0, 99);
                foreach (var field in cfb)
                {
                    r[field] = value;
                }
            }

            WriteHometown(p, r);
            WriteHead(p, r);
            ported++;
        }

        /// <summary>
        /// A random generic head of the NCAA 14 skin tone; each run picks anew. The head
        /// sets both face and skin (the CharacterVisuals skin value doesn't), and the portrait must match it. AssetName pointed
        /// at the old player's scan, so it's cleared like most generic-head players have it.
        /// </summary>
        private void WriteHead(NcaaPlayer p, FranchiseFileRecord r)
        {
            var pool = headsBySkin[SkinGroup(p.Int("PGHE"))];
            if (pool.Count == 0)
            {
                return;
            }

            var (head, portrait, genericHead) = pool.Choose();
            r["GenericHeadAssetName"] = head;
            r["PLYR_PORTRAIT"] = portrait;
            r["PLYR_GENERICHEAD"] = genericHead;
            r["PLYR_ASSETNAME"] = string.Empty;
            facesChanged++;
        }

        private void WriteHometown(NcaaPlayer p, FranchiseFileRecord r)
        {
            if (!cities.TryGetValue(p.Int("RCHD"), out var city) || city.Town.Length == 0)
            {
                return; // keeps the CFB 27 hometown
            }

            var state = city.Block == CanadaBlock ? "NonUS"
                : city.Block == DcBlock ? stateNames[MarylandState.ToString()]
                : stateNames.TryGetValue(city.Block.ToString(), out var name) ? name : null;
            if (state == null)
            {
                return;
            }

            r["PLYR_HOME_TOWN"] = city.Town;
            r["PLYR_HOME_STATE"] = state;

            if (hometowns.TryGetValue((city.Town.ToLowerInvariant(), state), out var known))
            {
                hometownsMatched++;
            }
            else if (stateDefaults.TryGetValue(state, out known))
            {
                hometownsByState++;
            }
            else
            {
                return;
            }

            r["HomePipeline"] = known.Pipeline;
            r["NearestCity"] = known.NearestCity;
        }

        /// <summary>CFB 27 players who stay on the team keep their number unless a ported player now wears it.</summary>
        private void RenumberLeftovers(List<FranchiseFileRecord> portedSlots, List<FranchiseFileRecord> leftovers)
        {
            var taken = portedSlots.Select(r => Convert.ToInt32(r["JerseyNum"])).ToHashSet();
            foreach (var r in leftovers)
            {
                int number = Convert.ToInt32(r["JerseyNum"]);
                if (taken.Add(number))
                {
                    continue;
                }

                int free = Enumerable.Range(1, 99).Concat(new[] { 0 }).FirstOrDefault(n => !taken.Contains(n), -1);
                if (free < 0)
                {
                    report.Add($"No free jersey number for {r["FirstName"]} {r["LastName"]}.");
                    continue;
                }

                r["JerseyNum"] = free;
                taken.Add(free);
                renumbered++;
            }
        }

        #region csv

        private sealed class NcaaPlayer
        {
            private readonly Dictionary<string, string> values;

            public NcaaPlayer(Dictionary<string, string> values) => this.values = values;

            public string this[string column] => values.TryGetValue(column, out var v) ? v.Trim() : string.Empty;

            public int Int(string column) => int.TryParse(this[column], out var v) ? v : 0;

            public string Name => $"{this["PFNA"]} {this["PLNA"]}";
        }

        /// <summary>PLAY.csv: a header row of NCAA 14 column names. The dump is Windows-1252.</summary>
        private static List<NcaaPlayer> ReadCsv(string path)
        {
            var lines = File.ReadAllLines(path, System.Text.Encoding.Latin1).Where(l => l.Length > 0).ToList();
            var header = lines[0].Split(',');
            return lines.Skip(1)
                .Select(l => l.Split(','))
                .Select(c => new NcaaPlayer(header.Select((h, i) => (h, v: i < c.Length ? c[i] : string.Empty)).ToDictionary(x => x.h, x => x.v)))
                .ToList();
        }

        /// <summary>cities.csv: "id, town, ST" per line (the id is PLAY.RCHD); unused ids have an empty town.</summary>
        private static Dictionary<int, (string Town, int Block)> ReadCities(string path)
        {
            var result = new Dictionary<int, (string, int)>();
            foreach (var line in File.ReadAllLines(path, System.Text.Encoding.Latin1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 2 && int.TryParse(parts[0].Trim(), out var id))
                {
                    result[id] = (parts[1].Trim(), id / CityBlockSize);
                }
            }

            return result;
        }

        #endregion
    }
}
