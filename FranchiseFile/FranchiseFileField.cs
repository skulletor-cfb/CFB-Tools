using MaddenFranchise.Schema;
using MaddenFranchise.Util;

namespace MaddenFranchise;

/// <summary>
/// Port of FranchiseFileField.js - bit-level get/set for a single record field.
///
/// The JS version lazily wraps the record's own Buffer *slice* (itself a zero-copy view into the
/// table's underlying memory) in a BitView per field. We replicate the "writes go straight into
/// the table's shared byte array" behavior by having the BitView reference the table's backing
/// array directly at (record.ByteOffset, record.ByteLength) rather than giving each record its own
/// copied array - see FranchiseFileRecord for the shared-buffer bookkeeping.
/// </summary>
public sealed class FranchiseFileField
{
    private readonly BitView _unformattedValue;
    private object? _value;

    public string Key { get; }
    public OffsetTableEntry Offset { get; }
    public FranchiseFileRecord Parent { get; }
    public bool IsChanged { get; set; }

    public FranchiseFileTable2Field? SecondTableField { get; }
    public FranchiseFileTable3Field? ThirdTableField { get; }

    public FranchiseFileField(string key, OffsetTableEntry offset, FranchiseFileRecord parent)
    {
        Key = key;
        Offset = offset;
        Parent = parent;
        _unformattedValue = new BitView(parent.TableDataArray, parent.ByteOffset, parent.ByteLength);

        if (offset.ValueInSecondTable)
        {
            int index = (int)_unformattedValue.GetBits(offset.Offset, 32, signed: false);
            SecondTableField = new FranchiseFileTable2Field(index, offset.MaxLength ?? 0) { FieldReference = this };
        }
        if (offset.ValueInThirdTable)
        {
            int index = (int)_unformattedValue.GetBits(offset.Offset, 32, signed: false);
            ThirdTableField = new FranchiseFileTable3Field(index, offset.MaxLength ?? 0) { FieldReference = this };
        }
    }

    public bool IsReference => Offset.IsReference;

    public RecordReference? ReferenceData =>
        IsReference ? BinaryUtil.GetReferenceDataFromBitView(_unformattedValue, Offset.Offset) : null;

    public BitView UnformattedValue => _unformattedValue;

    public object? Value
    {
        get
        {
            _value ??= ParseFieldValue(_unformattedValue, Offset);
            return _value;
        }
        set => SetValue(value);
    }

    public void ClearCachedValues()
    {
        _value = null;
    }

    public object? GetValueAs(OffsetTableEntry offset) => ParseFieldValue(_unformattedValue, offset);

    public void BubbleChangeToParent() => Parent.OnEvent("change", this);

    private void SetValue(object? value)
    {
        _value = value;
        IsChanged = true;

        if (Offset.ValueInSecondTable)
        {
            SecondTableField!.Value = value?.ToString() ?? "";
            return;
        }

        if (Offset.ValueInThirdTable)
        {
            ThirdTableField!.Value = value switch
            {
                byte[] bytes => bytes,
                string s => System.Text.Encoding.UTF8.GetBytes(s),
                null => Array.Empty<byte>(),
                _ => System.Text.Encoding.UTF8.GetBytes(value.ToString() ?? "")
            };
            return;
        }

        if (Offset.IsReference)
        {
            if (value is not string refValue || !BinaryUtil.StringOnlyContainsBinaryDigits(refValue))
            {
                throw new ArgumentException(
                    "Reference field values must be a binary-digit string. Use BinaryUtil.GetBinaryReferenceData(tableId, rowNumber).");
            }
            var reference = BinaryUtil.GetReferenceData(refValue);
            _unformattedValue.SetBits(Offset.Offset, reference.TableId, 15);
            _unformattedValue.SetBits(Offset.Offset + 15, reference.RowNumber, 17);
        }
        else if (Offset.Enum is not null)
        {
            try
            {
                var theEnum = GetEnumFromValue(value);
                long decimalEquivalent = BinaryUtil.Bin2Dec(theEnum.UnformattedValue);
                _unformattedValue.SetBits(Offset.Offset, decimalEquivalent, Offset.Length);
                _value = theEnum.Name;
            }
            catch
            {
                if (value is string vs && BinaryUtil.StringOnlyContainsBinaryDigits(vs))
                {
                    _value = vs;
                    _unformattedValue.SetBits(Offset.Offset, BinaryUtil.Bin2Dec(vs), Offset.Length);
                }
                else
                {
                    _value = null;
                    throw;
                }
            }
        }
        else
        {
            switch (Offset.Type)
            {
                case "s_int":
                {
                    long actualValue = Convert.ToInt64(value);
                    _value = actualValue;
                    _unformattedValue.SetBits(Offset.Offset, actualValue - (Offset.MinValue ?? 0), Offset.Length);
                    break;
                }
                case "bool":
                {
                    bool actualValue = value is bool b
                        ? b
                        : string.Equals(value?.ToString(), "true", StringComparison.OrdinalIgnoreCase) ||
                          (value is not null && Convert.ToDouble(value) == 1);
                    _value = actualValue;
                    _unformattedValue.SetBits(Offset.Offset + (Offset.Length - 1), actualValue ? 1 : 0, 1);
                    break;
                }
                case "float":
                {
                    float actualValue = Convert.ToSingle(value);
                    _value = actualValue;
                    _unformattedValue.SetFloat32(Offset.Offset, actualValue);
                    break;
                }
                case "int":
                default:
                {
                    long actualValue = Convert.ToInt64(value);
                    _value = actualValue;
                    if (Offset.MinValue.HasValue || Offset.MaxValue.HasValue)
                    {
                        _unformattedValue.SetBits(Offset.Offset, actualValue, Offset.Length);
                    }
                    else
                    {
                        long maxValue = 1L << (Offset.Length - 1);
                        _unformattedValue.SetBits(Offset.Offset, actualValue + maxValue, Offset.Length);
                    }
                    break;
                }
            }
        }

        Parent.OnEvent("change", this);
    }

