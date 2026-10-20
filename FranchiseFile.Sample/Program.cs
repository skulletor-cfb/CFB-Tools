using MaddenFranchise;

string path = args.Length > 0
    ? args[0]
    : @"D:\OneDrive\Documents\EA SPORTS College Football 27\saves\DYNASTY-Y26P";


Console.WriteLine($"Loading: {path}");
var franchise = FranchiseFile.Load(path);


Console.WriteLine($"Format: {franchise.Type.Format}, GameYear: {franchise.GameYear}, GameType: {franchise.GameType}, Compressed: {franchise.Type.Compressed}");
Console.WriteLine($"Expected schema: major={franchise.ExpectedSchemaVersion?.Major}, minor={franchise.ExpectedSchemaVersion?.Minor}");
Console.WriteLine($"Loaded schema: major={franchise.SchemaList?.Major}, minor={franchise.SchemaList?.Minor}, table count in schema={franchise.SchemaList?.Schemas.Count}");
Console.WriteLine($"Table count: {franchise.Tables.Count}");

int withSchema = franchise.Tables.Count(t => t.Schema is not null);
int withoutSchema = franchise.Tables.Count - withSchema;
Console.WriteLine($"Tables with schema attached: {withSchema}, without: {withoutSchema}");

var missingSchemaSample = franchise.Tables.Where(t => t.Schema is null).Select(t => t.Name).Distinct().Take(15).ToList();
if (missingSchemaSample.Count > 0)
{
    Console.WriteLine("Sample of tables missing a schema: " + string.Join(", ", missingSchemaSample));
}

var seasonGameTables = franchise.GetAllTablesByName("SeasonGame");
Console.WriteLine($"\nSeasonGame table instances: {seasonGameTables.Count}");
foreach (var table in seasonGameTables)
{
    table.ReadRecords();
    Console.WriteLine($"  Table id={table.Header.TableId} records={table.Records.Count} numMembers={table.Header.NumMembers}");
}

var bigTable = seasonGameTables.OrderByDescending(t => { t.ReadRecords(); return t.Records.Count; }).First();
{
    var table = bigTable;
    var fieldNames = table.Schema?.Attributes.Select(a => a.Name).ToList() ?? new();
    Console.WriteLine("\nSample SeasonGame fields available: " + string.Join(", ", fieldNames.Take(20)));

    Console.WriteLine("\nFirst 10 non-empty, played SeasonGame records (biggest table):");
    int shown = 0;
    foreach (var record in table.Records)
    {
        if (record.IsEmpty) continue;
        var awayTeam = record.GetFieldByKey("AwayTeam");
        var homeTeam = record.GetFieldByKey("HomeTeam");
        var awayRef = awayTeam?.ReferenceData;
        if (awayRef is null || (awayRef.Value.TableId == 0 && awayRef.Value.RowNumber == 0)) continue;

        var homeScore = record.GetFieldByKey("HomeScore");
        var awayScore = record.GetFieldByKey("AwayScore");
        Console.WriteLine(
            $"  idx={record.Index} homeRef={homeTeam?.ReferenceData} awayRef={awayTeam?.ReferenceData} score={awayScore?.Value}-{homeScore?.Value}");
        shown++;
        if (shown >= 10) break;
    }
}

// Round-trip save verification.
string scratchDir = Path.Combine(Path.GetTempPath(), "franchisefile-sample");
Directory.CreateDirectory(scratchDir);
string scratchPath = Path.Combine(scratchDir, "roundtrip-output");
Console.WriteLine($"\nRound-trip saving (no edits) to: {scratchPath}");
franchise.Save(scratchPath);

var reloaded = FranchiseFile.Load(scratchPath);
Console.WriteLine($"Reloaded table count: {reloaded.Tables.Count} (original: {franchise.Tables.Count})");

var origSeasonGame = franchise.GetTableByName("SeasonGame");
var reloadedSeasonGame = reloaded.GetTableByName("SeasonGame");
if (origSeasonGame is not null && reloadedSeasonGame is not null)
{
    reloadedSeasonGame.ReadRecords();
    Console.WriteLine($"SeasonGame record count matches: {origSeasonGame.Records.Count == reloadedSeasonGame.Records.Count} " +
                       $"({origSeasonGame.Records.Count} vs {reloadedSeasonGame.Records.Count})");
}

bool byteIdentical = franchise.RawContents.Length == reloaded.RawContents.Length &&
                      franchise.RawContents.AsSpan().SequenceEqual(reloaded.RawContents);
Console.WriteLine($"Round-trip output byte-identical to original (expected False - different deflate impl): {byteIdentical}");

bool unpackedIdentical = franchise.UnpackedFileContents.Length == reloaded.UnpackedFileContents.Length &&
                          franchise.UnpackedFileContents.AsSpan().SequenceEqual(reloaded.UnpackedFileContents);
Console.WriteLine($"Round-trip DECOMPRESSED content byte-identical (expected True): {unpackedIdentical}");

// --- Mutation test: flip AwayScore on one played game, save, reload, verify the write stuck and
// nothing else in the file changed. ---
Console.WriteLine("\nMutation test: editing AwayScore on one record...");
var mutTable = franchise.GetAllTablesByName("SeasonGame").OrderByDescending(t => { t.ReadRecords(); return t.Records.Count; }).First();
var targetRecord = mutTable.Records.FirstOrDefault(r =>
{
    if (r.IsEmpty) return false;
    var away = r.GetFieldByKey("AwayTeam")?.ReferenceData;
    return away is not null && !(away.Value.TableId == 0 && away.Value.RowNumber == 0);
});

if (targetRecord is null)
{
    Console.WriteLine("  No suitable record found to mutate - skipping.");
}
else
{
    long originalScore = Convert.ToInt64(targetRecord.GetFieldByKey("AwayScore")!.Value);
    long newScore = originalScore + 7;
    Console.WriteLine($"  Record idx={targetRecord.Index}: AwayScore {originalScore} -> {newScore}");
    targetRecord["AwayScore"] = newScore;

    string mutPath = Path.Combine(scratchDir, "mutated-output");
    franchise.Save(mutPath);

    var mutReloaded = FranchiseFile.Load(mutPath);
    var mutTableReloaded = mutReloaded.GetAllTablesByName("SeasonGame").First(t => t.Header.TableId == mutTable.Header.TableId);
    mutTableReloaded.ReadRecords();
    long reloadedScore = Convert.ToInt64(mutTableReloaded.Records[targetRecord.Index].GetFieldByKey("AwayScore")!.Value);
    Console.WriteLine($"  Reloaded AwayScore: {reloadedScore} (expected {newScore}) -> {(reloadedScore == newScore ? "PASS" : "FAIL")}");

    // Spot-check a handful of other fields/records are unaffected.
    var otherRecord = mutTableReloaded.Records.FirstOrDefault(r => r.Index != targetRecord.Index && !r.IsEmpty);
    if (otherRecord is not null)
    {
        var origOther = mutTable.Records[otherRecord.Index];
        bool sameAwayTeam = Equals(origOther.GetFieldByKey("AwayTeam")?.Value, otherRecord.GetFieldByKey("AwayTeam")?.Value);
        Console.WriteLine($"  Unrelated record idx={otherRecord.Index} AwayTeam unchanged: {sameAwayTeam}");
    }
}

Console.WriteLine("\nDone.");
