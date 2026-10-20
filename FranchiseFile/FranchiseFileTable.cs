using System.Buffers.Binary;
using MaddenFranchise.Schema;
using MaddenFranchise.Strategies;
using MaddenFranchise.Util;

namespace MaddenFranchise;

/// <summary>Port of FranchiseFileTable.js.</summary>
public sealed class FranchiseFileTable : IPatchUnit
{
    private TableSchema? _schema;

    public byte[] Data { get; private set; }
    public int LengthAtLastSave { get; set; }
    public int Offset { get; set; }
    public GameStrategy Strategy { get; }
    public bool RecordsRead { get; private set; }
    public int GameYear { get; }
    public string GameType { get; }
    public TableHeader Header { get; private set; }
    public string Name => Header.Name;
    public bool IsArray => Header.IsArray;
    public List<OffsetTableEntry> LoadedOffsets { get; private set; } = new();
    public bool IsChanged { get; set; }
    public List<FranchiseFileRecord> Records { get; private set; } = new();
    public List<FranchiseFileTable2Field> Table2Records { get; private set; } = new();
    public List<FranchiseFileTable3Field> Table3Records { get; private set; } = new();
    public List<int> ArraySizes { get; private set; } = new();
    public EmptyRecordMap EmptyRecords { get; private set; } = new();
    public List<OffsetTableEntry> OffsetTable { get; private set; } = new();
    public int Index { get; set; } = -1;

    private readonly FranchiseFileSettings _settings;

    public event Action<FranchiseFileTable>? Changed;

    public FranchiseFileTable(byte[] data, int offset, int gameYear, GameStrategy strategy, FranchiseFileSettings settings, string gameType)
    {
        Data = data;
        LengthAtLastSave = data.Length;
        Offset = offset;
        Strategy = strategy;
        RecordsRead = false;
        GameYear = gameYear;
        GameType = string.IsNullOrEmpty(gameType) ? MaddenFranchise.GameType.Madden : gameType;
        Header = strategy.ParseHeader(data);
        _settings = settings;
    }

    public byte[] HexData
    {
        get
        {
            UpdateBuffer();
            return Data;
        }
    }

    public TableSchema? Schema
    {
        get => _schema;
        set
        {
            if (value is null || value.Attributes.Count != Header.NumMembers)
            {
                return;
            }
            _schema = value;
            var modified = Strategy.ParseHeaderAttributesFromSchema(value, Data, Header);
            Header.HeaderSize = modified.HeaderSize;
            Header.Record1Size = modified.Record1Size;
            Header.Table1StartIndex = modified.Table1StartIndex;
            Header.Table2StartIndex = modified.Table2StartIndex;

            RecordsRead = false;
            OffsetTable = new();
            LoadedOffsets = new();
            Records = new();
            Table2Records = new();
            Table3Records = new();
            EmptyRecords = new();
        }
    }

    private TableSchema GenerateGenericSchema()
    {
        var attributes = new List<SchemaAttribute>(Header.NumMembers);
        for (int i = 0; i < Header.NumMembers; i++)
        {
            attributes.Add(new SchemaAttribute { Name = $"Field_{i}", Type = "int", MinValue = 1, MaxValue = 1 });
        }
        return new TableSchema { Name = "", NumMembers = Header.NumMembers, Attributes = attributes };
    }

    public string GetBinaryReferenceToRecord(int index) => BinaryUtil.GetBinaryReferenceData(Header.TableId, index);

    private void Emit() => Changed?.Invoke(this);

