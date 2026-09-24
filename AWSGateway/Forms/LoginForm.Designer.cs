namespace AWSGateway.Forms
{
    partial class LoginForm
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
            lblHeader = new Label();
            lblLangHr = new Label();
            lblLangEn = new Label();
            pnlAccountCard = new Panel();
            lblAccountTitle = new Label();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            pnlAwsCard = new Panel();
            lblAwsTitle = new Label();
            lblSavedProfiles = new Label();
            cmbSavedProfiles = new ComboBox();
            lblRegion = new Label();
            cmbRegion = new ComboBox();
            lblRegionInfo = new Label();
            lblAccessKey = new Label();
            txtAccessKey = new TextBox();
            lblSecretKey = new Label();
            txtSecretKey = new TextBox();
            chkUseRole = new CheckBox();
            pnlRoleSection = new Panel();
            lblRoleArn = new Label();
            txtRoleArn = new TextBox();
            lblMfaSerial = new Label();
            txtMfaSerial = new TextBox();
            lblMfaCode = new Label();
            txtMfaCode = new TextBox();
            chkSaveProfile = new CheckBox();
            txtSaveProfileName = new TextBox();
            lblStatus = new Label();
            btnLogin = new Button();
            pnlAccountCard.SuspendLayout();
            pnlAwsCard.SuspendLayout();
            pnlRoleSection.SuspendLayout();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeader.Location = new Point(24, 16);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(300, 37);
            lblHeader.TabIndex = 0;
            lblHeader.Text = "AWS Gateway";
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLangHr
            // 
            lblLangHr.Cursor = Cursors.Hand;
            lblLangHr.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblLangHr.Location = new Point(650, 25);
            lblLangHr.Name = "lblLangHr";
            lblLangHr.Size = new Size(26, 19);
            lblLangHr.TabIndex = 1;
            lblLangHr.Text = "HR";
            lblLangHr.TextAlign = ContentAlignment.MiddleCenter;
            lblLangHr.Click += lblLangHr_Click;
            // 
            // lblLangEn
            // 
            lblLangEn.Cursor = Cursors.Hand;
            lblLangEn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblLangEn.Location = new Point(682, 25);
            lblLangEn.Name = "lblLangEn";
            lblLangEn.Size = new Size(24, 19);
            lblLangEn.TabIndex = 2;
            lblLangEn.Text = "EN";
            lblLangEn.TextAlign = ContentAlignment.MiddleCenter;
            lblLangEn.Click += lblLangEn_Click;
            // 
            // pnlAccountCard
            // 
            pnlAccountCard.Controls.Add(lblAccountTitle);
            pnlAccountCard.Controls.Add(lblUsername);
            pnlAccountCard.Controls.Add(txtUsername);
            pnlAccountCard.Controls.Add(lblPassword);
            pnlAccountCard.Controls.Add(txtPassword);
            pnlAccountCard.Location = new Point(24, 69);
            pnlAccountCard.Name = "pnlAccountCard";
            pnlAccountCard.Size = new Size(682, 128);
            pnlAccountCard.TabIndex = 3;
            pnlAccountCard.Paint += pnlAccountCard_Paint;
            // 
            // lblAccountTitle
            // 
            lblAccountTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblAccountTitle.Location = new Point(16, 16);
            lblAccountTitle.Name = "lblAccountTitle";
            lblAccountTitle.Size = new Size(650, 26);
            lblAccountTitle.TabIndex = 0;
            lblAccountTitle.Text = "Račun aplikacije";
            lblAccountTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("Segoe UI", 9.75F);
            lblUsername.Location = new Point(16, 54);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(234, 23);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "Korisničko ime:";
            lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(256, 54);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(410, 23);
            txtUsername.TabIndex = 2;
            txtUsername.Leave += txtUsername_Leave;
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("Segoe UI", 9.75F);
            lblPassword.Location = new Point(16, 89);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(234, 23);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Lozinka:";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(256, 89);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(410, 23);
            txtPassword.TabIndex = 4;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // pnlAwsCard
            // 
            pnlAwsCard.Controls.Add(lblAwsTitle);
            pnlAwsCard.Controls.Add(lblSavedProfiles);
            pnlAwsCard.Controls.Add(cmbSavedProfiles);
            pnlAwsCard.Controls.Add(lblRegion);
            pnlAwsCard.Controls.Add(cmbRegion);
            pnlAwsCard.Controls.Add(lblRegionInfo);
            pnlAwsCard.Controls.Add(lblAccessKey);
            pnlAwsCard.Controls.Add(txtAccessKey);
            pnlAwsCard.Controls.Add(lblSecretKey);
            pnlAwsCard.Controls.Add(txtSecretKey);
            pnlAwsCard.Controls.Add(chkUseRole);
            pnlAwsCard.Controls.Add(pnlRoleSection);
            pnlAwsCard.Controls.Add(chkSaveProfile);
            pnlAwsCard.Controls.Add(txtSaveProfileName);
            pnlAwsCard.Location = new Point(24, 213);
            pnlAwsCard.Name = "pnlAwsCard";
            pnlAwsCard.Size = new Size(682, 312);
            pnlAwsCard.TabIndex = 4;
            pnlAwsCard.Paint += pnlAwsCard_Paint;
            // 
            // lblAwsTitle
            // 
            lblAwsTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblAwsTitle.Location = new Point(16, 16);
            lblAwsTitle.Name = "lblAwsTitle";
            lblAwsTitle.Size = new Size(650, 26);
            lblAwsTitle.TabIndex = 0;
            lblAwsTitle.Text = "AWS pristup";
            lblAwsTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSavedProfiles
            // 
            lblSavedProfiles.Font = new Font("Segoe UI", 9.75F);
            lblSavedProfiles.Location = new Point(16, 54);
            lblSavedProfiles.Name = "lblSavedProfiles";
            lblSavedProfiles.Size = new Size(234, 23);
            lblSavedProfiles.TabIndex = 1;
            lblSavedProfiles.Text = "Spremljen profil:";
            lblSavedProfiles.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbSavedProfiles
            // 
            cmbSavedProfiles.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSavedProfiles.FormattingEnabled = true;
            cmbSavedProfiles.Location = new Point(256, 54);
            cmbSavedProfiles.Name = "cmbSavedProfiles";
            cmbSavedProfiles.Size = new Size(410, 23);
            cmbSavedProfiles.TabIndex = 2;
            cmbSavedProfiles.SelectedIndexChanged += cmbSavedProfiles_SelectedIndexChanged;
            // 
            // lblRegion
            // 
            lblRegion.Font = new Font("Segoe UI", 9.75F);
            lblRegion.Location = new Point(16, 89);
            lblRegion.Name = "lblRegion";
            lblRegion.Size = new Size(234, 23);
            lblRegion.TabIndex = 3;
            lblRegion.Text = "Regija:";
            lblRegion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cmbRegion
            // 
            cmbRegion.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRegion.FormattingEnabled = true;
            cmbRegion.Location = new Point(256, 89);
            cmbRegion.Name = "cmbRegion";
            cmbRegion.Size = new Size(410, 23);
            cmbRegion.TabIndex = 4;
            cmbRegion.SelectedIndexChanged += cmbRegion_SelectedIndexChanged;
            // 
            // lblRegionInfo
            // 
            lblRegionInfo.AutoEllipsis = true;
            lblRegionInfo.ForeColor = SystemColors.GrayText;
            lblRegionInfo.Location = new Point(256, 118);
            lblRegionInfo.Name = "lblRegionInfo";
            lblRegionInfo.Size = new Size(410, 17);
            lblRegionInfo.TabIndex = 5;
            // 
            // lblAccessKey
            // 
            lblAccessKey.Font = new Font("Segoe UI", 9.75F);
            lblAccessKey.Location = new Point(16, 147);
            lblAccessKey.Name = "lblAccessKey";
            lblAccessKey.Size = new Size(234, 23);
            lblAccessKey.TabIndex = 6;
            lblAccessKey.Text = "Access Key:";
            lblAccessKey.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtAccessKey
            // 
            txtAccessKey.Location = new Point(256, 147);
            txtAccessKey.Name = "txtAccessKey";
            txtAccessKey.Size = new Size(410, 23);
            txtAccessKey.TabIndex = 7;
            // 
            // lblSecretKey
            // 
            lblSecretKey.Font = new Font("Segoe UI", 9.75F);
            lblSecretKey.Location = new Point(16, 182);
            lblSecretKey.Name = "lblSecretKey";
            lblSecretKey.Size = new Size(234, 23);
            lblSecretKey.TabIndex = 8;
            lblSecretKey.Text = "Secret Key:";
            lblSecretKey.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSecretKey
            // 
            txtSecretKey.Location = new Point(256, 182);
            txtSecretKey.Name = "txtSecretKey";
            txtSecretKey.Size = new Size(410, 23);
            txtSecretKey.TabIndex = 9;
            txtSecretKey.UseSystemPasswordChar = true;
            // 
            // chkUseRole
            // 
            chkUseRole.AutoSize = true;
            chkUseRole.Location = new Point(256, 217);
            chkUseRole.Name = "chkUseRole";
            chkUseRole.Size = new Size(194, 19);
            chkUseRole.TabIndex = 10;
            chkUseRole.Text = "Koristi IAM ulogu (AssumeRole)";
            chkUseRole.UseVisualStyleBackColor = true;
            chkUseRole.CheckedChanged += chkUseRole_CheckedChanged;
            // 
            // pnlRoleSection
            // 
            pnlRoleSection.Controls.Add(lblRoleArn);
            pnlRoleSection.Controls.Add(txtRoleArn);
            pnlRoleSection.Controls.Add(lblMfaSerial);
            pnlRoleSection.Controls.Add(txtMfaSerial);
            pnlRoleSection.Controls.Add(lblMfaCode);
            pnlRoleSection.Controls.Add(txtMfaCode);
            pnlRoleSection.Location = new Point(0, 248);
            pnlRoleSection.Name = "pnlRoleSection";
            pnlRoleSection.Size = new Size(682, 93);
            pnlRoleSection.TabIndex = 11;
            pnlRoleSection.Visible = false;
            // 
            // lblRoleArn
            // 
            lblRoleArn.Font = new Font("Segoe UI", 9.75F);
            lblRoleArn.Location = new Point(16, 0);
            lblRoleArn.Name = "lblRoleArn";
            lblRoleArn.Size = new Size(234, 23);
            lblRoleArn.TabIndex = 0;
            lblRoleArn.Text = "ARN uloge:";
            lblRoleArn.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtRoleArn
            // 
            txtRoleArn.Location = new Point(256, 0);
            txtRoleArn.Name = "txtRoleArn";
            txtRoleArn.Size = new Size(410, 23);
            txtRoleArn.TabIndex = 1;
            // 
            // lblMfaSerial
            // 
            lblMfaSerial.Font = new Font("Segoe UI", 9.75F);
            lblMfaSerial.Location = new Point(16, 35);
            lblMfaSerial.Name = "lblMfaSerial";
            lblMfaSerial.Size = new Size(234, 23);
            lblMfaSerial.TabIndex = 2;
            lblMfaSerial.Text = "MFA serijski broj (opcionalno):";
            lblMfaSerial.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMfaSerial
            // 
            txtMfaSerial.Location = new Point(256, 35);
            txtMfaSerial.Name = "txtMfaSerial";
            txtMfaSerial.Size = new Size(410, 23);
            txtMfaSerial.TabIndex = 3;
            // 
            // lblMfaCode
            // 
            lblMfaCode.Font = new Font("Segoe UI", 9.75F);
            lblMfaCode.Location = new Point(16, 70);
            lblMfaCode.Name = "lblMfaCode";
            lblMfaCode.Size = new Size(234, 23);
            lblMfaCode.TabIndex = 4;
            lblMfaCode.Text = "MFA kod (opcionalno):";
            lblMfaCode.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMfaCode
            // 
            txtMfaCode.Location = new Point(256, 70);
            txtMfaCode.MaxLength = 6;
            txtMfaCode.Name = "txtMfaCode";
            txtMfaCode.Size = new Size(150, 23);
            txtMfaCode.TabIndex = 5;
            // 
            // chkSaveProfile
            // 
            chkSaveProfile.AutoSize = true;
            chkSaveProfile.Location = new Point(256, 248);
            chkSaveProfile.Name = "chkSaveProfile";
            chkSaveProfile.Size = new Size(169, 19);
            chkSaveProfile.TabIndex = 12;
            chkSaveProfile.Text = "Spremi postavke kao profil:";
            chkSaveProfile.UseVisualStyleBackColor = true;
            chkSaveProfile.CheckedChanged += chkSaveProfile_CheckedChanged;
            // 
            // txtSaveProfileName
            // 
            txtSaveProfileName.Enabled = false;
            txtSaveProfileName.Location = new Point(256, 273);
            txtSaveProfileName.MaxLength = 50;
            txtSaveProfileName.Name = "txtSaveProfileName";
            txtSaveProfileName.PlaceholderText = "Naziv profila";
            txtSaveProfileName.Size = new Size(410, 23);
            txtSaveProfileName.TabIndex = 13;
            // 
            // lblStatus
            // 
            lblStatus.ForeColor = SystemColors.GrayText;
            lblStatus.Location = new Point(24, 531);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(682, 40);
            lblStatus.TabIndex = 5;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(24, 577);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(682, 36);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "PRIJAVA";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 629);
            Controls.Add(lblHeader);
            Controls.Add(lblLangHr);
            Controls.Add(lblLangEn);
            Controls.Add(pnlAccountCard);
            Controls.Add(pnlAwsCard);
            Controls.Add(lblStatus);
            Controls.Add(btnLogin);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Prijava";
            pnlAccountCard.ResumeLayout(false);
            pnlAccountCard.PerformLayout();
            pnlAwsCard.ResumeLayout(false);
            pnlAwsCard.PerformLayout();
            pnlRoleSection.ResumeLayout(false);
            pnlRoleSection.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblHeader;
        private Label lblLangHr;
        private Label lblLangEn;

        private Panel pnlAccountCard;
        private Label lblAccountTitle;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;

        private Panel pnlAwsCard;
        private Label lblAwsTitle;
        private Label lblSavedProfiles;
        private ComboBox cmbSavedProfiles;
        private Label lblRegion;
        private ComboBox cmbRegion;
        private Label lblRegionInfo;
        private Label lblAccessKey;
        private TextBox txtAccessKey;
        private Label lblSecretKey;
        private TextBox txtSecretKey;
        private CheckBox chkUseRole;
        private Panel pnlRoleSection;
        private Label lblRoleArn;
        private TextBox txtRoleArn;
        private Label lblMfaSerial;
        private TextBox txtMfaSerial;
        private Label lblMfaCode;
        private TextBox txtMfaCode;
        private CheckBox chkSaveProfile;
        private TextBox txtSaveProfileName;

        private Label lblStatus;
        private Button btnLogin;
    }
}