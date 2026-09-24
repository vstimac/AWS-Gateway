namespace AWSGateway.Forms
{
    partial class MainForm
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
            pnlSidebar = new Panel();
            lblAppName = new Label();
            pnlNav = new FlowLayoutPanel();
            pnlSidebarBottom = new FlowLayoutPanel();
            pnlHeader = new Panel();
            picAvatar = new PictureBox();
            lblWelcome = new Label();
            lblProfileName = new Label();
            lblRegion = new Label();
            lblSessionExpiry = new Label();
            lblServerStatus = new Label();
            pnlContent = new Panel();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            pnlSidebar.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Width = 240;
            pnlSidebar.Padding = new Padding(8, 16, 8, 8);
            pnlSidebar.Controls.Add(pnlNav);
            pnlSidebar.Controls.Add(pnlSidebarBottom);
            pnlSidebar.Controls.Add(lblAppName);
            // 
            // lblAppName
            // 
            lblAppName.Dock = DockStyle.Top;
            lblAppName.Height = 40;
            lblAppName.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblAppName.TextAlign = ContentAlignment.MiddleLeft;
            lblAppName.Padding = new Padding(8, 0, 0, 0);
            lblAppName.Text = "AWS Gateway";
            // 
            // pnlNav
            // 
            pnlNav.Dock = DockStyle.Fill;
            pnlNav.FlowDirection = FlowDirection.TopDown;
            pnlNav.WrapContents = false;
            pnlNav.AutoScroll = true;
            pnlNav.Padding = new Padding(0, 8, 0, 8);
            // 
            // pnlSidebarBottom
            // 
            pnlSidebarBottom.Dock = DockStyle.Bottom;
            pnlSidebarBottom.Height = 220;
            pnlSidebarBottom.FlowDirection = FlowDirection.TopDown;
            pnlSidebarBottom.WrapContents = false;
            pnlSidebarBottom.Padding = new Padding(0, 8, 0, 8);
            // 
            // pnlHeader
            // 
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 72;
            pnlHeader.Padding = new Padding(16, 8, 16, 8);
            pnlHeader.Controls.Add(lblSessionExpiry);
            pnlHeader.Controls.Add(lblServerStatus);
            pnlHeader.Controls.Add(lblRegion);
            pnlHeader.Controls.Add(lblProfileName);
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(picAvatar);
            pnlHeader.Paint += pnlHeader_Paint;
            // 
            // picAvatar
            // 
            picAvatar.Dock = DockStyle.Left;
            picAvatar.Width = 56;
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            picAvatar.Margin = new Padding(0, 0, 16, 0);
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblWelcome.Location = new Point(80, 8);
            lblWelcome.Text = "Dobrodošli";
            // 
            // lblProfileName
            // 
            // fiksna širina + AutoEllipsis - pun ARN inače nema granicu pa se na užem prozoru sudara s desno usidrenim natpisima
            lblProfileName.AutoSize = false;
            lblProfileName.AutoEllipsis = true;
            lblProfileName.Location = new Point(82, 40);
            lblProfileName.Size = new Size(420, 20);
            lblProfileName.Text = "AWS profil:";
            // 
            // lblRegion
            // 
            lblRegion.AutoSize = true;
            lblRegion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRegion.Location = new Point(520, 12);
            lblRegion.Text = "Regija:";
            // 
            // lblSessionExpiry
            // 
            lblSessionExpiry.AutoSize = true;
            lblSessionExpiry.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSessionExpiry.Location = new Point(520, 32);
            lblSessionExpiry.Visible = false;
            // 
            // lblServerStatus
            // 
            lblServerStatus.AutoSize = true;
            lblServerStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblServerStatus.Location = new Point(520, 52);
            lblServerStatus.Text = "Provjeravam TCP konekciju";
            // 
            // pnlContent
            // 
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(0);
            // 
            // lblStatus
            // 
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 32;
            lblStatus.Padding = new Padding(16, 6, 16, 6);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Paint += lblStatus_Paint;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1200, 760);
            // ModuleHost već čisti MinimumSize svakog modula pri ugradnji (sadržaj se oslanja na AutoScroll),
            // pa donja granica stvarno ovisi samo o bočnoj traci (240) i zaglavlju - ne o pojedinom modulu
            MinimumSize = new Size(800, 550);
            Controls.Add(pnlContent);
            Controls.Add(lblStatus);
            Controls.Add(pnlHeader);
            Controls.Add(pnlSidebar);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AWS Gateway";
            KeyPreview = true;
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            KeyDown += MainForm_KeyDown;
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            pnlSidebar.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
        }

        private Panel pnlSidebar;
        private Label lblAppName;
        private FlowLayoutPanel pnlNav;
        private FlowLayoutPanel pnlSidebarBottom;
        private Panel pnlHeader;
        private PictureBox picAvatar;
        private Label lblWelcome;
        private Label lblProfileName;
        private Label lblRegion;
        private Label lblSessionExpiry;
        private Label lblServerStatus;
        private Panel pnlContent;
        private Label lblStatus;
    }
}
