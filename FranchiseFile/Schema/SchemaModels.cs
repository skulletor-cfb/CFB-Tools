using Newtonsoft.Json;

namespace MaddenFranchise.Schema;

// --- Raw JSON DTOs matching the gzip'd schema files (data/schemas/<year>/*.gz in the JS library) ---

public sealed class SchemaFileDto
{
    [JsonProperty(PropertyName="meta")] public SchemaMetaDto? Meta { get; set; }
    [JsonProperty(PropertyName="schemas")] public List<TableSchemaDto> Schemas { get; set; } = new();
}

public sealed class SchemaMetaDto
{
    [JsonProperty(PropertyName="major")] public int Major { get; set; }
    [JsonProperty(PropertyName="minor")] public int Minor { get; set; }
    [JsonProperty(PropertyName="gameYear")] public int? GameYear { get; set; }
}

public sealed class TableSchemaDto
{
    [JsonProperty(PropertyName="name")] public string Name { get; set; } = "";
    [JsonProperty(PropertyName="numMembers")] public string? NumMembers { get; set; }
    [JsonProperty(PropertyName="base")] public string? Base { get; set; }
    [JsonProperty(PropertyName="assetId")] public string? AssetId { get; set; }
    [JsonProperty(PropertyName="attributes")] public List<SchemaAttributeDto> Attributes { get; set; } = new();
}

public sealed class SchemaAttributeDto
{
    [JsonProperty(PropertyName="index")] public string? Index { get; set; }
    [JsonProperty(PropertyName="name")] public string Name { get; set; } = "";
    [JsonProperty(PropertyName="type")] public string Type { get; set; } = "";
    [JsonProperty(PropertyName="minValue")] public string? MinValue { get; set; }
    [JsonProperty(PropertyName="maxValue")] public string? MaxValue { get; set; }
    [JsonProperty(PropertyName="maxLength")] public string? MaxLength { get; set; }
    [JsonProperty(PropertyName="default")] public string? Default { get; set; }
    [JsonProperty(PropertyName="final")] public string? Final { get; set; }
    [JsonProperty(PropertyName="const")] public string? Const { get; set; }
    [JsonProperty(PropertyName="enum")] public object? Enum { get; set; }
}

public sealed class SchemaEnumDto
{
    [JsonProperty(PropertyName="_name")] public string Name { get; set; } = "";
    [JsonProperty(PropertyName="_assetId")] public string? AssetId { get; set; }
    [JsonIgnore]
    public bool? IsRecordPersistent { get => bool.TryParse(_isRecordPersistent, out var result) ? result : null; set => _isRecordPersistent = value.ToString(); }
    [JsonProperty(PropertyName="_isRecordPersistent")] public string? _isRecordPersistent { get; set; }
    [JsonProperty(PropertyName="_members")] public List<SchemaEnumMemberDto> Members { get; set; } = new();
    [JsonProperty(PropertyName="_maxLength")] public int? MaxLength { get; set; }
}

public sealed class SchemaEnumMemberDto
{
    [JsonProperty(PropertyName="_name")] public string Name { get; set; } = "";
    [JsonProperty(PropertyName="_index")] public int Index { get; set; }
    [JsonProperty(PropertyName="_value")] public long Value { get; set; }
    [JsonProperty(PropertyName="_unformattedValue")] public string? UnformattedValue { get; set; }
}

// --- Runtime models (parsed/resolved from the DTOs above), mirroring the JS runtime shape ---

public sealed class TableSchema
{
    public required string Name { get; set; }
    public int NumMembers { get; set; }
    public string? Base { get; set; }
    public required List<SchemaAttribute> Attributes { get; set; }
}

public sealed class SchemaAttribute
{
    public int? Index { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public long? MinValue { get; set; }
    public long? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string? Default { get; set; }
    public bool Final { get; set; }
    public bool Const { get; set; }
    public FranchiseEnum? Enum { get; set; }

    public bool IsNegative => (MinValue.HasValue && MinValue.Value < 0) || (MaxValue.HasValue && MaxValue.Value < 0);
}
