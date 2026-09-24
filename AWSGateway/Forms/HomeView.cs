using System.Runtime.InteropServices;
using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;
using AWSGateway.Utils;
using System.Diagnostics;

namespace AWSGateway.Forms
{
    // početni pregled u ljusci - stanje sesije/računa/servera, zadnji prijenosi, nedavna aktivnost.
    // sve iz lokalnih podataka (baza, JSON dnevnik, već postojeće stanje ljuske) - bez ijednog novog AWS/TCP poziva
    // (trošak namjerno nije ovdje - vidi Troškovi)
    // raspored se računa u kodu (LayoutView) prema stvarnoj širini pogleda - kartice se nikad ne šire preko desnog ruba
    public class HomeView : UserControl, IAppModule
    {
        private const int MaxTransfers = 5;
        private const int MaxActivity = 5;

        // ispod ove širine sadržaja pogled dobiva vodoravni klizač umjesto da se kartice stisnu
        private const int MinContentWidth = 640;

        private readonly AWSProfile _profile;
        private readonly MainForm _shell;

        private readonly Label _title;
        private readonly Label _hint;

        // ---------------------------------------------------------------
        // Gornji red - tri kartice stanja (Sesija, AWS račun, Lokalni servis)

        private readonly Panel _sessionCard;
        private readonly Label _lblSessionTitle;
        private readonly Label _lblLoginMode;
        private readonly Label _lblSessionExpiry;
        private readonly Label _lblSessionRegion;

        private readonly Panel _accountCard;
        private readonly Label _lblAccountTitle;
        private readonly Label _lblAccountNumber;
        private readonly Button _btnCopyAccount;
        private readonly Label _lblAccountIdentity;

        private readonly Panel _serverCard;
        private readonly Label _lblServerTitle;
        private readonly Label _lblServerStatus;

        // ---------------------------------------------------------------
        // Srednji red - zadnji prijenosi (preko cijele širine)

        private readonly Panel _transfersCard;
        private readonly Label _lblTransfersTitle;
        private readonly Label _lnkTransfersShowAll;
        private readonly DataGridView _transfersList;
        private readonly Label _lblTransfersEmpty;

        // ---------------------------------------------------------------
        // Donji red - nedavna aktivnost

        private readonly Panel _activityCard;
        private readonly Label _lblActivityTitle;
        private readonly DataGridView _activityList;
        private readonly Label _lblActivityEmpty;

        // Visible vraća false i kad je cijeli pogled skriven - zato se stanje popisa pamti zasebno
        private bool _hasTransfers = false;
        private bool _hasActivity = false;

        // postavljanje veličine klizača mijenja ClientSize i ponovno izaziva Resize - sprječava rekurziju
        private bool _isLayingOut = false;

