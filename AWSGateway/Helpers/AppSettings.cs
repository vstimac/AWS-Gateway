using System.Diagnostics;

namespace AWSGateway.Helpers
{
    public class AppSettings
    {
        private static readonly AppSettings _instance = new AppSettings();

        public static AppSettings Instance
        {
            get { return _instance; }
        }

        public string CurrentLanguage { get; set; }
        public string Theme { get; set; }
        public string LastUser { get; set; }
        public string DatabasePath { get; set; }
        // koliko sati podaci o troskovima vrijede kao "svjezi" prije nego se ponovno dohvate s AWS-a (IP2)
        public int CacheTtlHours { get; set; }

        private AppSettings()
        {
            CurrentLanguage = "HRV";
            Theme = "Light";
            LastUser = string.Empty;
            DatabasePath = AppPaths.Combine("AWSGateway.db");
            CacheTtlHours = 6;
        }
        private string GetIniPath()
        {
            return AppPaths.Combine("AWSGateway.ini");
        }

        // cita INI
        public void LoadFromIni()
        {
            IniFile ini = new IniFile(GetIniPath());

            if (ini.FileExists() == false)
            {
                return;
            }

            try
            {
                CurrentLanguage = ini.Read("Settings", "Language", "HRV");
                LastUser = ini.Read("Settings", "LastUser", "");
                Theme = ini.Read("Settings", "Theme", "Light");

                string ttlText = ini.Read("Settings", "CacheTtlHours", "6");
                int parsedTtl;

                if (int.TryParse(ttlText, out parsedTtl) && parsedTtl > 0)
                {
                    CacheTtlHours = parsedTtl;
                }
                else
                {
                    CacheTtlHours = 6;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod učitavanja INI: " + ex.Message);
            }
        }

        // sprema INI
        public void SaveToIni()
        {
            try
            {
                IniFile ini = new IniFile(GetIniPath());

                ini.Write("Settings", "Language", CurrentLanguage);
                ini.Write("Settings", "LastUser", LastUser);
                ini.Write("Settings", "Theme", Theme);
                ini.Write("Settings", "CacheTtlHours", CacheTtlHours.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod spremanja INI: " + ex.Message);
            }
        }
    }
}