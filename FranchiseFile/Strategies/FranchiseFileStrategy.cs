using System.Buffers.Binary;

namespace MaddenFranchise.Strategies;

/// <summary>
/// File-level strategy for the "franchise" format. GenerateUnpackedContents is the JS library's
/// CommonAlgorithms.save; PostPackFile deliberately does NOT follow the JS library.
/// </summary>
public static class FranchiseFileStrategy
{
    private const int CompressedDataOffset = 0x52;
    private const int CompressedSizeOffset = 0x4A;

    public static byte[] GenerateUnpackedContents(List<IPatchUnit> tables, byte[] oldData)
    {
        return CommonAlgorithms.Save(tables, oldData);
    }

    /// <summary>
    /// Places a recompressed database back into the FBCHUNKS container. The zlib stream lives in a
    /// fixed slot at 0x52 (its length is a u32le at 0x4A), followed by a few bytes of zero slack and
    /// then more chunks (CharacterVisuals etc.) at fixed absolute offsets with no pointers to them.
    /// So the new stream must fit in old-stream + slack, the rest of the slot is zeroed, and every
    /// byte after the slot is kept where it was. The JS library splices the stream in unchecked,
    /// which overwrites the next chunk whenever the stream grows.
    /// </summary>
    /// <exception cref="SaveSlotOverflowException">The new stream doesn't fit the slot.</exception>
    public static byte[] PostPackFile(byte[] originalData, byte[] newData)
    {
        var slot = MeasureSlot(originalData);
        if (newData.Length > slot.Budget)
        {
            throw new SaveSlotOverflowException(newData.Length, slot.Budget);
        }

        var output = new byte[originalData.Length];
        Buffer.BlockCopy(originalData, 0, output, 0, CompressedDataOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(CompressedSizeOffset, 4), (uint)newData.Length);
        Buffer.BlockCopy(newData, 0, output, CompressedDataOffset, newData.Length);
        // Bytes between the new stream's end and the slot end stay zero (new byte[] is zeroed).
        int slotEnd = CompressedDataOffset + slot.Budget;
        Buffer.BlockCopy(originalData, slotEnd, output, slotEnd, originalData.Length - slotEnd);
        return output;
    }

    /// <summary>Size of the compressed-database slot in an FBCHUNKS container.</summary>
    public static (int StreamSize, int Slack, int Budget) MeasureSlot(byte[] containerData)
    {
        int streamSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(containerData.AsSpan(CompressedSizeOffset, 4));
        int streamEnd = CompressedDataOffset + streamSize;
        if (streamEnd > containerData.Length)
        {
            throw new InvalidDataException(
                $"The save header says the database is {streamSize:N0} bytes, which runs past the end of the file.");
        }

        int slack = 0;
        while (streamEnd + slack < containerData.Length && containerData[streamEnd + slack] == 0)
        {
            slack++;
        }
        return (streamSize, slack, streamSize + slack);
    }
}

/// <summary>Thrown when an edited database compresses larger than the slot the save reserves for it.</summary>
public sealed class SaveSlotOverflowException : InvalidOperationException
{
    public SaveSlotOverflowException(int compressedSize, int budget)
        : base($"The edited database compresses to {compressedSize:N0} bytes, but this save only has room for " +
               $"{budget:N0} bytes before the next chunk. Nothing was written; saving would corrupt the file.")
    {
        CompressedSize = compressedSize;
        Budget = budget;
    }

    public int CompressedSize { get; }
    public int Budget { get; }
}
