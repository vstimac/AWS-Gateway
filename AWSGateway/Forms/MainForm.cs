using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;
using AWSGateway.Utils;
using System.Diagnostics;
using System.Reflection;

namespace AWSGateway.Forms
{
    public partial class MainForm : Form
    {
        // javno dostupni radi HomeView-ovih brzih poveznica (NavigateToModule)
        public const string ModuleHome = "home";
        public const string ModuleS3 = "s3";
        public const string ModuleEc2 = "ec2";
        public const string ModuleCosts = "costs";
        public const string ModuleReports = "reports";
        public const string ModuleProfiles = "profiles";
        public const string ModuleTeam = "team";
        public const string ModuleSettings = "settings";
        public const string ModuleAbout = "about";
        public const string ModuleHelp = "help";

        private AWSProfile _profile;
        private System.Windows.Forms.Timer _sessionTimer;
        private bool _sessionExpiredPending;

        // pamti zadnje postavljeno "značenje" boje lblSessionExpiry - ForeColor iz koda nije "zadana" boja pa se
        // ne osvježava sam kroz ApplyToControls; kod prijave pristupnim ključem se timer/refresh nikad više ne
        // pokreće, pa bi boja bez ovoga ostala zaleđena na staroj temi do ponovnog pokretanja aplikacije
        private UiTheme.StatusKind _sessionExpiryKind = UiTheme.StatusKind.Neutral;
        private string _currentModuleKey = string.Empty;
        private readonly Dictionary<string, UserControl> _modules = new Dictionary<string, UserControl>();
        private readonly List<NavButton> _navButtons = new List<NavButton>();

        public MainForm(AWSProfile profile)
        {
            InitializeComponent();

            _profile = profile;

            pnlHeader.Resize += delegate
            {
                LayoutHeaderProfileLabel();
            };
            LayoutHeaderProfileLabel();

            LoadWindowSettings();
            BuildNavigation();
            ApplyLanguage();
            ApplyTheme();
            CheckServerStatusAsync();
            InitializeSessionMonitor();
            ShowModule(ModuleHome);
        }

        private void LayoutHeaderProfileLabel()
        {
            int rightColumnLeft = pnlHeader.ClientSize.Width;

            if (lblRegion.Visible)
            {
                rightColumnLeft = Math.Min(rightColumnLeft, lblRegion.Left);
            }

            if (lblSessionExpiry.Visible)
            {
                rightColumnLeft = Math.Min(rightColumnLeft, lblSessionExpiry.Left);
            }

            if (lblServerStatus.Visible)
            {
                rightColumnLeft = Math.Min(rightColumnLeft, lblServerStatus.Left);
            }

            int availableWidth = rightColumnLeft - lblProfileName.Left - UiTheme.Space1;
            lblProfileName.Width = Math.Max(120, availableWidth);
        }

        private void BuildNavigation()
        {
            AddNav(pnlNav, ModuleHome, UiTheme.Icons.Home, "nav_home");
            AddNav(pnlNav, ModuleS3, UiTheme.Icons.Cloud, "s3_browser");
            AddNav(pnlNav, ModuleEc2, UiTheme.Icons.Server, "ec2_manager");
            AddNav(pnlNav, ModuleCosts, UiTheme.Icons.Money, "costs_button");
            AddNav(pnlNav, ModuleReports, UiTheme.Icons.Report, "reports");
            AddNav(pnlNav, ModuleProfiles, UiTheme.Icons.Contact, "data_files_button");
            AddNav(pnlNav, ModuleTeam, UiTheme.Icons.People, "team_status");
            AddNav(pnlNav, ModuleSettings, UiTheme.Icons.Settings, "settings");

            AddNav(pnlSidebarBottom, ModuleAbout, UiTheme.Icons.Info, "about");
            AddNav(pnlSidebarBottom, ModuleHelp, UiTheme.Icons.Help, "help");
            AddActionNav(pnlSidebarBottom, UiTheme.Icons.SignOut, "logout", Logout);
        }

        private void AddNav(FlowLayoutPanel parent, string key, string icon, string languageKey)
        {
            NavButton button = CreateNavButton(icon, languageKey);
            button.ModuleKey = key;
            button.NavClick += delegate
            {
                ShowModule(key);
            };
            parent.Controls.Add(button);
            _navButtons.Add(button);
        }

