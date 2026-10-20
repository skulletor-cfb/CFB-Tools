namespace TVSlots
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dataGridView1 = new DataGridView();
            scheduleNodeBindingSource = new BindingSource(components);
            Week = new DataGridViewTextBoxColumn();
            ACC = new DataGridViewTextBoxColumn();
            Big10 = new DataGridViewTextBoxColumn();
            Big12 = new DataGridViewTextBoxColumn();
            Pac12 = new DataGridViewTextBoxColumn();
            SEC = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)scheduleNodeBindingSource).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Week, SEC, ACC, Big10, Pac12, Big12 });
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.Size = new Size(1013, 608);
            dataGridView1.TabIndex = 0;
            // 
            // scheduleNodeBindingSource
            // 
            scheduleNodeBindingSource.DataSource = typeof(NCAA14Utility.ScheduleNode);
            // 
            // Week
            // 
            Week.HeaderText = "Week";
            Week.MinimumWidth = 8;
            Week.Name = "Week";
            Week.ReadOnly = true;
            Week.Width = 150;
            // 
            // ACC
            // 
            ACC.HeaderText = "ACC";
            ACC.MinimumWidth = 8;
            ACC.Name = "ACC";
            ACC.ReadOnly = true;
            ACC.Width = 150;
            // 
            // Big10
            // 
            Big10.HeaderText = "Big 10";
            Big10.MinimumWidth = 8;
            Big10.Name = "Big10";
            Big10.ReadOnly = true;
            Big10.Width = 150;
            // 
            // Big12
            // 
            Big12.HeaderText = "Big 12";
            Big12.MinimumWidth = 8;
            Big12.Name = "Big12";
            Big12.ReadOnly = true;
            Big12.Width = 150;
            // 
            // Pac12
            // 
            Pac12.HeaderText = "Pac 12";
            Pac12.MinimumWidth = 8;
            Pac12.Name = "Pac12";
            Pac12.ReadOnly = true;
            Pac12.Width = 150;
            // 
            // SEC
            // 
            SEC.HeaderText = "SEC";
            SEC.MinimumWidth = 8;
            SEC.Name = "SEC";
            SEC.ReadOnly = true;
            SEC.Width = 150;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1013, 608);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "TV Slots";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)scheduleNodeBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private BindingSource scheduleNodeBindingSource;
        private DataGridViewTextBoxColumn Week;
        private DataGridViewTextBoxColumn ACC;
        private DataGridViewTextBoxColumn Big10;
        private DataGridViewTextBoxColumn Big12;
        private DataGridViewTextBoxColumn Pac12;
        private DataGridViewTextBoxColumn SEC;
    }
}
