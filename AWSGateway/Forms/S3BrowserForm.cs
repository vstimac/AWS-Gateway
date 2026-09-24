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
    public partial class S3BrowserForm : Form, IAppModule
    {
        // ishod jednog prijenosa u skupini
        private enum TransferOutcome
        {
            Completed, Failed, Cancelled, Skipped
        }

        // S3 ograničenja za multipart upload
        private const int MinPartSizeMB = 5;
        private const int MaxPartCount = 10000;

        // SDK zadano: TransferUtilityConfig.ConcurrentServiceRequests
        private const int SdkDefaultPartRequests = 10;

        // zadane vrijednosti naprednih postavki (moraju odgovarati početnim vrijednostima u Designeru)
        private const int DefaultPartSizeMB = 0;
        private const int DefaultFileConcurrency = 4;
        private const int DefaultPartConcurrency = 0;

        // statusi u TransferHistory - iste vrijednosti filtrira ReportForm
        private const string StatusCompleted = "completed";
        private const string StatusFailed = "failed";
        private const string StatusCancelled = "cancelled";

        // stupci liste prijenosa
        private const int ColTransferPercent = 2;
        private const int ColTransferStatus = 3;

        // koliko naziva najviše prikazati u dijalogu s popisom
        private const int MaxNamesInDialog = 10;

        private AWSProfile _profile;
        private S3Service _s3Service;
        private CancellationTokenSource _cancellation;
        private bool _isBusy;
        private bool _resourcesReleased;

        public bool IsBusy
        {
            get { return _isBusy; }
        }

        public void ApplyShellLanguage()
        {
            ApplyLanguage();
        }

        public void ApplyShellTheme()
        {
            UiTheme.Apply(this, btnUpload);
            UiTheme.StyleDangerButton(btnDelete);
            lblBucket.Font = UiTheme.Fonts.Section;
            lblList.Font = UiTheme.Fonts.Section;
            lblObjectsEmpty.ForeColor = UiTheme.Colors.Muted;
        }

        // bucket čiji su objekti trenutno prikazani - sve radnje idu na njega, ne na tekst koji je možda upisan a nije učitan
        private string _loadedBucket = string.Empty;
        private string _loadedBucketRegion = string.Empty;
        private List<AWSResource> _allObjects = new List<AWSResource>();
        private List<TimeSpan> _urlDurationOptions = new List<TimeSpan>();

        // napredak trenutne skupine prijenosa - isti indeks kao redak u lstTransfers
        private long[] _transferredBytes = new long[0];
        private long[] _totalBytes = new long[0];
        private bool[] _transferFinished = new bool[0];

        // raste s svakom novom skupinom - zakašnjela poruka o napretku iz prethodne skupine se prepoznaje i odbacuje
        private int _transferBatchId = 0;

        public S3BrowserForm(AWSProfile profile)
        {
            InitializeComponent();

            _profile = profile;
            _s3Service = new S3Service(_profile);

            Resize += delegate
            {
                LayoutObjectActions();
            };

            ApplyLanguage();
            ApplyShellTheme();

            PopulateUrlDurationOptions();
            UpdateConcurrencyHint();
            SetAdvancedVisible(false);
            SetBusy(false, false);
            LayoutObjectActions();
        }

        private void LayoutObjectActions()
        {
            int left = 20;
            int gap = UiTheme.Space1;
            int right = Math.Max(left, ClientSize.Width - left);
            int deleteWidth = btnDelete.Width;

            btnDownload.Left = left;
            cmbUrlDuration.Left = btnDownload.Right + gap;
            btnGenerateUrl.Left = cmbUrlDuration.Right + gap;
            btnDelete.Left = Math.Max(btnGenerateUrl.Right + gap, right - deleteWidth);

            txtUrl.Width = Math.Max(180, ClientSize.Width - txtUrl.Left - left);
            lblUrlInfo.Left = btnCopyUrl.Right + gap;
            lblUrlInfo.Width = Math.Max(0, ClientSize.Width - lblUrlInfo.Left - left);
        }

        // ako je iznimka posljedica istekle privremene (STS) sesije, prikazujemo razumljivu poruku umjesto teksta iznimke
        private static string GetErrorMessage(Exception ex)
        {
            if (AwsSessionHelper.IsExpiredSessionError(ex))
            {
                return LanguageHelper.Get("common_session_expired_inline");
            }

            return LanguageHelper.Format("common_error_prefix", ex.Message);
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("s3_title");
            lblBucket.Text = LanguageHelper.Get("bucket_name");
            btnRefresh.Text = LanguageHelper.Get("refresh");
            lblList.Text = LanguageHelper.Get("objects_label");
            btnUpload.Text = LanguageHelper.Get("upload");
            btnCancel.Text = LanguageHelper.Get("cancel");
            btnDownload.Text = LanguageHelper.Get("download");
            btnDelete.Text = LanguageHelper.Get("delete");
            btnGenerateUrl.Text = LanguageHelper.Get("generate_url");
            txtFilter.PlaceholderText = LanguageHelper.Get("s3_filter_placeholder");
            colName.HeaderText = LanguageHelper.Get("data_files_col_name");
            colSize.HeaderText = LanguageHelper.Get("report_col_size");
            colStorageClass.HeaderText = LanguageHelper.Get("s3_col_storage_class");
            colModified.HeaderText = LanguageHelper.Get("s3_col_modified");
            txtUrl.PlaceholderText = LanguageHelper.Get("s3_url_placeholder");
            btnCopyUrl.Text = LanguageHelper.Get("s3_copy_url");
            lblObjectsEmpty.Text = LanguageHelper.Get("s3_objects_empty_no_bucket");
            grpTransfer.Text = LanguageHelper.Get("s3_transfer_group");
            lblPartSize.Text = LanguageHelper.Get("s3_lbl_part_size");
            lblFileConcurrency.Text = LanguageHelper.Get("s3_lbl_file_concurrency");
            lblPartConcurrency.Text = LanguageHelper.Get("s3_lbl_part_concurrency");
            colTransferName.HeaderText = LanguageHelper.Get("s3_col_transfer_name");
            colTransferSize.HeaderText = LanguageHelper.Get("report_col_size");
            colTransferPercent.HeaderText = LanguageHelper.Get("s3_col_transfer_percent");
            colTransferStatus.HeaderText = LanguageHelper.Get("column_status");
        }

        // ---------------------------------------------------------------
        // Stanje forme

        // tijekom prijenosa ili brisanja zaključava sve što mijenja bucket ili odabir objekata
        private void SetBusy(bool busy, bool cancellable)
        {
            _isBusy = busy;

            bool idle = busy == false;

            cmbBucket.Enabled = idle;
            btnRefresh.Enabled = idle;
            btnUpload.Enabled = idle;
            btnDownload.Enabled = idle;
            btnDelete.Enabled = idle;
            numPartSize.Enabled = idle;
            numFileConcurrency.Enabled = idle;
            numPartConcurrency.Enabled = idle;
            cmbUrlDuration.Enabled = idle && _urlDurationOptions.Count > 0;
            btnGenerateUrl.Enabled = idle && _urlDurationOptions.Count > 0;
            btnCancel.Enabled = busy && cancellable;
        }

        private void S3BrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isBusy)
            {
                MessageBox.Show(LanguageHelper.Get("s3_transfer_busy_close"));
                e.Cancel = true;
            }
        }

        private void S3BrowserForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseTransferResources();
        }

        // ugrađen u ljusku - modul se ne zatvara pojedinačno pa ovo poziva i Dispose(bool) iz Designera kod gašenja aplikacije;
        // guard sprječava dvostruko oslobađanje ako oba puta ipak dođu do izražaja (npr. izravna samostalna upotreba forme)
        private void ReleaseTransferResources()
        {
            if (_resourcesReleased)
            {
                return;
            }

            _resourcesReleased = true;

            if (_cancellation != null)
            {
                _cancellation.Dispose();
            }

            _s3Service.Dispose();
        }

        // ---------------------------------------------------------------
        // Bucket i popis objekata

        private async void S3BrowserForm_Load(object sender, EventArgs e)
        {
            cmbBucket.Items.Clear();

            bool listed = false;

            try
            {
                List<string> buckets = await _s3Service.ListBucketsAsync();

                foreach (string b in buckets)
                {
                    cmbBucket.Items.Add(b);
                }

                listed = true;
            }
            catch (Exception ex)
            {
                if (AwsSessionHelper.IsExpiredSessionError(ex))
                {
                    lblBucketInfo.Text = GetErrorMessage(ex);
                    return;
                }

                // uloga često ima dozvole samo nad jednim bucketom, bez s3:ListAllMyBuckets - tada se naziv upisuje ručno
                ActivityLogger.LogError("ListBuckets greska", "S3BrowserForm", ex.Message);
            }

            string suggestedBucket = GetSuggestedBucket();

            if (listed)
            {
                lblBucketInfo.Text = LanguageHelper.Format("s3_buckets_found", cmbBucket.Items.Count);

                // zadnji bucket se učitava samo ako je još na popisu
                if (cmbBucket.Items.Contains(suggestedBucket) == false)
                {
                    suggestedBucket = string.Empty;
                }
            }
            else
            {
                lblBucketInfo.Text = LanguageHelper.Get("s3_buckets_list_unavailable");
            }

            if (string.IsNullOrEmpty(suggestedBucket) == false)
            {
                cmbBucket.Text = suggestedBucket;
                await LoadObjectsAsync(suggestedBucket);
            }
            else
            {
                // nema prijedloga za automatsko učitavanje - LoadObjectsAsync (koji inače postavlja prazno stanje) se neće pozvati
                ApplyFilter();
            }
        }

        // prijedlog bucketa: zadani bucket profila, inače zadnji uspješno učitan bucket (registar)
        private string GetSuggestedBucket()
        {
            if (string.IsNullOrEmpty(_profile.DefaultBucket) == false)
            {
                return _profile.DefaultBucket;
            }

            return RegistryHelper.ReadString("LastBucket", "");
        }

        private string GetEnteredBucket()
        {
            return cmbBucket.Text.Trim();
        }

        private async void cmbBucket_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbBucket.SelectedItem == null)
            {
                return;
            }

            await LoadObjectsAsync(cmbBucket.SelectedItem.ToString());
        }

        private async void cmbBucket_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            await LoadObjectsAsync(GetEnteredBucket());
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadObjectsAsync(GetEnteredBucket());
        }

        private async Task LoadObjectsAsync(string bucket)
        {
            if (string.IsNullOrEmpty(bucket))
            {
                MessageBox.Show(LanguageHelper.Get("s3_bucket_required"));
                return;
            }

            btnRefresh.Enabled = false;
            lblBucketInfo.Text = LanguageHelper.Get("s3_status_loading_objects");

            try
            {
                string region = await _s3Service.GetBucketRegionAsync(bucket);
                List<AWSResource> objects = await _s3Service.ListObjectsAsync(bucket);

                _allObjects = objects;
                _loadedBucket = bucket;
                _loadedBucketRegion = region;

                RegistryHelper.WriteString("LastBucket", bucket);

                if (cmbBucket.Items.Contains(bucket) == false)
                {
                    cmbBucket.Items.Add(bucket);
                }

                ApplyFilter();
                lblBucketInfo.Text = BuildBucketInfoText();
            }
            catch (Exception ex)
            {
                _allObjects = new List<AWSResource>();
                _loadedBucket = string.Empty;
                _loadedBucketRegion = string.Empty;

                ApplyFilter();
                lblBucketInfo.Text = GetErrorMessage(ex);
            }
            finally
            {
                btnRefresh.Enabled = _isBusy == false;
            }
        }

        // "bucket · broj objekata · ukupna veličina · regija · arhivirani objekti"
        private string BuildBucketInfoText()
        {
            int objectCount = 0;
            int archivedCount = 0;
            long totalBytes = 0;

            foreach (AWSResource x in _allObjects)
            {
                if (x.IsFolderMarker())
                {
                    continue;
                }

                objectCount++;
                totalBytes += x.Size;

                if (x.IsArchived())
                {
                    archivedCount++;
                }
            }

            string text = LanguageHelper.Format("s3_bucket_summary", _loadedBucket, objectCount, FormatUtils.FormatFileSize(totalBytes));

            if (string.IsNullOrEmpty(_loadedBucketRegion))
            {
                text = text + LanguageHelper.Format("s3_region_unknown_suffix", _profile.Region);
            }
            else
            {
                text = text + LanguageHelper.Format("s3_region_suffix", _loadedBucketRegion);
            }

            if (archivedCount > 0)
            {
                text = text + LanguageHelper.Format("s3_archived_suffix", archivedCount);
            }

            return text;
        }

        private void ApplyFilter()
        {
            bool noBucketLoaded = string.IsNullOrEmpty(_loadedBucket);
            lstObjects.Visible = noBucketLoaded == false;
            lblObjectsEmpty.Visible = noBucketLoaded;

            if (noBucketLoaded)
            {
                return;
            }

            string filter = txtFilter.Text;

            lstObjects.SuspendLayout();
            lstObjects.Rows.Clear();

            foreach (AWSResource x in _allObjects)
            {
                if (string.IsNullOrEmpty(filter) == false && x.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                string sizeDisplay;
                string classDisplay;

                if (x.IsFolderMarker())
                {
                    sizeDisplay = "-";
                    classDisplay = LanguageHelper.Get("s3_folder_label");
                }
                else
                {
                    sizeDisplay = FormatUtils.FormatFileSize(x.Size);
                    classDisplay = GetStorageClassDisplayName(x.StorageClass);
                }

                string modifiedDisplay;

                if (x.CreatedAt == DateTime.MinValue)
                {
                    modifiedDisplay = "-";
                }
                else
                {
                    modifiedDisplay = x.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                }

                int rowIndex = lstObjects.Rows.Add(x.Name, sizeDisplay, classDisplay, modifiedDisplay);

                // cijeli objekt čuvam u Tag - radnje trebaju i veličinu i klasu pohrane, ne samo ključ
                lstObjects.Rows[rowIndex].Tag = x;
            }

            lstObjects.ResumeLayout();
        }

        // čitljiv naziv klase pohrane; nepoznata vrijednost se prikazuje kako je AWS vraća
        private string GetStorageClassDisplayName(string storageClass)
        {
            switch (storageClass)
            {
                case "STANDARD":
                    return "Standard";
                case "STANDARD_IA":
                    return "Standard-IA";
                case "ONEZONE_IA":
                    return "One Zone-IA";
                case "INTELLIGENT_TIERING":
                    return "Intelligent-Tiering";
                case "GLACIER_IR":
                    return "Glacier Instant Retrieval";
                case "GLACIER":
                    return "Glacier Flexible Retrieval";
                case "DEEP_ARCHIVE":
                    return "Glacier Deep Archive";
                case "REDUCED_REDUNDANCY":
                    return "Reduced Redundancy";
                case "EXPRESS_ONEZONE":
                    return "Express One Zone";
                default:
                    return storageClass;
            }
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private List<AWSResource> GetSelectedObjects()
        {
            List<AWSResource> objects = new List<AWSResource>();

            foreach (DataGridViewRow row in lstObjects.SelectedRows)
            {
                AWSResource resource = row.Tag as AWSResource;

                if (resource != null)
                {
                    objects.Add(resource);
                }
            }

            return objects;
        }

        // objekti u arhivskoj klasi nisu čitljivi dok se ne vrate iz arhive - korisnik ipak može nastaviti jer je objekt možda već vraćen
        private bool ConfirmArchivedObjects(List<AWSResource> objects, string actionText)
        {
            int archivedCount = 0;

            foreach (AWSResource x in objects)
            {
                if (x.IsArchived())
                {
                    archivedCount++;
                }
            }

            if (archivedCount == 0)
            {
                return true;
            }

            DialogResult confirm = MessageBox.Show(
                LanguageHelper.Format("s3_archived_confirm", archivedCount, actionText),
                LanguageHelper.Get("s3_archived_confirm_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            return confirm == DialogResult.Yes;
        }

        // ---------------------------------------------------------------
        // Presigned poveznica

        // popunjava opcije trajanja poveznice - ako je sesija privremena (AssumeRole), ne nudi trajanja duza od preostalog vremena sesije
        // (URL potpisan privremenim vjerodajnicama prestaje raditi kad te vjerodajnice isteknu, bez obzira na Expires parametar)
        private void PopulateUrlDurationOptions()
        {
            cmbUrlDuration.Items.Clear();
            _urlDurationOptions.Clear();

            List<(string LabelKey, TimeSpan Duration)> allOptions = new List<(string LabelKey, TimeSpan Duration)>
            {
                ("url_duration_15min", TimeSpan.FromMinutes(15)),
                ("url_duration_1h", TimeSpan.FromHours(1)),
                ("url_duration_24h", TimeSpan.FromHours(24)),
                ("url_duration_7d", TimeSpan.FromDays(7))
            };

            bool isTemporary = _profile.IsTemporarySession();
            TimeSpan remaining = TimeSpan.Zero;

            if (isTemporary)
            {
                remaining = _profile.GetRemainingSessionTime();
            }

            foreach ((string labelKey, TimeSpan duration) in allOptions)
            {
                if (isTemporary && duration > remaining)
                {
                    continue;
                }

                cmbUrlDuration.Items.Add(LanguageHelper.Get(labelKey));
                _urlDurationOptions.Add(duration);
            }

            if (cmbUrlDuration.Items.Count > 0)
            {
                cmbUrlDuration.SelectedIndex = 0;
            }
            else
            {
                // preostalo trajanje sesije je krace od najkrace ponudene opcije (15 min)
                lblUrlInfo.Text = LanguageHelper.Get("s3_url_session_too_short");
            }
        }

        private TimeSpan GetSelectedUrlDuration()
        {
            if (cmbUrlDuration.SelectedIndex < 0 || cmbUrlDuration.SelectedIndex >= _urlDurationOptions.Count)
            {
                return TimeSpan.Zero;
            }

            return _urlDurationOptions[cmbUrlDuration.SelectedIndex];
        }

        private async void btnGenerateUrl_Click(object sender, EventArgs e)
        {
            List<AWSResource> selected = GetSelectedObjects();

            if (string.IsNullOrEmpty(_loadedBucket) || selected.Count == 0)
            {
                MessageBox.Show(LanguageHelper.Get("s3_select_object"));
                return;
            }

            if (selected.Count > 1)
            {
                MessageBox.Show(LanguageHelper.Get("s3_url_single_object_only"));
                return;
            }

            AWSResource target = selected[0];

            if (target.IsFolderMarker())
            {
                MessageBox.Show(LanguageHelper.Get("s3_url_folder_no_content"));
                return;
            }

            if (ConfirmArchivedObjects(selected, LanguageHelper.Get("s3_action_share_link")) == false)
            {
                return;
            }

            TimeSpan duration = GetSelectedUrlDuration();

            // dodatna provjera neposredno prije generiranja - vrijeme je moglo isteci dok je forma bila otvorena
            if (_profile.IsTemporarySession())
            {
                TimeSpan remaining = _profile.GetRemainingSessionTime();

                if (duration > remaining)
                {
                    MessageBox.Show(LanguageHelper.Get("s3_url_duration_exceeds_session"));
                    return;
                }
            }

            string bucket = _loadedBucket;
            string key = target.Name;

            btnGenerateUrl.Enabled = false;
            txtUrl.Clear();
            lblUrlInfo.Text = LanguageHelper.Get("s3_url_generating");

            try
            {
                // poveznica za nepostojeći objekt bi se generirala bez greške, ali bi vraćala 404
                bool exists = await _s3Service.ObjectExistsAsync(bucket, key);

                if (exists == false)
                {
                    lblUrlInfo.Text = LanguageHelper.Get("s3_url_object_gone");
                    return;
                }

                string url = await _s3Service.GetPresignedUrlAsync(bucket, key, duration);

                txtUrl.Text = url;

                string validUntil = DateTime.Now.Add(duration).ToString("dd.MM.yyyy HH:mm");

                try
                {
                    Clipboard.SetText(url);
                    lblUrlInfo.Text = LanguageHelper.Format("s3_url_copied", key, validUntil);
                }
                catch (ExternalException)
                {
                    // clipboard može biti zauzet drugim programom - poveznica je svejedno u polju
                    lblUrlInfo.Text = LanguageHelper.Format("s3_url_manual_copy", key, validUntil);
                }

                ActivityLogger.Log("PresignedUrl", "S3BrowserForm", key + " iz " + bucket + " (trajanje: " + cmbUrlDuration.SelectedItem + ")");
            }
            catch (Exception ex)
            {
                lblUrlInfo.Text = GetErrorMessage(ex);
                ActivityLogger.LogError("PresignedUrl greska", "S3BrowserForm", key + ": " + ex.Message);
            }
            finally
            {
                btnGenerateUrl.Enabled = _isBusy == false && _urlDurationOptions.Count > 0;
            }
        }

        // ručno ponovno kopiranje (npr. ako je korisnik u međuvremenu kopirao nešto drugo) - poveznica se već kopira automatski kod generiranja
        private void btnCopyUrl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUrl.Text))
            {
                return;
            }

            try
            {
                Clipboard.SetText(txtUrl.Text);
                lblUrlInfo.Text = LanguageHelper.Get("s3_url_copied_again");
            }
            catch (ExternalException ex)
            {
                Debug.WriteLine("Greška kod kopiranja poveznice: " + ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // Zajedničko za prijenose

        private void numConcurrency_ValueChanged(object sender, EventArgs e)
        {
            UpdateConcurrencyHint();
            UpdateAdvancedButtonText();
        }

        // napredne postavke prijenosa su skrivene - običnom korisniku zadane vrijednosti odgovaraju
        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            SetAdvancedVisible(pnlAdvanced.Visible == false);
        }

        // lista prijenosa zauzima prostor skrivenog panela; donji rub liste ostaje na mjestu (prati promjenu veličine prozora)
        private void SetAdvancedVisible(bool visible)
        {
            int listBottom = lstTransfers.Bottom;

            pnlAdvanced.Visible = visible;

            int listTop = pnlAdvanced.Top;

            if (visible)
            {
                listTop = pnlAdvanced.Bottom + 8;
            }

            lstTransfers.Top = listTop;
            lstTransfers.Height = listBottom - listTop;

            UpdateAdvancedButtonText();
        }

        // oznaka "(prilagođeno)" upozorava da vrijede postavke različite od zadanih i kad je panel skriven
        private void UpdateAdvancedButtonText()
        {
            bool customized = numPartSize.Value != DefaultPartSizeMB
                || numFileConcurrency.Value != DefaultFileConcurrency
                || numPartConcurrency.Value != DefaultPartConcurrency;

            string text;

            if (customized)
            {
                text = LanguageHelper.Get("s3_advanced_settings_customized");
            }
            else
            {
                text = LanguageHelper.Get("s3_advanced_settings");
            }

            if (pnlAdvanced.Visible)
            {
                text = text + " ▾";
            }
            else
            {
                text = text + " ▸";
            }

            btnAdvanced.Text = text;
        }

        // ukupan broj veza prema S3 = datoteke istovremeno × zahtjevi za dijelove po datoteci
        // (zahtjevi za dijelove vrijede za multipart upload, tj. datoteke od 16 MB naviše)
        private void UpdateConcurrencyHint()
        {
            int files = (int)numFileConcurrency.Value;
            int partRequests = (int)numPartConcurrency.Value;

            if (partRequests == 0)
            {
                partRequests = SdkDefaultPartRequests;
            }

            lblConcurrencyHint.Text = LanguageHelper.Format("s3_concurrency_hint", files * partRequests, files, partRequests);
        }

        // bilježenje u TransferHistory ne smije srušiti prijenos koji je već uspio
        // bytesTransferred je stvarno preneseno (0 za neuspjele i otkazane), fileSizeBytes veličina koja se prenosila
        private void LogTransfer(string direction, string bucket, string key, string localPath, long fileSizeBytes, long bytesTransferred, double? durationMs, string status, string errorMessage)
        {
            try
            {
                TransferModels transfer = new TransferModels();
                transfer.Username = AppSettings.Instance.LastUser;
                transfer.ProfileName = _profile.ProfileName;
                transfer.AwsIdentity = _profile.AwsArn;
                transfer.Direction = direction;
                transfer.Bucket = bucket;
                transfer.ObjectKey = key;
                transfer.LocalPath = localPath;
                transfer.FileSizeBytes = fileSizeBytes;
                transfer.BytesTransferred = bytesTransferred;
                transfer.DurationMs = durationMs;
                transfer.Status = status;
                transfer.ErrorMessage = errorMessage;

                DatabaseManager.Instance.SaveTransfer(transfer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod spremanja transfera: " + ex.Message);
                ActivityLogger.LogError("TransferHistory greska", "S3BrowserForm", ex.Message);
            }
        }

        // trajanje prijenosa; null ako prijenos nije ni počeo (otkazan dok je čekao u redu)
        private double? GetElapsedMs(Stopwatch stopwatch)
        {
            if (stopwatch == null)
            {
                return null;
            }

            return stopwatch.Elapsed.TotalMilliseconds;
        }

        // vraća prvi naziv koji se ponavlja ili null
        private string FindDuplicateName(List<string> names, StringComparer comparer)
        {
            HashSet<string> seen = new HashSet<string>(comparer);

            foreach (string name in names)
            {
                if (seen.Add(name) == false)
                {
                    return name;
                }
            }

            return null;
        }

        private string BuildNameList(List<string> names)
        {
            StringBuilder text = new StringBuilder();

            int shown = Math.Min(names.Count, MaxNamesInDialog);

            for (int i = 0; i < shown; i++)
            {
                text.AppendLine("  • " + names[i]);
            }

            if (names.Count > shown)
            {
                text.AppendLine(LanguageHelper.Format("s3_names_more", names.Count - shown));
            }

            return text.ToString();
        }

        // pita korisnika što s već postojećim datotekama/objektima: Da = prepiši, Ne = preskoči, Odustani = prekini
        // vraća false ako je korisnik odustao; skip[i] = true za preskočene
        private bool AskOverwrite(List<string> existingNames, List<int> existingIndexes, bool[] skip, string whereText)
        {
            if (existingIndexes.Count == 0)
            {
                return true;
            }

            DialogResult choice = MessageBox.Show(
                LanguageHelper.Format("s3_overwrite_confirm", whereText, existingIndexes.Count, BuildNameList(existingNames)),
                LanguageHelper.Get("s3_overwrite_title"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (choice == DialogResult.Cancel)
            {
                return false;
            }

            if (choice == DialogResult.No)
            {
                foreach (int index in existingIndexes)
                {
                    skip[index] = true;
                }
            }

            return true;
        }

        // priprema listu prijenosa i polja napretka za novu skupinu
        private void PrepareTransferList(List<string> names, long[] sizes, bool[] skip)
        {
            int count = names.Count;

            _transferBatchId++;
            _transferredBytes = new long[count];
            _totalBytes = new long[count];
            _transferFinished = new bool[count];

            lstTransfers.SuspendLayout();
            lstTransfers.Rows.Clear();

            for (int i = 0; i < count; i++)
            {
                string status;

                if (skip[i])
                {
                    // preskočene datoteke ne ulaze u ukupni napredak
                    status = LanguageHelper.Get("s3_transfer_status_skipped");
                    _transferFinished[i] = true;
                }
                else
                {
                    status = LanguageHelper.Get("s3_transfer_status_queued");
                    _totalBytes[i] = sizes[i];
                }

                lstTransfers.Rows.Add(names[i], FormatUtils.FormatFileSize(sizes[i]), "0 %", status);
            }

            lstTransfers.ResumeLayout();

            progressBar1.Value = 0;
        }

        private void SetTransferStatus(int index, string status)
        {
            lstTransfers.Rows[index].Cells[ColTransferStatus].Value = status;
        }

        // Progress<T> je stvoren na UI dretvi, pa se ova metoda izvodi na UI dretvi
        private void OnTransferProgress(int batchId, int index, TransferProgress progress)
        {
            // poruka iz prethodne skupine prijenosa
            if (batchId != _transferBatchId)
            {
                return;
            }

            // kasna poruka o napretku ne smije prepisati završni status
            if (_transferFinished[index])
            {
                return;
            }

            _transferredBytes[index] = progress.TransferredBytes;
            lstTransfers.Rows[index].Cells[ColTransferPercent].Value = progress.GetPercent() + " %";

            UpdateOverallProgress();
        }

        private void FinishTransfer(int index, bool completed, string status)
        {
            _transferFinished[index] = true;

            if (completed)
            {
                _transferredBytes[index] = _totalBytes[index];
                lstTransfers.Rows[index].Cells[ColTransferPercent].Value = "100 %";
            }

            SetTransferStatus(index, status);
            UpdateOverallProgress();
        }

        // ukupni napredak po bajtovima, ne po broju datoteka - velika datoteka nosi proporcionalno više
        private void UpdateOverallProgress()
        {
            long total = 0;
            long done = 0;

            for (int i = 0; i < _totalBytes.Length; i++)
            {
                total += _totalBytes[i];
                done += Math.Min(_transferredBytes[i], _totalBytes[i]);
            }

            int percent = 0;

            if (total > 0)
            {
                percent = (int)(done * 100 / total);
            }

            if (percent > 100)
            {
                percent = 100;
            }

            progressBar1.Value = percent;
        }

        // jedan sažetak na kraju skupine; poruka se prikazuje samo ako nešto nije uspjelo
        private void ShowTransferSummary(string operation, TransferOutcome[] outcomes, TimeSpan elapsed)
        {
            int completed = 0;
            int failed = 0;
            int cancelled = 0;
            int skipped = 0;

            foreach (TransferOutcome outcome in outcomes)
            {
                switch (outcome)
                {
                    case TransferOutcome.Completed:
                        completed++;
                        break;
                    case TransferOutcome.Failed:
                        failed++;
                        break;
                    case TransferOutcome.Cancelled:
                        cancelled++;
                        break;
                    case TransferOutcome.Skipped:
                        skipped++;
                        break;
                }
            }

            string text = LanguageHelper.Format("s3_transfer_summary", operation, completed, failed, cancelled, skipped, FormatUtils.FormatDuration(elapsed.TotalSeconds));

            lblTransferSummary.Text = text;

            if (failed == 0 && cancelled == 0 && completed > 0)
            {
                progressBar1.Value = 100;
            }

            if (failed > 0)
            {
                MessageBox.Show(text + Environment.NewLine + Environment.NewLine + LanguageHelper.Get("s3_transfer_failure_reason_hint"), operation, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StartCancellation()
        {
            if (_cancellation != null)
            {
                _cancellation.Dispose();
            }

            _cancellation = new CancellationTokenSource();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cancellation != null && _cancellation.IsCancellationRequested == false)
            {
                _cancellation.Cancel();
                btnCancel.Enabled = false;
                lblTransferSummary.Text = LanguageHelper.Get("s3_cancelling");
            }
        }

        // ---------------------------------------------------------------
        // Upload

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_loadedBucket))
            {
                MessageBox.Show(LanguageHelper.Get("s3_bucket_not_loaded"));
                return;
            }

            string[] paths;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                paths = dialog.FileNames;
            }

            string bucket = _loadedBucket;
            int partSizeMB = (int)numPartSize.Value;
            int fileConcurrency = (int)numFileConcurrency.Value;
            int partConcurrency = (int)numPartConcurrency.Value;

            List<string> keys = new List<string>();
            long[] sizes = new long[paths.Length];

            try
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    keys.Add(Path.GetFileName(paths[i]));
                    sizes[i] = new FileInfo(paths[i]).Length;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageHelper.Format("s3_read_files_error", ex.Message));
                return;
            }

            if (ValidatePartSize(keys, sizes, partSizeMB) == false)
            {
                return;
            }

            // ključ je naziv datoteke - dvije datoteke istog naziva iz različitih mapa završile bi na istom ključu
            // S3 ključevi razlikuju velika i mala slova, pa se uspoređuje točno
            string duplicateKey = FindDuplicateName(keys, StringComparer.Ordinal);

            if (duplicateKey != null)
            {
                MessageBox.Show(LanguageHelper.Format("s3_duplicate_upload_name", duplicateKey));
                return;
            }

            SetBusy(true, false);
            lblTransferSummary.Text = LanguageHelper.Get("s3_checking_existing");

            try
            {
                bool[] skip = new bool[paths.Length];

                bool proceed = await ResolveUploadConflictsAsync(bucket, keys, skip);

                if (proceed == false)
                {
                    lblTransferSummary.Text = LanguageHelper.Get("s3_upload_aborted_before_start");
                    return;
                }

                PrepareTransferList(keys, sizes, skip);
                StartCancellation();
                SetBusy(true, true);

                CancellationToken token = _cancellation.Token;
                SemaphoreSlim limiter = new SemaphoreSlim(fileConcurrency);
                List<Task<TransferOutcome>> tasks = new List<Task<TransferOutcome>>();
                Stopwatch stopwatch = Stopwatch.StartNew();

                lblTransferSummary.Text = LanguageHelper.Get("s3_upload_in_progress");

                for (int i = 0; i < paths.Length; i++)
                {
                    if (skip[i])
                    {
                        tasks.Add(Task.FromResult(TransferOutcome.Skipped));
                    }
                    else
                    {
                        tasks.Add(UploadOneFileAsync(i, paths[i], bucket, keys[i], limiter, partSizeMB, partConcurrency, token));
                    }
                }

                TransferOutcome[] outcomes = await Task.WhenAll(tasks);
                stopwatch.Stop();

                ShowTransferSummary(LanguageHelper.Get("upload"), outcomes, stopwatch.Elapsed);

                await LoadObjectsAsync(bucket);
            }
            finally
            {
                SetBusy(false, false);
            }
        }

        // S3: dio najmanje 5 MB (osim zadnjeg), najviše 10.000 dijelova po datoteci
        private bool ValidatePartSize(List<string> names, long[] sizes, int partSizeMB)
        {
            if (partSizeMB == 0)
            {
                return true;
            }

            if (partSizeMB < MinPartSizeMB)
            {
                MessageBox.Show(LanguageHelper.Format("s3_part_size_too_small", MinPartSizeMB));
                return false;
            }

            long partBytes = partSizeMB * 1024L * 1024L;

            for (int i = 0; i < sizes.Length; i++)
            {
                if (sizes[i] > partBytes * MaxPartCount)
                {
                    long minPartMB = (sizes[i] / MaxPartCount / (1024L * 1024L)) + 1;

                    MessageBox.Show(LanguageHelper.Format("s3_part_size_file_too_large", names[i], FormatUtils.FormatFileSize(sizes[i]), partSizeMB, MaxPartCount, minPartMB));
                    return false;
                }
            }

            return true;
        }

        // provjerava postoji li već objekt za svaki ključ; ako provjera ne uspije, korisnik bira nastavlja li bez nje
        private async Task<bool> ResolveUploadConflictsAsync(string bucket, List<string> keys, bool[] skip)
        {
            List<int> existingIndexes = new List<int>();
            List<string> existingNames = new List<string>();

            try
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    bool exists = await _s3Service.ObjectExistsAsync(bucket, keys[i]);

                    if (exists)
                    {
                        existingIndexes.Add(i);
                        existingNames.Add(keys[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                if (AwsSessionHelper.IsExpiredSessionError(ex))
                {
                    MessageBox.Show(GetErrorMessage(ex));
                    return false;
                }

                DialogResult confirm = MessageBox.Show(
                    LanguageHelper.Format("s3_conflict_check_failed", ex.Message),
                    LanguageHelper.Get("common_warning_title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                return confirm == DialogResult.Yes;
            }

            return AskOverwrite(existingNames, existingIndexes, skip, LanguageHelper.Get("s3_where_bucket"));
        }

        // izvodi se na UI dretvi (poziva se izravno iz btnUpload_Click, bez Task.Run) - SDK posao je asinkron,
        // pa UI ostaje responzivan, a lista, brojači i log se ažuriraju bez zaključavanja i Invoke poziva
        private async Task<TransferOutcome> UploadOneFileAsync(int index, string path, string bucket, string key, SemaphoreSlim limiter, int partSizeMB, int partConcurrency, CancellationToken token)
        {
            int batchId = _transferBatchId;
            long fileSize = _totalBytes[index];
            bool acquired = false;
            Stopwatch stopwatch = null;

            try
            {
                await limiter.WaitAsync(token);
                acquired = true;

                SetTransferStatus(index, LanguageHelper.Get("s3_transfer_status_in_progress"));

                // trajanje se mjeri od stvarnog početka prijenosa, bez čekanja u redu
                stopwatch = Stopwatch.StartNew();

                Progress<TransferProgress> progress = new Progress<TransferProgress>(p => OnTransferProgress(batchId, index, p));

                await _s3Service.UploadFileAsync(path, bucket, key, token, partSizeMB, partConcurrency, progress);

                stopwatch.Stop();

                FinishTransfer(index, true, LanguageHelper.Get("s3_transfer_status_completed"));
                LogTransfer("upload", bucket, key, path, fileSize, fileSize, GetElapsedMs(stopwatch), StatusCompleted, string.Empty);

                return TransferOutcome.Completed;
            }
            catch (Exception ex)
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                }

                // SDK kod otkazivanja ne baca uvijek OperationCanceledException, pa se provjerava i sam token
                if (ex is OperationCanceledException || token.IsCancellationRequested)
                {
                    FinishTransfer(index, false, LanguageHelper.Get("s3_transfer_status_cancelled"));
                    LogTransfer("upload", bucket, key, path, fileSize, 0, GetElapsedMs(stopwatch), StatusCancelled, string.Empty);
                    ActivityLogger.LogError("Upload prekinut", "S3BrowserForm", key);

                    return TransferOutcome.Cancelled;
                }

                string errorMessage = GetErrorMessage(ex);

                FinishTransfer(index, false, errorMessage);
                LogTransfer("upload", bucket, key, path, fileSize, 0, GetElapsedMs(stopwatch), StatusFailed, errorMessage);
                ActivityLogger.LogError("Upload greska", "S3BrowserForm", key + ": " + ex.Message);

                return TransferOutcome.Failed;
            }
            finally
            {
                if (acquired)
                {
                    limiter.Release();
                }
            }
        }

        // ---------------------------------------------------------------
        // Download

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            List<AWSResource> selected = GetSelectedObjects();

            if (string.IsNullOrEmpty(_loadedBucket) || selected.Count == 0)
            {
                MessageBox.Show(LanguageHelper.Get("s3_select_objects"));
                return;
            }

            // mape nemaju sadržaj za preuzimanje
            List<AWSResource> files = new List<AWSResource>();

            foreach (AWSResource x in selected)
            {
                if (x.IsFolderMarker() == false)
                {
                    files.Add(x);
                }
            }

            if (files.Count == 0)
            {
                MessageBox.Show(LanguageHelper.Get("s3_only_folders_selected"));
                return;
            }

            if (ConfirmArchivedObjects(files, LanguageHelper.Get("s3_action_download")) == false)
            {
                return;
            }

            string[] targets = new string[files.Count];
            bool[] skip = new bool[files.Count];

            if (files.Count == 1)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    // SaveFileDialog sam pita za prepisivanje postojeće datoteke (OverwritePrompt)
                    dialog.FileName = Path.GetFileName(files[0].Name);

                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    targets[0] = dialog.FileName;
                }
            }
            else
            {
                string folder;

                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    folder = dialog.SelectedPath;
                }

                List<string> localNames = new List<string>();

                foreach (AWSResource x in files)
                {
                    localNames.Add(Path.GetFileName(x.Name));
                }

                // "mapa1/x.txt" i "mapa2/x.txt" bi se preuzeli u istu datoteku; Windows ne razlikuje velika i mala slova
                string duplicateName = FindDuplicateName(localNames, StringComparer.OrdinalIgnoreCase);

                if (duplicateName != null)
                {
                    MessageBox.Show(LanguageHelper.Format("s3_duplicate_download_name", duplicateName));
                    return;
                }

                List<int> existingIndexes = new List<int>();
                List<string> existingNames = new List<string>();

                for (int i = 0; i < files.Count; i++)
                {
                    targets[i] = Path.Combine(folder, localNames[i]);

                    if (File.Exists(targets[i]))
                    {
                        existingIndexes.Add(i);
                        existingNames.Add(localNames[i]);
                    }
                }

                if (AskOverwrite(existingNames, existingIndexes, skip, LanguageHelper.Get("s3_where_folder")) == false)
                {
                    return;
                }
            }

            List<string> names = new List<string>();
            long[] sizes = new long[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                names.Add(files[i].Name);
                sizes[i] = files[i].Size;
            }

            string bucket = _loadedBucket;
            int fileConcurrency = (int)numFileConcurrency.Value;

            PrepareTransferList(names, sizes, skip);
            StartCancellation();
            SetBusy(true, true);

            try
            {
                CancellationToken token = _cancellation.Token;
                SemaphoreSlim limiter = new SemaphoreSlim(fileConcurrency);
                List<Task<TransferOutcome>> tasks = new List<Task<TransferOutcome>>();
                Stopwatch stopwatch = Stopwatch.StartNew();

                lblTransferSummary.Text = LanguageHelper.Get("s3_download_in_progress");

                for (int i = 0; i < files.Count; i++)
                {
                    if (skip[i])
                    {
                        tasks.Add(Task.FromResult(TransferOutcome.Skipped));
                    }
                    else
                    {
                        tasks.Add(DownloadOneFileAsync(i, bucket, files[i].Name, targets[i], limiter, token));
                    }
                }

                TransferOutcome[] outcomes = await Task.WhenAll(tasks);
                stopwatch.Stop();

                ShowTransferSummary(LanguageHelper.Get("download"), outcomes, stopwatch.Elapsed);
            }
            finally
            {
                SetBusy(false, false);
            }
        }

        private async Task<TransferOutcome> DownloadOneFileAsync(int index, string bucket, string key, string target, SemaphoreSlim limiter, CancellationToken token)
        {
            int batchId = _transferBatchId;
            long expectedSize = _totalBytes[index];
            bool acquired = false;
            Stopwatch stopwatch = null;

            try
            {
                await limiter.WaitAsync(token);
                acquired = true;

                SetTransferStatus(index, LanguageHelper.Get("s3_transfer_status_in_progress"));

                stopwatch = Stopwatch.StartNew();

                Progress<TransferProgress> progress = new Progress<TransferProgress>(p => OnTransferProgress(batchId, index, p));

                await _s3Service.DownloadFileAsync(bucket, key, target, token, progress);

                stopwatch.Stop();

                long fileSize = new FileInfo(target).Length;

                FinishTransfer(index, true, LanguageHelper.Get("s3_transfer_status_completed"));
                LogTransfer("download", bucket, key, target, fileSize, fileSize, GetElapsedMs(stopwatch), StatusCompleted, string.Empty);

                return TransferOutcome.Completed;
            }
            catch (Exception ex)
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                }

                if (ex is OperationCanceledException || token.IsCancellationRequested)
                {
                    FinishTransfer(index, false, LanguageHelper.Get("s3_transfer_status_cancelled"));
                    LogTransfer("download", bucket, key, target, expectedSize, 0, GetElapsedMs(stopwatch), StatusCancelled, string.Empty);
                    ActivityLogger.LogError("Download prekinut", "S3BrowserForm", key);

                    return TransferOutcome.Cancelled;
                }

                string errorMessage = GetErrorMessage(ex);

                FinishTransfer(index, false, errorMessage);
                LogTransfer("download", bucket, key, target, expectedSize, 0, GetElapsedMs(stopwatch), StatusFailed, errorMessage);
                ActivityLogger.LogError("Download greska", "S3BrowserForm", key + ": " + ex.Message);

                return TransferOutcome.Failed;
            }
            finally
            {
                if (acquired)
                {
                    limiter.Release();
                }
            }
        }

        // ---------------------------------------------------------------
        // Delete

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            List<AWSResource> selected = GetSelectedObjects();

            if (string.IsNullOrEmpty(_loadedBucket) || selected.Count == 0)
            {
                MessageBox.Show(LanguageHelper.Get("s3_select_objects"));
                return;
            }

            string bucket = _loadedBucket;
            string confirmText;

            if (selected.Count == 1)
            {
                confirmText = LanguageHelper.Format("s3_delete_confirm_single", selected[0].Name, bucket);
            }
            else
            {
                confirmText = LanguageHelper.Format("s3_delete_confirm_multiple", selected.Count, bucket);
            }

            DialogResult confirm = MessageBox.Show(
                confirmText,
                LanguageHelper.Get("common_confirm_delete_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            SetBusy(true, false);

            int successCount = 0;

            try
            {
                foreach (AWSResource x in selected)
                {
                    try
                    {
                        await _s3Service.DeleteObjectAsync(bucket, x.Name);
                        ActivityLogger.Log("Delete", "S3BrowserForm", x.Name + " obrisano iz " + bucket);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        ActivityLogger.LogError("Delete greska", "S3BrowserForm", x.Name + ": " + ex.Message);
                    }
                }

                if (successCount < selected.Count)
                {
                    MessageBox.Show(LanguageHelper.Format("s3_delete_partial_success", successCount, selected.Count));
                }

                await LoadObjectsAsync(bucket);
            }
            finally
            {
                SetBusy(false, false);
            }
        }
    }
}