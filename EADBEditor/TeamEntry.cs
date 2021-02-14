using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EA_DB_Editor
{
    public partial class TeamEntry : Form
    {
        public int TeamId { get; set; }

        public int? CoachPosition { get; set; } = null;

        public TeamEntry()
        {
            InitializeComponent();
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            if (this.textBox1.Text.Contains(":"))
            {
                var split = this.textBox1.Text.Split(':');
                this.TeamId = Convert.ToInt32(split[0]);
                this.CoachPosition = Convert.ToInt32(split[1]);
            }
            else
            {
                this.TeamId = Convert.ToInt32(this.textBox1.Text);
            }
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {

        }
    }
}