namespace MaddenFranchise.Strategies;

/// <summary>
/// Port of strategies/common/file/FranchiseFileStrategy.js (composed from CommonFileStrategy.js,
/// which is just CommonAlgorithms.save) - used as the "file" strategy for M20FileStrategy/M26Strategy.
/// </summary>
public static class FranchiseFileStrategy
{
    private const int CompressedDataOffset = 0x52;

    public static byte[] GenerateUnpackedContents(List<IPatchUnit> tables, byte[] oldData)
    {
        return CommonAlgorithms.Save(tables, oldData);
    }

    public static byte[] PostPackFile(byte[] originalData, byte[] newData)
    {
        byte[] header = originalData[..CompressedDataOffset];
        string endOfData = newData.Length.ToString("x");

        header[0x4a] = ParseHexByte(JsSubstr(endOfData, 4, null));
        header[0x4b] = ParseHexByte(JsSubstr(endOfData, 2, 2));
        header[0x4c] = ParseHexByte(JsSubstr(endOfData, 0, 2));

        int trailerStart = newData.Length + CompressedDataOffset;
        byte[] trailer = trailerStart < originalData.Length
            ? originalData[trailerStart..]
            : Array.Empty<byte>();

        return CommonAlgorithms.Concat(new[] { header, newData, trailer });
    }

    private static string JsSubstr(string s, int start, int? length)
    {
        if (start >= s.Length)
        {
            return "";
        }
        if (start < 0)
        {
            start = Math.Max(0, s.Length + start);
        }
        int len = length ?? (s.Length - start);
        len = Math.Min(len, s.Length - start);
        return len <= 0 ? "" : s.Substring(start, len);
    }

    private static byte ParseHexByte(string hex)
    {
        if (hex.Length == 0)
        {
            return 0;
        }
        return unchecked((byte)(Convert.ToInt32(hex, 16) & 0xFF));
    }
}
