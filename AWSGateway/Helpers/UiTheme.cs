using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AWSGateway.Helpers
{
    // Paleta, tipografija, razmaci i stil kontrola - jedino mjesto za boje i fontove sučelja.
    public static class UiTheme
    {
        public const int Space1 = 8;
        public const int Space2 = 16;
        public const int Space3 = 24;
        public const int SidebarWidth = 240;
        public const int HeaderHeight = 72;
        public const int StatusHeight = 32;
        public const int NavItemHeight = 44;
        public const int ButtonHeight = 36;

        // kompatibilnost s postojećim pozivima (graf troškova)
        public static Color AccentColor
        {
            get { return Colors.Accent; }
        }

        public static bool IsDark
        {
            get { return AppSettings.Instance.Theme == "Dark"; }
        }

        // Paleta prema AWS Cloudscape dizajnerskom sustavu (cloudscape-design, Apache 2.0), izvor @cloudscape-design/design-tokens.
        // Vrijednosti su orijentacijske (iz plana redizajna) - ne preuzima se AWS narančasta/logo ni font Amazon Ember (vlasnički), ostaje Segoe UI.
        public static class Colors
        {
            // color-background-layout-main - u praksi (za razliku od plana) sam rub kartice (PaintCardBorder) nije bio
            // dovoljno vidljiv bez ikakvog kontrasta podloge ("ne vide se obrubi"), pa Background i Surface ipak nisu isti
            public static Color Background
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(11, 20, 31);
                    }

                    return Color.FromArgb(245, 245, 247);
                }
            }

            // color-background-container-content - suptilno svjetlija (tamna tema) / bjelja (svijetla tema) od Background,
            // uz PaintCardBorder rub za dodatnu definiciju
            public static Color Surface
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(23, 37, 55);
                    }

                    return Color.White;
                }
            }

            // top-navigation kontekst - bočna traka je uvijek tamna, neovisno o temi aplikacije (kao AWS konzola)
            public static Color Sidebar
            {
                get { return Color.FromArgb(15, 20, 26); }
            }

            public static Color SidebarText
            {
                get { return Color.FromArgb(242, 242, 242); }
            }

            public static Color SidebarMuted
            {
                get { return Color.FromArgb(180, 180, 180); }
            }

            // color-text-body-default
            public static Color Foreground
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(209, 213, 219);
                    }

                    return Color.FromArgb(0, 7, 22);
                }
            }

            // color-text-body-secondary
            public static Color Muted
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(141, 153, 168);
                    }

                    return Color.FromArgb(95, 107, 122);
                }
            }

            // color-border-divider-default - potamnjeno u odnosu na orijentacijski token (plan) jer je originalna
            // vrijednost bila premalo kontrastna za vidljivost u ovoj aplikaciji (rub kartica/tablica se nije vidio)
            public static Color Border
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(90, 105, 125);
                    }

                    return Color.FromArgb(160, 160, 170);
                }
            }

            public static Color Hover
            {
                get
                {
                    if (IsDark)
                    {
                        // izvorna vrijednost (30,44,61) je bila premalo kontrastna na Surface (23,37,55) - hover/odabir retka
                        // u tablicama (npr. Izvještaji) se jedva vidio (G-vidljivost tablice u tamnoj temi)
                        return Color.FromArgb(45, 64, 87);
                    }

                    return Color.FromArgb(236, 236, 240);
                }
            }

            // color-background-button-primary-default - jedina naglašena boja u sučelju (primarna radnja, odabrani modul, poveznice, fokus)
            public static Color Accent
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(83, 159, 229);
                    }

                    return Color.FromArgb(9, 114, 211);
                }
            }

            // color-background-button-primary-hover - tamnija u svijetloj temi, svjetlija u tamnoj (Cloudscape obrazac)
            public static Color AccentHover
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(137, 189, 238);
                    }

                    return Color.FromArgb(3, 49, 96);
                }
            }

            // naglašena boja prilagođena uvijek-tamnoj bočnoj traci, neovisno o temi aplikacije
            public static Color AccentOnDark
            {
                get { return Color.FromArgb(83, 159, 229); }
            }

            // tekst na primarnom gumbu - u tamnoj temi Accent postaje svjetliji plavi ton, pa tekst mora biti taman
            public static Color OnAccent
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(0, 7, 22);
                    }

                    return Color.White;
                }
            }

            // color-text-status-success
            public static Color Success
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(41, 173, 50);
                    }

                    return Color.FromArgb(3, 127, 12);
                }
            }

            // color-text-status-warning
            public static Color Warning
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(255, 227, 71);
                    }

                    return Color.FromArgb(133, 89, 0);
                }
            }

            // color-text-status-error
            public static Color Danger
            {
                get
                {
                    if (IsDark)
                    {
                        return Color.FromArgb(255, 93, 100);
                    }

                    return Color.FromArgb(217, 21, 21);
                }
            }

            public static Color Header
            {
                get { return Surface; }
            }
        }

        public static class Fonts
        {
            public static readonly Font Small = new Font("Segoe UI", 9.75f);
            public static readonly Font Note = new Font("Segoe UI", 9.75f, FontStyle.Italic);
            public static readonly Font Label = new Font("Segoe UI", 9.75f);
            public static readonly Font Body = new Font("Segoe UI", 9.75f);
            public static readonly Font Section = new Font("Segoe UI", 11f, FontStyle.Bold);
            public static readonly Font Metric = new Font("Segoe UI", 11f, FontStyle.Bold);
            public static readonly Font Title = new Font("Segoe UI", 14f, FontStyle.Bold);
            public static readonly Font Display = new Font("Segoe UI", 14f, FontStyle.Bold);
            public static readonly Font Button = new Font("Segoe UI", 9.75f);
            public static readonly Font Mono = new Font("Consolas", 9.75f);

            private static Font _icon;

            public static Font Icon
            {
                get
                {
                    if (_icon != null)
                    {
                        return _icon;
                    }

                    Font fluent = new Font("Segoe Fluent Icons", 16f);
                    if (fluent.Name == "Segoe Fluent Icons")
                    {
                        _icon = fluent;
                        return _icon;
                    }

                    fluent.Dispose();
                    Font mdl2 = new Font("Segoe MDL2 Assets", 16f);
                    if (mdl2.Name == "Segoe MDL2 Assets")
                    {
                        _icon = mdl2;
                        return _icon;
                    }

                    mdl2.Dispose();
                    _icon = new Font("Segoe UI Symbol", 16f);
                    return _icon;
                }
            }
        }

        public static class Icons
        {
            public const string Home = "\uE80F";
            public const string Cloud = "\uE753";
            public const string Server = "\uE968";
            public const string Money = "\uE8D4";
            public const string Report = "\uE9F9";
            public const string Speed = "\uEC4A";
            public const string Contact = "\uE77B";
            public const string People = "\uE716";
            public const string Settings = "\uE713";
            public const string Info = "\uE946";
            public const string Help = "\uE897";
            public const string SignOut = "\uF3B1";
        }

        public enum StatusKind
        {
            Neutral,
            Success,
            Warning,
            Error,
            Busy
        }

        private static readonly PropertyInfo DoubleBufferedProperty =
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);

        // Forme, UserControli i DataGridView nisu double-buffered prema zadanome (DoubleBuffered je protected na
        // Control, pa se izvana može uključiti samo refleksijom). Bez toga se kod ručnog povlačenja ruba prozora
        // vide "repovi" starog iscrtavanja (stare granice stupaca, prazno zaglavlje iza zadnjeg stupca) - u tamnoj
        // temi to izgleda kao vidljivo smeće jer sustav taj prostor iscrtava svijetlom/sistemskom bojom.
        public static void EnableDoubleBuffering(Control control)
        {
            if (control == null || DoubleBufferedProperty == null)
            {
                return;
            }

            try
            {
                DoubleBufferedProperty.SetValue(control, true, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod uključivanja double bufferinga: " + ex.Message);
            }
        }

        public static void Apply(Form form, params Control[] primaryButtons)
        {
            Apply((Control)form, primaryButtons);
        }

        public static void Apply(Control root, params Control[] primaryButtons)
        {
            if (root == null)
            {
                return;
            }

            EnableDoubleBuffering(root);

            root.Font = Fonts.Small;
            root.ForeColor = Colors.Foreground;

            if (root is Form form)
            {
                form.BackColor = Colors.Background;
                ApplyDarkTitleBar(form);
            }
            else if (root is UserControl)
            {
                root.BackColor = Colors.Background;
            }

            ApplyToControls(root.Controls);

            if (primaryButtons == null)
            {
                return;
            }

            foreach (Control control in primaryButtons)
            {
                Button button = control as Button;
                if (button != null)
                {
                    StylePrimaryButton(button);
                }
            }
        }

        public static void StylePrimaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Colors.AccentHover;
            button.Font = Fonts.Button;
            button.Cursor = Cursors.Hand;
            button.Padding = new Padding(Space1, 0, Space1, 0);

            RegisterButtonStyle(button, ButtonKind.Primary, Colors.Accent, Colors.OnAccent);
        }

        // destruktivna radnja (Obriši i sl.) - obrub i tekst u boji greške, ne puna podloga (Cloudscape obrazac za destruktivne radnje)
        public static void StyleDangerButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Colors.Danger;
            button.FlatAppearance.MouseOverBackColor = Colors.Hover;
            button.Font = Fonts.Button;
            button.Cursor = Cursors.Hand;

            RegisterButtonStyle(button, ButtonKind.Danger, Colors.Surface, Colors.Danger);
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Colors.Border;
            button.FlatAppearance.MouseOverBackColor = Colors.Hover;
            button.Font = Fonts.Button;
            button.Cursor = Cursors.Hand;

            RegisterButtonStyle(button, ButtonKind.Secondary, Colors.Surface, Colors.Foreground);
        }

        public static void StyleCard(Panel panel)
        {
            panel.BackColor = Colors.Surface;
            panel.Padding = new Padding(Space2);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hWnd);

        private static void ReadOnlyTextBox_GotFocus(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                HideCaret(textBox.Handle);
            }
        }

        private const int PbmSetbarcolor = 0x0409;
        private const int PbmSetbkcolor = 0x2001;

        // sistemska (svijetlosiva) traka napretka nema boju kroz obična svojstva (G6) - PBM_SETBARCOLOR/PBM_SETBKCOLOR
        // su jedini pouzdan način bez potpunog ručnog iscrtavanja; Continuous stil je preduvjet da poruke djeluju
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        public static void StyleProgressBar(ProgressBar bar)
        {
            try
            {
                bar.Style = ProgressBarStyle.Continuous;

                // vizualni stil (temirano iscrtavanje) ignorira PBM_SETBARCOLOR kod 100% i uvijek prikaže sistemsko zeleno -
                // isključivanje vizualnog stila za ovu kontrolu vraća se na klasično iscrtavanje koje boju stvarno poštuje
                SetWindowTheme(bar.Handle, string.Empty, string.Empty);

                SendMessage(bar.Handle, PbmSetbarcolor, IntPtr.Zero, (IntPtr)ColorTranslator.ToWin32(Colors.Accent));
                SendMessage(bar.Handle, PbmSetbkcolor, IntPtr.Zero, (IntPtr)ColorTranslator.ToWin32(Colors.Hover));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod bojanja trake napretka: " + ex.Message);
            }
        }

        // jedinstven stil tablica - zaglavlje s pozadinom kartice, vidljivi razdjelnici stupaca i redaka,
        // retci 32px, suptilna izmjena boje redaka i odabir u boji teme
        public static void StyleGrid(DataGridView grid)
        {
            EnableDoubleBuffering(grid);

            grid.BackgroundColor = Colors.Surface;
            grid.ForeColor = Colors.Foreground;
            grid.GridColor = Colors.Border;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToOrderColumns = false;
            grid.ReadOnly = true;
            grid.ShowCellToolTips = true;
            grid.TabStop = true;
            grid.AccessibleRole = AccessibleRole.Table;
            grid.RowTemplate.Height = 32;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            grid.DefaultCellStyle.BackColor = Colors.Surface;
            grid.DefaultCellStyle.ForeColor = Colors.Foreground;
            grid.DefaultCellStyle.SelectionBackColor = Colors.Hover;
            grid.DefaultCellStyle.SelectionForeColor = Colors.Foreground;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.Padding = new Padding(Space1, 0, Space1, 0);

            grid.AlternatingRowsDefaultCellStyle.BackColor = Colors.Background;
            grid.AlternatingRowsDefaultCellStyle.ForeColor = Colors.Foreground;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Colors.Hover;
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Colors.Foreground;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Colors.Surface;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Colors.Muted;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Colors.Surface;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Colors.Muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(Fonts.Small, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersHeight = 36;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        }

        public static void SetStatus(Label label, string text, StatusKind kind)
        {
            label.Text = text;
            label.ForeColor = StatusKindColor(kind);
        }

        // boja za StatusKind, neovisno o tekstu - forme koje boju statusnog labela postavljaju izravno (ne kroz
        // SetStatus) trebaju ovo pozvati i iz ApplyShellTheme, inače boja ostaje "zaleđena" na starom stanju
        // teme kad se tema promijeni dok su podaci već prikazani (ForeColor iz koda nije "zadana" boja pa je
        // ApplyToControls/IsDefaultForeColor ne osvježava automatski)
        public static Color StatusKindColor(StatusKind kind)
        {
            if (kind == StatusKind.Success)
            {
                return Colors.Success;
            }

            if (kind == StatusKind.Warning)
            {
                return Colors.Warning;
            }

            if (kind == StatusKind.Error)
            {
                return Colors.Danger;
            }

            if (kind == StatusKind.Busy)
            {
                return Colors.Accent;
            }

            return Colors.Muted;
        }

        public static Color StatusForeColor(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return Colors.Foreground;
            }

            string lower = status.ToLowerInvariant();

            if (lower.Contains("running") || lower.Contains("completed") || lower.Contains("online") || lower.Contains("success") || lower.Contains("ok"))
            {
                return Colors.Success;
            }

            if (lower.Contains("stopped") || lower.Contains("pending") || lower.Contains("warning"))
            {
                return Colors.Warning;
            }

            if (lower.Contains("fail") || lower.Contains("error") || lower.Contains("terminat") || lower.Contains("offline"))
            {
                return Colors.Danger;
            }

            return Colors.Foreground;
        }

        public static void PaintCardBorder(Control control, PaintEventArgs e)
        {
            Rectangle bounds = control.ClientRectangle;
            bounds.Width -= 1;
            bounds.Height -= 1;

            using (Pen pen = new Pen(Colors.Border))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, bounds);
            }
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                switch (control)
                {
                    case Button button:
                        if (IsAccentButton(button) == false && IsDangerButton(button) == false)
                        {
                            StyleSecondaryButton(button);
                        }
                        break;

                    case TextBox textBox:
                        if (textBox.ReadOnly)
                        {
                            // bez ruba i I-belt kursora - čita se kao obični tekst, ne kao polje za unos
                            textBox.BackColor = Colors.Background;
                            textBox.BorderStyle = BorderStyle.None;
                            textBox.Cursor = Cursors.Default;

                            // ReadOnly i dalje prima fokus i trepće tekstualni kursor (G10) - I-beam/rub gore rješava izgled miša i ruba,
                            // ovo sakriva sam trepćući kursor kad polje dobije fokus (npr. klikom ili tabom)
                            textBox.GotFocus -= ReadOnlyTextBox_GotFocus;
                            textBox.GotFocus += ReadOnlyTextBox_GotFocus;
                        }
                        else
                        {
                            textBox.BackColor = Colors.Surface;
                            textBox.BorderStyle = BorderStyle.FixedSingle;
                        }

                        if (IsDefaultForeColor(textBox.ForeColor))
                        {
                            textBox.ForeColor = Colors.Foreground;
                        }
                        break;

                    case ComboBox comboBox:
                        // FlatStyle.Flat na uređivom ComboBoxu (DropDownStyle.DropDown) ne oboji ugrađeno polje za unos teksta -
                        // ostaje sistemski bijelo dok se ne počne tipkati (poznata WinForms greška), zato takav ostaje Standard
                        if (comboBox.DropDownStyle != ComboBoxStyle.DropDown)
                        {
                            comboBox.FlatStyle = FlatStyle.Flat;
                        }

                        comboBox.BackColor = Colors.Surface;
                        comboBox.ForeColor = Colors.Foreground;
                        break;

                    case NumericUpDown numericUpDown:
                        numericUpDown.BackColor = Colors.Surface;
                        numericUpDown.ForeColor = Colors.Foreground;
                        break;

                    case ProgressBar progressBar:
                        StyleProgressBar(progressBar);
                        break;

                    case CheckBox checkBox:
                        checkBox.ForeColor = Colors.Foreground;
                        checkBox.BackColor = Color.Transparent;
                        break;

                    case RadioButton radioButton:
                        radioButton.ForeColor = Colors.Foreground;
                        radioButton.BackColor = Color.Transparent;
                        break;

                    case ListBox listBox:
                        listBox.BackColor = Colors.Surface;
                        listBox.ForeColor = Colors.Foreground;
                        break;

                    case DataGridView grid:
                        StyleGrid(grid);
                        break;

                    case Label label:
                        if (label.ForeColor == SystemColors.GrayText)
                        {
                            label.ForeColor = Colors.Muted;
                        }
                        else if (IsDefaultForeColor(label.ForeColor))
                        {
                            label.ForeColor = Colors.Foreground;
                        }
                        break;

                    case GroupBox groupBox:
                        groupBox.ForeColor = Colors.Foreground;
                        ApplyToControls(groupBox.Controls);
                        break;

                    case TabControl tabControl:
                        tabControl.Font = Fonts.Small;
                        ApplyToControls(tabControl.Controls);
                        break;

                    case TabPage tabPage:
                        tabPage.BackColor = Colors.Background;
                        tabPage.ForeColor = Colors.Foreground;
                        ApplyToControls(tabPage.Controls);
                        break;

                    case TableLayoutPanel table:
                        if (table.BackColor == SystemColors.Control || table.BackColor.IsEmpty)
                        {
                            table.BackColor = Colors.Background;
                        }
                        ApplyToControls(table.Controls);
                        break;

                    case FlowLayoutPanel flow:
                        if (flow.BackColor == SystemColors.Control || flow.BackColor.IsEmpty)
                        {
                            flow.BackColor = Colors.Background;
                        }
                        ApplyToControls(flow.Controls);
                        break;

                    case Panel panel:
                        if (panel.BackColor == SystemColors.Control || panel.BackColor.IsEmpty)
                        {
                            panel.BackColor = Colors.Background;
                        }
                        ApplyToControls(panel.Controls);
                        break;

                    case SplitContainer split:
                        split.BackColor = Colors.Background;
                        ApplyToControls(split.Controls);
                        break;

                    case UserControl userControl:
                        userControl.BackColor = Colors.Background;
                        userControl.ForeColor = Colors.Foreground;
                        ApplyToControls(userControl.Controls);
                        break;

                    default:
                        ApplyToControls(control.Controls);
                        break;
                }
            }
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeOld = 19; // stariji Windows 10 build-ovi

        // tamna naslovna traka prozora prema trenutnoj temi - poziva se kod otvaranja i kad se tema promijeni dok je prozor otvoren;
        // treba postojeći handle prozora, pa se kod jos nestvorenog handlea odgađa na HandleCreated
        public static void ApplyDarkTitleBar(Form form)
        {
            if (form == null)
            {
                return;
            }

            if (form.IsHandleCreated == false)
            {
                form.HandleCreated += (s, e) => SetDarkTitleBar(form.Handle);
                return;
            }

            SetDarkTitleBar(form.Handle);
        }

        private static void SetDarkTitleBar(IntPtr handle)
        {
            try
            {
                int useDark = IsDark ? 1 : 0;
                int result = DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref useDark, sizeof(int));

                if (result != 0)
                {
                    DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkModeOld, ref useDark, sizeof(int));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod postavljanja tamne naslovne trake: " + ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // Identitet gumba (Primary/Danger/Secondary) i boje po Enabled stanju
        //
        // Otkad je destruktivni gumb bez pune podloge (vidi StyleDangerButton), BackColor više ne razlikuje
        // vrste gumba pa se identitet pamti eksplicitno umjesto uspoređivanja boja. ConditionalWeakTable ne
        // sprječava garbage collection gumba (za razliku od običnog Dictionary-ja), pa nema curenja memorije
        // kroz dug životni vijek aplikacije s više prijava/odjava.

        private enum ButtonKind
        {
            Secondary,
            Primary,
            Danger
        }

        private sealed class ButtonStyleInfo
        {
            public ButtonKind Kind;
            public Color EnabledBack;
            public Color EnabledFore;
        }

        private static readonly ConditionalWeakTable<Button, ButtonStyleInfo> ButtonStyles = new ConditionalWeakTable<Button, ButtonStyleInfo>();

        private static void RegisterButtonStyle(Button button, ButtonKind kind, Color enabledBackColor, Color enabledForeColor)
        {
            ButtonStyleInfo info = ButtonStyles.GetValue(button, _ => new ButtonStyleInfo());
            info.Kind = kind;
            info.EnabledBack = enabledBackColor;
            info.EnabledFore = enabledForeColor;

            button.EnabledChanged -= Button_EnabledChanged;
            button.EnabledChanged += Button_EnabledChanged;

            ApplyButtonColorsForState(button, info);
        }

        private static void Button_EnabledChanged(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (ButtonStyles.TryGetValue(button, out ButtonStyleInfo info))
            {
                ApplyButtonColorsForState(button, info);
            }
        }

        private static void ApplyButtonColorsForState(Button button, ButtonStyleInfo info)
        {
            if (button.Enabled)
            {
                button.BackColor = info.EnabledBack;
                button.ForeColor = info.EnabledFore;
                return;
            }

            // Flat gumbi kod Enabled=false iscrtavaju prigušen tekst neovisno o ForeColor - dosad su onemogućeni
            // gumbi zadržavali punu obojenu podlogu (Accent/Danger) pa je taj tekst gotovo nestajao (G13); neutralna
            // prigušena podloga daje dovoljno kontrasta u obje teme
            button.BackColor = Colors.Hover;
            button.ForeColor = Colors.Muted;
        }

        private static bool IsAccentButton(Button button)
        {
            return ButtonStyles.TryGetValue(button, out ButtonStyleInfo info) && info.Kind == ButtonKind.Primary;
        }

        private static bool IsDangerButton(Button button)
        {
            return ButtonStyles.TryGetValue(button, out ButtonStyleInfo info) && info.Kind == ButtonKind.Danger;
        }

        private static bool IsDefaultForeColor(Color color)
        {
            return color.IsEmpty
                || color == SystemColors.ControlText
                || color == SystemColors.WindowText
                || color == SystemColors.GrayText
                || color == Color.Black;
        }
    }
}
