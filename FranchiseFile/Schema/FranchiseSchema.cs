using System.IO.Compression;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MaddenFranchise.Schema;

/// <summary>
/// Port of FranchiseSchema.js. Loads madden-franchise .gz (V1 or V2) schemas, or FTX documents
/// extracted from the game via <see cref="FtxSchemaReader"/>.
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
        return LoadFromGzipStream(resourceStream);
    }

    /// <summary>
    /// Loads a schema from disk: a madden-franchise .gz schema, the cfb27-schema.xml stream
    /// extract-schema.py writes, or its directory of .ftx files.
    /// </summary>
    public static FranchiseSchema LoadFromPath(string path)
    {
        if (Directory.Exists(path))
        {
            return FromDto(FtxSchemaReader.ReadDirectory(path));
        }
        if (path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            using var file = File.OpenRead(path);
            return LoadFromGzipStream(file);
        }
        if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            return FromDto(FtxSchemaReader.ReadConcatenatedXml(path));
        }
        throw new ArgumentException($"Unsupported schema path: {path}. Use a .gz file, a .xml file, or a folder of .ftx files.");
    }

    public static FranchiseSchema LoadFromGzipStream(Stream gzStream)
    {
        using var gzip = new GZipStream(gzStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        using var jsonReader = new JsonTextReader(reader);
        var dto = JsonSerializer.CreateDefault().Deserialize<SchemaFileDto>(jsonReader)
            ?? throw new InvalidDataException("The schema file is empty or not valid JSON.");
        return FromDto(dto);
    }

    public static FranchiseSchema FromDto(SchemaFileDto dto)
    {
        var tableSchemas = new List<TableSchema>(dto.Schemas.Count);
        var enumsByName = new Dictionary<string, FranchiseEnum>();
        foreach (var schemaDto in dto.Schemas)
        {
            var attributes = new List<SchemaAttribute>(schemaDto.Attributes.Count);
            foreach (var attrDto in schemaDto.Attributes)
            {
                FranchiseEnum? franchiseEnum = ResolveEnum(attrDto.Enum, dto.EnumMap, enumsByName);
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

    private static FranchiseEnum? ResolveEnum(
        JToken? enumToken, Dictionary<string, SchemaEnumDto>? enumMap, Dictionary<string, FranchiseEnum> enumsByName)
    {
        switch (enumToken?.Type)
        {
            case JTokenType.Object:
                // V1: enum denormalized inline on the attribute.
                return FranchiseEnum.FromDto(enumToken.ToObject<SchemaEnumDto>()!);
            case JTokenType.String:
                // V2: name of an entry in the top-level enumMap.
                string name = enumToken.Value<string>()!;
                if (enumsByName.TryGetValue(name, out var cached))
                {
                    return cached;
                }
                if (enumMap is null || !enumMap.TryGetValue(name, out var enumDto))
                {
                    return null;
                }
                var resolved = FranchiseEnum.FromDto(enumDto);
                enumsByName[name] = resolved;
                return resolved;
            default:
                return null;
        }
    }

    private static int? ParseNullableInt(string? value) =>
        value is not null && int.TryParse(value, out var result) ? result : null;

    private static long? ParseNullableLong(string? value) =>
        value is not null && long.TryParse(value, out var result) ? result : null;
}
