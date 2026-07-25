using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor.DataEngine.Models
{
    [JsonObject]
    public class CFB27Payload<T>
    {
        [JsonProperty(PropertyName ="records")]
        public T[] Records { get; set; }
    }
}
