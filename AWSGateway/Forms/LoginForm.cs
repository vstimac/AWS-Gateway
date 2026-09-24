using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Amazon.SecurityToken.Model;
using AWSGateway.Data;
using AWSGateway.Helpers;
using AWSGateway.Models;
using AWSGateway.Services;

namespace AWSGateway.Forms
{
    public partial class LoginForm : Form
    {
        // naziv profila u sesiji kad prijava nije napravljena iz spremljenog profila
        private const string NoProfileName = "(bez profila)";

        // razmaci rasporeda u logičkim pikselima (96 DPI) - LayoutForm ih preračunava za trenutni DPI
        private const int CardPadding = 16;
        private const int RowGap = 12;
        private const int SectionGap = 16;
        private const int SmallGap = 6;

        private OnlineSoapService _soapService;
        private PasswordPolicy _passwordPolicy;
        private XmlProfileManager _xmlManager;

        // punjenje popisa profila ne smije pokrenuti popunjavanje polja
        private bool _isLoadingProfiles = false;

        // korisničko ime za koje je učitan popis profila - popis se ponovno učitava kad se ime promijeni
        private string _profilesLoadedForUser = string.Empty;

        public LoginForm()
        {
            InitializeComponent();

            _soapService = new OnlineSoapService();
            _passwordPolicy = new PasswordPolicy();
            _xmlManager = new XmlProfileManager();

            LoadSettings();
            ApplyLanguage();

            AwsRegions.FillComboBox(cmbRegion, AwsRegions.DefaultRegion);
            LoadSavedProfiles(RegistryHelper.ReadInt("LastProfileId", 0));

            UiTheme.Apply(this, btnLogin);
            UiTheme.StyleCard(pnlAccountCard);
            UiTheme.StyleCard(pnlAwsCard);

            // raspored se radi nakon teme - tema mijenja fontove i razmake labela, a raspored ih poravnava s poljima
            LayoutForm();
        }

        // automatsko skaliranje (font/DPI) završava prije prikaza - raspored se preračunava nad konačnim veličinama kontrola
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LayoutForm();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            FocusFirstEmptyField();
        }

