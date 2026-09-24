using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AWSGateway.Forms
{
    // upravljanje spremljenim AWS profilima - otvara se samo iz glavnog prozora, nakon prijave
    public partial class ProfileManagerForm : Form, IAppModule
    {
        private XmlProfileManager _xmlManager;

        // prijavljeni korisnik aplikacije - vidi i mijenja samo svoje profile i starije profile bez vlasnika
        private string _currentUser;

        // 0 = uređuje se novi profil
        private int _editingProfileId = 0;

        // profil koji treba odabrati kod otvaranja (profil trenutne sesije)
        private int _initialProfileId;

        // sprječava da punjenje liste pokrene učitavanje profila u editor
        private bool _isLoadingList = false;

        // ID zadnjeg spremljenog profila (0 ako ništa nije spremljeno)
        public int LastSavedProfileId { get; private set; }

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
            UiTheme.Apply(this, btnSave);
            UiTheme.StyleDangerButton(btnDelete);

            lblTitle.Font = UiTheme.Fonts.Title;
        }

        public ProfileManagerForm() : this(0)
        {
        }

        public ProfileManagerForm(int initialProfileId)
        {
            InitializeComponent();

            _xmlManager = new XmlProfileManager();
            _currentUser = AppSettings.Instance.LastUser;
            _initialProfileId = initialProfileId;
            LastSavedProfileId = 0;

            AwsRegions.FillComboBox(cmbRegion, AwsRegions.DefaultRegion);

            ApplyLanguage();
            ApplyShellTheme();
        }

        private void ApplyLanguage()
        {
            string title = LanguageHelper.Get("data_files_form_title");

            this.Text = title;
            lblTitle.Text = title;
            lblSubtitle.Text = LanguageHelper.Get("data_files_subtitle_hint");
            colName.HeaderText = LanguageHelper.Get("data_files_col_name");
            colMode.HeaderText = LanguageHelper.Get("data_files_col_mode");
            colRegion.HeaderText = LanguageHelper.Get("data_files_col_region");
            colAccessKey.HeaderText = LanguageHelper.Get("data_files_col_access_key");
            colBucket.HeaderText = LanguageHelper.Get("data_files_col_bucket");
            colOwner.HeaderText = LanguageHelper.Get("data_files_col_owner");
            btnNew.Text = LanguageHelper.Get("data_files_btn_new");
            btnDelete.Text = LanguageHelper.Get("delete");
            txtAccessKey.PlaceholderText = LanguageHelper.Get("data_files_access_key_placeholder");
            txtRoleArn.PlaceholderText = LanguageHelper.Get("data_files_role_arn_placeholder");
            txtMfaSerial.PlaceholderText = LanguageHelper.Get("data_files_mfa_placeholder");
            txtDefaultBucket.PlaceholderText = LanguageHelper.Get("common_optional_placeholder");
            lblName.Text = LanguageHelper.Get("data_files_lbl_name");
            lblMode.Text = LanguageHelper.Get("data_files_lbl_mode");
            rbAccessKey.Text = LanguageHelper.Get("data_files_mode_access_key");
            rbAssumeRole.Text = LanguageHelper.Get("data_files_mode_assume_role");
            lblAccessKey.Text = LanguageHelper.Get("access_key");
            lblRegion.Text = LanguageHelper.Get("region");
            lblRoleArn.Text = LanguageHelper.Get("data_files_lbl_role_arn");
            lblMfaSerial.Text = LanguageHelper.Get("data_files_lbl_mfa_serial");
            lblDefaultBucket.Text = LanguageHelper.Get("data_files_lbl_default_bucket");
            btnSave.Text = LanguageHelper.Get("save");
            btnCancelEdit.Text = LanguageHelper.Get("cancel");
            btnClose.Text = LanguageHelper.Get("common_close");

            // naslov editora ovisi o tome uređuje li se postojeći profil ili novi - obnovi ga u novom jeziku
            if (_editingProfileId == 0)
            {
                grpEdit.Text = LanguageHelper.Get("data_files_new_group_title");
            }
            else
            {
                AWSProfile selected = GetSelectedProfile();

                if (selected != null)
                {
                    grpEdit.Text = LanguageHelper.Format("data_files_edit_group_title", selected.ProfileName);
                }
            }

            RefreshProfileListLanguage();
        }

        // osvježava prijevod već učitanih redaka popisa (način prijave, regija, "bez vlasnika") kod promjene jezika -
        // ne poziva LoadProfiles jer bi to resetiralo/obrisalo eventualno nespremljene podatke u editoru
        private void RefreshProfileListLanguage()
        {
            string selectedId = string.Empty;

            if (lstProfiles.SelectedRows.Count > 0)
            {
                AWSProfile selectedProfile = lstProfiles.SelectedRows[0].Tag as AWSProfile;

                if (selectedProfile != null)
                {
                    selectedId = selectedProfile.ProfileId.ToString();
                }
            }

            _isLoadingList = true;

            foreach (DataGridViewRow row in lstProfiles.Rows)
            {
                AWSProfile profile = row.Tag as AWSProfile;

                if (profile == null)
                {
                    continue;
                }

                row.Cells[1].Value = profile.GetLoginModeDisplay();
                row.Cells[2].Value = GetRegionDisplay(profile.Region);

                if (profile.HasOwner())
                {
                    row.Cells[5].Value = LanguageHelper.Get("data_files_owner_you");
                }
                else
                {
                    row.Cells[5].Value = LanguageHelper.Get("data_files_owner_none");
                }

                row.Selected = profile.ProfileId.ToString() == selectedId;
            }

            _isLoadingList = false;
        }

        private void ProfileManagerForm_Load(object sender, EventArgs e)
        {
            if (TopLevel == false)
            {
                // ugrađen u ljusku - natrag se ide bočnom navigacijom, gumb Zatvori nema značenje
                int closeRight = btnClose.Right;
                btnClose.Visible = false;
                lblStatus.Width = closeRight - lblStatus.Left;
            }

            LoadProfiles(_initialProfileId);
        }

        // ---------------------------------------------------------------
        // Oštećena datoteka

        // nudi premještanje oštećene datoteke u sigurnosnu kopiju; true ako je datoteka premještena i popis se može ponovno učitati
        private static bool OfferCorruptedFileRecovery(XmlProfileManager manager, InvalidDataException ex)
        {
            DialogResult choice = MessageBox.Show(
                LanguageHelper.Format("data_files_corrupt_confirm", ex.Message),
                LanguageHelper.Get("data_files_corrupt_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (choice != DialogResult.Yes)
            {
                return false;
            }

            try
            {
                string backupPath = manager.BackupCorruptedFile();
                ActivityLogger.LogError("Oštećeni profili", "ProfileManagerForm", "Datoteka premještena u " + backupPath);
                MessageBox.Show(LanguageHelper.Format("data_files_corrupt_backup_done", backupPath));
                return true;
            }
            catch (Exception backupEx)
            {
                MessageBox.Show(LanguageHelper.Format("data_files_corrupt_backup_error", backupEx.Message));
                return false;
            }
        }

        // ---------------------------------------------------------------
        // Popis profila

        private void LoadProfiles(int selectProfileId)
        {
            List<AWSProfile> profiles;

            try
            {
                profiles = _xmlManager.GetForUser(_currentUser);
            }
            catch (InvalidDataException ex)
            {
                if (OfferCorruptedFileRecovery(_xmlManager, ex))
                {
                    LoadProfiles(0);
                }
                else
                {
                    lblStatus.Text = LanguageHelper.Get("data_files_status_load_failed_corrupt");
                    SetEditorEnabled(false);
                }

                return;
            }
            catch (Exception ex)
            {
                lblStatus.Text = LanguageHelper.Format("data_files_status_load_error", ex.Message);
                return;
            }

            _isLoadingList = true;

            lstProfiles.SuspendLayout();
            lstProfiles.Rows.Clear();

            DataGridViewRow rowToSelect = null;

            foreach (AWSProfile profile in profiles)
            {
                string bucketDisplay;

                if (string.IsNullOrEmpty(profile.DefaultBucket))
                {
                    bucketDisplay = "-";
                }
                else
                {
                    bucketDisplay = profile.DefaultBucket;
                }

                string ownerDisplay;

                // ovaj popis prikazuje samo profile prijavljenog korisnika i starije profile bez vlasnika (_xmlManager.GetForUser),
                // pa "ima vlasnika" ovdje uvijek znači "moj profil" - "Vi" je jasnije od ispisivanja vlastitog korisničkog imena
                if (profile.HasOwner())
                {
                    ownerDisplay = LanguageHelper.Get("data_files_owner_you");
                }
                else
                {
                    ownerDisplay = LanguageHelper.Get("data_files_owner_none");
                }

                int rowIndex = lstProfiles.Rows.Add(profile.ProfileName, profile.GetLoginModeDisplay(), GetRegionDisplay(profile.Region), profile.GetMaskedAccessKey(), bucketDisplay, ownerDisplay);
                DataGridViewRow row = lstProfiles.Rows[rowIndex];

                // cijeli profil u Tag - bez parsiranja teksta retka
                row.Tag = profile;

                if (profile.ProfileId == selectProfileId)
                {
                    rowToSelect = row;
                }
            }

            lstProfiles.ResumeLayout();

            _isLoadingList = false;

            SetEditorEnabled(true);

            if (rowToSelect != null)
            {
                rowToSelect.Selected = true;
                lstProfiles.FirstDisplayedScrollingRowIndex = rowToSelect.Index;
                ShowProfileInEditor((AWSProfile)rowToSelect.Tag);
            }
            else
            {
                StartNewProfile();
            }

            if (profiles.Count == 0)
            {
                lblStatus.Text = LanguageHelper.Get("data_files_status_empty");
            }
        }

        private string GetRegionDisplay(string code)
        {
            AwsRegionInfo region = AwsRegions.Find(code);

            if (region == null)
            {
                return code;
            }

            return region.ToString();
        }

        private AWSProfile GetSelectedProfile()
        {
            if (lstProfiles.SelectedRows.Count == 0)
            {
                return null;
            }

            return lstProfiles.SelectedRows[0].Tag as AWSProfile;
        }

        private void lstProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoadingList)
            {
                return;
            }

            AWSProfile profile = GetSelectedProfile();

            // odznačavanje (klik u prazno) ne briše ono što je u editoru
            if (profile == null)
            {
                btnDelete.Enabled = false;
                return;
            }

            ShowProfileInEditor(profile);
        }

        // ---------------------------------------------------------------
        // Editor

        private void SetEditorEnabled(bool enabled)
        {
            grpEdit.Enabled = enabled;
            btnNew.Enabled = enabled;
        }

        private void ShowProfileInEditor(AWSProfile profile)
        {
            _editingProfileId = profile.ProfileId;

            grpEdit.Text = LanguageHelper.Format("data_files_edit_group_title", profile.ProfileName);
            txtName.Text = profile.ProfileName;
            txtAccessKey.Text = profile.AccessKey;
            AwsRegions.SelectRegion(cmbRegion, profile.Region);
            txtRoleArn.Text = profile.RoleArn;
            txtMfaSerial.Text = profile.MfaSerialNumber;
            txtDefaultBucket.Text = profile.DefaultBucket;

            if (profile.UsesAssumeRole())
            {
                rbAssumeRole.Checked = true;
            }
            else
            {
                rbAccessKey.Checked = true;
            }

            UpdateRoleFields();

            btnDelete.Enabled = true;

            if (profile.AccessKeyUnreadable)
            {
                lblStatus.Text = LanguageHelper.Get("data_files_status_key_unreadable");
            }
            else if (profile.HasOwner() == false)
            {
                lblStatus.Text = LanguageHelper.Get("data_files_status_no_owner");
            }
            else
            {
                lblStatus.Text = string.Empty;
            }
        }

        private void StartNewProfile()
        {
            _editingProfileId = 0;

            // odznačava listu bez okidanja učitavanja u editor
            _isLoadingList = true;
            lstProfiles.ClearSelection();
            _isLoadingList = false;

            grpEdit.Text = LanguageHelper.Get("data_files_new_group_title");
            txtName.Clear();
            txtAccessKey.Clear();
            AwsRegions.SelectRegion(cmbRegion, AwsRegions.DefaultRegion);
            txtRoleArn.Clear();
            txtMfaSerial.Clear();
            txtDefaultBucket.Clear();
            rbAccessKey.Checked = true;

            UpdateRoleFields();

            btnDelete.Enabled = false;
            lblStatus.Text = string.Empty;

            txtName.Focus();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            StartNewProfile();
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            AWSProfile profile = GetSelectedProfile();

            if (profile == null)
            {
                StartNewProfile();
            }
            else
            {
                ShowProfileInEditor(profile);
            }
        }

        private void LoginMode_CheckedChanged(object sender, EventArgs e)
        {
            UpdateRoleFields();
        }

        // ARN uloge i MFA imaju smisla samo za prijavu ulogom
        private void UpdateRoleFields()
        {
            bool useRole = rbAssumeRole.Checked;

            // samo polja se onemogućuju (siva pozadina dovoljno govori) - Enabled=false na Labelu ignorira temu
            // i uvijek iscrtava sistemski SystemColors.GrayText, što je u tamnoj temi bilo praktički nečitljivo
            txtRoleArn.Enabled = useRole;
            txtMfaSerial.Enabled = useRole;
        }

        // vraća null ako je sve ispravno, inače opis prve greške
        private string ValidateEditor()
        {
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                return LanguageHelper.Get("data_files_validate_name_required");
            }

            if (name.Length > AwsFormatValidator.MaxProfileNameLength)
            {
                return LanguageHelper.Format("data_files_validate_name_length", AwsFormatValidator.MaxProfileNameLength);
            }

            // access key nije obavezan (npr. još nije izdan) - ali ako je upisan, mora imati ispravan oblik
            string accessKey = txtAccessKey.Text.Trim();

            if (string.IsNullOrEmpty(accessKey) == false && AwsFormatValidator.IsValidAccessKeyId(accessKey) == false)
            {
                return LanguageHelper.Get("data_files_validate_access_key");
            }

            if (rbAssumeRole.Checked)
            {
                if (AwsFormatValidator.IsValidRoleArn(txtRoleArn.Text.Trim()) == false)
                {
                    return LanguageHelper.Get("data_files_validate_role_arn");
                }

                string mfaSerial = txtMfaSerial.Text.Trim();

                if (string.IsNullOrEmpty(mfaSerial) == false && AwsFormatValidator.IsValidMfaSerial(mfaSerial) == false)
                {
                    return LanguageHelper.Get("data_files_validate_mfa");
                }
            }

            string bucket = txtDefaultBucket.Text.Trim();

            if (string.IsNullOrEmpty(bucket) == false && AwsFormatValidator.IsValidBucketName(bucket) == false)
            {
                return LanguageHelper.Get("data_files_validate_bucket");
            }

            return null;
        }

        private AWSProfile ReadEditor()
        {
            AWSProfile profile = new AWSProfile();
            profile.ProfileId = _editingProfileId;
            profile.ProfileName = txtName.Text.Trim();
            profile.AccessKey = txtAccessKey.Text.Trim();
            profile.Region = AwsRegions.GetSelectedCode(cmbRegion);
            profile.DefaultBucket = txtDefaultBucket.Text.Trim();

            // način prijave određuje prisutnost ARN-a uloge - za prijavu ključem polja uloge se brišu
            if (rbAssumeRole.Checked)
            {
                profile.RoleArn = txtRoleArn.Text.Trim();
                profile.MfaSerialNumber = txtMfaSerial.Text.Trim();
            }

            return profile;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string validationError = ValidateEditor();

            if (validationError != null)
            {
                lblStatus.Text = validationError;
                return;
            }

            AWSProfile profile = ReadEditor();

            try
            {
                if (_editingProfileId == 0)
                {
                    _xmlManager.Add(profile, _currentUser);
                    ActivityLogger.Log("Profil dodan", "ProfileManagerForm", profile.ProfileName);
                }
                else
                {
                    _xmlManager.Update(profile, _currentUser);
                    ActivityLogger.Log("Profil ažuriran", "ProfileManagerForm", profile.ProfileName);
                }

                LastSavedProfileId = profile.ProfileId;

                LoadProfiles(profile.ProfileId);
                lblStatus.Text = LanguageHelper.Format("data_files_status_saved", profile.ProfileName);
            }
            catch (InvalidOperationException ex)
            {
                // duplikat naziva ili profil obrisan u međuvremenu
                lblStatus.Text = ex.Message;
            }
            catch (UnauthorizedAccessException ex)
            {
                lblStatus.Text = ex.Message;
            }
            catch (Exception ex)
            {
                lblStatus.Text = LanguageHelper.Format("data_files_status_save_error", ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            AWSProfile profile = GetSelectedProfile();

            if (profile == null)
            {
                return;
            }

            DialogResult confirm = MessageBox.Show(
                LanguageHelper.Format("data_files_delete_confirm", profile.ProfileName),
                LanguageHelper.Get("common_confirm_delete_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _xmlManager.Delete(profile.ProfileId, _currentUser);
                ActivityLogger.Log("Profil obrisan", "ProfileManagerForm", profile.ProfileName);

                if (LastSavedProfileId == profile.ProfileId)
                {
                    LastSavedProfileId = 0;
                }

                LoadProfiles(0);
                lblStatus.Text = LanguageHelper.Format("data_files_status_deleted", profile.ProfileName);
            }
            catch (Exception ex)
            {
                lblStatus.Text = LanguageHelper.Format("data_files_status_delete_error", ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}