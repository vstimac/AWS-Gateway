using System.Globalization;
using Amazon.CostExplorer;
using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;
using AWSGateway.Utils;

namespace AWSGateway.Forms
{
    public partial class CostForm : Form, IAppModule
    {
        private AWSProfile _profile;
        private CostCacheOrchestrator _orchestrator;
        private CostSummaryCalculator _calculator = new CostSummaryCalculator();

        // sažetak trenutnog prikaza
        //
        // odabir usluge u tablici prikazuje njezinu raspodjelu po regijama
        private CostSummary _currentSummary;

        // pamti zadnje postavljeno "značenje" boje statusnih labela - ForeColor iz koda nije "zadana" boja pa se
        // ne osvježava sam kroz ApplyToControls; bez ovoga bi boja ostala zaleđena na staroj temi nakon promjene
        private UiTheme.StatusKind _cacheStatusKind = UiTheme.StatusKind.Neutral;
        private UiTheme.StatusKind _comparisonKind = UiTheme.StatusKind.Neutral;

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
            UiTheme.Apply(this, btnLoad);

            lblTotal.Font = UiTheme.Fonts.Title;
            lblServices.Font = UiTheme.Fonts.Section;
            lblRegions.Font = UiTheme.Fonts.Section;
            lblCacheStatus.ForeColor = UiTheme.StatusKindColor(_cacheStatusKind);
            lblComparison.ForeColor = UiTheme.StatusKindColor(_comparisonKind);
        }

        public CostForm(AWSProfile profile)
        {
            InitializeComponent();

            _profile = profile;

            // TTL predmemorije se cita iz postavki aplikacije (podesivo u SettingsForm), zadano 6 sati
            TimeSpan cacheFreshness = TimeSpan.FromHours(AppSettings.Instance.CacheTtlHours);
            _orchestrator = new CostCacheOrchestrator(new CostExplorerService(_profile), cacheFreshness);

            // ApplyLanguage puni i cmbPeriod (PopulatePeriods), pa se ne zove zasebno
            ApplyLanguage();
            ShowEmptyState();

            UiTheme.Apply(this, btnLoad);
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("cost_title");
            lblPeriod.Text = LanguageHelper.Get("ec2_summary_period");
            btnLoad.Text = LanguageHelper.Get("cost_btn_load");
            btnForceRefresh.Text = LanguageHelper.Get("cost_btn_force_refresh");
            btnExportLogCsv.Text = LanguageHelper.Get("cost_btn_export_log");

            // ne prepisuje stvaran status (npr. već dohvaćeni troškovi) - taj tekst se ionako iznova postavlja pri sljedećem dohvatu
            if (_currentSummary == null)
            {
                lblCacheStatus.Text = LanguageHelper.Get("cost_status_initial_hint");
            }

            lblEmptyState.Text = LanguageHelper.Get("cost_empty_state_hint");
            lblServices.Text = LanguageHelper.Get("cost_lbl_by_service");
            colService.HeaderText = LanguageHelper.Get("cost_column_service");
            colServiceAmount.HeaderText = LanguageHelper.Get("cost_column_amount");
            colServiceShare.HeaderText = LanguageHelper.Get("cost_col_share");
            lblRegions.Text = LanguageHelper.Get("cost_lbl_by_region");
            colRegion.HeaderText = LanguageHelper.Get("cost_column_region");
            colRegionAmount.HeaderText = LanguageHelper.Get("cost_column_amount");
            lblNote.Text = LanguageHelper.Get("cost_note_text");

            // naslov i "nema podataka" u grafu se iscrtavaju u OnPaint - treba ga eksplicitno ponovno iscrtati
            costChart.Invalidate();

            PopulatePeriods();
        }

        private void PopulatePeriods()
        {
            int previousIndex = cmbPeriod.SelectedIndex;

            cmbPeriod.Items.Clear();
            cmbPeriod.Items.Add(LanguageHelper.Get("cost_period_this_month"));
            cmbPeriod.Items.Add(LanguageHelper.Get("cost_period_last_month"));
            cmbPeriod.Items.Add(LanguageHelper.Get("cost_period_last_30_days"));

            // zadrži odabir korisnika kod ponovnog punjenja (promjena jezika), inače zadano na prvi period
            if (previousIndex >= 0 && previousIndex < cmbPeriod.Items.Count)
            {
                cmbPeriod.SelectedIndex = previousIndex;
            }
            else
            {
                cmbPeriod.SelectedIndex = 0;
            }
        }

