using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using ExcelApp = Microsoft.Office.Interop.Excel;

namespace FutureScheduleReader
{
    public partial class Form1 : Form
    {
        private const int TeamCount = 70; // 66 if usf/ucf/hou/smu aren't in
        private static ExcelApp.Application excelApp;
        List<(int year, string away, string home)> list = new List<(int year, string away, string home)>();
        int start = 0;
        int end = 0;
        private string excelSheet;

        public Form1()
        {
            this.excelSheet = ConfigurationManager.AppSettings["worksheet"];
            InitializeComponent();
            excelApp = new ExcelApp.Application();
            this.FormClosing += Form1_FormClosing;
            this.ReadExcel();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            excelApp.Quit();
            Marshal.ReleaseComObject(excelApp);
        }

        void ReadExcel()
        {
            this.Cursor = Cursors.WaitCursor;

            ExcelApp.Workbook excelBook = excelApp.Workbooks.Open(this.excelSheet, ReadOnly: true);

            try
            {
                ExcelApp._Worksheet excelSheet = excelBook.Sheets["Schedule"];
                var table = excelSheet.UsedRange;
                var rows = TeamCount + 1;//  table.Rows.Count;
                var cols = table.Columns.Count;
                list.Clear();
                start = Convert.ToInt32(table.Cells[1, 2].Value2);
                end = Convert.ToInt32(table.Cells[1, cols].Value2);

                for (var i = 2; i <= rows; i++)
                {
                    for (int j = 2; j <= cols; j++)
                    {
                        string away = table.Cells[i, j]?.Value2;
                        if (away == null)
                        {
                            continue;
                        }

                        // current year
                        var year = Convert.ToInt32(table.Cells[1, j].Value2);

                        foreach (var awayTeam in away.Split(','))
                        {
                            var home = table.Cells[i, 1].Value2;

                            list.Add((year, awayTeam.Trim(' '), home));
                        }
                    }
                }

            }
            finally
            {
                //after reading, relaase the excel project
                excelBook.Close();
                this.Cursor = Cursors.Default;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lookup.TryGetValue(textBox1.Text, out var team))
            {
                this.Refresh(team);
            }
            else
            {
                this.Refresh(textBox1.Text);
            }
        }

        #region aliases

