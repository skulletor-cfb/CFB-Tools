using System;
using System.Windows.Forms;

namespace EA_DB_Editor
{
    public partial class TeamEntry : Form
    {
        public int TeamId { get; set; }

        public int? CoachPosition { get; set; } = null;

        public TeamEntry(string label = null)
        {
            InitializeComponent();
            this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            if( !string.IsNullOrWhiteSpace(label))
            {
                this.label1.Text = label;
            }
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