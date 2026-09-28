using System.Linq;
namespace EA_DB_Editor
{
    public enum SleeveKind
    {
        ShortSleeve,
        LongSleeve,
        TopSleeve,
        BottomSleeve,
        ThreeQtrSleeve,
    }

    public enum ColorKind
    {
        White = 0,
        Black = 1,
        TeamColor = 2,
    }
    public abstract class Sleeve
    {
        /***************
            sleeves l/r

            PLSL (LEFT)
            PLSR (RIGHT)

            0 - short
            1 - long white
            2 - long black
            3 - long team color
            4 - bottom 1/2 white
            5 - bottom 1/2 black
            6 - bottom 1/2 team color
            7 - top 1/2 white
            8 - top 1/2 black
            9 - top 1/2 team color
            10 - 3/4 white
            11 - 3/4 black
            12 - 3/4 team color
            ***************/
        protected abstract int Seed { get; }

        public virtual ColorKind Color { get; }

        public abstract SleeveKind SleeveKind { get; }

        public virtual int Value => this.Seed + ((int)this.Color);

        protected Sleeve() { }

        protected Sleeve(ColorKind color) { this.Color = color; }

        /// <summary>
        /// returns true if the sleeve is one of the types specified
        /// </summary>
        /// <param name="kinds"></param>
        /// <returns></returns>
        public bool Match(params SleeveKind[] kinds)
        {
            return kinds.Any(k => k == this.SleeveKind);
        }
    }

    public class LongSleeve : Sleeve
    {
        protected override int Seed => 1;

        public LongSleeve(ColorKind color) : base(color) { }

        public override SleeveKind SleeveKind => SleeveKind.LongSleeve;
    }

    public class BottomSleeve : Sleeve
    {
        protected override int Seed => 4;

        public BottomSleeve(ColorKind color) : base(color) { }

        public override SleeveKind SleeveKind => SleeveKind.BottomSleeve;
    }

    public class TopSleeve : Sleeve
    {
        protected override int Seed => 7;
        public TopSleeve(ColorKind color) : base(color) { }

        public override SleeveKind SleeveKind => SleeveKind.TopSleeve;
    }

    public class ThreeQtrSleeve : Sleeve
    {
        protected override int Seed => 10;
        public ThreeQtrSleeve(ColorKind color) : base(color) { }

        public override SleeveKind SleeveKind => SleeveKind.ThreeQtrSleeve;
    }

    public class ShortSleeve : Sleeve
    {
        protected override int Seed => 0;

        public override int Value => 0;

        public override SleeveKind SleeveKind => SleeveKind.ShortSleeve;
    }
}
