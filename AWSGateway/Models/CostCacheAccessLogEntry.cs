namespace AWSGateway.Models
{
    // IP2 - jedan zapis loga pristupa predmemoriji troskova (odgovara redu u CostCacheAccessLog)
    public class CostCacheAccessLogEntry
    {
        public int Id { get; set; }
        public string CacheKey { get; set; }
        public bool WasCacheHit { get; set; }
        public double? DataAgeSeconds { get; set; }
        public double ResponseTimeMs { get; set; }
        public int? PageCount { get; set; }
        public double TtlHoursAtLog { get; set; }
        public string RequestedAt { get; set; }

        public CostCacheAccessLogEntry()
        {
            CacheKey = string.Empty;
            RequestedAt = string.Empty;
        }
    }
}
