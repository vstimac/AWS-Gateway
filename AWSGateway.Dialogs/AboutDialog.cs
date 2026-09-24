using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AWSGateway.Dialogs
{
    // prikazuje se ugrađeno u ljusku (ModuleHost, TopLevel = false) - ova biblioteka ne smije ovisiti o glavnom projektu
    // (UiTheme/LanguageHelper), pa glavni projekt sav prikazani sadržaj priprema i predaje kroz SetContent (refleksijom)
    public partial class AboutDialog : Form
    {
        private readonly FlowLayoutPanel _root;
        private Panel? _diagnosticsBody;
        private Label? _diagnosticsToggle;
        private Button? _btnCopyDiagnostics;
        private string _diagnosticsText = string.Empty;
        private string _copyButtonText = string.Empty;
        private string _copiedButtonText = string.Empty;

        public AboutDialog()
        {
            InitializeComponent();

            _root = new FlowLayoutPanel();
            _root.Dock = DockStyle.Fill;
            _root.FlowDirection = FlowDirection.TopDown;
            _root.WrapContents = false;
            _root.AutoScroll = true;
            _root.Padding = new Padding(20);

            Controls.Add(_root);

            LoadVersion();
        }

        // AWSGateway.Dialogs ne smije ovisiti o glavnom projektu (LanguageHelper) - Tag nosi sirovi broj verzije,
        // glavni projekt ga pročita i formatira preko SetContent
        private void LoadVersion()
        {
            try
            {
                Version version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0, 0);
                Tag = version.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod čitanja verzije: " + ex.Message);
            }
        }

        // poziva se refleksijom iz MainForm-a (kod otvaranja stranice i kod svake promjene jezika/teme dok je otvorena)
        public void SetContent(
            string appName,
            string versionText,
            string description,
            string contextTitle,
            List<string> contextLines,
            string technologiesTitle,
            List<string> technologies,
            string storageTitle,
            string storagePath,
            string storageContentsText,
            string storageNotSavedText,
            string openFolderButtonText,
            string licensesTitle,
            List<string> licenseLines,
            string diagnosticsTitle,
            string diagnosticsText,
            string copyButtonText,
            string copiedButtonText)
        {
            _diagnosticsText = diagnosticsText;
            _copyButtonText = copyButtonText;
            _copiedButtonText = copiedButtonText;

            _root.SuspendLayout();
            _root.Controls.Clear();

            Label lblAppName = new Label();
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblAppName.Text = appName;
            lblAppName.Margin = new Padding(0, 0, 0, 4);
            _root.Controls.Add(lblAppName);

            Label lblVersion = new Label();
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 10F);
            lblVersion.Text = versionText;
            lblVersion.Margin = new Padding(0, 0, 0, 16);
            _root.Controls.Add(lblVersion);

            Label lblDescription = new Label();
            lblDescription.AutoSize = true;
            lblDescription.MaximumSize = new Size(600, 0);
            lblDescription.Font = new Font("Segoe UI", 10F);
            lblDescription.Text = description;
            lblDescription.Margin = new Padding(0, 0, 0, 20);
            _root.Controls.Add(lblDescription);

            AddSection(contextTitle, contextLines);
            AddSection(technologiesTitle, technologies);

            AddSectionHeading(storageTitle);

            TextBox txtStoragePath = new TextBox();
            txtStoragePath.ReadOnly = true;
            txtStoragePath.Width = 620;
            txtStoragePath.BorderStyle = BorderStyle.FixedSingle;
            txtStoragePath.Text = storagePath;
            txtStoragePath.Margin = new Padding(0, 0, 0, 6);
            txtStoragePath.GotFocus += ReadOnlyTextBox_GotFocus;
            _root.Controls.Add(txtStoragePath);

            Button btnOpenFolder = new Button();
            btnOpenFolder.AutoSize = true;
            btnOpenFolder.Text = openFolderButtonText;
            btnOpenFolder.Margin = new Padding(0, 0, 0, 8);
            btnOpenFolder.Click += (s, e) => OpenStorageFolder(storagePath);
            _root.Controls.Add(btnOpenFolder);

            Label lblStorageContents = new Label();
            lblStorageContents.AutoSize = true;
            lblStorageContents.MaximumSize = new Size(620, 0);
            lblStorageContents.Text = storageContentsText;
            lblStorageContents.Margin = new Padding(0, 0, 0, 4);
            _root.Controls.Add(lblStorageContents);

            Label lblStorageNotSaved = new Label();
            lblStorageNotSaved.AutoSize = true;
            lblStorageNotSaved.MaximumSize = new Size(620, 0);
            lblStorageNotSaved.Text = storageNotSavedText;
            lblStorageNotSaved.Margin = new Padding(0, 0, 0, 20);
            _root.Controls.Add(lblStorageNotSaved);

            AddSection(licensesTitle, licenseLines);

            // Dijagnostika - sklopivo, zatvoreno prema zadanome
            _diagnosticsToggle = new Label();
            _diagnosticsToggle.AutoSize = true;
            _diagnosticsToggle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            _diagnosticsToggle.Text = "▸ " + diagnosticsTitle;
            _diagnosticsToggle.Cursor = Cursors.Hand;
            _diagnosticsToggle.Margin = new Padding(0, 4, 0, 4);
            _diagnosticsToggle.Tag = diagnosticsTitle;
            _diagnosticsToggle.Click += DiagnosticsToggle_Click;
            _root.Controls.Add(_diagnosticsToggle);

            _diagnosticsBody = new Panel();
            _diagnosticsBody.AutoSize = true;
            _diagnosticsBody.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _diagnosticsBody.Visible = false;
            _diagnosticsBody.Margin = new Padding(16, 0, 0, 0);

            Label lblDiagnostics = new Label();
            lblDiagnostics.AutoSize = true;
            lblDiagnostics.Location = new Point(0, 0);
            lblDiagnostics.Text = diagnosticsText;
            _diagnosticsBody.Controls.Add(lblDiagnostics);

            _btnCopyDiagnostics = new Button();
            _btnCopyDiagnostics.AutoSize = true;
            _btnCopyDiagnostics.Text = copyButtonText;
            _btnCopyDiagnostics.Location = new Point(0, lblDiagnostics.PreferredSize.Height + 8);
            _btnCopyDiagnostics.Click += BtnCopyDiagnostics_Click;
            _diagnosticsBody.Controls.Add(_btnCopyDiagnostics);

            _root.Controls.Add(_diagnosticsBody);

            _root.ResumeLayout(true);
        }

        private void AddSectionHeading(string title)
        {
            Label heading = new Label();
            heading.AutoSize = true;
            heading.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            heading.Text = title;
            heading.Margin = new Padding(0, 4, 0, 6);
            _root.Controls.Add(heading);
        }

        private void AddSection(string title, List<string> lines)
        {
            AddSectionHeading(title);

            foreach (string line in lines)
            {
                Label lblLine = new Label();
                lblLine.AutoSize = true;
                lblLine.Text = line;
                lblLine.Margin = new Padding(0, 0, 0, 2);
                _root.Controls.Add(lblLine);
            }

            Label spacer = new Label();
            spacer.AutoSize = false;
            spacer.Height = 14;
            spacer.Margin = new Padding(0);
            _root.Controls.Add(spacer);
        }

        private void DiagnosticsToggle_Click(object? sender, EventArgs e)
        {
            if (_diagnosticsBody == null || _diagnosticsToggle == null)
            {
                return;
            }

            bool expand = _diagnosticsBody.Visible == false;

            _diagnosticsBody.Visible = expand;

            string title = _diagnosticsToggle.Tag as string ?? string.Empty;

            if (expand)
            {
                _diagnosticsToggle.Text = "▾ " + title;
            }
            else
            {
                _diagnosticsToggle.Text = "▸ " + title;
            }
        }

        private void BtnCopyDiagnostics_Click(object? sender, EventArgs e)
        {
            if (_btnCopyDiagnostics == null)
            {
                return;
            }

            try
            {
                Clipboard.SetText(_diagnosticsText);
                _btnCopyDiagnostics.Text = _copiedButtonText;
            }
            catch (ExternalException ex)
            {
                Debug.WriteLine("Greška kod kopiranja dijagnostike: " + ex.Message);
            }
        }

        private static void OpenStorageFolder(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", "\"" + path + "\"");
                    startInfo.UseShellExecute = true;
                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod otvaranja mape: " + ex.Message);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        // isti razlog kao UiTheme.ReadOnlyTextBox_GotFocus u glavnom projektu - ReadOnly TextBox i dalje prima fokus i trepće kursor
        private void ReadOnlyTextBox_GotFocus(object? sender, EventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            if (textBox != null)
            {
                HideCaret(textBox.Handle);
            }
        }
    }
}
