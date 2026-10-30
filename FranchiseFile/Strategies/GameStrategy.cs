using MaddenFranchise.Schema;

namespace MaddenFranchise.Strategies;

/// <summary>
/// Port of the M26Strategy composition from StrategyPicker.js, narrowed to just the one strategy
/// this port supports (CFB27 dynasty saves resolve to M26Strategy: file=M20FileStrategy,
/// table=M24TableStrategy built on M20TableHeaderStrategy, table2Field=FranchiseTable2FieldStrategy,
/// table3Field=zstd blob strategy - replaced here with the raw-byte passthrough strategy).
/// </summary>
public sealed class GameStrategy
{
    public static readonly GameStrategy M26 = new();

    public string Name => "M26Strategy";

    public TableHeader ParseHeader(byte[] data) => M24TableHeaderStrategy.ParseHeader(data);

    public (int HeaderSize, int Record1Size, int Table1StartIndex, int Table2StartIndex) ParseHeaderAttributesFromSchema(
        TableSchema schema, byte[] data, TableHeader header) =>
        M20TableHeaderStrategy.ParseHeaderAttributesFromSchema(schema, data, header);

    public List<byte[]> GetTable2BinaryData(List<IPatchUnit> table2Records, byte[] fullTable2Buffer, bool isCompact) =>
        FranchiseTableStrategy.GetTable2BinaryData(table2Records, fullTable2Buffer, isCompact);

    public List<byte[]> GetTable3BinaryData(List<IPatchUnit> table3Records, byte[] fullTable3Buffer) =>
        FranchiseTableStrategy.GetTable2BinaryData(table3Records, fullTable3Buffer, isCompact: false);

    public List<string> GetMandatoryOffsets(List<OffsetTableEntry> offsetTable) =>
        FranchiseTableStrategy.GetMandatoryOffsets(offsetTable);

    public void RecalculateStringOffsets(FranchiseFileTable table, FranchiseFileRecord record) =>
        FranchiseTableStrategy.RecalculateStringOffsets(table, record);

    public void RecalculateBlobOffsets(FranchiseFileTable table, FranchiseFileRecord record) =>
        FranchiseTableStrategy.RecalculateBlobOffsets(table, record);

    public ITable2FieldStrategy Table2FieldStrategy { get; } = FranchiseTable2FieldStrategy.Instance;

    public ITable3FieldStrategy Table3FieldStrategy { get; } = RawTable3FieldStrategy.Instance;

    public byte[] GenerateUnpackedContents(List<IPatchUnit> tables, byte[] oldData) =>
        FranchiseFileStrategy.GenerateUnpackedContents(tables, oldData);

    public byte[] PostPackFile(byte[] originalData, byte[] newData) =>
        FranchiseFileStrategy.PostPackFile(originalData, newData);
}

/// <summary>Port of StrategyPicker.js, narrowed to the CFB27 (format=franchise, gameType=college) path.</summary>
public static class StrategyPicker
{
    public static GameStrategy Pick(string format, int gameYear, string gameType)
    {
        if (format == MaddenFranchise.FileFormat.Franchise && gameType == MaddenFranchise.GameType.College &&
            (gameYear == 26 || gameYear == 27))
        {
            return GameStrategy.M26;
        }

        throw new NotSupportedException(
            $"Unsupported file type for this port (format={format}, gameYear={gameYear}, gameType={gameType}). " +
            "This library only supports CFB27 dynasty saves (the 'franchise' format, M26Strategy).");
    }
}