    public void UpdateBuffer()
    {
        int table2EndIndex = Header.HasThirdTable ? Header.Table3StartIndex : Data.Length;
        var table2Data = Strategy.GetTable2BinaryData(
            Table2Records.Cast<IPatchUnit>().ToList(),
            Data.CopyRange(Header.Table2StartIndex, table2EndIndex),
            Header.HasCompactTable2);

        List<byte[]> table3Data = new();
        if (Header.HasThirdTable)
        {
            table3Data = Strategy.GetTable3BinaryData(Table3Records.Cast<IPatchUnit>().ToList(), Data.CopyRange(Header.Table3StartIndex));
        }

        if (RecordsRead)
        {
            int table2DataLength = table2Data.Sum(a => a.Length);
            int table3DataLength = table3Data.Sum(a => a.Length);
            Header.Table2Length = table2DataLength;
            Header.Table3Length = table3DataLength;

            int maxTable2LengthPerRecord = OffsetTable.Where(o => o.Type == "string").Sum(o => o.MaxLength ?? 0);
            int maxTable2Length = maxTable2LengthPerRecord * Records.Count;
            Header.TableTotalLength = Header.Table1Length + maxTable2Length + Header.Table3Length;

            WriteUInt32BE(Header.Table2Length, Header.OffsetStart - 44);
            WriteUInt32BE(Header.Table3Length, Header.OffsetStart - 40);
            WriteUInt32BE(Header.TableTotalLength, Header.OffsetStart - 24);

            if (Header.HasCompactTable2)
            {
                Header.Table2Capacity = Header.Table2Length;
                Header.TableUnknown1 = Header.Table2Capacity;
                WriteUInt32BE(Header.Table2Length, 0x88);
            }
        }

        var changedRecords = Records.Where(r => r.IsChanged).ToList();
        int currentOffset = 0;
        var bufferArrays = new List<byte[]>();

        if (IsArray && changedRecords.Count > 0)
        {
            bufferArrays.Add(Data.CopyRange(currentOffset, Header.HeaderSize));
            var arraySizeBuffer = new byte[Header.Data1RecordCount * 4];
            for (int i = 0; i < ArraySizes.Count; i++)
            {
                BinaryPrimitives.WriteUInt32BigEndian(arraySizeBuffer.AsSpan(i * 4, 4), unchecked((uint)ArraySizes[i]));
            }
            bufferArrays.Add(arraySizeBuffer);
            currentOffset += Header.HeaderSize + Header.Data1RecordCount * 4;
        }

        foreach (var record in changedRecords)
        {
            record.IsChanged = false;
            int recordOffset = Header.Table1StartIndex + record.Index * Header.Record1Size;
            bufferArrays.Add(Data.CopyRange(currentOffset, recordOffset));
            var recordHexData = record.HexData;
            bufferArrays.Add(recordHexData);
            currentOffset = recordOffset + recordHexData.Length;
        }

        bufferArrays.Add(Data.CopyRange(currentOffset, Header.Table2StartIndex));

        if (!RecordsRead && Header.HasSecondTable && table2Data.Count == 0)
        {
            table2Data = new List<byte[]> { Data.CopyRange(Header.Table2StartIndex, table2EndIndex) };
        }
        bufferArrays.AddRange(table2Data);

        if (!RecordsRead && Header.HasThirdTable && table3Data.Count == 0)
        {
            table3Data = new List<byte[]> { Data.CopyRange(Header.Table3StartIndex) };
        }
        bufferArrays.AddRange(table3Data);

        Data = CommonAlgorithms.Concat(bufferArrays);
    }

    private void WriteUInt32BE(long value, int offset) =>
        BinaryPrimitives.WriteUInt32BigEndian(Data.AsSpan(offset, 4), unchecked((uint)value));

    public void SetNextRecordToUse(int index, bool resetEmptyRecordMap = false)
    {
        SetNextRecordToUseBuffer(index);
        if (resetEmptyRecordMap && RecordsRead)
        {
            UpdateBuffer();
            EmptyRecords = ParseEmptyRecords();
        }
        Emit();
    }

    private void SetNextRecordToUseBuffer(int index)
    {
        Header.NextRecordToUse = index;
        WriteUInt32BE(index, Header.OffsetStart - 4);
    }

