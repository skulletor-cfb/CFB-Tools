using MaddenFranchise.Util;

namespace MaddenFranchise.Schema;

public sealed class FranchiseEnumValue
{
    public string Name { get; }
    public int Index { get; }
    public long Value { get; }
    public string UnformattedValue { get; private set; }

    public FranchiseEnumValue(string name, int index, long value, string? unformattedValue)
    {
        Name = name;
        Index = index;
        Value = value;
        UnformattedValue = unformattedValue ?? ParseFormattedValue(value);
    }

    public void SetMemberLength(int length)
    {
        if (Value < 0)
        {
            UnformattedValue = "1" + UnformattedValue.PadLeft(length - 1, '0');
        }
        else
        {
            UnformattedValue = UnformattedValue.PadLeft(length, '0');
        }
    }

    private static string ParseFormattedValue(long value)
    {
        if (value < 0)
        {
            return BinaryUtil.Dec2Bin(value * -1 - 1);
        }
        return BinaryUtil.Dec2Bin(value);
    }
}
