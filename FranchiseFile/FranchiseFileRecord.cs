namespace MaddenFranchise;

/// <summary>
/// Port of FranchiseFileRecord.js. JS uses a Proxy for `record.FieldName` dynamic property access;
/// C# exposes the same capability via the string indexer instead (`record["FieldName"]`).
///
/// Rather than owning a copied byte[] slice, a record references its parent table's backing byte
/// array directly (TableDataArray/ByteOffset/ByteLength) so that field writes land directly in the
/// table's shared buffer - mirroring how Node Buffer.slice() (a zero-copy view) behaves in the
/// original JS library.
/// </summary>
public sealed class FranchiseFileRecord
{
    private readonly Dictionary<string, FranchiseFileField> _fields;
    private readonly List<FranchiseFileField> _fieldsArray;
    private bool _isChanged;

    public int Index { get; }
    public int? ArraySize { get; set; }
    public bool IsEmpty { get; set; }
    public FranchiseFileTable Parent { get; }

    public byte[] TableDataArray { get; }
    public int ByteOffset { get; }
    public int ByteLength { get; }

    public FranchiseFileRecord(byte[] tableDataArray, int byteOffset, int byteLength, int index,
        List<OffsetTableEntry> offsetTable, FranchiseFileTable parent)
    {
        TableDataArray = tableDataArray;
        ByteOffset = byteOffset;
        ByteLength = byteLength;
        Index = index;
        Parent = parent;
        _fields = new Dictionary<string, FranchiseFileField>();
        _fieldsArray = new List<FranchiseFileField>(offsetTable.Count);
        foreach (var offset in offsetTable)
        {
            var field = new FranchiseFileField(offset.Name, offset, this);
            _fields[offset.Name] = field;
            _fieldsArray.Add(field);
        }
    }

    /// <summary>A snapshot copy of the record's current bytes (used when assembling save output).</summary>
    public byte[] HexData => TableDataArray.AsSpan(ByteOffset, ByteLength).ToArray();

    public IReadOnlyDictionary<string, FranchiseFileField> Fields => _fields;
    public IReadOnlyList<FranchiseFileField> FieldsArray => _fieldsArray;

    public bool IsChanged
    {
        get => _isChanged;
        set
        {
            _isChanged = value;
            if (!value)
            {
                foreach (var f in _fieldsArray)
                {
                    f.IsChanged = false;
                }
            }
        }
    }

    /// <summary>Indexer equivalent of the JS library's Proxy-based `record.FieldName` access.</summary>
    public object? this[string fieldName]
    {
        get => _fields.TryGetValue(fieldName, out var field) ? field.Value : null;
        set
        {
            if (_fields.TryGetValue(fieldName, out var field))
            {
                field.Value = value;
            }
        }
    }

    public FranchiseFileField? GetFieldByKey(string key) => _fields.GetValueOrDefault(key);

    public object? GetValueByKey(string key) => GetFieldByKey(key)?.Value;

    public MaddenFranchise.Util.RecordReference? GetReferenceDataByKey(string key) => GetFieldByKey(key)?.ReferenceData;

    public void Empty()
    {
        ClearCachedFieldValues();
        Parent.OnEvent("empty", this);
        IsEmpty = true;
    }

    public void OnEvent(string name, FranchiseFileField field)
    {
        if (name != "change")
        {
            return;
        }

        if (ArraySize.HasValue)
        {
            var referenceData = field.ReferenceData;
            if (field.Offset.Index >= ArraySize.Value)
            {
                if (field.IsReference && referenceData is not null &&
                    (referenceData.Value.TableId != 0 || referenceData.Value.RowNumber != 0))
                {
                    ArraySize = field.Offset.Index + 1;
                }
            }
            else if (field.IsReference && referenceData is not null &&
                     referenceData.Value.TableId == 0 && referenceData.Value.RowNumber == 0)
            {
                ArraySize = field.Offset.Index;
            }
        }

        Parent.OnEvent("change", this);
    }

    public void ClearCachedFieldValues()
    {
        foreach (var field in _fieldsArray)
        {
            field.ClearCachedValues();
        }
    }
}
