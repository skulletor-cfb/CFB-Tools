using System.Buffers.Binary;
using MaddenFranchise.Schema;
using MaddenFranchise.Strategies;
using MaddenFranchise.Util;

namespace MaddenFranchise;

public enum TableName
{
    Team,
    BowlGame,
    TeamHistoricalData,
    SeasonGame,
    Story,
}

public sealed class FranchiseFileType
{
    public string Format { get; init; } = "";
    public bool Compressed { get; init; }
    public int? Year { get; set; }
    public string? GameType { get; set; }
}

public sealed class SchemaMetadata
{
    public int? GameYear { get; init; }
    public long Major { get; init; }
    public long Minor { get; init; }
}

public readonly record struct AssetTableEntry(int AssetId, int Reference);

/// <summary>
/// Outcome of <see cref="FranchiseFile.Save"/>: compressed database size vs. the room the save has for
/// it, and whether the slower maximum-compression fallback was needed to make it fit.
/// </summary>
public readonly record struct FranchiseSaveResult(string Destination, int CompressedSize, int Budget, bool UsedMaxCompression = false);

public readonly record struct TableRecordReference(int TableId, string Name, FranchiseFileTable Table);

/// <summary>
/// Port of FranchiseFile.js, narrowed to the "franchise" format (the main single-file save) - the
/// FTC/"franchise-common" companion-file format is out of scope for this port.
/// </summary>
public sealed class FranchiseFile
{
    private const int CompressedDataOffset = 0x52;

    public byte[] RawContents { get; }
    public byte[]? PackedFileContents { get; private set; }
    public byte[] UnpackedFileContents { get; private set; }
    public FranchiseFileType Type { get; }
    public int? GameYear => Type.Year;
    public string GameType => Type.GameType ?? MaddenFranchise.GameType.Madden;
    public SchemaMetadata? ExpectedSchemaVersion { get; }
    public FranchiseFileSettings Settings { get; }
    public FranchiseSchema? SchemaList { get; private set; }
    public List<FranchiseFileTable> Tables { get; private set; } = new();
    public List<AssetTableEntry> AssetTable { get; private set; } = new();
    public bool IsLoaded { get; private set; }
    public string FilePath { get; set; }

    private GameStrategy _strategy = null!;

    private FranchiseFile(string filePath, byte[] rawContents, FranchiseFileSettings settings)
    {
        FilePath = filePath;
        Settings = settings;
        RawContents = rawContents;
        Type = GetFileType(rawContents, settings);

        if (Type.Compressed)
        {
            PackedFileContents = RawContents;
            UnpackedFileContents = Unpack(RawContents, Type);
        }
        else
        {
            UnpackedFileContents = RawContents;
        }

        ExpectedSchemaVersion = GetSchemaMetadata(
            Type.Compressed ? RawContents : UnpackedFileContents, Type);
    }

    public static FranchiseFile Load(string filePath, FranchiseFileSettings? settings = null)
    {
        var file = new FranchiseFile(filePath, File.ReadAllBytes(filePath), settings ?? new FranchiseFileSettings());
        file.Parse();
        return file;
    }

    public static FranchiseFile LoadFromBytes(string virtualPath, byte[] contents, FranchiseFileSettings? settings = null)
    {
        var file = new FranchiseFile(virtualPath, contents, settings ?? new FranchiseFileSettings());
        file.Parse();
        return file;
    }

    private void Parse()
    {
        if (Type.Year is null)
        {
            throw new NotSupportedException("Could not determine the game year for this save file.");
        }

        _strategy = StrategyPicker.Pick(Type.Format, Type.Year.Value, GameType);

        var schemaMeta = ExpectedSchemaVersion
            ?? throw new InvalidOperationException("Could not determine the schema version for this save file.");
        SchemaList = Settings.SchemaPath is not null
            ? FranchiseSchema.LoadFromPath(Settings.SchemaPath)
            : FranchiseSchema.LoadClosest(Type.Year.Value, schemaMeta.Major, schemaMeta.Minor, GameType);

        ScanTables();
        ParseAssetTable();

        foreach (var table in Tables)
        {
            var schema = SchemaList.GetSchema(table.Name);
            if (schema is not null)
            {
                table.Schema = schema;
            }
        }

        IsLoaded = true;
    }

