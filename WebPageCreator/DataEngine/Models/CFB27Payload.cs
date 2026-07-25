using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.DataEngine.Models
{
    [JsonObject]
    public class CFB27Payload<T> where T : CFB27Record
    {
        [JsonProperty(PropertyName = "records")]
        public T[] Records { get; set; }
    }

    [JsonObject]
    public abstract class CFB27Record
    {
        protected CFB27Record()
        {
        }

        [JsonProperty(PropertyName = "_row")]
        public int Row { get; set; }

        [JsonProperty(PropertyName = "_isEmpty")]
        public bool IsEmpty { get; set; }
    }
}