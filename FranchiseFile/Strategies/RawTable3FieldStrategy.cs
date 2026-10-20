namespace MaddenFranchise.Strategies;

/// <summary>
/// Structural port of strategies/common/table3Field/FranchiseZstdTable3FieldStrategy.js, minus the
/// zstd decompression + ISON&lt;-&gt;JSON visual-data translation (see README - out of scope for this
/// port). Values are exposed as raw bytes (the still-compressed payload) rather than a decoded
/// JSON object; the overflow-record mechanics and on-disk layout (2-byte little-endian length
/// prefix + payload + zero padding to maxLength) are preserved exactly so read/write round-trips
/// byte-for-byte for these fields as long as callers don't try to reinterpret the bytes.
/// </summary>
public sealed class RawTable3FieldStrategy : ITable3FieldStrategy
{
    public static readonly RawTable3FieldStrategy Instance = new();

    public byte[]? GetInitialUnformattedValue(
        FranchiseFileField field, byte[] table3Data, FranchiseFileField? overflowField, Table3StrategyContext context)
    {
        if (field.Parent?.IsEmpty == true)
        {
            return null;
        }

        var thirdTableField = field.ThirdTableField!;
        int mainLength = (field.Offset.MaxLength ?? 0) + 2;
        var initialData = SafeSlice(table3Data, thirdTableField.Index, mainLength);

        var overflowThirdTableField = overflowField?.ThirdTableField;
        if (overflowThirdTableField is not null)
        {
            int overflowLength = (overflowField!.Offset.MaxLength ?? 0) + 2;
            var overflowData = SafeSlice(table3Data, overflowThirdTableField.Index, overflowLength);
            return CommonAlgorithms.Concat(new[] { initialData, overflowData });
        }

        return initialData;
    }

    public byte[]? GetFormattedValueFromUnformatted(byte[]? unformattedValue, Table3StrategyContext context)
    {
        if (unformattedValue is null || unformattedValue.Length < 2)
        {
            return null;
        }
        int length = unformattedValue[0] | (unformattedValue[1] << 8);
        length = Net48Compat.Clamp(length, 0, unformattedValue.Length - 2);
        return unformattedValue.AsSpan(2, length).ToArray();
    }

    public byte[] SetUnformattedValueFromFormatted(
        byte[] formattedValue, byte[]? oldUnformattedValue, int maxLength, Table3StrategyContext context)
    {
        var sizeBuf = new byte[2];
        int len = formattedValue.Length;
        sizeBuf[0] = unchecked((byte)(len & 0xFF));
        sizeBuf[1] = unchecked((byte)((len >> 8) & 0xFF));

        if (len <= maxLength)
        {
            var padding = new byte[maxLength - len];
            return CommonAlgorithms.Concat(new[] { sizeBuf, formattedValue, padding });
        }

        // Doesn't fit in the main record - return size+data as-is; FranchiseFileTable3Field
        // detects the oversized result and routes the excess to an overflow record.
        return CommonAlgorithms.Concat(new[] { sizeBuf, formattedValue });
    }

    private static byte[] SafeSlice(byte[] data, int start, int length)
    {
        if (start < 0 || start >= data.Length)
        {
            return Array.Empty<byte>();
        }
        int end = Math.Min(start + length, data.Length);
        return data.CopyRange(start, end);
    }
}
