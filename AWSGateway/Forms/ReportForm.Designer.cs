namespace AWSGateway.Forms
{
    partial class ReportForm
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
            if (disposing)
            {
                // modul se u ljusci ne zatvara pojedinačno (FormClosed se ne okida) - oslobađanje mora proći i ovim putem
                ReleaseResources();

                if (components != null)
                {
                    components.Dispose();
                }
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
            tlpFilters = new TableLayoutPanel();
            pnlPeriod = new Panel();
            lblPeriod = new Label();
            cmbPeriod = new ComboBox();
            pnlDirection = new Panel();
            lblDirection = new Label();
            cmbDirection = new ComboBox();
            pnlStatus = new Panel();
            lblStatusFilter = new Label();
            cmbStatus = new ComboBox();
            pnlBucket = new Panel();
            lblBucketFilter = new Label();
            cmbBucketFilterCtrl = new ComboBox();
            pnlSearch = new Panel();
            lblSearch = new Label();
            txtSearch = new TextBox();
            pnlAllUsers = new Panel();
            chkAllUsers = new CheckBox();
            
            tlpSummary = new TableLayoutPanel();
            lblSumTotal = new Label();
            lblSumSuccess = new Label();
            lblSumUpload = new Label();
            lblSumDownload = new Label();
            lblSumErrors = new Label();
            lblSumSpeed = new Label();

            lblCommonError = new Label();
            dgvTransfers = new DataGridView();
            
            flowBottomButtons = new FlowLayoutPanel();
            btnCopyS3Path = new Button();
            btnOpenFolder = new Button();
            btnDeleteHistory = new Button();
            lblRowCount = new Label();
            btnExportCsv = new Button();
            btnExportPdf = new Button();

            tlpFilters.SuspendLayout();
            pnlPeriod.SuspendLayout();
            pnlDirection.SuspendLayout();
            pnlStatus.SuspendLayout();
            pnlBucket.SuspendLayout();
            pnlSearch.SuspendLayout();
            pnlAllUsers.SuspendLayout();
            tlpSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTransfers).BeginInit();
            flowBottomButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tlpFilters
            // 
            tlpFilters.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tlpFilters.ColumnCount = 6;
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F)); // Period
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Smjer
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F)); // Status
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Bucket (fleksibilan)
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F)); // Pretraga
            tlpFilters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F)); // Svi korisnici
            tlpFilters.Controls.Add(pnlPeriod, 0, 0);
            tlpFilters.Controls.Add(pnlDirection, 1, 0);
            tlpFilters.Controls.Add(pnlStatus, 2, 0);
            tlpFilters.Controls.Add(pnlBucket, 3, 0);
            tlpFilters.Controls.Add(pnlSearch, 4, 0);
            tlpFilters.Controls.Add(pnlAllUsers, 5, 0);
            tlpFilters.Location = new Point(12, 12);
            tlpFilters.Name = "tlpFilters";
            tlpFilters.RowCount = 1;
            tlpFilters.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpFilters.Size = new Size(1076, 52);
            tlpFilters.TabIndex = 0;
            // 
            // pnlPeriod
            // 
            pnlPeriod.Controls.Add(cmbPeriod);
            pnlPeriod.Controls.Add(lblPeriod);
            pnlPeriod.Dock = DockStyle.Fill;
            pnlPeriod.Location = new Point(0, 0);
            pnlPeriod.Name = "pnlPeriod";
            pnlPeriod.Size = new Size(150, 52);
            pnlPeriod.TabIndex = 0;
            // 
            // lblPeriod
            // 
            lblPeriod.AutoSize = true;
            lblPeriod.Location = new Point(0, 3);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.Size = new Size(44, 15);
            lblPeriod.TabIndex = 0;
            lblPeriod.Text = "Period";
            // 
            // cmbPeriod
            // 
            cmbPeriod.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPeriod.Location = new Point(0, 21);
            cmbPeriod.Name = "cmbPeriod";
            cmbPeriod.Size = new Size(144, 23);
            cmbPeriod.TabIndex = 1;
            cmbPeriod.SelectedIndexChanged += Filter_Changed;
            // 
            // pnlDirection
            // 
            pnlDirection.Controls.Add(cmbDirection);
            pnlDirection.Controls.Add(lblDirection);
            pnlDirection.Dock = DockStyle.Fill;
            pnlDirection.Location = new Point(150, 0);
            pnlDirection.Name = "pnlDirection";
            pnlDirection.Size = new Size(120, 52);
            pnlDirection.TabIndex = 1;
            // 
            // lblDirection
            // 
            lblDirection.AutoSize = true;
            lblDirection.Location = new Point(0, 3);
            lblDirection.Name = "lblDirection";
            lblDirection.Size = new Size(38, 15);
            lblDirection.TabIndex = 0;
            lblDirection.Text = "Smjer";
            // 
            // cmbDirection
            // 
            cmbDirection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbDirection.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDirection.Location = new Point(0, 21);
            cmbDirection.Name = "cmbDirection";
            cmbDirection.Size = new Size(114, 23);
            cmbDirection.TabIndex = 1;
            cmbDirection.SelectedIndexChanged += Filter_Changed;
            // 
            // pnlStatus
            // 
            pnlStatus.Controls.Add(cmbStatus);
            pnlStatus.Controls.Add(lblStatusFilter);
            pnlStatus.Dock = DockStyle.Fill;
            pnlStatus.Location = new Point(270, 0);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(130, 52);
            pnlStatus.TabIndex = 2;
            // 
            // lblStatusFilter
            // 
            lblStatusFilter.AutoSize = true;
            lblStatusFilter.Location = new Point(0, 3);
            lblStatusFilter.Name = "lblStatusFilter";
            lblStatusFilter.Size = new Size(39, 15);
            lblStatusFilter.TabIndex = 0;
            lblStatusFilter.Text = "Status";
            // 
            // cmbStatus
            // 
            cmbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Location = new Point(0, 21);
            cmbStatus.Name = "cmbStatus";
            cmbStatus.Size = new Size(124, 23);
            cmbStatus.TabIndex = 1;
            cmbStatus.SelectedIndexChanged += Filter_Changed;
            // 
            // pnlBucket
            // 
            pnlBucket.Controls.Add(cmbBucketFilterCtrl);
            pnlBucket.Controls.Add(lblBucketFilter);
            pnlBucket.Dock = DockStyle.Fill;
            pnlBucket.Location = new Point(400, 0);
            pnlBucket.Name = "pnlBucket";
            pnlBucket.Size = new Size(334, 52);
            pnlBucket.TabIndex = 3;
            // 
            // lblBucketFilter
            // 
            lblBucketFilter.AutoSize = true;
            lblBucketFilter.Location = new Point(0, 3);
            lblBucketFilter.Name = "lblBucketFilter";
            lblBucketFilter.Size = new Size(43, 15);
            lblBucketFilter.TabIndex = 0;
            lblBucketFilter.Text = "Bucket";
            // 
            // cmbBucketFilterCtrl
            // 
            cmbBucketFilterCtrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbBucketFilterCtrl.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBucketFilterCtrl.Location = new Point(0, 21);
            cmbBucketFilterCtrl.Name = "cmbBucketFilterCtrl";
            cmbBucketFilterCtrl.Size = new Size(328, 23);
            cmbBucketFilterCtrl.TabIndex = 1;
            cmbBucketFilterCtrl.SelectedIndexChanged += Filter_Changed;
            // 
            // pnlSearch
            // 
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(lblSearch);
            pnlSearch.Dock = DockStyle.Fill;
            pnlSearch.Location = new Point(734, 0);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Size = new Size(220, 52);
            pnlSearch.TabIndex = 4;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(0, 3);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(52, 15);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Pretraga";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(0, 21);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Naziv datoteke ili ključ...";
            txtSearch.Size = new Size(214, 23);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // pnlAllUsers
            // 
            pnlAllUsers.Controls.Add(chkAllUsers);
            pnlAllUsers.Dock = DockStyle.Fill;
            pnlAllUsers.Location = new Point(954, 0);
            pnlAllUsers.Name = "pnlAllUsers";
            pnlAllUsers.Size = new Size(122, 52);
            pnlAllUsers.TabIndex = 5;
            // 
            // chkAllUsers
            // 
            chkAllUsers.AutoSize = true;
            chkAllUsers.Location = new Point(0, 23);
            chkAllUsers.Name = "chkAllUsers";
            chkAllUsers.Size = new Size(95, 19);
            chkAllUsers.TabIndex = 0;
            chkAllUsers.Text = "Svi korisnici";
            chkAllUsers.UseVisualStyleBackColor = true;
            chkAllUsers.CheckedChanged += chkAllUsers_CheckedChanged;
            // 
            // tlpSummary
            // 
            tlpSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tlpSummary.ColumnCount = 6;
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.7F));
            tlpSummary.Controls.Add(lblSumTotal, 0, 0);
            tlpSummary.Controls.Add(lblSumSuccess, 1, 0);
            tlpSummary.Controls.Add(lblSumUpload, 2, 0);
            tlpSummary.Controls.Add(lblSumDownload, 3, 0);
            tlpSummary.Controls.Add(lblSumErrors, 4, 0);
            tlpSummary.Controls.Add(lblSumSpeed, 5, 0);
            tlpSummary.Location = new Point(12, 72);
            tlpSummary.Name = "tlpSummary";
            tlpSummary.RowCount = 1;
            tlpSummary.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpSummary.Size = new Size(1076, 52);
            tlpSummary.TabIndex = 1;
            // 
            // lblSumTotal
            // 
            lblSumTotal.Dock = DockStyle.Fill;
            lblSumTotal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSumTotal.Location = new Point(3, 0);
            lblSumTotal.Name = "lblSumTotal";
            lblSumTotal.Size = new Size(173, 52);
            lblSumTotal.TabIndex = 0;
            lblSumTotal.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSumSuccess
            // 
            lblSumSuccess.Dock = DockStyle.Fill;
            lblSumSuccess.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSumSuccess.Location = new Point(182, 0);
            lblSumSuccess.Name = "lblSumSuccess";
            lblSumSuccess.Size = new Size(173, 52);
            lblSumSuccess.TabIndex = 1;
            lblSumSuccess.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSumUpload
            // 
            lblSumUpload.Dock = DockStyle.Fill;
            lblSumUpload.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSumUpload.Location = new Point(361, 0);
            lblSumUpload.Name = "lblSumUpload";
            lblSumUpload.Size = new Size(173, 52);
            lblSumUpload.TabIndex = 2;
            lblSumUpload.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSumDownload
            // 
            lblSumDownload.Dock = DockStyle.Fill;
            lblSumDownload.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSumDownload.Location = new Point(540, 0);
            lblSumDownload.Name = "lblSumDownload";
            lblSumDownload.Size = new Size(173, 52);
            lblSumDownload.TabIndex = 3;
            lblSumDownload.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSumErrors
            // 
            lblSumErrors.Dock = DockStyle.Fill;
            lblSumErrors.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSumErrors.Location = new Point(719, 0);
            lblSumErrors.Name = "lblSumErrors";
            lblSumErrors.Size = new Size(173, 52);
            lblSumErrors.TabIndex = 4;
            lblSumErrors.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSumSpeed
            // 
            lblSumSpeed.Dock = DockStyle.Fill;
            lblSumSpeed.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSumSpeed.Location = new Point(898, 0);
            lblSumSpeed.Name = "lblSumSpeed";
            lblSumSpeed.Size = new Size(175, 52);
            lblSumSpeed.TabIndex = 5;
            lblSumSpeed.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCommonError
            // 
            lblCommonError.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCommonError.Location = new Point(12, 132);
            lblCommonError.Name = "lblCommonError";
            lblCommonError.Size = new Size(1076, 20);
            lblCommonError.TabIndex = 2;
            // 
            // dgvTransfers
            // 
            dgvTransfers.AllowUserToAddRows = false;
            dgvTransfers.AllowUserToDeleteRows = false;
            dgvTransfers.AllowUserToResizeRows = false;
            dgvTransfers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvTransfers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTransfers.Location = new Point(12, 158);
            dgvTransfers.MultiSelect = true;
            dgvTransfers.Name = "dgvTransfers";
            dgvTransfers.ReadOnly = true;
            dgvTransfers.RowHeadersVisible = false;
            dgvTransfers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTransfers.ShowCellToolTips = true;
            dgvTransfers.Size = new Size(1076, 495);
            dgvTransfers.TabIndex = 3;
            dgvTransfers.CellFormatting += dgvTransfers_CellFormatting;
            dgvTransfers.CellToolTipTextNeeded += dgvTransfers_CellToolTipTextNeeded;
            dgvTransfers.SelectionChanged += dgvTransfers_SelectionChanged;
            // 
            // flowBottomButtons
            // 
            flowBottomButtons.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowBottomButtons.Controls.Add(btnCopyS3Path);
            flowBottomButtons.Controls.Add(btnOpenFolder);
            flowBottomButtons.Controls.Add(btnDeleteHistory);
            flowBottomButtons.Controls.Add(lblRowCount);
            flowBottomButtons.Controls.Add(btnExportCsv);
            flowBottomButtons.Controls.Add(btnExportPdf);
            flowBottomButtons.Location = new Point(12, 663);
            flowBottomButtons.Name = "flowBottomButtons";
            flowBottomButtons.Size = new Size(1076, 40);
            flowBottomButtons.TabIndex = 4;
            // 
            // btnCopyS3Path
            // 
            btnCopyS3Path.Location = new Point(3, 3);
            btnCopyS3Path.Name = "btnCopyS3Path";
            btnCopyS3Path.Size = new Size(150, 34);
            btnCopyS3Path.TabIndex = 0;
            btnCopyS3Path.Text = "Kopiraj S3 putanju";
            btnCopyS3Path.UseVisualStyleBackColor = true;
            btnCopyS3Path.Click += btnCopyS3Path_Click;
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Location = new Point(159, 3);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(130, 34);
            btnOpenFolder.TabIndex = 1;
            btnOpenFolder.Text = "Otvori mapu";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // btnDeleteHistory
            // 
            btnDeleteHistory.Location = new Point(295, 3);
            btnDeleteHistory.Name = "btnDeleteHistory";
            btnDeleteHistory.Size = new Size(160, 34);
            btnDeleteHistory.TabIndex = 2;
            btnDeleteHistory.Text = "Obriši iz povijesti";
            btnDeleteHistory.UseVisualStyleBackColor = true;
            btnDeleteHistory.Click += btnDeleteHistory_Click;
            // 
            // lblRowCount
            // 
            lblRowCount.ForeColor = SystemColors.GrayText;
            lblRowCount.Location = new Point(461, 3);
            lblRowCount.Name = "lblRowCount";
            lblRowCount.Size = new Size(310, 34);
            lblRowCount.TabIndex = 3;
            lblRowCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnExportCsv
            // 
            btnExportCsv.Location = new Point(777, 3);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(140, 34);
            btnExportCsv.TabIndex = 4;
            btnExportCsv.Text = "Izvezi CSV";
            btnExportCsv.UseVisualStyleBackColor = true;
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // btnExportPdf
            // 
            btnExportPdf.Location = new Point(923, 3);
            btnExportPdf.Name = "btnExportPdf";
            btnExportPdf.Size = new Size(140, 34);
            btnExportPdf.TabIndex = 5;
            btnExportPdf.Text = "Izvezi PDF";
            btnExportPdf.UseVisualStyleBackColor = true;
            btnExportPdf.Click += btnExportPdf_Click;
            // 
            // ReportForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1100, 715);
            Controls.Add(tlpFilters);
            Controls.Add(tlpSummary);
            Controls.Add(lblCommonError);
            Controls.Add(dgvTransfers);
            Controls.Add(flowBottomButtons);
            MinimumSize = new Size(950, 600);
            Name = "ReportForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Izvještaj";
            FormClosed += ReportForm_FormClosed;
            Load += ReportForm_Load;
            tlpFilters.ResumeLayout(false);
            pnlPeriod.ResumeLayout(false);
            pnlPeriod.PerformLayout();
            pnlDirection.ResumeLayout(false);
            pnlDirection.PerformLayout();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            pnlBucket.ResumeLayout(false);
            pnlBucket.PerformLayout();
            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            pnlAllUsers.ResumeLayout(false);
            pnlAllUsers.PerformLayout();
            tlpSummary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTransfers).EndInit();
            flowBottomButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tlpFilters;
        private Panel pnlPeriod;
        private Label lblPeriod;
        private ComboBox cmbPeriod;
        private Panel pnlDirection;
        private Label lblDirection;
        private ComboBox cmbDirection;
        private Panel pnlStatus;
        private Label lblStatusFilter;
        private ComboBox cmbStatus;
        private Panel pnlBucket;
        private Label lblBucketFilter;
        private ComboBox cmbBucketFilterCtrl;
        private Panel pnlSearch;
        private Label lblSearch;
        private TextBox txtSearch;
        private Panel pnlAllUsers;
        private CheckBox chkAllUsers;
        
        private TableLayoutPanel tlpSummary;
        private Label lblSumTotal;
        private Label lblSumSuccess;
        private Label lblSumUpload;
        private Label lblSumDownload;
        private Label lblSumErrors;
        private Label lblSumSpeed;

        private Label lblCommonError;
        private DataGridView dgvTransfers;

        private FlowLayoutPanel flowBottomButtons;
        private Button btnCopyS3Path;
        private Button btnOpenFolder;
        private Button btnDeleteHistory;
        private Label lblRowCount;
        private Button btnExportCsv;
        private Button btnExportPdf;
    }
}