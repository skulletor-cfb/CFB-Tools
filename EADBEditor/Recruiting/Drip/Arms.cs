using System;

namespace EA_DB_Editor
{
    public class Arms
    {
        public Arm Left { get; }
        public Arm Right { get; }

        private Arms(ArmSet armSet)
        {
            var (left, right) = armSet.Create();
            this.Left = left;
            this.Right = right;
        }

        public static Arms Create(ArmSet armSet, bool leftHanded, int position)
        {
            var arms = new Arms(armSet);
            arms.AssignQbWrist(leftHanded, position);
            arms.AssignSkillWrist(position);
            arms.AssignBigWrist(position);
            arms.AssignSweatbands();
            return arms;
        }

        public void AssignSweatbands()
        {
            // get the gear we want
            var bandColor = ArmExtensions.PickColor(white: 35, black: 20, color: 45);
            var sweatBand = new[] { ElbowGear.SweatbandFullWhite, ElbowGear.SweatbandThinWhite, ElbowGear.SweatbandMediumWhite }.Choose() + (bandColor * 3);

            // 90 % of the time it's both
            if (TableUtility.Check(90))
            {
                this.Left.CheckElbow(sweatBand);
                this.Right.CheckElbow(sweatBand);
            }
            else
            {
                //otherwise its a 50/50 split, but not qutie
                if (TableUtility.Check(50))
                {
                    this.Right.CheckElbow(sweatBand);
                }

                if (TableUtility.Check(50))
                {
                    this.Left.CheckElbow(sweatBand);
                }
            }
        }

        public void AssignBigWrist(int position)
        {
            // filter out non skills
            switch (position)
            {
                case 13:
                case 14:
                case 15:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    break;

                default:
                    return;
            }

            // get the gear we want
            var wristColor = ArmExtensions.PickColor(white: 35, black: 35, color: 30);

            var gearType = TableUtility.Check(
                new[] { 25, 15, 3, 5, 5, 5, 10, 10, 10, 10, 2 },
                NoWristGear, WhiteWristband, DblWhiteWristBand, TapedWristLite, TapedWristNormal, TapedWristMax, TapedGloveHeavyBlack, TapedGloveHeavyWhite, TapedGloveNormalBlack, TapedGloveNormalWhite, WristBrace);

            var applyNoMatterWhat = true;

            if (gearType == WhiteWristband || gearType == DblWhiteWristBand)
            {
                gearType += wristColor;
                applyNoMatterWhat = false;
            }

            if (gearType != WristBrace)
            {
                Left.CheckWristBand(gearType, applyNoMatterWhat);
                Right.CheckWristBand(gearType, applyNoMatterWhat);
            }
            else
            {
                // wrist brace only goes one
                var wrist = TableUtility.Check(50) ? Left : Right;
                wrist.CheckWristBand(gearType, true);
            }
        }

        public void AssignSkillWrist(int position)
        {
            // filter out non skills
            switch (position)
            {
                case 0:
                case 13:
                case 14:
                case 15:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    return;

                default:
                    break;
            }

            // get the gear we want
            var wristColor = ArmExtensions.PickColor(white: 40, black: 20, color: 40);
            var gearType = TableUtility.Check(new[] { 40, 40, 3, 9, 8 }, NoWristGear, WhiteWristband, DblWhiteWristBand, TapedWristLite, TapedWristNormal);
            var applyNoMatterWhat = true;

            if (gearType == WhiteWristband || gearType == DblWhiteWristBand)
            {
                gearType += wristColor;
                applyNoMatterWhat = false;
            }

            Left.CheckWristBand(gearType, applyNoMatterWhat);
            Right.CheckWristBand(gearType, applyNoMatterWhat);
        }

        public void AssignQbWrist(bool leftHanded, int position)
        {
            // qb gets special treatment
            if (position != 0)
                return;

            var offHand = leftHanded ? this.Right : this.Left;
            var throwingHand = leftHanded ? this.Left : this.Right;

            // white, team, black
            var colorMod = ArmExtensions.PickColor();

            // all qbs wear a qb wristband, colors match
            offHand.Wrist = WhiteQBWristBand + colorMod;
            var throwingWrist = TableUtility.Check(new[] { 40, 50, 10 }, NoWristGear, WhiteWristband, DblWhiteWristBand);
            throwingWrist += throwingWrist == 0 ? 0 : colorMod;
            throwingHand.Wrist = throwingWrist;
        }