    private void ScanTables()
    {
        var data = UnpackedFileContents;
        var tableIndicies = new List<int>();
        int tableStartOffset = GetTableStartOffsetByGameYear(Type.Year);

        for (int i = 0; i <= data.Length - 4; i++)
        {
            if (MatchesMarker(data, i, 0x53, 0x50, 0x42, 0x46) ||   // "SPBF"
                MatchesMarker(data, i, 0x41, 0x53, 0x54, 0x4f) ||   // "ASTO"
                MatchesMarker(data, i, 0x53, 0x50, 0x45, 0x58))     // "SPEX"
            {
                tableIndicies.Add(i - tableStartOffset);
            }
        }

        Tables = new List<FranchiseFileTable>(tableIndicies.Count);
        for (int i = 0; i < tableIndicies.Count; i++)
        {
            int currentTable = tableIndicies[i];
            int nextTable = tableIndicies.Count > i + 1 ? tableIndicies[i + 1] : data.Length - 8;
            var tableData = data.CopyRange(currentTable, nextTable);

            var table = new FranchiseFileTable(tableData, currentTable, Type.Year!.Value, _strategy, Settings, GameType)
            {
                Index = i
            };
            Tables.Add(table);
            table.Changed += OnTableChanged;
        }
    }

    private void OnTableChanged(FranchiseFileTable table)
    {
        table.IsChanged = true;
        if (Settings.SaveOnChange)
        {
            Save();
        }
        Changed?.Invoke(table);
    }

    public event Action<FranchiseFileTable>? Changed;

