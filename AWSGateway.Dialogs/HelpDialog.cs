using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AWSGateway.Dialogs
{
    // prikazuje se ugrađeno u ljusku (ModuleHost, TopLevel = false) - ova biblioteka ne smije ovisiti o glavnom projektu
    // (UiTheme/LanguageHelper), pa glavni projekt poglavlja priprema iz Lang datoteka i predaje kroz SetContent (refleksijom)
    public partial class HelpDialog : Form
    {
        private readonly SplitContainer _split;
        private readonly ListBox _lstChapters;
        private readonly TextBox _txtBody;

        private List<string> _bodies = new List<string>();
        private Color _selectedBackColor = SystemColors.Highlight;
        private Color _selectedForeColor = SystemColors.HighlightText;

        public HelpDialog()
        {
            InitializeComponent();

            _split = new SplitContainer();
            _split.Dock = DockStyle.Fill;
            _split.FixedPanel = FixedPanel.Panel1;
            _split.SplitterWidth = 4;

            _lstChapters = new ListBox();
            _lstChapters.Dock = DockStyle.Fill;
            _lstChapters.BorderStyle = BorderStyle.None;
            _lstChapters.IntegralHeight = false;
            _lstChapters.DrawMode = DrawMode.OwnerDrawFixed;
            _lstChapters.ItemHeight = 32;
            _lstChapters.Font = new Font("Segoe UI", 10F);
            _lstChapters.DrawItem += LstChapters_DrawItem;
            _lstChapters.SelectedIndexChanged += LstChapters_SelectedIndexChanged;
            _split.Panel1.Controls.Add(_lstChapters);
            _split.Panel1.Padding = new Padding(8);
            _split.Panel1MinSize = 160;

            _txtBody = new TextBox();
            _txtBody.Dock = DockStyle.Fill;
            _txtBody.Multiline = true;
            _txtBody.ReadOnly = true;
            _txtBody.BorderStyle = BorderStyle.None;
            _txtBody.ScrollBars = ScrollBars.Vertical;
            _txtBody.Font = new Font("Segoe UI", 11F);
            _txtBody.GotFocus += ReadOnlyTextBox_GotFocus;
            _split.Panel2.Controls.Add(_txtBody);
            _split.Panel2.Padding = new Padding(20, 8, 20, 8);

            Controls.Add(_split);

            // SplitterDistance se ne smije postaviti prije nego je kontrola vidljiva s pravom širinom - inače baca iznimku
            HandleCreated += (s, e) => _split.SplitterDistance = 220;
        }

        // poziva se refleksijom iz MainForm-a (kod otvaranja stranice, kod F1 s druge stranice, i kod svake promjene jezika/teme dok je otvorena)
        public void SetContent(List<string> chapterTitles, List<string> chapterBodies, int initialIndex, Color selectedBackColor, Color selectedForeColor)
        {
            _bodies = chapterBodies;
            _selectedBackColor = selectedBackColor;
            _selectedForeColor = selectedForeColor;

            _lstChapters.SelectedIndexChanged -= LstChapters_SelectedIndexChanged;
            _lstChapters.Items.Clear();

            foreach (string title in chapterTitles)
            {
                _lstChapters.Items.Add(title);
            }

            _lstChapters.SelectedIndexChanged += LstChapters_SelectedIndexChanged;

            if (chapterTitles.Count == 0)
            {
                _txtBody.Text = string.Empty;
                return;
            }

            int index = initialIndex;

            if (index < 0 || index >= chapterTitles.Count)
            {
                index = 0;
            }

            _lstChapters.SelectedIndex = index;
            ShowChapter(index);
            _lstChapters.Invalidate();
        }

        private void LstChapters_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ShowChapter(_lstChapters.SelectedIndex);
        }

        private void ShowChapter(int index)
        {
            if (index < 0 || index >= _bodies.Count)
            {
                return;
            }

            _txtBody.Text = _bodies[index];
            _txtBody.SelectionStart = 0;
            _txtBody.SelectionLength = 0;
        }

        // sistemsko plavo označavanje se ne koristi nigdje drugdje u aplikaciji - isto pravilo i ovdje (boje stižu kroz SetContent)
        private void LstChapters_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor;
            Color foreColor;

            if (selected)
            {
                backColor = _selectedBackColor;
                foreColor = _selectedForeColor;
            }
            else
            {
                backColor = _lstChapters.BackColor;
                foreColor = _lstChapters.ForeColor;
            }

            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            Rectangle textBounds = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, _lstChapters.Items[e.Index].ToString(), _lstChapters.Font, textBounds, foreColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
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
