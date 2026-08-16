using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace CFB27.Data.Model
{
    [JsonObject]
    public class Table<T> where T : BaseRecord
    {
        [JsonProperty("header")]
        public Dictionary<string, object> Header { get; set; }

        [JsonProperty("fields")]
        public Dictionary<string, object> Fields { get; set; }

        [JsonProperty("records")]
        public List<T> Records { get; set; }
    }

    [JsonObject]
    public abstract class BaseRecord : Dictionary<string, object>
    {
        private const string RowKey = "_row";
        private const string IsEmptyKey = "_isEmpty";

        [JsonIgnore]
        public int Row
        {
            get => (int)this[RowKey];
            set => this[RowKey] = value;
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get => (bool)this[IsEmptyKey];
            set => this[IsEmptyKey] = value;
        }
    }

    [JsonObject]
    public class TeamRecord : BaseRecord
    {
    }
}