    public void RecalculateEmptyRecordReferences()
    {
        var emptyRecordReferenceIndicies = new List<int>();
        foreach (var record in Records)
        {
            bool isEmptyReference = false;
            var firstFour = record.TableDataArray.AsSpan(record.ByteOffset, 4).ToArray();
            var firstFourBytesReference = BinaryUtil.GetReferenceDataFromBuffer(firstFour);
            if (firstFourBytesReference.TableId == 0 && firstFourBytesReference.RowNumber != 0)
            {
                var firstOffset = OffsetTable[0];
                if (firstOffset.Type != "string")
                {
                    isEmptyReference = true;
                    emptyRecordReferenceIndicies.Add(firstFourBytesReference.RowNumber);
                }
            }
            record.IsEmpty = isEmptyReference;
        }

        var unreachableRecords = Records
            .Where(r => r.IsEmpty)
            .Where(r => !emptyRecordReferenceIndicies.Contains(r.Index))
            .ToList();

        if (unreachableRecords.Count > 1)
        {
            // Matches the JS library's behavior: warn and leave nextRecordToUse untouched.
            Console.Error.WriteLine(
                $"({Header.TableId}) {Name} - More than one unreachable records found: " +
                $"({string.Join(", ", unreachableRecords.Select(r => r.Index))}). " +
                "The game will most likely crash if you do not fix this problem. The nextRecordToUse has NOT been updated.");
        }
        else
        {
            int nextRecordToUse = Header.RecordCapacity;
            if (unreachableRecords.Count == 1)
            {
                nextRecordToUse = unreachableRecords[0].Index;
            }
            SetNextRecordToUseBuffer(nextRecordToUse);
            EmptyRecords = ParseEmptyRecords();
            Emit();
        }
    }

    public FranchiseFileTable ReplaceRawData(byte[] buf, bool shouldReadRecords)
    {
        Data = buf;
        RecordsRead = false;
        Header = Strategy.ParseHeader(Data);
        LoadedOffsets = new();
        IsChanged = false;
        Records = new();
        Table2Records = new();
        Table3Records = new();
        ArraySizes = new();
        EmptyRecords = new();
        Emit();

        if (shouldReadRecords)
        {
            ReadRecords();
        }
        return this;
    }

    public FranchiseFileTable ReadRecords(List<string>? attribsToLoad = null)
    {
        if (RecordsRead && !IsLoadingNewOffsets(LoadedOffsets, attribsToLoad, OffsetTable))
        {
            return this;
        }

        if (IsArray)
        {
            int numberOfFields = Header.Record1Size / 4;
            var offsetTable = new List<OffsetTableEntry>(numberOfFields);
            string baseType = Name.Length >= 2 ? Name.Substring(0, Name.Length - 2) : Name;
            for (int i = 0; i < numberOfFields; i++)
            {
                bool isReference = baseType.Length > 0 &&
                    (char.IsUpper(baseType[0]) || baseType.Contains("[]"));
                offsetTable.Add(new OffsetTableEntry
                {
                    Index = i,
                    Name = $"{baseType}{i}",
                    Type = baseType,
                    IndexOffset = i * 32,
                    Offset = i * 32,
                    Length = 32,
                    IsSigned = false,
                    IsReference = isReference,
                    ValueInSecondTable = false,
                    ValueInThirdTable = false,
                    Final = false
                });
            }
            var arraySizes = new List<int>(Header.Data1RecordCount);
            for (int i = 0; i < Header.Data1RecordCount; i++)
            {
                arraySizes.Add(TableBinaryReader.ReadUInt32BE(Data, Header.HeaderSize + i * 4));
            }
            OffsetTable = offsetTable;
            ArraySizes = arraySizes;
        }
        else
        {
            if (Schema is null)
            {
                Schema = GenerateGenericSchema();
            }
            OffsetTable = ReadOffsetTableFromSchema(Data, Schema!, Header);
        }

        var offsetTableToUse = OffsetTable;
        var mandatoryOffsetsToLoad = Strategy.GetMandatoryOffsets(OffsetTable);
        if (attribsToLoad is not null)
        {
            var loadedNames = LoadedOffsets.Select(o => o.Name).ToHashSet();
            offsetTableToUse = OffsetTable.Where(a =>
                mandatoryOffsetsToLoad.Contains(a.Name) ||
                attribsToLoad.Contains(a.Name) ||
                loadedNames.Contains(a.Name)).ToList();
        }
        LoadedOffsets = offsetTableToUse;

        Records = ReadRecordsFromData(Data, Header, offsetTableToUse, this);
        EmptyRecords = ParseEmptyRecords();
        for (int index = 0; index < Records.Count; index++)
        {
            var record = Records[index];
            if (IsArray)
            {
                record.ArraySize = ArraySizes[index];
            }
            if (EmptyRecords.Get(index) is not null)
            {
                record.IsEmpty = true;
            }
        }

        if (Header.HasSecondTable)
        {
            ParseTable2Values(Records);
        }
        if (Header.HasThirdTable)
        {
            ParseTable3Values(Records);
        }

        RecordsRead = true;
        return this;
    }

