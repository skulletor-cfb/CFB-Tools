using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace NCAA14Utility
{
    public static class JsonHelpers
    {
        public static T ReadJsonFile<T>(this string file)
        {
            var json = File.ReadAllText(file);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void WriteJsonFile<T>(this T obj, string file)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(file, json);
        }

    }
}