    private FranchiseEnumValue GetEnumFromValue(object? value)
    {
        var e = Offset.Enum!;
        if (value is string s)
        {
            var byName = e.GetMemberByName(s);
            if (byName is not null)
            {
                return byName;
            }
            if (long.TryParse(s, out long asLong))
            {
                try { return e.GetMemberByValue(asLong); } catch { /* fall through */ }
            }
            if (BinaryUtil.StringOnlyContainsBinaryDigits(s))
            {
                if (e.TryGetMemberByUnformattedValue(s, out var byBits))
                {
                    return byBits!;
                }
            }
            return e.Members[0];
        }
        if (value is not null)
        {
            try { return e.GetMemberByValue(Convert.ToInt64(value)); } catch { /* fall through */ }
        }
        return e.Members[0];
    }

    private object? ParseFieldValue(BitView unformatted, OffsetTableEntry offset)
    {
        if (offset.ValueInSecondTable)
        {
            return SecondTableField!.Value;
        }
        if (offset.ValueInThirdTable)
        {
            return ThirdTableField!.Value;
        }
        if (offset.Enum is not null)
        {
            string enumUnformattedValue = BinaryUtil.Dec2Bin(unformatted.GetBits(offset.Offset, offset.Length), offset.Enum.MaxLength);
            return offset.Enum.TryGetMemberByUnformattedValue(enumUnformattedValue, out var theEnum)
                ? theEnum!.Name
                : enumUnformattedValue;
        }
        if (offset.IsReference)
        {
            try
            {
                var referenceData = BinaryUtil.GetReferenceDataFromBitView(unformatted, offset.Offset);
                return BinaryUtil.GetBinaryReferenceData(referenceData.TableId, referenceData.RowNumber);
            }
            catch
            {
                return unformatted.GetBits(offset.Offset, offset.Length);
            }
        }

        switch (offset.Type)
        {
            case "s_int":
                return unformatted.GetBits(offset.Offset, offset.Length) + (offset.MinValue ?? 0);
            case "int":
                if (offset.MinValue.HasValue || offset.MaxValue.HasValue)
                {
                    return unformatted.GetBits(offset.Offset, offset.Length);
                }
                else
                {
                    // int[] tables: no min/max, value is stored offset by the field's max representable value.
                    long maxValue = 1L << (offset.Length - 1);
                    long newValue = unformatted.GetBits(offset.Offset, offset.Length);
                    return newValue == 0 ? 0 : newValue - maxValue;
                }
            case "bool":
                return unformatted.GetBits(offset.Offset + (offset.Length - 1), 1) != 0;
            case "float":
                return unformatted.GetFloat32(offset.Offset);
            default:
                // Unhandled/compound type (e.g. a nested struct not caught by isReference) - mirrors
                // the JS library returning the raw BitView in this case.
                return unformatted;
        }
    }
}
