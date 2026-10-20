namespace EA_DB_Editor
{
    public static class RecruitExtensions
    {
        /// <summary>
        /// reads a property and returns int value
        /// </summary>
        /// <param name="mr"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static int ReadProperty(this MaddenRecord mr, string field) => mr[field].ToInt32();

        /// <summary>
        /// 0 right
        /// 1 left
        /// </summary>
        /// <param name="mr"></param>
        /// <returns></returns>
        public static int PlayerHandedness(this MaddenRecord mr) => mr.ReadProperty("PHAN");

        public static void Update(this MaddenRecord mr, string field, int value) => mr[field] = value.ToString();

        public static bool IsSkillPlayer(this int position)
        {
            switch (position)
            {
                case 0: // qb
                case 1:
                case 3:
                case 4:
                case 16:
                case 18:
                case 17:
                    return true;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 2:
                default:
                    return false;
            }
        }

        public static void SetShoulderPadsForSkillPosition(this MaddenRecord mr, int position)
        {
            if (position.IsSkillPlayer())
            {
                mr.Update("BSPA", 4); // definition
                mr.Update("BSPT", 5); // size
            }
        }

        /// <summary>
        /// about 8% of players wear visors, that works out to 5-6 a team
        /// </summary>
        /// <param name="mr"></param>
        public static void SetVisor(this MaddenRecord mr)
        {
            if (TableUtility.Check(8))
            {
                mr.Update("PVIS", Visors.Choose());
            }
        }

        public static void SetJerseySleeves(this MaddenRecord mr)
        {
            // tight is the usual pick
            var jersey = TableUtility.Check(92) ? 0 : 1;
            mr.Update("PJER", jersey);
        }

        public static void SetShoes(this MaddenRecord mr)
        {
            var rand = TableUtility.Rand100();
            var shoe = 3; // low cut shoe

            if (rand < 5)
            {
                shoe = 2; // generic
            }
            else if (rand < 40)
            {
                shoe = 4; //mid cut
            }

            mr.Update("PLSH", shoe);
            mr.Update("PRSH", shoe);
        }

        public static void SetFlakJacket(this MaddenRecord mr, int threshold)
        {
            var value = TableUtility.Check(threshold) ? 1 : 0;
            mr.Update("PFLK", value);
        }
        public static void SetBackPlate(this MaddenRecord mr, int threshold)
        {
            var value = TableUtility.Check(threshold) ? 1 : 0;
            mr.Update("PBAK", value);
        }

        public static void SetHands(this MaddenRecord mr, params int[] choices)
        {
            var value = choices.Choose();
            mr.SetLeftHand(value);
            mr.SetRightHand(value);
        }

        public static void SetKneeBraces(this MaddenRecord mr, int left, int right)
        {
            mr.Update("PLKN", left);
            mr.Update("PRKN", right);
        }

        public static void SetSplats(this MaddenRecord mr, int left, int right)
        {
            mr.Update("PSPL", left);
            mr.Update("PSPR", right);
        }

        public static void SetLeftHand(this MaddenRecord mr, int value) => mr.Update("PLHA", value);
        public static void SetRightHand(this MaddenRecord mr, int value) => mr.Update("PRHA", value);

        public static void SetArms(this MaddenRecord mr, Arms arms)
        {
            // sleeve
            mr.Update("PLSL", arms.Left.Sleeve.Value);
            mr.Update("PLSR", arms.Right.Sleeve.Value);

            // wrist
            mr.Update("PLWR", arms.Left.Wrist);
            mr.Update("PRWR", arms.Right.Wrist);

            // elbow
            mr.Update("PLEL", arms.Left.Elbow);
            mr.Update("PREL", arms.Right.Elbow);
        }


        #region gear and helpers
        private static int[] Visors = new int[]
        {
            0 , //  none
            1 , //  nike
            2 , //  under armor
            3 , //  Oakley
            4 , //  dark nike
            5 , //  dark UA
            6 , //  dark oakley
        };

        public static PlayerDrip Create(this MaddenRecord mr)
        {
            var position = mr.Position();

            switch (position)
            {
                case 0:
                    return new QBDrip(mr, position);

                case 1:
                    return new HBDrip(mr, position);

                case 2:
                    return new FBDrip(mr, position);

                case 3:
                    return new WRDrip(mr, position);

                case 4:
                    return new TEDrip(mr, position);

                case 13:
                case 14:
                case 15:
                    return new LBDrip(mr, position);

                case 16:
                case 17:
                case 18:
                    return new DBDrip(mr, position);

                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return new OLDrip(mr, position); ;

                case 10:
                case 11:
                case 12:
                    return new DLDrip(mr, position);

                default:
                    return new StandardPlayer(mr, position);
            }
        }
        #endregion
    }
}
