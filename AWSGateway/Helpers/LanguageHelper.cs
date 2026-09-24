using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace AWSGateway.Helpers
{
    // Prijevodi se drže u Lang/*.json (uredivo bez rekompilacije), ne u kodu.
    public static class LanguageHelper
    {
        // <jezik <ključ, prijevod>>
        private static Dictionary<string, Dictionary<string, string>>
            _translations = new Dictionary<string, Dictionary<string, string>>();

        static LanguageHelper()
        {
            LoadTranslations("HRV", "hrv.json");
            LoadTranslations("ENG", "eng.json");
        }

        public static string Get(string key, string language = "")
        {
            string translation = Resolve(key, language);

            if (translation == null)
            {
                return key;
            }

            return translation;
        }

        // za tekstove s vrijednostima - redoslijed dijelova se razlikuje po jeziku, pa se ne smiju graditi spajanjem stringova
        // u JSON-u ključ koristi {0}, {1}... (npr. "s3_bucket_info": "{0} · objekata: {1} · {2}")
        public static string Format(string key, params object[] args)
        {
            string template = Resolve(key, string.Empty);

            if (template == null)
            {
                return key;
            }

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException ex)
            {
                Debug.WriteLine("Greška kod formatiranja prijevoda (" + key + "): " + ex.Message);
                return template;
            }
        }

        // odabrani jezik -> hrvatski (ako ključ tamo nedostaje) -> null (ključ ne postoji nigdje, ne ruši aplikaciju)
        private static string Resolve(string key, string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                language = AppSettings.Instance.CurrentLanguage;
            }

            if (_translations.ContainsKey(language) == false)
            {
                language = "HRV";
            }

            if (_translations.ContainsKey(language) && _translations[language].ContainsKey(key))
            {
                return _translations[language][key];
            }

            if (language != "HRV" && _translations.ContainsKey("HRV") && _translations["HRV"].ContainsKey(key))
            {
                return _translations["HRV"][key];
            }

            Debug.WriteLine("Nedostaje prijevod za ključ: " + key);
            return null;
        }

        public static void SetLanguage(string language)
        {
            if (_translations.ContainsKey(language))
            {
                AppSettings.Instance.CurrentLanguage = language;
                AppSettings.Instance.SaveToIni();
                ApplyCulture(language);
            }
        }

        // brojevi/datumi u sučelju prate odabrani jezik; CSV izvoz, ključevi predmemorije, zapisi u bazi i parsiranje
        // AWS iznosa ostaju na InvariantCulture (ne dira se ovom metodom - ti pozivi eksplicitno navode InvariantCulture)
        public static void ApplyCulture(string language)
        {
            CultureInfo culture;

            if (language == "ENG")
            {
                culture = new CultureInfo("en-US");
            }
            else
            {
                culture = new CultureInfo("hr-HR");
            }

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private static void LoadTranslations(string language, string fileName)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Lang", fileName);

                if (!File.Exists(path))
                {
                    _translations[language] = new Dictionary<string, string>();
                    return;
                }

                string json = File.ReadAllText(path);
                Dictionary<string, string> values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                _translations[language] = values ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod učitavanja prijevoda (" + fileName + "): " + ex.Message);
                _translations[language] = new Dictionary<string, string>();
            }
        }
    }
}
