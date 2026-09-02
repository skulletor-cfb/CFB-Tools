using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public class TeamBowlAppearances
    {
        public string Name { get; set; }
        public int TeamId { get; set; }
        public int Appearances { get { return Wins + Loss + Tie; } }
        public int Wins { get; set; }
        public int Loss { get; set; }
        public int Tie { get; set; }
        public string Pct
        {
            get
            {
                var pct = (Wins * 1000) / Appearances;

                if (pct == 1000)
                    return "1.000";

                if (pct == 0)
                    return ".000";

                var result = $".{(pct < 100 ? string.Concat("0", pct.ToString()) : pct.ToString())}";

                return result;
            }
        }
    }
}
