using MaddenFranchise.Schema;

namespace MaddenFranchise;

/// <summary>Port of the OffsetTableEntry shape built by FranchiseFileTable.js's readOffsetTable.</summary>
public sealed class OffsetTableEntry
{
    public required int Index { get; init; }
    public int OriginalIndex { get; init; }
    public required string Name { get; init; }
    public required string Type { get; set; }
    public bool IsReference { get; init; }
    public bool ValueInSecondTable { get; init; }
    public bool ValueInThirdTable { get; init; }
    public bool IsSigned { get; init; }
    public long? MinValue { get; init; }
    public long? MaxValue { get; init; }
    public int? MaxLength { get; init; }
    public bool Final { get; init; }
    public int IndexOffset { get; init; }
    public FranchiseEnum? Enum { get; init; }
    public bool Const { get; init; }

    /// <summary>Bit offset within the record. Assigned after the offset table is laid out.</summary>
    public int Offset { get; set; }

    /// <summary>Bit length of the field. Assigned after the offset table is laid out.</summary>
    public int Length { get; set; }
}
