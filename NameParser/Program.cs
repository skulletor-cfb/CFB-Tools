using NameParser;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
var file = @"D:\CFB27\RO27-Official-V3.4-win-x64-portable\data\weightedNamePools.json";
var namesFile = @"D:\repos\CFB-Tools\Release\names.txt";
var data = JsonConvert.DeserializeObject<NameModel>(File.ReadAllText(file));
var names = JsonConvert.DeserializeObject<NamesFile>(File.ReadAllText(namesFile));

if (names.WFN == null) { names.WFN = new List<string>(); names.WLN = new List<string>(); }
if (names.CLN == null) { names.CLN = new List<string>(); }

// black names first
names.First.AddRange(Extract(data.BlackFirstNames,9));
names.Last.AddRange(Extract(data.BlackLastNames,12));
names.HIFN.AddRange(Extract(data.HawaiianFirstNames, 9));
names.HILN.AddRange(Extract(data.HawaiianLastNames, 12));
names.WFN.AddRange(Extract(data.WhiteFirstNames, 9));
names.WLN.AddRange(Extract(data.WhiteLastNames, 12));
names.CLN.AddRange(Extract(data.CajunNames, 12));
File.WriteAllText(namesFile.Replace("names.txt", "namesnew.txt"), JsonConvert.SerializeObject(names));


string[] Extract(IName[] names, int length)
{
    return names.Select(n => n.name).Where(n => n.Length <= length).ToArray();
}
