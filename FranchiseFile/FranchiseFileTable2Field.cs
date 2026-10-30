using System.Text;
using MaddenFranchise.Strategies;

namespace MaddenFranchise;

/// <summary>Port of FranchiseFileTable2Field.js - a string entry in a table's "second table" region.</summary>
public sealed class FranchiseFileTable2Field : IPatchUnit
{
    private string? _value;
    private byte[]? _unformattedValue;
    private int _offset;

    public int RawIndex { get; }
    public int MaxLength { get; }
    public FranchiseFileField? FieldReference { get; set; }
    public int? LengthAtLastSaveNullable { get; set; }
    public ITable2FieldStrategy? Strategy { get; set; }
    public FranchiseFileTable? Parent { get; set; }

    public FranchiseFileTable2Field(int index, int maxLength, FranchiseFileTable? parent = null)
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

    public string? Value
    {
        get
        {
            if (_unformattedValue is null)
            {
                return null;
            }
            if (_value is null)
            {
                string decoded = Encoding.UTF8.GetString(_unformattedValue);
                int nullIndex = decoded.IndexOf('\0');
                _value = nullIndex >= 0 ? decoded.Substring(0, nullIndex) : decoded;
            }
            return _value;
        }
        set
        {
            _value = value;
            string toWrite = value ?? "";
            if (toWrite.Length > MaxLength)
            {
                toWrite = toWrite.Substring(0, MaxLength);
            }
            _unformattedValue = Strategy!.SetUnformattedValueFromFormatted(toWrite, MaxLength);
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
            bool changed = _offset != value;
            _offset = value;
            Index = value;
            if (changed && FieldReference is not null)
            {
                FieldReference.UnformattedValue.SetBits(FieldReference.Offset.Offset, value, 32);
                FieldReference.IsChanged = true;
                FieldReference.BubbleChangeToParent();
            }
        }
    }

    public bool IsChanged { get; set; }
    public int LengthAtLastSave
    {
        get => LengthAtLastSaveNullable ?? 0;
        set => LengthAtLastSaveNullable = value;
    }
}