        static readonly string BostonCollege = "	Boston College	".Trim();
        static readonly string Clemson = "	Clemson	".Trim();
        static readonly string Duke = "	Duke	".Trim();
        static readonly string FloridaState = "	Florida State	".Trim();
        static readonly string GeorgiaTech = "	Georgia Tech	".Trim();
        static readonly string Louisville = "	Louisville	".Trim();
        static readonly string Maryland = "	Maryland	".Trim();
        static readonly string Miami = "	Miami	".Trim();
        static readonly string NCState = "	NC State	".Trim();
        static readonly string NorthCarolina = "	North Carolina	".Trim();
        static readonly string Pittsburgh = "	Pittsburgh	".Trim();
        static readonly string Syracuse = "	Syracuse	".Trim();
        static readonly string Virginia = "	Virginia	".Trim();
        static readonly string VirginiaTech = "	Virginia Tech	".Trim();
        static readonly string WakeForest = "	Wake Forest	".Trim();
        static readonly string WestVirginia = "	West Virginia	".Trim();
        static readonly string Illinois = "	Illinois	".Trim();
        static readonly string Indiana = "	Indiana	".Trim();
        static readonly string Iowa = "	Iowa	".Trim();
        static readonly string Michigan = "	Michigan	".Trim();
        static readonly string MichiganState = "	Michigan State	".Trim();
        static readonly string Minnesota = "	Minnesota	".Trim();
        static readonly string Northwestern = "	Northwestern	".Trim();
        static readonly string OhioState = "	Ohio State	".Trim();
        static readonly string PennState = "	Penn State	".Trim();
        static readonly string Purdue = "	Purdue	".Trim();
        static readonly string Rutgers = "	Rutgers	".Trim();
        static readonly string Wisconsin = "	Wisconsin	".Trim();
        static readonly string Baylor = "	Baylor	".Trim();
        static readonly string Colorado = "	Colorado	".Trim();
        static readonly string IowaState = "	Iowa State	".Trim();
        static readonly string Kansas = "	Kansas	".Trim();
        static readonly string KansasState = "	Kansas State	".Trim();
        static readonly string Nebraska = "	Nebraska	".Trim();
        static readonly string Oklahoma = "	Oklahoma	".Trim();
        static readonly string OklahomaState = "	Oklahoma State	".Trim();
        static readonly string TCU = "	TCU	".Trim();
        static readonly string Texas = "	Texas	".Trim();
        static readonly string TexasTech = "	Texas Tech	".Trim();
        static readonly string BoiseState = "	Boise State	".Trim();
        static readonly string BYU = "	BYU	".Trim();
        static readonly string Arizona = "	Arizona	".Trim();
        static readonly string ArizonaState = "	Arizona State	".Trim();
        static readonly string California = "	California	".Trim();
        static readonly string Oregon = "	Oregon	".Trim();
        static readonly string OregonState = "	Oregon State	".Trim();
        static readonly string Stanford = "	Stanford	".Trim();
        static readonly string UCLA = "	UCLA	".Trim();
        static readonly string USC = "	USC	".Trim();
        static readonly string Utah = "	Utah	".Trim();
        static readonly string Washington = "	Washington	".Trim();
        static readonly string WashingtonState = "	Washington State	".Trim();
        static readonly string Alabama = "	Alabama	".Trim();
        static readonly string Arkansas = "	Arkansas	".Trim();
        static readonly string Auburn = "	Auburn	".Trim();
        static readonly string Florida = "	Florida	".Trim();
        static readonly string Georgia = "	Georgia	".Trim();
        static readonly string Kentucky = "	Kentucky	".Trim();
        static readonly string LSU = "	LSU	".Trim();
        static readonly string MississippiState = "	Mississippi State	".Trim();
        static readonly string Missouri = "	Missouri	".Trim();
        static readonly string OleMiss = "	Ole Miss	".Trim();
        static readonly string SouthCarolina = "	South Carolina	".Trim();
        static readonly string Tennessee = "	Tennessee	".Trim();
        static readonly string TexasAM = "	Texas A&M	".Trim();
        static readonly string Vanderbilt = "	Vanderbilt	".Trim();
        static readonly string Houston = "Houston";
        static readonly string Cincy = "Cincy";
        static readonly string AtBoiseSt = "at Boise State";
        static readonly string UCF = "UCF";
        static readonly string USF = "USF";
        static readonly string SMU = "SMU";

