using System;
using System.Globalization;

namespace AWSGateway.Models
{
    // jedan zapis lokalnog cachea - podaci + trenutak kada su ti podaci dohvaćeni s AWS-a
    public class CostCacheEntry
    {
        public string CacheKey { get; set; }
        public string DataJson { get; set; }
        public string FetchedAt { get; set; }

        public CostCacheEntry()
        {
            CacheKey = string.Empty;
            DataJson = string.Empty;
            FetchedAt = string.Empty;
        }

        // DataAge = koliko je vremena prošlo od trenutka dohvata podataka s AWS-a (FetchedAt), ne od trenutka čitanja iz cachea
        // NAPOMENA: DateTimeStyles.RoundtripKind se izbjegava jer string s eksplicitnim offsetom (npr. "+02:00")
        // pretvara u Kind=Local umjesto u UTC. AdjustToUniversal|AssumeUniversal jamci Kind=Utc za sve oblike zapisa:
        // sa "Z", s eksplicitnim offsetom i bez oznake zone (bez oznake se pretpostavlja da je vec UTC).
        public TimeSpan GetDataAge()
        {
            DateTime fetchedAtUtc = DateTime.Parse(
                FetchedAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

            return DateTime.UtcNow - fetchedAtUtc;
        }

        public bool IsFresh(TimeSpan maxAge)
        {
            return GetDataAge() <= maxAge;
        }
    }
}
