using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MaddenFranchise.Schema;

/// <summary>
/// Port of madden-franchise's schemaGeneratorV2: turns the FranTk FTX schema documents extracted
/// from the game (one document per enum/schema, linked by &lt;IncludeFile&gt;) into a schema.
///
/// One deliberate difference from the JS generator: enum types are resolved after every document is
/// parsed. The JS version only resolves an enum if its document was parsed before the schema that
/// uses it, which is how the community C27_486_6 schema ended up with Team/Player/Coach fields
/// whose enums read as raw bits.
/// </summary>
public static class FtxSchemaReader
{
    private const string MainDocument = "Franchise-Schemas";
    private static readonly string[] Namespaces = { "Core-Schemas", "Football-Schemas", "Franchise-Schemas" };

    /// <summary>Reads a directory of .ftx files laid out as extract-schema.py writes them.</summary>
    public static SchemaFileDto ReadDirectory(string ftxRoot)
    {
        var docs = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        string root = Path.GetFullPath(ftxRoot).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
        foreach (var file in Directory.EnumerateFiles(root, "*.ftx", SearchOption.AllDirectories))
        {
            string key = StripFtx(file.Substring(root.Length).Replace('/', '\\'));
            foreach (var doc in ReadDocuments(file))
            {
                docs[key] = doc;
            }
        }
        return Build(docs);
    }

