using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Editor
{
    public class TransferCandidate
    {
        public int Id { get; set; }
        public int OVR { get; set; }
        public int Year { get; set; }
        public string First { get; set; }
        public string Last { get; set; }
        public string Team { get; set; }
        public int TeamId { get; set; }

        public bool Redshirted { get; set; }

        public string State { get; set; }

        public int P5 => this.TeamId.IsP5() ? 1 : 2;

        public string Position { get; set; }

        public int PositionNumber { get; set; }

        public bool IsOT => this.PositionNumber == 5 || this.PositionNumber == 6;

        public bool IsOG => this.PositionNumber == 6 || this.PositionNumber == 8;

        public bool IsDE => this.PositionNumber == 10 || this.PositionNumber == 11;

        public bool IsOLB => this.PositionNumber == 13 || this.PositionNumber == 15;

        public string ToCsvLine()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Id, OVR, Position, First, Last, Team, TeamId, Year, State);
        }
    }
}