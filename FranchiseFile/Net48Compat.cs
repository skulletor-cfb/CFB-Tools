using System.IO.Compression;

namespace MaddenFranchise;

/// <summary>Stand-ins for .NET Core APIs that don't exist on .NET Framework 4.8.</summary>
internal static class Net48Compat
{
    /// <summary>Copy of <paramref name="source"/>[start..end] (net48 has no array range indexer).</summary>
    public static byte[] CopyRange(this byte[] source, int start, int end)
    {
        var result = new byte[end - start];
        Buffer.BlockCopy(source, start, result, 0, result.Length);
        return result;
    }

    public static byte[] CopyRange(this byte[] source, int start) => source.CopyRange(start, source.Length);

    public static TValue? GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull =>
        dictionary.TryGetValue(key, out var value) ? value : default;

    public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

    public static float Int32BitsToSingle(int value) => BitConverter.ToSingle(BitConverter.GetBytes(value), 0);

    public static int SingleToInt32Bits(float value) => BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

    /// <summary>Decompresses an RFC 1950 zlib stream (net48 has DeflateStream but no ZLibStream).</summary>
    public static byte[] ZlibInflate(byte[] data)
    {
        // Skip the 2-byte zlib header; DeflateStream stops at the end of the deflate data, so the
        // trailing Adler-32 checksum is ignored.
        using var input = new MemoryStream(data, 2, data.Length - 2);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);
        return output.ToArray();
    }

    /// <summary>Produces an RFC 1950 zlib stream: 2-byte header + raw deflate + big-endian Adler-32.</summary>
    public static byte[] ZlibDeflate(byte[] data)
    {
        using var output = new MemoryStream();
        output.WriteByte(0x78);
        output.WriteByte(0x9C);
        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data, 0, data.Length);
        }

        uint adler = Adler32(data);
        output.WriteByte((byte)(adler >> 24));
        output.WriteByte((byte)(adler >> 16));
        output.WriteByte((byte)(adler >> 8));
        output.WriteByte((byte)adler);
        return output.ToArray();
    }

    /// <summary>
    /// zlib stream at maximum compression (SharpZipLib level 9). Several seconds for a full dynasty
    /// database, but well under the size of EA's own stream.
    /// </summary>
    public static byte[] ZlibDeflateMax(byte[] data)
    {
        using var output = new MemoryStream();
        using (var deflater = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(
                   output, new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(
                       ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION, noZlibHeaderOrFooter: false),
                   1 << 16) { IsStreamOwner = false })
        {
            deflater.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    private static uint Adler32(byte[] data)
    {
        const uint Mod = 65521;
        uint a = 1, b = 0;
        int index = 0;
        while (index < data.Length)
        {
            // 5552 is the largest block size for which the sums can't overflow before reducing.
            int blockEnd = Math.Min(index + 5552, data.Length);
            for (; index < blockEnd; index++)
            {
                a += data[index];
                b += a;
            }
            a %= Mod;
            b %= Mod;
        }
        return (b << 16) | a;
    }
}
