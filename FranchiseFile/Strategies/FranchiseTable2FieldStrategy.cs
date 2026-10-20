using System.Text;

namespace MaddenFranchise.Strategies;

/// <summary>Port of strategies/common/table2Field/FranchiseTable2FieldStrategy.js.</summary>
public sealed class FranchiseTable2FieldStrategy : ITable2FieldStrategy
{
    public static readonly FranchiseTable2FieldStrategy Instance = new();

    public byte[]? GetInitialUnformattedValue(FranchiseFileField field, byte[] table2Data)
    {
        if (field.Parent?.IsEmpty == true)
        {
            return null;
        }

        var secondTableField = field.SecondTableField!;
        int start = secondTableField.Index;
        int length = field.Offset.MaxLength ?? 0;
        if (start < 0 || start + length > table2Data.Length)
        {
            // Defensive clamp: some tables report offsets beyond the current buffer for unused rows.
            length = Math.Max(0, Math.Min(length, table2Data.Length - start));
        }
        return table2Data.AsSpan(start, length).ToArray();
    }

    public byte[] SetUnformattedValueFromFormatted(string formattedValue, int maxLength)
    {
        var buffer = new byte[maxLength];
        var bytes = Encoding.UTF8.GetBytes(formattedValue);
        int toCopy = Math.Min(bytes.Length, maxLength);
        Array.Copy(bytes, buffer, toCopy);
        return buffer;
    }
}
