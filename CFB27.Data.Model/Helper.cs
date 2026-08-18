using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CFB27.Data.Model
{
    public static class Helper
    {
        public static CFBTable<T> ReadJson<T>(this string file) where T : BaseRecord
        {
            return JsonConvert.DeserializeObject<CFBTable<T>>(File.ReadAllText(file));
        }

        public static string WriteJson(this object payload)
        {
            return JsonConvert.SerializeObject(payload, Formatting.Indented);
        }

        public static int ToTableId(this string id, int prefixLength = 15)
        {
            return Convert.ToInt32(id.Substring(0, prefixLength), 2);
        }

        public static int ToRowId(this string id, int suffixLength = 12)
        {
            return Convert.ToInt32(id.Substring(id.Length - suffixLength), 2);
        }

        public static int ToInt32(this string id) => Convert.ToInt32(id, 2);
    }
}