    /// <summary>Reads the single concatenated stream extract-schema.py writes as cfb27-schema.xml.</summary>
    public static SchemaFileDto ReadConcatenatedXml(string xmlPath)
    {
        var docs = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var doc in ReadDocuments(xmlPath))
        {
            docs[StripFtx((string?)doc.Attribute("fileName") ?? "")] = doc;
        }
        return Build(docs);
    }

    /// <summary>
    /// Yields each &lt;FranTkData&gt; document in a file. A few documents in the game's stream are
    /// missing their own &lt;FranTkData&gt; wrapper, so their &lt;schemas&gt; block follows the
    /// previous document's closing tag; those are folded into that previous document.
    /// </summary>
    private static IEnumerable<XElement> ReadDocuments(string path)
    {
        var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, DtdProcessing = DtdProcessing.Prohibit };
        XElement? current = null;
        using var reader = XmlReader.Create(path, settings);
        reader.MoveToContent();
        while (!reader.EOF)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.Read();
                continue;
            }
            var element = (XElement)XNode.ReadFrom(reader);
            if (element.Name.LocalName == "FranTkData")
            {
                if (current is not null)
                {
                    yield return current;
                }
                current = element;
            }
            else if (element.Name.LocalName == "schemas" && current is not null)
            {
                current.Add(element);
            }
        }
        if (current is not null)
        {
            yield return current;
        }
    }

    /// <summary>Reads an FTX directory or concatenated .xml and writes it as a madden-franchise V2 .gz schema.</summary>
    public static SchemaFileDto ConvertToGzip(string source, string gzPath)
    {
        var dto = Directory.Exists(source) ? ReadDirectory(source) : ReadConcatenatedXml(source);
        WriteGzip(dto, gzPath);
        return dto;
    }

    public static void WriteGzip(SchemaFileDto dto, string gzPath)
    {
        using var file = File.Create(gzPath);
        using var gzip = new GZipStream(file, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzip);
        JsonSerializer.Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
            .Serialize(writer, dto);
    }

    private static SchemaFileDto Build(Dictionary<string, XElement> docs)
    {
        if (!docs.ContainsKey(MainDocument))
        {
            throw new InvalidDataException($"No {MainDocument} document found. Point this at the extract-schema.py output.");
        }

        var schemas = new List<TableSchemaDto>();
        var schemaMap = new Dictionary<string, TableSchemaDto>();
        var enumMap = new Dictionary<string, SchemaEnumDto>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Parse(string key)
        {
            if (!visited.Add(key) || !docs.TryGetValue(key, out var doc))
            {
                return;
            }
            foreach (var include in doc.Elements("Includes").Elements("IncludeFile"))
            {
                Parse(StripFtx((string?)include.Attribute("fileName") ?? ""));
            }

            // fast-xml-parser groups same-named children, so the JS generator handles all of one tag
            // before the next, in order of first appearance. Kept for identical schema ordering.
            var items = doc.Elements("schemas").Elements().ToList();
            foreach (var tag in items.Select(e => e.Name.LocalName).Distinct().ToList())
            {
                foreach (var item in items.Where(e => e.Name.LocalName == tag))
                {
                    if (tag == "enum")
                    {
                        var e = ReadEnum(item);
                        enumMap[e.Name] = e;
                    }
                    else if (tag == "schema")
                    {
                        var s = ReadSchema(item);
                        schemas.Add(s);
                        schemaMap[s.Name] = s;
                    }
                }
            }
        }

        Parse(MainDocument);
        AddExtraSchemas(schemas, schemaMap);
        CalculateInheritedSchemas(schemas);

        foreach (var attr in schemas.SelectMany(s => s.Attributes))
        {
            attr.Enum = enumMap.ContainsKey(attr.Type) ? new JValue(attr.Type) : null;
        }

        var main = docs[MainDocument];
        int major = Namespaces.Sum(ns => docs.TryGetValue(ns, out var d) ? (int?)d.Attribute("dataMajorVersion") ?? 0 : 0);
        string databaseName = (string?)main.Attribute("databaseName") ?? "";
        var year = Regex.Match(databaseName, databaseName.Contains("CollegeFB") ? @"CollegeFB(\d{2})" : @"Madden(\d{2})");

        return new SchemaFileDto
        {
            // Major is the sum of the three namespace majors, which is what save headers store
            // (e.g. 57 + 290 + 486 = 833); this is what SchemaPicker matches against.
            Meta = new SchemaMetaDto { Major = major, Minor = 0, GameYear = year.Success ? int.Parse(year.Groups[1].Value) : null },
            Schemas = schemas,
            EnumMap = enumMap
        };
    }

    private static SchemaEnumDto ReadEnum(XElement item)
    {
        var e = new FranchiseEnum((string?)item.Attribute("name") ?? "", (string?)item.Attribute("assetId"));
        foreach (var member in item.Elements("attribute"))
        {
            e.AddMember((string?)member.Attribute("name") ?? "",
                ParseIntLikeJs((string?)member.Attribute("idx")),
                ParseIntLikeJs((string?)member.Attribute("value")));
        }
        e.SetMemberLength();
        return new SchemaEnumDto
        {
            Name = e.Name,
            AssetId = e.AssetId,
            _isRecordPersistent = (string?)item.Attribute("isRecordPersistent"),
            MaxLength = e.MaxLength,
            Members = e.Members.Select(m => new SchemaEnumMemberDto
            {
                Name = m.Name, Index = m.Index, Value = m.Value, UnformattedValue = m.UnformattedValue
            }).ToList()
        };
    }

    private static TableSchemaDto ReadSchema(XElement item)
    {
        return new TableSchemaDto
        {
            Name = (string?)item.Attribute("name") ?? "",
            AssetId = (string?)item.Attribute("assetId"),
            NumMembers = (string?)item.Attribute("numMembers"),
            Base = (string?)item.Attribute("base"),
            Attributes = item.Elements("attribute").Select(a => new SchemaAttributeDto
            {
                Index = (string?)a.Attribute("idx"),
                Name = (string?)a.Attribute("name") ?? "",
                Type = (string?)a.Attribute("type") ?? "",
                MinValue = (string?)a.Attribute("minValue"),
                MaxValue = (string?)a.Attribute("maxValue"),
                MaxLength = (string?)a.Attribute("maxLen"),
                Default = NullIfEmpty((string?)a.Attribute("default")),
                Final = (string?)a.Attribute("final"),
                Const = (string?)a.Attribute("const")
            }).ToList()
        };
    }

    /// <summary>
    /// madden-franchise's data/schemas/extra-schemas.json: base types (Reaction, Event, Request,
    /// Scheduler...) the FTX set references but never defines. Added only when missing.
    /// </summary>
    private static void AddExtraSchemas(List<TableSchemaDto> schemas, Dictionary<string, TableSchemaDto> schemaMap)
    {
        var assembly = typeof(FtxSchemaReader).Assembly;
        string? resource = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("extra-schemas.json", StringComparison.OrdinalIgnoreCase));
        if (resource is null)
        {
            return;
        }
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        var extras = JsonConvert.DeserializeObject<List<TableSchemaDto>>(reader.ReadToEnd()) ?? new();
        foreach (var extra in extras)
        {
            if (schemaMap.ContainsKey(extra.Name))
            {
                continue;
            }
            for (int i = 0; i < extra.Attributes.Count; i++)
            {
                extra.Attributes[i].Index ??= i.ToString();
            }
            schemas.Insert(0, extra);
            schemaMap[extra.Name] = extra;
        }
    }

    /// <summary>
    /// Moves inherited fields to the front of derived schemas in the base schema's order. The save's
    /// table headers list field offsets in that order, so this must match the JS generator exactly.
    /// </summary>
    private static void CalculateInheritedSchemas(List<TableSchemaDto> schemas)
    {
        foreach (var schema in schemas.Where(s => s.Base is not null && !s.Base.Contains("()")).ToList())
        {
            var baseSchema = schemas.FirstOrDefault(s => s.Name == schema.Base);
            if (baseSchema is null)
            {
                continue;
            }
            for (int i = 0; i < baseSchema.Attributes.Count; i++)
            {
                int oldIndex = schema.Attributes.FindIndex(a => a.Name == baseSchema.Attributes[i].Name);
                if (oldIndex >= 0)
                {
                    var item = schema.Attributes[oldIndex];
                    schema.Attributes.RemoveAt(oldIndex);
                    schema.Attributes.Insert(Math.Min(i, schema.Attributes.Count), item);
                }
            }
        }
    }

    private static string StripFtx(string name) =>
        name.EndsWith(".ftx", StringComparison.OrdinalIgnoreCase) ? name.Substring(0, name.Length - 4) : name;

    private static string? NullIfEmpty(string? s) => string.IsNullOrEmpty(s) ? null : s;

    /// <summary>JS parseInt: leading integer digits, NaN (here 0) otherwise.</summary>
    private static int ParseIntLikeJs(string? s)
    {
        var m = s is null ? null : Regex.Match(s.Trim(), @"^[+-]?\d+");
        return m is { Success: true } ? int.Parse(m.Value) : 0;
    }
}