        public HomeView(AWSProfile profile, MainForm shell)
        {
            _profile = profile;
            _shell = shell;

            Dock = DockStyle.Fill;
            AutoScroll = true;
            Padding = Padding.Empty;

            _title = CreateLabel(UiTheme.Fonts.Display);
            _hint = CreateLabel(UiTheme.Fonts.Body);

            // ---------------------------------------------------------------
            // Sesija - MainForm već prati preostalo vrijeme, ovdje se samo čita/prikazuje

            _sessionCard = CreateCard();
            _lblSessionTitle = CreateLabel(UiTheme.Fonts.Title);
            _lblLoginMode = CreateLabel(UiTheme.Fonts.Body);
            _lblSessionExpiry = CreateLabel(UiTheme.Fonts.Body);
            _lblSessionRegion = CreateLabel(UiTheme.Fonts.Body);

            _sessionCard.Controls.Add(_lblSessionTitle);
            _sessionCard.Controls.Add(_lblLoginMode);
            _sessionCard.Controls.Add(_lblSessionExpiry);
            _sessionCard.Controls.Add(_lblSessionRegion);

            // ---------------------------------------------------------------
            // AWS račun - broj računa i identitet iz sesije, bez novog AWS poziva

            _accountCard = CreateCard();
            _lblAccountTitle = CreateLabel(UiTheme.Fonts.Title);
            _lblAccountNumber = CreateLabel(UiTheme.Fonts.Body);

            // gumb stoji u istom retku desno od broja računa - veličinu mu određuje LayoutView prema tekstu
            _btnCopyAccount = new Button();
            _btnCopyAccount.Font = UiTheme.Fonts.Small;
            _btnCopyAccount.Click += BtnCopyAccount_Click;

            _lblAccountIdentity = CreateLabel(UiTheme.Fonts.Small);

            _accountCard.Controls.Add(_lblAccountTitle);
            _accountCard.Controls.Add(_lblAccountNumber);
            _accountCard.Controls.Add(_btnCopyAccount);
            _accountCard.Controls.Add(_lblAccountIdentity);

            // ---------------------------------------------------------------
            // Lokalni servis - stanje TCP servera koje MainForm već prati

            _serverCard = CreateCard();
            _lblServerTitle = CreateLabel(UiTheme.Fonts.Title);
            _lblServerStatus = CreateLabel(UiTheme.Fonts.Body);

            _serverCard.Controls.Add(_lblServerTitle);
            _serverCard.Controls.Add(_lblServerStatus);

            // ---------------------------------------------------------------
            // Zadnji prijenosi - stupci: smjer, naziv, status, vrijeme

            _transfersCard = CreateCard();
            _lblTransfersTitle = CreateLabel(UiTheme.Fonts.Title);

            _lnkTransfersShowAll = CreateLabel(UiTheme.Fonts.Small);
            _lnkTransfersShowAll.TextAlign = ContentAlignment.MiddleRight;
            _lnkTransfersShowAll.Cursor = Cursors.Hand;
            _lnkTransfersShowAll.Click += (s, e) => _shell.NavigateToModule(MainForm.ModuleReports);

            _transfersList = CreateGrid();
            _transfersList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.AllCells, DataGridViewContentAlignment.MiddleLeft));
            _transfersList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.Fill, DataGridViewContentAlignment.MiddleLeft));
            _transfersList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.AllCells, DataGridViewContentAlignment.MiddleLeft));
            _transfersList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.AllCells, DataGridViewContentAlignment.MiddleRight));

            // dvoklik ili Enter na prijenos otvara izvještaje, gdje su sve akcije nad prijenosom
            _transfersList.CellDoubleClick += (s, e) => _shell.NavigateToModule(MainForm.ModuleReports);
            _transfersList.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && _transfersList.CurrentRow != null)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    _shell.NavigateToModule(MainForm.ModuleReports);
                }
            };

            _lblTransfersEmpty = CreateLabel(UiTheme.Fonts.Small);

            _transfersCard.Controls.Add(_lblTransfersTitle);
            _transfersCard.Controls.Add(_lnkTransfersShowAll);
            _transfersCard.Controls.Add(_transfersList);
            _transfersCard.Controls.Add(_lblTransfersEmpty);

            // ---------------------------------------------------------------
            // Nedavna aktivnost (zadnjih 5 zapisa iz vlastitog JSON dnevnika, bez timskog TCP poziva) - stupci: vrijeme, akcija, detalji

            _activityCard = CreateCard();
            _lblActivityTitle = CreateLabel(UiTheme.Fonts.Title);

            _activityList = CreateGrid();
            _activityList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.AllCells, DataGridViewContentAlignment.MiddleLeft));
            _activityList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.AllCells, DataGridViewContentAlignment.MiddleLeft));
            _activityList.Columns.Add(CreateColumn(DataGridViewAutoSizeColumnMode.Fill, DataGridViewContentAlignment.MiddleLeft));

            _lblActivityEmpty = CreateLabel(UiTheme.Fonts.Small);

            _activityCard.Controls.Add(_lblActivityTitle);
            _activityCard.Controls.Add(_activityList);
            _activityCard.Controls.Add(_lblActivityEmpty);

            Controls.Add(_title);
            Controls.Add(_hint);
            Controls.Add(_sessionCard);
            Controls.Add(_accountCard);
            Controls.Add(_serverCard);
            Controls.Add(_transfersCard);
            Controls.Add(_activityCard);

            ApplyShellLanguage();
            ApplyShellTheme();
        }

        public bool IsBusy
        {
            get { return false; }
        }

        public void OnActivated()
        {
            RefreshData();
        }

        public void ApplyShellLanguage()
        {
            _title.Text = LanguageHelper.Get("nav_home");
            _hint.Text = LanguageHelper.Get("home_hint");
            _lblSessionTitle.Text = LanguageHelper.Get("home_session_title");
            _lblAccountTitle.Text = LanguageHelper.Get("home_account_title");
            _lblServerTitle.Text = LanguageHelper.Get("home_server_title");
            _lblTransfersTitle.Text = LanguageHelper.Get("home_transfers_title");
            _lnkTransfersShowAll.Text = LanguageHelper.Get("home_transfers_show_all");
            _lblTransfersEmpty.Text = LanguageHelper.Get("home_transfers_empty");
            _lblActivityTitle.Text = LanguageHelper.Get("home_activity_title");
            _lblActivityEmpty.Text = LanguageHelper.Get("home_activity_empty");

            // sadržaj koji nosi stvarne podatke (i natpis gumba za kopiranje) osvježava se zasebno; RefreshData na kraju slaže raspored
            RefreshData();
        }

        public void ApplyShellTheme()
        {
            UiTheme.Apply(this);
            UiTheme.StyleCard(_sessionCard);
            UiTheme.StyleCard(_accountCard);
            UiTheme.StyleCard(_serverCard);
            UiTheme.StyleCard(_transfersCard);
            UiTheme.StyleCard(_activityCard);

            // tema može promijeniti fontove kontrola - pogled ih vraća na svoju hijerarhiju (naslov, naslov kartice, tekst, sitni tekst)
            _title.Font = UiTheme.Fonts.Display;
            _hint.Font = UiTheme.Fonts.Body;
            _lblSessionTitle.Font = UiTheme.Fonts.Title;
            _lblAccountTitle.Font = UiTheme.Fonts.Title;
            _lblServerTitle.Font = UiTheme.Fonts.Title;
            _lblTransfersTitle.Font = UiTheme.Fonts.Title;
            _lblActivityTitle.Font = UiTheme.Fonts.Title;
            _lblLoginMode.Font = UiTheme.Fonts.Body;
            _lblSessionExpiry.Font = UiTheme.Fonts.Body;
            _lblSessionRegion.Font = UiTheme.Fonts.Body;
            _lblAccountNumber.Font = UiTheme.Fonts.Body;
            _lblServerStatus.Font = UiTheme.Fonts.Body;
            _lblAccountIdentity.Font = UiTheme.Fonts.Small;
            _lnkTransfersShowAll.Font = UiTheme.Fonts.Small;
            _lblTransfersEmpty.Font = UiTheme.Fonts.Small;
            _lblActivityEmpty.Font = UiTheme.Fonts.Small;
            _btnCopyAccount.Font = UiTheme.Fonts.Small;

            _title.ForeColor = UiTheme.Colors.Foreground;
            _hint.ForeColor = UiTheme.Colors.Muted;
            _lnkTransfersShowAll.ForeColor = UiTheme.Colors.Accent;
            _lblTransfersEmpty.ForeColor = UiTheme.Colors.Muted;
            _lblAccountIdentity.ForeColor = UiTheme.Colors.Muted;
            _lblActivityEmpty.ForeColor = UiTheme.Colors.Muted;

            ApplyGridTheme(_transfersList, _transfersCard);
            ApplyGridTheme(_activityList, _activityCard);

            // visina retka ovisi o fontu - retci se ponovno dodaju s novom visinom
            RefreshData();

            _sessionCard.Invalidate();
            _accountCard.Invalidate();
            _serverCard.Invalidate();
            _transfersCard.Invalidate();
            _activityCard.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutView();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            LayoutView();
        }

        // ---------------------------------------------------------------
        // Izrada kontrola

        private static Label CreateLabel(Font font)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Font = font;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private static Panel CreateCard()
        {
            Panel card = new Panel();
            card.Paint += (s, e) => UiTheme.PaintCardBorder(card, e);
            return card;
        }

        // popis samo za prikaz - bez zaglavlja, klizača i uređivanja; visinu mu određuje broj redaka
        private static DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView();
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersVisible = false;
            grid.RowHeadersVisible = false;
            grid.ScrollBars = ScrollBars.None;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.TabStop = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeColumns = false;
            grid.AllowUserToResizeRows = false;
            grid.AllowUserToOrderColumns = false;
            grid.EnableHeadersVisualStyles = false;
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            return grid;
        }

        private static DataGridViewTextBoxColumn CreateColumn(DataGridViewAutoSizeColumnMode sizeMode, DataGridViewContentAlignment alignment)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.AutoSizeMode = sizeMode;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            column.DefaultCellStyle.Alignment = alignment;
            return column;
        }

        // popis koristi boju kartice, a odabrani redak koristi zajedničku hover boju radi jasnog fokusa
        private void ApplyGridTheme(DataGridView grid, Panel card)
        {
            Color background = card.BackColor;
            int cellPadding = LogicalToDeviceUnits(UiTheme.Space1);

            grid.BackgroundColor = background;
            grid.GridColor = Blend(background, UiTheme.Colors.Muted, 0.25f);

            grid.DefaultCellStyle.BackColor = background;
            grid.DefaultCellStyle.ForeColor = UiTheme.Colors.Foreground;
            grid.DefaultCellStyle.SelectionBackColor = UiTheme.Colors.Hover;
            grid.DefaultCellStyle.SelectionForeColor = UiTheme.Colors.Foreground;
            grid.DefaultCellStyle.Font = UiTheme.Fonts.Body;
            grid.DefaultCellStyle.Padding = new Padding(cellPadding, 0, cellPadding, 0);

            grid.AlternatingRowsDefaultCellStyle.BackColor = background;
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = UiTheme.Colors.Hover;

            grid.RowTemplate.Height = LineHeight(UiTheme.Fonts.Body) + LogicalToDeviceUnits(UiTheme.Space2);
        }

        // miješa dvije boje - amount 0 = prva boja, 1 = druga boja
        private static Color Blend(Color first, Color second, float amount)
        {
            int red = (int)(first.R + (second.R - first.R) * amount);
            int green = (int)(first.G + (second.G - first.G) * amount);
            int blue = (int)(first.B + (second.B - first.B) * amount);
            return Color.FromArgb(red, green, blue);
        }

        // visina jednog retka teksta za zadani font
        private static int LineHeight(Font font)
        {
            return TextRenderer.MeasureText("Ag", font).Height;
        }

        private static int TextWidth(string text, Font font)
        {
            return TextRenderer.MeasureText(text, font).Width;
        }

        // ---------------------------------------------------------------
        // Raspored

        private void LayoutView()
        {
            if (_isLayingOut)
            {
                return;
            }

            _isLayingOut = true;

            try
            {
                int widthBefore = ClientSize.Width;

                LayoutCore();

                // pojava ili nestanak okomitog klizača mijenja širinu - raspored se slaže još jednom za novu širinu
                if (ClientSize.Width != widthBefore)
                {
                    LayoutCore();
                }
            }
            finally
            {
                _isLayingOut = false;
            }
        }

        // slaže pogled odozgo prema dolje: naslov, tri kartice stanja u redu, zadnji prijenosi, nedavna aktivnost
        // koordinate se računaju u prostoru sadržaja; PlaceInView dodaje pomak klizača za kontrole izravno na pogledu
        private void LayoutCore()
        {
            int outer = LogicalToDeviceUnits(UiTheme.Space3);
            int gap = LogicalToDeviceUnits(UiTheme.Space2);
            int inner = LogicalToDeviceUnits(UiTheme.Space2);
            int small = LogicalToDeviceUnits(UiTheme.Space1);

            int contentWidth = Math.Max(ClientSize.Width - outer * 2, LogicalToDeviceUnits(MinContentWidth));

            int titleLine = LineHeight(UiTheme.Fonts.Title);
            int bodyLine = LineHeight(UiTheme.Fonts.Body);
            int smallLine = LineHeight(UiTheme.Fonts.Small);

            SuspendLayout();

            // naslov i opis
            int y = outer;
            PlaceInView(_title, outer, y, contentWidth, LineHeight(UiTheme.Fonts.Display));
            y = y + _title.Height + small;
            PlaceInView(_hint, outer, y, contentWidth, bodyLine);
            y = y + _hint.Height + gap;

            // gornji red - tri kartice iste visine; zadnja popunjava ostatak širine (zaokruživanje dijeljenja s 3)
            int cardWidth = (contentWidth - gap * 2) / 3;
            int lastCardWidth = contentWidth - (cardWidth + gap) * 2;

            int sessionHeight = LayoutSessionCard(cardWidth - inner * 2, inner, small, titleLine, bodyLine);
            int accountHeight = LayoutAccountCard(cardWidth - inner * 2, inner, small, titleLine, bodyLine, smallLine);
            int serverHeight = LayoutServerCard(lastCardWidth - inner * 2, inner, small, titleLine, bodyLine);

            int topRowHeight = Math.Max(sessionHeight, Math.Max(accountHeight, serverHeight));

            PlaceInView(_sessionCard, outer, y, cardWidth, topRowHeight);
            PlaceInView(_accountCard, outer + cardWidth + gap, y, cardWidth, topRowHeight);
            PlaceInView(_serverCard, outer + (cardWidth + gap) * 2, y, lastCardWidth, topRowHeight);

            y = y + topRowHeight + gap;

            // zadnji prijenosi - poveznica "prikaži sve" u istom retku kao naslov, uz desni rub
            int fullInnerWidth = contentWidth - inner * 2;
            int linkWidth = TextWidth(_lnkTransfersShowAll.Text, _lnkTransfersShowAll.Font) + small;

            int cy = inner;
            _lblTransfersTitle.SetBounds(inner, cy, fullInnerWidth - linkWidth - gap, titleLine);
            _lnkTransfersShowAll.SetBounds(inner + fullInnerWidth - linkWidth, cy + (titleLine - smallLine) / 2, linkWidth, smallLine);
            cy = cy + titleLine + small;
            cy = LayoutList(_transfersList, _lblTransfersEmpty, _hasTransfers, inner, cy, fullInnerWidth, smallLine);

            int transfersHeight = cy + inner;
            PlaceInView(_transfersCard, outer, y, contentWidth, transfersHeight);
            y = y + transfersHeight + gap;

            // nedavna aktivnost
            cy = inner;
            _lblActivityTitle.SetBounds(inner, cy, fullInnerWidth, titleLine);
            cy = cy + titleLine + small;
            cy = LayoutList(_activityList, _lblActivityEmpty, _hasActivity, inner, cy, fullInnerWidth, smallLine);

            int activityHeight = cy + inner;
            PlaceInView(_activityCard, outer, y, contentWidth, activityHeight);
            y = y + activityHeight + outer;

            // klizač se pojavljuje samo kad sadržaj ne stane u pogled
            AutoScrollMinSize = new Size(contentWidth + outer * 2, y);

            ResumeLayout(true);

            _sessionCard.Invalidate();
            _accountCard.Invalidate();
            _serverCard.Invalidate();
            _transfersCard.Invalidate();
            _activityCard.Invalidate();
        }

        // kontrole izravno na pogledu pomiču se zajedno s klizačem (AutoScrollPosition je negativan kad je sadržaj pomaknut)
        private void PlaceInView(Control control, int x, int y, int width, int height)
        {
            control.SetBounds(x + AutoScrollPosition.X, y + AutoScrollPosition.Y, width, height);
        }

        // vraća visinu sadržaja kartice (s gornjim i donjim razmakom)
        private int LayoutSessionCard(int innerWidth, int inner, int small, int titleLine, int bodyLine)
        {
            int cy = inner;

            _lblSessionTitle.SetBounds(inner, cy, innerWidth, titleLine);
            cy = cy + titleLine + small;

            _lblLoginMode.SetBounds(inner, cy, innerWidth, bodyLine);
            cy = cy + bodyLine + small;

            _lblSessionExpiry.SetBounds(inner, cy, innerWidth, bodyLine);
            cy = cy + bodyLine + small;

            _lblSessionRegion.SetBounds(inner, cy, innerWidth, bodyLine);
            cy = cy + bodyLine;

            return cy + inner;
        }

        // broj računa i gumb Kopiraj u istom retku, ispod njih puni ARN identiteta
        private int LayoutAccountCard(int innerWidth, int inner, int small, int titleLine, int bodyLine, int smallLine)
        {
            int cy = inner;

            _lblAccountTitle.SetBounds(inner, cy, innerWidth, titleLine);
            cy = cy + titleLine + small;

            int buttonWidth = TextWidth(_btnCopyAccount.Text, _btnCopyAccount.Font) + small * 4;
            int buttonHeight = Math.Max(bodyLine + small, smallLine + small * 2);

            int numberWidth = TextWidth(_lblAccountNumber.Text, _lblAccountNumber.Font) + small;
            int maxNumberWidth = innerWidth - buttonWidth - small;

            if (numberWidth > maxNumberWidth)
            {
                numberWidth = maxNumberWidth;
            }

            _lblAccountNumber.SetBounds(inner, cy + (buttonHeight - bodyLine) / 2, numberWidth, bodyLine);
            _btnCopyAccount.SetBounds(inner + numberWidth + small, cy, buttonWidth, buttonHeight);
            cy = cy + buttonHeight + small;

            _lblAccountIdentity.SetBounds(inner, cy, innerWidth, smallLine);
            cy = cy + smallLine;

            return cy + inner;
        }

        private int LayoutServerCard(int innerWidth, int inner, int small, int titleLine, int bodyLine)
        {
            int cy = inner;

            _lblServerTitle.SetBounds(inner, cy, innerWidth, titleLine);
            cy = cy + titleLine + small;

            _lblServerStatus.SetBounds(inner, cy, innerWidth, bodyLine);
            cy = cy + bodyLine;

            return cy + inner;
        }

        // popis je visok točno koliko i njegovi retci (bez praznog prostora ispod); bez podataka se prikazuje jedan redak poruke
        private int LayoutList(DataGridView grid, Label emptyLabel, bool hasRows, int left, int top, int width, int smallLine)
        {
            grid.Visible = hasRows;
            emptyLabel.Visible = hasRows == false;

            if (hasRows)
            {
                int rowsHeight = grid.Rows.GetRowsHeight(DataGridViewElementStates.Visible);
                grid.SetBounds(left, top, width, rowsHeight + 1);
                return top + grid.Height;
            }

            emptyLabel.SetBounds(left, top, width, smallLine);
            return top + smallLine;
        }

        // ---------------------------------------------------------------
        // Podaci - isključivo lokalni izvori (profil, baza, JSON dnevnik, već postojeće stanje ljuske), bez novih AWS/TCP poziva

        private void RefreshData()
        {
            RefreshSession();
            RefreshAccount();
            RefreshServerStatus();
            RefreshTransfers();
            RefreshActivity();

            // duljina tekstova i broj redaka mijenjaju visine kartica
            LayoutView();
        }

        private void RefreshSession()
        {
            _lblLoginMode.Text = LanguageHelper.Format("home_session_mode", _profile.GetLoginModeDisplay());

            if (_profile.IsTemporarySession())
            {
                TimeSpan remaining = _profile.GetRemainingSessionTime();
                _lblSessionExpiry.Text = LanguageHelper.Format("session_expires", FormatUtils.FormatDuration(remaining.TotalSeconds));

                if (remaining.TotalMinutes < 1)
                {
                    _lblSessionExpiry.ForeColor = UiTheme.Colors.Danger;
                }
                else if (remaining.TotalMinutes < 5)
                {
                    _lblSessionExpiry.ForeColor = UiTheme.Colors.Warning;
                }
                else
                {
                    _lblSessionExpiry.ForeColor = UiTheme.Colors.Muted;
                }
            }
            else
            {
                _lblSessionExpiry.Text = LanguageHelper.Get("session_access_key");
                _lblSessionExpiry.ForeColor = UiTheme.Colors.Muted;
            }

            AwsRegionInfo region = AwsRegions.Find(_profile.Region);
            string regionDisplay;

            if (region == null)
            {
                regionDisplay = _profile.Region;
            }
            else
            {
                regionDisplay = region.ToString();
            }

            _lblSessionRegion.Text = LanguageHelper.Format("home_session_region", regionDisplay);
        }

        private void RefreshAccount()
        {
            string account = _profile.AwsAccountId;

            if (string.IsNullOrEmpty(account))
            {
                account = LanguageHelper.Get("cost_account_unknown");
            }

            _lblAccountNumber.Text = LanguageHelper.Format("home_account_number", account);

            // natpis "Kopirano" vrijedi samo do sljedećeg osvježavanja; bez broja računa nema se što kopirati
            _btnCopyAccount.Text = LanguageHelper.Get("home_account_copy");
            _btnCopyAccount.Enabled = string.IsNullOrEmpty(_profile.AwsAccountId) == false;

            if (string.IsNullOrEmpty(_profile.AwsArn))
            {
                _lblAccountIdentity.Text = LanguageHelper.Get("home_account_identity_unknown");
            }
            else
            {
                _lblAccountIdentity.Text = _profile.AwsArn;
            }
        }

        private void BtnCopyAccount_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_profile.AwsAccountId))
            {
                return;
            }

            try
            {
                Clipboard.SetText(_profile.AwsAccountId);
                _btnCopyAccount.Text = LanguageHelper.Get("home_account_copied");

                // drugi natpis ima drugu širinu - gumb se ponovno slaže
                LayoutView();
            }
            catch (ExternalException ex)
            {
                // clipboard može biti zauzet drugim programom - nije kritično, samo se natpis gumba ne mijenja
                Debug.WriteLine("Greška kod kopiranja broja računa: " + ex.Message);
            }
        }

        private void RefreshServerStatus()
        {
            _lblServerStatus.Text = _shell.ServerStatusText;
            _lblServerStatus.ForeColor = _shell.ServerStatusColor;
        }

        private void RefreshTransfers()
        {
            List<TransferModels> transfers;

            try
            {
                TransferReportFilter filter = new TransferReportFilter();
                filter.Username = AppSettings.Instance.LastUser;
                transfers = DatabaseManager.Instance.GetTransfers(filter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod dohvata zadnjih prijenosa: " + ex.Message);
                transfers = new List<TransferModels>();
            }

            _transfersList.Rows.Clear();
            _hasTransfers = transfers.Count > 0;

            foreach (TransferModels transfer in transfers.Take(MaxTransfers))
            {
                string name = transfer.ObjectKey;

                if (string.IsNullOrEmpty(name))
                {
                    name = transfer.Bucket;
                }

                string time = transfer.GetCreatedAtLocal().ToString("dd.MM. HH:mm");

                int rowIndex = _transfersList.Rows.Add(transfer.GetDirectionDisplay(), name, transfer.GetStatusDisplay(), time);
                DataGridViewRow row = _transfersList.Rows[rowIndex];

                // bojom se ističe samo status, ne cijeli redak
                Color statusColor = GetStatusColor(transfer.Status);
                row.Cells[2].Style.ForeColor = statusColor;
                row.Cells[2].Style.SelectionForeColor = statusColor;

                // dugački naziv se skraćuje s "..." - puni naziv je u tooltipu
                row.Cells[1].ToolTipText = name;
            }

            _transfersList.ClearSelection();
        }

        // ista logika kao ReportForm.GetStatusColor - boja po sirovoj vrijednosti iz baze, ne po prevedenom prikazu
        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "completed":
                    return UiTheme.Colors.Success;
                case "failed":
                    return UiTheme.Colors.Danger;
                case "cancelled":
                    return UiTheme.Colors.Warning;
                default:
                    return UiTheme.Colors.Foreground;
            }
        }

        private void RefreshActivity()
        {
            List<JsonLogManager.ActivityLogEntry> entries;

            try
            {
                int userId = DatabaseManager.Instance.GetUserId(AppSettings.Instance.LastUser);
                entries = new JsonLogManager().GetForUser(userId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod dohvata dnevnika aktivnosti: " + ex.Message);
                entries = new List<JsonLogManager.ActivityLogEntry>();
            }

            List<JsonLogManager.ActivityLogEntry> recent = entries.OrderByDescending(x => x.CreatedAt).Take(MaxActivity).ToList();

            _activityList.Rows.Clear();
            _hasActivity = recent.Count > 0;

            foreach (JsonLogManager.ActivityLogEntry entry in recent)
            {
                // ActivityLogEntry.CreatedAt je već lokalno vrijeme (DateTime.Now u konstruktoru) - ToLocalTime() bi ga pogrešno pomaknuo
                string time = entry.CreatedAt.ToString("dd.MM. HH:mm");

                int rowIndex = _activityList.Rows.Add(time, entry.Action, entry.Details);
                DataGridViewRow row = _activityList.Rows[rowIndex];

                if (entry.IsError)
                {
                    row.DefaultCellStyle.ForeColor = UiTheme.Colors.Danger;
                    row.DefaultCellStyle.SelectionForeColor = UiTheme.Colors.Danger;
                }

                row.Cells[2].ToolTipText = entry.Details;
            }

            _activityList.ClearSelection();
        }
    }
}