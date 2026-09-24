namespace AWSGateway.Helpers
{
    // centralna putanja za sve podatke aplikacije - %AppData%\AWSGateway, umjesto Application.StartupPath
    // (StartupPath ne radi kad je app instalirana u Program Files bez admin prava, ili kad je vise korisnika na racunalu)
    public static class AppPaths
    {
        private static readonly string _dataDirectory = InitializeDataDirectory();

        public static string DataDirectory => _dataDirectory;

        public static string Combine(string fileName)
        {
            return Path.Combine(_dataDirectory, fileName);
        }

        private static string InitializeDataDirectory()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AWSGateway");
            Directory.CreateDirectory(dir);

            // jednokratna migracija podataka spremljenih uz stariju verziju (Application.StartupPath)
            MigrateLegacyFile(dir, "AWSGateway.db");
            MigrateLegacyFile(dir, "AWSGateway.ini");
            MigrateLegacyFile(dir, "aws_profiles.xml");
            MigrateLegacyFile(dir, "activity_logs.json");

            return dir;
        }

        private static void MigrateLegacyFile(string newDir, string fileName)
        {
            try
            {
                string legacyPath = Path.Combine(Application.StartupPath, fileName);
                string newPath = Path.Combine(newDir, fileName);

                if (File.Exists(legacyPath) && !File.Exists(newPath))
                {
                    File.Copy(legacyPath, newPath);
                }
            }
            catch
            {
                // migracija nije kritična - ako ne uspije, app kreće s praznim podacima na novoj putanji
            }
        }
    }
}
