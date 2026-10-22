namespace MaddenFranchise.Schema;

public sealed class FranchiseEnum
{
    private readonly List<FranchiseEnumValue> _members = new();

    public string Name { get; }
    public string? AssetId { get; }
    public bool? IsRecordPersistent { get; }
    public IReadOnlyList<FranchiseEnumValue> Members => _members;
    public int MaxLength { get; private set; } = -1;

    public FranchiseEnum(string name, string? assetId = null, bool? isRecordPersistent = null)
    {
        Name = name;
        AssetId = assetId;
        IsRecordPersistent = isRecordPersistent;
    }

    public static FranchiseEnum FromDto(SchemaEnumDto dto)
    {
        var e = new FranchiseEnum(dto.Name, dto.AssetId, dto.IsRecordPersistent);
        foreach (var member in dto.Members)
        {
            e.AddMember(member.Name, member.Index, member.Value, member.UnformattedValue);
        }
        e.MaxLength = e._members.Count > 0 ? e._members[0].UnformattedValue.Length : dto.MaxLength ?? -1;
        return e;
    }

    public void AddMember(string name, int index, long value, string? unformattedValue = null)
    {
        _members.Add(new FranchiseEnumValue(name, index, value, unformattedValue));
    }

    public FranchiseEnumValue GetMemberByValue(long value)
    {
        var matches = _members.Where(m => m.Name != "First_" && m.Name != "Last_" && m.Value == value).ToList();
        if (matches.Count == 0)
        {
            throw new ArgumentException(
                $"Argument is not a valid enum value for this field. You passed in {value}. Field name: {Name}");
        }
        var noUnderscore = matches.FirstOrDefault(m => !m.Name.EndsWith("_"));
        return noUnderscore ?? matches[0];
    }

    public FranchiseEnumValue GetMemberByUnformattedValue(string value)
    {
        if (!TryGetMemberByUnformattedValue(value, out var member))
        {
            throw new ArgumentException(
                $"Argument is not a valid enum value for this field. You passed in {value}. Field name: {Name}");
        }
        return member!;
    }

    /// <summary>
    /// Non-throwing lookup used when parsing records: saves can hold values the schema doesn't
    /// define (e.g. SeasonGame.WeatherIconId), and throwing per field is slow and noisy under a debugger.
    /// </summary>
    public bool TryGetMemberByUnformattedValue(string value, out FranchiseEnumValue? member)
    {
        member = null;
        if (value.Length > MaxLength)
        {
            string cutOff = value.Substring(0, value.Length - MaxLength);
            bool cutOffContainsData = cutOff.Any(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '1' && c <= '9'));
            if (cutOffContainsData)
            {
                return false;
            }
            value = value.Substring(value.Length - MaxLength);
        }

        var matches = _members.Where(m => m.Name != "First_" && m.Name != "Last_" && m.UnformattedValue == value).ToList();
        member = matches.FirstOrDefault(m => !m.Name.EndsWith("_")) ?? matches.FirstOrDefault();
        return member is not null;
    }

    public FranchiseEnumValue? GetMemberByName(string name)
    {
        return _members.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void SetMemberLength()
    {
        if (_members.Count == 0)
        {
            return;
        }
        var maxValueMember = _members.Aggregate((a, b) => a.Value > b.Value ? a : b);
        bool hasNegative = _members.Any(m => m.Value < 0);
        MaxLength = hasNegative ? maxValueMember.UnformattedValue.Length + 1 : maxValueMember.UnformattedValue.Length;
        foreach (var member in _members)
        {
            member.SetMemberLength(MaxLength);
        }
    }
}
