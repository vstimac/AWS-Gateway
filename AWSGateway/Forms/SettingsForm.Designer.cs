namespace AWSGateway.Forms
{
    partial class SettingsForm
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
            grpApp = new GroupBox();
            lblLanguage = new Label();
            cmbLanguage = new ComboBox();
            lblTheme = new Label();
            cmbTheme = new ComboBox();
            lblCacheTtl = new Label();
            numCacheTtl = new NumericUpDown();
            grpAccount = new GroupBox();
            picAvatar = new PictureBox();
            btnAvatar = new Button();
            btnDeleteAccount = new Button();
            btnSave = new Button();
            btnCancel = new Button();
            grpApp.SuspendLayout();
            grpAccount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCacheTtl).BeginInit();
            SuspendLayout();
            //
            // grpApp
            //
            grpApp.Controls.Add(lblLanguage);
            grpApp.Controls.Add(cmbLanguage);
            grpApp.Controls.Add(lblTheme);
            grpApp.Controls.Add(cmbTheme);
            grpApp.Controls.Add(lblCacheTtl);
            grpApp.Controls.Add(numCacheTtl);
            grpApp.Location = new Point(20, 20);
            grpApp.Name = "grpApp";
            grpApp.Size = new Size(320, 180);
            grpApp.TabIndex = 0;
            grpApp.Text = "Aplikacija";
            //
            // lblLanguage
            //
            lblLanguage.AutoSize = true;
            lblLanguage.Location = new Point(16, 30);
            lblLanguage.Name = "lblLanguage";
            lblLanguage.Size = new Size(40, 15);
            lblLanguage.TabIndex = 0;
            lblLanguage.Text = "Jezik:";
            //
            // cmbLanguage
            //
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.Items.AddRange(new object[] { "HRV", "ENG" });
            cmbLanguage.Location = new Point(16, 50);
            cmbLanguage.Name = "cmbLanguage";
            cmbLanguage.Size = new Size(288, 23);
            cmbLanguage.TabIndex = 1;
            //
            // lblTheme
            //
            lblTheme.AutoSize = true;
            lblTheme.Location = new Point(16, 85);
            lblTheme.Name = "lblTheme";
            lblTheme.Size = new Size(44, 15);
            lblTheme.TabIndex = 2;
            lblTheme.Text = "Tema:";
            //
            // cmbTheme
            //
            cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTheme.Items.AddRange(new object[] { "Light", "Dark" });
            cmbTheme.Location = new Point(16, 105);
            cmbTheme.Name = "cmbTheme";
            cmbTheme.Size = new Size(288, 23);
            cmbTheme.TabIndex = 3;
            //
            // lblCacheTtl
            //
            lblCacheTtl.AutoSize = true;
            lblCacheTtl.Location = new Point(16, 138);
            lblCacheTtl.Name = "lblCacheTtl";
            lblCacheTtl.Size = new Size(200, 15);
            lblCacheTtl.TabIndex = 4;
            lblCacheTtl.Text = "Trajanje predmemorije (sati):";
            //
            // numCacheTtl
            //
            numCacheTtl.Location = new Point(220, 136);
            numCacheTtl.Minimum = 1;
            numCacheTtl.Maximum = 72;
            numCacheTtl.Value = 6;
            numCacheTtl.Name = "numCacheTtl";
            numCacheTtl.Size = new Size(80, 23);
            numCacheTtl.TabIndex = 5;
            //
            // grpAccount
            //
            grpAccount.Controls.Add(picAvatar);
            grpAccount.Controls.Add(btnAvatar);
            grpAccount.Controls.Add(btnDeleteAccount);
            grpAccount.Location = new Point(360, 20);
            grpAccount.Name = "grpAccount";
            grpAccount.Size = new Size(220, 280);
            grpAccount.TabIndex = 1;
            grpAccount.Text = "Korisnički račun";
            //
            // picAvatar
            //
            picAvatar.BorderStyle = BorderStyle.FixedSingle;
            picAvatar.Location = new Point(20, 30);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(180, 150);
            picAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
            picAvatar.TabIndex = 0;
            picAvatar.TabStop = false;
            //
            // btnAvatar
            //
            btnAvatar.Location = new Point(20, 190);
            btnAvatar.Name = "btnAvatar";
            btnAvatar.Size = new Size(180, 36);
            btnAvatar.TabIndex = 1;
            btnAvatar.Text = "Učitaj avatar";
            btnAvatar.UseVisualStyleBackColor = true;
            btnAvatar.Click += btnAvatar_Click;
            //
            // btnDeleteAccount
            //
            btnDeleteAccount.Location = new Point(20, 236);
            btnDeleteAccount.Name = "btnDeleteAccount";
            btnDeleteAccount.Size = new Size(180, 36);
            btnDeleteAccount.TabIndex = 2;
            btnDeleteAccount.Text = "Obriši račun";
            btnDeleteAccount.UseVisualStyleBackColor = true;
            btnDeleteAccount.Click += btnDeleteAccount_Click;
            //
            // btnSave
            //
            btnSave.Location = new Point(20, 220);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(150, 36);
            btnSave.TabIndex = 2;
            btnSave.Text = "Spremi";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            //
            // btnCancel
            //
            btnCancel.Location = new Point(190, 220);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(150, 36);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Odustani";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            //
            // SettingsForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 320);
            Controls.Add(grpApp);
            Controls.Add(grpAccount);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Postavke";
            Load += SettingsForm_Load;
            grpApp.ResumeLayout(false);
            grpApp.PerformLayout();
            grpAccount.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCacheTtl).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpApp;
        private Label lblLanguage;
        private ComboBox cmbLanguage;
        private Label lblTheme;
        private ComboBox cmbTheme;
        private Label lblCacheTtl;
        private NumericUpDown numCacheTtl;
        private GroupBox grpAccount;
        private PictureBox picAvatar;
        private Button btnAvatar;
        private Button btnDeleteAccount;
        private Button btnSave;
        private Button btnCancel;
    }
}
