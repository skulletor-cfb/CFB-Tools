using NCAA14Utility;
using System;
using System.Collections.Generic;
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
                        var scheduleModel = this.file.ReadJsonFile<ScheduleModel>();

                        try
                        {
                            bool addRows = this.dataGridView1.Rows.Count <= 1;

                            int idx = 0;
                            const int mod = 1; // how many elements in the list we have
                            foreach (var teamSchedule in scheduleModel.TeamSchedules)
                            {
                                var list = new List<string>()
                                {
                                    teamSchedule.TeamName,
                                    string.Empty,
                                };

                                for (int i = 1; i <= 15; i++)
                                {
                                    if (teamSchedule.ScheduleNodesByWeek.TryGetValue(i, out var node))
                                    {
                                        var prefix = node.IsLaborDayGame ? "R: " : string.Empty;
                                        list.Insert(mod + i, prefix + node.Opponent);
                                    }
                                    else
                                    {
                                        list.Insert(mod + i, string.Empty);
                                    }
                                }

                                list.Insert(mod + 16, string.Empty);
                                list.Insert(mod + 17, string.Join("  ", teamSchedule.Notes));
                                list.Insert(mod + 18, teamSchedule.G5Note);

                                if (addRows)
                                {
                                    this.dataGridView1.Rows.Add(list.ToArray());
                                }
                                else
                                {
                                    var row = this.dataGridView1.Rows[idx++];
                                    for (int i = 0; i < row.Cells.Count; i++)
                                    {
                                        var desc = i >= list.Count ? string.Empty : list[i];

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

                            if (addRows)
                            {
                                this.dataGridView1.Rows.Add(string.Empty);
                                this.dataGridView1.Rows.Add(scheduleModel.FCSGames);
                                this.dataGridView1.Rows.Add(scheduleModel.G5Games);
                                this.dataGridView1.Rows.Add(scheduleModel.P5Games);
                                this.dataGridView1.Rows.Add(scheduleModel.P5G5Games);
                                this.dataGridView1.Rows.Add(scheduleModel.TotalGames);
                            }
                            else
                            {
                                var values = new List<string>()
                                {
                                    string.Empty,
                                    scheduleModel.FCSGames,
                                    scheduleModel.G5Games,
                                    scheduleModel.P5Games,
                                    scheduleModel.P5G5Games,
                                    scheduleModel.TotalGames
                                };

                                foreach (var value in values)
                                {
                                    var row = this.dataGridView1.Rows[idx++];
                                    row.Cells[0].Value = value;
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
