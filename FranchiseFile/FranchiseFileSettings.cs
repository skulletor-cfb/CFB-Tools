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
}
