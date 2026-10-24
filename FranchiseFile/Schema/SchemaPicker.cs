using System.Reflection;
using System.Text.RegularExpressions;

namespace MaddenFranchise.Schema;

public sealed class SchemaResourceInfo
{
    public string ResourceName { get; init; } = "";
    public string? GameType { get; init; }
    public int? GameYear { get; init; }
    public long? Major { get; init; }
    public long? Minor { get; init; }
}

/// <summary>
/// Port of services/schemaPicker.js, adapted to scan embedded resources instead of a filesystem
/// directory. Only bundled `.gz` schemas are supported (matching the CFB27-only scope).
/// </summary>
public static class SchemaPicker
{
    private static readonly Regex NamePattern = new(@"(?:(M|C)(\d+)_(\d+)_(\d+))\.gz$", RegexOptions.Compiled);

    public static List<SchemaResourceInfo> RetrieveSchemas(Assembly? assembly = null)
    {
        assembly ??= typeof(SchemaPicker).Assembly;
        var results = new List<SchemaResourceInfo>();
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var match = NamePattern.Match(resourceName);
            string? gameType = null;
            int? gameYear = null;
            long? major = null;
            long? minor = null;

            if (match.Success)
            {
                gameType = string.Equals(match.Groups[1].Value, "C", StringComparison.OrdinalIgnoreCase)
                    ? MaddenFranchise.GameType.College
                    : MaddenFranchise.GameType.Madden;
                gameYear = int.Parse(match.Groups[2].Value);
                major = long.Parse(match.Groups[3].Value);
                minor = long.Parse(match.Groups[4].Value);
            }

            results.Add(new SchemaResourceInfo
            {
                ResourceName = resourceName,
                GameType = gameType,
                GameYear = gameYear,
                Major = major,
                Minor = minor
            });
        }
        return results;
    }

    public static SchemaResourceInfo? Pick(int? gameYear, long major, long minor, string? gameType, Assembly? assembly = null)
    {
        var schemas = RetrieveSchemas(assembly);
        return FindApplicableSchema(schemas, gameYear, major, minor, gameType);
    }

    private static SchemaResourceInfo? FindApplicableSchema(
        List<SchemaResourceInfo> schemaMeta, int? gameYear, long major, long minor, string? gameType)
    {
        IEnumerable<SchemaResourceInfo> schemasToSearch;
        if (gameYear.HasValue)
        {
            schemasToSearch = schemaMeta.Where(s =>
                s.GameYear == gameYear.Value ||
                (s.GameYear is null && s.Major.HasValue && s.Minor.HasValue));
        }
        else
        {
            schemasToSearch = schemaMeta.Where(s => s.Major.HasValue && s.Minor.HasValue);
        }
        var list = schemasToSearch.ToList();

        if (gameType is not null)
        {
            var typedSchemas = list.Where(s => s.GameType == gameType).ToList();
            if (typedSchemas.Count > 0)
            {
                list = typedSchemas;
            }
            else
            {
                var untypedSchemas = list.Where(s => s.GameType is null).ToList();
                if (untypedSchemas.Count > 0)
                {
                    list = untypedSchemas;
                }
            }
        }

        var exactMajor = list.Where(s => s.Major == major).ToList();
        if (exactMajor.Count > 0)
        {
            return GetClosestMinor(exactMajor, minor);
        }

        var majors = list.Where(s => s.Major.HasValue).Select(s => s.Major!.Value).ToList();
        if (majors.Count == 0)
        {
            return null;
        }
        long closestMajor = GetClosestValue(majors, major);
        var majorMatches = list.Where(s => s.Major == closestMajor).ToList();
        if (majorMatches.Count > 0)
        {
            return GetClosestMinor(majorMatches, minor);
        }
        return null;
    }

    private static SchemaResourceInfo GetClosestMinor(List<SchemaResourceInfo> arr, long goal)
    {
        var minors = arr.Where(s => s.Minor.HasValue).Select(s => s.Minor!.Value).ToList();
        long closest = GetClosestValue(minors, goal);
        return arr.First(s => s.Minor == closest);
    }

    private static long GetClosestValue(List<long> arr, long goal)
    {
        return arr.Aggregate((prev, curr) => Math.Abs(curr - goal) < Math.Abs(prev - goal) ? curr : prev);
    }
}
