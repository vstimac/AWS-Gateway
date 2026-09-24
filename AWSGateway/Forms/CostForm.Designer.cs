namespace AWSGateway.Forms
{
    partial class CostForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CostForm));
            panelTopBar = new FlowLayoutPanel();
            lblPeriod = new Label();
            cmbPeriod = new ComboBox();
            btnLoad = new Button();
            btnForceRefresh = new Button();
            btnExportLogCsv = new Button();
            lblCacheStatus = new Label();
            lblTotal = new Label();
            lblComparison = new Label();
            lblTopService = new Label();
            costChart = new DailyCostChart();
            lblEmptyState = new Label();
            tableLayoutTables = new TableLayoutPanel();
            panelServices = new Panel();
            listViewServices = new DataGridView();
            colService = new DataGridViewTextBoxColumn();
            colServiceAmount = new DataGridViewTextBoxColumn();
            colServiceShare = new DataGridViewTextBoxColumn();
            lblServices = new Label();
            panelRegions = new Panel();
            listViewRegions = new DataGridView();
            colRegion = new DataGridViewTextBoxColumn();
            colRegionAmount = new DataGridViewTextBoxColumn();
            lblRegions = new Label();
            lblNote = new Label();
            panelTopBar.SuspendLayout();
            tableLayoutTables.SuspendLayout();
            panelServices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)listViewServices).BeginInit();
            panelRegions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)listViewRegions).BeginInit();
            SuspendLayout();
            // 
            // panelTopBar
            // 
            panelTopBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelTopBar.Controls.Add(lblPeriod);
            panelTopBar.Controls.Add(cmbPeriod);
            panelTopBar.Controls.Add(btnLoad);
            panelTopBar.Controls.Add(btnForceRefresh);
            panelTopBar.Controls.Add(btnExportLogCsv);
            panelTopBar.Location = new Point(20, 16);
            panelTopBar.Name = "panelTopBar";
            panelTopBar.Size = new Size(920, 40);
            panelTopBar.TabIndex = 0;
            panelTopBar.WrapContents = false;
            // 
            // lblPeriod
            // 
            lblPeriod.Anchor = AnchorStyles.None;
            lblPeriod.AutoSize = true;
            lblPeriod.Location = new Point(3, 14);
            lblPeriod.Margin = new Padding(3, 10, 3, 0);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.Size = new Size(44, 15);
            lblPeriod.TabIndex = 0;
            lblPeriod.Text = "Period:";
            // 
            // cmbPeriod
            // 
            cmbPeriod.Anchor = AnchorStyles.None;
            cmbPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriod.Location = new Point(53, 8);
            cmbPeriod.Margin = new Padding(3, 6, 10, 0);
            cmbPeriod.Name = "cmbPeriod";
            cmbPeriod.Size = new Size(180, 23);
            cmbPeriod.TabIndex = 1;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.None;
            btnLoad.Location = new Point(246, 3);
            btnLoad.Margin = new Padding(3, 3, 6, 0);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(140, 30);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "Prikaži troškove";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // btnForceRefresh
            // 
            btnForceRefresh.Anchor = AnchorStyles.None;
            btnForceRefresh.Location = new Point(395, 3);
            btnForceRefresh.Margin = new Padding(3, 3, 6, 0);
            btnForceRefresh.Name = "btnForceRefresh";
            btnForceRefresh.Size = new Size(140, 30);
            btnForceRefresh.TabIndex = 3;
            btnForceRefresh.Text = "Osvježi s AWS-a";
            btnForceRefresh.UseVisualStyleBackColor = true;
            btnForceRefresh.Click += btnForceRefresh_Click;
            // 
            // btnExportLogCsv
            // 
            btnExportLogCsv.Anchor = AnchorStyles.None;
            btnExportLogCsv.Location = new Point(544, 3);
            btnExportLogCsv.Margin = new Padding(3, 3, 3, 0);
            btnExportLogCsv.Name = "btnExportLogCsv";
            btnExportLogCsv.Size = new Size(160, 30);
            btnExportLogCsv.TabIndex = 4;
            btnExportLogCsv.Text = "Izvoz loga predmemorije";
            btnExportLogCsv.UseVisualStyleBackColor = true;
            btnExportLogCsv.Click += btnExportLogCsv_Click;
            // 
            // lblCacheStatus
            // 
            lblCacheStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCacheStatus.ForeColor = SystemColors.GrayText;
            lblCacheStatus.Location = new Point(20, 60);
            lblCacheStatus.Name = "lblCacheStatus";
            lblCacheStatus.Size = new Size(920, 25);
            lblCacheStatus.TabIndex = 1;
            lblCacheStatus.Text = "Odaberite period i kliknite Prikaži troškove.";
            // 
            // lblTotal
            // 
            lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTotal.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTotal.Location = new Point(20, 88);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(920, 28);
            lblTotal.TabIndex = 2;
            // 
            // lblComparison
            // 
            lblComparison.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblComparison.ForeColor = SystemColors.GrayText;
            lblComparison.Location = new Point(20, 118);
            lblComparison.Name = "lblComparison";
            lblComparison.Size = new Size(920, 20);
            lblComparison.TabIndex = 3;
            // 
            // lblTopService
            // 
            lblTopService.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTopService.Location = new Point(20, 140);
            lblTopService.Name = "lblTopService";
            lblTopService.Size = new Size(920, 20);
            lblTopService.TabIndex = 4;
            // 
            // costChart
            // 
            costChart.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            costChart.Location = new Point(20, 166);
            costChart.Name = "costChart";
            costChart.Size = new Size(920, 185);
            costChart.TabIndex = 5;
            // 
            // lblEmptyState
            // 
            lblEmptyState.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblEmptyState.ForeColor = SystemColors.GrayText;
            lblEmptyState.Location = new Point(20, 166);
            lblEmptyState.Name = "lblEmptyState";
            lblEmptyState.Size = new Size(920, 185);
            lblEmptyState.TabIndex = 6;
            lblEmptyState.TextAlign = ContentAlignment.MiddleCenter;
            lblEmptyState.Visible = false;
            // 
            // tableLayoutTables
            // 
            tableLayoutTables.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutTables.ColumnCount = 2;
            tableLayoutTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutTables.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutTables.Controls.Add(panelServices, 0, 0);
            tableLayoutTables.Controls.Add(panelRegions, 1, 0);
            tableLayoutTables.Location = new Point(20, 358);
            tableLayoutTables.Name = "tableLayoutTables";
            tableLayoutTables.RowCount = 1;
            tableLayoutTables.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutTables.Size = new Size(920, 270);
            tableLayoutTables.TabIndex = 7;
            // 
            // panelServices
            // 
            panelServices.Controls.Add(listViewServices);
            panelServices.Controls.Add(lblServices);
            panelServices.Dock = DockStyle.Fill;
            panelServices.Location = new Point(0, 0);
            panelServices.Margin = new Padding(0, 0, 8, 0);
            panelServices.Name = "panelServices";
            panelServices.Size = new Size(452, 270);
            panelServices.TabIndex = 0;
            // 
            // listViewServices
            // 
            listViewServices.AllowUserToAddRows = false;
            listViewServices.AllowUserToDeleteRows = false;
            listViewServices.AllowUserToResizeRows = false;
            listViewServices.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewServices.Columns.AddRange(new DataGridViewColumn[] { colService, colServiceAmount, colServiceShare });
            listViewServices.Location = new Point(0, 24);
            listViewServices.MultiSelect = false;
            listViewServices.Name = "listViewServices";
            listViewServices.ReadOnly = true;
            listViewServices.RowHeadersVisible = false;
            listViewServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            listViewServices.Size = new Size(452, 246);
            listViewServices.TabIndex = 1;
            listViewServices.SelectionChanged += listViewServices_SelectedIndexChanged;
            // 
            // colService
            // 
            colService.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colService.HeaderText = "Usluga";
            colService.MinimumWidth = 150;
            colService.Name = "colService";
            colService.ReadOnly = true;
            // 
            // colServiceAmount
            // 
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleRight;
            colServiceAmount.DefaultCellStyle = dataGridViewCellStyle1;
            colServiceAmount.HeaderText = "Trošak";
            colServiceAmount.Name = "colServiceAmount";
            colServiceAmount.ReadOnly = true;
            colServiceAmount.Width = 110;
            // 
            // colServiceShare
            // 
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight;
            colServiceShare.DefaultCellStyle = dataGridViewCellStyle2;
            colServiceShare.HeaderText = "Udio";
            colServiceShare.Name = "colServiceShare";
            colServiceShare.ReadOnly = true;
            colServiceShare.Width = 75;
            // 
            // lblServices
            // 
            lblServices.AutoSize = true;
            lblServices.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblServices.Location = new Point(0, 0);
            lblServices.Name = "lblServices";
            lblServices.Size = new Size(68, 19);
            lblServices.TabIndex = 0;
            lblServices.Text = "Po usluzi";
            // 
            // panelRegions
            // 
            panelRegions.Controls.Add(listViewRegions);
            panelRegions.Controls.Add(lblRegions);
            panelRegions.Dock = DockStyle.Fill;
            panelRegions.Location = new Point(468, 0);
            panelRegions.Margin = new Padding(8, 0, 0, 0);
            panelRegions.Name = "panelRegions";
            panelRegions.Size = new Size(452, 270);
            panelRegions.TabIndex = 1;
            // 
            // listViewRegions
            // 
            listViewRegions.AllowUserToAddRows = false;
            listViewRegions.AllowUserToDeleteRows = false;
            listViewRegions.AllowUserToResizeRows = false;
            listViewRegions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewRegions.Columns.AddRange(new DataGridViewColumn[] { colRegion, colRegionAmount });
            listViewRegions.Location = new Point(0, 24);
            listViewRegions.MultiSelect = false;
            listViewRegions.Name = "listViewRegions";
            listViewRegions.ReadOnly = true;
            listViewRegions.RowHeadersVisible = false;
            listViewRegions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            listViewRegions.Size = new Size(452, 246);
            listViewRegions.TabIndex = 1;
            // 
            // colRegion
            // 
            colRegion.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colRegion.HeaderText = "Regija";
            colRegion.MinimumWidth = 130;
            colRegion.Name = "colRegion";
            colRegion.ReadOnly = true;
            // 
            // colRegionAmount
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleRight;
            colRegionAmount.DefaultCellStyle = dataGridViewCellStyle3;
            colRegionAmount.HeaderText = "Trošak";
            colRegionAmount.Name = "colRegionAmount";
            colRegionAmount.ReadOnly = true;
            colRegionAmount.Width = 110;
            // 
            // lblRegions
            // 
            lblRegions.AutoSize = true;
            lblRegions.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblRegions.Location = new Point(0, 0);
            lblRegions.Name = "lblRegions";
            lblRegions.Size = new Size(66, 19);
            lblRegions.TabIndex = 0;
            lblRegions.Text = "Po regiji";
            // 
            // lblNote
            // 
            lblNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblNote.ForeColor = SystemColors.GrayText;
            lblNote.Location = new Point(20, 634);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(920, 45);
            lblNote.TabIndex = 8;
            lblNote.Text = resources.GetString("lblNote.Text");
            // 
            // CostForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(960, 690);
            Controls.Add(panelTopBar);
            Controls.Add(lblCacheStatus);
            Controls.Add(lblTotal);
            Controls.Add(lblComparison);
            Controls.Add(lblTopService);
            Controls.Add(costChart);
            Controls.Add(lblEmptyState);
            Controls.Add(tableLayoutTables);
            Controls.Add(lblNote);
            MinimumSize = new Size(900, 650);
            Name = "CostForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Troškovi";
            panelTopBar.ResumeLayout(false);
            panelTopBar.PerformLayout();
            tableLayoutTables.ResumeLayout(false);
            panelServices.ResumeLayout(false);
            panelServices.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)listViewServices).EndInit();
            panelRegions.ResumeLayout(false);
            panelRegions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)listViewRegions).EndInit();
            ResumeLayout(false);
        }

        private FlowLayoutPanel panelTopBar;
        private Label lblPeriod;
        private ComboBox cmbPeriod;
        private Button btnLoad;
        private Button btnForceRefresh;
        private Button btnExportLogCsv;
        private Label lblCacheStatus;
        private Label lblTotal;
        private Label lblComparison;
        private Label lblTopService;
        private DailyCostChart costChart;
        private Label lblEmptyState;
        private TableLayoutPanel tableLayoutTables;
        private Panel panelServices;
        private Label lblServices;
        private DataGridView listViewServices;
        private DataGridViewTextBoxColumn colService;
        private DataGridViewTextBoxColumn colServiceAmount;
        private DataGridViewTextBoxColumn colServiceShare;
        private Panel panelRegions;
        private Label lblRegions;
        private DataGridView listViewRegions;
        private DataGridViewTextBoxColumn colRegion;
        private DataGridViewTextBoxColumn colRegionAmount;
        private Label lblNote;
    }
}