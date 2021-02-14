using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ScheduleDisplay
{
    public partial class Form1 : Form
    {
        private string file = null;
        private DateTime lastEdit = DateTime.MinValue;
        public Form1()
        {
            InitializeComponent();
            this.file = ConfigurationManager.AppSettings["ScheduleFile"];
            ThreadPool.QueueUserWorkItem(Loop, null);
        }

        private void Loop(object state)
        {
            while (true)
            {
                Thread.Sleep(2000);

                var lastWrite = File.GetLastWriteTimeUtc(this.file);

                if (File.Exists(this.file) && lastWrite > this.lastEdit)
                {
                    this.lastEdit = lastWrite;

                    this.Invoke(new Action(() =>
                    {
                        //                        this.dataGridView1.Rows.Clear();

                        try
                        {
                            var lines = File.ReadAllLines(this.file);
                            bool addRows = this.dataGridView1.Rows.Count <= 1;

                            int idx = 0;
                            foreach (var line in lines.Skip(2))
                            {
                                var split = line.Split(',').ToList();

                                if (line.EndsWith(","))
                                {
                                    split.Add(string.Empty);
                                }

                                if (addRows)
                                {
                                    this.dataGridView1.Rows.Add(split.ToArray());
                                }
                                else
                                {
                                    var row = this.dataGridView1.Rows[idx++];
                                    for (int i = 0; i < row.Cells.Count; i++)
                                    {
                                        var desc = i >= split.Count ? string.Empty : split[i];

                                        if (desc.StartsWith("R: "))
                                        {
                                            desc = desc.Substring(3);
                                            row.Cells[i].Style.BackColor = Color.LightGray;
                                        }
                                        else if (row.Cells[i].Style.BackColor == Color.LightGray)
                                        {
                                            row.Cells[i].Style.BackColor = Color.White;
                                        }

                                        row.Cells[i].Value = desc;
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    }));
                }
            }
        }

        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            var cell = this.dataGridView1.SelectedCells[0];

            this.Invoke(
                new Action(()=>
                {
                    cell.Value = Clipboard.GetData(DataFormats.Text) as string;
            }));
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
        }


        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
        }
    }
}
