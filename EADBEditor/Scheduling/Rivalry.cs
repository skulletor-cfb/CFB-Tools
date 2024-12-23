using System;

namespace EA_DB_Editor.Scheduling
{
    public class Rivalry
    {
        public Rivalry(int a, int b)
        {
            this.A = Math.Min(a, b);
            this.B = Math.Max(a, b);
        }

        public int A { get; }

        public int B { get; }

        public override bool Equals(object obj)
        {
            return obj is Rivalry other && this.A == other.A && this.B == other.B;

        }

        public override int GetHashCode()
        {
            return (this.A << 16) | this.B;
        }
    }
}