namespace AWSGateway.Forms
{
    partial class S3BrowserForm
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
                ReleaseTransferResources();

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
            lblBucket = new Label();
            cmbBucket = new ComboBox();
            btnRefresh = new Button();
            lblBucketInfo = new Label();
            lblList = new Label();
            txtFilter = new TextBox();
            lstObjects = new DataGridView();
            colName = new DataGridViewTextBoxColumn();
            colSize = new DataGridViewTextBoxColumn();
            colStorageClass = new DataGridViewTextBoxColumn();
            colModified = new DataGridViewTextBoxColumn();
            lblObjectsEmpty = new Label();
            btnDownload = new Button();
            cmbUrlDuration = new ComboBox();
            btnGenerateUrl = new Button();
            btnDelete = new Button();
            txtUrl = new TextBox();
            btnCopyUrl = new Button();
            lblUrlInfo = new Label();
            grpTransfer = new GroupBox();
            btnUpload = new Button();
            btnCancel = new Button();
            lblTransferSummary = new Label();
            btnAdvanced = new Button();
            pnlAdvanced = new Panel();
            lblPartSize = new Label();
            numPartSize = new NumericUpDown();
            lblFileConcurrency = new Label();
            numFileConcurrency = new NumericUpDown();
            lblPartConcurrency = new Label();
            numPartConcurrency = new NumericUpDown();
            lblConcurrencyHint = new Label();
            lstTransfers = new DataGridView();
            colTransferName = new DataGridViewTextBoxColumn();
            colTransferSize = new DataGridViewTextBoxColumn();
            colTransferPercent = new DataGridViewTextBoxColumn();
            colTransferStatus = new DataGridViewTextBoxColumn();
            progressBar1 = new ProgressBar();
            ((System.ComponentModel.ISupportInitialize)lstObjects).BeginInit();
            grpTransfer.SuspendLayout();
            pnlAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPartSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFileConcurrency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPartConcurrency).BeginInit();
            ((System.ComponentModel.ISupportInitialize)lstTransfers).BeginInit();
            SuspendLayout();
            // 
            // lblBucket
            // 
            lblBucket.AutoSize = true;
            lblBucket.Font = new Font("Segoe UI", 9.75F);
            lblBucket.Location = new Point(20, 23);
            lblBucket.Name = "lblBucket";
            lblBucket.Size = new Size(53, 19);
            lblBucket.TabIndex = 0;
            lblBucket.Text = "Bucket:";
            // 
            // cmbBucket
            // 
            cmbBucket.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbBucket.FormattingEnabled = true;
            cmbBucket.Location = new Point(99, 20);
            cmbBucket.Name = "cmbBucket";
            cmbBucket.Size = new Size(510, 23);
            cmbBucket.TabIndex = 1;
            cmbBucket.SelectionChangeCommitted += cmbBucket_SelectionChangeCommitted;
            cmbBucket.KeyDown += cmbBucket_KeyDown;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Location = new Point(629, 14);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(146, 35);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "Osvježi";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblBucketInfo
            // 
            lblBucketInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblBucketInfo.ForeColor = SystemColors.GrayText;
            lblBucketInfo.Location = new Point(20, 55);
            lblBucketInfo.Name = "lblBucketInfo";
            lblBucketInfo.Size = new Size(755, 20);
            lblBucketInfo.TabIndex = 3;
            // 
            // lblList
            // 
            lblList.AutoSize = true;
            lblList.Font = new Font("Segoe UI", 9.75F);
            lblList.Location = new Point(20, 85);
            lblList.Name = "lblList";
            lblList.Size = new Size(56, 19);
            lblList.TabIndex = 4;
            lblList.Text = "Objekti:";
            // 
            // txtFilter
            // 
            txtFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtFilter.Location = new Point(555, 82);
            txtFilter.Name = "txtFilter";
            txtFilter.PlaceholderText = "Filtriraj po nazivu...";
            txtFilter.Size = new Size(220, 23);
            txtFilter.TabIndex = 5;
            txtFilter.TextChanged += txtFilter_TextChanged;
            // 
            // lstObjects
            // 
            lstObjects.AllowUserToAddRows = false;
            lstObjects.AllowUserToDeleteRows = false;
            lstObjects.AllowUserToResizeRows = false;
            lstObjects.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstObjects.Columns.AddRange(new DataGridViewColumn[] { colName, colSize, colStorageClass, colModified });
            lstObjects.Location = new Point(20, 110);
            lstObjects.Name = "lstObjects";
            lstObjects.ReadOnly = true;
            lstObjects.RowHeadersVisible = false;
            lstObjects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            lstObjects.Size = new Size(755, 250);
            lstObjects.TabIndex = 6;
            // 
            // colName
            // 
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colName.HeaderText = "Naziv";
            colName.MinimumWidth = 180;
            colName.Name = "colName";
            colName.ReadOnly = true;
            // 
            // colSize
            // 
            colSize.HeaderText = "Veličina";
            colSize.Name = "colSize";
            colSize.ReadOnly = true;
            colSize.Width = 95;
            // 
            // colStorageClass
            // 
            colStorageClass.HeaderText = "Klasa pohrane";
            colStorageClass.Name = "colStorageClass";
            colStorageClass.ReadOnly = true;
            colStorageClass.Width = 150;
            // 
            // colModified
            // 
            colModified.HeaderText = "Zadnja izmjena";
            colModified.Name = "colModified";
            colModified.ReadOnly = true;
            colModified.Width = 145;
            // 
            // lblObjectsEmpty
            // 
            lblObjectsEmpty.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblObjectsEmpty.Location = new Point(20, 110);
            lblObjectsEmpty.Name = "lblObjectsEmpty";
            lblObjectsEmpty.Size = new Size(755, 250);
            lblObjectsEmpty.TabIndex = 6;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(20, 370);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(135, 36);
            btnDownload.TabIndex = 7;
            btnDownload.Text = "Download";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // cmbUrlDuration
            // 
            cmbUrlDuration.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbUrlDuration.FormattingEnabled = true;
            cmbUrlDuration.Location = new Point(165, 376);
            cmbUrlDuration.Name = "cmbUrlDuration";
            cmbUrlDuration.Size = new Size(150, 23);
            cmbUrlDuration.TabIndex = 8;
            // 
            // btnGenerateUrl
            // 
            btnGenerateUrl.Location = new Point(325, 370);
            btnGenerateUrl.Name = "btnGenerateUrl";
            btnGenerateUrl.Size = new Size(170, 36);
            btnGenerateUrl.TabIndex = 9;
            btnGenerateUrl.Text = "Generiraj poveznicu";
            btnGenerateUrl.UseVisualStyleBackColor = true;
            btnGenerateUrl.Click += btnGenerateUrl_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(645, 370);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(130, 36);
            btnDelete.TabIndex = 10;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // txtUrl
            // 
            txtUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtUrl.Location = new Point(20, 413);
            txtUrl.Name = "txtUrl";
            txtUrl.PlaceholderText = "Generirana poveznica";
            txtUrl.ReadOnly = true;
            txtUrl.Size = new Size(755, 23);
            txtUrl.TabIndex = 11;
            // 
            // btnCopyUrl
            // 
            btnCopyUrl.Location = new Point(20, 441);
            btnCopyUrl.Name = "btnCopyUrl";
            btnCopyUrl.Size = new Size(120, 32);
            btnCopyUrl.TabIndex = 12;
            btnCopyUrl.Text = "Kopiraj";
            btnCopyUrl.UseVisualStyleBackColor = true;
            btnCopyUrl.Click += btnCopyUrl_Click;
            // 
            // lblUrlInfo
            // 
            lblUrlInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblUrlInfo.ForeColor = SystemColors.GrayText;
            lblUrlInfo.Location = new Point(150, 447);
            lblUrlInfo.Name = "lblUrlInfo";
            lblUrlInfo.Size = new Size(653, 18);
            lblUrlInfo.TabIndex = 13;
            // 
            // grpTransfer
            // 
            grpTransfer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpTransfer.Controls.Add(btnUpload);
            grpTransfer.Controls.Add(btnCancel);
            grpTransfer.Controls.Add(lblTransferSummary);
            grpTransfer.Controls.Add(btnAdvanced);
            grpTransfer.Controls.Add(pnlAdvanced);
            grpTransfer.Controls.Add(lstTransfers);
            grpTransfer.Controls.Add(progressBar1);
            grpTransfer.Location = new Point(20, 479);
            grpTransfer.Name = "grpTransfer";
            grpTransfer.Size = new Size(755, 369);
            grpTransfer.TabIndex = 13;
            grpTransfer.TabStop = false;
            grpTransfer.Text = "Prijenos";
            // 
            // btnUpload
            // 
            btnUpload.Location = new Point(15, 25);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(130, 36);
            btnUpload.TabIndex = 7;
            btnUpload.Text = "Upload";
            btnUpload.UseVisualStyleBackColor = true;
            btnUpload.Click += btnUpload_Click;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(153, 25);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(130, 36);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblTransferSummary
            // 
            lblTransferSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTransferSummary.Location = new Point(289, 35);
            lblTransferSummary.Name = "lblTransferSummary";
            lblTransferSummary.Size = new Size(260, 20);
            lblTransferSummary.TabIndex = 9;
            // 
            // btnAdvanced
            // 
            btnAdvanced.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdvanced.Location = new Point(560, 25);
            btnAdvanced.Name = "btnAdvanced";
            btnAdvanced.Size = new Size(180, 35);
            btnAdvanced.TabIndex = 12;
            btnAdvanced.Text = "Napredne postavke ▸";
            btnAdvanced.UseVisualStyleBackColor = true;
            btnAdvanced.Click += btnAdvanced_Click;
            // 
            // pnlAdvanced
            // 
            pnlAdvanced.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlAdvanced.Controls.Add(lblPartSize);
            pnlAdvanced.Controls.Add(numPartSize);
            pnlAdvanced.Controls.Add(lblFileConcurrency);
            pnlAdvanced.Controls.Add(numFileConcurrency);
            pnlAdvanced.Controls.Add(lblPartConcurrency);
            pnlAdvanced.Controls.Add(numPartConcurrency);
            pnlAdvanced.Controls.Add(lblConcurrencyHint);
            pnlAdvanced.Location = new Point(15, 68);
            pnlAdvanced.Name = "pnlAdvanced";
            pnlAdvanced.Size = new Size(725, 88);
            pnlAdvanced.TabIndex = 13;
            pnlAdvanced.Visible = false;
            // 
            // lblPartSize
            // 
            lblPartSize.AutoSize = true;
            lblPartSize.Location = new Point(0, 5);
            lblPartSize.Name = "lblPartSize";
            lblPartSize.Size = new Size(160, 15);
            lblPartSize.TabIndex = 0;
            lblPartSize.Text = "Veličina dijela (MB, 0 = auto):";
            // 
            // numPartSize
            // 
            numPartSize.Location = new Point(215, 2);
            numPartSize.Maximum = new decimal(new int[] { 5120, 0, 0, 0 });
            numPartSize.Name = "numPartSize";
            numPartSize.Size = new Size(70, 23);
            numPartSize.TabIndex = 1;
            numPartSize.ValueChanged += numConcurrency_ValueChanged;
            // 
            // lblFileConcurrency
            // 
            lblFileConcurrency.AutoSize = true;
            lblFileConcurrency.Location = new Point(0, 37);
            lblFileConcurrency.Name = "lblFileConcurrency";
            lblFileConcurrency.Size = new Size(126, 15);
            lblFileConcurrency.TabIndex = 2;
            lblFileConcurrency.Text = "Datoteka istovremeno:";
            // 
            // numFileConcurrency
            // 
            numFileConcurrency.Location = new Point(215, 34);
            numFileConcurrency.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numFileConcurrency.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numFileConcurrency.Name = "numFileConcurrency";
            numFileConcurrency.Size = new Size(70, 23);
            numFileConcurrency.TabIndex = 3;
            numFileConcurrency.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numFileConcurrency.ValueChanged += numConcurrency_ValueChanged;
            // 
            // lblPartConcurrency
            // 
            lblPartConcurrency.AutoSize = true;
            lblPartConcurrency.Location = new Point(315, 37);
            lblPartConcurrency.Name = "lblPartConcurrency";
            lblPartConcurrency.Size = new Size(188, 15);
            lblPartConcurrency.TabIndex = 4;
            lblPartConcurrency.Text = "Zahtjeva po datoteci (0 = SDK, 10):";
            // 
            // numPartConcurrency
            // 
            numPartConcurrency.Location = new Point(530, 34);
            numPartConcurrency.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            numPartConcurrency.Name = "numPartConcurrency";
            numPartConcurrency.Size = new Size(70, 23);
            numPartConcurrency.TabIndex = 5;
            numPartConcurrency.ValueChanged += numConcurrency_ValueChanged;
            // 
            // lblConcurrencyHint
            // 
            lblConcurrencyHint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblConcurrencyHint.ForeColor = SystemColors.GrayText;
            lblConcurrencyHint.Location = new Point(0, 67);
            lblConcurrencyHint.Name = "lblConcurrencyHint";
            lblConcurrencyHint.Size = new Size(725, 18);
            lblConcurrencyHint.TabIndex = 6;
            // 
            // lstTransfers
            // 
            lstTransfers.AllowUserToAddRows = false;
            lstTransfers.AllowUserToDeleteRows = false;
            lstTransfers.AllowUserToResizeRows = false;
            lstTransfers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstTransfers.Columns.AddRange(new DataGridViewColumn[] { colTransferName, colTransferSize, colTransferPercent, colTransferStatus });
            lstTransfers.Location = new Point(15, 68);
            lstTransfers.MultiSelect = false;
            lstTransfers.Name = "lstTransfers";
            lstTransfers.ReadOnly = true;
            lstTransfers.RowHeadersVisible = false;
            lstTransfers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            lstTransfers.Size = new Size(725, 251);
            lstTransfers.TabIndex = 10;
            // 
            // colTransferName
            // 
            colTransferName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colTransferName.HeaderText = "Datoteka";
            colTransferName.MinimumWidth = 180;
            colTransferName.Name = "colTransferName";
            colTransferName.ReadOnly = true;
            // 
            // colTransferSize
            // 
            colTransferSize.HeaderText = "Veličina";
            colTransferSize.Name = "colTransferSize";
            colTransferSize.ReadOnly = true;
            colTransferSize.Width = 90;
            // 
            // colTransferPercent
            // 
            colTransferPercent.HeaderText = "Napredak";
            colTransferPercent.Name = "colTransferPercent";
            colTransferPercent.ReadOnly = true;
            colTransferPercent.Width = 80;
            // 
            // colTransferStatus
            // 
            colTransferStatus.HeaderText = "Status";
            colTransferStatus.Name = "colTransferStatus";
            colTransferStatus.ReadOnly = true;
            colTransferStatus.Width = 220;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(15, 327);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(725, 25);
            progressBar1.TabIndex = 11;
            // 
            // S3BrowserForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 852);
            Controls.Add(lblBucket);
            Controls.Add(cmbBucket);
            Controls.Add(btnRefresh);
            Controls.Add(lblBucketInfo);
            Controls.Add(lblList);
            Controls.Add(txtFilter);
            Controls.Add(lstObjects);
            Controls.Add(lblObjectsEmpty);
            Controls.Add(btnDownload);
            Controls.Add(cmbUrlDuration);
            Controls.Add(btnGenerateUrl);
            Controls.Add(btnDelete);
            Controls.Add(txtUrl);
            Controls.Add(btnCopyUrl);
            Controls.Add(lblUrlInfo);
            Controls.Add(grpTransfer);
            MinimumSize = new Size(816, 760);
            Name = "S3BrowserForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "S3";
            FormClosing += S3BrowserForm_FormClosing;
            FormClosed += S3BrowserForm_FormClosed;
            Load += S3BrowserForm_Load;
            ((System.ComponentModel.ISupportInitialize)lstObjects).EndInit();
            grpTransfer.ResumeLayout(false);
            pnlAdvanced.ResumeLayout(false);
            pnlAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPartSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFileConcurrency).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPartConcurrency).EndInit();
            ((System.ComponentModel.ISupportInitialize)lstTransfers).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBucket;
        private ComboBox cmbBucket;
        private Button btnRefresh;
        private Label lblBucketInfo;
        private Label lblList;
        private TextBox txtFilter;
        private DataGridView lstObjects;
        private Label lblObjectsEmpty;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colSize;
        private DataGridViewTextBoxColumn colStorageClass;
        private DataGridViewTextBoxColumn colModified;
        private Button btnDownload;
        private ComboBox cmbUrlDuration;
        private Button btnGenerateUrl;
        private Button btnDelete;
        private TextBox txtUrl;
        private Button btnCopyUrl;
        private Label lblUrlInfo;
        private GroupBox grpTransfer;
        private Button btnAdvanced;
        private Panel pnlAdvanced;
        private Label lblPartSize;
        private NumericUpDown numPartSize;
        private Label lblFileConcurrency;
        private NumericUpDown numFileConcurrency;
        private Label lblPartConcurrency;
        private NumericUpDown numPartConcurrency;
        private Label lblConcurrencyHint;
        private Button btnUpload;
        private Button btnCancel;
        private Label lblTransferSummary;
        private DataGridView lstTransfers;
        private DataGridViewTextBoxColumn colTransferName;
        private DataGridViewTextBoxColumn colTransferSize;
        private DataGridViewTextBoxColumn colTransferPercent;
        private DataGridViewTextBoxColumn colTransferStatus;
        private ProgressBar progressBar1;
    }
}