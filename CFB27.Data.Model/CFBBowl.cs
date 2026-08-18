using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CFB27.Data.Model
{
    public class CFBBowl : BaseRecord
    {
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
                if (this.Row >= 12 && this.Row <= 17)
                {
                    return LookupBowlId(this.StadiumId);
                }
         
                return LookupBowlId(this.Row);
            }
        }

        public static int LookupBowlId(long id)
        {
            if(NewBowlIdToOldBowlIdMap.TryGetValue(id, out var classicId)   )
            {
                return classicId;
            }

            // playoff bowl
            return -1;
        }

        /// <summary>
        /// bowls 12-17 are dynamic based on playoff rotation.  
        /// </summary>
        public static Dictionary<long,int> NewBowlIdToOldBowlIdMap = new Dictionary<long, int>
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
            { 20, 31 }, // none
            { 21, 6 }, // Famous Idaho Potato Bowl
            { 22, 987100 }, // Fenway Bowl
            { 23, 32 }, // First Responder Bowl
            { 24, 987101 }, // Frisco Bowl
            { 25, 23 }, // Gasparilla Bowl
            { 26, 19 }, // Gator Bowl
            { 27, 31 }, // na
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