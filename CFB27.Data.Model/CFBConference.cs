using Newtonsoft.Json;

namespace CFB27.Data.Model
{
    public class CFBConference : BaseRecord
    {
        public string Name { get; set; }
        public string Divisions { get; set; }

        [JsonIgnore]
        public bool HasDivisions => !string.Equals(this.Divisions, NoRefString);

        [JsonIgnore]
        public int DivisionListRow => this.Divisions.ToRowId();
    }

    /// <summary>
    /// wrapper array table a Conference's Divisions field points at; each slot is itself a reference into CFBDivision
    /// </summary>
    public class CFBDivisionList : BaseRecord
    {
        public string Division0 { get; set; }
        public string Division1 { get; set; }
    }

    public class CFBDivision : BaseRecord
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
