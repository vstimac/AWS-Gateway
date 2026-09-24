namespace AWSGateway.Forms
{
    partial class EC2ManagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listView1 = new DataGridView();
            btnStart = new Button();
            btnStop = new Button();
            btnReboot = new Button();
            btnRefresh = new Button();
            lblStatus = new Label();
            grpMetrics = new GroupBox();
            lblPeriod = new Label();
            cmbPeriod = new ComboBox();
            btnMetrics = new Button();
            lblUsageLevel = new Label();
            txtUsageSummary = new TextBox();
            btnDetails = new Button();
            listViewMetrics = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)listView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)listViewMetrics).BeginInit();
            grpMetrics.SuspendLayout();
            SuspendLayout();
            //
            // listView1
            //
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listView1.AllowUserToAddRows = false;
            listView1.AllowUserToDeleteRows = false;
            listView1.AllowUserToResizeRows = false;
            listView1.RowHeadersVisible = false;
            listView1.ReadOnly = true;
            listView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            listView1.MultiSelect = false;
            listView1.Location = new Point(20, 55);
            listView1.Name = "listView1";
            listView1.Size = new Size(755, 250);
            listView1.TabIndex = 1;
            listView1.Columns.Add("colEc2Id", "ID");
            listView1.Columns.Add("colEc2Name", "Naziv");
            listView1.Columns.Add("colEc2Status", "Status");
            listView1.Columns.Add("colEc2Region", "Regija");
            listView1.Columns[0].Width = 125;
            listView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            listView1.Columns[1].MinimumWidth = 200;
            listView1.Columns[2].Width = 150;
            listView1.Columns[3].Width = 145;
            // 
            // btnStart
            // 
            btnStart.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnStart.Location = new Point(20, 315);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(100, 30);
            btnStart.TabIndex = 3;
            btnStart.Text = "Pokreni";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnStop.Location = new Point(158, 315);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(100, 30);
            btnStop.TabIndex = 4;
            btnStop.Text = "Zaustavi";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnReboot
            // 
            btnReboot.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnReboot.Location = new Point(304, 315);
            btnReboot.Name = "btnReboot";
            btnReboot.Size = new Size(100, 30);
            btnReboot.TabIndex = 5;
            btnReboot.Text = "Restartaj";
            btnReboot.UseVisualStyleBackColor = true;
            btnReboot.Click += btnReboot_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(675, 315);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Osvježi";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Font = new Font("Segoe UI", 9.75F);
            lblStatus.ForeColor = SystemColors.GrayText;
            lblStatus.Location = new Point(20, 22);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(755, 30);
            lblStatus.TabIndex = 6;
            // 
            // grpMetrics
            // 
            grpMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpMetrics.Controls.Add(lblPeriod);
            grpMetrics.Controls.Add(cmbPeriod);
            grpMetrics.Controls.Add(btnMetrics);
            grpMetrics.Controls.Add(lblUsageLevel);
            grpMetrics.Controls.Add(txtUsageSummary);
            grpMetrics.Controls.Add(btnDetails);
            grpMetrics.Controls.Add(listViewMetrics);
            grpMetrics.Location = new Point(20, 360);
            grpMetrics.Name = "grpMetrics";
            grpMetrics.Size = new Size(755, 530);
            grpMetrics.TabIndex = 7;
            grpMetrics.TabStop = false;
            grpMetrics.Text = "CloudWatch analiza opterećenja";
            // 
            // lblPeriod
            // 
            lblPeriod.AutoSize = true;
            lblPeriod.Location = new Point(15, 32);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.Size = new Size(44, 15);
            lblPeriod.TabIndex = 0;
            lblPeriod.Text = "Period:";
            // 
            // cmbPeriod
            // 
            cmbPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriod.Location = new Point(75, 28);
            cmbPeriod.Name = "cmbPeriod";
            cmbPeriod.Size = new Size(170, 23);
            cmbPeriod.TabIndex = 1;
            // 
            // btnMetrics
            // 
            btnMetrics.Location = new Point(260, 25);
            btnMetrics.Name = "btnMetrics";
            btnMetrics.Size = new Size(150, 30);
            btnMetrics.TabIndex = 2;
            btnMetrics.Text = "Analiziraj";
            btnMetrics.UseVisualStyleBackColor = true;
            btnMetrics.Click += btnMetrics_Click;
            // 
            // lblUsageLevel
            // 
            lblUsageLevel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblUsageLevel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblUsageLevel.Location = new Point(15, 68);
            lblUsageLevel.Name = "lblUsageLevel";
            lblUsageLevel.Size = new Size(725, 28);
            lblUsageLevel.TabIndex = 3;
            lblUsageLevel.Text = "Procjena: -";
            // 
            // txtUsageSummary
            // 
            txtUsageSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUsageSummary.Font = new Font("Consolas", 9.75F);
            txtUsageSummary.Location = new Point(15, 100);
            txtUsageSummary.Multiline = true;
            txtUsageSummary.Name = "txtUsageSummary";
            txtUsageSummary.ReadOnly = true;
            txtUsageSummary.ScrollBars = ScrollBars.Vertical;
            txtUsageSummary.Size = new Size(725, 235);
            txtUsageSummary.TabIndex = 4;
            // 
            // btnDetails
            // 
            btnDetails.Location = new Point(15, 345);
            btnDetails.Name = "btnDetails";
            btnDetails.Size = new Size(100, 30);
            btnDetails.TabIndex = 5;
            btnDetails.Text = "Detalji";
            btnDetails.UseVisualStyleBackColor = true;
            btnDetails.Click += btnDetails_Click;
            // 
            // listViewMetrics
            // 
            listViewMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewMetrics.AllowUserToAddRows = false;
            listViewMetrics.AllowUserToDeleteRows = false;
            listViewMetrics.AllowUserToResizeRows = false;
            listViewMetrics.RowHeadersVisible = false;
            listViewMetrics.ReadOnly = true;
            listViewMetrics.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            listViewMetrics.MultiSelect = false;
            listViewMetrics.Location = new Point(15, 383);
            listViewMetrics.Name = "listViewMetrics";
            listViewMetrics.Size = new Size(725, 132);
            listViewMetrics.TabIndex = 6;
            listViewMetrics.Columns.Add("colMetricName", "Metrika");
            listViewMetrics.Columns.Add("colMetricTime", "Vrijeme");
            listViewMetrics.Columns.Add("colMetricAvg", "Prosjek");
            listViewMetrics.Columns.Add("colMetricMax", "Maksimum");
            listViewMetrics.Columns.Add("colMetricMin", "Minimum");
            listViewMetrics.Columns.Add("colMetricSum", "Zbroj");
            listViewMetrics.Columns.Add("colMetricUnit", "Jedinica");
            listViewMetrics.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            listViewMetrics.Columns[0].MinimumWidth = 130;
            listViewMetrics.Columns[1].Width = 120;
            listViewMetrics.Columns[2].Width = 90;
            listViewMetrics.Columns[3].Width = 90;
            listViewMetrics.Columns[4].Width = 90;
            listViewMetrics.Columns[5].Width = 110;
            listViewMetrics.Columns[6].Width = 70;
            // 
            // EC2ManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(800, 905);
            Controls.Add(listView1);
            Controls.Add(btnRefresh);
            Controls.Add(btnStart);
            Controls.Add(btnStop);
            Controls.Add(btnReboot);
            Controls.Add(grpMetrics);
            Controls.Add(lblStatus);
            Load += EC2ManagerForm_Load;
            MinimumSize = new Size(816, 600);
            Name = "EC2ManagerForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "EC2";
            ((System.ComponentModel.ISupportInitialize)listView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)listViewMetrics).EndInit();
            grpMetrics.ResumeLayout(false);
            grpMetrics.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private DataGridView listView1;
        private Button btnStart;
        private Button btnStop;
        private Button btnReboot;
        private Button btnRefresh;
        private Label lblStatus;
        private GroupBox grpMetrics;
        private Label lblPeriod;
        private ComboBox cmbPeriod;
        private Button btnMetrics;
        private Label lblUsageLevel;
        private TextBox txtUsageSummary;
        private Button btnDetails;
        private DataGridView listViewMetrics;
    }
}