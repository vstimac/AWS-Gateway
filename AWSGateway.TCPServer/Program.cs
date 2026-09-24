using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace AWSGateway.TCPServer
{
    // AWS Gateway TCP server - osnovna verzija
    //
    // Zadano sluša samo na ovom računalu (127.0.0.1). Uredski način (--lan) sluša na svim mrežnim sučeljima i
    // placeholder je za sljedeće izdanje: u ovoj verziji NEMA autentikacije, pa korisničko ime u poruci nije provjereno.
    //
    // Protokol - jedan redak zahtjeva, jedan redak odgovora (UTF-8):
    //   PING                          -> PONG
    //   ACTIVITY {"username","message"}          -> OK | ERROR:razlog
    //   SESSION_LOGS {"username","entries":[...]} -> OK | ERROR:razlog
    //   GET_ACTIVITY                  -> OK [zapisi bez stavki dnevnika, najnoviji prvi]
    //   GET_SESSION_LOG {"id"}        -> OK {zapis sa stavkama} | ERROR:razlog
    class Program
    {
        private const int Port = 5555;

        // zaštita od klijenta koji se spoji i ništa ne pošalje, i od prevelike poruke
        private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(10);
        private const int MaxRequestChars = 1024 * 1024;
        private const int MaxConcurrentClients = 20;

        // ograničenja sadržaja
        private const int MaxUsernameLength = 50;
        private const int MaxMessageLength = 500;
        private const int MaxSessionEntries = 500;

        // arhiva - zadnjih MaxArchiveRecords zapisa, GET_ACTIVITY vraća zadnjih MaxReturnedRecords
        private const int MaxArchiveRecords = 5000;
        private const int MaxReturnedRecords = 200;
        private const int RotationCheckInterval = 100;

        public const string TypeMessage = "message";
        public const string TypeSessionLogs = "session_logs";

        // ista mapa podataka kao glavna aplikacija (AppPaths): %AppData%\AWSGateway
        private static readonly string ArchivePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AWSGateway", "team_activity.jsonl");

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // klijenti se obrađuju istovremeno, pa se pristup datoteci arhive zaključava
        private static readonly object ArchiveLock = new object();
        private static int _appendsSinceRotation = 0;

        private static SemaphoreSlim _clientLimiter = new SemaphoreSlim(MaxConcurrentClients);

        static async Task Main(string[] args)
        {
            bool lanMode = args.Contains("--lan");

            IPAddress bindAddress = IPAddress.Loopback;

            if (lanMode)
            {
                bindAddress = IPAddress.Any;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ArchivePath));
            RotateArchive();

            TcpListener listener = new TcpListener(bindAddress, Port);
            listener.Start();

            Console.WriteLine("AWS Gateway TCP server");
            Console.WriteLine("Adresa: " + bindAddress + ":" + Port);
            Console.WriteLine("Arhiva: " + ArchivePath);

            if (lanMode)
            {
                Console.WriteLine("UPOZORENJE: uredski način (--lan) - server je dostupan u lokalnoj mreži bez autentikacije.");
            }

            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();

                    // svaki klijent u zasebnom zadatku - spor ili neaktivan klijent ne blokira ostale
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška kod prihvaćanja klijenta: " + ex.Message);
                }
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            await _clientLimiter.WaitAsync();

            string clientAddress = "nepoznato";

            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false)))
                using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                {
                    writer.AutoFlush = true;

                    if (client.Client.RemoteEndPoint != null)
                    {
                        clientAddress = client.Client.RemoteEndPoint.ToString();
                    }

                    string request;

                    try
                    {
                        request = await ReadLimitedLineAsync(reader);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Klijent " + clientAddress + " nije poslao zahtjev na vrijeme");
                        return;
                    }
                    catch (InvalidDataException ex)
                    {
                        await writer.WriteLineAsync("ERROR:" + ex.Message);
                        return;
                    }

                    if (request == null)
                    {
                        return;
                    }

                    string response = ProcessRequest(request, clientAddress);
                    await writer.WriteLineAsync(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška kod klijenta " + clientAddress + ": " + ex.Message);
            }
            finally
            {
                _clientLimiter.Release();
            }
        }

        // čita jedan redak s vremenskim ograničenjem i najvećom duljinom; null ako je klijent zatvorio vezu bez podataka
        private static async Task<string> ReadLimitedLineAsync(StreamReader reader)
        {
            StringBuilder line = new StringBuilder();
            char[] buffer = new char[1];

            using (CancellationTokenSource timeout = new CancellationTokenSource(ReadTimeout))
            {
                while (true)
                {
                    int read = await reader.ReadAsync(buffer.AsMemory(0, 1), timeout.Token);

                    if (read == 0)
                    {
                        if (line.Length == 0)
                        {
                            return null;
                        }

                        return line.ToString();
                    }

                    char c = buffer[0];

                    if (c == '\n')
                    {
                        return line.ToString().TrimEnd('\r');
                    }

                    if (line.Length >= MaxRequestChars)
                    {
                        throw new InvalidDataException("zahtjev je prevelik");
                    }

                    line.Append(c);
                }
            }
        }

        private static string ProcessRequest(string request, string clientAddress)
        {
            try
            {
                string command = request;
                string payload = string.Empty;

                int space = request.IndexOf(' ');

                if (space > 0)
                {
                    command = request.Substring(0, space);
                    payload = request.Substring(space + 1);
                }

                switch (command)
                {
                    case "PING":
                        return "PONG";
                    case "ACTIVITY":
                        return HandleActivity(payload, clientAddress);
                    case "SESSION_LOGS":
                        return HandleSessionLogs(payload, clientAddress);
                    case "GET_ACTIVITY":
                        return HandleGetActivity();
                    case "GET_SESSION_LOG":
                        return HandleGetSessionLog(payload);
                    default:
                        Console.WriteLine("Neprepoznat zahtjev od " + clientAddress + ": " + command);
                        return "ERROR:neprepoznat zahtjev";
                }
            }
            catch (JsonException)
            {
                return "ERROR:neispravan JSON";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška kod obrade zahtjeva: " + ex.Message);
                return "ERROR:greška na serveru";
            }
        }

        private static string HandleActivity(string payload, string clientAddress)
        {
            ActivityRequest request = JsonSerializer.Deserialize<ActivityRequest>(payload, JsonOptions);

            if (request == null)
            {
                return "ERROR:prazan zahtjev";
            }

            string usernameError = ValidateUsername(request.Username);

            if (usernameError != null)
            {
                return "ERROR:" + usernameError;
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return "ERROR:poruka je prazna";
            }

            if (request.Message.Length > MaxMessageLength)
            {
                return "ERROR:poruka je dulja od " + MaxMessageLength + " znakova";
            }

            TeamActivityRecord record = CreateRecord(request.Username, TypeMessage, clientAddress);
            record.Message = request.Message.Trim();

            AppendRecord(record);
            Console.WriteLine("Poruka od " + record.Username + " (" + clientAddress + ")");

            return "OK";
        }

        private static string HandleSessionLogs(string payload, string clientAddress)
        {
            SessionLogsRequest request = JsonSerializer.Deserialize<SessionLogsRequest>(payload, JsonOptions);

            if (request == null)
            {
                return "ERROR:prazan zahtjev";
            }

            string usernameError = ValidateUsername(request.Username);

            if (usernameError != null)
            {
                return "ERROR:" + usernameError;
            }

            if (request.Entries == null || request.Entries.Count == 0)
            {
                return "ERROR:dnevnik je prazan";
            }

            if (request.Entries.Count > MaxSessionEntries)
            {
                return "ERROR:dnevnik ima više od " + MaxSessionEntries + " zapisa";
            }

            int errorCount = 0;

            foreach (SessionLogItem item in request.Entries)
            {
                if (item.IsError)
                {
                    errorCount++;
                }
            }

            TeamActivityRecord record = CreateRecord(request.Username, TypeSessionLogs, clientAddress);
            record.Message = "dnevnik: " + request.Entries.Count + " zapisa, grešaka: " + errorCount;
            record.EntryCount = request.Entries.Count;
            record.ErrorCount = errorCount;
            record.Entries = request.Entries;

            AppendRecord(record);
            Console.WriteLine("Dnevnik od " + record.Username + " (" + clientAddress + "): " + request.Entries.Count + " zapisa");

            return "OK";
        }

        // zapisi bez stavki dnevnika - stavke se dohvaćaju zasebno (GET_SESSION_LOG), da odgovor ostane malen
        private static string HandleGetActivity()
        {
            List<TeamActivityRecord> records = ReadRecords();
            List<TeamActivityRecord> result = new List<TeamActivityRecord>();

            for (int i = records.Count - 1; i >= 0 && result.Count < MaxReturnedRecords; i--)
            {
                TeamActivityRecord summary = records[i];
                summary.Entries = null;
                result.Add(summary);
            }

            return "OK " + JsonSerializer.Serialize(result, JsonOptions);
        }

        private static string HandleGetSessionLog(string payload)
        {
            RecordIdRequest request = JsonSerializer.Deserialize<RecordIdRequest>(payload, JsonOptions);

            if (request == null || string.IsNullOrEmpty(request.Id))
            {
                return "ERROR:nedostaje id zapisa";
            }

            foreach (TeamActivityRecord record in ReadRecords())
            {
                if (record.Id == request.Id)
                {
                    return "OK " + JsonSerializer.Serialize(record, JsonOptions);
                }
            }

            return "ERROR:zapis nije pronađen";
        }

        private static string ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return "nedostaje korisničko ime";
            }

            if (username.Length > MaxUsernameLength)
            {
                return "korisničko ime je predugo";
            }

            return null;
        }

        private static TeamActivityRecord CreateRecord(string username, string type, string clientAddress)
        {
            TeamActivityRecord record = new TeamActivityRecord();
            record.Id = Guid.NewGuid().ToString("N");
            record.TimestampUtc = DateTime.UtcNow;
            record.Username = username.Trim();
            record.Type = type;
            record.ClientAddress = clientAddress;

            return record;
        }

        // ---------------------------------------------------------------
        // Arhiva (JSON Lines - jedan JSON objekt po retku)

        private static void AppendRecord(TeamActivityRecord record)
        {
            string line = JsonSerializer.Serialize(record, JsonOptions);

            lock (ArchiveLock)
            {
                File.AppendAllText(ArchivePath, line + Environment.NewLine, new UTF8Encoding(false));

                _appendsSinceRotation++;

                if (_appendsSinceRotation >= RotationCheckInterval)
                {
                    RotateArchive();
                }
            }
        }

        // neispravni retci (npr. prekinut zapis) se preskaču, ne ruše čitanje cijele arhive
        private static List<TeamActivityRecord> ReadRecords()
        {
            List<TeamActivityRecord> records = new List<TeamActivityRecord>();

            lock (ArchiveLock)
            {
                if (File.Exists(ArchivePath) == false)
                {
                    return records;
                }

                foreach (string line in File.ReadAllLines(ArchivePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        TeamActivityRecord record = JsonSerializer.Deserialize<TeamActivityRecord>(line, JsonOptions);

                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Preskočen neispravan redak arhive");
                    }
                }
            }

            return records;
        }

        // zadržava zadnjih MaxArchiveRecords redaka
        private static void RotateArchive()
        {
            lock (ArchiveLock)
            {
                _appendsSinceRotation = 0;

                if (File.Exists(ArchivePath) == false)
                {
                    return;
                }

                string[] lines = File.ReadAllLines(ArchivePath);

                if (lines.Length <= MaxArchiveRecords)
                {
                    return;
                }

                string[] kept = lines.Skip(lines.Length - MaxArchiveRecords).ToArray();
                string tempPath = ArchivePath + ".tmp";

                File.WriteAllLines(tempPath, kept, new UTF8Encoding(false));
                File.Move(tempPath, ArchivePath, true);

                Console.WriteLine("Arhiva skraćena na " + MaxArchiveRecords + " zapisa");
            }
        }
    }

    // ---------------------------------------------------------------
    // Modeli protokola

    class ActivityRequest
    {
        public string Username { get; set; }
        public string Message { get; set; }
    }

    class SessionLogsRequest
    {
        public string Username { get; set; }
        public List<SessionLogItem> Entries { get; set; }
    }

    class RecordIdRequest
    {
        public string Id { get; set; }
    }

    class SessionLogItem
    {
        public DateTime CreatedAt { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public string Details { get; set; }
        public bool IsError { get; set; }
    }

    class TeamActivityRecord
    {
        public string Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
        public string ClientAddress { get; set; }
        public string Message { get; set; }
        public int EntryCount { get; set; }
        public int ErrorCount { get; set; }

        // null u popisu (GET_ACTIVITY), popunjeno kod GET_SESSION_LOG
        public List<SessionLogItem> Entries { get; set; }
    }
}