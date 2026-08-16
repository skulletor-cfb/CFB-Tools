using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaker.Contracts
{
    public class Player
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public int Stat1 { get; set; }
        public int Stat2 { get; set; }
        public int Stat3 { get; set; }
        public int Stat4 { get; set; }
        public int Stat5 { get; set; }
        public int Stat6 { get; set; }
        public int Games { get; set; }
        public int TableIndex { get; set; }
        public int TeamId { get; set; }
        public int FirstYear { get; set; }
        public int LastYear { get; set; }

        //No,Name,PlayerClass,Position,Height,Weight,Stat1,Stat2,Stat3,Stat4,Stat5,Stat6,Games,TableIdx,Year
        public static Player Generate(string[] parts, int teamId, int year)
        {
            return new Player
            {
                FirstYear = year,
                LastYear = year,
                Number = parts[0].ToInt(),
                Name = parts[1],
                Position = parts[3],
                Height = parts[4],
                Weight = parts[5],
                Stat1 = parts[6].ToInt(),
                Stat2 = parts[7].ToInt(),
                Stat3 = parts[8].ToInt(),
                Stat4 = parts[9].ToInt(),
                Stat5 = parts[10].ToInt(),
                Stat6 = parts[11].ToInt(),
                Games = parts[12].ToInt(),
                TableIndex = parts[13].ToInt(),
                TeamId = teamId
            };
        }

        public TableRow ToTableRow(bool useStat6 = false, bool isDefense = false)
        {
            var stat3 = this.Stat3.ToString();

            if (isDefense)
            {
                stat3 = (((float)Stat3) / 10).ToString();
            }

            var tr = new TableRow(
                string.Format("{0}-{1}", FirstYear, LastYear),
                this.Number.ToString(),
                this.Name.MakeBold(),
                this.Position,
                this.Height,
                this.Weight,
                this.Stat1.ToString(),
                this.Stat2.ToString(),
                stat3,
                this.Stat4.ToString(),
                this.Stat5.ToString());

            if (useStat6)
                tr.Cells.Add(this.Stat6.ToString());

            tr.Cells.Add(this.Games.ToString());
            return tr;
        }

        public void Merge(Player other)
        {
            this.FirstYear = Math.Min(this.FirstYear, other.FirstYear);
            this.LastYear = Math.Max(this.LastYear, other.LastYear);
            this.Stat1 += other.Stat1;
            this.Stat2 += other.Stat2;
            this.Stat3 += other.Stat3;
            this.Stat4 += other.Stat4;
            this.Stat5 += other.Stat5;
            this.Stat6 += other.Stat6;
            this.Games += other.Games;
        }

        public override int GetHashCode()
        {
            return this.Number.GetHashCode() ^ this.Name.GetHashCode() ^ this.TableIndex.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as Player;
            return other != null && other.Number == this.Number && string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) && this.TableIndex == other.TableIndex;
        }
    }
}