        // premještanje prozora na monitor s drugim DPI-jem skalira kontrole - razmaci se ponovno slažu
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            LayoutForm();
        }

        // korisničko ime je obično već upisano (zadnji korisnik iz INI datoteke) - tada fokus ide na lozinku
        private void FocusFirstEmptyField()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Focus();
            }
            else
            {
                txtPassword.Focus();
            }
        }

        private void pnlAccountCard_Paint(object sender, PaintEventArgs e)
        {
            UiTheme.PaintCardBorder(pnlAccountCard, e);
        }

        private void pnlAwsCard_Paint(object sender, PaintEventArgs e)
        {
            UiTheme.PaintCardBorder(pnlAwsCard, e);
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("login_title");
            lblHeader.Text = LanguageHelper.Get("main_title");
            lblAccountTitle.Text = LanguageHelper.Get("login_account_section");
            lblAwsTitle.Text = LanguageHelper.Get("login_aws_section");
            lblUsername.Text = LanguageHelper.Get("username");
            lblPassword.Text = LanguageHelper.Get("password");
            lblSavedProfiles.Text = LanguageHelper.Get("saved_profile");
            lblRegion.Text = LanguageHelper.Get("region");
            lblAccessKey.Text = LanguageHelper.Get("access_key");
            lblSecretKey.Text = LanguageHelper.Get("secret_key");
            btnLogin.Text = LanguageHelper.Get("login");
            chkUseRole.Text = LanguageHelper.Get("login_use_role");
            lblRoleArn.Text = LanguageHelper.Get("data_files_lbl_role_arn");
            lblMfaSerial.Text = LanguageHelper.Get("login_mfa_serial_label");
            lblMfaCode.Text = LanguageHelper.Get("login_mfa_code_label");
            chkSaveProfile.Text = LanguageHelper.Get("login_save_profile_label");
            txtSaveProfileName.PlaceholderText = LanguageHelper.Get("profile_name_");

            RefreshLanguageToggle();
        }

        // istaknuti odabrani jezik, drugi ostaje klikabilan ali prigušen
        private void RefreshLanguageToggle()
        {
            bool isEnglish = AppSettings.Instance.CurrentLanguage == "ENG";

            if (isEnglish)
            {
                lblLangHr.ForeColor = UiTheme.Colors.Muted;
                lblLangEn.ForeColor = UiTheme.Colors.Accent;
            }
            else
            {
                lblLangHr.ForeColor = UiTheme.Colors.Accent;
                lblLangEn.ForeColor = UiTheme.Colors.Muted;
            }
        }

        private void lblLangHr_Click(object sender, EventArgs e)
        {
            SetLanguageAndRefresh("HRV");
        }

        private void lblLangEn_Click(object sender, EventArgs e)
        {
            SetLanguageAndRefresh("ENG");
        }

        // isto mjesto spremanja kao Postavke (LanguageHelper.SetLanguage) - odabir ostaje i nakon ponovnog pokretanja
        private void SetLanguageAndRefresh(string language)
        {
            if (AppSettings.Instance.CurrentLanguage == language)
            {
                return;
            }

            LanguageHelper.SetLanguage(language);
            ApplyLanguage();

            // popis spremljenih profila prikazuje način prijave (GetLoginModeDisplay) preko ToString - ponovno punjenje ga osvježi
            AWSProfile selected = GetSelectedProfile();
            int selectedId = 0;

            if (selected != null)
            {
                selectedId = selected.ProfileId;
            }

            LoadSavedProfiles(selectedId);

            // tekstovi na drugom jeziku imaju drugu duljinu - natpisi jezika i labele se ponovno slažu
            LayoutForm();
        }

        // zadnji korisnik iz INI datoteke
        private void LoadSettings()
        {
            try
            {
                AppSettings.Instance.LoadFromIni();
                LanguageHelper.ApplyCulture(AppSettings.Instance.CurrentLanguage);

                if (!string.IsNullOrEmpty(AppSettings.Instance.LastUser))
                {
                    txtUsername.Text = AppSettings.Instance.LastUser;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod učitavanja postavki: " + ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // Raspored obrasca

        // slaže cijeli obrazac odozgo prema dolje prema stvarnim visinama kontrola
        // labela dobiva visinu svog polja i okomito centriran tekst - tako je uvijek u ravnini s poljem,
        // bez obzira na font, padding ili DPI koje postavlja tema
        // sklopivi dio (ARN uloge, MFA) prikazuje se kad je "Koristi IAM ulogu" uključeno, ručno ili iz odabranog profila
        private void LayoutForm()
        {
            int cardPadding = LogicalToDeviceUnits(CardPadding);
            int rowGap = LogicalToDeviceUnits(RowGap);
            int sectionGap = LogicalToDeviceUnits(SectionGap);
            int smallGap = LogicalToDeviceUnits(SmallGap);

            // stupci su zajednički za obje kartice i sekciju uloge (sekcija uloge je na X=0 unutar AWS kartice)
            int labelLeft = cardPadding;
            int inputLeft = txtUsername.Left;
            int inputWidth = pnlAccountCard.ClientSize.Width - inputLeft - cardPadding;

            SuspendLayout();

            // zaglavlje
            lblHeader.AutoSize = false;
            lblHeader.Size = TextRenderer.MeasureText(lblHeader.Text, lblHeader.Font);
            lblHeader.Left = pnlAccountCard.Left;
            lblHeader.Top = sectionGap;

            // odabir jezika poravnat s desnim rubom kartica - duži natpis ne izlazi iz prozora
            LayoutLanguageLabel(lblLangEn);
            LayoutLanguageLabel(lblLangHr);
            lblLangEn.Left = pnlAccountCard.Right - lblLangEn.Width;
            lblLangEn.Top = lblHeader.Top + (lblHeader.Height - lblLangEn.Height) / 2;
            lblLangHr.Left = lblLangEn.Left - smallGap - lblLangHr.Width;
            lblLangHr.Top = lblLangEn.Top;

            // kartica računa aplikacije
            pnlAccountCard.Top = lblHeader.Bottom + sectionGap;

            int y = LayoutCardTitle(lblAccountTitle, cardPadding) + rowGap;
            y = LayoutRow(lblUsername, txtUsername, y, labelLeft, inputLeft, inputWidth) + rowGap;
            y = LayoutRow(lblPassword, txtPassword, y, labelLeft, inputLeft, inputWidth);

            pnlAccountCard.Height = y + cardPadding;

            // kartica AWS pristupa
            pnlAwsCard.Top = pnlAccountCard.Bottom + sectionGap;

            y = LayoutCardTitle(lblAwsTitle, cardPadding) + rowGap;
            y = LayoutRow(lblSavedProfiles, cmbSavedProfiles, y, labelLeft, inputLeft, inputWidth) + rowGap;
            y = LayoutRow(lblRegion, cmbRegion, y, labelLeft, inputLeft, inputWidth);

            // podatci o regiji (SOAP) - jedan redak ispod odabira regije
            lblRegionInfo.AutoSize = false;
            lblRegionInfo.AutoEllipsis = true;
            lblRegionInfo.Padding = Padding.Empty;
            lblRegionInfo.Left = inputLeft;
            lblRegionInfo.Width = inputWidth;
            lblRegionInfo.Top = y + smallGap;
            lblRegionInfo.Height = LineHeight(lblRegionInfo);

            y = lblRegionInfo.Bottom + rowGap;
            y = LayoutRow(lblAccessKey, txtAccessKey, y, labelLeft, inputLeft, inputWidth) + rowGap;
            y = LayoutRow(lblSecretKey, txtSecretKey, y, labelLeft, inputLeft, inputWidth) + rowGap;

            // checkbox je u stupcu polja, odmah ispod Secret Key
            chkUseRole.Left = inputLeft;
            chkUseRole.Top = y;
            y = chkUseRole.Bottom;

            bool useRole = chkUseRole.Checked;
            pnlRoleSection.Visible = useRole;

            if (useRole)
            {
                pnlRoleSection.Left = 0;
                pnlRoleSection.Width = pnlAwsCard.ClientSize.Width;
                pnlRoleSection.Top = y + rowGap;

                int roleY = 0;
                roleY = LayoutRow(lblRoleArn, txtRoleArn, roleY, labelLeft, inputLeft, inputWidth) + rowGap;
                roleY = LayoutRow(lblMfaSerial, txtMfaSerial, roleY, labelLeft, inputLeft, inputWidth) + rowGap;

                // MFA kod ima 6 znamenki - polje zadržava svoju užu širinu
                roleY = LayoutRow(lblMfaCode, txtMfaCode, roleY, labelLeft, inputLeft, txtMfaCode.Width);

                pnlRoleSection.Height = roleY;
                y = pnlRoleSection.Bottom;
            }

            chkSaveProfile.Left = inputLeft;
            chkSaveProfile.Top = y + rowGap;

            txtSaveProfileName.Left = inputLeft;
            txtSaveProfileName.Width = inputWidth;
            txtSaveProfileName.Top = chkSaveProfile.Bottom + smallGap;

            pnlAwsCard.Height = txtSaveProfileName.Bottom + cardPadding;

            // status (mjesto za dva retka poruke) i gumb za prijavu
            lblStatus.Left = pnlAwsCard.Left;
            lblStatus.Width = pnlAwsCard.Width;
            lblStatus.Top = pnlAwsCard.Bottom + smallGap;
            lblStatus.Height = LineHeight(lblStatus) * 2 + smallGap;

            btnLogin.Left = pnlAwsCard.Left;
            btnLogin.Width = pnlAwsCard.Width;
            btnLogin.Top = lblStatus.Bottom + smallGap;

            // lijevi i desni rub prozora su jednaki
            ClientSize = new Size(pnlAwsCard.Right + pnlAwsCard.Left, btnLogin.Bottom + sectionGap);

            ResumeLayout(true);

            // kartice same crtaju rub - nakon promjene visine rub se ponovno iscrtava
            pnlAccountCard.Invalidate();
            pnlAwsCard.Invalidate();
        }

        // naslov kartice u gornjem lijevom kutu; vraća donji rub naslova
        private int LayoutCardTitle(Label title, int cardPadding)
        {
            title.AutoSize = false;
            title.Padding = Padding.Empty;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Left = cardPadding;
            title.Top = cardPadding;
            title.Width = title.Parent.ClientSize.Width - cardPadding * 2;
            title.Height = LineHeight(title);

            return title.Bottom;
        }

        // jedan redak obrasca: labela lijevo, polje desno, okomito centrirani; vraća donji rub retka
        private int LayoutRow(Label label, Control input, int top, int labelLeft, int inputLeft, int inputWidth)
        {
            int gap = LogicalToDeviceUnits(SmallGap);

            input.Left = inputLeft;
            input.Top = top;
            input.Width = inputWidth;

            label.AutoSize = false;
            label.AutoEllipsis = true;
            label.Padding = Padding.Empty;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Left = labelLeft;
            label.Width = inputLeft - labelLeft - gap;
            label.Height = Math.Max(input.Height, LineHeight(label));
            label.Top = input.Top + (input.Height - label.Height) / 2;

            return Math.Max(input.Bottom, label.Bottom);
        }

        // natpis jezika je širok točno koliko i tekst
        private void LayoutLanguageLabel(Label label)
        {
            label.AutoSize = false;
            label.Padding = Padding.Empty;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Size = TextRenderer.MeasureText(label.Text, label.Font);
        }

        // visina jednog retka teksta za font kontrole
        private static int LineHeight(Control control)
        {
            return TextRenderer.MeasureText("Ag", control.Font).Height;
        }

        // ---------------------------------------------------------------
        // Spremljeni profili

        // puni popis profila upisanog korisnika i odabire zadani profil (zadnji korišteni); 0 = ručni unos
        // prikazuju se samo profili tog korisnika i stariji profili bez vlasnika - to su netajne postavke (naziv, regija, ARN, access key ID)
        // prijava profile samo čita - uređivanje, brisanje i oporavak oštećene datoteke dostupni su tek nakon prijave (AWS profili u glavnom prozoru)
        private void LoadSavedProfiles(int selectProfileId)
        {
            List<AWSProfile> profiles;

            _profilesLoadedForUser = txtUsername.Text.Trim();

            try
            {
                profiles = _xmlManager.GetForUser(_profilesLoadedForUser);
            }
            catch (InvalidDataException ex)
            {
                Debug.WriteLine("Greška kod učitavanja profila: " + ex.Message);
                profiles = new List<AWSProfile>();
                lblStatus.Text = LanguageHelper.Get("login_profiles_load_corrupt");
            }
            catch (Exception ex)
            {
                profiles = new List<AWSProfile>();
                lblStatus.Text = LanguageHelper.Format("login_profiles_load_error", ex.Message);
            }

            _isLoadingProfiles = true;

            cmbSavedProfiles.Items.Clear();
            cmbSavedProfiles.Items.Add(LanguageHelper.Get("login_no_profile_item"));

            int indexToSelect = 0;

            foreach (AWSProfile profile in profiles)
            {
                // ComboBox prikazuje AWSProfile.ToString - "naziv (regija, način prijave)"
                int index = cmbSavedProfiles.Items.Add(profile);

                if (profile.ProfileId == selectProfileId)
                {
                    indexToSelect = index;
                }
            }

            cmbSavedProfiles.SelectedIndex = indexToSelect;

            _isLoadingProfiles = false;

            ApplySelectedProfile();
        }

        // null za ručni unos
        private AWSProfile GetSelectedProfile()
        {
            return cmbSavedProfiles.SelectedItem as AWSProfile;
        }

        private void cmbSavedProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoadingProfiles)
            {
                return;
            }

            ApplySelectedProfile();
        }

        // popunjava sva spremljena polja profila; tajni ključ i MFA kod uvijek upisuje korisnik
        private void ApplySelectedProfile()
        {
            AWSProfile profile = GetSelectedProfile();

            if (profile == null)
            {
                // ručni unos - postojeća polja ostaju kako jesu, samo se ne predlaže naziv profila
                txtSaveProfileName.Clear();
                return;
            }

            // drugi access key znači i drugi secret key - stari tajni ključ se briše da se ne pošalje uz krivi access key
            if (txtAccessKey.Text != profile.AccessKey)
            {
                txtSecretKey.Clear();
            }

            txtAccessKey.Text = profile.AccessKey;
            AwsRegions.SelectRegion(cmbRegion, profile.Region);

            // promjena checkboxa preko chkUseRole_CheckedChanged ponovno slaže obrazac
            chkUseRole.Checked = profile.UsesAssumeRole();
            txtRoleArn.Text = profile.RoleArn;
            txtMfaSerial.Text = profile.MfaSerialNumber;
            txtMfaCode.Clear();

            // spremanje s istim nazivom ažurira odabrani profil
            txtSaveProfileName.Text = profile.ProfileName;

            if (profile.AccessKeyUnreadable)
            {
                lblStatus.Text = LanguageHelper.Get("login_profile_key_unreadable");
            }
            else if (string.IsNullOrEmpty(profile.AccessKey))
            {
                lblStatus.Text = LanguageHelper.Format("login_profile_no_access_key", profile.ProfileName);
            }
            else
            {
                lblStatus.Text = LanguageHelper.Format("login_profile_enter_secret", profile.ProfileName);
            }
        }

        // popis profila ovisi o korisniku - ponovno se učitava kad korisnik izađe iz polja s promijenjenim imenom
        // (klik na PRIJAVA također izaziva Leave, pa je popis uvijek usklađen s imenom prije prijave)
        private void txtUsername_Leave(object sender, EventArgs e)
        {
            if (txtUsername.Text.Trim() == _profilesLoadedForUser)
            {
                return;
            }

            LoadSavedProfiles(RegistryHelper.ReadInt("LastProfileId", 0));
        }

        private void chkSaveProfile_CheckedChanged(object sender, EventArgs e)
        {
            txtSaveProfileName.Enabled = chkSaveProfile.Checked;
        }

        // prikazuje/skriva sklopivi dio za AssumeRole prijavu i pomiče ostatak obrasca za razliku u visini
        private void chkUseRole_CheckedChanged(object sender, EventArgs e)
        {
            LayoutForm();
        }

        // sprema postavke uspješne prijave kao profil (bez tajnih podataka); vraća ID profila ili 0 ako nije spremljen
        private int SaveLoginAsProfile(AWSProfile loginProfile)
        {
            string profileName = txtSaveProfileName.Text.Trim();

            try
            {
                string username = txtUsername.Text.Trim();
                AWSProfile existing = _xmlManager.FindByName(profileName, username);

                AWSProfile toSave = new AWSProfile();
                toSave.ProfileName = profileName;
                toSave.AccessKey = loginProfile.AccessKey;
                toSave.Region = loginProfile.Region;
                toSave.RoleArn = loginProfile.RoleArn;
                toSave.MfaSerialNumber = loginProfile.MfaSerialNumber;

                if (existing == null)
                {
                    _xmlManager.Add(toSave, username);
                    ActivityLogger.Log("Profil dodan", "LoginForm", profileName);
                    return toSave.ProfileId;
                }

                // postojeći profil: odabrani se ažurira bez pitanja, a za drugi profil istog naziva traži se potvrda
                AWSProfile selected = GetSelectedProfile();
                bool isSelectedProfile = selected != null && selected.ProfileId == existing.ProfileId;

                if (isSelectedProfile == false)
                {
                    DialogResult confirm = MessageBox.Show(
                        LanguageHelper.Format("login_profile_overwrite_confirm", existing.ProfileName),
                        LanguageHelper.Get("login_save_profile_title"),
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes)
                    {
                        return 0;
                    }
                }

                // zadani bucket se ne unosi na prijavi - zadržava se postojeća vrijednost
                toSave.ProfileId = existing.ProfileId;
                toSave.DefaultBucket = existing.DefaultBucket;

                _xmlManager.Update(toSave, username);
                ActivityLogger.Log("Profil ažuriran", "LoginForm", profileName);

                return toSave.ProfileId;
            }
            catch (Exception ex)
            {
                // prijava je uspjela - neuspjelo spremanje profila ne smije je zaustaviti
                ActivityLogger.LogError("Spremanje profila", "LoginForm", ex.Message);
                MessageBox.Show(LanguageHelper.Format("login_profile_save_failed", ex.Message), LanguageHelper.Get("login_save_profile_title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
        }

        // ---------------------------------------------------------------
        // Prijava

        // provjeravam prijave
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;

            // korisničko ime se poravnava jednom - baza, vlasništvo profila i postavke tako koriste istu vrijednost
            txtUsername.Text = txtUsername.Text.Trim();

            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show(LanguageHelper.Get("login_enter_username"));
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show(LanguageHelper.Get("login_enter_password"));
                    return;
                }

                // naziv profila se provjerava prije poziva na AWS, da se korisnik ne prijavljuje dvaput
                if (chkSaveProfile.Checked)
                {
                    string profileName = txtSaveProfileName.Text.Trim();

                    if (string.IsNullOrEmpty(profileName))
                    {
                        MessageBox.Show(LanguageHelper.Get("login_enter_profile_name_or_disable"));
                        return;
                    }

                    if (profileName.Length > AwsFormatValidator.MaxProfileNameLength)
                    {
                        MessageBox.Show(LanguageHelper.Format("data_files_validate_name_length", AwsFormatValidator.MaxProfileNameLength));
                        return;
                    }
                }

                // račun aplikacije - postojeći korisnik mora upisati ispravnu lozinku, a novi korisnik lozinku koja zadovoljava pravila
                DatabaseManager db = DatabaseManager.Instance;
                bool isNewUser = db.UserExists(txtUsername.Text) == false;

                if (isNewUser)
                {
                    if (_passwordPolicy.IsValid(txtPassword.Text) == false)
                    {
                        lblStatus.Text = LanguageHelper.Get("login_wrong_password");
                        MessageBox.Show(_passwordPolicy.GetRequirementsText());
                        return;
                    }
                }
                else
                {
                    if (db.VerifyUserPassword(txtUsername.Text, txtPassword.Text) == false)
                    {
                        lblStatus.Text = LanguageHelper.Get("login_wrong_password");
                        ActivityLogger.LogErrorForUser(txtUsername.Text, "Login nije uspio", "LoginForm", "Kriva lozinka za " + txtUsername.Text);
                        MessageBox.Show(LanguageHelper.Get("login_wrong_password"));
                        return;
                    }
                }

                // AWS podatci
                if (string.IsNullOrWhiteSpace(txtAccessKey.Text) || string.IsNullOrWhiteSpace(txtSecretKey.Text))
                {
                    MessageBox.Show(LanguageHelper.Get("login_enter_aws_data"));
                    return;
                }

                // format ključeva AWS-a
                AWSProfile profile = new AWSProfile();
                profile.ProfileName = NoProfileName;
                profile.AccessKey = txtAccessKey.Text.Trim();
                profile.SecretKey = txtSecretKey.Text.Trim();
                profile.Region = AwsRegions.GetSelectedCode(cmbRegion);

                if (profile.Validate() == false)
                {
                    MessageBox.Show(LanguageHelper.Get("login_invalid_aws_key_format"));
                    return;
                }

                // AssumeRole - MFA kod bez serijskog broja je nevaljana kombinacija, upozoravamo prije poziva na AWS
                if (chkUseRole.Checked)
                {
                    if (string.IsNullOrWhiteSpace(txtRoleArn.Text))
                    {
                        MessageBox.Show(LanguageHelper.Get("login_enter_role_arn"));
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtMfaCode.Text) == false && string.IsNullOrWhiteSpace(txtMfaSerial.Text))
                    {
                        MessageBox.Show(LanguageHelper.Get("login_mfa_code_without_serial"));
                        return;
                    }
                }

                // provjera na AWS-u
                lblStatus.Text = LanguageHelper.Get("login_status_checking");

                bool credentialsGood = false;
                StsService stsService = new StsService();

                try
                {
                    if (chkUseRole.Checked)
                    {
                        profile.RoleArn = txtRoleArn.Text.Trim();
                        profile.MfaSerialNumber = txtMfaSerial.Text.Trim();

                        string roleSessionName = StsService.BuildRoleSessionName(txtUsername.Text, DateTime.UtcNow);
                        Credentials stsCredentials;

                        try
                        {
                            stsCredentials = await stsService.AssumeRoleAsync(profile, profile.RoleArn, profile.MfaSerialNumber, txtMfaCode.Text.Trim(), roleSessionName, true);
                        }
                        catch (Exception durationEx)
                        {
                            // uloga može imati manji MaxSessionDuration od traženog - pokušaj ponovno bez DurationSeconds (SDK/AWS onda vrati zadano trajanje uloge)
                            if (durationEx.Message.IndexOf("duration", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                stsCredentials = await stsService.AssumeRoleAsync(profile, profile.RoleArn, profile.MfaSerialNumber, txtMfaCode.Text.Trim(), roleSessionName, false);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        profile.TempAccessKeyId = stsCredentials.AccessKeyId;
                        profile.TempSecretAccessKey = stsCredentials.SecretAccessKey;
                        profile.SessionToken = stsCredentials.SessionToken;

                        DateTime expiration = stsCredentials.Expiration ?? DateTime.UtcNow.AddHours(1);

                        if (expiration.Kind == DateTimeKind.Utc)
                        {
                            profile.SessionExpiresAtUtc = expiration;
                        }
                        else
                        {
                            profile.SessionExpiresAtUtc = DateTime.SpecifyKind(expiration, DateTimeKind.Utc);
                        }
                    }

                    // provjera vjerodajnica (oba načina prijave) - GetCallerIdentity ne ovisi o S3 dozvolama poput s3:ListAllMyBuckets
                    (string account, string arn) = await stsService.GetCallerIdentityAsync(profile);
                    profile.AwsAccountId = account;
                    profile.AwsArn = arn;

                    credentialsGood = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Greška kod AWS-a: " + ex.Message);
                }

                if (credentialsGood == false)
                {
                    lblStatus.Text = LanguageHelper.Get("login_status_invalid");

                    // novi korisnik još ne postoji u bazi - greška se bilježi bez vezanja uz korisnika
                    if (isNewUser)
                    {
                        ActivityLogger.LogError("Login neuspješan", "LoginForm", "Krivi AWS podatci za " + txtUsername.Text);
                    }
                    else
                    {
                        ActivityLogger.LogErrorForUser(txtUsername.Text, "Login neuspješan", "LoginForm", "Krivi AWS podatci za " + txtUsername.Text);
                    }

                    MessageBox.Show(LanguageHelper.Get("login_status_invalid"));
                    return;
                }

                // registracija tek nakon uspješne provjere AWS-a - pogrešno upisano korisničko ime uz krive AWS podatke ne ostavlja račun u bazi
                if (isNewUser)
                {
                    db.RegisterUser(txtUsername.Text, txtPassword.Text);
                }

                // naziv i zadani bucket dolaze iz odabranog profila - S3 preglednik nudi zadani bucket, izvještaj bilježi naziv profila
                AWSProfile selectedProfile = GetSelectedProfile();

                // dodatna provjera - profil drugog korisnika se ne prenosi u sesiju
                if (selectedProfile != null && selectedProfile.IsVisibleTo(txtUsername.Text) == false)
                {
                    selectedProfile = null;
                }

                if (selectedProfile != null)
                {
                    profile.ProfileId = selectedProfile.ProfileId;
                    profile.ProfileName = selectedProfile.ProfileName;
                    profile.DefaultBucket = selectedProfile.DefaultBucket;
                }

                if (chkSaveProfile.Checked)
                {
                    int savedProfileId = SaveLoginAsProfile(profile);

                    if (savedProfileId > 0)
                    {
                        AWSProfile savedProfile = _xmlManager.GetById(savedProfileId, txtUsername.Text);

                        if (savedProfile != null)
                        {
                            profile.ProfileId = savedProfile.ProfileId;
                            profile.ProfileName = savedProfile.ProfileName;
                            profile.DefaultBucket = savedProfile.DefaultBucket;
                        }
                    }
                }

                // spremi postavke
                lblStatus.Text = LanguageHelper.Get("login_status_success");
                AppSettings.Instance.LastUser = txtUsername.Text;
                AppSettings.Instance.SaveToIni();

                // zadnji profil se pamti po ID-u (0 = ručni unos)
                RegistryHelper.WriteString("LastProfileId", profile.ProfileId.ToString());

                ActivityLogger.Log("Login", "LoginForm", "Korisnik " + txtUsername.Text + " se prijavio - profil: " + profile.ProfileName + ", regija: " + profile.Region);

                // profil -> MainForm; using oslobađa glavni prozor nakon zatvaranja (inače ostaje u memoriji nakon svake odjave)
                bool showLoginAgain = false;
                bool sessionExpired = false;

                using (MainForm mainForm = new MainForm(profile))
                {
                    Hide();

                    DialogResult mainFormResult = mainForm.ShowDialog();

                    if (mainFormResult == DialogResult.Retry)
                    {
                        showLoginAgain = true;
                        sessionExpired = mainForm.SessionExpired;
                    }
                }

                if (showLoginAgain)
                {
                    // sesija je istekla ILI se korisnik odjavio - očisti osjetljiva i privremena polja, prikaži prijavu ponovno (aplikacija se ne zatvara)
                    txtPassword.Clear();
                    txtSecretKey.Clear();
                    txtMfaCode.Clear();
                    chkSaveProfile.Checked = false;

                    // profili su se mogli promijeniti tijekom sesije (AWS profili u glavnom prozoru)
                    LoadSavedProfiles(profile.ProfileId);

                    // poruka o isteku sesije prikazuje se samo kad je sesija stvarno istekla - odjava ne prikazuje posebnu poruku
                    if (sessionExpired)
                    {
                        lblStatus.Text = LanguageHelper.Get("login_session_expired_retry");
                    }

                    Show();
                    FocusFirstEmptyField();
                    return;
                }

                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = LanguageHelper.Get("login_status_error");
                ActivityLogger.LogErrorForUser(txtUsername.Text, "Login greška", "LoginForm", ex.Message);
                MessageBox.Show(LanguageHelper.Format("login_error_generic", ex.Message));
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        // online soap servis - naziv države + glavni grad
        private async void cmbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                lblRegionInfo.Text = "";

                if (cmbRegion.SelectedItem == null)
                {
                    return;
                }

                string regionCode = AwsRegions.GetSelectedCode(cmbRegion);
                string info = await _soapService.GetAwsRegionInfoAsync(regionCode);

                // rezultat stiže asinkrono - ako je korisnik u međuvremenu odabrao drugu regiju, stari rezultat se ne prikazuje
                if (AwsRegions.GetSelectedCode(cmbRegion) == regionCode)
                {
                    lblRegionInfo.Text = regionCode + " > " + info;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod prikaza regije: " + ex.Message);
            }
        }
    }
}