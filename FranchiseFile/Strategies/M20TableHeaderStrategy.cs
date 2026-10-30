using MaddenFranchise.Schema;

namespace MaddenFranchise.Strategies;

/// <summary>Port of strategies/common/header/m20/M20TableHeaderStrategy.js.</summary>
public static class M20TableHeaderStrategy
{
    public static TableHeader ParseHeader(byte[] data)
    {
        const int headerStart = 0x80;
        string tableName = TableBinaryReader.ReadTableName(data, 0);
        bool isArray = tableName.Contains("[]");
        int tableId = TableBinaryReader.ReadUInt32BE(data, headerStart);
        int tableUniqueId = TableBinaryReader.ReadUInt32BE(data, headerStart + 4);
        int table2Capacity = TableBinaryReader.ReadUInt32BE(data, headerStart + 8);
        int tableUnknown2 = TableBinaryReader.ReadUInt32BE(data, headerStart + 12);
        string data1Id = TableBinaryReader.ReadTableName(data, headerStart + 20, 4);
        int data1Type = TableBinaryReader.ReadUInt32BE(data, headerStart + 24);
        int data1Unknown1 = TableBinaryReader.ReadUInt32BE(data, headerStart + 28);
        int data1Flag1 = data[headerStart + 32];
        int data1Flag2 = data[headerStart + 33];
        int data1Flag3 = data[headerStart + 34];
        int data1Flag4 = data[headerStart + 35];
        int tableStoreLength = TableBinaryReader.ReadUInt32BE(data, headerStart + 36);

        int headerOffset = headerStart + 40;
        int records1SizeOffset = 1720;
        string? tableStoreName = null;
        if (tableStoreLength > 0)
        {
            headerOffset += tableStoreLength;
            records1SizeOffset += tableStoreLength * 8;
            tableStoreName = TableBinaryReader.ReadTableName(data, headerStart + 40, tableStoreLength);
        }

        int data1Offset = TableBinaryReader.ReadUInt32BE(data, headerOffset);
        int data1TableId = TableBinaryReader.ReadUInt32BE(data, headerOffset + 4);
        int data1RecordCount = TableBinaryReader.ReadUInt32BE(data, headerOffset + 8);
        int data1Pad2 = TableBinaryReader.ReadUInt32BE(data, headerOffset + 12);
        int table1Length = TableBinaryReader.ReadUInt32BE(data, headerOffset + 16);
        int table2Length = TableBinaryReader.ReadUInt32BE(data, headerOffset + 20);
        int data1Pad3 = TableBinaryReader.ReadUInt32BE(data, headerOffset + 24);
        int data1Pad4 = TableBinaryReader.ReadUInt32BE(data, headerOffset + 28);
        string data2Id = TableBinaryReader.ReadTableName(data, headerOffset + 32, 4);
        int table1Length2 = TableBinaryReader.ReadUInt32BE(data, headerOffset + 36);
        int tableTotalLength = TableBinaryReader.ReadUInt32BE(data, headerOffset + 40);
        int data2RecordWords = TableBinaryReader.ReadUInt32BE(data, headerOffset + 44);
        int data2RecordCapacity = TableBinaryReader.ReadUInt32BE(data, headerOffset + 48);
        int data2IndexEntries = TableBinaryReader.ReadUInt32BE(data, headerOffset + 52);
        int nextRecordToUse = TableBinaryReader.ReadUInt32BE(data, headerOffset + 60);

        int offsetStart = 0xe8 + tableStoreLength;
        bool hasSecondTable = tableTotalLength > table1Length;
        int headerSize = offsetStart;
        int records1Size = data2RecordWords * 4;
        int table1StartIndex, table2StartIndex;

        if (!isArray)
        {
            headerSize += data2IndexEntries * 4;
            table1StartIndex = headerSize;
            table2StartIndex = headerSize + data1RecordCount * records1Size;
        }
        else
        {
            table1StartIndex = headerSize + data1RecordCount * 4;
            table2StartIndex = table1StartIndex + data1RecordCount * records1Size;
        }

        return new TableHeader
        {
            Name = tableName,
            IsArray = isArray,
            TableId = tableId,
            TablePad1 = tableUniqueId,
            UniqueId = tableUniqueId,
            TableUnknown1 = table2Capacity,
            Table2Capacity = table2Capacity,
            TableUnknown2 = tableUnknown2,
            Data1Id = data1Id,
            Data1Type = data1Type,
            Data1Unknown1 = data1Unknown1,
            Data1Flag1 = data1Flag1,
            Data1Flag2 = data1Flag2,
            Data1Flag3 = data1Flag3,
            Data1Flag4 = data1Flag4,
            TableStoreLength = tableStoreLength,
            TableStoreName = tableStoreName,
            Data1Offset = data1Offset,
            Data1TableId = data1TableId,
            Data1RecordCount = data1RecordCount,
            Data1Pad2 = data1Pad2,
            Table1Length = table1Length,
            Table2Length = table2Length,
            Data1Pad3 = data1Pad3,
            Data1Pad4 = data1Pad4,
            HeaderSize = headerSize,
            HeaderOffset = 0xe8,
            Record1SizeOffset = records1SizeOffset,
            Record1SizeLength = 10,
            Record1Size = records1Size,
            OffsetStart = offsetStart,
            Data2Id = data2Id,
            Table1Length2 = table1Length2,
            TableTotalLength = tableTotalLength,
            HasSecondTable = hasSecondTable,
            Table1StartIndex = table1StartIndex,
            Table2StartIndex = table2StartIndex,
            RecordWords = data2RecordWords,
            RecordCapacity = data2RecordCapacity,
            NumMembers = data2IndexEntries,
            NextRecordToUse = nextRecordToUse,
            HasThirdTable = false,
            HasCompactTable2 = table2Capacity > 0
        };
    }

    public static (int HeaderSize, int Record1Size, int Table1StartIndex, int Table2StartIndex) ParseHeaderAttributesFromSchema(
        TableSchema schema, byte[] data, TableHeader header)
    {
        return (header.HeaderSize, header.Record1Size, header.Table1StartIndex, header.Table2StartIndex);
    }
}
