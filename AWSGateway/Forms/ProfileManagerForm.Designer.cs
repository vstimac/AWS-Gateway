namespace AWSGateway.Forms
{
    partial class ProfileManagerForm
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
            lblTitle = new Label();
            lblSubtitle = new Label();
            lstProfiles = new DataGridView();
            colName = new DataGridViewTextBoxColumn();
            colMode = new DataGridViewTextBoxColumn();
            colRegion = new DataGridViewTextBoxColumn();
            colAccessKey = new DataGridViewTextBoxColumn();
            colBucket = new DataGridViewTextBoxColumn();
            colOwner = new DataGridViewTextBoxColumn();
            btnNew = new Button();
            btnDelete = new Button();
            grpEdit = new GroupBox();
            lblName = new Label();
            txtName = new TextBox();
            lblMode = new Label();
            rbAccessKey = new RadioButton();
            rbAssumeRole = new RadioButton();
            lblAccessKey = new Label();
            txtAccessKey = new TextBox();
            lblRegion = new Label();
            cmbRegion = new ComboBox();
            lblRoleArn = new Label();
            txtRoleArn = new TextBox();
            lblMfaSerial = new Label();
            txtMfaSerial = new TextBox();
            lblDefaultBucket = new Label();
            txtDefaultBucket = new TextBox();
            btnSave = new Button();
            btnCancelEdit = new Button();
            lblStatus = new Label();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)lstProfiles).BeginInit();
            grpEdit.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(129, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "AWS profili";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.ForeColor = SystemColors.GrayText;
            lblSubtitle.Location = new Point(22, 48);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(454, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Profil sprema postavke prijave. Tajni ključ se ne sprema - upisuje se kod svake prijave.";
            // 
            // lstProfiles
            // 
            lstProfiles.AllowUserToAddRows = false;
            lstProfiles.AllowUserToDeleteRows = false;
            lstProfiles.AllowUserToResizeRows = false;
            lstProfiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstProfiles.ColumnHeadersHeight = 32;
            lstProfiles.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            lstProfiles.Columns.AddRange(new DataGridViewColumn[] { colName, colMode, colRegion, colAccessKey, colBucket, colOwner });
            lstProfiles.Location = new Point(20, 75);
            lstProfiles.MultiSelect = false;
            lstProfiles.Name = "lstProfiles";
            lstProfiles.ReadOnly = true;
            lstProfiles.RowHeadersVisible = false;
            lstProfiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            lstProfiles.Size = new Size(720, 190);
            lstProfiles.TabIndex = 2;
            lstProfiles.SelectionChanged += lstProfiles_SelectedIndexChanged;
            // 
            // colName
            // 
            colName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colName.HeaderText = "Naziv";
            colName.MinimumWidth = 180;
            colName.Name = "colName";
            colName.ReadOnly = true;
            // 
            // colMode
            // 
            colMode.HeaderText = "Način prijave";
            colMode.Name = "colMode";
            colMode.ReadOnly = true;
            colMode.Width = 125;
            // 
            // colRegion
            // 
            colRegion.HeaderText = "Regija";
            colRegion.Name = "colRegion";
            colRegion.ReadOnly = true;
            colRegion.Width = 120;
            // 
            // colAccessKey
            // 
            colAccessKey.HeaderText = "Access key";
            colAccessKey.Name = "colAccessKey";
            colAccessKey.ReadOnly = true;
            colAccessKey.Width = 135;
            // 
            // colBucket
            // 
            colBucket.HeaderText = "Zadani bucket";
            colBucket.Name = "colBucket";
            colBucket.ReadOnly = true;
            colBucket.Width = 150;
            // 
            // colOwner
            // 
            colOwner.HeaderText = "Vlasnik";
            colOwner.Name = "colOwner";
            colOwner.ReadOnly = true;
            colOwner.Width = 110;
            // 
            // btnNew
            // 
            btnNew.Location = new Point(20, 275);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(130, 36);
            btnNew.TabIndex = 3;
            btnNew.Text = "Novi profil";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += btnNew_Click;
            // 
            // btnDelete
            // 
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(158, 275);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(130, 36);
            btnDelete.TabIndex = 4;
            btnDelete.Text = "Obriši";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // grpEdit
            // 
            grpEdit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpEdit.Controls.Add(lblName);
            grpEdit.Controls.Add(txtName);
            grpEdit.Controls.Add(lblMode);
            grpEdit.Controls.Add(rbAccessKey);
            grpEdit.Controls.Add(rbAssumeRole);
            grpEdit.Controls.Add(lblAccessKey);
            grpEdit.Controls.Add(txtAccessKey);
            grpEdit.Controls.Add(lblRegion);
            grpEdit.Controls.Add(cmbRegion);
            grpEdit.Controls.Add(lblRoleArn);
            grpEdit.Controls.Add(txtRoleArn);
            grpEdit.Controls.Add(lblMfaSerial);
            grpEdit.Controls.Add(txtMfaSerial);
            grpEdit.Controls.Add(lblDefaultBucket);
            grpEdit.Controls.Add(txtDefaultBucket);
            grpEdit.Controls.Add(btnSave);
            grpEdit.Controls.Add(btnCancelEdit);
            grpEdit.Location = new Point(20, 361);
            grpEdit.Name = "grpEdit";
            grpEdit.Size = new Size(720, 265);
            grpEdit.TabIndex = 5;
            grpEdit.TabStop = false;
            grpEdit.Text = "Novi profil";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(15, 32);
            lblName.Name = "lblName";
            lblName.Size = new Size(39, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Naziv:";
            // 
            // txtName
            // 
            txtName.Location = new Point(160, 28);
            txtName.MaxLength = 50;
            txtName.Name = "txtName";
            txtName.Size = new Size(300, 23);
            txtName.TabIndex = 1;
            // 
            // lblMode
            // 
            lblMode.AutoSize = true;
            lblMode.Location = new Point(15, 64);
            lblMode.Name = "lblMode";
            lblMode.Size = new Size(79, 15);
            lblMode.TabIndex = 2;
            lblMode.Text = "Način prijave:";
            // 
            // rbAccessKey
            // 
            rbAccessKey.AutoSize = true;
            rbAccessKey.Checked = true;
            rbAccessKey.Location = new Point(160, 62);
            rbAccessKey.Name = "rbAccessKey";
            rbAccessKey.Size = new Size(100, 19);
            rbAccessKey.TabIndex = 3;
            rbAccessKey.TabStop = true;
            rbAccessKey.Text = "Pristupni ključ";
            rbAccessKey.UseVisualStyleBackColor = true;
            rbAccessKey.CheckedChanged += LoginMode_CheckedChanged;
            // 
            // rbAssumeRole
            // 
            rbAssumeRole.AutoSize = true;
            rbAssumeRole.Location = new Point(300, 62);
            rbAssumeRole.Name = "rbAssumeRole";
            rbAssumeRole.Size = new Size(199, 19);
            rbAssumeRole.TabIndex = 4;
            rbAssumeRole.Text = "Preuzimanje uloge (AssumeRole)";
            rbAssumeRole.UseVisualStyleBackColor = true;
            rbAssumeRole.CheckedChanged += LoginMode_CheckedChanged;
            // 
            // lblAccessKey
            // 
            lblAccessKey.AutoSize = true;
            lblAccessKey.Location = new Point(15, 96);
            lblAccessKey.Name = "lblAccessKey";
            lblAccessKey.Size = new Size(67, 15);
            lblAccessKey.TabIndex = 5;
            lblAccessKey.Text = "Access key:";
            // 
            // txtAccessKey
            // 
            txtAccessKey.CharacterCasing = CharacterCasing.Upper;
            txtAccessKey.Location = new Point(160, 92);
            txtAccessKey.MaxLength = 20;
            txtAccessKey.Name = "txtAccessKey";
            txtAccessKey.PlaceholderText = "AKIA... (opcionalno)";
            txtAccessKey.Size = new Size(300, 23);
            txtAccessKey.TabIndex = 6;
            // 
            // lblRegion
            // 
            lblRegion.AutoSize = true;
            lblRegion.Location = new Point(15, 128);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new Size(42, 15);
            lblRegion.TabIndex = 7;
            lblRegion.Text = "Regija:";
            // 
            // cmbRegion
            // 
            cmbRegion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRegion.Location = new Point(160, 124);
            cmbRegion.Name = "cmbRegion";
            cmbRegion.Size = new Size(300, 23);
            cmbRegion.TabIndex = 8;
            // 
            // lblRoleArn
            // 
            lblRoleArn.AutoSize = true;
            lblRoleArn.Location = new Point(15, 160);
            lblRoleArn.Name = "lblRoleArn";
            lblRoleArn.Size = new Size(67, 15);
            lblRoleArn.TabIndex = 9;
            lblRoleArn.Text = "ARN uloge:";
            // 
            // txtRoleArn
            // 
            txtRoleArn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRoleArn.Location = new Point(160, 156);
            txtRoleArn.Name = "txtRoleArn";
            txtRoleArn.PlaceholderText = "arn:aws:iam::123456789012:role/naziv-uloge";
            txtRoleArn.Size = new Size(545, 23);
            txtRoleArn.TabIndex = 10;
            // 
            // lblMfaSerial
            // 
            lblMfaSerial.AutoSize = true;
            lblMfaSerial.Location = new Point(15, 192);
            lblMfaSerial.Name = "lblMfaSerial";
            lblMfaSerial.Size = new Size(96, 15);
            lblMfaSerial.TabIndex = 11;
            lblMfaSerial.Text = "MFA serijski broj:";
            // 
            // txtMfaSerial
            // 
            txtMfaSerial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMfaSerial.Location = new Point(160, 188);
            txtMfaSerial.Name = "txtMfaSerial";
            txtMfaSerial.PlaceholderText = "opcionalno - arn:aws:iam::123456789012:mfa/naziv-uredaja";
            txtMfaSerial.Size = new Size(545, 23);
            txtMfaSerial.TabIndex = 12;
            // 
            // lblDefaultBucket
            // 
            lblDefaultBucket.AutoSize = true;
            lblDefaultBucket.Location = new Point(15, 224);
            lblDefaultBucket.Name = "lblDefaultBucket";
            lblDefaultBucket.Size = new Size(85, 15);
            lblDefaultBucket.TabIndex = 13;
            lblDefaultBucket.Text = "Zadani bucket:";
            // 
            // txtDefaultBucket
            // 
            txtDefaultBucket.CharacterCasing = CharacterCasing.Lower;
            txtDefaultBucket.Location = new Point(160, 220);
            txtDefaultBucket.MaxLength = 63;
            txtDefaultBucket.Name = "txtDefaultBucket";
            txtDefaultBucket.PlaceholderText = "opcionalno";
            txtDefaultBucket.Size = new Size(300, 23);
            txtDefaultBucket.TabIndex = 14;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(465, 216);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 36);
            btnSave.TabIndex = 15;
            btnSave.Text = "Spremi";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancelEdit
            // 
            btnCancelEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelEdit.Location = new Point(595, 216);
            btnCancelEdit.Name = "btnCancelEdit";
            btnCancelEdit.Size = new Size(120, 36);
            btnCancelEdit.TabIndex = 16;
            btnCancelEdit.Text = "Odustani";
            btnCancelEdit.UseVisualStyleBackColor = true;
            btnCancelEdit.Click += btnCancelEdit_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Location = new Point(17, 651);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(590, 40);
            lblStatus.TabIndex = 6;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(627, 653);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(120, 36);
            btnClose.TabIndex = 7;
            btnClose.Text = "Zatvori";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // ProfileManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(760, 700);
            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lstProfiles);
            Controls.Add(btnNew);
            Controls.Add(btnDelete);
            Controls.Add(grpEdit);
            Controls.Add(lblStatus);
            Controls.Add(btnClose);
            MinimumSize = new Size(700, 684);
            Name = "ProfileManagerForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AWS profili";
            Load += ProfileManagerForm_Load;
            ((System.ComponentModel.ISupportInitialize)lstProfiles).EndInit();
            grpEdit.ResumeLayout(false);
            grpEdit.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Label lblSubtitle;
        private DataGridView lstProfiles;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colMode;
        private DataGridViewTextBoxColumn colRegion;
        private DataGridViewTextBoxColumn colAccessKey;
        private DataGridViewTextBoxColumn colBucket;
        private DataGridViewTextBoxColumn colOwner;
        private Button btnNew;
        private Button btnDelete;
        private GroupBox grpEdit;
        private Label lblName;
        private TextBox txtName;
        private Label lblMode;
        private RadioButton rbAccessKey;
        private RadioButton rbAssumeRole;
        private Label lblAccessKey;
        private TextBox txtAccessKey;
        private Label lblRegion;
        private ComboBox cmbRegion;
        private Label lblRoleArn;
        private TextBox txtRoleArn;
        private Label lblMfaSerial;
        private TextBox txtMfaSerial;
        private Label lblDefaultBucket;
        private TextBox txtDefaultBucket;
        private Button btnSave;
        private Button btnCancelEdit;
        private Label lblStatus;
        private Button btnClose;
    }
}