using System;
using System.Collections.Generic;

namespace EA_DB_Editor
{
    public static class ArmExtensions
    {
        public static readonly ArmSet ShortSleeves = new ArmSet(SleeveKind.ShortSleeve);
        public static readonly ArmSet LongSleeve = new ArmSet(SleeveKind.LongSleeve);
        public static readonly ArmSet TopSleeve = new ArmSet(SleeveKind.TopSleeve);
        public static readonly ArmSet BottomSleeve = new ArmSet(SleeveKind.BottomSleeve);
        public static readonly ArmSet ThreeQtrSleeve = new ArmSet(SleeveKind.ThreeQtrSleeve);

        /// <summary>
        /// one top, one bottom - it's like all you had was a single sleeve to cut into two
        /// </summary>
        public static readonly ArmSet SingleSleeve = new ArmSet(SleeveKind.TopSleeve, SleeveKind.BottomSleeve);

        /// <summary>
        /// long sleeve, top sleeve
        /// </summary>
        public static readonly ArmSet WarmShoulder = new ArmSet(SleeveKind.LongSleeve, SleeveKind.TopSleeve);

        /// <summary>
        /// long sleeve, bottom sleeve
        /// </summary>
        public static readonly ArmSet ColdShoulder = new ArmSet(SleeveKind.LongSleeve, SleeveKind.BottomSleeve);

        /// <summary>
        /// one long, one short
        /// </summary>
        public static readonly ArmSet Shooter = new ArmSet(SleeveKind.ShortSleeve, SleeveKind.LongSleeve);

        /// <summary>
        /// one short, one top
        /// </summary>
        public static readonly ArmSet Sailor = new ArmSet(SleeveKind.ShortSleeve, SleeveKind.TopSleeve);

        /// <summary>
        /// one bottom, one short
        /// </summary>
        public static readonly ArmSet ScudMissile = new ArmSet(SleeveKind.ShortSleeve, SleeveKind.BottomSleeve);

        /// <summary>
        /// long sleeve, top sleeve
        /// </summary>
        public static readonly ArmSet ReverseWarmShoulder = new ArmSet(SleeveKind.TopSleeve, SleeveKind.LongSleeve);

        /// <summary>
        /// one short, one top
        /// </summary>
        public static readonly ArmSet ReverseSailor = new ArmSet(SleeveKind.TopSleeve, SleeveKind.ShortSleeve);
        /// <summary>
        /// one bottom, one short
        /// </summary>
        public static readonly ArmSet ReverseScudMissile = new ArmSet(SleeveKind.BottomSleeve, SleeveKind.ShortSleeve);

        /// <summary>
        /// one long, one short
        /// </summary>
        public static readonly ArmSet ReverseShooter = new ArmSet(SleeveKind.LongSleeve, SleeveKind.ShortSleeve);

        /// <summary>
        /// long sleeve, bottom sleeve
        /// </summary>
        public static readonly ArmSet ReverseColdShoulder = new ArmSet(SleeveKind.BottomSleeve, SleeveKind.LongSleeve);

        /// <summary>
        /// one top, one bottom - it's like all you had was a single sleeve to cut into two
        /// </summary>
        public static readonly ArmSet ReverseSingleSleeve = new ArmSet(SleeveKind.BottomSleeve, SleeveKind.TopSleeve);

        /// <summary>
        /// white/black/team color percentages by 50/15/35
        /// </summary>
        /// <param name="pcts"></param>
        /// <returns></returns>
        public static int PickColor(int white = 50, int black = 15, int color = 35)
        {
            var result = TableUtility.Check(new[] { white, black, color }, ColorKind.White, ColorKind.Black, ColorKind.TeamColor);
            return (int)result;
        }

        private static Dictionary<SleeveKind, Func<ColorKind, Sleeve>> SleeveCreators = new Dictionary<SleeveKind, Func<ColorKind, Sleeve>>()
        {
            [SleeveKind.ShortSleeve] = c => new ShortSleeve(),
            [SleeveKind.LongSleeve] = c => new LongSleeve(c),
            [SleeveKind.TopSleeve] = c => new TopSleeve(c),
            [SleeveKind.BottomSleeve] = c => new BottomSleeve(c),
            [SleeveKind.ThreeQtrSleeve] = c => new ThreeQtrSleeve(c),
        };

        public static readonly ArmSet[] All = new ArmSet[]
        {
            ShortSleeves,
            LongSleeve,
            TopSleeve,
            BottomSleeve,
            ThreeQtrSleeve,
            SingleSleeve,
            WarmShoulder,
            ColdShoulder,
            Shooter,
            Sailor,
            ScudMissile,
            ReverseSingleSleeve,
            ReverseWarmShoulder,
            ReverseColdShoulder,
            ReverseShooter,
            ReverseSailor,
            ReverseScudMissile,
        };

        public static Sleeve CreateSleeve(this SleeveKind kind, ColorKind color)
        {
            return SleeveCreators[kind](color);
        }
    }
}
