namespace AWSGateway.Models
{
    // rezultat jednog dohvaćanja troškova - uz podatke, nosi IP2 metapodatke (cache hit/miss, starost, vrijeme odgovora)
    public class CostFetchResult
    {
        public List<CostRow> Rows { get; set; }
        public bool WasCacheHit { get; set; }
        public double DataAgeSeconds { get; set; }
        public double ResponseTimeMs { get; set; }
        public int? PageCount { get; set; }

        // trenutak kad su podaci dohvaćeni s AWS-a
        public DateTime FetchedAtUtc { get; set; }

        // true ako AWS poziv nije uspio pa su prikazani istekli podaci iz predmemorije
        public bool IsStale { get; set; }
        public string StaleReason { get; set; }

        public CostFetchResult()
        {
            Rows = new List<CostRow>();
            FetchedAtUtc = DateTime.UtcNow;
            IsStale = false;
            StaleReason = string.Empty;
        }
    }
}