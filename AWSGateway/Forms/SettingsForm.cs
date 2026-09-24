using AWSGateway.Data;
using AWSGateway.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AWSGateway.Forms
{
    public partial class SettingsForm : Form, IAppModule
    {
        public event EventHandler SettingsSaved;

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
            UiTheme.StyleDangerButton(btnDeleteAccount);
        }

        public SettingsForm()
        {
            InitializeComponent();
            ApplyLanguage();
            ApplyShellTheme();
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageHelper.Get("settings_title");
            lblLanguage.Text = LanguageHelper.Get("language");
            lblTheme.Text = LanguageHelper.Get("theme");
            lblCacheTtl.Text = LanguageHelper.Get("cache_ttl_hours");
            btnSave.Text = LanguageHelper.Get("save");
            btnCancel.Text = LanguageHelper.Get("cancel");
            grpApp.Text = LanguageHelper.Get("app_section");
            grpAccount.Text = LanguageHelper.Get("user_profile_title");
            btnAvatar.Text = LanguageHelper.Get("upload_avatar");
            btnDeleteAccount.Text = LanguageHelper.Get("delete_account");
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            LoadCurrentSettings();
            LoadAvatar();
        }

        // zadnje spremljene vrijednosti - koristi se i kod početnog učitavanja i kod Odustani u ugrađenom modulu
        private void LoadCurrentSettings()
        {
            cmbLanguage.SelectedItem = AppSettings.Instance.CurrentLanguage;
            cmbTheme.SelectedItem = AppSettings.Instance.Theme;
            numCacheTtl.Value = AppSettings.Instance.CacheTtlHours;
        }

        private void LoadAvatar()
        {
            try
            {
                string username = AppSettings.Instance.LastUser;

                if (string.IsNullOrEmpty(username))
                {
                    return;
                }

                byte[] avatarData = DatabaseManager.Instance.GetUserAvatar(username);

                if (avatarData == null)
                {
                    return;
                }

                // kopiram u bitmap
                using (MemoryStream stream = new MemoryStream(avatarData))
                {
                    using (Image tempImage = Image.FromStream(stream))
                    {
                        Image oldImage = picAvatar.Image;
                        picAvatar.Image = new Bitmap(tempImage);

                        if (oldImage != null)
                        {
                            oldImage.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod učitavanja avatara: " + ex.Message);
            }
        }

        private void btnAvatar_Click(object sender, EventArgs e)
        {
            try
            {
                string path;

                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = LanguageHelper.Get("settings_avatar_filter");

                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    path = dialog.FileName;
                }

                string username = AppSettings.Instance.LastUser;

                // cijela datoteka u bazu kao niz bajtova
                byte[] imageData = File.ReadAllBytes(path);

                DatabaseManager.Instance.SaveAvatar(username, imageData);

                Image newImage;

                using (Image tempImage = Image.FromFile(path))
                {
                    newImage = new Bitmap(tempImage);
                }

                Image oldImage = picAvatar.Image;
                picAvatar.Image = newImage;

                if (oldImage != null)
                {
                    oldImage.Dispose();
                }

                if (SettingsSaved != null)
                {
                    SettingsSaved(this, EventArgs.Empty);
                }

                MessageBox.Show(LanguageHelper.Format("settings_avatar_saved", imageData.Length));
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageHelper.Format("settings_avatar_save_error", ex.Message));
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            string username = AppSettings.Instance.LastUser;

            DialogResult confirm = MessageBox.Show(
                LanguageHelper.Format("settings_delete_account_confirm", username),
                LanguageHelper.Get("settings_delete_account_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                DatabaseManager.Instance.DeleteUser(username);

                XmlProfileManager profileManager = new XmlProfileManager();
                int deletedProfiles = profileManager.DeleteAllForOwner(username);

                ActivityLogger.Log("Brisanje računa", "SettingsForm", "Obrisan račun " + username + ", obrisano profila: " + deletedProfiles);

                MessageBox.Show(LanguageHelper.Get("settings_delete_account_done"));

                AppSettings.Instance.LastUser = "";
                AppSettings.Instance.SaveToIni();

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageHelper.Format("settings_delete_account_error", ex.Message));
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // SetLanguage prebaci rjecnik i zapamti izbor u postavkama
            LanguageHelper.SetLanguage(cmbLanguage.SelectedItem.ToString());

            AppSettings.Instance.Theme = cmbTheme.SelectedItem.ToString();
            AppSettings.Instance.CacheTtlHours = (int)numCacheTtl.Value;

            AppSettings.Instance.SaveToIni();

            DialogResult = DialogResult.OK;

            if (SettingsSaved != null)
            {
                SettingsSaved(this, EventArgs.Empty);
            }

            // ugrađen u ljusku (nije TopLevel) - nema modalni prozor za zatvoriti
            if (TopLevel)
            {
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            if (TopLevel)
            {
                Close();
            }
            else
            {
                // ugrađen u ljusku - nema modalni prozor za zatvoriti, vrati polja na zadnje spremljene vrijednosti
                LoadCurrentSettings();
            }
        }
    }
}
