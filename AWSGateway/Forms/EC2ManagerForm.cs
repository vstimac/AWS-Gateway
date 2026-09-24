using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;
using AWSGateway.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AWSGateway.Forms
{
    public partial class EC2ManagerForm : Form, IAppModule
    {
        // širina naziva u sažetku - tekst je u Consolas fontu pa se vrijednosti poravnaju u stupac
        private const int SummaryLabelWidth = 24;

        private AWSProfile _profile;
        private EC2Service _ec2Service;
        private InstanceUsageAnalyzer _analyzer;

        // opcije perioda analize - isti indeks kao stavke u cmbPeriod
        private List<TimeSpan> _periodLookbacks = new List<TimeSpan>();
        private List<int> _periodStepSeconds = new List<int>();

        // pamti zadnje postavljeno "značenje" boje lblUsageLevel - ForeColor iz koda nije "zadana" boja pa se
        // ne osvježava sam kroz ApplyToControls; bez ovoga bi boja ostala zaleđena na staroj temi nakon promjene
        private UiTheme.StatusKind _usageLevelKind = UiTheme.StatusKind.Neutral;

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
            UiTheme.Apply(this, btnRefresh);

            lblStatus.Font = UiTheme.Fonts.Label;
            lblUsageLevel.Font = UiTheme.Fonts.Metric;
            txtUsageSummary.Font = UiTheme.Fonts.Mono;
            lblUsageLevel.ForeColor = UiTheme.StatusKindColor(_usageLevelKind);
        }

        public EC2ManagerForm(AWSProfile profile)
        {
            InitializeComponent();

            _profile = profile;
            _ec2Service = new EC2Service(_profile);
            _analyzer = new InstanceUsageAnalyzer();

            // ApplyLanguage puni i cmbPeriod (PopulatePeriodOptions), pa se ne zove zasebno
            ApplyLanguage();

            Helpers.UiTheme.Apply(this, btnRefresh);
        }

        // ako je iznimka posljedica istekle privremene (STS) sesije, prikazujemo razumljivu poruku umjesto teksta iznimke
        private static string GetErrorMessage(Exception ex, string prefixKey)
        {
            if (AwsSessionHelper.IsExpiredSessionError(ex))
            {
                return LanguageHelper.Get("common_session_expired_inline");
            }

            return LanguageHelper.Format(prefixKey, ex.Message);
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("ec2_title");
            btnStart.Text = LanguageHelper.Get("start_instance");
            btnStop.Text = LanguageHelper.Get("stop_instance");
            btnReboot.Text = LanguageHelper.Get("reboot_instance");
            btnRefresh.Text = LanguageHelper.Get("refresh");
            lblStatus.Text = LanguageHelper.Get("loading_instances");
            listView1.Columns[0].HeaderText = LanguageHelper.Get("column_id");
            listView1.Columns[1].HeaderText = LanguageHelper.Get("data_files_col_name");
            listView1.Columns[2].HeaderText = LanguageHelper.Get("column_status");
            listView1.Columns[3].HeaderText = LanguageHelper.Get("data_files_col_region");
            grpMetrics.Text = LanguageHelper.Get("ec2_metrics_group_title");
            lblPeriod.Text = LanguageHelper.Get("ec2_summary_period");
            btnMetrics.Text = LanguageHelper.Get("ec2_btn_analyze");
            btnDetails.Text = LanguageHelper.Get("ec2_btn_details");
            listViewMetrics.Columns[0].HeaderText = LanguageHelper.Get("metric_column");
            listViewMetrics.Columns[1].HeaderText = LanguageHelper.Get("time_column");
            listViewMetrics.Columns[2].HeaderText = LanguageHelper.Get("ec2_col_avg");
            listViewMetrics.Columns[3].HeaderText = LanguageHelper.Get("ec2_col_max");
            listViewMetrics.Columns[4].HeaderText = LanguageHelper.Get("ec2_col_min");
            listViewMetrics.Columns[5].HeaderText = LanguageHelper.Get("ec2_col_sum");
            listViewMetrics.Columns[6].HeaderText = LanguageHelper.Get("ec2_col_unit");

            PopulatePeriodOptions();
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (listViewMetrics.Visible)
            {
                listViewMetrics.Visible = false;
                btnDetails.Text = LanguageHelper.Get("ec2_btn_details");
            }
            else
            {
                listViewMetrics.Visible = true;
                btnDetails.Text = LanguageHelper.Get("ec2_btn_hide_details");
            }
        }

        // korak se prilagođava periodu - 5 min je najsitniji korak osnovnog (besplatnog) nadzora,
        // a za 7 dana se uzima 1 h da broj točaka ostane pregledan (AWS dopušta najviše 1440 točaka po pozivu)
        private void PopulatePeriodOptions()
        {
            int previousIndex = cmbPeriod.SelectedIndex;

            cmbPeriod.Items.Clear();
            _periodLookbacks.Clear();
            _periodStepSeconds.Clear();

            cmbPeriod.Items.Add(LanguageHelper.Get("ec2_period_3h"));
            _periodLookbacks.Add(TimeSpan.FromHours(3));
            _periodStepSeconds.Add(300);

            cmbPeriod.Items.Add(LanguageHelper.Get("ec2_period_24h"));
            _periodLookbacks.Add(TimeSpan.FromHours(24));
            _periodStepSeconds.Add(300);

            cmbPeriod.Items.Add(LanguageHelper.Get("ec2_period_7d"));
            _periodLookbacks.Add(TimeSpan.FromDays(7));
            _periodStepSeconds.Add(3600);

            // zadrži odabir korisnika kod ponovnog punjenja (promjena jezika), inače zadano na 24 h
            if (previousIndex >= 0 && previousIndex < cmbPeriod.Items.Count)
            {
                cmbPeriod.SelectedIndex = previousIndex;
            }
            else
            {
                cmbPeriod.SelectedIndex = 1;
            }
        }

        private async void EC2ManagerForm_Load(object sender, EventArgs e)
        {
            await LoadInstancesAsync();
        }

        // puni listu instancama
        private async Task LoadInstancesAsync()
        {
            try
            {
                lblStatus.Text = LanguageHelper.Get("loading_instances");

                List<AWSResource> instances = await _ec2Service.DescribeInstancesAsync();

                listView1.Rows.Clear();

                int count = 0;

                foreach (AWSResource resource in instances)
                {
                    int rowIndex = listView1.Rows.Add(resource.ResourceId, resource.Name, resource.Status, resource.Region);

                    // cijeli objekt cuvam u Tag da ga kasnije ne moramo slagati iz teksta
                    listView1.Rows[rowIndex].Tag = resource;

                    if (resource.IsActive()) // EC2 running
                    {
                        count++;
                    }
                }

                lblStatus.Text = LanguageHelper.Format("ec2_instance_count", instances.Count, count);
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetErrorMessage(ex, "ec2_error_list");
            }
        }

        private AWSResource GetSelectedResource()
        {
            if (listView1.SelectedRows.Count == 0)
            {
                return null;
            }

            return listView1.SelectedRows[0].Tag as AWSResource;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            AWSResource selected = GetSelectedResource();

            if (selected == null)
            {
                MessageBox.Show(LanguageHelper.Get("ec2_select_instance"));
                return;
            }

            try
            {
                await _ec2Service.StartInstanceAsync(selected.ResourceId);
                lblStatus.Text = LanguageHelper.Format("ec2_status_started", selected.GetDisplayName());
                await LoadInstancesAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetErrorMessage(ex, "ec2_error_start");
            }
        }

        private async void btnStop_Click(object sender, EventArgs e)
        {
            AWSResource selected = GetSelectedResource();

            if (selected == null)
            {
                MessageBox.Show(LanguageHelper.Get("ec2_select_instance"));
                return;
            }

            try
            {
                await _ec2Service.StopInstanceAsync(selected.ResourceId);
                lblStatus.Text = LanguageHelper.Format("ec2_status_stopped", selected.GetDisplayName());
                await LoadInstancesAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetErrorMessage(ex, "ec2_error_stop");
            }
        }

        private async void btnReboot_Click(object sender, EventArgs e)
        {
            AWSResource selected = GetSelectedResource();

            if (selected == null)
            {
                MessageBox.Show(LanguageHelper.Get("ec2_select_instance"));
                return;
            }

            try
            {
                await _ec2Service.RebootInstanceAsync(selected.ResourceId);
                lblStatus.Text = LanguageHelper.Format("ec2_status_rebooted", selected.GetDisplayName());
                await LoadInstancesAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = GetErrorMessage(ex, "ec2_error_reboot");
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadInstancesAsync();
        }

        // dohvat metrika -> analiza -> sažetak s procjenom, sirove točke ostaju ispod kao detalji
        private async void btnMetrics_Click(object sender, EventArgs e)
        {
            AWSResource selected = GetSelectedResource();

            if (selected == null)
            {
                MessageBox.Show(LanguageHelper.Get("ec2_select_instance"));
                return;
            }

            int periodIndex = cmbPeriod.SelectedIndex;

            if (periodIndex < 0 || periodIndex >= _periodLookbacks.Count)
            {
                MessageBox.Show(LanguageHelper.Get("ec2_select_period"));
                return;
            }

            TimeSpan lookback = _periodLookbacks[periodIndex];
            int periodSeconds = _periodStepSeconds[periodIndex];
            string periodText = cmbPeriod.SelectedItem.ToString();

            btnMetrics.Enabled = false;
            listViewMetrics.Rows.Clear();
            listViewMetrics.Visible = false;
            btnDetails.Text = LanguageHelper.Get("ec2_btn_details");
            txtUsageSummary.Clear();
            _usageLevelKind = UiTheme.StatusKind.Neutral;
            lblUsageLevel.ForeColor = UiTheme.StatusKindColor(_usageLevelKind);
            lblUsageLevel.Text = LanguageHelper.Get("ec2_fetching_metrics");

            try
            {
                List<EC2MetricPoint> points = await _ec2Service.GetInstanceMetricsAsync(selected.ResourceId, lookback, periodSeconds);
                InstanceUsageReport report = _analyzer.Analyze(points, lookback, periodSeconds);

                lblUsageLevel.Text = LanguageHelper.Format("ec2_usage_level", report.GetLevelTitle());
                _usageLevelKind = GetLevelKind(report.Level);
                lblUsageLevel.ForeColor = UiTheme.StatusKindColor(_usageLevelKind);
                txtUsageSummary.Text = BuildSummaryText(selected, report, periodText);

                FillDetails(points);

                lblStatus.Text = LanguageHelper.Format("ec2_status_analyzed", selected.GetDisplayName());
                ActivityLogger.Log("EC2 analiza", "EC2ManagerForm", selected.ResourceId + " - " + report.GetLevelTitle() + " (" + periodText + ")");
            }
            catch (Exception ex)
            {
                lblUsageLevel.Text = LanguageHelper.Get("ec2_analysis_failed");
                lblStatus.Text = GetErrorMessage(ex, "ec2_error_metrics");
                ActivityLogger.LogError("EC2 analiza greška", "EC2ManagerForm", selected.ResourceId + ": " + ex.Message);
            }
            finally
            {
                btnMetrics.Enabled = true;
            }
        }

        private string BuildSummaryText(AWSResource instance, InstanceUsageReport report, string periodText)
        {
            StringBuilder text = new StringBuilder();

            AppendRow(text, LanguageHelper.Get("ec2_summary_instance"), instance.Name + " (" + instance.ResourceId + ")");
            AppendRow(text, LanguageHelper.Get("ec2_summary_period"), LanguageHelper.Format("ec2_period_value", periodText, FormatStep(report.PeriodSeconds)));
            AppendRow(text, LanguageHelper.Get("ec2_summary_coverage"), LanguageHelper.Format("ec2_coverage_value", report.CoveragePercent.ToString("F0"), report.CpuIntervalCount, report.ExpectedIntervalCount));
            text.AppendLine();

            if (report.CpuIntervalCount == 0)
            {
                text.AppendLine(LanguageHelper.Get("ec2_summary_no_data"));
            }
            else
            {
                AppendRow(text, LanguageHelper.Get("ec2_summary_cpu_avg"), report.CpuAverage.ToString("F1") + " %");
                AppendRow(text, LanguageHelper.Get("ec2_summary_cpu_max"), report.CpuMaximum.ToString("F1") + " %");
                AppendRow(text, LanguageHelper.Get("ec2_summary_network"), LanguageHelper.Format("ec2_network_value",
                    FormatUtils.FormatFileSize(report.GetTotalNetworkBytes()),
                    FormatUtils.FormatFileSize(report.NetworkInBytes),
                    FormatUtils.FormatFileSize(report.NetworkOutBytes)));
                AppendRow(text, LanguageHelper.Get("ec2_summary_network_per_day"), FormatUtils.FormatFileSize(report.NetworkBytesPerDay));

                if (report.HasCreditData)
                {
                    AppendRow(text, LanguageHelper.Get("ec2_summary_cpu_credits"), report.CpuCreditMinimum.ToString("F1"));
                }
            }

            text.AppendLine();
            AppendRow(text, LanguageHelper.Get("ec2_summary_description"), report.GetDescription());
            AppendRow(text, LanguageHelper.Get("ec2_summary_recommendation"), report.GetRecommendation());
            text.AppendLine();

            text.AppendLine(LanguageHelper.Get("ec2_summary_warnings_header"));

            if (report.Warnings.Count == 0)
            {
                text.AppendLine(LanguageHelper.Get("ec2_warnings_none"));
            }
            else
            {
                foreach (string warning in report.Warnings)
                {
                    text.AppendLine("  • " + warning);
                }
            }

            return text.ToString();
        }

        private void AppendRow(StringBuilder text, string label, string value)
        {
            text.AppendLine(label.PadRight(SummaryLabelWidth) + value);
        }

        private string FormatStep(int seconds)
        {
            if (seconds >= 3600)
            {
                return (seconds / 3600) + " h";
            }

            return (seconds / 60) + " min";
        }

        // boja procjene - Idle/Low znače trošak bez koristi (upozorenje), High znači da je instanca preopterećena (razmotriti veći tip - greška)
        private static UiTheme.StatusKind GetLevelKind(InstanceUsageReport.UsageLevel level)
        {
            switch (level)
            {
                case InstanceUsageReport.UsageLevel.Idle:
                    return UiTheme.StatusKind.Warning;
                case InstanceUsageReport.UsageLevel.Low:
                    return UiTheme.StatusKind.Warning;
                case InstanceUsageReport.UsageLevel.Moderate:
                    return UiTheme.StatusKind.Success;
                case InstanceUsageReport.UsageLevel.High:
                    return UiTheme.StatusKind.Error;
                default:
                    return UiTheme.StatusKind.Neutral;
            }
        }

        // sirove točke kao dokaz uz procjenu
        private void FillDetails(List<EC2MetricPoint> points)
        {
            foreach (EC2MetricPoint point in points)
            {
                listViewMetrics.Rows.Add(
                    point.MetricName,
                    point.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    point.Average.ToString("F2"),
                    point.Maximum.ToString("F2"),
                    point.Minimum.ToString("F2"),
                    point.Sum.ToString("F2"),
                    point.Unit);
            }
        }
    }
}