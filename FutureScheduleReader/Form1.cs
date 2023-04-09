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
                var rows = 67;//  table.Rows.Count;
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
            var team = textBox1.Text;
            this.Refresh(team);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.ReadExcel();
            var team = textBox1.Text;
            this.Refresh(team);
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