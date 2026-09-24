using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    // odlučuje cache-hit vs. AWS dohvat i zapisuje IP2 podatke (cache hit/miss, starost, vrijeme odgovora, broj stranica)
    // izdvojeno iz CostForm da bi se moglo unit-testirati bez stvarnog AWS Cost Explorer poziva i bez baze (ICostExplorerService, ICostCache)
    public class CostCacheOrchestrator
    {
        private readonly ICostExplorerService _costService;
        private readonly ICostCache _cache;
        private readonly TimeSpan _freshness;

        public CostCacheOrchestrator(ICostExplorerService costService, TimeSpan freshness)
            : this(costService, new SqliteCostCache(), freshness)
        {
        }

        public CostCacheOrchestrator(ICostExplorerService costService, ICostCache cache, TimeSpan freshness)
        {
            _costService = costService;
            _cache = cache;
            _freshness = freshness;
        }

        // ključ uključuje AWS račun - bez njega bi prijava drugim računom unutar TTL-a prikazala tuđe troškove
        public static string BuildCacheKey(string accountId, DateTime startUtc, DateTime endUtc)
        {
            string account = accountId;

            if (string.IsNullOrEmpty(account))
            {
                account = "nepoznat-racun";
            }

            return "Cost:" + account + ":"
                + startUtc.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ":"
                + endUtc.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ":DAILY";
        }

        public async Task<CostFetchResult> GetCostsAsync(string cacheKey, DateTime start, DateTime end, bool forceRefresh)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            CostCacheEntry cached = null;
            List<CostRow> cachedRows = null;

            if (forceRefresh == false)
            {
                cached = _cache.Get(cacheKey);
                cachedRows = TryReadRows(cached);
            }

            if (cachedRows != null && cached.IsFresh(_freshness))
            {
                double dataAgeSeconds = cached.GetDataAge().TotalSeconds;
                stopwatch.Stop();

                CostFetchResult hitResult = new CostFetchResult();
                hitResult.Rows = cachedRows;
                hitResult.WasCacheHit = true;
                hitResult.DataAgeSeconds = dataAgeSeconds;
                hitResult.ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds;
                hitResult.PageCount = null;
                hitResult.FetchedAtUtc = DateTime.UtcNow.AddSeconds(-dataAgeSeconds);

                _cache.LogAccess(cacheKey, true, dataAgeSeconds, hitResult.ResponseTimeMs, null, _freshness.TotalHours);

                return hitResult;
            }

            List<CostRow> fetchedRows;
            int pageCount;

            try
            {
                (fetchedRows, pageCount) = await _costService.GetCostByServiceAndRegionAsync(start, end);
            }
            catch (Exception ex)
            {
                // AWS nije dostupan - ako postoje istekli podaci u predmemoriji, prikazuju se uz upozorenje umjesto same greške
                // takav prikaz se NE bilježi u IP2 log: nije ni regularan cache hit ni uspješan dohvat
                CostFetchResult staleResult = TryBuildStaleResult(cacheKey, cached, cachedRows, ex, stopwatch);

                if (staleResult != null)
                {
                    return staleResult;
                }

                throw;
            }

            _cache.Set(cacheKey, JsonSerializer.Serialize(fetchedRows));
            stopwatch.Stop();

            CostFetchResult missResult = new CostFetchResult();
            missResult.Rows = fetchedRows;
            missResult.WasCacheHit = false;
            missResult.DataAgeSeconds = 0;
            missResult.ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            missResult.PageCount = pageCount;
            missResult.FetchedAtUtc = DateTime.UtcNow;

            _cache.LogAccess(cacheKey, false, 0, missResult.ResponseTimeMs, pageCount, _freshness.TotalHours);

            return missResult;
        }

        // kod prisilnog osvježavanja predmemorija se nije čitala - čita se tek sad, za rezervni prikaz
        private CostFetchResult TryBuildStaleResult(string cacheKey, CostCacheEntry cached, List<CostRow> cachedRows, Exception error, Stopwatch stopwatch)
        {
            if (cached == null)
            {
                cached = _cache.Get(cacheKey);
                cachedRows = TryReadRows(cached);
            }

            if (cachedRows == null)
            {
                return null;
            }

            double dataAgeSeconds = cached.GetDataAge().TotalSeconds;
            stopwatch.Stop();

            CostFetchResult staleResult = new CostFetchResult();
            staleResult.Rows = cachedRows;
            staleResult.WasCacheHit = true;
            staleResult.DataAgeSeconds = dataAgeSeconds;
            staleResult.ResponseTimeMs = stopwatch.Elapsed.TotalMilliseconds;
            staleResult.PageCount = null;
            staleResult.FetchedAtUtc = DateTime.UtcNow.AddSeconds(-dataAgeSeconds);
            staleResult.IsStale = true;
            staleResult.StaleReason = error.Message;

            return staleResult;
        }

        // neispravan zapis u predmemoriji tretira se kao da ga nema (promašaj), umjesto rušenja forme
        private static List<CostRow> TryReadRows(CostCacheEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.DataJson))
            {
                return null;
            }

            try
            {
                // provjera vremena dohvata - GetDataAge baca iznimku za neispravan FetchedAt
                entry.GetDataAge();

                return JsonSerializer.Deserialize<List<CostRow>>(entry.DataJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Neispravan zapis u predmemoriji troškova: " + ex.Message);
                return null;
            }
        }
    }
}