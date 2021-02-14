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
    public partial class PlayerEntry : Form
    {
        public int From { get; set; }
        public int To { get; set; }
        public PlayerEntry()
        {
            InitializeComponent();
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.From = Convert.ToInt32(this.textBox1.Text);
            this.To = Convert.ToInt32(this.textBox2.Text);
        }
    }
}