        private void AddActionNav(FlowLayoutPanel parent, string icon, string languageKey, Action action)
        {
            NavButton button = CreateNavButton(icon, languageKey);
            button.NavClick += delegate
            {
                action();
            };
            parent.Controls.Add(button);
            _navButtons.Add(button);
        }

        private NavButton CreateNavButton(string icon, string languageKey)
        {
            NavButton button = new NavButton();
            // pnlSidebar.Padding (8+8) + NavButton.Margin (8+8) = 32 širine treba oduzeti, ne 24 - inače je gumb
            // 8px širi od raspoloživog prostora u pnlNav, pa AutoScroll doda nepotrebnu vodoravnu traku (G1)
            button.Width = UiTheme.SidebarWidth - 32;
            button.IconGlyph = icon;
            button.Tag = languageKey;
            button.Caption = LanguageHelper.Get(languageKey);
            button.ApplyColors();
            return button;
        }

        private bool AnyModuleBusy()
        {
            foreach (UserControl control in _modules.Values)
            {
                IAppModule module = control as IAppModule;
                if (module != null && module.IsBusy)
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowModule(string key)
        {
            if (_modules.TryGetValue(_currentModuleKey, out UserControl current))
            {
                IAppModule currentModule = current as IAppModule;
                if (currentModule != null && currentModule.IsBusy && key != _currentModuleKey)
                {
                    UiTheme.SetStatus(lblStatus, LanguageHelper.Get("status_busy_switch"), UiTheme.StatusKind.Warning);
                    return;
                }
            }

            // uhvatiti PRIJE prepisivanja _currentModuleKey - Pomoć njime bira poglavlje s kojeg se došlo (sidebar klik ili F1)
            string previousModuleKey = _currentModuleKey;

            if (_modules.ContainsKey(key) == false)
            {
                UserControl created = CreateModule(key);
                created.Dock = DockStyle.Fill;
                _modules[key] = created;
                pnlContent.Controls.Add(created);
            }

            // AboutDialog/HelpDialog ne mogu implementirati IAppModule (AWSGateway.Dialogs ne smije ovisiti o glavnom projektu),
            // pa se ne oslanjaju na ModuleHost.OnActivated - sadržaj se ovdje ponovno predaje pri svakoj aktivaciji
            // (Dijagnostika u AboutDialogu tako uvijek prikazuje trenutno vrijeme, Pomoć uvijek ispravno poglavlje)
            if (key == ModuleAbout)
            {
                Form aboutForm = GetDialogInnerForm(_modules[key]);
                if (aboutForm != null)
                {
                    ApplyAboutContent(aboutForm);
                }
            }
            else if (key == ModuleHelp)
            {
                Form helpForm = GetDialogInnerForm(_modules[key]);
                if (helpForm != null)
                {
                    ApplyHelpContent(helpForm, previousModuleKey);
                }
            }

            foreach (KeyValuePair<string, UserControl> pair in _modules)
            {
                pair.Value.Visible = pair.Key == key;
                if (pair.Key == key)
                {
                    pair.Value.BringToFront();
                }
            }

            _currentModuleKey = key;
            HighlightNav(key);

            // ne brisati upozorenje o isteku sesije dok se čeka završetak radnje u pozadini
            if (_sessionExpiredPending == false)
            {
                UiTheme.SetStatus(lblStatus, string.Empty, UiTheme.StatusKind.Neutral);
            }

            (_modules[key] as IAppModule)?.OnActivated();
        }

        // HomeView-ove brze poveznice - jedini javni način da modul izvan MainForma zatraži promjenu prikaza
        public void NavigateToModule(string key)
        {
            ShowModule(key);
        }

        private UserControl CreateModule(string key)
        {
            if (key == ModuleHome)
            {
                return new HomeView(_profile, this);
            }

            if (key == ModuleS3)
            {
                return new ModuleHost(new S3BrowserForm(_profile));
            }

            if (key == ModuleEc2)
            {
                return new ModuleHost(new EC2ManagerForm(_profile));
            }

            if (key == ModuleCosts)
            {
                return new ModuleHost(new CostForm(_profile));
            }

            if (key == ModuleReports)
            {
                return new ModuleHost(new ReportForm(_profile));
            }

            if (key == ModuleProfiles)
            {
                return new ModuleHost(new ProfileManagerForm());
            }

            if (key == ModuleTeam)
            {
                return new ModuleHost(new TeamStatusForm());
            }

            if (key == ModuleAbout)
            {
                return CreateDialogModule("AWSGateway.Dialogs.AboutDialog", LanguageHelper.Get("dialog_about_missing"), ApplyAboutContent);
            }

            if (key == ModuleHelp)
            {
                return CreateDialogModule("AWSGateway.Dialogs.HelpDialog", LanguageHelper.Get("dialog_help_missing"), dialog => ApplyHelpContent(dialog, ModuleHome));
            }

            SettingsForm settings = new SettingsForm();
            settings.SettingsSaved += Settings_SettingsSaved;
            return new ModuleHost(settings);
        }

        // AboutDialog/HelpDialog dolaze iz AWSGateway.Dialogs, učitani refleksijom - ta biblioteka namjerno ne ovisi
        // o glavnom projektu, pa se cijeli sadržaj priprema ovdje i predaje kroz SetContent na formi (također refleksijom,
        // jer glavni projekt ne može referencirati AboutDialog/HelpDialog kao tip)
        private UserControl CreateDialogModule(string typeName, string missingTypeMessage, Action<Form> applyContent)
        {
            try
            {
                string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AWSGateway.Dialogs.dll");

                Assembly assembly = Assembly.LoadFrom(dllPath);
                Type dialogType = assembly.GetType(typeName);

                if (dialogType == null)
                {
                    return CreateDialogErrorModule(missingTypeMessage);
                }

                Form dialog = (Form)Activator.CreateInstance(dialogType);

                applyContent(dialog);
                UiTheme.Apply(dialog);

                return new ModuleHost(dialog);
            }
            catch (Exception ex)
            {
                return CreateDialogErrorModule(LanguageHelper.Format("dialog_error", ex.Message));
            }
        }

        private static Form GetDialogInnerForm(UserControl module)
        {
            ModuleHost host = module as ModuleHost;
            return host?.InnerForm;
        }

        private UserControl CreateDialogErrorModule(string message)
        {
            Label label = new Label();
            label.Dock = DockStyle.Top;
            label.AutoSize = true;
            label.Padding = new Padding(UiTheme.Space3);
            label.Text = message;
            label.ForeColor = UiTheme.Colors.Danger;

            UserControl fallback = new UserControl();
            fallback.Dock = DockStyle.Fill;
            fallback.Controls.Add(label);

            return fallback;
        }

        private static void InvokeSetContent(Form dialog, params object[] args)
        {
            MethodInfo method = dialog.GetType().GetMethod("SetContent");
            method?.Invoke(dialog, args);
        }

        private void Settings_SettingsSaved(object sender, EventArgs e)
        {
            ApplyLanguage();
            ApplyTheme();
            LoadUserInfo();
            Task.Run(LoadAvatarInBackground);
            RefreshModules();
        }

        private void RefreshModules()
        {
            foreach (KeyValuePair<string, UserControl> pair in _modules)
            {
                IAppModule module = pair.Value as IAppModule;
                if (module != null)
                {
                    module.ApplyShellLanguage();
                    module.ApplyShellTheme();
                }

                // AboutDialog/HelpDialog ne implementiraju IAppModule (vidi ShowModule) - ModuleHost.ApplyShellTheme
                // gore ipak već temira formu samu (UiTheme.Apply(_inner) je bezuvjetan), ostaje samo osvježiti sadržaj/jezik
                if (pair.Key == ModuleAbout)
                {
                    Form aboutForm = GetDialogInnerForm(pair.Value);
                    if (aboutForm != null)
                    {
                        ApplyAboutContent(aboutForm);
                    }
                }
                else if (pair.Key == ModuleHelp)
                {
                    Form helpForm = GetDialogInnerForm(pair.Value);
                    if (helpForm != null)
                    {
                        ApplyHelpContent(helpForm, _currentModuleKey);
                    }
                }
            }
        }

        private void HighlightNav(string key)
        {
            foreach (NavButton button in _navButtons)
            {
                button.Selected = button.ModuleKey == key;
            }
        }

        // AssumeRole prijava - prikazuje preostalo vrijeme sesije i reagira na istek (bez rušenja aplikacije)
        private void InitializeSessionMonitor()
        {
            if (_profile.IsTemporarySession() == false)
            {
                lblSessionExpiry.Text = LanguageHelper.Get("session_access_key");
                _sessionExpiryKind = UiTheme.StatusKind.Neutral;
                lblSessionExpiry.ForeColor = UiTheme.StatusKindColor(_sessionExpiryKind);
                lblSessionExpiry.Visible = true;
                return;
            }

            lblSessionExpiry.Visible = true;
            UpdateSessionLabel();

            _sessionTimer = new System.Windows.Forms.Timer();
            _sessionTimer.Interval = 30000;
            _sessionTimer.Tick += SessionTimer_Tick;
            _sessionTimer.Start();
        }

        private void UpdateSessionLabel()
        {
            TimeSpan remaining = _profile.GetRemainingSessionTime();

            lblSessionExpiry.Text = LanguageHelper.Format("session_expires", FormatUtils.FormatDuration(remaining.TotalSeconds));

            if (remaining.TotalMinutes < 1)
            {
                _sessionExpiryKind = UiTheme.StatusKind.Error;
            }
            else if (remaining.TotalMinutes < 5)
            {
                _sessionExpiryKind = UiTheme.StatusKind.Warning;
            }
            else
            {
                _sessionExpiryKind = UiTheme.StatusKind.Neutral;
            }

            lblSessionExpiry.ForeColor = UiTheme.StatusKindColor(_sessionExpiryKind);
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            UpdateSessionLabel();
            RefreshHomeIfActive();

            if (_profile.GetRemainingSessionTime() > TimeSpan.Zero)
            {
                return;
            }

            if (AnyModuleBusy())
            {
                _sessionExpiredPending = true;
                UiTheme.SetStatus(lblStatus, LanguageHelper.Get("session_expired_busy"), UiTheme.StatusKind.Warning);
            }
            else
            {
                _sessionExpiredPending = false;
                HandleSessionExpired();
            }
        }

        // sesija je istekla - prikazi poruku i vrati se na prijavu, bez zatvaranja cijele aplikacije
        private void HandleSessionExpired()
        {
            if (_sessionTimer != null)
            {
                _sessionTimer.Stop();
            }

            MessageBox.Show(LanguageHelper.Get("session_expired"), LanguageHelper.Get("session_expired_title"));

            SessionExpired = true;
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void ApplyLanguage()
        {
            Text = LanguageHelper.Get("main_title");
            lblAppName.Text = LanguageHelper.Get("main_title");
            lblServerStatus.Text = LanguageHelper.Get("tcp_checking");

            foreach (NavButton button in _navButtons)
            {
                string key = button.Tag as string;
                if (string.IsNullOrEmpty(key) == false)
                {
                    button.Caption = LanguageHelper.Get(key);
                }
            }

            RefreshModules();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadUserInfo();
        }

        // Pomoć se otvara na poglavlju trenutno otvorenog modula (ShowModule to čita iz _currentModuleKey prije prelaska)
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ShowModule(ModuleHelp);
                e.Handled = true;
            }
        }

        private void LoadUserInfo()
        {
            lblWelcome.Text = LanguageHelper.Get("welcome") + ", " + AppSettings.Instance.LastUser;
            lblProfileName.Text = LanguageHelper.Get("aws_profile") + " " + _profile.ProfileName;
            lblRegion.Text = LanguageHelper.Get("region") + " " + _profile.Region;

            if (string.IsNullOrEmpty(_profile.AwsArn) == false)
            {
                lblProfileName.Text = lblProfileName.Text + " (" + _profile.AwsArn + ")";
            }

            Task.Run(LoadAvatarInBackground);
        }

        private void LoadWindowSettings()
        {
            try
            {
                int savedWidth = RegistryHelper.ReadInt("MainFormWidth", 0);
                int savedHeight = RegistryHelper.ReadInt("MainFormHeight", 0);
                int savedX = RegistryHelper.ReadInt("MainFormX", -1);
                int savedY = RegistryHelper.ReadInt("MainFormY", -1);

                if (savedWidth > 0 && savedHeight > 0)
                {
                    Size = new Size(savedWidth, savedHeight);
                }

                if (savedX >= 0 && savedY >= 0)
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(savedX, savedY);
                }

                int maximized = RegistryHelper.ReadInt("MainFormMaximized", 0);

                if (maximized == 1)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod postavki prozora: " + ex.Message);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AnyModuleBusy() && DialogResult != DialogResult.Retry)
            {
                MessageBox.Show(LanguageHelper.Get("status_busy_close"));
                e.Cancel = true;
                return;
            }

            if (_sessionTimer != null)
            {
                _sessionTimer.Stop();
                _sessionTimer.Dispose();
            }

            try
            {
                Rectangle bounds;

                if (WindowState == FormWindowState.Maximized)
                {
                    bounds = RestoreBounds;
                }
                else
                {
                    bounds = new Rectangle(Location, Size);
                }

                RegistryHelper.WriteString("MainFormWidth", bounds.Width.ToString());
                RegistryHelper.WriteString("MainFormHeight", bounds.Height.ToString());
                RegistryHelper.WriteString("MainFormX", bounds.X.ToString());
                RegistryHelper.WriteString("MainFormY", bounds.Y.ToString());

                if (WindowState == FormWindowState.Maximized)
                {
                    RegistryHelper.WriteString("MainFormMaximized", "1");
                }
                else
                {
                    RegistryHelper.WriteString("MainFormMaximized", "0");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod spremanja postavki prozora: " + ex.Message);
            }

            // moduli se ne zatvaraju pojedinačno (Close/FormClosed se ne okida) - ljuska ih oslobađa ovdje kod gašenja
            foreach (KeyValuePair<string, UserControl> pair in _modules)
            {
                try
                {
                    pair.Value.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Greška kod oslobađanja modula '" + pair.Key + "': " + ex.Message);
                }
            }
        }

        private void ApplyTheme()
        {
            UiTheme.Apply(this);
            pnlSidebar.BackColor = UiTheme.Colors.Sidebar;
            lblAppName.ForeColor = UiTheme.Colors.SidebarText;
            lblAppName.BackColor = UiTheme.Colors.Sidebar;
            pnlNav.BackColor = UiTheme.Colors.Sidebar;
            pnlSidebarBottom.BackColor = UiTheme.Colors.Sidebar;
            pnlHeader.BackColor = UiTheme.Colors.Surface;
            // PictureBox nema svoj slučaj u UiTheme.ApplyToControls - bez ovoga ostaje na zadanoj (svijetloj) boji
            // i u tamnoj temi izgleda kao prazan svijetli prostor kad korisnik nema postavljen avatar (G3)
            picAvatar.BackColor = UiTheme.Colors.Surface;
            pnlContent.BackColor = UiTheme.Colors.Background;
            lblStatus.BackColor = UiTheme.Colors.Surface;
            lblStatus.ForeColor = UiTheme.Colors.Muted;
            lblSessionExpiry.ForeColor = UiTheme.StatusKindColor(_sessionExpiryKind);
            lblAppName.Font = UiTheme.Fonts.Title;
            lblWelcome.Font = UiTheme.Fonts.Title;
            lblWelcome.ForeColor = UiTheme.Colors.Foreground;
            lblProfileName.ForeColor = UiTheme.Colors.Muted;
            lblRegion.ForeColor = UiTheme.Colors.Muted;

            foreach (NavButton button in _navButtons)
            {
                button.ApplyColors();
            }

            pnlHeader.Invalidate();
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(UiTheme.Colors.Border))
            {
                e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            }
        }

        // statusna traka je bez ovoga izgledala kao neuklopljena bijela/svijetla pruga - rub joj daje isti tretman kao zaglavlju
        private void lblStatus_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(UiTheme.Colors.Border))
            {
                e.Graphics.DrawLine(pen, 0, 0, lblStatus.Width, 0);
            }
        }

