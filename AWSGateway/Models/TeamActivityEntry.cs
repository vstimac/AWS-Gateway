using AWSGateway.Data;
using AWSGateway.Helpers;

namespace AWSGateway.Models
{
    // jedan zapis iz arhive TCP servera - poruka ili poslani dnevnik (isti oblik kao TeamActivityRecord na serveru)
    public class TeamActivityEntry
    {
        public const string TypeMessage = "message";
        public const string TypeSessionLogs = "session_logs";

        public string Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
        public string ClientAddress { get; set; }
        public string Message { get; set; }
        public int EntryCount { get; set; }
        public int ErrorCount { get; set; }

        // null u popisu aktivnosti; popunjava se tek kad se zatraži pojedini dnevnik
        public List<JsonLogManager.ActivityLogEntry> Entries { get; set; }

        public TeamActivityEntry()
        {
            Id = string.Empty;
            Username = string.Empty;
            Type = string.Empty;
            ClientAddress = string.Empty;
            Message = string.Empty;
            Entries = null;
        }

        public bool IsSessionLog()
        {
            return Type == TypeSessionLogs;
        }

        public string GetTypeDisplay()
        {
            if (IsSessionLog())
            {
                return LanguageHelper.Get("team_type_log");
            }

            return LanguageHelper.Get("team_type_message");
        }

        public DateTime GetLocalTime()
        {
            return DateTime.SpecifyKind(TimestampUtc, DateTimeKind.Utc).ToLocalTime();
        }
    }
}