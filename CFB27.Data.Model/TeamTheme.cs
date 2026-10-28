#nullable enable
using System;
using System.Linq;
using MaddenFranchise;

namespace CFB27.Data.Model
{
    /// <summary>
    /// Page colors from a team (Team.TEAM_BACKGROUNDCOLOR* primary, *2 secondary): the status page uses the user's team, the
    /// TV schedule last season's national champion.
    /// </summary>
    public sealed class TeamTheme
    {
        private const string NoRef = "00000000000000000000000000000000";
        private const string White = "#ffffff";
        private const string Ink = "#1a1a1a";
        /// <summary>WCAG AA for normal text</summary>
        private const double ReadableContrast = 4.5;

        public string Team { get; set; } = string.Empty;
        /// <summary>pill, day header and button fill</summary>
        public string Accent { get; set; } = string.Empty;
        /// <summary>text on <see cref="Accent"/></summary>
        public string OnAccent { get; set; } = string.Empty;
        /// <summary>the accent darkened until it reads as text on white</summary>
        public string AccentInk { get; set; } = string.Empty;
        /// <summary>row hover tint</summary>
        public string Hover { get; set; } = string.Empty;
        public string Secondary { get; set; } = string.Empty;

        /// <summary>
        /// The user's team: the Team whose UserCharacter is set, else the team of the user-controlled coach. Null when the
        /// save has neither (the page keeps its default green).
        /// </summary>
        public static TeamTheme? ForUserTeam(FranchiseFile dynasty, FranchiseFileTable teams)
        {
            var team = teams.Records.FirstOrDefault(t => !t.IsEmpty && (t["UserCharacter"] as string ?? NoRef) != NoRef);
            if (team == null)
            {
                var coach = dynasty.GetPrimaryTable("Coach").Records.FirstOrDefault(c => !c.IsEmpty && c["IsUserControlled"] is true);
                var teamIndex = coach == null ? -1 : Convert.ToInt32(coach["TeamIndex"]);
                team = teams.Records.FirstOrDefault(t => !t.IsEmpty && Convert.ToInt32(t["TeamIndex"]) == teamIndex);
            }

            return team == null ? null : ForTeam(team);
        }

        /// <summary>The colors of a Team record.</summary>
        public static TeamTheme ForTeam(FranchiseFileRecord team)
        {
            var primary = Rgb(team, string.Empty);
            var secondary = Convert.ToBoolean(team["TEAM_HAS_SECONDARY_COLOR"]) ? Rgb(team, "2") : primary;
            return new TeamTheme
            {
                Team = team["DisplayName"] as string ?? string.Empty,
                Accent = Hex(primary),
                OnAccent = Contrast(primary, Parse(White)) >= Contrast(primary, Parse(Ink)) ? White : Ink,
                AccentInk = Hex(Darken(primary)),
                Hover = Hex(Mix(primary, Parse(White), 0.88)),
                Secondary = Hex(secondary),
            };
        }

        private static (double R, double G, double B) Rgb(FranchiseFileRecord team, string suffix) => (
            Convert.ToInt32(team["TEAM_BACKGROUNDCOLORR" + suffix]),
            Convert.ToInt32(team["TEAM_BACKGROUNDCOLORG" + suffix]),
            Convert.ToInt32(team["TEAM_BACKGROUNDCOLORB" + suffix]));

        /// <summary>Mixes in black, 5% at a time, until the color reads on white.</summary>
        private static (double R, double G, double B) Darken((double R, double G, double B) color)
        {
            var white = Parse(White);
            for (double black = 0; black <= 1; black += 0.05)
            {
                var candidate = Mix(color, (0, 0, 0), black);
                if (Contrast(candidate, white) >= ReadableContrast)
                {
                    return candidate;
                }
            }

            return Parse(Ink);
        }

        private static (double R, double G, double B) Mix((double R, double G, double B) a, (double R, double G, double B) b, double amount) =>
            (a.R + (b.R - a.R) * amount, a.G + (b.G - a.G) * amount, a.B + (b.B - a.B) * amount);

        private static double Contrast((double R, double G, double B) a, (double R, double G, double B) b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        private static double Luminance((double R, double G, double B) c)
        {
            static double Channel(double v)
            {
                v /= 255;
                return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
        }

        private static (double R, double G, double B) Parse(string hex) =>
            (Convert.ToInt32(hex.Substring(1, 2), 16), Convert.ToInt32(hex.Substring(3, 2), 16), Convert.ToInt32(hex.Substring(5, 2), 16));

        private static string Hex((double R, double G, double B) c) =>
            $"#{(int)Math.Round(c.R):x2}{(int)Math.Round(c.G):x2}{(int)Math.Round(c.B):x2}";
    }
}
