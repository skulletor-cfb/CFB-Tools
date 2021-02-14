using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EA_DB_Editor
{
    using System.Runtime.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Text;

    namespace CAPGen
    {
        #region CAP GEn for getting a player
        public class CAPGen
        {
            public static Player GetRandomPlayer()
            {
                Array values = Enum.GetValues(typeof(ProfileId));
                ProfileId curr = (ProfileId)values.GetValue(CAPGen.RAND.Next(0, values.Length));
                var player = CAPGen.GeneratePlayer(curr);

                if (player == null)
                    return null;

                player.CreatePlayer();
                return player;
            }
            public static Player GetPlayer(ProfileId position)
            {
                var player = CAPGen.GeneratePlayer(position);
                player.CreatePlayer();
                return player;
            }
            public static Player GeneratePlayer(int position, int tendency)
            {
                return GeneratePlayer(GetProfileId(position, tendency));
            }

            public static int QBCounter = 0; 

            public static Player GeneratePlayer(ProfileId Id)
            {
                switch (Id)
                {
                    case ProfileId.QB_Scrambling:
                    case ProfileId.QB_PocketPasser:
                    case ProfileId.QB_Balanced:
                        return QB(Id);
                    case ProfileId.HB_Balanced:
                    case ProfileId.HB_Power:
                    case ProfileId.HB_Speed:
                        return HB(Id);
                    case ProfileId.WR_Balanced:
                    case ProfileId.WR_Possession:
                    case ProfileId.WR_Speed:
                        return WR(Id);
                    /*case ProfileId.TE_Blocking:
                    case ProfileId.TE_Recieving:
                    case ProfileId.TE_Balanced:*/
                    case ProfileId.TE:
                        return new TE(Id);
                    case ProfileId.ROLB:
                    case ProfileId.LOLB:
                        return new OLB(Id);
                    case ProfileId.SS:
                        return new SS(Id);
                    case ProfileId.CB:
                        return new CB(Id);
                    case ProfileId.MLB:
                        return new MLB(Id);
                    case ProfileId.FS:
                        return new FS(Id);
                    case ProfileId.FB:
                        return new FB(Id);
                    case ProfileId.DE:
                        return new DE(Id);
                    case ProfileId.DT:
                        return new DT(Id);
                    case ProfileId.OL:
                        return new OL(Id);
                    default:
                        break;
                }
                return null;
            }
            public static Random RAND = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0));
            public static Player QB(ProfileId Position)
            {
                return new QB(Position);
            }
            public static Player HB(ProfileId Position)
            {
                return new HB(Position);
            }
            public static Player WR(ProfileId Position)
            {
                return new WR(Position);
            }
#if true
            public static Tuple<int, int> GetArms(int positionGroup)
            {
                /*
 * Defintion/Size
7 and 4 decent for qb
8 and 0... actually looks ok for safety/wr (but I go minimum 3 for size)
7 and 5 = decent midsized arm for 210ish lbs (nice for FS, big WR)
7 and 8 = decent RB arm
8 and 5 good for fs, big cb
7 and 15 good for TE and LB and jacked RB/FB
10 and 24 - decent for lb, rb, fb, smallish de (but I find too small for bigger OL and DT and big DE)
10 and 21 = big arms (good for DT and OL)
6 and 21 looks really big, like it







1 - the typical skinny arm
4 - cut with a little mass
5 - cut with a liitle more mass than 4
15 - super jacked...i mean super jacked...doesnt look right either
21 - big but looks real good on big defensive or offensive linemen
24 - some definition more mass but great build for rb, fb, lb, de
25 - best build in the game by far...athletic frame with mass...used for rb, fb, te, ol, dl, lb...jacked wr or db

good luck let me know if you have any other questions..

also you can do a mass edit in the excel and import it back in based on every position as well.

BSAA is arm definition = set it to 10 for all playesr no matter what
BSAT is arm size = numbers can be 1, 4, 5, 15, 21, 24, or 25.. */
                
                int bsaa = 10;
                int bsat = 1;
                var profile = GetProfileId(positionGroup, 0);

                switch (profile)
                {
                    case ProfileId.QB_Balanced:
                    case ProfileId.QB_PocketPasser:
                    case ProfileId.QB_Scrambling:
                    case ProfileId.WR_Balanced:
                    case ProfileId.WR_Possession:
                    case ProfileId.WR_Speed:
                    case ProfileId.SS:
                    case ProfileId.CB:
                    case ProfileId.FS:
                    case ProfileId.Athlete:
                        bsaa = RAND.Next() % 2 == 0 ? 7 : 8;
                        bsat = 4;
                        break;
                    case ProfileId.HB_Speed:
                    case ProfileId.HB_Power:
                    case ProfileId.HB_Balanced:
                        bsaa = 7;
                        bsat = 15;
                        break; 
                    case ProfileId.TE:
                    case ProfileId.DE:
                    case ProfileId.LOLB:
                    case ProfileId.MLB:
                    case ProfileId.FB:
                    case ProfileId.ROLB:
                        bsaa = 10;
                        bsat = 24;
                        break; 
                    case ProfileId.OL:
                    case ProfileId.DT:
                        bsaa = 10;
                        bsat = 21;
                        break;
                    default:
                        return null;
                }

                return new Tuple<int, int>(bsaa, bsat);
            }
#endif
            public static ProfileId GetProfileId(int position, int tendency)
            {
                switch (position)
                {
                    case 0:
                        tendency = QBCounter % 3;
                        QBCounter++;
                        if (tendency == 0) return ProfileId.QB_PocketPasser;
                        if (tendency == 1) return ProfileId.QB_Balanced;
                        if (tendency == 2) return ProfileId.QB_Scrambling;
                        return ProfileId.None;
                    case 1:
                        return (ProfileId)RAND.Next(3, 6);
                    case 2:
                        return ProfileId.FB;
                    case 3:
                        return (ProfileId)RAND.Next(6, 9);
                    case 4:
                        return ProfileId.TE;
                    case 5:
                    case 6:
                    case 7:
                        return ProfileId.OL;
                    case 8:
                        return ProfileId.DE;
                    case 9:
                        return ProfileId.DT;
                    case 10:
                        return RAND.Next() % 2 == 0 ? ProfileId.LOLB : ProfileId.ROLB;
                    case 11:
                        return ProfileId.MLB;
                    case 12:
                        return ProfileId.CB;
                    case 13:
                        return ProfileId.FS;
                    case 14:
                        return ProfileId.SS;
                    case 18:
                        return ProfileId.Athlete;
                    default:
                        return ProfileId.None;
                }
            }
        }

        public enum ProfileId
        {
            QB_PocketPasser = 0,
            QB_Balanced = 1,
            QB_Scrambling = 2,
            HB_Power = 3,
            HB_Speed = 4,
            HB_Balanced = 5,
            WR_Speed = 6,
            WR_Balanced = 7,
            WR_Possession = 8,
            //TE_Balanced =9, 
            //TE_Blocking =10, 
            //TE_Recieving =11,
            TE = 9,
            ROLB = 10,
            LOLB = 11,
            SS = 12,
            CB = 13,
            MLB = 14,
            FS = 15,
            FB = 16,
            DE = 17,
            DT = 18,
            OL = 100,
            Athlete = 200,
            None = 1000
        }
        [DataContract]
        public class Rating
        {
            public int Min, Max;
            [DataMember]
            public string Name;
            [DataMember]
            public int Value;
            public Rating(int Min, int Max, string Name)
            {
                this.Max = Max;
                this.Min = Min;
                this.Name = Name;
            }
            public int GetRandomRating()
            {
                int mod = 0;//(Max - Min) / 4;
                return Value = CAPGen.RAND.Next(Min - mod, Max + 1);
            }
        }
        [DataContract]
        [KnownType(typeof(QB))]
        [KnownType(typeof(HB))]
        [KnownType(typeof(WR))]
        [KnownType(typeof(TE))]
        [KnownType(typeof(OLB))]
        [KnownType(typeof(MLB))]
        [KnownType(typeof(SS))]
        [KnownType(typeof(CB))]
        [KnownType(typeof(DE))]
        [KnownType(typeof(DT))]
        [KnownType(typeof(FS))]
        [KnownType(typeof(FB))]
        public class Player
        {
            public string TendencyOverride = null;
            public ProfileId Position;
            public int HeightMin;
            public int HeightMax; //in inches
            public int WeightMin, WeightMax; //in lbs
            [DataMember]
            public Rating[] Ratings;
            public int Height;
            public int Weight;
            [DataMember]
            public string Pos;
            [DataMember]
            public string Description
            {
                get
                {
                    return String.Format("{0}'{1}\"   {2} LBS", Height / 12, Height % 12, Weight);
                }
                set { }
            }
            public int GetRandomHeight()
            {
                return CAPGen.RAND.Next(HeightMin, HeightMax + 1);
            }
            public int GetRandomWieght()
            {
                return CAPGen.RAND.Next(WeightMin, WeightMax + 1);
            }
            public virtual void CreatePlayer()
            {
            }
            public void Display()
            {
                Console.WriteLine(this.Pos);
                Height = GetRandomHeight();
                Weight = GetRandomWieght();
                Console.WriteLine("{0}'{1}\"   {2} LBS", Height / 12, Height % 12, Weight);
                for (int i = 0; i < Ratings.Length; i++)
                {
                    Console.WriteLine("{0}     {1}", Ratings[i].Name, Ratings[i].GetRandomRating());
                }
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
                Console.WriteLine(String.Empty);
            }

            public static Dictionary<string, string> RatingMap = new Dictionary<string, string>()
            {
                {"PSPD","SPD"} ,
                {"PSTR","STR"},
                {"PAGI","AGI"},
                { "PACC","ACC"},
                {"PJMP","JMP"},
                {"PRTR","RRN"},
                {"TRAF","CIT"},
                {"SPCT","SPC"},
                {"PCTH","CAT"},
                {"PCAR","CAR"},
                {"PJMV","JUKE"},
                {"PSMV","SPIN"},
                {"PSAR","ARM"},
                {"PBCV","BCV"},
                {"PESV","ELU"},
                {"PTRK","TRK"},
                {"PBTK","BRK"},
                {"PAWR","AWR"},
                {"PPRC","PRC"},
                {"PPRS","PURSUIT"},
                {"PBSH","BSH"},
                {"PPMV","PMV"},
                {"PFMV","FMV"},
                {"PTAK","TKL"},
                {"PHIT","HIT"},
                {"PTHA","THA"},
                {"PTHP","THP"},
                {"PMCV","MCV"},
                {"PZCV","ZCV"},
                {"PYRS","PRESS"},
                {"RELS","RLS"},
                {"PPBK","PBK"},
                {"PPBF","PBF"},
                {"PPBS","PBS"},
                {"PRBK","RBK"},
                {"PBFW","RBF"},
                {"PRBS","RBS"},
                {"PIBL","IBK"},
                {"PKRT","RET"},
                {"PSTA","STA"},
                {"PINJ","INJ"},
            };

            public int GetRating(string dbTag, int currentRating)
            {
                Rating rating = null;
                if (RatingMap.ContainsKey(dbTag))
                {
                    rating = this.Ratings.Where(r => r.Name == RatingMap[dbTag]).FirstOrDefault();

                    if (rating != null && rating.Value < currentRating)
                        rating = null;
                }

                return rating == null ? currentRating : rating.Value;
            }
        }
        #endregion
        [DataContract]
        public class QB : Player
        {
            public QB(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "QB";
                if ((CAPGen.RAND.Next(0, Int32.MaxValue) % 10) == 0)
                {
                    this.Pos += " (LH)";
                }

                this.TendencyOverride = ((int)Position).ToString();
            }
            public override void CreatePlayer()
            {
                switch (this.Position)
                {
                    case ProfileId.QB_Balanced:
                        CreateBalancedQB();
                        break;
                    case ProfileId.QB_PocketPasser:
                        CreatePocketQB();
                        break;
                    case ProfileId.QB_Scrambling:
                        CreateScrabmlingQB();
                        break;
                    default:
                        break;
                }
                Display();
            }
            public void CreateBalancedQB()
            {
                //height weight
                this.HeightMin = 74;
                this.HeightMax = 77;
                this.WeightMin = 215;
                this.WeightMax = 245;
                this.Ratings = new Rating[]{
                new Rating( 70 , 80 , "SPD" ) , 
                new Rating( 52 , 70 , "STR" ) , 
                new Rating( 70 , 80 , "AGI" ) , 
                new Rating( 75 , 85 , "ACC" ) , 
                new Rating( 75 , 85 , "AWR" ) , 
                new Rating( 52 , 62 , "BRK" ) , 
                new Rating( 60 , 75 , "ELU" ) , 
                new Rating( 65 , 75 , "BCV" ) , 
                new Rating( 45 , 60 , "ARM" ) , 
                new Rating( 60 , 70 , "SPIN" ) ,
                new Rating( 60 , 70 , "JUKE" ) ,  
                new Rating( 60 , 75 , "CAR" ) , 
                new Rating( 65 , 75 , "JMP" ) , 
                new Rating( 85 , 90 , "THP" ) , 
                new Rating( 85 , 95 , "THA" ) , 
                new Rating( 81 , 96 , "STA" ) ,
                new Rating( 85 , 95 , "INJ" ) };
            }
            public void CreatePocketQB()
            {
                //height weight
                this.HeightMin = 75;
                this.HeightMax = 79;
                this.WeightMin = 225;
                this.WeightMax = 265;
                this.Ratings = new Rating[]{
                new Rating( 63 , 73 , "STR" ) , 
                new Rating( 56 , 66 , "AGI" ) , 
                new Rating( 50 , 66 , "SPD" ) , 
                new Rating( 62 , 75 , "ACC" ) , 
                new Rating( 68 , 80 , "AWR" ) , 
                new Rating( 62 , 70 , "CAR" ) , 
                new Rating( 88 , 99 , "THP" ) , 
                new Rating( 85 , 95 , "THA" ) , 
                new Rating( 62 , 72 , "BRK" ) , 
                new Rating( 85 , 95 , "INJ" ) , 
                new Rating( 60 , 76 , "JMP" ) , 
                new Rating( 35 , 45 , "ELU" ) , 
                new Rating( 55 , 65 , "BCV" ) , 
                new Rating( 25 , 35 , "ARM" ) , 
                new Rating( 20 , 30 , "SPIN" ) , 
                new Rating( 30 , 40 , "JUKE" ) , 
                new Rating( 81 , 96 , "STA" )  };
            }
            public void CreateScrabmlingQB()
            {
                //height weight
                this.HeightMin = 72;
                this.HeightMax = 77;
                this.WeightMin = 190;
                this.WeightMax = 245;
                this.Ratings = new Rating[]{
                new Rating( 56 , 63 , "STR" ) , 
                new Rating( 85 , 95 , "AGI" ) , 
                new Rating( 85 , 99 , "SPD" ) , 
                new Rating( 85 , 95 , "ACC" ) , 
                new Rating( 68 , 78 , "AWR" ) , 
                new Rating( 65 , 75 , "CAR" ) , 
                new Rating( 80 , 95 , "THP" ) , 
                new Rating( 80 , 90 , "THA" ) , 
                new Rating( 60 , 70 , "BRK" ) , 
                new Rating( 80 , 95 , "INJ" ) , 
                new Rating( 60 , 80 , "JMP" ) , 
                new Rating( 75 , 85 , "ELU" ) , 
                new Rating( 60 , 75 , "BCV" ) , 
                new Rating( 60 , 70 , "ARM" ) , 
                new Rating( 75 , 90 , "SPIN" ) , 
                new Rating( 75 , 90 , "JUKE" ) , 
                new Rating( 81 , 96 , "STA" )  };
            }
        }
        [DataContract]
        public class HB : Player
        {
            public HB(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "HB";
            }
            public override void CreatePlayer()
            {
                switch (this.Position)
                {
                    case ProfileId.HB_Balanced:
                        CreateBalancedHB();
                        break;
                    case ProfileId.HB_Speed:
                        CreateSpeedHB();
                        break;
                    case ProfileId.HB_Power:
                        CreatePowerHB();
                        break;
                    default:
                        break;
                }
                Display();
            }
            public void CreateBalancedHB()
            {
                //height weight
                this.HeightMin = 70;
                this.HeightMax = 74;
                this.WeightMin = 195;
                this.WeightMax = 225;
                this.Ratings = new Rating[]{
                new Rating( 71 , 80 , "STR" ) , 
                new Rating( 88 , 96 , "AGI" ) , 
                new Rating( 88 , 95 , "SPD" ) , 
                new Rating( 88 , 97 , "ACC" ) , 
                new Rating( 70 , 80 , "AWR" ) , 
                new Rating( 58 , 70 , "CAT" ) , 
                new Rating( 70 , 90 , "CAR" ) , 
                new Rating( 75 , 92 , "BRK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 70 , 80 , "JMP" ) , 
                new Rating( 70 , 95 , "RET" ) , 
                new Rating( 75 , 90 , "TRK" ) , 
                new Rating( 75 , 92 , "ELU" ) , 
                new Rating( 75 , 90 , "BCV" ) , 
                new Rating( 70 , 85 , "ARM" ) , 
                new Rating( 80 , 95 , "SPIN" ) , 
                new Rating( 80 , 95 , "JUKE" ) , 
                new Rating( 70 , 80 , "SPC" ) , 
                new Rating( 60 , 70 , "CIT" ) , 
                new Rating( 65 , 75 , "RRN" ) , 
                new Rating( 65 , 75 , "RLS" ) , 
                new Rating( 80 , 90 , "STA" )  };
            }
            public void CreateSpeedHB()
            {
                //height weight
                this.HeightMin = 67;
                this.HeightMax = 72;
                this.WeightMin = 165;
                this.WeightMax = 205;
                this.Ratings = new Rating[]{
                new Rating( 60 , 70 , "STR" ) , 
                new Rating( 90 , 99 , "AGI" ) , 
                new Rating( 90 , 99 , "SPD" ) , 
                new Rating( 90 , 99 , "ACC" ) , 
                new Rating( 65 , 75 , "AWR" ) , 
                new Rating( 65 , 75 , "CAT" ) , 
                new Rating( 65 , 85 , "CAR" ) , 
                new Rating( 70 , 85 , "BRK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 75 , 85 , "JMP" ) , 
                new Rating( 80 , 95 , "RET" ) , 
                new Rating( 70 , 80 , "TRK" ) , 
                new Rating( 80 , 99 , "ELU" ) , 
                new Rating( 75 , 95 , "BCV" ) , 
                new Rating( 65 , 85 , "ARM" ) , 
                new Rating( 85 , 99 , "SPIN" ) , 
                new Rating( 85 , 99 , "JUKE" ) , 
                new Rating( 65, 75 , "SPC" ) , 
                new Rating( 60 , 70 , "CIT" ) , 
                new Rating( 60 , 70 , "RRN" ) , 
                new Rating( 75 , 85 , "RLS" ) , 
                new Rating( 85 , 95 , "STA" )  };
            }
            public void CreatePowerHB()
            {
                //height weight
                this.HeightMin = 70;
                this.HeightMax = 75;
                this.WeightMin = 200;
                this.WeightMax = 250;
                this.Ratings = new Rating[]{
                new Rating( 74 , 85 , "STR" ) , 
                new Rating( 80 , 89 , "AGI" ) , 
                new Rating( 80 , 90 , "SPD" ) , 
                new Rating( 80 , 90 , "ACC" ) , 
                new Rating( 60 , 75 , "AWR" ) , 
                new Rating( 70 , 80 , "CAT" ) , 
                new Rating( 85 , 95 , "CAR" ) , 
                new Rating( 85 , 95 , "BRK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 60 , 70 , "JMP" ) , 
                new Rating( 30 , 45 , "RET" ) , 
                new Rating( 85 , 95 , "TRK" ) , 
                new Rating( 70 , 80 , "ELU" ) , 
                new Rating( 75 , 85 , "BCV" ) , 
                new Rating( 85 , 99 , "ARM" ) , 
                new Rating( 70 , 85 , "SPIN" ) , 
                new Rating( 70 , 85 , "JUKE" ) , 
                new Rating( 60 , 70 , "SPC" ) , 
                new Rating( 65 , 75 , "CIT" ) , 
                new Rating( 80 , 90 , "RRN" ) , 
                new Rating( 80 , 90 , "RLS" ) , 
                new Rating( 80 , 90 , "STA" )  };
            }
        }
        [DataContract]
        public class WR : Player
        {
            public WR(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "WR";
            }
            public override void CreatePlayer()
            {
                switch (this.Position)
                {
                    case ProfileId.WR_Balanced:
                        CreateBalancedWR();
                        break;
                    case ProfileId.WR_Speed:
                        CreateSpeedWR();
                        break;
                    case ProfileId.WR_Possession:
                        CreatePossWR();
                        break;
                    default:
                        break;
                }
                Display();
            }
            public void CreateBalancedWR()
            {
                //height weight
                this.HeightMin = 71;
                this.HeightMax = 75;
                this.WeightMin = 175;
                this.WeightMax = 215;
                this.Ratings = new Rating[]{
                new Rating( 55 , 65 , "STR" ) , 
                new Rating( 85 , 95 , "AGI" ) , 
                new Rating( 85 , 95 , "SPD" ) , 
                new Rating( 85 , 95 , "ACC" ) , 
                new Rating( 70 , 80 , "AWR" ) , 
                new Rating( 80 , 90 , "CAT" ) , 
                new Rating( 50 , 70 , "CAR" ) , 
                new Rating( 55 , 65 , "BRK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 75 , 90 , "JMP" ) , 
                new Rating( 75 , 90 , "RET" ) , 
                new Rating( 80 , 95 , "ELU" ) , 
                new Rating( 70 , 90 , "BCV" ) , 
                new Rating( 40 , 60 , "ARM" ) , 
                new Rating( 75 , 85 , "SPIN" ) , 
                new Rating( 80 , 95 , "JUKE" ) , 
                new Rating( 85, 99 , "SPC" ) , 
                new Rating( 75 , 90 , "CIT" ) , 
                new Rating( 80 , 95 , "RRN" ) , 
                new Rating( 80 , 95 , "RLS" ) , 
                new Rating( 85 , 95 , "STA" )  };
            }
            public void CreateSpeedWR()
            {
                //height weight
                this.HeightMin = 68;
                this.HeightMax = 72;
                this.WeightMin = 150;
                this.WeightMax = 185;
                this.Ratings = new Rating[]{
                new Rating( 48 , 59 , "STR" ) , 
                new Rating( 90 , 99 , "AGI" ) , 
                new Rating( 90 , 99 , "SPD" ) , 
                new Rating( 90 , 99 , "ACC" ) , 
                new Rating( 70 , 80 , "AWR" ) , 
                new Rating( 70 , 77 , "CAT" ) , 
                new Rating( 60 , 80 , "CAR" ) , 
                new Rating( 45 , 60 , "BRK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 80 , 95 , "JMP" ) , 
                new Rating( 85 , 99 , "RET" ) , 
                new Rating( 85 , 99 , "ELU" ) , 
                new Rating( 75 , 95 , "BCV" ) , 
                new Rating( 30 , 50 , "ARM" ) , 
                new Rating( 85 , 99 , "SPIN" ) , 
                new Rating( 85 , 99 , "JUKE" ) , 
                new Rating( 80, 95 , "SPC" ) , 
                new Rating( 70 , 85 , "CIT" ) , 
                new Rating( 75 , 90 , "RRN" ) , 
                new Rating( 80 , 95 , "RLS" ) , 
                new Rating( 85 , 95 , "STA" )  };
            }
            public void CreatePossWR()
            {
                //height weight
                this.HeightMin = 73;
                this.HeightMax = 77;
                this.WeightMin = 200;
                this.WeightMax = 235;
                this.Ratings = new Rating[]{
                new Rating( 60 , 70 , "STR" ) , 
                new Rating( 80 , 90 , "AGI" ) , 
                new Rating( 80 , 90 , "SPD" ) , 
                new Rating( 80 , 95 , "ACC" ) , 
                new Rating( 70 , 80 , "AWR" ) , 
                new Rating( 85 , 99 , "CAT" ) , 
                new Rating( 65 , 80 , "CAR" ) , 
                new Rating( 55 , 65 , "BRK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 85 , 99 , "JMP" ) , 
                new Rating( 70 , 80 , "RET" ) , 
                new Rating( 80 , 95 , "ELU" ) , 
                new Rating( 65 , 80 , "BCV" ) , 
                new Rating( 45 , 65 , "ARM" ) , 
                new Rating( 75 , 85 , "SPIN" ) , 
                new Rating( 80 , 95 , "JUKE" ) , 
                new Rating( 80, 95 , "SPC" ) , 
                new Rating( 85 , 99 , "CIT" ) , 
                new Rating( 86 , 99 , "RRN" ) , 
                new Rating( 85 , 99 , "RLS" ) , 
                new Rating( 85 , 95 , "STA" )  };
            }
        }
        [DataContract]
        public class TE : Player
        {
            public TE(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "TE";
            }
            public override void CreatePlayer()
            {
                switch (CAPGen.RAND.Next(0, 2))
                {
                    case 0:
                        CreateBalancedTE();
                        break;
                    case 1:
                        CreateRecTE();
                        break;
                    case 2:
                        CreateBlkTE();
                        break;
                    default:
                        break;
                }
                Display();
            }
            public void CreateBalancedTE()
            {
                this.Pos = "TE BAL";

                //height weight
                this.HeightMin = 75;
                this.HeightMax = 78;
                this.WeightMin = 230;
                this.WeightMax = 260;
                this.Ratings = new Rating[]{
                new Rating( 70 , 80 , "STR" ) , 
                new Rating( 70 , 80 , "AGI" ) , 
                new Rating( 70 , 80 , "SPD" ) , 
                new Rating( 70 , 80 , "ACC" ) , 
                new Rating( 70 , 80 , "AWR" ) , 
                new Rating( 70 , 85 , "CAT" ) , 
                new Rating( 60 , 70 , "CAR" ) , 
                new Rating( 60 , 70 , "BRK" ) , 
                new Rating( 55 +5+5, 65 +5+5, "RBK" ) , 
                new Rating( 55 +5+5, 65 +5+5, "PBK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 65+5 , 75 +5, "JMP" ) , 
                new Rating( 60 +5, 75 +5, "TRK" ) , 
                new Rating( 40 +5, 55 +5, "ELU" ) , 
                new Rating( 60 , 70 , "BCV" ) , 
                new Rating( 55+5 , 70 +5, "ARM" ) , 
                new Rating( 40 +5, 50+5 , "SPIN" ) , 
                new Rating( 40 +5, 50+5 , "JUKE" ) , 
                new Rating( 60 +5, 80+5 , "IBK" ) , 
                new Rating( 60 +5, 80 +5, "RBS" ) , 
                new Rating( 50+5 , 70+5 , "RBF" ) , 
                new Rating( 55+5 , 75+5 , "PBS" ) , 
                new Rating( 55+5 , 75+5 , "PBF" ) , 
                new Rating( 50, 65 , "SPC" ) , 
                new Rating( 80 , 95 , "CIT" ) , 
                new Rating( 80 , 95 , "RRN" ) , 
                new Rating( 50 , 60 , "RLS" ) , 
                new Rating( 70 , 85 , "STA" )  };
            }
            public void CreateRecTE()
            {
                //height weight
                this.Pos = "TE REC";
                this.HeightMin = 74;
                this.HeightMax = 77;
                this.WeightMin = 215;
                this.WeightMax = 245;
                this.Ratings = new Rating[]{
                new Rating( 65 , 75 , "STR" ) , 
                new Rating( 80 , 90 , "AGI" ) , 
                new Rating( 80 , 90 , "SPD" ) , 
                new Rating( 80 , 90 , "ACC" ) , 
                new Rating( 65 , 75 , "AWR" ) , 
                new Rating( 75 , 90 , "CAT" ) , 
                new Rating( 60 , 70 , "CAR" ) , 
                new Rating( 50 , 60 , "BRK" ) , 
                new Rating( 55 , 70 , "RBK" ) , 
                new Rating( 55 , 70 , "PBK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 70 , 80 , "JMP" ) , 
                new Rating( 45 , 60 , "TRK" ) , 
                new Rating( 50 , 65 , "ELU" ) , 
                new Rating( 60 , 70 , "BCV" ) , 
                new Rating( 45 , 55 , "ARM" ) , 
                new Rating( 65 , 70 , "SPIN" ) , 
                new Rating( 50 , 60 , "JUKE" ) , 
                new Rating( 60 , 75 , "IBK" ) , 
                new Rating( 60 , 75 , "RBS" ) , 
                new Rating( 60 , 75 , "RBF" ) , 
                new Rating( 60 , 75 , "PBS" ) , 
                new Rating( 60 , 75 , "PBF" ) , 
                new Rating( 80, 90 , "SPC" ) , 
                new Rating( 80 , 95 , "CIT" ) , 
                new Rating( 85 , 99 , "RRN" ) , 
                new Rating( 70 , 85 , "RLS" ) , 
                new Rating( 75 , 90 , "STA" )  };
            }
            public void CreateBlkTE()
            {
                this.Pos = "TE Blk";

                //height weight
                this.HeightMin = 77;
                this.HeightMax = 80;
                this.WeightMin = 240;
                this.WeightMax = 275;
                this.Ratings = new Rating[]{
                new Rating( 75 , 85 , "STR" ) , 
                new Rating( 60 , 75 , "AGI" ) , 
                new Rating( 60 , 75 , "SPD" ) , 
                new Rating( 60 , 75 , "ACC" ) , 
                new Rating( 70 , 80 , "AWR" ) , 
                new Rating( 60 , 70 , "CAT" ) , 
                new Rating( 55 , 75 , "CAR" ) , 
                new Rating( 60 , 75 , "BRK" ) , 
                new Rating( 65 , 75 , "RBK" ) , 
                new Rating( 65 , 75 , "PBK" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 50 , 60 , "JMP" ) , 
                new Rating( 70 , 85 , "TRK" ) , 
                new Rating( 50 , 75 , "ELU" ) , 
                new Rating( 60 , 70 , "BCV" ) , 
                new Rating( 65 , 75 , "ARM" ) , 
                new Rating( 40 , 50 , "SPIN" ) , 
                new Rating( 40 , 50 , "JUKE" ) , 
                new Rating( 75 , 90 , "IBK" ) , 
                new Rating( 70 , 85 , "RBS" ) , 
                new Rating( 70 , 90 , "RBF" ) , 
                new Rating( 70 , 85 , "PBS" ) , 
                new Rating( 70 , 85 , "PBF" ) , 
                new Rating( 45, 60 , "SPC" ) , 
                new Rating( 50 , 60 , "CIT" ) , 
                new Rating( 70 , 85 , "RRN" ) , 
                new Rating( 50 , 60 , "RLS" ) , 
                new Rating( 65 , 80 , "STA" )  };
            }
        }
        [DataContract]
        public class OLB : Player
        {
            public OLB(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = Position.ToString();
            }
            public override void CreatePlayer()
            {
                if (this.Position == ProfileId.ROLB)
                    CreateBalancedROLB();
                else
                    CreateBalancedLOLB();
                Display();
            }
            //WILL is more likely to support pass coverage
            public void CreateBalancedROLB()
            {
                //height weight
                this.HeightMin = 72; //6'0"
                this.HeightMax = 77; //6'5"
                this.WeightMin = 215;
                this.WeightMax = 265;
                this.Ratings = new Rating[]{
                new Rating( 55 , 80 , "STR" ) , 
                new Rating( 75 , 95 , "AGI" ) , 
                new Rating( 75 , 95 , "SPD" ) , 
                new Rating( 75 , 95 , "ACC" ) , 
                new Rating( 60 , 75 , "AWR" ) , 
                new Rating( 80 , 95 , "INJ" ) , 
                new Rating( 75 , 90 , "JMP" ) , 
                new Rating( 70+5 , 90+5 , "TKL" ) , 
                new Rating( 65+5 , 80+5 , "PMV" ) , 
                new Rating( 75 +5, 90 +5, "FMV" ) , 
                new Rating( 70+5 , 85 +5, "HIT" ) , 
                new Rating( 70 +5, 85 +5, "BSH" ) , 
                new Rating( 70 +5, 85+5 , "PURSUIT" ) , 
                new Rating( 65+5 , 80 +5, "PRC" ) , 
                new Rating( 75+5 , 90+5 , "MCV" ) , 
                new Rating( 70 +5, 85+5 , "ZCV" ) , 
                new Rating( 80 , 95 , "STA" )  };
            }
            //SAM is more likely to support the run
            public void CreateBalancedLOLB()
            {
                //height weight
                this.HeightMin = 72; //6'0"
                this.HeightMax = 77; //6'5"
                this.WeightMin = 215;
                this.WeightMax = 265;
                this.Ratings = new Rating[]{
                new Rating( 60 , 85 , "STR" ) , 
                new Rating( 75 , 90 , "AGI" ) , 
                new Rating( 75 , 90 , "SPD" ) , 
                new Rating( 75 , 90 , "ACC" ) , 
                new Rating( 60 +5, 70+5 , "AWR" ) , 
                new Rating( 80 , 95 , "INJ" ) , 
                new Rating( 70 , 85 , "JMP" ) , 
                new Rating( 75 +5, 95+4 , "TKL" ) , 
                new Rating( 70 +5, 85+5 , "PMV" ) , 
                new Rating( 70+5 , 85 +5, "FMV" ) , 
                new Rating( 75 , 90 , "HIT" ) , 
                new Rating( 75 , 90 , "BSH" ) , 
                new Rating( 70+5 , 90 , "PURSUIT" ) , 
                new Rating( 65 +5, 80 , "PRC" ) , 
                new Rating( 70 , 85 , "MCV" ) , 
                new Rating( 65 , 80 , "ZCV" ) , 
                new Rating( 80 , 95 , "STA" )  };
            }
        }
        [DataContract]
        public class MLB : Player
        {
            public MLB(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = Position.ToString();
            }
            public override void CreatePlayer()
            {
                CreateMLB();
                Display();
            }
            //WILL is more likely to support pass coverage
            public void CreateMLB()
            {
                //height weight
                this.HeightMin = 70; //5'10"
                this.HeightMax = 77; //6'5"
                this.WeightMin = 220;
                this.WeightMax = 265;
                this.Ratings = new Rating[]{
                new Rating( 75 , 90 , "STR" ) , 
                new Rating( 75 , 90 , "AGI" ) , 
                new Rating( 80 , 90 , "SPD" ) , 
                new Rating( 75 , 90 , "ACC" ) , 
                new Rating( 60 , 75 , "AWR" ) , 
                new Rating( 85 , 95 , "INJ" ) , 
                new Rating( 65 , 85 , "JMP" ) , 
                new Rating( 70 , 99 , "TKL" ) , 
                new Rating( 65+5 , 85+5 , "PMV" ) , 
                new Rating( 70 +5, 85 +5, "FMV" ) , 
                new Rating( 70 , 90 , "HIT" ) , 
                new Rating( 75+5 , 90+5, "BSH" ) , 
                new Rating( 80 , 95 , "PURSUIT" ) , 
                new Rating( 75 , 85 , "PRC" ) , 
                new Rating( 70 ,85 , "MCV" ) , 
                new Rating( 70 , 85 , "ZCV" ) , 
                new Rating( 65 , 80 , "PRESS" ) , 
                new Rating( 80 , 90 , "STA" )  };
            }
        }
        [DataContract]
        public class SS : Player
        {
            public SS(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "SS";
            }
            public override void CreatePlayer()
            {
                CreateSS();
                Display();
            }
            //WILL is more likely to support pass coverage
            public void CreateSS()
            {
                //height weight
                this.HeightMin = 70; //5'10"
                this.HeightMax = 75; //6'3"
                this.WeightMin = 180;
                this.WeightMax = 235;
                this.Ratings = new Rating[]{
                new Rating( 55 , 75 , "STR" ) , 
                new Rating( 80 , 99 , "AGI" ) , 
                new Rating( 80 , 99 , "SPD" ) , 
                new Rating( 80 , 99 , "ACC" ) , 
                new Rating( 60 , 70 , "AWR" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 70 +5, 85+5 , "JMP" ) , 
                new Rating( 70 +5, 85 +5, "TKL" ) , 
                new Rating( 70 +5, 90 , "HIT" ) , 
                new Rating( 65 +5, 85 +5, "BSH" ) , 
                new Rating( 65 , 80 , "CAT" ) , 
                new Rating( 70 , 90 , "PURSUIT" ) , 
                new Rating( 65+5 , 80 +5, "PRC" ) , 
                new Rating( 70+5 , 90+5 , "MCV" ) , 
                new Rating( 70+5 , 85 +5, "ZCV" ) , 
                new Rating( 80 , 95 , "STA" )  };
            }
        }
        [DataContract]
        public class CB : Player
        {
            public CB(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "CB";
            }
            public override void CreatePlayer()
            {
                CreateCB();
                Display();
            }
            //WILL is more likely to support pass coverage
            public void CreateCB()
            {
                //height weight
                this.HeightMin = 68; //5'8"
                this.HeightMax = 75; //6'3"
                this.WeightMin = 160;
                this.WeightMax = 210;
                this.Ratings = new Rating[]{
                new Rating( 50 , 65 , "STR" ) , 
                new Rating( 85 , 99 , "AGI" ) , 
                new Rating( 80 , 99 , "SPD" ) , 
                new Rating( 85 , 99 , "ACC" ) , 
                new Rating( 60 , 75 , "AWR" ) , 
                new Rating( 65 , 80 , "CAT" ) , 
                new Rating( 45 , 55 , "CAR" ) , 
                new Rating( 45 , 60 , "BRK" ) , 
                new Rating( 60 , 75 , "TKL" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 80 , 95 , "JMP" ) , 
                new Rating( 80 , 99 , "RET" ) , 
                new Rating( 40 , 55 , "TRK" ) , 
                new Rating( 75 , 95 , "ELU" ) , 
                new Rating( 70 , 85 , "BCV" ) , 
                new Rating( 50 , 60 , "ARM" ) , 
                new Rating( 80 , 95 , "SPIN" ) , 
                new Rating( 80 , 95 , "JUKE" ) , 
                new Rating( 40 , 55 , "PMV" ) , 
                new Rating( 40 , 55 , "FMV" ) , 
                new Rating( 50 , 65 , "BSH" ) , 
                new Rating( 80 , 99 , "PURSUIT" ) , 
                new Rating( 75 , 90 , "PRC" ) , 
                new Rating( 80 , 99 , "MCV" ) , 
                new Rating( 80 , 95 , "ZCV" ) , 
                new Rating( 50, 65 , "SPC" ) , 
                new Rating( 60 , 70 , "CIT" ) , 
                new Rating( 60 , 75 , "RRN" ) , 
                new Rating( 35 , 85 , "HIT" ) , 
                new Rating( 75 , 95 , "PRESS" ) , 
                new Rating( 75 , 85 , "RLS" ) , 
                new Rating( 85 , 99 , "STA" )  };
            }
        }
        [DataContract]
        public class DE : Player
        {
            public DE(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = CAPGen.RAND.Next() % 2 == 0 ? "LE" : "RE";
            }
            public override void CreatePlayer()
            {
                CreateDE();
                Display();
            }
            public void CreateDE()
            {
                //height weight
                this.HeightMin = 74; //6'2"
                this.HeightMax = 80; //6'8"
                this.WeightMin = 225;
                this.WeightMax = 295;
                this.Ratings = new Rating[]{
                new Rating( 70 +5, 90 , "STR" ) , 
                new Rating( 70 , 85 , "AGI" ) , 
                new Rating( 70 , 90 , "SPD" ) , 
                new Rating( 75 , 85 , "ACC" ) , 
                new Rating( 65 , 85 , "AWR" ) , 
                new Rating( 75 , 85 , "TKL" ) , 
                new Rating( 75 , 90 , "INJ" ) , 
                new Rating( 75 , 85 , "JMP" ) , 
                new Rating( 65+5 , 75+5 , "PMV" ) , 
                new Rating( 70 , 95 , "FMV" ) , 
                new Rating( 65 +5, 80 +5, "BSH" ) , 
                new Rating( 65+5 , 80 +5, "PURSUIT" ) , 
                new Rating( 65 +5, 80+5 , "PRC" ) , 
                new Rating( 40 , 70 , "MCV" ) , 
                new Rating( 40 , 75 , "ZCV" ) , 
                new Rating( 70 , 85 , "HIT" ) , 
                new Rating( 70 , 90 , "STA" )  };
            }
        }
        [DataContract]
        public class DT : Player
        {
            public DT(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "DT";
            }
            public override void CreatePlayer()
            {
                CreateDT();
                Display();
            }
            public void CreateDT()
            {
                //height weight
                this.HeightMin = 72; //6'0"
                this.HeightMax = 78; //6'6"
                this.WeightMin = 260;
                this.WeightMax = 360;
                this.Ratings = new Rating[]{
                new Rating( 75 , 90 , "STR" ) , 
                new Rating( 55 , 70 , "AGI" ) , 
                new Rating( 55 , 70 , "SPD" ) , 
                new Rating( 70 , 80 , "ACC" ) , 
                new Rating( 60 , 80 , "AWR" ) , 
                new Rating( 75 , 95 , "TKL" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 55 , 65 , "JMP" ) , 
                new Rating( 70 +5, 90 , "PMV" ) , 
                new Rating( 75+5 , 90 , "FMV" ) , 
                new Rating( 75 , 90 , "BSH" ) , 
                new Rating( 70 , 80 , "PURSUIT" ) , 
                new Rating( 55 , 75 , "PRC" ) , 
                new Rating( 75 , 90 , "HIT" ) , 
                new Rating( 65 , 85 , "STA" )  };
            }
        }
        [DataContract]
        public class FS : Player
        {
            public FS(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "FS";
            }
            public override void CreatePlayer()
            {
                CreateFS();
                Display();
            }
            public void CreateFS()
            {
                //height weight
                this.HeightMin = 70; //5'10"
                this.HeightMax = 77; //6'5"
                this.WeightMin = 175;
                this.WeightMax = 220;
                this.Ratings = new Rating[]{
                new Rating( 55 , 70 , "STR" ) , 
                new Rating( 80 , 99 , "AGI" ) , 
                new Rating( 80 , 99 , "SPD" ) , 
                new Rating( 80 , 99 , "ACC" ) , 
                new Rating( 60 , 75 , "AWR" ) , 
                new Rating( 60 , 74 , "CAT" ) , 
                new Rating( 45 , 55 , "CAR" ) , 
                new Rating( 45 , 60 , "BRK" ) , 
                new Rating( 75 , 90 , "TKL" ) , 
                new Rating( 80 , 90 , "INJ" ) , 
                new Rating( 70 , 90 , "JMP" ) , 
                new Rating( 80 , 99 , "RET" ) , 
                new Rating( 40 , 55 , "PMV" ) , 
                new Rating( 40 , 55 , "FMV" ) , 
                new Rating( 60+5 , 75+5 , "BSH" ) , 
                new Rating( 70+5 , 90 +5, "PURSUIT" ) , 
                new Rating( 70+5 , 85+5 , "PRC" ) , 
                new Rating( 70+5 , 90+5 , "MCV" ) , 
                new Rating( 70+5 , 90 +5, "ZCV" ) , 
                new Rating( 50 +5, 80+5 , "HIT" ) , 
                new Rating( 65 +5, 85+5 , "PRESS" ) , 
                new Rating( 80 , 99 , "STA" )  };
            }
        }


        [DataContract]
        public class OL : Player
        {
            public OL(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "OL";
            }
            public override void CreatePlayer()
            {
                //height weight
                this.HeightMin = 70; //5'10"
                this.HeightMax = 77; //6'5"
                this.WeightMin = 225;
                this.WeightMax = 270;
                this.Ratings = new Rating[]{
                new Rating( 75 , 95 , "STR" ) , 
                new Rating( 60 , 80 , "SPD" ) , 
                new Rating( 55 , 75 , "AGI" ) , 
                      new Rating( 80 , 95 , "ACC" ) , 
                new Rating( 65 , 80 , "AWR" ) , 
                  new Rating( 80 , 90 , "RBK" ) , 
                new Rating( 80 , 90 , "PBK" ) , 
                new Rating( 80 , 90 , "IBK" ) , 
                new Rating( 80 , 90 , "RBS" ) , 
                new Rating( 80 , 90 , "RBF" ) , 
                new Rating( 80 , 90 , "PBS" ) , 
                new Rating( 80 , 90 , "PBF" ) , 
                new Rating( 85 , 95 , "INJ" ) , 
                new Rating( 65 , 80 , "STA" ) , 
        };
            }
        }

        [DataContract]
        public class FB : Player
        {
            public FB(ProfileId Position)
            {
                this.Position = Position;
                this.Pos = "FB";
            }
            public override void CreatePlayer()
            {
                CreateFB();
                Display();
            }
            public void CreateFB()
            {
                //height weight
                this.HeightMin = 70; //5'10"
                this.HeightMax = 77; //6'5"
                this.WeightMin = 225;
                this.WeightMax = 270;
                this.Ratings = new Rating[]{
                new Rating( 65 , 85 , "STR" ) , 
                new Rating( 50 , 90 , "AGI" ) , 
                new Rating( 70 , 90 , "SPD" ) , 
                new Rating( 65 , 90 , "ACC" ) , 
                new Rating( 60 , 85 , "AWR" ) , 
                new Rating( 60 , 75 , "CAT" ) , 
                new Rating(60 , 80 , "CAR" ) , 
                new Rating( 65 , 95 , "TRK" ) , 
                new Rating( 60 , 90 , "BRK" ) , 
                new Rating( 50 , 85 , "ELU" ) , 
                new Rating( 60 , 85 , "BCV" ) , 
                new Rating( 60 , 95 , "ARM" ) , 
                new Rating( 50 , 90 , "SPIN" ) , 
                new Rating( 50 , 90 , "JUKE" ) , 
                new Rating( 60 , 90 , "JMP" ) , 
                new Rating( 60 , 75 , "SPC" ) , 
                new Rating( 60 , 75 , "CIT" ) , 
                new Rating( 60 , 75 , "RRN" ) , 
                new Rating( 50 , 80 , "RLS") , 
                new Rating( 50 , 80 , "RBK") , 
                new Rating( 50 , 80 , "PBK") , 
                new Rating( 60 , 99 , "IBK") , 
                new Rating( 60 , 99 , "RBS") , 
                new Rating( 60 , 90 , "RBF") , 
                new Rating( 60 , 85 , "PBS") , 
                new Rating( 60 , 95 , "PBF") , 
                new Rating( 70 , 90 , "STA" )  };
            }
        }
    }
}