    private void ParseAssetTable()
    {
        var data = UnpackedFileContents;
        int assetTableOffset = unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(4, 4)));
        int assetTableEntries = unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(36, 4)));
        var entries = new List<AssetTableEntry>(assetTableEntries);
        int currentOffset = assetTableOffset;
        for (int i = 0; i < assetTableEntries; i++)
        {
            int assetId = unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(currentOffset, 4)));
            int reference = unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(currentOffset + 4, 4)));
            entries.Add(new AssetTableEntry(assetId, reference));
            currentOffset += 8;
        }
        AssetTable = entries;
    }

    /// <summary>
    /// Writes the save. The database is recompressed into its original slot; everything after the
    /// slot (CharacterVisuals and the other chunks) is kept byte-for-byte, and the output is always
    /// the same size as the input. Throws <see cref="SaveSlotOverflowException"/> without writing
    /// anything if the edited database no longer fits.
    /// </summary>
    public FranchiseSaveResult Save(string? outputFilePath = null)
    {
        bool saveOnChangeEnabled = Settings.SaveOnChange;
        Settings.SaveOnChange = false;
        try
        {
            UnpackedFileContents = _strategy.GenerateUnpackedContents(
                Tables.Cast<IPatchUnit>().ToList(), UnpackedFileContents);

            string destination = outputFilePath ?? FilePath;
            if (!Type.Compressed)
            {
                WriteFileSafely(destination, UnpackedFileContents);
                return new FranchiseSaveResult(destination, UnpackedFileContents.Length, UnpackedFileContents.Length);
            }

            byte[] original = PackedFileContents ?? RawContents;
            // .NET Framework's deflate reproduces EA's own stream, so unedited data comes back
            // byte-identical. Saves leave only a few bytes of slack after the stream, though, so
            // when an edit no longer fits, recompress at maximum level (~90 KB smaller than EA's).
            byte[] compressed = Net48Compat.ZlibDeflate(UnpackedFileContents);
            bool usedMaxCompression = false;
            if (compressed.Length > FranchiseFileStrategy.MeasureSlot(original).Budget)
            {
                compressed = Net48Compat.ZlibDeflateMax(UnpackedFileContents);
                usedMaxCompression = true;
            }
            if (!Net48Compat.ZlibInflate(compressed).AsSpan().SequenceEqual(UnpackedFileContents))
            {
                throw new InvalidOperationException(
                    "The recompressed database did not decompress back to the same bytes. Nothing was written.");
            }

            byte[] output = _strategy.PostPackFile(original, compressed);
            WriteFileSafely(destination, output);

            // Later saves measure the slot from this output, so repeated saves keep working.
            PackedFileContents = output;
            return new FranchiseSaveResult(destination, compressed.Length,
                FranchiseFileStrategy.MeasureSlot(output).Budget, usedMaxCompression);
        }
        finally
        {
            if (saveOnChangeEnabled)
            {
                Settings.SaveOnChange = true;
            }
        }
    }

    private static void WriteFileSafely(string destination, byte[] data)
    {
        string temp = destination + ".tmp";
        File.WriteAllBytes(temp, data);
        if (File.Exists(destination))
        {
            File.Copy(temp, destination, overwrite: true);
            File.Delete(temp);
        }
        else
        {
            File.Move(temp, destination);
        }
    }

    /// <summary>
    /// The first table with this name. Many names have several tables (Team has 9 in a typical
    /// dynasty, TeamHistoricalData one per team); use <see cref="GetPrimaryTable"/> for the main one.
    /// </summary>
    public FranchiseFileTable? GetTableByName(string name) => Tables.FirstOrDefault(t => t.Name == name);

    /// <summary>
    /// The main table among those sharing a name: the one with the most rows in use, optionally
    /// limited to tables whose schema has every field in <paramref name="requiredFields"/>.
    /// Returns null when no table with that name (and those fields) exists.
    /// </summary>
    public FranchiseFileTable GetPrimaryTable(TableName name, params string[] requiredFields) => GetPrimaryTable(name.ToString(), requiredFields);
    public FranchiseFileTable GetPrimaryTable(string name, params string[] requiredFields)
    {
        var table = Tables
            .Where(t => t.Name == name)
            .Where(t => requiredFields.Length == 0 ||
                        (t.Schema is not null &&
                         requiredFields.All(f => t.Schema.Attributes.Any(a => a.Name == f))))
            .Select(t => (Table: t, Used: t.CountRowsInUse()))
            .OrderByDescending(x => x.Used)
            .ThenByDescending(x => x.Table.Header.RecordCapacity)
            .Select(x => x.Table)
            .FirstOrDefault();
        table.ReadRecords();
        return table;
    }

    public List<FranchiseFileTable> GetAllTablesByName(string name) => Tables.Where(t => t.Name == name).ToList();

    public FranchiseFileTable? GetTableById(int id) => Tables.FirstOrDefault(t => t.Header.TableId == id);

    public FranchiseFileTable? GetTableByIndex(int index) => index >= 0 && index < Tables.Count ? Tables[index] : null;

    public FranchiseFileTable? GetTableByUniqueId(int id) => Tables.FirstOrDefault(t => t.Header.UniqueId == id);

    public FranchiseFileRecord? GetReferencedRecord(string referenceValue)
    {
        var reference = BinaryUtil.GetReferenceData(referenceValue);
        var table = GetTableById(reference.TableId);
        if (table is null || reference.RowNumber >= table.Records.Count)
        {
            return null;
        }
        return table.Records[reference.RowNumber];
    }

    public RecordReference? GetReferenceFromAssetId(int assetId)
    {
        foreach (var entry in AssetTable)
        {
            if (entry.AssetId == assetId)
            {
                string binary = BinaryUtil.Dec2Bin(unchecked((uint)entry.Reference)).PadLeft(32, '0');
                return BinaryUtil.GetReferenceData(binary);
            }
        }
        return null;
    }

    public List<TableRecordReference>? GetReferencesToRecord(int tableId, int recordIndex)
    {
        var referencedTable = GetTableById(tableId);
        if (referencedTable is null)
        {
            return null;
        }

        uint referenceValue = unchecked((uint)BinaryUtil.Bin2Dec(BinaryUtil.GetBinaryReferenceData(tableId, recordIndex)));
        var needle = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(needle, referenceValue);

        return Tables
            .Where(table =>
            {
                if (table.Schema is not null)
                {
                    return table.Schema.Attributes.Any(a => a.Type == referencedTable.Name || a.Type == "record");
                }
                if (table.IsArray && referencedTable.Schema is not null)
                {
                    string baseName = table.Name.Length >= 2 ? table.Name.Substring(0, table.Name.Length - 2) : table.Name;
                    return baseName == referencedTable.Name || baseName == referencedTable.Schema.Base;
                }
                if (table.IsArray)
                {
                    string baseName = table.Name.Length >= 2 ? table.Name.Substring(0, table.Name.Length - 2) : table.Name;
                    return baseName == referencedTable.Name;
                }
                return false;
            })
            .Where(table => IndexOfBytes(table.Data, needle) != -1)
            .Select(table => new TableRecordReference(table.Header.TableId, table.Name, table))
            .ToList();
    }

    private static int IndexOfBytes(byte[] haystack, byte[] needle)
    {
        if (needle.Length == 0 || haystack.Length < needle.Length)
        {
            return -1;
        }
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { match = false; break; }
            }
            if (match) return i;
        }
        return -1;
    }

    private static bool MatchesMarker(byte[] data, int i, byte b0, byte b1, byte b2, byte b3) =>
        data[i] == b0 && data[i + 1] == b1 && data[i + 2] == b2 && data[i + 3] == b3;

    private static int GetTableStartOffsetByGameYear(int? gameYear) => gameYear switch
    {
        19 => 0x90,
        _ => 0x94
    };

    private static byte[] Unpack(byte[] data, FranchiseFileType type)
    {
        int offset = type.Format == MaddenFranchise.FileFormat.Franchise ? CompressedDataOffset : 0;
        return Net48Compat.ZlibInflate(data.CopyRange(offset));
    }

    // --- File type / schema-version detection (ported from FranchiseFile.js) ---

    private static FranchiseFileType GetFileType(byte[] data, FranchiseFileSettings settings)
    {
        bool compressed = IsCompressed(data);
        string format = GetFormat(data, compressed);
        int? year = settings.GameYearOverride ?? GetGameYear(data, compressed, format);
        string? gameType = settings.GameTypeOverride ?? GetGameType(data, compressed, format, year);
        return new FranchiseFileType { Format = format, Compressed = compressed, Year = year, GameType = gameType };
    }

    private static bool IsCompressed(byte[] data)
    {
        ReadOnlySpan<byte> decompressedHeader = new byte[] { 0x46, 0x72, 0x54, 0x6b }; // "FrTk"
        return data.Length < 4 || !data.AsSpan(0, 4).SequenceEqual(decompressedHeader);
    }

    private static string GetFormat(byte[] data, bool compressed)
    {
        if (compressed)
        {
            ReadOnlySpan<byte> zlibHeader = new byte[] { 0x78, 0x9c };
            if (data.Length >= 2 && data.AsSpan(0, 2).SequenceEqual(zlibHeader))
            {
                return MaddenFranchise.FileFormat.FranchiseCommon;
            }
            return MaddenFranchise.FileFormat.Franchise;
        }
        return data.Length > 0x895440 ? MaddenFranchise.FileFormat.Franchise : MaddenFranchise.FileFormat.FranchiseCommon;
    }

    private static int? GetGameYear(byte[] data, bool compressed, string format)
    {
        var schemaMax = new (int Year, int Max)[] { (19, 95), (27, 999) };

        if (compressed)
        {
            if (format == MaddenFranchise.FileFormat.FranchiseCommon)
            {
                return null;
            }

            byte y0 = data[0x22], y2 = data[0x24];
            if (y0 == 0x52) return 19;
            if (y2 == 0x30) return 20;
            if (y2 == 0x31) return 21;
            if (y2 == 0x32) return 22;
            if (y2 == 0x33) return 23;
            if (y2 == 0x34 || data[0x2a] == 0x34) return 24;
            if (data[0x2a] == 0x35) return 25;
            if (data[0x2a] == 0x36) return 26;
            if (data[0x2a] == 0x37 || data[0x2b] == 0x37) return 27;

            long schemaMajor = GetCompressedSchema(data).Major;
            foreach (var s in schemaMax)
            {
                if (s.Max >= schemaMajor) return s.Year;
            }
            return null;
        }
        else
        {
            long schemaMajor = GetDecompressedM20Schema(data).Major;
            if (schemaMajor == 0)
            {
                return 19;
            }
            return IndexOfBytes(data, System.Text.Encoding.ASCII.GetBytes("M21")) > -1 ? 21 : null;
        }
    }

    private static string? GetGameType(byte[] data, bool compressed, string format, int? year)
    {
        if (year is not null && year <= 26)
        {
            return MaddenFranchise.GameType.Madden;
        }

        if (compressed && format == MaddenFranchise.FileFormat.Franchise)
        {
            byte y0 = data[0x22];
            if (y0 == 0x43)
            {
                return MaddenFranchise.GameType.College;
            }
            if (year == 27 && data[0x2b] == 0x37 && data[0x2a] != 0x37)
            {
                return MaddenFranchise.GameType.College;
            }
        }

        return MaddenFranchise.GameType.Madden;
    }

    private static SchemaMetadata? GetSchemaMetadata(byte[] data, FranchiseFileType type)
    {
        if (type.Compressed)
        {
            if (type.Format == MaddenFranchise.FileFormat.FranchiseCommon)
            {
                return null;
            }
            var schemaData = GetCompressedSchema(data);
            return new SchemaMetadata { GameYear = type.Year, Major = schemaData.Major, Minor = schemaData.Minor };
        }
        else
        {
            if (type.Year == 19)
            {
                return new SchemaMetadata { GameYear = type.Year, Major = 0, Minor = 0 };
            }
            var schemaData = GetDecompressedM20Schema(data);
            return new SchemaMetadata { GameYear = type.Year, Major = schemaData.Major, Minor = schemaData.Minor };
        }
    }

    private static (long Major, long Minor) GetCompressedSchema(byte[] data) =>
        (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0x3e, 4)), BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(0x42, 4)));

    private static (long Major, long Minor) GetDecompressedM20Schema(byte[] data) =>
        (BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(0x2c, 4)), BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(0x28, 4)));
}