        private async void CheckServerStatusAsync()
        {
            try
            {
                TcpClientService tcpService = new TcpClientService();
                bool isOnline = await tcpService.EnsureServerRunningAsync();

                if (isOnline)
                {
                    UiTheme.SetStatus(lblServerStatus, LanguageHelper.Get("tcp_online"), UiTheme.StatusKind.Success);
                }
                else
                {
                    UiTheme.SetStatus(lblServerStatus, LanguageHelper.Get("tcp_offline"), UiTheme.StatusKind.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod provjere statusa TCP servera: " + ex.Message);
                UiTheme.SetStatus(lblServerStatus, LanguageHelper.Get("tcp_unknown"), UiTheme.StatusKind.Neutral);
            }
            finally
            {
                RefreshHomeIfActive();
            }
        }

        // status TCP servera se ovdje NE provjerava ponovno - Početna samo čita ono što je MainForm već utvrdio
        public string ServerStatusText
        {
            get { return lblServerStatus.Text; }
        }

        public Color ServerStatusColor
        {
            get { return lblServerStatus.ForeColor; }
        }

        // razlikuje DialogResult.Retry uzrokovan istekom sesije od onog uzrokovanog odjavom - LoginForm time bira poruku
        public bool SessionExpired { get; private set; }

        // Početna nema svoj timer/provjeru - osvježava se kad se ono što MainForm već prati promijeni (sesija, TCP status)
        private void RefreshHomeIfActive()
        {
            if (_currentModuleKey != ModuleHome)
            {
                return;
            }

            if (_modules.TryGetValue(ModuleHome, out UserControl home))
            {
                (home as IAppModule)?.OnActivated();
            }
        }

        private void LoadAvatarInBackground()
        {
            try
            {
                string username = AppSettings.Instance.LastUser;
                byte[] avatarData = DatabaseManager.Instance.GetUserAvatar(username);

                if (avatarData == null)
                {
                    return;
                }

                Image image;

                using (MemoryStream stream = new MemoryStream(avatarData))
                {
                    using (Image tempImage = Image.FromStream(stream))
                    {
                        image = new Bitmap(tempImage);
                    }
                }

                ThreadManager.SafeInvoke(picAvatar, () =>
                {
                    Image oldImage = picAvatar.Image;
                    picAvatar.Image = image;

                    if (oldImage != null)
                    {
                        oldImage.Dispose();
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod učitavanja avatara: " + ex.Message);
            }
        }

        private void Logout()
        {
            if (AnyModuleBusy())
            {
                UiTheme.SetStatus(lblStatus, LanguageHelper.Get("status_busy_switch"), UiTheme.StatusKind.Warning);
                return;
            }

            ActivityLogger.Log("Odjava", "MainForm", "Korisnik " + AppSettings.Instance.LastUser + " se odjavio");

            AppSettings.Instance.SaveToIni();

            // isti povratak na prijavu kao kod isteka sesije (DialogResult.Retry) - aplikacija se ne zatvara,
            // SessionExpired ostaje false pa LoginForm ne prikazuje poruku o isteku sesije
            DialogResult = DialogResult.Retry;
            Close();
        }

        // AboutDialog gradi Tag u vlastitom konstruktoru (sirovi broj verzije, prije nego ljuska ima priliku intervenirati) -
        // ostatak sadržaja glavni projekt priprema ovdje i predaje kroz SetContent (refleksijom - AWSGateway.Dialogs ne smije ovisiti o glavnom projektu)
        private static void ApplyAboutContent(Form dialog)
        {
            string versionRaw = dialog.Tag as string;

            if (string.IsNullOrEmpty(versionRaw))
            {
                versionRaw = "0.0.0.0";
            }

            List<string> contextLines = new List<string>
            {
                LanguageHelper.Format("about_context_author", LanguageHelper.Get("about_context_author_value")),
                LanguageHelper.Format("about_context_study", LanguageHelper.Get("about_context_study_value")),
                LanguageHelper.Format("about_context_institution", LanguageHelper.Get("about_context_institution_value")),
                LanguageHelper.Format("about_context_mentor", LanguageHelper.Get("about_context_mentor_value")),
                LanguageHelper.Format("about_context_year", "2026.")
            };

            List<string> technologies = new List<string>
            {
                ".NET 10",
                "Windows Forms",
                "AWS SDK for .NET (S3, EC2, STS, CloudWatch, Cost Explorer)",
                "SQLite (Microsoft.Data.Sqlite) + Dapper",
                "iText 9.7.0"
            };

            List<string> licenseLines = new List<string>
            {
                LanguageHelper.Get("about_license_aws_sdk"),
                LanguageHelper.Get("about_license_dapper"),
                LanguageHelper.Get("about_license_sqlite"),
                LanguageHelper.Get("about_license_itext")
            };

            string dotnetVersion = Environment.Version.ToString();
            string osVersion = "Microsoft Windows";
            string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string language = AppSettings.Instance.CurrentLanguage;
            string theme = AppSettings.Instance.Theme;

            string diagnosticsText =
                LanguageHelper.Format("about_dotnet_version", dotnetVersion) + "\r\n" +
                LanguageHelper.Format("about_os", osVersion) + "\r\n" +
                LanguageHelper.Format("about_current_time", currentTime) + "\r\n" +
                language + " / " + theme;

            InvokeSetContent(
                dialog,
                LanguageHelper.Get("main_title"),
                LanguageHelper.Format("about_version", versionRaw),
                LanguageHelper.Get("about_description"),
                LanguageHelper.Get("about_context_title"),
                contextLines,
                LanguageHelper.Get("about_technologies_title"),
                technologies,
                LanguageHelper.Get("about_storage_title"),
                AppPaths.DataDirectory,
                LanguageHelper.Get("about_storage_contents"),
                LanguageHelper.Get("about_storage_not_saved"),
                LanguageHelper.Get("about_open_folder"),
                LanguageHelper.Get("about_licenses_title"),
                licenseLines,
                LanguageHelper.Get("about_diagnostics_title"),
                diagnosticsText,
                LanguageHelper.Get("about_copy"),
                LanguageHelper.Get("about_copied"));
        }

        // poglavlje otvoreno na startu ovisi o modulu s kojeg se došlo (sidebar klik ili F1) - contextModuleKey je _currentModuleKey PRIJE prelaska na Pomoć
        private static void ApplyHelpContent(Form dialog, string contextModuleKey)
        {
            List<string> titles = new List<string>();
            List<string> bodies = new List<string>();
            Dictionary<string, int> moduleChapterIndex = new Dictionary<string, int>();

            titles.Add(LanguageHelper.Get("help_ch_getting_started_title"));
            bodies.Add(LanguageHelper.Get("help_ch_getting_started_body"));

            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleHome, "help_ch_home_title", "help_ch_home_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleS3, "help_ch_s3_title", "help_ch_s3_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleEc2, "help_ch_ec2_title", "help_ch_ec2_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleCosts, "help_ch_costs_title", "help_ch_costs_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleReports, "help_ch_reports_title", "help_ch_reports_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleProfiles, "help_ch_profiles_title", "help_ch_profiles_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleTeam, "help_ch_team_title", "help_ch_team_body");
            AddHelpChapter(titles, bodies, moduleChapterIndex, ModuleSettings, "help_ch_settings_title", "help_ch_settings_body");

            titles.Add(LanguageHelper.Get("help_ch_problems_title"));
            bodies.Add(LanguageHelper.Get("help_ch_problems_body"));

            titles.Add(LanguageHelper.Get("help_ch_security_title"));
            bodies.Add(LanguageHelper.Get("help_ch_security_body"));

            int initialIndex = 0;
            moduleChapterIndex.TryGetValue(contextModuleKey, out initialIndex);

            InvokeSetContent(dialog, titles, bodies, initialIndex, UiTheme.Colors.Hover, UiTheme.Colors.Foreground);
        }

        private static void AddHelpChapter(List<string> titles, List<string> bodies, Dictionary<string, int> moduleChapterIndex, string moduleKey, string titleLangKey, string bodyLangKey)
        {
            moduleChapterIndex[moduleKey] = titles.Count;
            titles.Add(LanguageHelper.Get(titleLangKey));
            bodies.Add(LanguageHelper.Get(bodyLangKey));
        }
    }
}
