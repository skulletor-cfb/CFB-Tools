using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CFB27.Data.Model
{
    /// <summary>
    /// A CFB 27 playbook. Saves only hold asset references (Coach.OffensivePlaybook / DefensivePlaybook) and numbers
    /// (Team.TEAM_OFFPLAYBOOK / TEAM_DEFPLAYBOOK, the PlaybookId here), so names and styles come from the catalog MyDynasty
    /// embeds, extracted to Resources\playbooks.csv. A few PlaybookIds are shared by test and NFL playbooks; the asset id is unique.
    /// </summary>
    public class CFBPlaybook
    {
        private const string ResourceName = "CFB27.Data.Model.Resources.playbooks.csv";

        public bool IsOffense { get; set; }
        public int PlaybookId { get; set; }
        public uint AssetId { get; set; }
        public string Name { get; set; }

        /// <summary>The playbook's style, a BaseScheme name such as OFF_AIR_RAID or DEF_BASE4_3.</summary>
        public string Scheme { get; set; }

        /// <summary>The binary reference string a save stores for this playbook.</summary>
        public string Reference => Convert.ToString(AssetId, 2).PadLeft(32, '0');

        private static List<CFBPlaybook> embedded;

        public static IReadOnlyList<CFBPlaybook> Embedded
        {
            get
            {
                if (embedded == null)
                {
                    using (var stream = typeof(CFBPlaybook).Assembly.GetManifestResourceStream(ResourceName))
                    using (var reader = new StreamReader(stream))
                    {
                        reader.ReadLine(); // Side,PlaybookID,AssetId,Name,Scheme,IsEnabled
                        var list = new List<CFBPlaybook>();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length < 5)
                            {
                                continue;
                            }

                            list.Add(new CFBPlaybook
                            {
                                IsOffense = parts[0] == "Offense",
                                PlaybookId = int.Parse(parts[1]),
                                AssetId = Convert.ToUInt32(parts[2].Substring(2), 16),
                                Name = parts[3],
                                Scheme = parts[4],
                            });
                        }

                        embedded = list;
                    }
                }

                return embedded;
            }
        }

        /// <summary>The playbook a save reference (binary string) points to, or null.</summary>
        public static CFBPlaybook Find(string reference)
        {
            if (string.IsNullOrEmpty(reference))
            {
                return null;
            }

            var id = Convert.ToUInt32(reference, 2);
            return Embedded.FirstOrDefault(p => p.AssetId == id);
        }

        /// <summary>School playbooks a coach can be given: not the test, NFL (CHI) or generic FCS ones, nor the unused college defenses.</summary>
        public bool IsCollegeTeamPlaybook =>
            !Name.StartsWith("Test ", StringComparison.Ordinal)
            && !Name.StartsWith("CHI ", StringComparison.Ordinal)
            && !Name.StartsWith("FCS ", StringComparison.Ordinal)
            && Scheme != "DEF_UNUSED";
    }
}
