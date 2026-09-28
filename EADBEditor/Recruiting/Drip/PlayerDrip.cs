using System;

namespace EA_DB_Editor
{

    public abstract class PlayerDrip
    {
        #region constants
        public const int NoGloves = 0; // NONE
        public const int NikeGloves = 3; // NIKE
        public const int UAGloves = 4; // UNDER ARMOR
        public const int GenericGloves = 7; // GENERIC
        public const int AdidasGloves = 10; // ADIDAS
        public const int OLGloves = 13; // OL
        public const int TapedFingersWhite = 15; // TAPED FINGERS WHITE
        public const int TapedFingersBlack = 16; // TAPED FINGERS BLACK
        public const int TapedFingersTeamColor = 17; // TAPED FINGERS TEAM COLOR
        public const int TapedHand = 18; // TAPED HAND
        public const int TapedHandMax = 19; // TAPED HAND MAX
        public const int TapedHandCombo = 20; // TAPED HAND COMBO(FINGERS / HAND)

        public const int NoKneeBrace = 1;
        public const int KneeBrace = 2;
        public const int RightHanded = 0;
        public const int LeftHanded = 1;


        public const int NoAnkleGear = 0; //  NONE
        public const int WhiteSplat = 1; //  WHITE THIN
        public const int WhiteSplatLong = 2; //  WHITE BULKY
        public const int BlackSplat = 3; //  BLACK THIN
        public const int BlackSplatLong = 4; //  BLACK BULKY
        public const int AnkleBrace = 5; //  BRACE


        #endregion
        protected MaddenRecord PlayerRecord { get; }

        public int Position { get; }

        public bool IsLeftHanded { get; }

        public int RecruitId { get; }

        protected virtual int FlackJacketThreshold => 0;

        protected virtual int BackPlateThreshold => 0;

        protected Arms Arms { get; private set; }

        protected PlayerDrip(MaddenRecord mr, int position)
        {
            this.PlayerRecord = mr;
            this.Position = position;
            this.IsLeftHanded = mr.PlayerHandedness() == LeftHanded;
            this.RecruitId = mr.RecruitId();
        }

        protected abstract ArmSet FindArmSet();

        public void DripHimOut()
        {
            PlayerRecord.SetShoulderPadsForSkillPosition(Position);
            //PlayerRecord.SetVisor();
            PlayerRecord.SetJerseySleeves();
            //PlayerRecord.SetShoes();
            PlayerRecord.SetFlakJacket(this.FlackJacketThreshold);
            PlayerRecord.SetBackPlate(this.BackPlateThreshold);
            //this.SetHands();
            this.SetKnee();
            this.SetArms();
            this.SetSplat();
        }

        public void SetArms()
        {
            var armSet = FindArmSet();
            this.Arms = Arms.Create(armSet, IsLeftHanded, Position);
            PlayerRecord.SetSleeves(this.Arms);
            PlayerRecord.SetWrists(this.Arms);
        }

        /// <summary>
        /// by default you get no gloves
        /// </summary>
        public virtual void SetHands()
        {
            PlayerRecord.SetHands(NoGloves);
        }

        public virtual void SetKnee()
        {
            PlayerRecord.SetKneeBraces(NoKneeBrace, NoKneeBrace);
        }

        public virtual void SetSplat()
        {
            PlayerRecord.SetSplats(NoAnkleGear, NoAnkleGear);
        }
    }


    public class StandardPlayer : PlayerDrip
    {
        public StandardPlayer(MaddenRecord mr, int position) : base(mr, position)
        {
        }

        protected override ArmSet FindArmSet()
        {
            return TableUtility.Choose(ArmExtensions.All);
        }
    }
}