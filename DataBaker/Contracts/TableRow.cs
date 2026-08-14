using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DataBaker.Contracts
{
    [JsonObject]
    public class TableSet
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor Awards { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor AllAmericans { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor BowlWins { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor ConferenceChampionships { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Coach CoachBio { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor CoachCareer { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor CoachHistory { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor TeamHistory { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor[] AllTimeGreats { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Debug { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TableDescriptor CoachH2HSummary { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, TableDescriptor> CoachH2HDrilldown { get; set; }
    }

    [JsonObject]
    public class TableDescriptor
    {
        public TableDescriptor() : this(new List<TableRow>()) { }

        public TableDescriptor(List<TableRow> rows)
        {
            Rows = rows;
        }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public List<TableRow> Rows { get; set; }

    }

    [JsonObject]
    [KnownType(typeof(AllAmericanTableRow))]
    public class TableRow
    {
        public TableRow() : this(new string[0])
        {
        }

        public TableRow(params string[] cells) : this(0, cells)
        {
        }

        public TableRow(int year, params string[] cells)
        {
            Year = year;
            this.Cells = new List<string>(cells);
        }

        [JsonProperty]
        public List<string> Cells { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Year { get; set; }
    }

    [JsonObject]
    public class AllAmericanTableRow : TableRow
    {
        private AllAmerican aa;
        public AllAmericanTableRow(AllAmerican aa, params string[] cells) : base(cells)
        {
            this.aa = aa;
        }

        public int TeamNum { get { return aa.AATeamInt; } }
        public int Pos { get { return aa.Pos; } }
    }
}