using System.Text.RegularExpressions;

namespace MaddenFranchise.Util;

public readonly record struct RecordReference(int TableId, int RowNumber);

/// <summary>
/// Port of services/utilService.js — binary string / reference helpers used throughout the field
/// read/write logic. Kept literal (string-based) rather than "cleaned up" since callers depend on
/// the exact binary-string semantics (padding, sign handling) used by the original library.
/// </summary>
public static class BinaryUtil
{
    public static string Dec2Bin(long dec, int? len = null)
    {
        // Mirrors `(dec >>> 0).toString(2)` - treat as unsigned 32-bit.
        uint unsigned = unchecked((uint)dec);
        string bin = Convert.ToString(unsigned, 2);
        if (len.HasValue)
        {
            return bin.PadLeft(len.Value, '0');
        }
        return bin;
    }

    public static long Bin2Dec(string binary)
    {
        if (binary is null || !StringOnlyContainsBinaryDigits(binary))
        {
            throw new ArgumentException("Argument invalid - string must only contain binary digits.");
        }
        if (binary.Length == 0)
        {
            return 0;
        }
        return Convert.ToInt64(binary, 2);
    }

    public static bool StringOnlyContainsBinaryDigits(string str)
    {
        return !Regex.IsMatch(str, "[a-zA-Z2-9]");
    }

    public static bool IsBinaryDigitsOnly(string str) => StringOnlyContainsBinaryDigits(str);

    public static RecordReference GetReferenceData(string value)
    {
        int tableId = (int)Bin2Dec(value.Substring(0, 15));
        int rowNumber = (int)Bin2Dec(value.Substring(15));
        return new RecordReference(tableId, rowNumber);
    }

    public static RecordReference GetReferenceDataFromBuffer(byte[] buf)
    {
        var bv = new BitView(buf, 0, buf.Length);
        return GetReferenceDataFromBitView(bv, 0);
    }

    public static RecordReference GetReferenceDataFromBitView(BitView bv, int start = 0)
    {
        int tableId = (int)bv.GetBits(start, 15);
        int rowNumber = (int)bv.GetBits(start + 15, 17);
        return new RecordReference(tableId, rowNumber);
    }

    public static string GetBinaryReferenceData(int tableId, int rowNumber)
    {
        string referenceBinary = Dec2Bin(tableId, 15);
        string recordIndexBinary = Dec2Bin(rowNumber, 17);
        return referenceBinary + recordIndexBinary;
    }

    public static FranchiseFileRecord? GetTable3OverflowRecord(FranchiseFileRecord record)
    {
        var overflowField = record.GetFieldByKey("Overflow");
        if (overflowField is null)
        {
            return null;
        }

        var value = overflowField.Value as string;
        if (value is null)
        {
            return null;
        }

        var reference = GetReferenceData(value);
        if (reference.TableId != 0 && reference.TableId == record.Parent.Header.TableId)
        {
            return record.Parent.Records[reference.RowNumber];
        }

        return null;
    }
}
