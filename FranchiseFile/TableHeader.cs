namespace MaddenFranchise;

/// <summary>Mutable port of the FranchiseFileTableHeader shape produced by the header strategies.</summary>
public sealed class TableHeader
{
    public required string Name { get; set; }
    public bool IsArray { get; set; }
    public int TableId { get; set; }
    public int TablePad1 { get; set; }
    public int UniqueId { get; set; }
    public int TableUnknown1 { get; set; }
    public int Table2Capacity { get; set; }
    public int TableUnknown2 { get; set; }
    public string? Data1Id { get; set; }
    public int Data1Type { get; set; }
    public int Data1Unknown1 { get; set; }
    public int Data1Flag1 { get; set; }
    public int Data1Flag2 { get; set; }
    public int Data1Flag3 { get; set; }
    public int Data1Flag4 { get; set; }
    public int TableStoreLength { get; set; }
    public string? TableStoreName { get; set; }
    public int Data1Offset { get; set; }
    public int Data1TableId { get; set; }
    public int Data1RecordCount { get; set; }
    public int Data1Pad2 { get; set; }
    public int Table1Length { get; set; }
    public int Table2Length { get; set; }
    public int Table3Length { get; set; }
    public int Data1Pad3 { get; set; }
    public int Data1Pad4 { get; set; }
    public int HeaderSize { get; set; }
    public int HeaderOffset { get; set; }
    public int Record1SizeOffset { get; set; }
    public int Record1SizeLength { get; set; }
    public int Record1Size { get; set; }
    public int OffsetStart { get; set; }
    public string? Data2Id { get; set; }
    public int Table1Length2 { get; set; }
    public int TableTotalLength { get; set; }
    public bool HasSecondTable { get; set; }
    public int Table1StartIndex { get; set; }
    public int Table2StartIndex { get; set; }
    public int Table3StartIndex { get; set; }
    public int RecordWords { get; set; }
    public int RecordCapacity { get; set; }
    public int NumMembers { get; set; }
    public int NextRecordToUse { get; set; }
    public bool HasThirdTable { get; set; }
    public bool HasCompactTable2 { get; set; }
}
