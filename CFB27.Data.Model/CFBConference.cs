using MaddenFranchise;
using System.Collections.Generic;
using System.Linq;

namespace CFB27.Data.Model
{
    /// <summary>
    /// A conference with its divisions and member teams (Conference.Divisions -> Division.Teams), using the classic
    /// NCAA 14 conference and division ids the web page creator is built around.
    /// </summary>
    public class CFBConference
    {
        /// <summary>Classic division id for conferences without divisions.</summary>
        public const int NoDivisionId = 30;

        /// <summary>Classic conference id by the save's Conference.ConferenceEnum.</summary>
        public static readonly IReadOnlyDictionary<string, int> ClassicIds = new Dictionary<string, int>
        {
            { "ACC", 0 },
            { "BigTen", 1 },
            { "Big12", 2 },
            { "AAC", 3 },
            { "CUSA", 4 },
            { "Independents", 5 },
            { "MAC", 7 },
            { "MWC", 9 },
            { "Pac12", 10 },
            { "SEC", 11 },
            { "SunBelt", 13 },
        };

        /// <summary>Classic conference name by classic id (the save's names differ, e.g. CUSA, MW, Independent).</summary>
        public static readonly IReadOnlyDictionary<int, string> ClassicNames = new Dictionary<int, string>
        {
            { 0, "ACC" },
            { 1, "Big Ten" },
            { 2, "Big 12" },
            { 3, "American" },
            { 4, "C-USA" },
            { 5, "Independents" },
            { 7, "MAC" },
            { 9, "MWC" },
            { 10, "Pac-12" },
            { 11, "SEC" },
            { 13, "Sun Belt" },
        };

        /// <summary>Classic division ids by conference enum and division name; only the Sun Belt still has divisions.</summary>
        private static readonly IReadOnlyDictionary<string, int> ClassicDivisionIds = new Dictionary<string, int>
        {
            { "SunBelt/West", 16 },
            { "SunBelt/East", 17 },
        };

        public int Row { get; set; }
        public int ClassicId { get; set; }

        /// <summary>the conference's name in the save</summary>
        public string Name { get; set; }

        public string ClassicName => ClassicNames.TryGetValue(this.ClassicId, out var name) ? name : this.Name;
        public List<CFBDivision> Divisions { get; } = new List<CFBDivision>();

        /// <summary>Team table rows of every member, across divisions.</summary>
        public IEnumerable<int> TeamRows => this.Divisions.SelectMany(d => d.TeamRows);

        public bool HasDivisions => this.Divisions.Any(d => d.ClassicId != NoDivisionId);

        /// <summary>Classic id for a Conference record, or -1 for unnamed slots and unmapped conferences.</summary>
        public static int ToClassicId(FranchiseFileRecord conference)
        {
            if (conference == null || conference.IsEmpty || string.IsNullOrEmpty(conference["Name"] as string))
            {
                return -1;
            }

            return ClassicIds.TryGetValue(conference["ConferenceEnum"] as string ?? string.Empty, out var id) ? id : -1;
        }

        /// <summary>
        /// Team table row -> classic conference id for every team in a conference, as the save has them (membership can
        /// change between seasons). The generic FCS teams are in no conference and aren't included.
        /// </summary>
        public static Dictionary<int, int> ReadTeamConferences(FranchiseFile dynasty) =>
            Read(dynasty)
                .SelectMany(c => c.Divisions.SelectMany(d => d.TeamRows).Select(row => (Row: row, c.ClassicId)))
                .ToDictionary(t => t.Row, t => t.ClassicId);

        /// <summary>The save's conferences (named slots only), with divisions and member teams.</summary>
        public static List<CFBConference> Read(FranchiseFile dynasty)
        {
            var conferences = new List<CFBConference>();
            var table = dynasty.GetPrimaryTable("Conference", "TeamSlots");
            var teamTableId = dynasty.GetPrimaryTable(TableName.Team).Header.TableId;

            foreach (var record in table.Records)
            {
                var classicId = ToClassicId(record);
                if (classicId < 0)
                {
                    continue;
                }

                var conference = new CFBConference
                {
                    Row = record.Index,
                    ClassicId = classicId,
                    Name = record["Name"] as string,
                };

                var conferenceEnum = record["ConferenceEnum"] as string;
                foreach (var divisionRecord in dynasty.FollowArray(record, "Divisions"))
                {
                    var name = divisionRecord["Name"] as string;
                    var teams = dynasty.Follow(divisionRecord.GetReferenceDataByKey("Teams").Value);
                    conference.Divisions.Add(new CFBDivision
                    {
                        // single-division conferences name theirs "Division 1"
                        ClassicId = ClassicDivisionIds.TryGetValue($"{conferenceEnum}/{name}", out var divisionId) ? divisionId : NoDivisionId,
                        Name = name,
                        TeamRows = teams == null
                            ? new List<int>()
                            : teams.FieldsArray
                                .Select(f => f.ReferenceData)
                                .Where(r => r.HasValue && r.Value.TableId == teamTableId)
                                .Select(r => r.Value.RowNumber)
                                .ToList(),
                    });
                }

                conferences.Add(conference);
            }

            return conferences;
        }
    }

    public class CFBDivision
    {
        /// <summary>Classic division id; <see cref="CFBConference.NoDivisionId"/> for a conference's only division.</summary>
        public int ClassicId { get; set; }
        public string Name { get; set; }
        public List<int> TeamRows { get; set; }
    }
}
