using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public enum Ethnicity
    {
        Dark,
        Light,
        Medium,
    }

    public class RecruitFace
    {
        static RecruitFace()
        {
            LeanFaces = Array.Empty<RecruitFace>()
                .Concat(Generate(1, 14, Ethnicity.Light))
                .Concat(Generate(29, 51, Ethnicity.Light))
                .Concat(Generate(Ethnicity.Light, 64, 92))
                .Concat(Generate(65, 81, Ethnicity.Medium))
                .Concat(Generate(88, 91, Ethnicity.Medium))
                .Concat(Generate(93, 104, Ethnicity.Light))
                .Concat(Generate(105, 114, Ethnicity.Medium))
                .Concat(Generate(121, 140, Ethnicity.Medium))
                .Concat(Generate(151, 159, Ethnicity.Medium))
                .Concat(Generate(160, 170, Ethnicity.Dark))
                .Concat(Generate(184, 221, Ethnicity.Dark))
                .Concat(Generate(233, 246, Ethnicity.Dark))
  //              .Concat(Generate(Ethnicity.Dark, 253, 254, 255, 256, 257))
                .ToList();

            FatFaces = Array.Empty<RecruitFace>()
                .Concat(Generate(15, 28, Ethnicity.Light))
                .Concat(Generate(52, 63, Ethnicity.Light))
                .Concat(Generate(82, 87, Ethnicity.Light))
                .Concat(Generate(115, 120, Ethnicity.Medium))
                .Concat(Generate(141, 151, Ethnicity.Medium))
                .Concat(Generate(170, 183, Ethnicity.Dark))
                .Concat(Generate(223, 232, Ethnicity.Dark))
//                .Concat(Generate(Ethnicity.Dark, 251, 252))
                .ToList();

            FatFacesSet = new HashSet<int>(FatFaces.Select(f => f.Id));

            AllFaces = new Dictionary<int, RecruitFace>();

            foreach ( var face in LeanFaces.Concat(FatFaces))
            {
                AllFaces[face.Id] = face;
            }
        }
        public static Random RAND = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0));
        public static List<RecruitFace> FatFaces { get; private set; }
        public static List<RecruitFace> LeanFaces { get; private set; }
        private static HashSet<int> FatFacesSet { get; }
        private static Dictionary<int, RecruitFace> AllFaces { get; }

        public Ethnicity Ethnicity { get; set; }
        public int Id { get; set; }

        public static bool IsFatFace(int face) => FatFacesSet.Contains(face);

        public static int FindNewFace(int face)
        {
            if (AllFaces.TryGetValue(face, out var rf))
            {
                while (true)
                {
                    var idx = RAND.Next(0, LeanFaces.Count);

                    if (rf.Ethnicity == LeanFaces[idx].Ethnicity)
                    {
                        return LeanFaces[idx].Id;
                    }
                }
            }

            // no match
            return face;
        }


        private static RecruitFace Create(int id, Ethnicity ethnicity)
        {
            return new RecruitFace
            {
                Id = id,
                Ethnicity = ethnicity,
            };
        }

        public static IEnumerable<RecruitFace> Generate(Ethnicity ethnicity, params int[] ids)
        {
            List<RecruitFace> result = new List<RecruitFace>();
            foreach (var id in ids)
            {
                result.Add(Create(id, ethnicity));
            }

            return result;
        }

        public static IEnumerable<RecruitFace> Generate(int start, int end, Ethnicity ethnicity)
        {
            List<RecruitFace> result = new List<RecruitFace>();
            for (int i = start; i <= end; i++)
            {
                result.Add(Create(i, ethnicity));
            }
            return result;
        }
    }
}