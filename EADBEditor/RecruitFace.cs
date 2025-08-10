using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public enum FaceDescriptor
    {
        Lean,
        Fat,
        Muscular,
    }

    public enum Ethnicity
    {
        Dark,
        Light,
        Medium,
    }

    public class RecruitFace
    {
        public static List<RecruitFace> All = CreateFaces();

        public FaceDescriptor FaceDescriptor { get; set; }
        public Ethnicity Ethnicity { get; set; }
        public int Id { get; set; }

        public static RecruitFace Create(int id, FaceDescriptor descriptor, Ethnicity ethnicity)
        {
            return new RecruitFace
            {
                Id = id,
                FaceDescriptor = descriptor,
                Ethnicity = ethnicity,
            };
        }

        public static List<RecruitFace> CreateFaces()
        {
            List<RecruitFace> result = new List<RecruitFace>();

            // lean white 
            result.AddRange(Generate(1, 14, FaceDescriptor.Lean, Ethnicity.Light));
            result.AddRange(Generate(29, 51, FaceDescriptor.Lean, Ethnicity.Light));
            result.AddRange(Generate(64, 64, FaceDescriptor.Lean, Ethnicity.Light));

            // fat white
            result.AddRange(Generate(15, 28, FaceDescriptor.Fat, Ethnicity.Light));
            result.AddRange(Generate(52, 63, FaceDescriptor.Fat, Ethnicity.Light));
            result.AddRange(Generate(82, 87, FaceDescriptor.Fat, Ethnicity.Light));

            // lean medium
            result.AddRange(Generate(65, 81, FaceDescriptor.Lean, Ethnicity.Medium));

            return result;
        }

        public static IEnumerable<RecruitFace> Generate(int start, int end, FaceDescriptor descriptor, Ethnicity ethnicity)
        {
            List<RecruitFace> result = new List<RecruitFace>();
            for (int i = start; i <= end; i++)
            {
                result.Add(Create(i, descriptor, ethnicity));
            }
            return result;
        }
    }
}