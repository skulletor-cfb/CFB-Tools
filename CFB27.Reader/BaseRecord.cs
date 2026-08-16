using Newtonsoft.Json;
using System;

namespace CFB27.Reader
{
    [JsonObject]
    public abstract class BaseRecord
    {
        [JsonProperty(PropertyName ="_row")]
        public int Row { get; set; }

        [JsonProperty(PropertyName = "_isEmpty")]
        public bool IsEmpty { get; set; }
    }

    [JsonObject]
    public class TeamRecord : BaseRecord
    {
    }
}
