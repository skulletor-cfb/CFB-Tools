namespace MaddenFranchise;

/// <summary>
/// Port of the `bit-buffer` npm package's BitView (big-endian bit reads/writes over a byte buffer).
/// Only the big-endian path is implemented since that's the only mode the original library uses.
/// </summary>
public sealed class BitView
{
    private readonly byte[] _buffer;
    private readonly int _byteOffset;
    private readonly int _byteLength;

    public BitView(byte[] buffer, int byteOffset = 0, int? byteLength = null)
    {
        _buffer = buffer;
        _byteOffset = byteOffset;
        _byteLength = byteLength ?? (buffer.Length - byteOffset);
    }

    public int ByteLength => _byteLength;

    private int GetByte(int index) => _buffer[_byteOffset + index];

    private void SetByte(int index, byte value) => _buffer[_byteOffset + index] = value;

    public long GetBits(int offset, int bits, bool signed = false)
    {
        int available = _byteLength * 8 - offset;
        if (bits > available)
        {
            throw new InvalidOperationException(
                $"Cannot get {bits} bit(s) from offset {offset}, {available} available");
        }

        int value = 0;
        int i = 0;
        while (i < bits)
        {
            int remaining = bits - i;
            int bitOffset = offset & 7;
            int currentByte = GetByte(offset >> 3);

            int read = Math.Min(remaining, 8 - bitOffset);

            int mask = ~(0xFF << read);
            int readBits = (currentByte >> (8 - read - bitOffset)) & mask;

            value <<= read;
            value |= readBits;

            offset += read;
            i += read;
        }

        if (signed)
        {
            if (bits != 32 && (value & (1 << (bits - 1))) != 0)
            {
                value |= -1 ^ ((1 << bits) - 1);
            }
            return value;
        }

        return unchecked((uint)value);
    }

    public void SetBits(int offset, long value, int bits)
    {
        int available = _byteLength * 8 - offset;
        if (bits > available)
        {
            throw new InvalidOperationException(
                $"Cannot set {bits} bit(s) from offset {offset}, {available} available");
        }

        int v = unchecked((int)(uint)(value & 0xFFFFFFFFL));
        int i = 0;
        while (i < bits)
        {
            int remaining = bits - i;
            int bitOffset = offset & 7;
            int byteOffset = offset >> 3;
            int wrote = Math.Min(remaining, 8 - bitOffset);

            int mask = ~(~0 << wrote);
            int writeBits = (v >> (bits - i - wrote)) & mask;

            int destShift = 8 - bitOffset - wrote;
            int destMask = ~(mask << destShift);

            int current = GetByte(byteOffset);
            SetByte(byteOffset, (byte)((current & destMask) | (writeBits << destShift)));

            offset += wrote;
            i += wrote;
        }
    }

    public float GetFloat32(int offset)
    {
        uint bits = (uint)GetBits(offset, 32, signed: false);
        return Net48Compat.Int32BitsToSingle(unchecked((int)bits));
    }

    public void SetFloat32(int offset, float value)
    {
        uint bits = unchecked((uint)Net48Compat.SingleToInt32Bits(value));
        SetBits(offset, bits, 32);
    }
}
