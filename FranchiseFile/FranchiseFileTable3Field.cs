using MaddenFranchise.Strategies;
using MaddenFranchise.Util;

namespace MaddenFranchise;

/// <summary>Port of FranchiseFileTable3Field.js - a raw-blob entry in a table's "third table" region.</summary>
public sealed class FranchiseFileTable3Field : IPatchUnit
{
    private static readonly string EmptyRef = BinaryUtil.GetBinaryReferenceData(0, 0);

    private byte[]? _value;
    private byte[]? _unformattedValue;
    private int _offset;

    public int RawIndex { get; }
    public int MaxLength { get; }
    public FranchiseFileField? FieldReference { get; set; }
    public int? LengthAtLastSaveNullable { get; set; }
    public ITable3FieldStrategy? Strategy { get; set; }
    public Table3StrategyContext StrategyContext { get; set; } = new();
    public FranchiseFileTable? Parent { get; set; }

    public FranchiseFileTable3Field(int index, int maxLength, FranchiseFileTable? parent = null)
    {
        RawIndex = index;
        MaxLength = maxLength;
        _offset = index;
        Index = index;
        Parent = parent;
    }

    public int Index { get; private set; }

    public byte[]? UnformattedValue
    {
        get => _unformattedValue;
        set
        {
            _unformattedValue = value;
            LengthAtLastSaveNullable ??= value?.Length ?? 0;
            _value = null;
            Parent?.OnEvent("change", this);
        }
    }

    public byte[]? Value
    {
        get
        {
            if (_value is null)
            {
                _value = Strategy!.GetFormattedValueFromUnformatted(_unformattedValue, StrategyContext);
            }
            return _value;
        }
        set
        {
            _value = value;
            byte[] newUnformattedValue = Strategy!.SetUnformattedValueFromFormatted(
                value ?? Array.Empty<byte>(), _unformattedValue, MaxLength, StrategyContext);

            if (newUnformattedValue.Length > MaxLength + 2)
            {
                newUnformattedValue = PopulateOverflowRecord(newUnformattedValue);
            }
            else
            {
                ClearOverflowRecordIfExists();
            }

            _unformattedValue = newUnformattedValue;
            LengthAtLastSaveNullable ??= _unformattedValue.Length;
            Parent?.OnEvent("change", this);
        }
    }

    public byte[] HexData => _unformattedValue ?? Array.Empty<byte>();

    public int Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            Index = value;
            if (FieldReference is not null)
            {
                FieldReference.UnformattedValue.SetBits(FieldReference.Offset.Offset, value, 32);
            }
        }
    }

    public bool IsChanged { get; set; }
    public int LengthAtLastSave
    {
        get => LengthAtLastSaveNullable ?? 0;
        set => LengthAtLastSaveNullable = value;
    }

    private byte[] PopulateOverflowRecord(byte[] newUnformattedValue)
    {
        var mainFieldData = newUnformattedValue[..(MaxLength + 2)];
        var overflowData = newUnformattedValue[(MaxLength + 2)..];

        var field = FieldReference!;
        var record = field.Parent;
        var table = record.Parent;

        var overflowRecord = BinaryUtil.GetTable3OverflowRecord(record);

        if (overflowRecord is not null)
        {
            overflowRecord.GetFieldByKey(field.Key)!.ThirdTableField!.UnformattedValue = overflowData;
        }
        else
        {
            int nextRecordIndex = table.Header.NextRecordToUse;
            if (nextRecordIndex != table.Header.RecordCapacity)
            {
                string reference = BinaryUtil.GetBinaryReferenceData(table.Header.TableId, nextRecordIndex);
                field.Parent.GetFieldByKey("Overflow")!.Value = reference;

                table.Records[nextRecordIndex].GetFieldByKey("Overflow")!.Value = EmptyRef;
                table.Records[nextRecordIndex].GetFieldByKey(field.Key)!.ThirdTableField!.UnformattedValue = overflowData;
            }
        }

        return mainFieldData;
    }

    private void ClearOverflowRecordIfExists()
    {
        var hasOverflowRef = FieldReference!.Parent.GetFieldByKey("Overflow");
        if (hasOverflowRef is null)
        {
            return;
        }

        var overflowRef = hasOverflowRef.Value as string;
        if (overflowRef == EmptyRef)
        {
            return;
        }

        var field = FieldReference;
        var record = field.Parent;

        var overflowRecord = BinaryUtil.GetTable3OverflowRecord(record);
        if (overflowRecord is not null)
        {
            overflowRecord.GetFieldByKey(field.Key)!.Value = Array.Empty<byte>();
            overflowRecord.Empty();
        }

        record.GetFieldByKey("Overflow")!.Value = EmptyRef;
    }
}