        private static Dictionary<string, string> lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {SMU, SMU },
            {UCF, UCF },
            {USF, USF },
            {AtBoiseSt, AtBoiseSt },
            {"at BSU", AtBoiseSt },
            { Houston, Houston },
            { Cincy, Cincy },
            {"bc", BostonCollege },
            {BostonCollege , BostonCollege },
            {Clemson, Clemson },
            {Duke, Duke },
            { FloridaState, FloridaState },
            { "fsu", FloridaState },
            {"GT", GeorgiaTech },
            {GeorgiaTech, GeorgiaTech },
            {"UL", Louisville },
            {Louisville, Louisville },
            {Maryland, Maryland },
            {"UMD", Maryland },
            {Miami, Miami },
            {NCState, NCState },
            {"NCSU", NCState },
            {NorthCarolina, NorthCarolina },
            {"UNC", NorthCarolina },
            {"Pitt", Pittsburgh },
            { Pittsburgh, Pittsburgh },
            {Syracuse, Syracuse },
            {"SU", Syracuse },
            {Virginia, Virginia },
            {"UVA", Virginia },
            {VirginiaTech, VirginiaTech },
            {"VT", VirginiaTech },
            {WakeForest, WakeForest },
            {"WF", WakeForest },
            {WestVirginia, WestVirginia },
            {"WVU", WestVirginia },
            {"UH", "Houston" },
            {"Hou", "Houston" },
{   Illinois    ,   Illinois    },
{ "Ill"    ,   Illinois    },
{ "UI"    ,   Illinois    },
{   Indiana ,   Indiana },
{   "Indy",   Indiana },
{   "IU",   Indiana },
{   Iowa    ,   Iowa    },
{   "Mich"    ,   Michigan    },
{   Michigan    ,   Michigan    },
{   MichiganState  ,   MichiganState  },
{   "MichSt"  ,   MichiganState  },
{   "Mich St"  ,   MichiganState  },
{   Minnesota   ,   Minnesota   },
{   "Minn"   ,   Minnesota   },
{   Northwestern    ,   Northwestern    },
{   "NW"    ,   Northwestern    },
{   OhioState  ,   OhioState  },
{   "OSU" ,   OhioState  },
{   PennState  ,   PennState  },
{   "PSU" ,   PennState  },
{   Purdue  ,   Purdue  },
{   "Pur"  ,   Purdue  },
{   Rutgers ,   Rutgers },
{   "RU" ,   Rutgers },
{   Wisconsin   ,   Wisconsin   },
{   "Wisc",   Wisconsin   },
{   Baylor  ,   Baylor  },
{   "bay" ,   Baylor  },
{   "bu"  ,   Baylor  },
{   Colorado    ,   Colorado    },
{   "CU"    ,   Colorado    },
{   IowaState  ,   IowaState  },
{   "ISU" ,   IowaState  },
{   Kansas  ,   Kansas  },
{   "KU"  ,   Kansas  },
{   KansasState    ,   KansasState    },
{   "KSU"    ,   KansasState    },
{   Nebraska    ,   Nebraska    },
{   "Neb"    ,   Nebraska    },
{   Oklahoma    ,   Oklahoma    },
{   "OU"    ,   Oklahoma    },
{   OklahomaState  ,   OklahomaState  },
{   "OkSt"  ,   OklahomaState  },
{   "Ok St"  ,   OklahomaState  },
{   TCU ,   TCU },
{   Texas   ,   Texas   },
{   TexasTech  ,   TexasTech  },
{   "TT"  ,   TexasTech  },
{   BoiseState ,   BoiseState },
{   "BSU" ,   BoiseState },
{   BYU ,   BYU },
{   Arizona ,   Arizona },
{   "AU" ,   Arizona },
{   ArizonaState   ,   ArizonaState   },
{   "ASU" ,   ArizonaState   },
{   California  ,   California  },
{   "cal",   California  },
{   Oregon  ,   Oregon  },
{   "UO",   Oregon  },
{   OregonState    ,   OregonState    },
{   "orst",   OregonState    },
{   "or st",   OregonState    },
{   Stanford    ,   Stanford    },
{   "Stan",   Stanford    },
{   UCLA    ,   UCLA    },
{   USC ,   USC },
{   Utah    ,   Utah    },
{   Washington  ,   Washington  },
{   "UW" ,   Washington  },
{   WashingtonState    ,   WashingtonState    },
{   "WSU",   WashingtonState    },
{   Alabama ,   Alabama },
{   "bama",   Alabama },
{   Arkansas    ,   Arkansas    },
{   "Ark",   Arkansas    },
{   Auburn  ,   Auburn  },
{   Florida ,   Florida },
{   "UF",   Florida },
{   Georgia ,   Georgia },
{   "UGA",   Georgia },
{   Kentucky    ,   Kentucky    },
{   "UK",   Kentucky    },
{   LSU ,   LSU },
{   MississippiState   ,   MississippiState   },
{   "MissSt",   MississippiState   },
{   "Miss St",   MississippiState   },
{   Missouri    ,   Missouri    },
{   "mizzou",   Missouri    },
{   OleMiss    ,   OleMiss    },
{   SouthCarolina  ,   SouthCarolina  },
{   "SCAR",   SouthCarolina  },
{   Tennessee   ,   Tennessee   },
{   "TENN",   Tennessee   },
{   TexasAM   ,   TexasAM   },
{   "TAMU",   TexasAM   },
{   Vanderbilt  ,   Vanderbilt  },
{   "VandY",   Vanderbilt  },
        };
        #endregion
        private void button2_Click(object sender, EventArgs e)
        {
            this.ReadExcel();

            if (lookup.TryGetValue(textBox1.Text, out var team))
            {
                this.Refresh(team);
            }
            else
            {
                this.Refresh(textBox1.Text);
            }
        }

        private void Refresh(string team)
        {
            this.dataGridView1.Rows.Clear();
            var filtered = list.Where(g => g.home.Equals(team) || g.away.Equals(team)).OrderBy(v => v.year).ToArray();

            var arr = new List<(int year, string away, string home)>[1 + end - start];

            foreach (var f in filtered)
            {
                var idx = f.year - start;
                var list = arr[idx];

                if (list == null)
                {
                    list = arr[idx] = new List<(int year, string away, string home)>();
                }

                list.Add(f);
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != null)
                {
                    foreach (var r in arr[i])
                    {
                        this.dataGridView1.Rows.Add(r.year, r.away, r.home);
                    }
                }
                else
                {
                    this.dataGridView1.Rows.Add(i + start, string.Empty, string.Empty);
                }
            }
        }
    }
}