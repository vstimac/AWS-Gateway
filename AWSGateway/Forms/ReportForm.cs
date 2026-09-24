using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;
using AWSGateway.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace AWSGateway.Forms
{
    public partial class ReportForm : Form, IAppModule
    {
        // nazivi stupaca tablice (DataTable i DataGridView)
        private const string ColId = "Id";
        private const string ColTime = "Vrijeme";
        private const string ColDirection = "Smjer";
        private const string ColBucket = "Bucket";
        private const string ColKey = "Ključ";
        private const string ColSize = "Veličina";
        private const string ColDuration = "Trajanje";
        private const string ColSpeed = "Brzina";
        private const string ColStatus = "Status";
        private const string ColError = "Greška";
        private const string ColUser = "Korisnik";
        private const string ColIdentity = "AWS identitet";

        private AWSProfile _profile;
        private TransferReportCalculator _calculator = new TransferReportCalculator();

        // retci i filter trenutnog prikaza - isti podaci idu u sažetak, PDF i CSV
        private List<TransferModels> _currentTransfers = new List<TransferModels>();
        private Dictionary<int, TransferModels> _transfersById = new Dictionary<int, TransferModels>();
        private TransferReportFilter _currentFilter = new TransferReportFilter();
        private TransferReportSummary _currentSummary = new TransferReportSummary();

        // sprječava višestruko učitavanje dok se filteri pune pri otvaranju forme
        private bool _isInitializing = true;

        // punjenje popisa bucketa mijenja odabir, a to ne smije pokrenuti zasebno učitavanje
        private bool _isRefreshingBuckets = false;

        // pretraga se pokreće kratko nakon zadnjeg upisanog znaka, ne na svaki znak
        private System.Windows.Forms.Timer _searchTimer;
        private bool _resourcesReleased;

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
            UiTheme.Apply(this, btnExportPdf);

            // fiksna boja u Designeru ne prati temu - ApplyToControls je preskace jer nije "zadana" boja
            lblCommonError.ForeColor = UiTheme.Colors.Danger;

            lblSumTotal.Font = UiTheme.Fonts.Section;
            lblSumSuccess.Font = UiTheme.Fonts.Section;
            lblSumUpload.Font = UiTheme.Fonts.Section;
            lblSumDownload.Font = UiTheme.Fonts.Section;
            lblSumErrors.Font = UiTheme.Fonts.Section;
            lblSumSpeed.Font = UiTheme.Fonts.Section;

            UiTheme.StyleDangerButton(btnDeleteHistory);
            StyleSummaryTiles();
        }

        // kartice sažetka istog stila kao na Početnoj (UiTheme.StyleCard/PaintCardBorder) umjesto sistemskog FixedSingle ruba
        private void StyleSummaryTiles()
        {
            Label[] tiles = { lblSumTotal, lblSumSuccess, lblSumUpload, lblSumDownload, lblSumErrors, lblSumSpeed };

            foreach (Label tile in tiles)
            {
                tile.BackColor = UiTheme.Colors.Surface;
                tile.Paint -= SummaryTile_Paint;
                tile.Paint += SummaryTile_Paint;
            }
        }

        private void SummaryTile_Paint(object sender, PaintEventArgs e)
        {
            UiTheme.PaintCardBorder((Control)sender, e);
        }

        public ReportForm(AWSProfile profile)
        {
            InitializeComponent();

            _profile = profile;

            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 400;
            _searchTimer.Tick += SearchTimer_Tick;

            // ApplyLanguage puni i filtre (PopulateFilterOptions), pa se ne zove zasebno
            ApplyLanguage();

            ApplyShellTheme();
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("report_title");
            lblPeriod.Text = LanguageHelper.Get("report_lbl_period");
            lblDirection.Text = LanguageHelper.Get("column_direction");
            lblStatusFilter.Text = LanguageHelper.Get("column_status");
            lblBucketFilter.Text = LanguageHelper.Get("report_col_bucket");
            lblSearch.Text = LanguageHelper.Get("report_lbl_search");
            txtSearch.PlaceholderText = LanguageHelper.Get("report_search_placeholder");
            chkAllUsers.Text = LanguageHelper.Get("report_chk_all_users");
            btnCopyS3Path.Text = LanguageHelper.Get("report_btn_copy_path");
            btnOpenFolder.Text = LanguageHelper.Get("report_btn_open_folder");
            btnDeleteHistory.Text = LanguageHelper.Get("report_btn_delete_history");
            btnExportCsv.Text = LanguageHelper.Get("report_btn_export_csv");
            btnExportPdf.Text = LanguageHelper.Get("report_btn_export_pdf");

            // stupci tablice postoje tek nakon prvog FillGrid - kod prve izgradnje forme se preskaču
            if (dgvTransfers.Columns.Count > 0)
            {
                ConfigureGridColumns();
            }

            PopulateFilterOptions();
        }

        private void PopulateFilterOptions()
        {
            int previousPeriod = cmbPeriod.SelectedIndex;
            int previousDirection = cmbDirection.SelectedIndex;
            int previousStatus = cmbStatus.SelectedIndex;

            cmbPeriod.Items.Clear();
            cmbPeriod.Items.Add(LanguageHelper.Get("report_period_today"));
            cmbPeriod.Items.Add(LanguageHelper.Get("ec2_period_7d"));
            cmbPeriod.Items.Add(LanguageHelper.Get("cost_period_last_30_days"));
            cmbPeriod.Items.Add(LanguageHelper.Get("report_period_all"));
            cmbPeriod.SelectedIndex = ResolveIndex(previousPeriod, 2, cmbPeriod.Items.Count);

            cmbDirection.Items.Clear();
            cmbDirection.Items.Add(LanguageHelper.Get("report_filter_all"));
            cmbDirection.Items.Add(LanguageHelper.Get("upload"));
            cmbDirection.Items.Add(LanguageHelper.Get("download"));
            cmbDirection.SelectedIndex = ResolveIndex(previousDirection, 0, cmbDirection.Items.Count);

            cmbStatus.Items.Clear();
            cmbStatus.Items.Add(LanguageHelper.Get("report_filter_all"));
            cmbStatus.Items.Add(LanguageHelper.Get("report_status_completed"));
            cmbStatus.Items.Add(LanguageHelper.Get("report_status_failed"));
            cmbStatus.Items.Add(LanguageHelper.Get("report_status_cancelled"));
            cmbStatus.SelectedIndex = ResolveIndex(previousStatus, 0, cmbStatus.Items.Count);

            RefreshBucketOptions();
        }

        // zadrži odabir korisnika kod ponovnog punjenja (promjena jezika), inače zadano
        private static int ResolveIndex(int previousIndex, int fallback, int itemCount)
        {
            if (previousIndex >= 0 && previousIndex < itemCount)
            {
                return previousIndex;
            }

            return fallback;
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            _isInitializing = false;
            LoadReport();
        }

        private void ReportForm_FormClosed(object sender, FormClosedEventArgs e)
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
        // Filteri

        // popis bucketa ovisi o tome gleda li se samo trenutni korisnik ili svi - odabir se zadržava ako bucket i dalje postoji
        private void RefreshBucketOptions()
        {
            string previousBucket = string.Empty;

            if (cmbBucketFilterCtrl.SelectedIndex > 0)
            {
                previousBucket = cmbBucketFilterCtrl.SelectedItem.ToString();
            }

            _isRefreshingBuckets = true;

            cmbBucketFilterCtrl.Items.Clear();
            cmbBucketFilterCtrl.Items.Add(LanguageHelper.Get("report_filter_all"));

            try
            {
                List<string> buckets = DatabaseManager.Instance.GetTransferBuckets(GetUsernameFilter());

                foreach (string bucket in buckets)
                {
                    cmbBucketFilterCtrl.Items.Add(bucket);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod učitavanja bucketa za filter: " + ex.Message);
            }

            int previousIndex = cmbBucketFilterCtrl.Items.IndexOf(previousBucket);

            if (previousIndex > 0)
            {
                cmbBucketFilterCtrl.SelectedIndex = previousIndex;
            }
            else
            {
                cmbBucketFilterCtrl.SelectedIndex = 0;
            }

            _isRefreshingBuckets = false;
        }

        // prazno = svi korisnici
        private string GetUsernameFilter()
        {
            if (chkAllUsers.Checked)
            {
                return string.Empty;
            }

            return AppSettings.Instance.LastUser;
        }

        // lokalna ponoć pretvorena u UTC - CreatedAt u bazi je UTC
        private DateTime? GetPeriodStartUtc()
        {
            switch (cmbPeriod.SelectedIndex)
            {
                case 0:
                    return DateTime.Today.ToUniversalTime();
                case 1:
                    return DateTime.Today.AddDays(-6).ToUniversalTime();
                case 2:
                    return DateTime.Today.AddDays(-29).ToUniversalTime();
                default:
                    return null;
            }
        }

        private TransferReportFilter BuildFilter()
        {
            TransferReportFilter filter = new TransferReportFilter();
            filter.FromUtc = GetPeriodStartUtc();

            switch (cmbDirection.SelectedIndex)
            {
                case 1:
                    filter.Direction = "upload";
                    break;
                case 2:
                    filter.Direction = "download";
                    break;
            }

            switch (cmbStatus.SelectedIndex)
            {
                case 1:
                    filter.Status = TransferReportCalculator.StatusCompleted;
                    break;
                case 2:
                    filter.Status = TransferReportCalculator.StatusFailed;
                    break;
                case 3:
                    filter.Status = TransferReportCalculator.StatusCancelled;
                    break;
            }

            if (cmbBucketFilterCtrl.SelectedIndex > 0)
            {
                filter.Bucket = cmbBucketFilterCtrl.SelectedItem.ToString();
            }

            filter.SearchText = txtSearch.Text.Trim();
            filter.Username = GetUsernameFilter();
            filter.Description = BuildFilterDescription(filter);

            return filter;
        }

        // opis filtra za zaglavlje PDF-a
        private string BuildFilterDescription(TransferReportFilter filter)
        {
            string search = "-";

            if (string.IsNullOrEmpty(filter.SearchText) == false)
            {
                search = "\"" + filter.SearchText + "\"";
            }

            string users;

            if (string.IsNullOrEmpty(filter.Username))
            {
                users = LanguageHelper.Get("report_all_users_label");
            }
            else
            {
                users = filter.Username;
            }

            return LanguageHelper.Format("report_filter_description",
                cmbPeriod.SelectedItem, cmbDirection.SelectedItem, cmbStatus.SelectedItem,
                cmbBucketFilterCtrl.SelectedItem, search, users);
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_isRefreshingBuckets)
            {
                return;
            }

            LoadReport();
        }

        private void chkAllUsers_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            // vidljivost stupca Korisnik postavlja ConfigureGridColumns kod učitavanja
            RefreshBucketOptions();
            LoadReport();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            LoadReport();
        }

        // ---------------------------------------------------------------
        // Učitavanje, tablica i sažetak

        private void LoadReport()
        {
            if (_isInitializing)
            {
                return;
            }

            try
            {
                TransferReportFilter filter = BuildFilter();
                List<TransferModels> transfers = DatabaseManager.Instance.GetTransfers(filter);

                _currentFilter = filter;
                _currentTransfers = transfers;
                _currentSummary = _calculator.Calculate(transfers);

                _transfersById.Clear();

                foreach (TransferModels transfer in transfers)
                {
                    _transfersById[transfer.Id] = transfer;
                }

                FillGrid(transfers);
                ShowSummary(_currentSummary);

                lblRowCount.Text = LanguageHelper.Format("report_row_count", transfers.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageHelper.Format("report_load_error", ex.Message));
            }

            UpdateActionButtons();
        }

        // stupci DataTable-a su tipizirani (DateTime, long, double) - sortiranje klikom na zaglavlje radi po vrijednosti, ne po tekstu
        private DataTable CreateTable()
        {
            // redoslijed stupaca DataTable-a = redoslijed prikaza u tablici (AutoGenerateColumns); AWS identitet
            // je odmah uz Ključ, Greška je zadnja (jedini Fill stupac - vidi ConfigureGridColumns)
            DataTable table = new DataTable();
            table.Columns.Add(ColId, typeof(int));
            table.Columns.Add(ColTime, typeof(DateTime));
            table.Columns.Add(ColDirection, typeof(string));
            table.Columns.Add(ColBucket, typeof(string));
            table.Columns.Add(ColKey, typeof(string));
            table.Columns.Add(ColIdentity, typeof(string));
            table.Columns.Add(ColSize, typeof(long));
            table.Columns.Add(ColDuration, typeof(double));
            table.Columns.Add(ColSpeed, typeof(double));
            table.Columns.Add(ColStatus, typeof(string));
            table.Columns.Add(ColUser, typeof(string));
            table.Columns.Add(ColError, typeof(string));

            return table;
        }

        private void FillGrid(List<TransferModels> transfers)
        {
            // sortiranje koje je korisnik odabrao se zadržava nakon promjene filtra
            string sortedColumnName = string.Empty;
            ListSortDirection sortDirection = ListSortDirection.Descending;

            if (dgvTransfers.SortedColumn != null)
            {
                sortedColumnName = dgvTransfers.SortedColumn.Name;

                if (dgvTransfers.SortOrder == SortOrder.Ascending)
                {
                    sortDirection = ListSortDirection.Ascending;
                }
            }

            DataTable table = CreateTable();

            foreach (TransferModels transfer in transfers)
            {
                DataRow row = table.NewRow();
                row[ColId] = transfer.Id;

                DateTime local = transfer.GetCreatedAtLocal();

                if (local == DateTime.MinValue)
                {
                    row[ColTime] = DBNull.Value;
                }
                else
                {
                    row[ColTime] = local;
                }

                row[ColDirection] = transfer.GetDirectionDisplay();
                row[ColBucket] = transfer.Bucket;
                row[ColKey] = transfer.ObjectKey;
                row[ColSize] = transfer.FileSizeBytes;

                if (transfer.DurationMs.HasValue)
                {
                    row[ColDuration] = transfer.DurationMs.Value;
                }
                else
                {
                    row[ColDuration] = DBNull.Value;
                }

                double? speed = transfer.GetSpeedMBps();

                if (speed.HasValue)
                {
                    row[ColSpeed] = speed.Value;
                }
                else
                {
                    row[ColSpeed] = DBNull.Value;
                }

                row[ColStatus] = transfer.GetStatusDisplay();
                row[ColError] = transfer.ErrorMessage;
                row[ColUser] = transfer.Username;
                row[ColIdentity] = transfer.GetAwsIdentityDisplay();

                table.Rows.Add(row);
            }

            dgvTransfers.DataSource = table;

            ConfigureGridColumns();

            if (string.IsNullOrEmpty(sortedColumnName) == false && dgvTransfers.Columns.Contains(sortedColumnName))
            {
                dgvTransfers.Sort(dgvTransfers.Columns[sortedColumnName], sortDirection);
            }
        }

        private void ConfigureGridColumns()
        {
            dgvTransfers.Columns[ColId].Visible = false;
            dgvTransfers.Columns[ColUser].Visible = chkAllUsers.Checked;

            // Name ostaje tehnički identifikator (koriste ga pretrage stupaca u kodu), HeaderText je odvojen i prevodiv
            dgvTransfers.Columns[ColTime].HeaderText = LanguageHelper.Get("time_column");
            dgvTransfers.Columns[ColDirection].HeaderText = LanguageHelper.Get("column_direction");
            dgvTransfers.Columns[ColBucket].HeaderText = LanguageHelper.Get("report_col_bucket");
            dgvTransfers.Columns[ColKey].HeaderText = LanguageHelper.Get("report_col_key");
            dgvTransfers.Columns[ColSize].HeaderText = LanguageHelper.Get("report_col_size");
            dgvTransfers.Columns[ColDuration].HeaderText = LanguageHelper.Get("report_col_duration");
            dgvTransfers.Columns[ColSpeed].HeaderText = LanguageHelper.Get("report_col_speed");
            dgvTransfers.Columns[ColStatus].HeaderText = LanguageHelper.Get("column_status");
            dgvTransfers.Columns[ColError].HeaderText = LanguageHelper.Get("report_col_error");
            dgvTransfers.Columns[ColUser].HeaderText = LanguageHelper.Get("column_user");
            dgvTransfers.Columns[ColIdentity].HeaderText = LanguageHelper.Get("report_col_identity");

            dgvTransfers.Columns[ColTime].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
            dgvTransfers.Columns[ColTime].Width = 130;
            dgvTransfers.Columns[ColDirection].Width = 75;
            dgvTransfers.Columns[ColBucket].Width = 140;
            dgvTransfers.Columns[ColKey].Width = 220;
            dgvTransfers.Columns[ColIdentity].Width = 150;
            dgvTransfers.Columns[ColSize].Width = 85;
            dgvTransfers.Columns[ColDuration].Width = 75;
            dgvTransfers.Columns[ColSpeed].Width = 85;
            dgvTransfers.Columns[ColStatus].Width = 85;
            dgvTransfers.Columns[ColUser].Width = 100;

            // jedini Fill stupac, sad zadnji u prikazu - upija preostali prostor da tablica ne ostavlja prazninu
            dgvTransfers.Columns[ColError].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvTransfers.Columns[ColError].MinimumWidth = 120;

            dgvTransfers.Columns[ColSize].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvTransfers.Columns[ColDuration].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvTransfers.Columns[ColSpeed].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            foreach (DataGridViewColumn column in dgvTransfers.Columns)
            {
                if (column.Visible)
                {
                    column.SortMode = DataGridViewColumnSortMode.Automatic;
                }
            }
        }

        // prikaz tipiziranih vrijednosti - u tablici ostaje broj (za sortiranje), korisnik vidi formatirani tekst
        private void dgvTransfers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dgvTransfers.Columns[e.ColumnIndex].Name;

            if (e.Value == null || e.Value == DBNull.Value)
            {
                if (columnName == ColTime || columnName == ColDuration || columnName == ColSpeed)
                {
                    e.Value = "-";
                    e.FormattingApplied = true;
                }

                return;
            }

            switch (columnName)
            {
                case ColSize:
                    e.Value = FormatUtils.FormatFileSize((long)e.Value);
                    e.FormattingApplied = true;
                    break;
                case ColDuration:
                    e.Value = TransferModels.FormatDurationMs((double)e.Value);
                    e.FormattingApplied = true;
                    break;
                case ColSpeed:
                    e.Value = ((double)e.Value).ToString("F2") + " MB/s";
                    e.FormattingApplied = true;
                    break;
                case ColStatus:
                    TransferModels rowTransfer = GetTransferForRow(dgvTransfers.Rows[e.RowIndex]);

                    if (rowTransfer != null)
                    {
                        Color statusColor = GetStatusColor(rowTransfer.Status);
                        e.CellStyle.ForeColor = statusColor;
                        e.CellStyle.SelectionForeColor = statusColor;
                    }

                    break;
            }
        }

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
                    return dgvTransfers.DefaultCellStyle.ForeColor;
            }
        }

        // puni ARN i puni tekst greške kao tooltip - u stupcu se vide skraćeno
        private void dgvTransfers_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            TransferModels transfer = GetTransferForRow(dgvTransfers.Rows[e.RowIndex]);

            if (transfer == null)
            {
                return;
            }

            string columnName = dgvTransfers.Columns[e.ColumnIndex].Name;

            if (columnName == ColIdentity)
            {
                e.ToolTipText = transfer.AwsIdentity;
            }
            else if (columnName == ColError)
            {
                e.ToolTipText = transfer.ErrorMessage;
            }
            else if (columnName == ColKey)
            {
                e.ToolTipText = transfer.LocalPath;
            }
            else if (columnName == ColBucket)
            {
                e.ToolTipText = transfer.Bucket;
            }
            else if (columnName == ColUser)
            {
                e.ToolTipText = transfer.Username;
            }
        }

        private void ShowSummary(TransferReportSummary summary)
        {
            lblSumTotal.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("report_sum_total_label"), summary.TotalCount);
            lblSumSuccess.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("report_sum_success_label"), summary.GetSuccessPercent().ToString("F0") + " %");
            lblSumUpload.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("upload"), FormatUtils.FormatFileSize(summary.UploadedBytes));
            lblSumDownload.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("download"), FormatUtils.FormatFileSize(summary.DownloadedBytes));
            lblSumErrors.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("report_sum_errors_label"), summary.FailedCount + " / " + summary.CancelledCount);

            if (summary.AverageSpeedMBps.HasValue)
            {
                lblSumSpeed.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("report_sum_speed_label"), summary.AverageSpeedMBps.Value.ToString("F2") + " MB/s");
            }
            else
            {
                lblSumSpeed.Text = LanguageHelper.Format("report_summary_tile", LanguageHelper.Get("report_sum_speed_label"), "-");
            }

            if (summary.MostCommonErrorCount > 0)
            {
                lblCommonError.Text = LanguageHelper.Format("report_common_error", summary.MostCommonError, summary.MostCommonErrorCount);
            }
            else
            {
                lblCommonError.Text = string.Empty;
            }
        }

        // ---------------------------------------------------------------
        // Akcije nad odabranim retcima

        private TransferModels GetTransferForRow(DataGridViewRow row)
        {
            object idValue = row.Cells[ColId].Value;

            if (idValue == null || idValue == DBNull.Value)
            {
                return null;
            }

            TransferModels transfer;

            if (_transfersById.TryGetValue((int)idValue, out transfer))
            {
                return transfer;
            }

            return null;
        }

        private List<TransferModels> GetSelectedTransfers()
        {
            List<TransferModels> selected = new List<TransferModels>();

            foreach (DataGridViewRow row in dgvTransfers.SelectedRows)
            {
                TransferModels transfer = GetTransferForRow(row);

                if (transfer != null)
                {
                    selected.Add(transfer);
                }
            }

            return selected;
        }

        private void dgvTransfers_SelectionChanged(object sender, EventArgs e)
        {
            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            List<TransferModels> selected = GetSelectedTransfers();

            bool anyWithS3Path = false;

            foreach (TransferModels transfer in selected)
            {
                if (string.IsNullOrEmpty(transfer.Bucket) == false)
                {
                    anyWithS3Path = true;
                    break;
                }
            }

            btnCopyS3Path.Enabled = anyWithS3Path;
            btnOpenFolder.Enabled = selected.Count == 1 && string.IsNullOrEmpty(selected[0].LocalPath) == false;
            btnDeleteHistory.Enabled = selected.Count > 0;
            btnExportCsv.Enabled = _currentTransfers.Count > 0;
            btnExportPdf.Enabled = _currentTransfers.Count > 0;
        }

        private void btnCopyS3Path_Click(object sender, EventArgs e)
        {
            List<string> paths = new List<string>();

            foreach (TransferModels transfer in GetSelectedTransfers())
            {
                string path = transfer.GetS3Path();

                if (string.IsNullOrEmpty(path) == false && paths.Contains(path) == false)
                {
                    paths.Add(path);
                }
            }

            if (paths.Count == 0)
            {
                return;
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, paths));
                lblRowCount.Text = LanguageHelper.Format("report_copied_paths", paths.Count);
            }
            catch (ExternalException)
            {
                MessageBox.Show(LanguageHelper.Get("report_clipboard_busy"));
            }
        }

        // otvara Explorer s označenom datotekom; ako datoteka više ne postoji, otvara njezinu mapu
        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            List<TransferModels> selected = GetSelectedTransfers();

            if (selected.Count != 1)
            {
                return;
            }

            string localPath = selected[0].LocalPath;

            try
            {
                if (File.Exists(localPath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", "/select,\"" + localPath + "\"");
                    startInfo.UseShellExecute = true;
                    Process.Start(startInfo);
                    return;
                }

                string folder = Path.GetDirectoryName(localPath);

                if (string.IsNullOrEmpty(folder) == false && Directory.Exists(folder))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", "\"" + folder + "\"");
                    startInfo.UseShellExecute = true;
                    Process.Start(startInfo);
                    lblRowCount.Text = LanguageHelper.Get("report_file_gone_folder_opened");
                    return;
                }

                MessageBox.Show(LanguageHelper.Format("report_file_and_folder_gone", localPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageHelper.Format("report_open_folder_error", ex.Message));
            }
        }

        private void btnDeleteHistory_Click(object sender, EventArgs e)
        {
            List<TransferModels> selected = GetSelectedTransfers();

            if (selected.Count == 0)
            {
                return;
            }

            DialogResult confirm = MessageBox.Show(
                LanguageHelper.Format("report_delete_confirm", selected.Count),
                LanguageHelper.Get("common_confirm_delete_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            List<int> ids = new List<int>();

            foreach (TransferModels transfer in selected)
            {
                ids.Add(transfer.Id);
            }

            try
            {
                DatabaseManager.Instance.DeleteTransfers(ids);
                ActivityLogger.Log("Brisanje povijesti", "ReportForm", "Obrisano zapisa: " + ids.Count);

                RefreshBucketOptions();
                LoadReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageHelper.Format("report_delete_error", ex.Message));
            }
        }

        // ---------------------------------------------------------------
        // Izvoz

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (_currentTransfers.Count == 0)
            {
                MessageBox.Show(LanguageHelper.Get("report_no_transfers_for_filter"));
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV|*.csv";
                dialog.FileName = "AWSGateway_prijenosi_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    TransferCsvExporter exporter = new TransferCsvExporter();
                    exporter.Export(dialog.FileName, _currentTransfers);

                    ActivityLogger.Log("CSV izvještaj", "ReportForm", _currentTransfers.Count + " prijenosa - " + _currentFilter.Description);
                    MessageBox.Show(LanguageHelper.Format("report_export_csv_done", _currentTransfers.Count));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageHelper.Format("report_export_csv_error", ex.Message));
                }
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (_currentTransfers.Count == 0)
            {
                MessageBox.Show(LanguageHelper.Get("report_no_transfers_for_filter"));
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = LanguageHelper.Get("report_pdf_filter");
                dialog.FileName = "AWSGateway_izvjestaj_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    PdfReportGenerator generator = new PdfReportGenerator();
                    generator.GenerateTransferReport(dialog.FileName, _currentTransfers, _currentSummary, _currentFilter, chkAllUsers.Checked);

                    ActivityLogger.Log("PDF izvještaj", "ReportForm", _currentTransfers.Count + " prijenosa - " + _currentFilter.Description);
                    MessageBox.Show(LanguageHelper.Get("report_pdf_created"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageHelper.Format("report_pdf_error", ex.Message));
                }
            }
        }
    }
}