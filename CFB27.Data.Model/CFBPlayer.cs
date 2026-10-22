using MaddenFranchise;
using System;

namespace CFB27.Data.Model
{
    public class CFBPlayer : BaseRecord
    {
        /// <summary>
        /// TeamIndex value for players not on an FBS roster: the 5 generic FCS teams, recruits and other unattached players.
        /// </summary>
        public const int NoTeamIndex = 255;

        /// <summary>
        /// Reads only the fields the web page creator uses. The Player table has ~280 fields and ~16k rows,
        /// so the JSON round trip in DefaultTransform is too slow here.
        /// </summary>
        public static CFBPlayer Transform(FranchiseFile dynasty, FranchiseFileRecord record)
        {
            var player = new CFBPlayer
            {
                Row = record.Index,
                IsEmpty = record.IsEmpty,
            };

            if (record.IsEmpty)
            {
                return player;
            }

            player.FirstName = record["FirstName"] as string;
            player.LastName = record["LastName"] as string;
            player.TeamIndex = Convert.ToInt32(record["TeamIndex"]);
            player.Position = EnumValue(record, "Position");
            player.SchoolYear = EnumValue(record, "SchoolYear");
            player.RedshirtStatus = record["RedshirtStatus"] as string;
            player.JerseyNum = Convert.ToInt32(record["JerseyNum"]);
            player.Height = Convert.ToInt32(record["Height"]);
            player.Weight = Convert.ToInt32(record["Weight"]);
            player.OverallRating = Convert.ToInt32(record["OverallRating"]);
            player.SpeedRating = Convert.ToInt32(record["SpeedRating"]);
            player.AccelerationRating = Convert.ToInt32(record["AccelerationRating"]);
            player.AgilityRating = Convert.ToInt32(record["AgilityRating"]);
            player.StrengthRating = Convert.ToInt32(record["StrengthRating"]);
            player.AwarenessRating = Convert.ToInt32(record["AwarenessRating"]);
            player.Handedness = record["PLYR_HANDEDNESS"] as string;
            player.Portrait = Convert.ToInt32(record["PLYR_PORTRAIT"]);
            player.PresentationId = Convert.ToInt32(record["PresentationId"]);
            player.HomeTown = record["PLYR_HOME_TOWN"] as string;
            player.HomeState = record["PLYR_HOME_STATE"] as string;
            player.StarRating = ToStars(record["ProspectStarRating"] as string);
            return player;
        }

        // ProspectQuality enum: FIVE_STAR ... ONE_STAR, Invalid for players who never were recruits
        private static int ToStars(string prospectQuality)
        {
            switch (prospectQuality)
            {
                case "FIVE_STAR": return 5;
                case "FOUR_STAR": return 4;
                case "THREE_STAR": return 3;
                case "TWO_STAR": return 2;
                case "ONE_STAR": return 1;
                default: return 0;
            }
        }

        // enum fields come back as member names; use the schema's numeric value (e.g. PositionE QB=0 ... P=20, SchoolYear Freshman=0 ... Senior=3)
        private static int EnumValue(FranchiseFileRecord record, string key)
        {
            var field = record.GetFieldByKey(key);
            var member = field.Offset.Enum.GetMemberByName(field.Value as string);
            return member == null ? -1 : (int)member.Value;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TeamIndex { get; set; }
        public int Position { get; set; }
        public int SchoolYear { get; set; }
        public string RedshirtStatus { get; set; }
        public int JerseyNum { get; set; }

        /// <summary>inches</summary>
        public int Height { get; set; }

        /// <summary>pounds over 160, same as NCAA 14</summary>
        public int Weight { get; set; }

        public int OverallRating { get; set; }
        public int SpeedRating { get; set; }
        public int AccelerationRating { get; set; }
        public int AgilityRating { get; set; }
        public int StrengthRating { get; set; }
        public int AwarenessRating { get; set; }
        public string Handedness { get; set; }
        public int Portrait { get; set; }

        /// <summary>not unique: several players can share one, so key players by Row instead</summary>
        public int PresentationId { get; set; }

        /// <summary>recruiting stars, 1-5; 0 when not set</summary>
        public int StarRating { get; set; }

        public string HomeTown { get; set; }
        public string HomeState { get; set; }

        public bool IsOnTeam => !this.IsEmpty && this.TeamIndex != NoTeamIndex;

        /// <summary>has already used a redshirt season (NCAA 14's "RS" designation)</summary>
        public bool HasRedshirted => this.RedshirtStatus == "Previous";

        public int TeamId => CFBTeam.TeamIndexToOldIdMap[this.TeamIndex];
    }
}
