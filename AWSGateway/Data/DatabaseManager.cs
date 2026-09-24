using AWSGateway.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text;

namespace AWSGateway.Data
{
    // Users - TransferHistory - povezane preko UserId.
    public class DatabaseManager
    {
        // singleton
        private static readonly DatabaseManager _instance = new DatabaseManager();

        public static DatabaseManager Instance
        {
            get { return _instance; }
        }

        // format vremena u TransferHistory - UTC, isti kao SQLite CURRENT_TIMESTAMP kojim su pisani stari zapisi,
        // pa se stari i novi zapisi mogu uspoređivati kao tekst u WHERE uvjetu
        public const string DbTimestampFormat = "yyyy-MM-dd HH:mm:ss";

        // razlog greške se skraćuje - poruke AWS iznimki znaju biti vrlo duge
        private const int MaxErrorMessageLength = 500;

        // lookup Username; COALESCE jer stari zapisi nemaju nove stupce
        private const string TransferSelect = @"
            SELECT
                th.Id,
                th.Direction,
                COALESCE(th.SourcePath, '') AS SourcePath,
                COALESCE(th.DestPath, '') AS DestPath,
                COALESCE(th.Bucket, '') AS Bucket,
                COALESCE(th.ObjectKey, '') AS ObjectKey,
                COALESCE(th.LocalPath, '') AS LocalPath,
                COALESCE(th.FileSizeBytes, th.BytesTransferred, 0) AS FileSizeBytes,
                COALESCE(th.BytesTransferred, 0) AS BytesTransferred,
                th.DurationMs,
                th.Status,
                COALESCE(th.ErrorMessage, '') AS ErrorMessage,
                th.CreatedAt,
                COALESCE(th.ProfileName, '') AS ProfileName,
                COALESCE(th.AwsIdentity, '') AS AwsIdentity,
                COALESCE(u.Username, '') AS Username
            FROM TransferHistory th
            LEFT JOIN Users u ON th.UserId = u.Id";

        private string _connectionString;

        private DatabaseManager()
        {
            string dbPath = Helpers.AppSettings.Instance.DatabasePath;
            _connectionString = "Data Source=" + dbPath;

            InitializeDatabase();
        }

