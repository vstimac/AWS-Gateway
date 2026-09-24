using AWSGateway.Helpers;
using System.ComponentModel;

namespace AWSGateway.Forms
{
    // stavka bočne navigacije: Fluent/MDL2 glif + natpis
    public class NavButton : UserControl
    {
        private readonly Label _icon;
        private readonly Label _caption;
        private bool _selected;
        private string _moduleKey = string.Empty;

        public event EventHandler NavClick;

        public NavButton()
        {
            Height = UiTheme.NavItemHeight;
            Cursor = Cursors.Hand;
            Margin = new Padding(UiTheme.Space1, 2, UiTheme.Space1, 2);
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;

            _icon = new Label();
            _icon.AutoSize = false;
            _icon.Dock = DockStyle.Left;
            _icon.Width = 40;
            _icon.TextAlign = ContentAlignment.MiddleCenter;
            _icon.Font = UiTheme.Fonts.Icon;

            _caption = new Label();
            _caption.AutoSize = false;
            _caption.Dock = DockStyle.Fill;
            _caption.TextAlign = ContentAlignment.MiddleLeft;
            _caption.Font = UiTheme.Fonts.Small;

            // natpis nema tipkovnički mnemonik - bez ovoga se "&" u tekstu tiho gubi i ostavlja dvostruki razmak
            _caption.UseMnemonic = false;

            Controls.Add(_caption);
            Controls.Add(_icon);

            _icon.Click += OnClicked;
            _caption.Click += OnClicked;
            Click += OnClicked;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ModuleKey
        {
            get { return _moduleKey; }
            set { _moduleKey = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string IconGlyph
        {
            get { return _icon.Text; }
            set { _icon.Text = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Caption
        {
            get { return _caption.Text; }
            set
            {
                _caption.Text = value;
                AccessibleName = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                ApplyColors();
            }
        }

        public void ApplyColors()
        {
            if (_selected)
            {
                // Color.Transparent/alpha ARGB na standardnim kontrolama u FlowLayoutPanelu se ne iscrtava pouzdano
                // (poznata WinForms greška - zna ostati bijelo) - zato se koristi stvarna, izračunata boja
                BackColor = BlendWithSidebar(UiTheme.Colors.AccentOnDark, 60);
                _caption.Font = UiTheme.Fonts.Button;
            }
            else
            {
                BackColor = UiTheme.Colors.Sidebar;
                _caption.Font = UiTheme.Fonts.Small;
            }

            _icon.ForeColor = UiTheme.Colors.AccentOnDark;
            _caption.ForeColor = UiTheme.Colors.SidebarText;

            // _icon i _caption prekrivaju cijelu površinu (Dock Left/Fill) - moraju pratiti istu boju kao BackColor gore,
            // inače podloga odabranog stanja ostaje nevidljiva iza njih (G4 - odabrani modul bez podloge)
            _icon.BackColor = BackColor;
            _caption.BackColor = BackColor;
            Invalidate();
        }

        // ručno izračunata neprozirna boja umjesto alpha-blend ARGB (vidi komentar iznad)
        private static Color BlendWithSidebar(Color overlay, int alpha)
        {
            Color baseColor = UiTheme.Colors.Sidebar;
            double a = alpha / 255.0;

            int r = (int)(baseColor.R * (1 - a) + overlay.R * a);
            int g = (int)(baseColor.G * (1 - a) + overlay.G * a);
            int b = (int)(baseColor.B * (1 - a) + overlay.B * a);

            return Color.FromArgb(r, g, b);
        }

        private void OnClicked(object sender, EventArgs e)
        {
            Focus();

            if (NavClick != null)
            {
                NavClick(this, EventArgs.Empty);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OnClicked(this, EventArgs.Empty);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Focused)
            {
                using (Pen pen = new Pen(UiTheme.Colors.AccentOnDark, 2f))
                {
                    Rectangle bounds = ClientRectangle;
                    bounds.Width -= 1;
                    bounds.Height -= 1;
                    e.Graphics.DrawRectangle(pen, bounds);
                }
            }
        }
    }
}
