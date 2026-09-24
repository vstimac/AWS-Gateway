using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;
using AWSGateway.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace AWSGateway.Forms
{
    public partial class TeamStatusForm : Form, IAppModule
    {
        // stupci tablica dnevnika (vlastiti dnevnik i stavke poslanog dnevnika)
        private const string ColLogTime = "Vrijeme";
        private const string ColLogAction = "Radnja";
        private const string ColLogSource = "Izvor";
        private const string ColLogDetails = "Detalji";
        private const string ColLogIsError = "IsError";

        // stupci tablice aktivnosti tima
        private const string ColTeamId = "Id";
        private const string ColTeamTime = "Vrijeme";
        private const string ColTeamUser = "Korisnik";
        private const string ColTeamType = "Vrsta";
        private const string ColTeamContent = "Sadržaj";
        private const string ColTeamAddress = "Adresa";

        // slanje dnevnika - samo vlastiti zapisi iz zadnja 24 sata, najviše 500 (isto ograničenje ima server)
        private static readonly TimeSpan SessionLogWindow = TimeSpan.FromHours(24);
        private const int MaxSessionEntries = 500;

        private TcpClientService _tcpService;
        private JsonLogManager _jsonManager;

        private int _currentUserId = 0;
        private List<JsonLogManager.ActivityLogEntry> _myLogs = new List<JsonLogManager.ActivityLogEntry>();
        private List<JsonLogManager.ActivityLogEntry> _filteredLogs = new List<JsonLogManager.ActivityLogEntry>();
        private List<TeamActivityEntry> _teamEntries = new List<TeamActivityEntry>();

        private bool _isInitializing = true;
        private bool _isFillingTeamGrid = false;
        private bool _isRefreshingSources = false;

        // dnevnik čiji se zapisi trenutno dohvaćaju - odgovor za prethodno odabrani redak se odbacuje
        private string _requestedSessionId = string.Empty;

        private System.Windows.Forms.Timer _searchTimer;
        private bool _resourcesReleased;

        // pamti zadnje postavljeno "značenje" boje lblServerStatus - ForeColor iz koda nije "zadana" boja pa se
        // ne osvježava sam kroz ApplyToControls; bez ovoga bi boja ostala zaleđena na staroj temi nakon promjene
        private UiTheme.StatusKind _serverStatusKind = UiTheme.StatusKind.Neutral;

        public bool IsBusy
        {
            get { return false; }
        }

        public void ApplyShellLanguage()
        {
            ApplyLanguage();
        }

        public void ApplyShellTheme()
        {
            UiTheme.Apply(this, btnSend);

            lblServerStatus.Font = UiTheme.Fonts.Section;
            lblBasicNote.Font = UiTheme.Fonts.Note;
            lblTeamActivity.Font = UiTheme.Fonts.Section;
            lblServerStatus.ForeColor = UiTheme.StatusKindColor(_serverStatusKind);
        }

        public TeamStatusForm()
        {
            InitializeComponent();

            _tcpService = new TcpClientService();
            _jsonManager = new JsonLogManager();

            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 300;
            _searchTimer.Tick += SearchTimer_Tick;

            // ApplyLanguage puni i filtre dnevnika (PopulateLogFilters), pa se ne zove zasebno
            ApplyLanguage();

            UiTheme.Apply(this, btnSend);
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("team_status_title");
            tabPageTeam.Text = LanguageHelper.Get("team_tab");
            tabPageLog.Text = LanguageHelper.Get("activity_tab");
            lblServerStatus.Text = LanguageHelper.Get("team_server_checking");
            btnCheckServer.Text = LanguageHelper.Get("team_btn_check_short");
            lblBasicNote.Text = LanguageHelper.Get("team_basic_note");
            txtMessage.PlaceholderText = LanguageHelper.Get("team_message_placeholder");
            btnSend.Text = LanguageHelper.Get("team_btn_send_short");
            btnSendMyLog.Text = LanguageHelper.Get("team_btn_send_log_short");
            lblTeamActivity.Text = LanguageHelper.Get("team_lbl_activity");
            btnRefreshTeam.Text = LanguageHelper.Get("refresh_short");
            lblSessionDetails.Text = LanguageHelper.Get("team_session_select_hint");
            lblLogPeriod.Text = LanguageHelper.Get("report_lbl_period");
            chkOnlyErrors.Text = LanguageHelper.Get("team_chk_only_errors");
            lblSource.Text = LanguageHelper.Get("column_source");
            lblLogSearch.Text = LanguageHelper.Get("report_lbl_search");
            txtLogSearch.PlaceholderText = LanguageHelper.Get("team_log_search_placeholder");
            btnRefreshLog.Text = LanguageHelper.Get("refresh_short");
            btnExportLog.Text = LanguageHelper.Get("report_btn_export_csv");

            // stupci tablica postoje tek nakon prvog punjenja - kod prve izgradnje forme se preskaču
            if (dgvTeam.Columns.Count > 0)
            {
                dgvTeam.Columns[ColTeamTime].HeaderText = LanguageHelper.Get("time_column");
                dgvTeam.Columns[ColTeamUser].HeaderText = LanguageHelper.Get("column_user");
                dgvTeam.Columns[ColTeamType].HeaderText = LanguageHelper.Get("team_col_type");
                dgvTeam.Columns[ColTeamContent].HeaderText = LanguageHelper.Get("team_col_content");
                dgvTeam.Columns[ColTeamAddress].HeaderText = LanguageHelper.Get("team_col_address");
            }

            if (dgvLog.Columns.Count > 0)
            {
                ConfigureLogGrid(dgvLog);
            }

            if (dgvSessionEntries.Columns.Count > 0)
            {
                ConfigureLogGrid(dgvSessionEntries);
            }

            PopulateLogFilters();
        }

        private async void TeamStatusForm_Load(object sender, EventArgs e)
        {
            _isInitializing = false;

            LoadMyLogs();

            bool serverRunning = await CheckServerAsync(true);

            if (serverRunning)
            {
                await LoadTeamActivityAsync();
            }
        }

        private void TeamStatusForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseResources();
        }

        // ugrađen u ljusku - modul se ne zatvara pojedinačno pa ovo poziva i Dispose(bool) iz Designera kod gašenja aplikacije
        private void ReleaseResources()
        {
            if (_resourcesReleased)
            {
                return;
            }

            _resourcesReleased = true;

            _searchTimer.Stop();
            _searchTimer.Dispose();
        }

        // ---------------------------------------------------------------
        // Tim - status servera

        // startIfNeeded - pokreće lokalni server ako ne radi (samo lokalni način, nikad --lan)
        private async Task<bool> CheckServerAsync(bool startIfNeeded)
        {
            btnCheckServer.Enabled = false;
            _serverStatusKind = UiTheme.StatusKind.Neutral;
            lblServerStatus.ForeColor = UiTheme.StatusKindColor(_serverStatusKind);
            lblServerStatus.Text = LanguageHelper.Get("team_server_checking");

            try
            {
                bool available;

                if (startIfNeeded)
                {
                    available = await _tcpService.EnsureServerRunningAsync();
                }
                else
                {
                    available = await _tcpService.IsServerAvailableAsync();
                }

                if (available == false)
                {
                    SetServerOffline(LanguageHelper.Get("team_server_not_started"));
                    return false;
                }

                long responseMs = await _tcpService.PingAsync();

                _serverStatusKind = UiTheme.StatusKind.Success;
                lblServerStatus.ForeColor = UiTheme.StatusKindColor(_serverStatusKind);
                lblServerStatus.Text = LanguageHelper.Get("team_server_online");
                lblServerDetails.Text = LanguageHelper.Format("team_server_response", TcpClientService.DefaultHost, TcpClientService.DefaultPort, responseMs);

                SetTeamActionsEnabled(true);
                return true;
            }
            catch (Exception ex)
            {
                SetServerOffline(ex.Message);
                return false;
            }
            finally
            {
                btnCheckServer.Enabled = true;
            }
        }

        private void SetServerOffline(string reason)
        {
            _serverStatusKind = UiTheme.StatusKind.Error;
            lblServerStatus.ForeColor = UiTheme.StatusKindColor(_serverStatusKind);
            lblServerStatus.Text = LanguageHelper.Get("team_server_offline");
            lblServerDetails.Text = LanguageHelper.Format("team_server_offline_details", TcpClientService.DefaultHost, TcpClientService.DefaultPort, reason);

            SetTeamActionsEnabled(false);
        }

        private void SetTeamActionsEnabled(bool enabled)
        {
            btnSend.Enabled = enabled;
            btnSendMyLog.Enabled = enabled;
            btnRefreshTeam.Enabled = enabled;
        }

        private async void btnCheckServer_Click(object sender, EventArgs e)
        {
            bool serverRunning = await CheckServerAsync(true);

            if (serverRunning)
            {
                await LoadTeamActivityAsync();
            }
        }

        // ---------------------------------------------------------------
        // Tim - slanje

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();

            if (string.IsNullOrEmpty(message))
            {
                lblTeamStatus.Text = LanguageHelper.Get("team_enter_message");
                return;
            }

            btnSend.Enabled = false;

            try
            {
                await _tcpService.SendActivityAsync(AppSettings.Instance.LastUser, message);

                txtMessage.Clear();
                lblTeamStatus.Text = LanguageHelper.Get("team_message_sent");
                ActivityLogger.Log("Poruka timu", "TeamStatusForm", message);

                await LoadTeamActivityAsync();
            }
            catch (Exception ex)
            {
                lblTeamStatus.Text = LanguageHelper.Format("team_send_message_error", ex.Message);
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }

        // šalje samo vlastite zapise iz zadnja 24 sata - ne dnevnik drugih korisnika na ovom računalu
        private async void btnSendMyLog_Click(object sender, EventArgs e)
        {
            List<JsonLogManager.ActivityLogEntry> recentLogs = GetMyRecentLogs();

            if (recentLogs.Count == 0)
            {
                lblTeamStatus.Text = LanguageHelper.Get("team_no_recent_logs");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                LanguageHelper.Format("team_send_log_confirm", recentLogs.Count),
                LanguageHelper.Get("team_send_log_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            btnSendMyLog.Enabled = false;

            try
            {
                await _tcpService.SendSessionLogsAsync(AppSettings.Instance.LastUser, recentLogs);

                lblTeamStatus.Text = LanguageHelper.Format("team_log_sent", recentLogs.Count);
                ActivityLogger.Log("Dnevnik poslan timu", "TeamStatusForm", recentLogs.Count + " zapisa");

                await LoadTeamActivityAsync();
            }
            catch (Exception ex)
            {
                lblTeamStatus.Text = LanguageHelper.Format("team_send_log_error", ex.Message);
            }
            finally
            {
                btnSendMyLog.Enabled = true;
            }
        }

        // najnovijih MaxSessionEntries zapisa iz zadnja 24 sata, kronološkim redom
        private List<JsonLogManager.ActivityLogEntry> GetMyRecentLogs()
        {
            DateTime from = DateTime.Now.Subtract(SessionLogWindow);
            List<JsonLogManager.ActivityLogEntry> recent = new List<JsonLogManager.ActivityLogEntry>();

            foreach (JsonLogManager.ActivityLogEntry log in _jsonManager.GetForUser(_currentUserId))
            {
                if (log.CreatedAt >= from)
                {
                    recent.Add(log);
                }
            }

            if (recent.Count > MaxSessionEntries)
            {
                recent = recent.GetRange(recent.Count - MaxSessionEntries, MaxSessionEntries);
            }

            return recent;
        }

        // ---------------------------------------------------------------
        // Tim - aktivnost

        private async void btnRefreshTeam_Click(object sender, EventArgs e)
        {
            await LoadTeamActivityAsync();
        }

        private async Task LoadTeamActivityAsync()
        {
            try
            {
                _teamEntries = await _tcpService.GetTeamActivityAsync();

                FillTeamGrid();
                lblTeamStatus.Text = LanguageHelper.Format("team_activity_count", _teamEntries.Count);
            }
            catch (Exception ex)
            {
                lblTeamStatus.Text = LanguageHelper.Format("team_load_activity_error", ex.Message);
            }
        }

        private void FillTeamGrid()
        {
            DataTable table = new DataTable();
            table.Columns.Add(ColTeamId, typeof(string));
            table.Columns.Add(ColTeamTime, typeof(DateTime));
            table.Columns.Add(ColTeamUser, typeof(string));
            table.Columns.Add(ColTeamType, typeof(string));
            table.Columns.Add(ColTeamContent, typeof(string));
            table.Columns.Add(ColTeamAddress, typeof(string));

            foreach (TeamActivityEntry entry in _teamEntries)
            {
                DataRow row = table.NewRow();
                row[ColTeamId] = entry.Id;
                row[ColTeamTime] = entry.GetLocalTime();
                row[ColTeamUser] = entry.Username;
                row[ColTeamType] = entry.GetTypeDisplay();
                row[ColTeamContent] = entry.Message;
                row[ColTeamAddress] = entry.ClientAddress;
                table.Rows.Add(row);
            }

            _isFillingTeamGrid = true;

            dgvTeam.DataSource = table;

            dgvTeam.Columns[ColTeamId].Visible = false;
            dgvTeam.Columns[ColTeamTime].HeaderText = LanguageHelper.Get("time_column");
            dgvTeam.Columns[ColTeamUser].HeaderText = LanguageHelper.Get("column_user");
            dgvTeam.Columns[ColTeamType].HeaderText = LanguageHelper.Get("team_col_type");
            dgvTeam.Columns[ColTeamContent].HeaderText = LanguageHelper.Get("team_col_content");
            dgvTeam.Columns[ColTeamAddress].HeaderText = LanguageHelper.Get("team_col_address");
            dgvTeam.Columns[ColTeamTime].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
            dgvTeam.Columns[ColTeamTime].Width = 130;
            dgvTeam.Columns[ColTeamUser].Width = 110;
            dgvTeam.Columns[ColTeamType].Width = 80;
            dgvTeam.Columns[ColTeamAddress].Width = 130;
            dgvTeam.Columns[ColTeamContent].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            foreach (DataGridViewColumn column in dgvTeam.Columns)
            {
                if (column.Visible)
                {
                    column.SortMode = DataGridViewColumnSortMode.Automatic;
                }
            }

            // bez automatskog odabira prvog retka - dnevnik se dohvaća tek kad ga korisnik odabere
            dgvTeam.ClearSelection();

            _isFillingTeamGrid = false;

            ClearSessionDetails(LanguageHelper.Get("team_session_select_hint"));
        }

        private TeamActivityEntry GetSelectedTeamEntry()
        {
            if (dgvTeam.SelectedRows.Count == 0)
            {
                return null;
            }

            object idValue = dgvTeam.SelectedRows[0].Cells[ColTeamId].Value;

            if (idValue == null || idValue == DBNull.Value)
            {
                return null;
            }

            string id = idValue.ToString();

            foreach (TeamActivityEntry entry in _teamEntries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return null;
        }

        private async void dgvTeam_SelectionChanged(object sender, EventArgs e)
        {
            if (_isFillingTeamGrid)
            {
                return;
            }

            TeamActivityEntry selected = GetSelectedTeamEntry();

            if (selected == null)
            {
                return;
            }

            if (selected.IsSessionLog() == false)
            {
                ClearSessionDetails(LanguageHelper.Get("team_message_no_logs"));
                return;
            }

            _requestedSessionId = selected.Id;
            lblSessionDetails.Text = LanguageHelper.Format("team_fetching_user_log", selected.Username);

            try
            {
                TeamActivityEntry sessionLog = await _tcpService.GetSessionLogAsync(selected.Id);

                // korisnik je u međuvremenu odabrao drugi redak
                if (_requestedSessionId != selected.Id)
                {
                    return;
                }

                List<JsonLogManager.ActivityLogEntry> entries = new List<JsonLogManager.ActivityLogEntry>();

                if (sessionLog != null && sessionLog.Entries != null)
                {
                    entries = sessionLog.Entries;
                }

                dgvSessionEntries.DataSource = BuildLogTable(entries);
                ConfigureLogGrid(dgvSessionEntries);

                lblSessionDetails.Text = LanguageHelper.Format("team_session_summary",
                    selected.Username, selected.GetLocalTime().ToString("dd.MM.yyyy HH:mm"), entries.Count, selected.ErrorCount);
            }
            catch (Exception ex)
            {
                if (_requestedSessionId == selected.Id)
                {
                    ClearSessionDetails(LanguageHelper.Format("team_fetch_log_error", ex.Message));
                }
            }
        }

        private void ClearSessionDetails(string message)
        {
            _requestedSessionId = string.Empty;
            dgvSessionEntries.DataSource = null;
            lblSessionDetails.Text = message;
        }

        // ---------------------------------------------------------------
        // Dnevnik aktivnosti (samo pregled)

        private void PopulateLogFilters()
        {
            int previousPeriod = cmbLogPeriod.SelectedIndex;

            cmbLogPeriod.Items.Clear();
            cmbLogPeriod.Items.Add(LanguageHelper.Get("report_period_today"));
            cmbLogPeriod.Items.Add(LanguageHelper.Get("ec2_period_7d"));
            cmbLogPeriod.Items.Add(LanguageHelper.Get("cost_period_last_30_days"));
            cmbLogPeriod.Items.Add(LanguageHelper.Get("report_period_all"));

            if (previousPeriod >= 0 && previousPeriod < cmbLogPeriod.Items.Count)
            {
                cmbLogPeriod.SelectedIndex = previousPeriod;
            }
            else
            {
                cmbLogPeriod.SelectedIndex = 1;
            }

            // RefreshSourceOptions ponovno gradi cmbSource (uklj. "Svi izvori") - prije prvog LoadMyLogs samo prazan popis
            RefreshSourceOptions();
        }

        private void LoadMyLogs()
        {
            try
            {
                _currentUserId = DatabaseManager.Instance.GetUserId(AppSettings.Instance.LastUser);
                _myLogs = _jsonManager.GetForUser(_currentUserId);
            }
            catch (Exception ex)
            {
                _myLogs = new List<JsonLogManager.ActivityLogEntry>();
                lblLogCount.Text = LanguageHelper.Format("team_read_log_error", ex.Message);
            }

            RefreshSourceOptions();
            ApplyLogFilter();
        }

        // izvori (forme) koji se pojavljuju u dnevniku; odabir se zadržava ako izvor i dalje postoji
        private void RefreshSourceOptions()
        {
            string previousSource = string.Empty;

            if (cmbSource.SelectedIndex > 0)
            {
                previousSource = cmbSource.SelectedItem.ToString();
            }

            List<string> sources = new List<string>();

            foreach (JsonLogManager.ActivityLogEntry log in _myLogs)
            {
                if (string.IsNullOrEmpty(log.Resource) == false && sources.Contains(log.Resource) == false)
                {
                    sources.Add(log.Resource);
                }
            }

            sources.Sort(StringComparer.OrdinalIgnoreCase);

            _isRefreshingSources = true;

            cmbSource.Items.Clear();
            cmbSource.Items.Add(LanguageHelper.Get("team_all_sources"));

            foreach (string source in sources)
            {
                cmbSource.Items.Add(source);
            }

            int previousIndex = cmbSource.Items.IndexOf(previousSource);

            if (previousIndex > 0)
            {
                cmbSource.SelectedIndex = previousIndex;
            }
            else
            {
                cmbSource.SelectedIndex = 0;
            }

            _isRefreshingSources = false;
        }

        // CreatedAt u dnevniku je lokalno vrijeme
        private DateTime? GetLogPeriodStart()
        {
            switch (cmbLogPeriod.SelectedIndex)
            {
                case 0:
                    return DateTime.Today;
                case 1:
                    return DateTime.Today.AddDays(-6);
                case 2:
                    return DateTime.Today.AddDays(-29);
                default:
                    return null;
            }
        }

        private void ApplyLogFilter()
        {
            if (_isInitializing)
            {
                return;
            }

            DateTime? from = GetLogPeriodStart();
            bool onlyErrors = chkOnlyErrors.Checked;
            string source = string.Empty;
            string search = txtLogSearch.Text.Trim();

            if (cmbSource.SelectedIndex > 0)
            {
                source = cmbSource.SelectedItem.ToString();
            }

            List<JsonLogManager.ActivityLogEntry> filtered = new List<JsonLogManager.ActivityLogEntry>();
            int errorCount = 0;

            // najnoviji prvi - dnevnik je u datoteci kronološki
            for (int i = _myLogs.Count - 1; i >= 0; i--)
            {
                JsonLogManager.ActivityLogEntry log = _myLogs[i];

                if (from.HasValue && log.CreatedAt < from.Value)
                {
                    continue;
                }

                if (onlyErrors && log.IsError == false)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(source) == false && log.Resource != source)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(search) == false && ContainsText(log, search) == false)
                {
                    continue;
                }

                filtered.Add(log);

                if (log.IsError)
                {
                    errorCount++;
                }
            }

            _filteredLogs = filtered;

            dgvLog.DataSource = BuildLogTable(filtered);
            ConfigureLogGrid(dgvLog);

            lblLogCount.Text = LanguageHelper.Format("team_log_filtered_count", filtered.Count, errorCount);
            btnExportLog.Enabled = filtered.Count > 0;
        }

        private bool ContainsText(JsonLogManager.ActivityLogEntry log, string search)
        {
            if (log.Action != null && log.Action.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return log.Details != null && log.Details.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void LogFilter_Changed(object sender, EventArgs e)
        {
            if (_isRefreshingSources)
            {
                return;
            }

            ApplyLogFilter();
        }

        private void txtLogSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            ApplyLogFilter();
        }

        private void btnRefreshLog_Click(object sender, EventArgs e)
        {
            LoadMyLogs();
        }

        private void btnExportLog_Click(object sender, EventArgs e)
        {
            if (_filteredLogs.Count == 0)
            {
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV|*.csv";
                dialog.FileName = "AWSGateway_dnevnik_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    // UTF-8 s BOM oznakom - Excel tada ispravno prikazuje hrvatske znakove
                    using (StreamWriter writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                    {
                        writer.WriteLine("Vrijeme,Radnja,Izvor,Detalji,Greska");

                        foreach (JsonLogManager.ActivityLogEntry log in _filteredLogs)
                        {
                            string isError = "ne";

                            if (log.IsError)
                            {
                                isError = "da";
                            }

                            writer.WriteLine(
                                FormatUtils.EscapeCsv(log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)) + ","
                                + FormatUtils.EscapeCsv(log.Action) + ","
                                + FormatUtils.EscapeCsv(log.Resource) + ","
                                + FormatUtils.EscapeCsv(log.Details) + ","
                                + isError);
                        }
                    }

                    lblLogCount.Text = LanguageHelper.Format("team_export_log_done", _filteredLogs.Count);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageHelper.Format("team_export_log_error", ex.Message));
                }
            }
        }

        // ---------------------------------------------------------------
        // Zajedničko za tablice dnevnika

        private DataTable BuildLogTable(List<JsonLogManager.ActivityLogEntry> logs)
        {
            DataTable table = new DataTable();
            table.Columns.Add(ColLogTime, typeof(DateTime));
            table.Columns.Add(ColLogAction, typeof(string));
            table.Columns.Add(ColLogSource, typeof(string));
            table.Columns.Add(ColLogDetails, typeof(string));
            table.Columns.Add(ColLogIsError, typeof(bool));

            foreach (JsonLogManager.ActivityLogEntry log in logs)
            {
                DataRow row = table.NewRow();
                row[ColLogTime] = log.CreatedAt;
                row[ColLogAction] = log.Action;
                row[ColLogSource] = log.Resource;
                row[ColLogDetails] = log.Details;
                row[ColLogIsError] = log.IsError;
                table.Rows.Add(row);
            }

            return table;
        }

        private void ConfigureLogGrid(DataGridView grid)
        {
            grid.Columns[ColLogIsError].Visible = false;
            grid.Columns[ColLogTime].HeaderText = LanguageHelper.Get("time_column");
            grid.Columns[ColLogAction].HeaderText = LanguageHelper.Get("team_col_action");
            grid.Columns[ColLogSource].HeaderText = LanguageHelper.Get("column_source");
            grid.Columns[ColLogDetails].HeaderText = LanguageHelper.Get("team_col_details");
            grid.Columns[ColLogTime].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
            grid.Columns[ColLogTime].Width = 130;
            grid.Columns[ColLogAction].Width = 170;
            grid.Columns[ColLogSource].Width = 140;
            grid.Columns[ColLogDetails].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Visible)
                {
                    column.SortMode = DataGridViewColumnSortMode.Automatic;
                }
            }
        }

        // greške su obojane u cijelom retku
        private void LogGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            if (e.RowIndex < 0 || grid.Columns.Contains(ColLogIsError) == false)
            {
                return;
            }

            object isErrorValue = grid.Rows[e.RowIndex].Cells[ColLogIsError].Value;

            if (isErrorValue is bool && (bool)isErrorValue)
            {
                e.CellStyle.ForeColor = UiTheme.Colors.Danger;
            }
        }

        // puni tekst detalja kao tooltip - dugi detalji se u stupcu odrežu
        private void LogGrid_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;

            if (e.RowIndex < 0 || e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != ColLogDetails)
            {
                return;
            }

            object value = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (value != null && value != DBNull.Value)
            {
                e.ToolTipText = value.ToString();
            }
        }
    }
}