        private CostPeriod GetSelectedPeriod()
        {
            DateTime todayUtc = DateTime.UtcNow;

            switch (cmbPeriod.SelectedIndex)
            {
                case 1:
                    return CostPeriod.LastMonth(todayUtc);
                case 2:
                    return CostPeriod.Last30Days(todayUtc);
                default:
                    return CostPeriod.ThisMonth(todayUtc);
            }
        }

        // ako je iznimka posljedica istekle privremene (STS) sesije, prikazujemo razumljivu poruku umjesto teksta iznimke
        private static string GetErrorMessage(Exception ex)
        {
            if (AwsSessionHelper.IsExpiredSessionError(ex))
            {
                return LanguageHelper.Get("common_session_expired_inline");
            }

            if (IsCostExplorerAccessDenied(ex))
            {
                return LanguageHelper.Get("cost_access_denied");
            }

            return LanguageHelper.Format("common_error_prefix", ex.Message);
        }

        // CostExplorerService omata AmazonCostExplorerException(AccessDeniedException) u Exception s vlastitim (hrvatskim,
        // neprevedivim) tekstom - Services sloj se ne dira, pa forma ovdje prepozna slučaj po unutarnjoj iznimci
        // i prikaže svoju prevedenu poruku umjesto ex.Message. Ostale iznimke i dalje prikazuju ex.Message kao dosad.
        private static bool IsCostExplorerAccessDenied(Exception ex)
        {
            Exception current = ex;

            while (current != null)
            {
                AmazonCostExplorerException costExplorerEx = current as AmazonCostExplorerException;

                if (costExplorerEx != null && costExplorerEx.ErrorCode == "AccessDeniedException")
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }

        // ---------------------------------------------------------------
        // Dohvat

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            await LoadCostsAsync(false);
        }

        // zaobilazi predmemoriju - korisnik mora znati da svaki zahtjev prema Cost Exploreru košta
        private async void btnForceRefresh_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                LanguageHelper.Get("cost_force_refresh_confirm"),
                LanguageHelper.Get("cost_force_refresh_confirm_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            await LoadCostsAsync(true);
        }

