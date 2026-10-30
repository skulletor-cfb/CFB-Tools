namespace MaddenFranchise;

public sealed class FranchiseFileSettings
{
    /// <summary>If true, any field value change immediately triggers a re-pack of the in-memory buffer.</summary>
    public bool SaveOnChange { get; set; } = false;

    /// <summary>
    /// If true, un-empty a record's row automatically whenever any of its fields are edited
    /// (matches autoUnempty in the JS library).
    /// </summary>
    public bool AutoUnempty { get; set; } = false;

    /// <summary>Overrides game-year detection (useful for FTC-style files that can't self-report a year).</summary>
    public int? GameYearOverride { get; set; }

    /// <summary>Overrides game-type detection ("madden" or "college").</summary>
    public string? GameTypeOverride { get; set; }

    /// <summary>
    /// Load the schema from this path instead of picking the closest embedded one: a .gz schema,
    /// the cfb27-schema.xml from extract-schema.py, or its folder of .ftx files.
    /// </summary>
    public string? SchemaPath { get; set; }
}
