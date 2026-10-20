namespace MaddenFranchise.Strategies;

/// <summary>
/// Port of strategies/common/CommonAlgorithms.js - the generic "diff changed units into the old
/// buffer, preserving everything else" write algorithm shared by table-level and file-level
/// packing.
/// </summary>
public static class CommonAlgorithms
{
    public static byte[] Save(List<IPatchUnit> units, byte[] oldData, Action<List<IPatchUnit>>? customUnitSort = null)
    {
        bool anyChanged = units.Any(u => u.IsChanged);
        if (!anyChanged)
        {
            return oldData;
        }

        int offsetDifference = 0;
        int oldOffsetCounter = 0;
        var bufferArrays = new List<byte[]>();

        if (customUnitSort is not null)
        {
            customUnitSort(units);
        }
        else
        {
            units.Sort((a, b) => a.Index.CompareTo(b.Index));
        }

        for (int index = 0; index < units.Count; index++)
        {
            var unit = units[index];
            if (unit.Offset == 0 && index > 0)
            {
                continue;
            }

            unit.Offset += offsetDifference;
            if (unit.IsChanged)
            {
                var unchangedSinceLastChange = oldData[oldOffsetCounter..(unit.Offset - offsetDifference)];
                var newHexData = unit.HexData;
                bufferArrays.Add(unchangedSinceLastChange);
                bufferArrays.Add(newHexData);

                oldOffsetCounter = unit.Offset - offsetDifference + unit.LengthAtLastSave;
                offsetDifference += newHexData.Length - unit.LengthAtLastSave;
                unit.LengthAtLastSave = newHexData.Length;
                unit.IsChanged = false;
            }
        }

        bufferArrays.Add(oldData[oldOffsetCounter..]);
        return Concat(bufferArrays);
    }

    public static byte[] Concat(IReadOnlyList<byte[]> arrays)
    {
        int total = 0;
        foreach (var a in arrays) total += a.Length;
        var result = new byte[total];
        int pos = 0;
        foreach (var a in arrays)
        {
            Buffer.BlockCopy(a, 0, result, pos, a.Length);
            pos += a.Length;
        }
        return result;
    }
}