        #region constants
        public const int NoWristGear = 0; // NONE
        public const int WhiteWristband = 1; // WRISTBAND WHITE
        public const int BlackWristBand = 2; // WRISTBAND BLACK
        public const int TeamColorWristBand = 3; // WRISTBAND TEAM COLOR
        public const int DblWhiteWristBand = 4; // DBL WRISTBAND WHITE
        public const int DblBlackWristBand = 5; // DBL WRISTBAND BLACK
        public const int DblTeanWristBand = 6; // DBL WRISTBAND TEAM COLOR
        public const int WhiteQBWristBand = 7; // QB WRISTBAND WHITE
        public const int BlackQBWristBand = 8; // QB WRISTBAND BLACK
        public const int TeamColorQBWristBand = 9; // QB WRISTBAND TEAM COLOR
        public const int TapedWristLite = 10; // TAPED WRIST LITE
        public const int TapedWristNormal = 11; // TAPED WRIST NORMAL
        public const int TapedWristMax = 12; // TAPED WRIST MAX
        public const int TapedGloveHeavyWhite = 13; // TAPED GLOVE HEAVY WHITE
        public const int TapedGloveNormalWhite = 14; // TAPED GLOVE NORMAL WHITE
        public const int TapedGloveHeavyBlack = 15; // TAPED GLOVE HEAVY BLACK
        public const int TapedGloveNormalBlack = 16; // TAPED GLOVE NORMAL BLACK
        public const int WristBrace = 17; // BRACE
        #endregion
    }

    public class Arm
    {

        public Sleeve Sleeve { get; private set; }
        public int Elbow => (int)(this.elbowGear ?? ElbowGear.None);
        public int Wrist { get; set; }
        public int Bicep { get; set; }
        public int Forearm { get; set; }
        public ElbowGear? elbowGear { get; set; }

        private Arm()
        {
        }

        public void CheckElbow(ElbowGear value)
        {
            // assigned some other way
            if(this.elbowGear.HasValue)
            {
                return;
            }

            if (!this.Sleeve.Match(SleeveKind.LongSleeve, SleeveKind.BottomSleeve, SleeveKind.TopSleeve))
            {
                this.elbowGear = value;
            }
        }

        public static Arm Create(Sleeve sleeve, ElbowGear? elbowGear)
        {
            var arm = new Arm()
            {
                Sleeve = sleeve,
                elbowGear = elbowGear,
            };

            return arm;
        }

        public void CheckWristBand(int value, bool applyNoMatterWhat)
        {
            // for wristbands, we need to check that they fit the sleeve
            if (applyNoMatterWhat ||
                !this.Sleeve.Match(SleeveKind.LongSleeve, SleeveKind.BottomSleeve))
            {
                this.Wrist = value;
            }
        }
    }

    public class ArmSet
    {
        private readonly SleeveKind leftSleeveKind;
        private readonly SleeveKind rightSleeveKind;

        private readonly int team;
        private readonly int white;
        private readonly int black;
        private readonly ElbowGear? leftElbow;
        private readonly ElbowGear? rightElbow;

        private readonly Func<(ElbowGear lElbow, ElbowGear rElbow)> elbowFunc;

        public ArmSet(
            SleeveKind left,
            SleeveKind? right = null,
            int teamColorPct = 50,
            int whitePct = 35,
            int blackPct = 15,
            ElbowGear? leftElbow = null,
            ElbowGear? rightElbow = null,
            Func<(ElbowGear lElbow, ElbowGear rElbow)> elbowFunc = null)
        {
            this.leftSleeveKind = left;
            this.rightSleeveKind = right ?? left;
            this.team = teamColorPct;
            this.white = whitePct;
            this.black = blackPct;
            this.leftElbow = leftElbow;
            this.rightElbow = rightElbow;
            this.elbowFunc = elbowFunc;
        }

        public (Arm left, Arm right) Create()
        {
            var color = TableUtility.Check(
                new int[] { team, white, black },
                ColorKind.TeamColor, ColorKind.White, ColorKind.Black
                );

            var left = leftSleeveKind.CreateSleeve(color);
            var right = rightSleeveKind.CreateSleeve(color);

            var leftElbowToSubmit = leftElbow.MatchGear(color);
            var rightElbowToSubmit = rightElbow.MatchGear(color);

            if (this.elbowFunc != null)
            {
                (leftElbowToSubmit, rightElbowToSubmit) = elbowFunc();
            }

            return (Arm.Create(left, leftElbowToSubmit), Arm.Create(right, rightElbowToSubmit));
        }
    }
}