using AWSGateway.Data;
using AWSGateway.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace AWSGateway.Services
{
    // klijent za AWS Gateway TCP server (osnovna verzija - server na ovom računalu)
    // protokol: jedan redak zahtjeva "NAREDBA {json}", jedan redak odgovora "OK [json]" ili "ERROR:razlog"
    public class TcpClientService
    {
        // 127.0.0.1, a ne "localhost" - server sluša na IPv4 loopbacku, a localhost se može najprije razriješiti u IPv6 (::1)
        public const string DefaultHost = "127.0.0.1";
        public const int DefaultPort = 5555;

        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(10);

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // proces servera koji je pokrenula ova aplikacija - null ako je server već radio od prije
        private static Process _startedServer;

        // ---------------------------------------------------------------
        // Job Object - gasi server i kad se ova aplikacija ugasi bez izvršavanja StopStartedServer (rušenje, "Stop" u Visual Studiju)
        // u job se dodaje SAMO proces servera, nikad glavna aplikacija - inače bi se ugasili i njeni podprocesi (npr. Explorer iz "Otvori mapu")

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(IntPtr hJob, int jobObjectInfoClass, ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        private const int JobObjectExtendedLimitInformation = 9;
        private const uint JobObjectLimitKillOnJobClose = 0x2000;

        // drži se za cijeli životni vijek aplikacije i nikad se ručno ne zatvara - Windows ga zatvara (i time gasi server) kad ovaj proces završi na bilo koji način
        private static IntPtr _jobHandle = IntPtr.Zero;

        // dodaje proces servera u Job Object s KILL_ON_JOB_CLOSE; neuspjeh se samo bilježi - aplikacija ne smije pasti zbog ovoga
        private static void AssignServerToJobObject(Process serverProcess)
        {
            try
            {
                if (_jobHandle == IntPtr.Zero)
                {
                    _jobHandle = CreateJobObject(IntPtr.Zero, null);

                    if (_jobHandle == IntPtr.Zero)
                    {
                        Debug.WriteLine("Greška kod stvaranja Job Objecta za TCP server (kod " + Marshal.GetLastWin32Error() + ")");
                        return;
                    }

                    JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                    info.BasicLimitInformation.LimitFlags = JobObjectLimitKillOnJobClose;

                    uint length = (uint)Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));

                    if (SetInformationJobObject(_jobHandle, JobObjectExtendedLimitInformation, ref info, length) == false)
                    {
                        Debug.WriteLine("Greška kod postavljanja Job Object ograničenja (kod " + Marshal.GetLastWin32Error() + ")");
                    }
                }

                if (AssignProcessToJobObject(_jobHandle, serverProcess.Handle) == false)
                {
                    Debug.WriteLine("Greška kod dodjele TCP servera Job Objectu (kod " + Marshal.GetLastWin32Error() + ")");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod Job Objecta za TCP server: " + ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // Pokretanje i status servera

        public async Task<bool> IsServerAvailableAsync()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                using (CancellationTokenSource timeout = new CancellationTokenSource(ConnectTimeout))
                {
                    await client.ConnectAsync(DefaultHost, DefaultPort, timeout.Token);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ako TCP server nije dostupan, pokreće AWSGateway.TCPServer.exe kao skriveni pozadinski proces
        // bez argumenata - server tada sluša samo na ovom računalu; uredski način (--lan) pokreće se isključivo ručno
        public async Task<bool> EnsureServerRunningAsync()
        {
            if (await IsServerAvailableAsync())
            {
                return true;
            }

            try
            {
                string exePath = Path.Combine(Application.StartupPath, "AWSGateway.TCPServer.exe");

                if (File.Exists(exePath) == false)
                {
                    return false;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(exePath);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                _startedServer = Process.Start(startInfo);

                AssignServerToJobObject(_startedServer);

                // kratko čekanje da server podigne listener prije prve provjere statusa
                await Task.Delay(800);

                return await IsServerAvailableAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod pokretanja TCP servera: " + ex.Message);
                return false;
            }
        }

        // gasi TCP server samo ako ga je pokrenula ova aplikacija (poziva se iz Program.cs nakon zatvaranja aplikacije)
        public static void StopStartedServer()
        {
            if (_startedServer == null)
            {
                return;
            }

            try
            {
                if (_startedServer.HasExited == false)
                {
                    _startedServer.Kill();
                    _startedServer.WaitForExit(2000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod gašenja TCP servera: " + ex.Message);
            }
            finally
            {
                _startedServer.Dispose();
                _startedServer = null;
            }
        }

        // vraća vrijeme odziva u milisekundama; baca iznimku ako server ne odgovori ispravno
        public async Task<long> PingAsync()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string response = await SendLineAsync("PING");

            stopwatch.Stop();

            if (response != "PONG")
            {
                throw new Exception("Neočekivan odgovor servera: " + response);
            }

            return stopwatch.ElapsedMilliseconds;
        }

        // ---------------------------------------------------------------
        // Aktivnost tima

        public async Task SendActivityAsync(string username, string message)
        {
            object payload = new { username = username, message = message };

            await SendRequestAsync("ACTIVITY", payload);
        }

        public async Task SendSessionLogsAsync(string username, List<JsonLogManager.ActivityLogEntry> entries)
        {
            object payload = new { username = username, entries = entries };

            await SendRequestAsync("SESSION_LOGS", payload);
        }

        // zapisi bez stavki dnevnika, najnoviji prvi
        public async Task<List<TeamActivityEntry>> GetTeamActivityAsync()
        {
            string json = await SendRequestAsync("GET_ACTIVITY", null);

            List<TeamActivityEntry> entries = JsonSerializer.Deserialize<List<TeamActivityEntry>>(json, JsonOptions);

            if (entries == null)
            {
                return new List<TeamActivityEntry>();
            }

            return entries;
        }

        // jedan dnevnik sa svim stavkama
        public async Task<TeamActivityEntry> GetSessionLogAsync(string id)
        {
            object payload = new { id = id };

            string json = await SendRequestAsync("GET_SESSION_LOG", payload);

            return JsonSerializer.Deserialize<TeamActivityEntry>(json, JsonOptions);
        }

        // ---------------------------------------------------------------
        // Protokol

        // šalje "NAREDBA {json}", vraća dio odgovora iza "OK " (prazan string za samo "OK"); ERROR odgovor postaje iznimka
        // JsonSerializer escapea prijelome redaka u tekstu, pa zahtjev uvijek ostaje u jednom retku
        private async Task<string> SendRequestAsync(string command, object payload)
        {
            string request = command;

            if (payload != null)
            {
                request = command + " " + JsonSerializer.Serialize(payload, JsonOptions);
            }

            string response = await SendLineAsync(request);

            if (response.StartsWith("ERROR:"))
            {
                throw new Exception("Server je odbio zahtjev: " + response.Substring(6));
            }

            if (response == "OK")
            {
                return string.Empty;
            }

            if (response.StartsWith("OK "))
            {
                return response.Substring(3);
            }

            throw new Exception("Neočekivan odgovor servera: " + response);
        }

        private async Task<string> SendLineAsync(string line)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    using (CancellationTokenSource connectTimeout = new CancellationTokenSource(ConnectTimeout))
                    {
                        await client.ConnectAsync(DefaultHost, DefaultPort, connectTimeout.Token);
                    }

                    using (NetworkStream stream = client.GetStream())
                    using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false)))
                    using (CancellationTokenSource readTimeout = new CancellationTokenSource(ReadTimeout))
                    {
                        writer.AutoFlush = true;

                        await writer.WriteLineAsync(line);

                        string response = await reader.ReadLineAsync(readTimeout.Token);

                        if (response == null)
                        {
                            throw new Exception("Server je zatvorio vezu bez odgovora");
                        }

                        return response;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw new Exception("Server nije odgovorio na vrijeme");
            }
            catch (SocketException ex)
            {
                throw new Exception("Server nije dostupan (" + DefaultHost + ":" + DefaultPort + "): " + ex.Message, ex);
            }
        }
    }
}