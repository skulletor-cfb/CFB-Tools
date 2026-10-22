using MaddenFranchise;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFB27.Data.Model
{
    /// <summary>
    /// A Recruit row: rankings and recruiting state. The recruit's name, position, stars and ratings live on the
    /// linked Player row (<see cref="PlayerRow"/>); recruits keep TeamIndex 255 until they join a roster.
    /// </summary>
    public class CFBRecruit : BaseRecord
    {
        public static CFBRecruit Transform(FranchiseFile dynasty, FranchiseFileRecord record)
        {
            var recruit = new CFBRecruit
            {
                Row = record.Index,
                IsEmpty = record.IsEmpty,
            };

            if (record.IsEmpty)
            {
                return recruit;
            }

            var player = record.GetReferenceDataByKey("Player").Value;
            recruit.PlayerRow = player.TableId > 0 ? player.RowNumber : -1;
            recruit.NationalRank = Convert.ToInt32(record["NationalRank"]);
            recruit.PositionRank = Convert.ToInt32(record["PositionRank"]);
            recruit.StateRank = Convert.ToInt32(record["StateRank"]);
            recruit.RecruitStage = record["RecruitStage"] as string;
            recruit.Class = record["Class"] as string;
            recruit.QualityModifier = record["QualityModifier"] as string;
            recruit.AlternatePosition1 = record["AlternatePosition1"] as string;

            // ProspectTargetSchool: TeamId is a TeamIndex; strongest interest first
            recruit.TopSchoolTeamIndexes = dynasty.FollowArray(record, "TopSchoolsList")
                .Select((school, order) => new { TeamIndex = Convert.ToInt32(school["TeamId"]), Influence = Convert.ToInt32(school["TeamInfluence"]), Order = order })
                .Where(s => s.TeamIndex != CFBPlayer.NoTeamIndex)
                .OrderByDescending(s => s.Influence)
                .ThenBy(s => s.Order)
                .Select(s => s.TeamIndex)
                .ToList();

            return recruit;
        }

        public int PlayerRow { get; set; }

        /// <summary>1 is the top recruit; the empty placeholder row has 0</summary>
        public int NationalRank { get; set; }
        public int PositionRank { get; set; }
        public int StateRank { get; set; }

        /// <summary>RecruitStage enum name: Top10, Top5, Top3, Battle, Signed ...</summary>
        public string RecruitStage { get; set; }

        /// <summary>RecruitingClass enum name: HighSchool, JuniorCollege_Sophomore ...</summary>
        public string Class { get; set; }

        /// <summary>GemBust enum name: NORMAL, GEM or BUST</summary>
        public string QualityModifier { get; set; }

        /// <summary>DraftPositionE name of a second position (SS, WR, CB ...), Invalid_ for none</summary>
        public string AlternatePosition1 { get; set; }

        public List<int> TopSchoolTeamIndexes { get; set; } = new List<int>();

        public bool IsRanked => !this.IsEmpty && this.NationalRank > 0 && this.PlayerRow >= 0;

        public bool HasAlternatePosition => !string.IsNullOrEmpty(this.AlternatePosition1) && this.AlternatePosition1 != "Invalid_";
    }
}
