namespace MaddenFranchise.Strategies;

public interface ITable2FieldStrategy
{
    byte[]? GetInitialUnformattedValue(FranchiseFileField field, byte[] table2Data);
    byte[] SetUnformattedValueFromFormatted(string formattedValue, int maxLength);
}

public sealed class Table3StrategyContext
{
    public int GameYear { get; init; }
    public string? GameType { get; init; }
    public string? TableName { get; init; }
}

public interface ITable3FieldStrategy
{
    byte[]? GetInitialUnformattedValue(FranchiseFileField field, byte[] table3Data, FranchiseFileField? overflowField, Table3StrategyContext context);
    byte[]? GetFormattedValueFromUnformatted(byte[]? unformattedValue, Table3StrategyContext context);
    byte[] SetUnformattedValueFromFormatted(byte[] formattedValue, byte[]? oldUnformattedValue, int maxLength, Table3StrategyContext context);
}
