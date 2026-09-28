namespace EA_DB_Editor
{
    public class SkillPlayer : StandardPlayer
    {
        public SkillPlayer(MaddenRecord mr, int position) : base(mr, position)
        {
        }

        public override void SetHands()
        {
            // hands
            if (TableUtility.Check(5))
            {
                PlayerRecord.SetHands(GenericGloves);
            }
            else
            {
                PlayerRecord.SetHands(NikeGloves, UAGloves, AdidasGloves);
            }
        }

        public override void SetSplat()
        {
            var splat = NoAnkleGear;

            // about a third are splatting
            if (TableUtility.Check(35))
            {
                // a fourth of the splats are bulky
                var bulkyMod = TableUtility.Check(25) ? 1 : 0;

                // most are using white splats
                if (TableUtility.Check(65))
                {
                    splat = WhiteSplat + bulkyMod;
                }
                else
                {
                    splat = BlackSplat + bulkyMod;
                }
            }

            // always symmetrical
            PlayerRecord.SetSplats(splat, splat);
        }
    }

    public class TEDrip : SkillPlayer
    {
        protected override int FlackJacketThreshold => 25;
        protected override int BackPlateThreshold => 25;
        public TEDrip(MaddenRecord mr, int position) : base(mr, position)
        {
        }
    }


    public class WRDrip : SkillPlayer
    {
        protected override int FlackJacketThreshold => 50;
        protected override int BackPlateThreshold => 85
            ;
        public WRDrip(MaddenRecord mr, int position) : base(mr, position)
        {
        }
    }

    public class HBDrip : SkillPlayer
    {
        protected override int FlackJacketThreshold => 75;
        protected override int BackPlateThreshold => 20;
        public HBDrip(MaddenRecord mr, int position) : base(mr, position)
        {
        }
    }
    public class FBDrip : SkillPlayer
    {
        protected override int FlackJacketThreshold => 50;
        protected override int BackPlateThreshold => 10;
        public FBDrip(MaddenRecord mr, int position) : base(mr, position)
        {
        }
    }

    public class QBDrip : SkillPlayer
    {
        // 90% of QBs wear one
        protected override int FlackJacketThreshold => 90;

        /// <summary>
        /// let's say about 20% of them also opt for backplate
        /// </summary>
        protected override int BackPlateThreshold => 20;

        public QBDrip(MaddenRecord mr, int position) : base(mr, position)
        {
        }

        public override void SetKnee()
        {
            // small percentage has a brace for stability
            if (TableUtility.Check(3))
            {
                var left = NoKneeBrace;
                var right = NoKneeBrace;

                if (IsLeftHanded)
                {
                    left = KneeBrace;
                }
                else
                {
                    right = KneeBrace;
                }

                PlayerRecord.SetKneeBraces(left, right);
            }
            else
            {
                base.SetKnee();
            }
        }

        public override void SetHands()
        {
            // some qbs like to wear 1 glove
            if (TableUtility.Check(8))
            {
                var gloves = new[] { NikeGloves, UAGloves, AdidasGloves }.Choose();

                if (IsLeftHanded)
                {
                    PlayerRecord.SetRightHand(gloves);
                    PlayerRecord.SetLeftHand(NoGloves);
                }
                else
                {
                    PlayerRecord.SetRightHand(NoGloves);
                    PlayerRecord.SetLeftHand(gloves);
                }
            }
            else
            {
                base.SetHands();
            }
        }
    }
}