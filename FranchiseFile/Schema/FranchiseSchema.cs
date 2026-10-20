using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace MaddenFranchise.Schema;

/// <summary>
/// Port of FranchiseSchema.js, narrowed to the gzip+JSON schema format only (the XML/FTX schema
/// generator is not ported - see README/plan for rationale).
/// </summary>
public sealed class FranchiseSchema
{
    private readonly Dictionary<string, TableSchema> _schemaMap;

    public int Major { get; }
    public int Minor { get; }
    public int? GameYear { get; }
    public IReadOnlyList<TableSchema> Schemas { get; }

    private FranchiseSchema(int major, int minor, int? gameYear, List<TableSchema> schemas)
    {
        Major = major;
        Minor = minor;
        GameYear = gameYear;
        Schemas = schemas;
        _schemaMap = new Dictionary<string, TableSchema>();
        foreach (var schema in schemas)
        {
            _schemaMap[schema.Name] = schema;
        }
    }

    public TableSchema? GetSchema(string name) => _schemaMap.GetValueOrDefault(name);

    public static FranchiseSchema LoadClosest(int gameYear, long major, long minor, string? gameType, Assembly? assembly = null)
    {
        var picked = SchemaPicker.Pick(gameYear, major, minor, gameType, assembly)
            ?? throw new InvalidOperationException(
                $"No embedded schema found for game year {gameYear} (gameType={gameType}, major={major}, minor={minor}).");
        return LoadFromResource(picked.ResourceName, assembly ?? typeof(FranchiseSchema).Assembly);
    }

    public static FranchiseSchema LoadFromResource(string resourceName, Assembly? assembly = null)
    {
        assembly ??= typeof(FranchiseSchema).Assembly;
        using var resourceStream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded schema resource not found: {resourceName}");
        using var gzip = new GZipStream(resourceStream, CompressionMode.Decompress);
        using var memory = new MemoryStream();
        gzip.CopyTo(memory);
        memory.Position = 0;

        var dto = JsonSerializer.Deserialize<SchemaFileDto>(memory)
            ?? throw new InvalidOperationException($"Failed to parse schema resource: {resourceName}");

        var tableSchemas = new List<TableSchema>(dto.Schemas.Count);
        // Enums are denormalized inline in V1 schemas (which is the only format the bundled
        // .gz resources use), so we build a fresh FranchiseEnum per attribute occurrence.
        foreach (var schemaDto in dto.Schemas)
        {
            var attributes = new List<SchemaAttribute>(schemaDto.Attributes.Count);
            foreach (var attrDto in schemaDto.Attributes)
            {
                FranchiseEnum? franchiseEnum = attrDto.Enum is SchemaEnumDto attrDtoEnum ? FranchiseEnum.FromDto(attrDtoEnum) : null;
                attributes.Add(new SchemaAttribute
                {
                    Index = ParseNullableInt(attrDto.Index),
                    Name = attrDto.Name,
                    Type = attrDto.Type,
                    MinValue = ParseNullableLong(attrDto.MinValue),
                    MaxValue = ParseNullableLong(attrDto.MaxValue),
                    MaxLength = ParseNullableInt(attrDto.MaxLength),
                    Default = attrDto.Default,
                    Final = attrDto.Final == "true",
                    Const = attrDto.Const == "true",
                    Enum = franchiseEnum
                });
            }

            tableSchemas.Add(new TableSchema
            {
                Name = schemaDto.Name,
                NumMembers = ParseNullableInt(schemaDto.NumMembers) ?? attributes.Count,
                Base = schemaDto.Base,
                Attributes = attributes
            });
        }

        return new FranchiseSchema(dto.Meta?.Major ?? 0, dto.Meta?.Minor ?? 0, dto.Meta?.GameYear, tableSchemas);
    }

    private static int? ParseNullableInt(string? value) =>
        value is not null && int.TryParse(value, out var result) ? result : null;

    private static long? ParseNullableLong(string? value) =>
        value is not null && long.TryParse(value, out var result) ? result : null;
}
