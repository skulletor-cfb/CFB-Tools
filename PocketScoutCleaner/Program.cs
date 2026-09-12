using Newtonsoft.Json;

var exportDir = @"D:\OneDrive\Documents\EA SPORTS College Football 27\export";
var dataDirs = Directory.GetDirectories(exportDir);
foreach (var dir in dataDirs)
{
    var files = Directory.GetFiles(dir);
    foreach (var file in files)
    {
        var json = File.ReadAllText(file);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        if(data.ContainsKey("meta"))
            data.Remove("meta");
        File.WriteAllText(file, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}
