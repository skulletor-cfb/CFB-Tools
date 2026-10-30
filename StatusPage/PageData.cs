using System.Text.Json;
using System.Text.Json.Serialization;
using CFB27.Data.Model;

namespace StatusPage
{
    internal sealed class PageData
    {
        public string Save { get; set; } = string.Empty;
        public int Season { get; set; }
        /// <summary>SeasonWeek of the newest week with results (-1 before any); its scores start hidden.</summary>
        public int LatestWeek { get; set; }
        public List<WeekInfo> Weeks { get; set; } = new List<WeekInfo>();
        public List<string> Conferences { get; set; } = new List<string>();
        public List<GameInfo> Games { get; set; } = new List<GameInfo>();
        /// <summary>the user's team colors; null keeps the default green</summary>
        public TeamTheme? Theme { get; set; }
    }

    internal sealed class WeekInfo
    {
        public int Week { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    internal sealed class TeamInfo
    {
        /// <summary>Team row, for adding up records</summary>
        public int Row { get; set; }
        public string Name { get; set; } = string.Empty;
        /// <summary>a generic FCS team (TeamIndex 255), which gets no record</summary>
        public bool Fcs { get; set; }
        /// <summary>AP (media poll) rank, 0 when unranked</summary>
        public int Rank { get; set; }
    }

    internal sealed class GameInfo
    {
        /// <summary>SeasonGame row</summary>
        public int Id { get; set; }
        public int Week { get; set; }
        public string Date { get; set; } = string.Empty;
        public int SortKey { get; set; }
        public string Time { get; set; } = string.Empty;
        public TeamInfo Home { get; set; } = new TeamInfo();
        public TeamInfo Away { get; set; } = new TeamInfo();
        public bool Neutral { get; set; }
        public string? Site { get; set; }
        public string? Bowl { get; set; }
        /// <summary>Rivalry.Name when the two teams are rivals</summary>
        public string? Rivalry { get; set; }
        public bool Played { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public bool Overtime { get; set; }
        /// <summary>conferences of the two teams (one for a conference game, none for an FCS team)</summary>
        public List<string> Conferences { get; set; } = new List<string>();
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(PageData))]
    internal sealed partial class PageDataContext : JsonSerializerContext
    {
    }

    internal static class PageWriter
    {
        private const string DataPlaceholder = "/*PAGE_DATA*/null";

        /// <summary>The page template with the season's data embedded (System.Text.Json escapes &lt; so it can't close the script).</summary>
        public static string Write(PageData page)
        {
            using var stream = typeof(PageWriter).Assembly.GetManifestResourceStream("StatusPage.status.html")!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Replace(DataPlaceholder, JsonSerializer.Serialize(page, PageDataContext.Default.PageData));
        }
    }
}
