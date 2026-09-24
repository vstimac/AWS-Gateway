using AWSGateway.Data;
using System.Diagnostics;

namespace AWSGateway.Helpers
{
    // slaze zapise u JsonLogManager
    public static class ActivityLogger
    {
        // username određuje UserId zapisa; nepostojeći korisnik daje UserId 0 (zapis koji ne pripada nijednom korisniku)
        private static void WriteLog(string username, string action, string resource, string details, bool isError)
        {
            int userId = 0;

            if (string.IsNullOrEmpty(username) == false)
            {
                userId = DatabaseManager.Instance.GetUserId(username);
            }

            JsonLogManager.ActivityLogEntry entry = new JsonLogManager.ActivityLogEntry();
            entry.UserId = userId;
            entry.Action = action;
            entry.Resource = resource;
            entry.Details = details;
            entry.IsError = isError;

            JsonLogManager manager = new JsonLogManager();
            manager.Add(entry);
        }

        // obican log - za prijavljenog korisnika
        public static void Log(string action, string resource, string details)
        {
            try
            {
                WriteLog(AppSettings.Instance.LastUser, action, resource, details, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod loga: " + ex.Message);
            }
        }

        // dogadaj kao gresku - za prijavljenog korisnika
        public static void LogError(string action, string resource, string errorMessage)
        {
            try
            {
                WriteLog(AppSettings.Instance.LastUser, action, resource, errorMessage, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod loga: " + ex.Message);
            }
        }

        // greška za korisnika koji još nije prijavljen (npr. kriva lozinka) - LastUser je tada prethodno prijavljeni korisnik,
        // pa bi se pokušaj prijave za korisnika X inače upisao u dnevnik nekog drugog korisnika
        public static void LogErrorForUser(string username, string action, string resource, string errorMessage)
        {
            try
            {
                WriteLog(username, action, resource, errorMessage, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod loga: " + ex.Message);
            }
        }
    }
}