        // IP2: svako dohvaćanje (odabrani i usporedni period zasebno) bilježi cache hit/miss, starost podataka, vrijeme odgovora i broj stranica
        private async Task LoadCostsAsync(bool forceRefresh)
        {
            SetBusy(true);

            CostPeriod period = GetSelectedPeriod();

            _cacheStatusKind = UiTheme.StatusKind.Neutral;
            lblCacheStatus.ForeColor = UiTheme.StatusKindColor(_cacheStatusKind);
            lblCacheStatus.Text = LanguageHelper.Format("cost_loading_for_period", period.Label);

            try
            {
                string cacheKey = CostCacheOrchestrator.BuildCacheKey(_profile.AwsAccountId, period.StartUtc, period.EndUtc);
                CostFetchResult current = await _orchestrator.GetCostsAsync(cacheKey, period.StartUtc, period.EndUtc, forceRefresh);

                // usporedni period nije nužan - njegova greška ne sprječava prikaz odabranog perioda
                CostFetchResult comparison = null;
                string comparisonError = string.Empty;

                try
                {
                    string comparisonKey = CostCacheOrchestrator.BuildCacheKey(_profile.AwsAccountId, period.ComparisonStartUtc, period.ComparisonEndUtc);
                    comparison = await _orchestrator.GetCostsAsync(comparisonKey, period.ComparisonStartUtc, period.ComparisonEndUtc, forceRefresh);
                }
                catch (Exception comparisonEx)
                {
                    comparisonError = GetErrorMessage(comparisonEx);
                }

                List<CostRow> comparisonRows = null;

                if (comparison != null)
                {
                    comparisonRows = comparison.Rows;
                }

                CostSummary summary = _calculator.Calculate(current.Rows, comparisonRows, period.StartUtc, period.EndUtc);

                ShowSummary(summary, period, comparisonError);
                ShowCacheStatus(current);

                string source;

                if (current.IsStale)
                {
                    source = "istekla predmemorija";
                }
                else if (current.WasCacheHit)
                {
                    source = "predmemorija";
                }
                else
                {
                    source = "AWS";
                }

                ActivityLogger.Log("CostExplorer", "CostForm", period.Label + ": " + FormatAmount(summary.Total, summary.Unit) + " (" + source + ")");
            }
            catch (Exception ex)
            {
                ShowEmptyState();
                _cacheStatusKind = UiTheme.StatusKind.Error;
                lblCacheStatus.ForeColor = UiTheme.StatusKindColor(_cacheStatusKind);
                lblCacheStatus.Text = GetErrorMessage(ex);
                ActivityLogger.LogError("CostExplorer greska", "CostForm", ex.Message);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy)
        {
            bool idle = busy == false;

            btnLoad.Enabled = idle;
            btnForceRefresh.Enabled = idle;
            cmbPeriod.Enabled = idle;
        }

        // ---------------------------------------------------------------
        // Prikaz

        private void ShowEmptyState()
        {
            _currentSummary = null;

            lblTotal.Text = LanguageHelper.Get("cost_total_empty");
            lblComparison.Text = string.Empty;
            lblTopService.Text = string.Empty;

            listViewServices.Rows.Clear();
            listViewRegions.Rows.Clear();
            lblRegions.Text = LanguageHelper.Get("cost_lbl_by_region");

            costChart.SetData(null, "USD");

            // prazan graf i prazne tablice se ne prikazuju - jedna poruka s uputom umjesto njih
            SetResultsVisible(false);
        }

        // prije prvog dohvata ili nakon greške prikazuje se samo uputa (lblEmptyState) umjesto praznog grafa i tablica
        private void SetResultsVisible(bool visible)
        {
            costChart.Visible = visible;
            tableLayoutTables.Visible = visible;

            lblEmptyState.Text = LanguageHelper.Get("cost_empty_state_hint");
            lblEmptyState.Visible = visible == false;
        }

        private void ShowSummary(CostSummary summary, CostPeriod period, string comparisonError)
        {
            _currentSummary = summary;
            SetResultsVisible(true);

            lblTotal.Text = LanguageHelper.Format("cost_total_value", period.Label, FormatAmount(summary.Total, summary.Unit));

            ShowComparison(summary, period, comparisonError);

            ServiceCostTotal topService = summary.GetTopService();

            if (topService == null)
            {
                lblTopService.Text = LanguageHelper.Get("cost_no_costs");
            }
            else
            {
                lblTopService.Text = LanguageHelper.Format("cost_top_service", topService.Service, topService.SharePercent.ToString("F0"));
            }

            costChart.SetData(summary.Days, summary.Unit);

            listViewServices.SuspendLayout();
            listViewServices.Rows.Clear();

            foreach (ServiceCostTotal service in summary.Services)
            {
                int rowIndex = listViewServices.Rows.Add(service.Service, FormatAmount(service.Amount, summary.Unit), service.SharePercent.ToString("F1") + " %");
                listViewServices.Rows[rowIndex].Tag = service;
            }

            listViewServices.ResumeLayout();

            listViewRegions.Rows.Clear();
            lblRegions.Text = LanguageHelper.Get("cost_lbl_by_region");

            if (listViewServices.Rows.Count > 0)
            {
                listViewServices.Rows[0].Selected = true;
            }
        }

        private void ShowComparison(CostSummary summary, CostPeriod period, string comparisonError)
        {
            if (summary.ComparisonTotal.HasValue == false)
            {
                _comparisonKind = UiTheme.StatusKind.Neutral;
                lblComparison.ForeColor = UiTheme.StatusKindColor(_comparisonKind);
                lblComparison.Text = LanguageHelper.Format("cost_comparison_unavailable", period.ComparisonLabel, comparisonError);
                return;
            }

            string text = LanguageHelper.Format("cost_comparison_value", period.ComparisonLabel, FormatAmount(summary.ComparisonTotal.Value, summary.Unit));

            if (summary.ChangePercent.HasValue == false)
            {
                _comparisonKind = UiTheme.StatusKind.Neutral;
                lblComparison.ForeColor = UiTheme.StatusKindColor(_comparisonKind);
                lblComparison.Text = text;
                return;
            }

            double change = summary.ChangePercent.Value;
            string sign = string.Empty;

            if (change > 0)
            {
                sign = "+";
                _comparisonKind = UiTheme.StatusKind.Error;
            }
            else if (change < 0)
            {
                _comparisonKind = UiTheme.StatusKind.Success;
            }
            else
            {
                _comparisonKind = UiTheme.StatusKind.Neutral;
            }

            lblComparison.ForeColor = UiTheme.StatusKindColor(_comparisonKind);
            lblComparison.Text = LanguageHelper.Format("cost_comparison_with_change", text, sign, change.ToString("F0"));
        }

        private void ShowCacheStatus(CostFetchResult result)
        {
            string account = _profile.AwsAccountId;

            if (string.IsNullOrEmpty(account))
            {
                account = LanguageHelper.Get("cost_account_unknown");
            }

            string fetchedAt = result.FetchedAtUtc.ToLocalTime().ToString("dd.MM.yyyy. HH:mm");
            string age = FormatUtils.FormatDuration(result.DataAgeSeconds);

            if (result.IsStale)
            {
                _cacheStatusKind = UiTheme.StatusKind.Warning;
                lblCacheStatus.ForeColor = UiTheme.StatusKindColor(_cacheStatusKind);
                lblCacheStatus.Text = LanguageHelper.Format("cost_stale_warning", result.StaleReason, fetchedAt, age);
                return;
            }

            _cacheStatusKind = UiTheme.StatusKind.Neutral;
            lblCacheStatus.ForeColor = UiTheme.StatusKindColor(_cacheStatusKind);

            if (result.WasCacheHit)
            {
                lblCacheStatus.Text = LanguageHelper.Format("cost_status_cached", account, fetchedAt, age);
            }
            else
            {
                lblCacheStatus.Text = LanguageHelper.Format("cost_status_fresh", account, result.PageCount);
            }
        }

        private void listViewServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_currentSummary == null || listViewServices.SelectedRows.Count == 0)
            {
                return;
            }

