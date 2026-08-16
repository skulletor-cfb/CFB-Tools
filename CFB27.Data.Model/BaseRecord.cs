using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace CFB27.Data.Model
{
    [JsonObject]
    public class CFB27Table<T> where T : BaseRecord
    {
        [JsonProperty("header")]
        public Dictionary<string, object> Header { get; set; }

        [JsonProperty("fields")]
        public string[] Fields { get; set; }

        [JsonProperty("records")]
        public List<T> Records { get; set; }
    }

    [JsonObject]
    public abstract class BaseRecord 
    {
        private const string RowKey = "_row";
        private const string IsEmptyKey = "_isEmpty";

        [JsonExtensionData]
        protected IDictionary<string, JToken> data { get; set; }

        [JsonProperty(RowKey)]
        public int Row { get; set; }

        [JsonProperty(IsEmptyKey)]
        public bool IsEmpty { get; set; }
    }
}