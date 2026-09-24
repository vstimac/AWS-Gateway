using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AWSGateway.Data
{
    public class JsonLogManager
    {
        public class ActivityLogEntry
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Action { get; set; }
            public string Resource { get; set; }
            public string Details { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsError { get; set; }

            public ActivityLogEntry()
            {
                Id = 0;
                UserId = 0;
                Action = string.Empty;
                Resource = string.Empty;
                Details = string.Empty;
                CreatedAt = DateTime.Now;
                IsError = false;
            }
        }

        // ograničava rast activity_logs.json - drži samo posljednjih N zapisa
        private const int MaxEntries = 2000;

        // dnevnik aktivnosti se samo dopisuje - nema izmjene ni brisanja pojedinih zapisa (inače bi se mogli ukloniti tragovi, npr. neuspjele prijave)
        // svako dodavanje je "pročitaj cijelu datoteku -> dodaj -> zapiši cijelu datoteku";
        // bez zaključavanja dvije istovremene izmjene pročitaju isto stanje i druga prepiše prvu (izgubljeni zapis)
        // static - vrijedi za sve instance JsonLogManagera u procesu (ActivityLogger svaki put stvara novu)
        private static readonly object FileLock = new object();

        private string _jsonPath;

        public JsonLogManager()
        {
            _jsonPath = Helpers.AppPaths.Combine("activity_logs.json");
        }

        // Save logs - pozivatelj već drži FileLock
        private void SaveLogs(List<ActivityLogEntry> logs)
        {
            string json = JsonSerializer.Serialize(logs, GetJsonOptions());
            File.WriteAllText(_jsonPath, json);
        }

        private JsonSerializerOptions GetJsonOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true; // čitljivije
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            return options;
        }

        // add
        public void Add(ActivityLogEntry entry)
        {
            try
            {
                lock (FileLock)
                {
                    List<ActivityLogEntry> logs = GetAll();

                    int highestId = 0;

                    foreach (ActivityLogEntry log in logs)
                    {
                        if (log.Id > highestId)
                        {
                            highestId = log.Id;
                        }
                    }

                    entry.Id = highestId + 1;
                    entry.CreatedAt = DateTime.Now;
                    logs.Add(entry);

                    // rotacija - odbaci najstarije zapise iznad limita
                    if (logs.Count > MaxEntries)
                    {
                        logs = logs.Skip(logs.Count - MaxEntries).ToList();
                    }

                    SaveLogs(logs);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod dodavanja JSONa: " + ex.Message);
            }
        }

        // read - lock je ponovno ulazan (reentrant), pa GetAll smije zvati i metoda koja ga već drži
        public List<ActivityLogEntry> GetAll()
        {
            try
            {
                lock (FileLock)
                {
                    // ako ne postoji napravi praznu listu
                    if (!File.Exists(_jsonPath))
                    {
                        return new List<ActivityLogEntry>();
                    }

                    string json = File.ReadAllText(_jsonPath);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<ActivityLogEntry>();
                    }

                    List<ActivityLogEntry> logs = JsonSerializer.Deserialize<List<ActivityLogEntry>>(json, GetJsonOptions());

                    if (logs == null)
                    {
                        return new List<ActivityLogEntry>();
                    }

                    return logs;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod čitanja svih JSON-a: " + ex.Message);
            }
        }

        // READ - zapisi jednog korisnika; dnevnik se ne mijenja i ne briše iz aplikacije (samo dopisivanje + rotacija)
        public List<ActivityLogEntry> GetForUser(int userId)
        {
            List<ActivityLogEntry> userLogs = new List<ActivityLogEntry>();

            foreach (ActivityLogEntry log in GetAll())
            {
                if (log.UserId == userId)
                {
                    userLogs.Add(log);
                }
            }

            return userLogs;
        }
    }
}