        private SqliteConnection OpenConnection()
        {
            SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public int Execute(string sql, object parameters = null)
        {
            using (SqliteConnection connection = OpenConnection())
            {
                return connection.Execute(sql, parameters);
            }
        }

        private void InitializeDatabase()
        {
            CreateUsersTable();
            CreateTransfersTable();
            CreateCostCacheTable();
            CreateCacheAccessLogTable();

            // migracija za baze koje su vec postojale prije ove izmjene - CREATE TABLE IF NOT EXISTS ne dodaje nove stupce
            EnsureColumnExists("CostCacheAccessLog", "TtlHoursAtLog", "REAL NOT NULL DEFAULT 0");

            EnsureColumnExists("TransferHistory", "Bucket", "TEXT");
            EnsureColumnExists("TransferHistory", "ObjectKey", "TEXT");
            EnsureColumnExists("TransferHistory", "LocalPath", "TEXT");
            EnsureColumnExists("TransferHistory", "FileSizeBytes", "INTEGER");
            EnsureColumnExists("TransferHistory", "DurationMs", "REAL");
            EnsureColumnExists("TransferHistory", "ErrorMessage", "TEXT");
            EnsureColumnExists("TransferHistory", "AwsIdentity", "TEXT");

            MigrateOldTransfers();
        }

        // provjerava PRAGMA table_info i dodaje stupac ako ne postoji (ALTER TABLE ADD COLUMN)
        private void EnsureColumnExists(string tableName, string columnName, string columnDefinition)
        {
            using (SqliteConnection connection = OpenConnection())
            {
                IEnumerable<dynamic> columns = connection.Query("PRAGMA table_info(" + tableName + ")");

                bool exists = false;

                foreach (dynamic column in columns)
                {
                    IDictionary<string, object> columnRow = column;
                    string existingName = columnRow["name"].ToString();

                    if (string.Equals(existingName, columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists == false)
                {
                    connection.Execute("ALTER TABLE " + tableName + " ADD COLUMN " + columnName + " " + columnDefinition);
                }
            }
        }

        // stari zapisi imaju S3 stranu kao "bucket/ključ" u DestPath (upload) ili SourcePath (download) -
        // razdvaja se u nove stupce da i stara povijest radi s filtrom po bucketu; WHERE Bucket IS NULL čini migraciju ponovljivom
        private void MigrateOldTransfers()
        {
            Execute(@"UPDATE TransferHistory SET
                        Bucket = substr(DestPath, 1, instr(DestPath, '/') - 1),
                        ObjectKey = substr(DestPath, instr(DestPath, '/') + 1),
                        LocalPath = SourcePath
                      WHERE Bucket IS NULL AND Direction = 'upload' AND instr(DestPath, '/') > 0");

            Execute(@"UPDATE TransferHistory SET
                        Bucket = substr(SourcePath, 1, instr(SourcePath, '/') - 1),
                        ObjectKey = substr(SourcePath, instr(SourcePath, '/') + 1),
                        LocalPath = DestPath
                      WHERE Bucket IS NULL AND Direction = 'download' AND instr(SourcePath, '/') > 0");
        }

        private void CreateUsersTable()
        {
            Execute(@"CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        Avatar BLOB)");
        }

        // CreatedAt je UTC (CURRENT_TIMESTAMP i zapis iz aplikacije koriste isti format)
        private void CreateTransfersTable()
        {
            Execute(@"CREATE TABLE IF NOT EXISTS TransferHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        ProfileName TEXT,
                        Direction TEXT NOT NULL,
                        SourcePath TEXT,
                        DestPath TEXT,
                        BytesTransferred INTEGER DEFAULT 0,
                        Status TEXT DEFAULT 'pending',
                        CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                        Bucket TEXT,
                        ObjectKey TEXT,
                        LocalPath TEXT,
                        FileSizeBytes INTEGER,
                        DurationMs REAL,
                        ErrorMessage TEXT,
                        AwsIdentity TEXT,
                        FOREIGN KEY (UserId) REFERENCES Users(Id))");
        }

        // ključ upita (npr. "Cost:2026-08:eu-north-1") - podaci - vrijeme dohvaćanja s AWS-a (za DataAge)
        private void CreateCostCacheTable()
        {
            Execute(@"CREATE TABLE IF NOT EXISTS CostCache (
                        CacheKey TEXT PRIMARY KEY,
                        DataJson TEXT NOT NULL,
                        FetchedAt TEXT NOT NULL)");
        }

        // IP2 - log svakog dohvaćanja podataka o troškovima (cache hit/miss, starost, vrijeme odgovora)
        private void CreateCacheAccessLogTable()
        {
            Execute(@"CREATE TABLE IF NOT EXISTS CostCacheAccessLog (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CacheKey TEXT NOT NULL,
                        WasCacheHit INTEGER NOT NULL,
                        DataAgeSeconds REAL,
                        ResponseTimeMs REAL NOT NULL,
                        PageCount INTEGER,
                        TtlHoursAtLog REAL NOT NULL DEFAULT 0,
                        RequestedAt TEXT DEFAULT CURRENT_TIMESTAMP)");
        }

        // ---------------------------------------------------------------
        // CostCache

        // vraća cache zapis ako postoji, inače null
        public CostCacheEntry GetCacheEntry(string cacheKey)
        {
            using (SqliteConnection connection = OpenConnection())
            {
                return connection.QueryFirstOrDefault<CostCacheEntry>(
                    "SELECT CacheKey, DataJson, FetchedAt FROM CostCache WHERE CacheKey = @CacheKey",
                    new { CacheKey = cacheKey });
            }
        }

        // upisuje/ažurira cache zapis, FetchedAt = trenutak dohvata s AWS-a
        public void SetCacheEntry(string cacheKey, string dataJson)
        {
            Execute(@"INSERT INTO CostCache (CacheKey, DataJson, FetchedAt)
                       VALUES (@CacheKey, @DataJson, @FetchedAt)
                       ON CONFLICT(CacheKey) DO UPDATE SET DataJson = @DataJson, FetchedAt = @FetchedAt",
                new { CacheKey = cacheKey, DataJson = dataJson, FetchedAt = DateTime.UtcNow.ToString("O") });
        }

        // IP2 - zapisuje jedno dohvaćanje (cache hit/miss, starost podataka, vrijeme odgovora, TTL koji je tada vrijedio)
        public void LogCacheAccess(string cacheKey, bool wasCacheHit, double? dataAgeSeconds, double responseTimeMs, int? pageCount, double ttlHours)
        {
            int wasCacheHitValue;

            if (wasCacheHit)
            {
                wasCacheHitValue = 1;
            }
            else
            {
                wasCacheHitValue = 0;
            }

            Execute(@"INSERT INTO CostCacheAccessLog (CacheKey, WasCacheHit, DataAgeSeconds, ResponseTimeMs, PageCount, TtlHoursAtLog)
                       VALUES (@CacheKey, @WasCacheHit, @DataAgeSeconds, @ResponseTimeMs, @PageCount, @TtlHoursAtLog)",
                new { CacheKey = cacheKey, WasCacheHit = wasCacheHitValue, DataAgeSeconds = dataAgeSeconds, ResponseTimeMs = responseTimeMs, PageCount = pageCount, TtlHoursAtLog = ttlHours });
        }

        // IP2 - svi zapisi loga predmemorije, za CSV izvoz
        public List<CostCacheAccessLogEntry> GetAllCacheAccessLog()
        {
            using (SqliteConnection connection = OpenConnection())
            {
                return connection.Query<CostCacheAccessLogEntry>(
                    "SELECT * FROM CostCacheAccessLog ORDER BY RequestedAt DESC").ToList();
            }
        }

        // ---------------------------------------------------------------
        // Users

        // CREATE
        public void RegisterUser(string username, string password)
        {
            if (UserExists(username))
            {
                throw new Exception("Korisnik već postoji.");
            }

            DateTime registrationDate = DateTime.Now;
            string salt = Helpers.CryptoHelper.GenerateSalt(username, registrationDate);
            string passwordHash = Helpers.CryptoHelper.HashPassword(password, salt);

            string sql = @"INSERT INTO Users (Username, PasswordHash, CreatedAt) 
                            VALUES (@Username, @PasswordHash, @CreatedAt)";

            Execute(sql, new
            {
                Username = username,
                PasswordHash = passwordHash,
                CreatedAt = registrationDate.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        // READ
        public bool UserExists(string username)
        {
            using (SqliteConnection connection = OpenConnection())
            {
                // executescalar uzme prvu vrijednost
                int count = connection.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Users WHERE Username = @Username",
                    new { Username = username });

                return count > 0;
            }
        }

        // READ Id
        public int GetUserId(string username)
        {
            using (SqliteConnection connection = OpenConnection())
            {
                return connection.ExecuteScalar<int>(
                    "SELECT Id FROM Users WHERE Username = @Username",
                    new { Username = username });
            }
        }

        private class UserLoginData
        {
            public string PasswordHash { get; set; }
            public string CreatedAt { get; set; }
        }

        // READ 
        public bool VerifyUserPassword(string username, string password)
        {
            UserLoginData user;

            using (SqliteConnection connection = OpenConnection())
            {
                user = connection.QueryFirstOrDefault<UserLoginData>(
                    "SELECT PasswordHash, CreatedAt FROM Users WHERE Username = @Username",
                    new { Username = username });
            }

            if (user == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.CreatedAt))
            {
                return false;
            }

            DateTime registrationDate = DateTime.Parse(user.CreatedAt);
            string salt = Helpers.CryptoHelper.GenerateSalt(username, registrationDate);

            return Helpers.CryptoHelper.VerifyPassword(password, user.PasswordHash, salt);
        }

        // READ 
        public byte[] GetUserAvatar(string username)
        {
            using (SqliteConnection connection = OpenConnection())
            {
                return connection.ExecuteScalar<byte[]>(
                    "SELECT Avatar FROM Users WHERE Username = @Username",
                    new { Username = username });
            }
        }

        // UPDATE 
        public void SaveAvatar(string username, byte[] avatarData)
        {
            Execute("UPDATE Users SET Avatar = @Avatar WHERE Username = @Username",
                new { Avatar = avatarData, Username = username });
        }

        // DELETE
        public void DeleteUser(string username)
        {
            int userId = GetUserId(username);

            if (userId == 0)
            {
                return;
            }

            Execute("DELETE FROM TransferHistory WHERE UserId = @UserId", new { UserId = userId });
            Execute("DELETE FROM Users WHERE Id = @Id", new { Id = userId });
        }

        // ---------------------------------------------------------------
        // TransferHistory

        // CREATE - zapis o jednom prijenosu; transfer.Username određuje korisnika
        public void SaveTransfer(TransferModels transfer)
        {
            int userId = GetUserId(transfer.Username);

            if (userId == 0)
            {
                throw new InvalidOperationException("Korisnik za transfer nije pronađen.");
            }

            // SourcePath/DestPath se i dalje pune - smjer određuje koja je strana izvor, a koja odredište
            string s3Side = transfer.Bucket + "/" + transfer.ObjectKey;
            string sourcePath;
            string destPath;

            if (transfer.Direction == "upload")
            {
                sourcePath = transfer.LocalPath;
                destPath = s3Side;
            }
            else
            {
                sourcePath = s3Side;
                destPath = transfer.LocalPath;
            }

            string errorMessage = transfer.ErrorMessage;

            if (errorMessage != null && errorMessage.Length > MaxErrorMessageLength)
            {
                errorMessage = errorMessage.Substring(0, MaxErrorMessageLength);
            }

            string sql = @"INSERT INTO TransferHistory
                            (UserId, ProfileName, Direction, SourcePath, DestPath, BytesTransferred, Status, CreatedAt,
                             Bucket, ObjectKey, LocalPath, FileSizeBytes, DurationMs, ErrorMessage, AwsIdentity)
                           VALUES
                            (@UserId, @ProfileName, @Direction, @SourcePath, @DestPath, @BytesTransferred, @Status, @CreatedAt,
                             @Bucket, @ObjectKey, @LocalPath, @FileSizeBytes, @DurationMs, @ErrorMessage, @AwsIdentity)";

            Execute(sql, new
            {
                UserId = userId,
                transfer.ProfileName,
                transfer.Direction,
                SourcePath = sourcePath,
                DestPath = destPath,
                transfer.BytesTransferred,
                transfer.Status,
                CreatedAt = DateTime.UtcNow.ToString(DbTimestampFormat, CultureInfo.InvariantCulture),
                transfer.Bucket,
                transfer.ObjectKey,
                transfer.LocalPath,
                transfer.FileSizeBytes,
                transfer.DurationMs,
                ErrorMessage = errorMessage,
                transfer.AwsIdentity
            });
        }

        // READ - filter se primjenjuje u SQL-u, svi uvjeti su parametrizirani
        public List<TransferModels> GetTransfers(TransferReportFilter filter)
        {
            StringBuilder sql = new StringBuilder(TransferSelect);
            sql.Append(" WHERE 1 = 1");

            DynamicParameters parameters = new DynamicParameters();

            if (filter.FromUtc.HasValue)
            {
                sql.Append(" AND th.CreatedAt >= @FromUtc");
                parameters.Add("FromUtc", filter.FromUtc.Value.ToString(DbTimestampFormat, CultureInfo.InvariantCulture));
            }

            if (string.IsNullOrEmpty(filter.Direction) == false)
            {
                sql.Append(" AND th.Direction = @Direction");
                parameters.Add("Direction", filter.Direction);
            }

            if (string.IsNullOrEmpty(filter.Status) == false)
            {
                sql.Append(" AND th.Status = @Status");
                parameters.Add("Status", filter.Status);
            }

            if (string.IsNullOrEmpty(filter.Bucket) == false)
            {
                sql.Append(" AND th.Bucket = @Bucket");
                parameters.Add("Bucket", filter.Bucket);
            }

            if (string.IsNullOrWhiteSpace(filter.SearchText) == false)
            {
                // pretraga po S3 ključu i lokalnoj putanji
                sql.Append(" AND (th.ObjectKey LIKE @Search OR th.LocalPath LIKE @Search OR th.SourcePath LIKE @Search OR th.DestPath LIKE @Search)");
                parameters.Add("Search", "%" + filter.SearchText.Trim() + "%");
            }

            if (string.IsNullOrEmpty(filter.Username) == false)
            {
                sql.Append(" AND u.Username = @Username");
                parameters.Add("Username", filter.Username);
            }

            sql.Append(" ORDER BY th.CreatedAt DESC, th.Id DESC");

            using (SqliteConnection connection = OpenConnection())
            {
                return connection.Query<TransferModels>(sql.ToString(), parameters).ToList();
            }
        }

        // popis bucketa za filter - samo bucketi koji se pojavljuju u povijesti (trenutnog korisnika ili svih)
        public List<string> GetTransferBuckets(string username)
        {
            string sql = @"SELECT DISTINCT th.Bucket
                           FROM TransferHistory th
                           LEFT JOIN Users u ON th.UserId = u.Id
                           WHERE th.Bucket IS NOT NULL AND th.Bucket <> ''";

            if (string.IsNullOrEmpty(username) == false)
            {
                sql = sql + " AND u.Username = @Username";
            }

            sql = sql + " ORDER BY th.Bucket";

            using (SqliteConnection connection = OpenConnection())
            {
                return connection.Query<string>(sql, new { Username = username }).ToList();
            }
        }

        // DELETE - jedan ili više zapisa odjednom
        public void DeleteTransfers(List<int> transferIds)
        {
            if (transferIds.Count == 0)
            {
                return;
            }

            // Dapper listu pretvara u IN (@Ids1, @Ids2, ...)
            Execute("DELETE FROM TransferHistory WHERE Id IN @Ids", new { Ids = transferIds });
        }
    }
}