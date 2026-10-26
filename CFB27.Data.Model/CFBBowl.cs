using MaddenFranchise;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CFB27.Data.Model
{
    public class CFBBowl : BaseRecord
    {
        public static CFBBowl Transform(FranchiseFile dynasty, FranchiseFileRecord record)
        {
            var bowl = dynasty.DefaultTransform<CFBBowl>(record);

            if (!string.Equals(bowl.Conference1, NoRefString))
            {
                var conf1 = dynasty.Follow(record.GetReferenceDataByKey("Conference1").Value);
                bowl.ConferenceId1 = CFBConference.ToClassicId(conf1);
            }

            if (!string.Equals(bowl.Conference2, NoRefString))
            {
                var conf2 = dynasty.Follow(record.GetReferenceDataByKey("Conference2").Value);
                bowl.ConferenceId2 = CFBConference.ToClassicId(conf2);
            }

            return bowl;
        }

        /// <summary>classic conference id of the tie-in (see CFBConference.ClassicIds), -1 for none</summary>
        [JsonIgnore]
        public int ConferenceId1 { get; private set; } = -1;
        [JsonIgnore]
        public int ConferenceId2 { get; private set; } = -1;
        public string Stadium { get; set; }
        public string RelativeAppt { get; set; }
        public string Conference2 { get; set; }
        public string Conference1 { get; set; }
        public string Trophy { get; set; }
        public string Name { get; set; }
        public string AssetName { get; set; }
        public int BOWL_SECONDARY_COLOR_R { get; set; }
        public int BOWL_TERTIARY_COLOR_B { get; set; }
        public int BOWL_TERTIARY_COLOR_G { get; set; }
        public int BOWL_TERTIARY_COLOR_R { get; set; }
        public int BOWL_PRIMARY_COLOR_G { get; set; }
        public int BOWL_PRIMARY_COLOR_R { get; set; }
        public int BOWL_SECONDARY_COLOR_B { get; set; }
        public int BOWL_SECONDARY_COLOR_G { get; set; }
        public bool IsPlayoffBowl { get; set; }
        public bool NoSatNoSun { get; set; }
        public bool ShouldPlayNewYears { get; set; }
        public int Conference1Rank { get; set; }
        public int Conference2Rank { get; set; }
        public int PlayoffBracketSlot { get; set; }
        public int BOWL_PRIMARY_COLOR_B { get; set; }
        public bool CanPlayXmas { get; set; }
        public int DaysOffset { get; set; }
        public string GameTime { get; set; }
        public int BowlLogoId { get; set; }
        public int PresentationId { get; set; }

        [JsonIgnore]
        public long StadiumId { get; set; }

        [JsonIgnore]
        public int BowlId
        {
            get
            {
                return LookupBowlId(this.Row, this.StadiumId);
            }
        }

        /// <summary>
        /// The NCAA 14 bowl id for a BowlGame row; the rows that rotate between the New Year's Six (CFP quarterfinals and
        /// semifinals, 12-17) aren't in the map, so they go by the stadium the game is played in.
        /// </summary>
        public static int LookupBowlId(long id, long? bowlGameStadium = null)
        {
            if (NewBowlIdToOldBowlIdMap.TryGetValue(id, out var classicId) ||
                (bowlGameStadium.HasValue && NewBowlIdToOldBowlIdMap.TryGetValue(bowlGameStadium.Value, out classicId)))
            {
                return classicId;
            }

            // playoff bowl
            return -1;
        }

        /// <summary>
        /// The bowl's name; a CFP quarterfinal or semifinal row ("CFP Quarterfinal") goes by the New Year's Six bowl hosting
        /// the game, from its stadium, and a first-round game by its seeds as in NCAA 14 ("CFP 1st Round 5v12").
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                if (this.IsPlayoffBowl && this.PlayoffBracketSlot < FirstQuarterfinalSlot)
                {
                    return $"CFP 1st Round {this.Conference1Rank + 1}v{this.Conference2Rank + 1}";
                }

                return !NewBowlIdToOldBowlIdMap.ContainsKey(this.Row) && NewYearsSixBowlNames.TryGetValue(this.StadiumId, out var host) ? host : this.Name;
            }
        }

        /// <summary>PlayoffBracketSlot 0-3 are the first round; Conference1Rank/2Rank hold their 0-based seeds.</summary>
        private const int FirstQuarterfinalSlot = 4;

        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>The New Year's Six bowls by stadium (as in NewBowlIdToOldBowlIdMap).</summary>
        public static Dictionary<long, string> NewYearsSixBowlNames = new Dictionary<long, string>
        {
            { 0x806385CF, "Sugar Bowl" },
            { 0x80654CDE, "Rose Bowl" },
            { 0x80638637, "Peach Bowl" },
            { 0x80638607, "Orange Bowl" },
            { 0x80639DE7, "Fiesta Bowl" },
            { 0x806385D0, "Cotton Bowl" },
        };

        /// <summary>
        /// bowls 12-17 are dynamic based on playoff rotation.  
        /// </summary>
        public static Dictionary<long, int> NewBowlIdToOldBowlIdMap = new Dictionary<long, int>
        {
            { 0, 0 }, // 68 ventures bowl
            { 1, 14 }, // alamo bowl
            { 2, 987045 },// arizona bowl
            { 3, 2 }, // Armed Forces Bowl
            { 4, 21 }, // Birmingham Bowl
            { 5, 9000 }, // Boca Raton Bowl
            { 6, 20 }, // Citrus Bowl
            { 7, 987050 }, // CFP First Round 5v12
            { 8, 987049 }, // CFP First Round 6v11
            { 9, 987048 }, // CFP First Round 7v10
            { 10, 987047 }, // CFP First Round 8v9
            { 11, 39 }, // National Championship
            { 18, 987043 }, // Cure Bowl
            { 19, 31 }, // Duke's Mayo Bowl
            { 20, 999999 }, // none
            { 21, 6 }, // Famous Idaho Potato Bowl
            { 22, 987100 }, // Fenway Bowl
            { 23, 32 }, // First Responder Bowl
            { 24, 987101 }, // Frisco Bowl
            { 25, 23 }, // Gasparilla Bowl
            { 26, 19 }, // Gator Bowl
            { 27, 999998 }, // na
            { 28, 30 }, // Hawaii Bowl
            { 29, 13 }, // Holiday Bowl
            { 30, 15 }, // Independence Bowl
            { 31, 1 }, // Las Vegas Bowl
            { 32, 10 }, // Liberty Bowl
            { 33, 24 }, // Military Bowl
            { 34, 9 }, // Music City Bowl
            { 35, 987044 }, // Myrtle Beach Bowl
            { 36, 22 }, // New Mexico Bowl
            { 37, 29 }, // New Orleans Bowl
            { 38, 8 }, // Pop-Tarts Bowl
            { 39, 7 }, // Rate Bowl
            { 40, 18 }, // Reliaquest Bowl
            { 41, 987051 }, // Salute to Veterans Bowl
            { 42, 11 }, // Sun Bowl
            { 43, 5 }, // Texas Bowl
            { 44, 987052 }, // Xbox Bowl
            { 0x806385CF, 27 }, // sugar bowl
            { 0x80654CDE, 25 }, // rose bowl
            { 0x80638637, 12 }, // peach bowl
            { 0x80638607, 28 }, // orange bowl
            { 0x80639DE7, 26 }, // fiesta bowl
            { 0x806385D0, 17 }, // cotton bowl
        };
    }
}