namespace AWSGateway.Forms
{
    partial class TeamStatusForm
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
            tabControl = new TabControl();
            tabPageTeam = new TabPage();
            lblServerStatus = new Label();
            lblServerDetails = new Label();
            btnCheckServer = new Button();
            lblBasicNote = new Label();
            txtMessage = new TextBox();
            btnSend = new Button();
            btnSendMyLog = new Button();
            lblTeamActivity = new Label();
            btnRefreshTeam = new Button();
            dgvTeam = new DataGridView();
            lblSessionDetails = new Label();
            dgvSessionEntries = new DataGridView();
            lblTeamStatus = new Label();
            tabPageLog = new TabPage();
            lblLogPeriod = new Label();
            cmbLogPeriod = new ComboBox();
            chkOnlyErrors = new CheckBox();
            lblSource = new Label();
            cmbSource = new ComboBox();
            lblLogSearch = new Label();
            txtLogSearch = new TextBox();
            dgvLog = new DataGridView();
            lblLogCount = new Label();
            btnRefreshLog = new Button();
            btnExportLog = new Button();
            tabControl.SuspendLayout();
            tabPageTeam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTeam).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvSessionEntries).BeginInit();
            tabPageLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLog).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageTeam);
            tabControl.Controls.Add(tabPageLog);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(900, 660);
            tabControl.TabIndex = 0;
            // 
            // tabPageTeam
            // 
            tabPageTeam.Controls.Add(lblServerStatus);
            tabPageTeam.Controls.Add(lblServerDetails);
            tabPageTeam.Controls.Add(btnCheckServer);
            tabPageTeam.Controls.Add(lblBasicNote);
            tabPageTeam.Controls.Add(txtMessage);
            tabPageTeam.Controls.Add(btnSend);
            tabPageTeam.Controls.Add(btnSendMyLog);
            tabPageTeam.Controls.Add(lblTeamActivity);
            tabPageTeam.Controls.Add(btnRefreshTeam);
            tabPageTeam.Controls.Add(dgvTeam);
            tabPageTeam.Controls.Add(lblSessionDetails);
            tabPageTeam.Controls.Add(dgvSessionEntries);
            tabPageTeam.Controls.Add(lblTeamStatus);
            tabPageTeam.Location = new Point(4, 24);
            tabPageTeam.Name = "tabPageTeam";
            tabPageTeam.Padding = new Padding(3);
            tabPageTeam.Size = new Size(892, 632);
            tabPageTeam.TabIndex = 0;
            tabPageTeam.Text = "Tim";
            tabPageTeam.UseVisualStyleBackColor = true;
            // 
            // lblServerStatus
            // 
            lblServerStatus.AutoSize = true;
            lblServerStatus.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblServerStatus.Location = new Point(18, 15);
            lblServerStatus.Name = "lblServerStatus";
            lblServerStatus.Size = new Size(120, 20);
            lblServerStatus.TabIndex = 0;
            lblServerStatus.Text = "Server: provjera...";
            // 
            // lblServerDetails
            // 
            lblServerDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblServerDetails.ForeColor = SystemColors.GrayText;
            lblServerDetails.Location = new Point(20, 40);
            lblServerDetails.Name = "lblServerDetails";
            lblServerDetails.Size = new Size(720, 18);
            lblServerDetails.TabIndex = 1;
            // 
            // btnCheckServer
            // 
            btnCheckServer.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCheckServer.Location = new Point(752, 15);
            btnCheckServer.Name = "btnCheckServer";
            btnCheckServer.Size = new Size(96, 32);
            btnCheckServer.TabIndex = 2;
            btnCheckServer.Text = "Provjeri server";
            btnCheckServer.UseVisualStyleBackColor = true;
            btnCheckServer.Click += btnCheckServer_Click;
            // 
            // lblBasicNote
            // 
            lblBasicNote.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblBasicNote.Font = new Font("Segoe UI", 9.75F, FontStyle.Italic);
            lblBasicNote.ForeColor = SystemColors.GrayText;
            lblBasicNote.Location = new Point(20, 62);
            lblBasicNote.Name = "lblBasicNote";
            lblBasicNote.Size = new Size(850, 18);
            lblBasicNote.TabIndex = 3;
            lblBasicNote.Text = "Osnovna verzija: server radi na ovom računalu. Timski rad u uredu planiran je za sljedeće izdanje.";
            // 
            // txtMessage
            // 
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMessage.Location = new Point(20, 97);
            txtMessage.MaxLength = 500;
            txtMessage.Name = "txtMessage";
            txtMessage.PlaceholderText = "Poruka timu (najviše 500 znakova)";
            txtMessage.Size = new Size(560, 23);
            txtMessage.TabIndex = 4;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.Enabled = false;
            btnSend.Location = new Point(592, 93);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(96, 30);
            btnSend.TabIndex = 5;
            btnSend.Text = "Pošalji poruku";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnSendMyLog
            // 
            btnSendMyLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSendMyLog.Enabled = false;
            btnSendMyLog.Location = new Point(722, 93);
            btnSendMyLog.Name = "btnSendMyLog";
            btnSendMyLog.Size = new Size(110, 30);
            btnSendMyLog.TabIndex = 6;
            btnSendMyLog.Text = "Pošalji moj dnevnik";
            btnSendMyLog.UseVisualStyleBackColor = true;
            btnSendMyLog.Click += btnSendMyLog_Click;
            // 
            // lblTeamActivity
            // 
            lblTeamActivity.AutoSize = true;
            lblTeamActivity.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTeamActivity.Location = new Point(18, 142);
            lblTeamActivity.Name = "lblTeamActivity";
            lblTeamActivity.Size = new Size(110, 19);
            lblTeamActivity.TabIndex = 7;
            lblTeamActivity.Text = "Aktivnost tima";
            // 
            // btnRefreshTeam
            // 
            btnRefreshTeam.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefreshTeam.Enabled = false;
            btnRefreshTeam.Location = new Point(752, 136);
            btnRefreshTeam.Name = "btnRefreshTeam";
            btnRefreshTeam.Size = new Size(88, 30);
            btnRefreshTeam.TabIndex = 8;
            btnRefreshTeam.Text = "Osvježi";
            btnRefreshTeam.UseVisualStyleBackColor = true;
            btnRefreshTeam.Click += btnRefreshTeam_Click;
            // 
            // dgvTeam
            // 
            dgvTeam.AllowUserToAddRows = false;
            dgvTeam.AllowUserToDeleteRows = false;
            dgvTeam.AllowUserToResizeRows = false;
            dgvTeam.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvTeam.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvTeam.Location = new Point(20, 172);
            dgvTeam.MultiSelect = false;
            dgvTeam.Name = "dgvTeam";
            dgvTeam.ReadOnly = true;
            dgvTeam.RowHeadersVisible = false;
            dgvTeam.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTeam.Size = new Size(852, 230);
            dgvTeam.TabIndex = 9;
            dgvTeam.SelectionChanged += dgvTeam_SelectionChanged;
            // 
            // lblSessionDetails
            // 
            lblSessionDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSessionDetails.ForeColor = SystemColors.GrayText;
            lblSessionDetails.Location = new Point(20, 412);
            lblSessionDetails.Name = "lblSessionDetails";
            lblSessionDetails.Size = new Size(852, 18);
            lblSessionDetails.TabIndex = 10;
            lblSessionDetails.Text = "Odaberite dnevnik u tablici za prikaz njegovih zapisa.";
            // 
            // dgvSessionEntries
            // 
            dgvSessionEntries.AllowUserToAddRows = false;
            dgvSessionEntries.AllowUserToDeleteRows = false;
            dgvSessionEntries.AllowUserToResizeRows = false;
            dgvSessionEntries.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvSessionEntries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSessionEntries.Location = new Point(20, 434);
            dgvSessionEntries.Name = "dgvSessionEntries";
            dgvSessionEntries.ReadOnly = true;
            dgvSessionEntries.RowHeadersVisible = false;
            dgvSessionEntries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSessionEntries.ShowCellToolTips = true;
            dgvSessionEntries.Size = new Size(852, 160);
            dgvSessionEntries.TabIndex = 11;
            dgvSessionEntries.CellFormatting += LogGrid_CellFormatting;
            dgvSessionEntries.CellToolTipTextNeeded += LogGrid_CellToolTipTextNeeded;
            // 
            // lblTeamStatus
            // 
            lblTeamStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblTeamStatus.ForeColor = SystemColors.GrayText;
            lblTeamStatus.Location = new Point(20, 604);
            lblTeamStatus.Name = "lblTeamStatus";
            lblTeamStatus.Size = new Size(852, 20);
            lblTeamStatus.TabIndex = 12;
            // 
            // tabPageLog
            // 
            tabPageLog.Controls.Add(lblLogPeriod);
            tabPageLog.Controls.Add(cmbLogPeriod);
            tabPageLog.Controls.Add(chkOnlyErrors);
            tabPageLog.Controls.Add(lblSource);
            tabPageLog.Controls.Add(cmbSource);
            tabPageLog.Controls.Add(lblLogSearch);
            tabPageLog.Controls.Add(txtLogSearch);
            tabPageLog.Controls.Add(dgvLog);
            tabPageLog.Controls.Add(lblLogCount);
            tabPageLog.Controls.Add(btnRefreshLog);
            tabPageLog.Controls.Add(btnExportLog);
            tabPageLog.Location = new Point(4, 24);
            tabPageLog.Name = "tabPageLog";
            tabPageLog.Padding = new Padding(3);
            tabPageLog.Size = new Size(892, 632);
            tabPageLog.TabIndex = 1;
            tabPageLog.Text = "Dnevnik aktivnosti";
            tabPageLog.UseVisualStyleBackColor = true;
            // 
            // lblLogPeriod
            // 
            lblLogPeriod.AutoSize = true;
            lblLogPeriod.Location = new Point(20, 14);
            lblLogPeriod.Name = "lblLogPeriod";
            lblLogPeriod.Size = new Size(44, 15);
            lblLogPeriod.TabIndex = 0;
            lblLogPeriod.Text = "Period";
            // 
            // cmbLogPeriod
            // 
            cmbLogPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLogPeriod.Location = new Point(20, 34);
            cmbLogPeriod.Name = "cmbLogPeriod";
            cmbLogPeriod.Size = new Size(140, 23);
            cmbLogPeriod.TabIndex = 1;
            cmbLogPeriod.SelectedIndexChanged += LogFilter_Changed;
            // 
            // chkOnlyErrors
            // 
            chkOnlyErrors.AutoSize = true;
            chkOnlyErrors.Location = new Point(175, 36);
            chkOnlyErrors.Name = "chkOnlyErrors";
            chkOnlyErrors.Size = new Size(95, 19);
            chkOnlyErrors.TabIndex = 2;
            chkOnlyErrors.Text = "Samo greške";
            chkOnlyErrors.UseVisualStyleBackColor = true;
            chkOnlyErrors.CheckedChanged += LogFilter_Changed;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Location = new Point(290, 14);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(34, 15);
            lblSource.TabIndex = 3;
            lblSource.Text = "Izvor";
            // 
            // cmbSource
            // 
            cmbSource.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSource.Location = new Point(290, 34);
            cmbSource.Name = "cmbSource";
            cmbSource.Size = new Size(190, 23);
            cmbSource.TabIndex = 4;
            cmbSource.SelectedIndexChanged += LogFilter_Changed;
            // 
            // lblLogSearch
            // 
            lblLogSearch.AutoSize = true;
            lblLogSearch.Location = new Point(495, 14);
            lblLogSearch.Name = "lblLogSearch";
            lblLogSearch.Size = new Size(52, 15);
            lblLogSearch.TabIndex = 5;
            lblLogSearch.Text = "Pretraga";
            // 
            // txtLogSearch
            // 
            txtLogSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLogSearch.Location = new Point(495, 34);
            txtLogSearch.Name = "txtLogSearch";
            txtLogSearch.PlaceholderText = "Radnja ili detalji...";
            txtLogSearch.Size = new Size(377, 23);
            txtLogSearch.TabIndex = 6;
            txtLogSearch.TextChanged += txtLogSearch_TextChanged;
            // 
            // dgvLog
            // 
            dgvLog.AllowUserToAddRows = false;
            dgvLog.AllowUserToDeleteRows = false;
            dgvLog.AllowUserToResizeRows = false;
            dgvLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLog.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLog.Location = new Point(20, 70);
            dgvLog.Name = "dgvLog";
            dgvLog.ReadOnly = true;
            dgvLog.RowHeadersVisible = false;
            dgvLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLog.ShowCellToolTips = true;
            dgvLog.Size = new Size(852, 505);
            dgvLog.TabIndex = 7;
            dgvLog.CellFormatting += LogGrid_CellFormatting;
            dgvLog.CellToolTipTextNeeded += LogGrid_CellToolTipTextNeeded;
            // 
            // lblLogCount
            // 
            lblLogCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblLogCount.ForeColor = SystemColors.GrayText;
            lblLogCount.Location = new Point(20, 592);
            lblLogCount.Name = "lblLogCount";
            lblLogCount.Size = new Size(590, 20);
            lblLogCount.TabIndex = 8;
            // 
            // btnRefreshLog
            // 
            btnRefreshLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRefreshLog.Location = new Point(632, 585);
            btnRefreshLog.Name = "btnRefreshLog";
            btnRefreshLog.Size = new Size(115, 32);
            btnRefreshLog.TabIndex = 9;
            btnRefreshLog.Text = "Osvježi";
            btnRefreshLog.UseVisualStyleBackColor = true;
            btnRefreshLog.Click += btnRefreshLog_Click;
            // 
            // btnExportLog
            // 
            btnExportLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportLog.Enabled = false;
            btnExportLog.Location = new Point(757, 585);
            btnExportLog.Name = "btnExportLog";
            btnExportLog.Size = new Size(115, 32);
            btnExportLog.TabIndex = 10;
            btnExportLog.Text = "Izvezi CSV";
            btnExportLog.UseVisualStyleBackColor = true;
            btnExportLog.Click += btnExportLog_Click;
            // 
            // TeamStatusForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 660);
            Controls.Add(tabControl);
            MinimumSize = new Size(760, 600);
            Name = "TeamStatusForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tim i aktivnost";
            FormClosed += TeamStatusForm_FormClosed;
            Load += TeamStatusForm_Load;
            tabControl.ResumeLayout(false);
            tabPageTeam.ResumeLayout(false);
            tabPageTeam.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTeam).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvSessionEntries).EndInit();
            tabPageLog.ResumeLayout(false);
            tabPageLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLog).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabPageTeam;
        private Label lblServerStatus;
        private Label lblServerDetails;
        private Button btnCheckServer;
        private Label lblBasicNote;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnSendMyLog;
        private Label lblTeamActivity;
        private Button btnRefreshTeam;
        private DataGridView dgvTeam;
        private Label lblSessionDetails;
        private DataGridView dgvSessionEntries;
        private Label lblTeamStatus;
        private TabPage tabPageLog;
        private Label lblLogPeriod;
        private ComboBox cmbLogPeriod;
        private CheckBox chkOnlyErrors;
        private Label lblSource;
        private ComboBox cmbSource;
        private Label lblLogSearch;
        private TextBox txtLogSearch;
        private DataGridView dgvLog;
        private Label lblLogCount;
        private Button btnRefreshLog;
        private Button btnExportLog;
    }
}