    private EmptyRecordMap ParseEmptyRecords()
    {
        var emptyRecords = new EmptyRecordMap();
        int firstEmptyRecord = Header.NextRecordToUse;
        int? previousEmptyRecordIndex = null;
        int currentEmptyRecordIndex = firstEmptyRecord;
        if (firstEmptyRecord != Header.RecordCapacity)
        {
            while (currentEmptyRecordIndex != Header.RecordCapacity)
            {
                var record = Records[currentEmptyRecordIndex];
                int nextEmptyRecordIndex = unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(
                    record.TableDataArray.AsSpan(record.ByteOffset, 4)));
                emptyRecords.Set(currentEmptyRecordIndex, new EmptyRecordEntry(previousEmptyRecordIndex, nextEmptyRecordIndex));
                previousEmptyRecordIndex = currentEmptyRecordIndex;
                currentEmptyRecordIndex = nextEmptyRecordIndex;
            }
        }
        return emptyRecords;
    }

    private void OnRecordEmpty(FranchiseFileRecord record)
    {
        if (record.IsEmpty)
        {
            return;
        }
        record.IsChanged = true;
        var last = EmptyRecords.Last();
        if (last.HasValue)
        {
            int lastEmptyRecordIndex = last.Value.Key;
            EmptyRecords.Set(lastEmptyRecordIndex, new EmptyRecordEntry(last.Value.Value.Previous, record.Index));
            EmptyRecords.Set(record.Index, new EmptyRecordEntry(lastEmptyRecordIndex, Header.RecordCapacity));
            ChangeRecordBuffers(lastEmptyRecordIndex, record.Index);
            ChangeRecordBuffers(record.Index, Header.RecordCapacity);
            Records[lastEmptyRecordIndex].ClearCachedFieldValues();
        }
        else
        {
            EmptyRecords.Set(record.Index, new EmptyRecordEntry(null, Header.RecordCapacity));
            SetNextRecordToUse(record.Index);
            ChangeRecordBuffers(record.Index, Header.RecordCapacity);
        }
    }

    private void ParseTable2Values(List<FranchiseFileRecord> records)
    {
        var secondTableData = Data.CopyRange(Header.Table2StartIndex);
        foreach (var record in records)
        {
            foreach (var field in record.FieldsArray.Where(f => f.SecondTableField is not null))
            {
                field.SecondTableField!.UnformattedValue = Strategy.Table2FieldStrategy.GetInitialUnformattedValue(field, secondTableData);
                field.SecondTableField.Strategy = Strategy.Table2FieldStrategy;
                Table2Records.Add(field.SecondTableField);
                field.SecondTableField.Parent = this;
            }
        }
    }

    private void ParseTable3Values(List<FranchiseFileRecord> records)
    {
        var thirdTableData = Data.CopyRange(Header.Table3StartIndex);
        foreach (var record in records)
        {
            var overflowRecord = BinaryUtil.GetTable3OverflowRecord(record);
            foreach (var field in record.FieldsArray.Where(f => f.ThirdTableField is not null))
            {
                var context = new Table3StrategyContext { GameYear = GameYear, GameType = GameType, TableName = Name };
                var overflowField = overflowRecord?.GetFieldByKey(field.Key);
                field.ThirdTableField!.UnformattedValue = Strategy.Table3FieldStrategy.GetInitialUnformattedValue(
                    field, thirdTableData, overflowField, context);
                field.ThirdTableField.Strategy = Strategy.Table3FieldStrategy;
                field.ThirdTableField.StrategyContext = context;
                Table3Records.Add(field.ThirdTableField);
                field.ThirdTableField.Parent = this;
            }
        }
    }

    private void ChangeRecordBuffers(int index, int emptyRecordReference)
    {
        int recordStartIndex = Header.Table1StartIndex + index * Header.Record1Size;
        WriteUInt32BE(emptyRecordReference, recordStartIndex);
        // Second write is a no-op in our shared-buffer model (Records[index]'s bytes ARE Data at
        // the same offset) - kept for parity with the JS library's (also redundant) second write.
        BinaryPrimitives.WriteUInt32BigEndian(
            Records[index].TableDataArray.AsSpan(Records[index].ByteOffset, 4), unchecked((uint)emptyRecordReference));
    }

    public void OnEvent(string name, FranchiseFileRecord record)
    {
        if (name == "change")
        {
            record.IsChanged = true;
            if (IsArray)
            {
                while (ArraySizes.Count <= record.Index) ArraySizes.Add(0);
                ArraySizes[record.Index] = record.ArraySize ?? 0;
            }
            if (Header.Record1Size >= 4)
            {
                var emptyRecordReference = EmptyRecords.Get(record.Index);
                if (emptyRecordReference is not null)
                {
                    var changedFieldsInFirst4Bytes = record.FieldsArray
                        .Where(f => f.IsChanged && f.Offset.IndexOffset < 32).ToList();

                    if (_settings.AutoUnempty && changedFieldsInFirst4Bytes.Count == 0)
                    {
                        ChangeRecordBuffers(record.Index, 0);
                        foreach (var field in record.FieldsArray.Where(f => f.Offset.IndexOffset < 32))
                        {
                            field.ClearCachedValues();
                        }
                    }

                    if (_settings.AutoUnempty || changedFieldsInFirst4Bytes.Count > 0)
                    {
                        Strategy.RecalculateStringOffsets(this, record);
                        Strategy.RecalculateBlobOffsets(this, record);
                        EmptyRecords.Delete(record.Index);
                        record.IsEmpty = false;

                        var reference = emptyRecordReference.Value;
                        var previousEmptyReference = reference.Previous.HasValue ? EmptyRecords.Get(reference.Previous.Value) : null;
                        if (previousEmptyReference is not null)
                        {
                            EmptyRecords.Set(reference.Previous!.Value, new EmptyRecordEntry(previousEmptyReference.Value.Previous, reference.Next));
                            ChangeRecordBuffers(reference.Previous.Value, reference.Next);
                            Records[reference.Previous.Value].ClearCachedFieldValues();
                        }

                        var nextEmptyReference = EmptyRecords.Get(reference.Next);
                        if (nextEmptyReference is not null)
                        {
                            EmptyRecords.Set(reference.Next, new EmptyRecordEntry(reference.Previous, nextEmptyReference.Value.Next));
                            if (previousEmptyReference is null)
                            {
                                SetNextRecordToUse(reference.Next);
                            }
                        }

                        if (previousEmptyReference is null && nextEmptyReference is null)
                        {
                            SetNextRecordToUse(Header.RecordCapacity);
                        }
                    }
                }
            }
            Emit();
        }
        else if (name == "empty")
        {
            OnRecordEmpty(record);
            Emit();
        }
    }

    public void OnEvent(string name, FranchiseFileTable2Field field)
    {
        field.IsChanged = true;
        if (field.FieldReference is not null)
        {
            OnEvent("change", field.FieldReference.Parent);
        }
        else
        {
            Emit();
        }
    }

    public void OnEvent(string name, FranchiseFileTable3Field field)
    {
        field.IsChanged = true;
        if (field.FieldReference is not null)
        {
            OnEvent("change", field.FieldReference.Parent);
        }
        else
        {
            Emit();
        }
    }

    private static bool IsSkippedOffset(OffsetTableEntry offset) =>
        offset.Final || offset.Const || offset.Type.Contains("()") || offset.Type == "ITransaction_Sleep";

    private static List<OffsetTableEntry> ReadOffsetTableFromSchema(byte[] data, TableSchema schema, TableHeader header)
    {
        int currentIndex = header.OffsetStart;
        var seenIndexOffsets = new HashSet<long>();
        var offsetTable = new List<OffsetTableEntry>(schema.Attributes.Count);

        for (int index = 0; index < schema.Attributes.Count; index++)
        {
            var attribute = schema.Attributes[index];
            long indexOffset = TableBinaryReader.ReadUInt32BE(data, currentIndex);
            bool isRepeatedIndexOffset = seenIndexOffsets.Contains(indexOffset);
            bool isNegative = attribute.IsNegative;
            string type = isNegative ? "s_" + attribute.Type : attribute.Type;
            bool isReference = attribute.Enum is null &&
                (attribute.Type.Length > 0 && char.IsUpper(attribute.Type[0]) || attribute.Type.Contains("[]") || attribute.Type == "record");

            var entry = new OffsetTableEntry
            {
                Index = index,
                OriginalIndex = attribute.Index ?? 0,
                Name = attribute.Name,
                Type = type,
                IsReference = isReference,
                ValueInSecondTable = header.HasSecondTable && attribute.Type == "string",
                ValueInThirdTable = header.HasThirdTable && attribute.Type == "binaryblob",
                IsSigned = isNegative,
                MinValue = attribute.MinValue,
                MaxValue = attribute.MaxValue,
                MaxLength = attribute.MaxLength,
                Final = attribute.Final || isRepeatedIndexOffset,
                IndexOffset = (int)indexOffset,
                Enum = attribute.Enum,
                Const = attribute.Const
            };
            offsetTable.Add(entry);

            if (!IsSkippedOffset(entry))
            {
                seenIndexOffsets.Add(indexOffset);
            }
            currentIndex += 4;
        }

        offsetTable.Sort((a, b) => a.IndexOffset.CompareTo(b.IndexOffset));

        for (int i = 0; i < offsetTable.Count; i++)
        {
            var curOffset = offsetTable[i];
            OffsetTableEntry? nextOffset = offsetTable.Count > i + 1 ? offsetTable[i + 1] : null;
            if (nextOffset is not null)
            {
                int curIndex = i + 2;
                while (nextOffset is not null && IsSkippedOffset(nextOffset))
                {
                    nextOffset = curIndex < offsetTable.Count ? offsetTable[curIndex] : null;
                    curIndex += 1;
                }
                curOffset.Length = nextOffset is not null
                    ? nextOffset.IndexOffset - curOffset.IndexOffset
                    : header.Record1Size * 8 - curOffset.IndexOffset;
            }
            else
            {
                curOffset.Length = header.Record1Size * 8 - curOffset.IndexOffset;
            }
            if (curOffset.Length > 32)
            {
                curOffset.Length = 32;
            }
        }

        int currentOffsetIndex = 0;
        var chunked32Bit = new List<List<OffsetTableEntry>>();
        for (int i = 0; i < header.Record1Size * 8; i += 32)
        {
            var chunkedOffsets = new List<OffsetTableEntry>();
            int offsetLength = i % 32;
            while (true)
            {
                if (currentOffsetIndex >= offsetTable.Count)
                {
                    break;
                }
                var currentOffset = offsetTable[currentOffsetIndex];
                if (IsSkippedOffset(currentOffset))
                {
                    currentOffsetIndex += 1;
                    continue;
                }
                offsetLength += currentOffset.Length;
                chunkedOffsets.Add(currentOffset);
                currentOffsetIndex += 1;
                if (!(currentOffsetIndex < offsetTable.Count && offsetLength < 32))
                {
                    break;
                }
            }
            chunked32Bit.Add(chunkedOffsets);
        }

        foreach (var offsetArray in chunked32Bit)
        {
            if (offsetArray.Count > 0)
            {
                int currentOffsetValue = offsetArray[0].IndexOffset;
                offsetArray[offsetArray.Count - 1].Offset = currentOffsetValue;
                for (int i = offsetArray.Count - 2; i >= 0; i--)
                {
                    var previousOffset = offsetArray[i + 1];
                    var offset = offsetArray[i];
                    offset.Offset = previousOffset.Offset + previousOffset.Length;
                }
            }
        }

        var filtered = offsetTable.Where(o => !IsSkippedOffset(o)).ToList();
        filtered.Sort((a, b) => a.Offset.CompareTo(b.Offset));

        return filtered;
    }

    private static List<FranchiseFileRecord> ReadRecordsFromData(
        byte[] data, TableHeader header, List<OffsetTableEntry> offsetTable, FranchiseFileTable table)
    {
        var records = new List<FranchiseFileRecord>();
        int index = 0;
        for (int i = header.Table1StartIndex; i < header.Table2StartIndex; i += header.Record1Size)
        {
            records.Add(new FranchiseFileRecord(data, i, header.Record1Size, index, offsetTable, table));
            index += 1;
        }
        return records;
    }

    private static bool IsLoadingNewOffsets(List<OffsetTableEntry> currentlyLoaded, List<string>? attribsToLoad, List<OffsetTableEntry> offsetTable)
    {
        if (attribsToLoad is not null)
        {
            var names = currentlyLoaded.Select(o => o.Name).ToHashSet();
            return attribsToLoad.Any(a => !names.Contains(a));
        }
        return currentlyLoaded.Count != offsetTable.Count;
    }
}
