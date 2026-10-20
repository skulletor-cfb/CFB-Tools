namespace MaddenFranchise.Strategies;

/// <summary>
/// Port of strategies/common/table/FranchiseTableStrategy.js - the table2 (string) / table3 (blob)
/// binary regeneration logic used for writing, plus offset recalculation for un-emptied records.
/// </summary>
public static class FranchiseTableStrategy
{
    /// <summary>
    /// Also used, unmodified, as the table3 binary-data strategy (M24TableStrategy.getTable3BinaryData
    /// aliases this same function in the JS library, always called with isCompact=false).
    /// </summary>
    public static List<byte[]> GetTable2BinaryData(List<IPatchUnit> table2Records, byte[] fullTable2Buffer, bool isCompact = false)
    {
        if (isCompact)
        {
            return new List<byte[]> { CommonAlgorithms.Save(table2Records, fullTable2Buffer) };
        }

        var table2Data = new List<byte[]>();
        var changedTable2Records = table2Records.Where(r => r.IsChanged).OrderBy(r => r.Index).ToList();
        int currentOffset = 0;
        for (int i = 0; i < changedTable2Records.Count; i++)
        {
            var record = changedTable2Records[i];
            record.IsChanged = false;
            int recordOffset = record.Offset;
            if (i > 0 && recordOffset == 0)
            {
                continue;
            }
            if (recordOffset > currentOffset)
            {
                var preData = fullTable2Buffer[currentOffset..recordOffset];
                if (preData.Length > 0)
                {
                    table2Data.Add(preData);
                }
            }
            var recordHexData = record.HexData;
            table2Data.Add(recordHexData);
            currentOffset = recordOffset + recordHexData.Length;
        }

        if (table2Records.Count > 0)
        {
            table2Data.Add(currentOffset <= fullTable2Buffer.Length ? fullTable2Buffer[currentOffset..] : Array.Empty<byte>());
        }
        else
        {
            table2Data.Add(fullTable2Buffer);
        }
        return table2Data;
    }

    public static List<string> GetMandatoryOffsets(List<OffsetTableEntry> offsetTable) => new();

    public static void RecalculateStringOffsets(FranchiseFileTable table, FranchiseFileRecord record)
    {
        if (table.Header.HasCompactTable2)
        {
            int runningOffset = table.Header.Table2Length;
            foreach (var field in record.FieldsArray
                         .Where(f => f.Offset.Type == "string")
                         .OrderBy(f => f.Offset.Index))
            {
                if (field.SecondTableField is not null)
                {
                    field.SecondTableField.Offset = runningOffset;
                }
                runningOffset += ((field.Value as string)?.Length ?? 0) + 1;
            }
        }
        else
        {
            int byteLengthPerRecord = table.OffsetTable
                .Where(o => o.Type == "string")
                .Sum(o => o.MaxLength ?? 0);

            int runningOffset = 0;
            foreach (var field in record.FieldsArray
                         .Where(f => f.Offset.Type == "string")
                         .OrderBy(f => f.Offset.Index))
            {
                if (field.SecondTableField is not null)
                {
                    field.SecondTableField.Offset = record.Index * byteLengthPerRecord + runningOffset;
                }
                runningOffset += field.Offset.MaxLength ?? 0;
            }
        }
    }

    public static void RecalculateBlobOffsets(FranchiseFileTable table, FranchiseFileRecord record)
    {
        int byteLengthPerRecord = table.OffsetTable
            .Where(o => o.Type == "binaryblob")
            .Sum(o => (o.MaxLength ?? 0) + 2);

        int runningOffset = 0;
        foreach (var field in record.FieldsArray
                     .Where(f => f.Offset.Type == "binaryblob")
                     .OrderBy(f => f.Offset.Index))
        {
            if (field.ThirdTableField is not null)
            {
                field.ThirdTableField.Offset = record.Index * byteLengthPerRecord + runningOffset;
            }
            runningOffset += field.Offset.MaxLength ?? 0;
        }
    }
}
