using System;
using System.Collections.Generic;
using System.Linq;

namespace EA_DB_Editor
{
    public enum ElbowGear : int
    {
        None = 0,
        ElbowPadWhite = 1,
        ElbowPadBlack = 2,
        ElbowPadTeamColor = 3,
        ElbowPadWhiteStripe = 4,
        ElbowPadBlackStripe = 5,
        RubberPad = 7,
        SweatbandFullWhite = 8,
        SweatbandMediumWhite = 9,
        SweatbandThinWhite = 10,
        SweatbandFullBlack = 11,
        SweatbandMediumBlack = 12,
        SweatbandThinBlack = 13,
        SweatbandFullTeamColor = 14,
        SweatbandMediumTeamColor = 15,
        SweatbandThinTeamColor = 16,
        Brace = 17,
        Gaurd = 18,
    }

    public static class ArmExtensions
    {
        public static readonly ArmSet ShortSleeves = new ArmSet(SleeveKind.ShortSleeve);
        public static readonly ArmSet LongSleeve = new ArmSet(SleeveKind.LongSleeve);
        public static readonly ArmSet TopSleeve = new ArmSet(SleeveKind.TopSleeve);
        public static readonly ArmSet BottomSleeve = new ArmSet(SleeveKind.BottomSleeve);
        public static readonly ArmSet ThreeQtrSleeve = new ArmSet(SleeveKind.ThreeQtrSleeve);

        public static (ElbowGear lElbow, ElbowGear rElbow) ChooseElbowPads()
        {
            var elbowPad = new[] { ElbowGear.ElbowPadBlack, ElbowGear.ElbowPadBlackStripe, ElbowGear.ElbowPadWhite, ElbowGear.ElbowPadWhiteStripe, ElbowGear.ElbowPadTeamColor }.Choose();
            return TableUtility.Check(
                new[] { 34, 33, 33 },
                (elbowPad, elbowPad), (elbowPad, ElbowGear.None), (ElbowGear.None, elbowPad)
                );
        }

        /// <summary>
        ///  short sleeves with elbow pads
        /// </summary>
        public static readonly ArmSet ElbowPadShortSleeve = new ArmSet(SleeveKind.ShortSleeve, elbowFunc: ChooseElbowPads);

        /// <summary>
        ///  short sleeves with elbow pads
        /// </summary>
        public static readonly ArmSet ElbowPadThreeQtr = new ArmSet(SleeveKind.ThreeQtrSleeve, elbowFunc: ChooseElbowPads);

        /// <summary>
        /// short sleeves and rubber pads
        /// </summary>
        public static readonly ArmSet RubberPadShort = new ArmSet(SleeveKind.ShortSleeve, leftElbow: ElbowGear.RubberPad, rightElbow: ElbowGear.RubberPad);

        /// <summary>
        /// short sleeves and rubber pads
        /// </summary>
        public static readonly ArmSet RubberThreeQtr = new ArmSet(SleeveKind.ThreeQtrSleeve, leftElbow: ElbowGear.RubberPad, rightElbow: ElbowGear.RubberPad);


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
        public static readonly ArmSet OneSleeve = new ArmSet(SleeveKind.ShortSleeve, SleeveKind.BottomSleeve);


        /// <summary>
        /// one bottom, one short, but we define the gear on the off arm
        /// </summary>
        public static readonly ArmSet ScudMissile = new ArmSet(SleeveKind.ShortSleeve, SleeveKind.BottomSleeve, leftElbow: ElbowGear.SweatbandFullWhite);

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
        public static readonly ArmSet ReverseScudMissile = new ArmSet(SleeveKind.BottomSleeve, SleeveKind.ShortSleeve, rightElbow: ElbowGear.SweatbandFullWhite);

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
        /// one bottom, one short
        /// </summary>
        public static readonly ArmSet ReverseOneSleeve = new ArmSet(SleeveKind.ShortSleeve, SleeveKind.BottomSleeve);


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

        public static readonly ArmSet[] Skill = new ArmSet[]
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
            ReverseOneSleeve,
            OneSleeve,
            RubberThreeQtr,
            RubberPadShort,
        };

        public static readonly ArmSet[] OL = new ArmSet[]
        {
            ShortSleeves,
            LongSleeve,
            TopSleeve,
            BottomSleeve,
            ThreeQtrSleeve,
            RubberThreeQtr,
            RubberPadShort,
            ElbowPadShortSleeve,
            ElbowPadThreeQtr,
        };

        public static readonly ArmSet[] Front7 = Skill.Concat(OL).Distinct().ToArray();

        public static Sleeve CreateSleeve(this SleeveKind kind, ColorKind color)
        {
            return SleeveCreators[kind](color);
        }

        public static ElbowGear MatchGear(this ElbowGear? gear, ColorKind color)
        {
            var elbowGear = gear ?? ElbowGear.None;

            if (elbowGear < ElbowGear.SweatbandFullWhite || elbowGear > ElbowGear.SweatbandThinTeamColor)
            {
                return elbowGear;
            }

            if (elbowGear <= ElbowGear.SweatbandThinWhite)
            {
                elbowGear += (3 * ((int)color));
            }
            else if (elbowGear <= ElbowGear.SweatbandThinBlack)
            {
                elbowGear += (3 * (((int)color) - 1));
            }
            else
            {
                elbowGear += (3 * (((int)color) - 2));
            }

            return elbowGear;
        }
    }
}