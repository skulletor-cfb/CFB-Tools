using System.Buffers.Binary;
using System.Text;

namespace MaddenFranchise.Strategies;

internal static class TableBinaryReader
{
    public static int ReadUInt32BE(byte[] data, int offset) =>
        unchecked((int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, 4)));

    /// <summary>Port of the JS `readTableName` helper - a do/while null-terminated ASCII read.</summary>
    public static string ReadTableName(byte[] data, int offset, int? length = null)
    {
        int len = length ?? data.Length - offset;
        var sb = new StringBuilder();
        int i = 0;
        do
        {
            sb.Append((char)data[offset + i]);
            i++;
        } while (i < len && data[offset + i] != 0);
        return sb.ToString();
    }
}
