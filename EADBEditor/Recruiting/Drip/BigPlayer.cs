namespace EA_DB_Editor
{
    /// <summary>
    /// prototype for big players like OL, DL, LB
    /// </summary>
    public abstract class BigPlayer : StandardPlayer
    {
        private readonly int kneeBraceTreshold;
        protected BigPlayer(MaddenRecord mr, int position, int kneeBraceTreshold) : base(mr, position)
        {
            this.kneeBraceTreshold = kneeBraceTreshold;
        }

        public override void SetKnee()
        {
            // about 20 % of these guys have one at least
            if (TableUtility.Check(kneeBraceTreshold))
            {
                var knees = new int[] { NoKneeBrace, KneeBrace };
                var left = knees.Choose();
                var right = knees.Choose();
                PlayerRecord.SetKneeBraces(left, right);
            }
            else
            {
                base.SetKnee();
            }
        }

        public override void SetSplat()
        {
            if (TableUtility.Check(kneeBraceTreshold))
            {
                var knees = new int[] { NoAnkleGear, AnkleBrace };
                var left = knees.Choose();
                var right = knees.Choose();

                while ((left + right) == 0)
                {
                    left = knees.Choose();
                    right = knees.Choose();
                }

                PlayerRecord.SetSplats(left, right);
                return;
            }

            base.SetSplat();
        }
    }

    public class DBDrip : SkillPlayer
    {
        protected override int FlackJacketThreshold => 15;
        protected override int BackPlateThreshold => 90;
        public DBDrip(MaddenRecord mr, int position) : base(mr, position)
        {
        }
    }

    public class OLDrip : BigPlayer
    {
        public OLDrip(MaddenRecord mr, int position) : base(mr, position, 15)
        {
        }

        public override void SetHands()
        {
            PlayerRecord.SetHands(NoGloves, OLGloves, OLGloves, OLGloves, TapedFingersBlack, TapedFingersWhite, TapedFingersTeamColor, TapedHand, TapedHandMax, TapedHandCombo, TapedHand, TapedHandMax, TapedHandCombo);
        }

        protected override ArmSet FindArmSet() => ArmExtensions.OL.Choose();
    }

    public class DLDrip : BigPlayer
    {
        public DLDrip(MaddenRecord mr, int position) : base(mr, position, 8)
        {
        }

        public override void SetHands()
        {
            PlayerRecord.SetHands(
                NoGloves, NikeGloves, UAGloves, AdidasGloves, OLGloves, NikeGloves, UAGloves, AdidasGloves, OLGloves, TapedFingersBlack,
                TapedFingersWhite, TapedFingersTeamColor, TapedHand, TapedHandMax, TapedHandCombo, TapedHand, TapedHandMax, TapedHandCombo);
        }

        protected override ArmSet FindArmSet() => ArmExtensions.Front7.Choose();
    }

    public class LBDrip : BigPlayer
    {
        /// <summary>
        /// let's say about 20% of them also opt for backplate
        /// </summary>
        protected override int BackPlateThreshold => 25;

        public LBDrip(MaddenRecord mr, int position) : base(mr, position, 4)
        {
        }

        public override void SetHands()
        {
            PlayerRecord.SetHands(
                NoGloves, NikeGloves, UAGloves, AdidasGloves, OLGloves, NikeGloves, UAGloves, AdidasGloves, OLGloves, TapedFingersBlack,
                TapedFingersWhite, TapedFingersTeamColor, TapedHand, TapedHandMax, TapedHandCombo, TapedHand, TapedHandMax, TapedHandCombo);
        }

        protected override ArmSet FindArmSet() => ArmExtensions.Front7.Choose();
    }
}