            ServiceCostTotal service = listViewServices.SelectedRows[0].Tag as ServiceCostTotal;

            if (service == null)
            {
                return;
            }

            lblRegions.Text = LanguageHelper.Format("cost_lbl_by_region_for", service.Service);

            listViewRegions.SuspendLayout();
            listViewRegions.Rows.Clear();

            foreach (RegionCostTotal region in _calculator.GetRegionTotals(_currentSummary, service.Service))
            {
                listViewRegions.Rows.Add(GetRegionDisplay(region.Region), FormatAmount(region.Amount, _currentSummary.Unit));
            }

            listViewRegions.ResumeLayout();
        }

        // Cost Explorer za usluge bez regije vraća "global" ili "NoRegion"
        private string GetRegionDisplay(string region)
        {
            switch (region)
            {
                case "global":
                    return LanguageHelper.Get("cost_region_global");
                case "NoRegion":
                    return LanguageHelper.Get("cost_region_none");
                case "":
                    return "-";
            }

            AwsRegionInfo info = AwsRegions.Find(region);

            if (info == null)
            {
                return region;
            }

            return info.ToString();
        }

        // iznosi ispod 0,01 prikazuju se kao "< 0,01" - zaokruživanje na 0,00 skrivalo bi da trošak postoji
        private static string FormatAmount(double amount, string unit)
        {
            if (amount > 0 && amount < 0.01)
            {
                return "< " + 0.01.ToString("N2") + " " + unit;
            }

            return amount.ToString("N2") + " " + unit;
        }

        // ---------------------------------------------------------------
        // Izvoz loga predmemorije (IP2)

        // izvozi SVE zapise loga predmemorije (ne samo zadnji upit) u CSV
        private void btnExportLogCsv_Click(object sender, EventArgs e)
        {
            List<CostCacheAccessLogEntry> entries = DatabaseManager.Instance.GetAllCacheAccessLog();

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV|*.csv";
                dialog.FileName = "cache_log_export.csv";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (StreamWriter writer = new StreamWriter(dialog.FileName, false, new System.Text.UTF8Encoding(true)))
                    {
                        writer.WriteLine("CacheKey,WasCacheHit,DataAgeSeconds,ResponseTimeMs,PageCount,TtlHoursAtLog,RequestedAt");

                        foreach (CostCacheAccessLogEntry entry in entries)
                        {
                            string wasCacheHitText;

                            if (entry.WasCacheHit)
                            {
                                wasCacheHitText = "true";
                            }
                            else
                            {
                                wasCacheHitText = "false";
                            }

                            string dataAgeText;

                            if (entry.DataAgeSeconds.HasValue)
                            {
                                dataAgeText = entry.DataAgeSeconds.Value.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                dataAgeText = string.Empty;
                            }

                            string pageCountText;

                            if (entry.PageCount.HasValue)
                            {
                                pageCountText = entry.PageCount.Value.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                pageCountText = string.Empty;
                            }

                            writer.WriteLine(
                                FormatUtils.EscapeCsv(entry.CacheKey) + "," +
                                wasCacheHitText + "," +
                                dataAgeText + "," +
                                entry.ResponseTimeMs.ToString(CultureInfo.InvariantCulture) + "," +
                                pageCountText + "," +
                                entry.TtlHoursAtLog.ToString(CultureInfo.InvariantCulture) + "," +
                                FormatUtils.EscapeCsv(entry.RequestedAt));
                        }
                    }

                    MessageBox.Show(LanguageHelper.Format("cost_export_done", entries.Count, dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageHelper.Format("cost_export_error", ex.Message));
                }
            }
        }
    }
}