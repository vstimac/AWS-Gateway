using AWSGateway.Data;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    // predmemorija troškova - sučelje omogućuje testiranje CostCacheOrchestrator logike bez stvarne baze
    public interface ICostCache
    {
        // null ako zapis ne postoji
        CostCacheEntry Get(string cacheKey);

        void Set(string cacheKey, string dataJson);

        // IP2 - zapis jednog dohvaćanja
        void LogAccess(string cacheKey, bool wasCacheHit, double? dataAgeSeconds, double responseTimeMs, int? pageCount, double ttlHours);
    }

    // predmemorija u SQLite bazi aplikacije (tablice CostCache i CostCacheAccessLog)
    public class SqliteCostCache : ICostCache
    {
        public CostCacheEntry Get(string cacheKey)
        {
            return DatabaseManager.Instance.GetCacheEntry(cacheKey);
        }

        public void Set(string cacheKey, string dataJson)
        {
            DatabaseManager.Instance.SetCacheEntry(cacheKey, dataJson);
        }

        public void LogAccess(string cacheKey, bool wasCacheHit, double? dataAgeSeconds, double responseTimeMs, int? pageCount, double ttlHours)
        {
            DatabaseManager.Instance.LogCacheAccess(cacheKey, wasCacheHit, dataAgeSeconds, responseTimeMs, pageCount, ttlHours);
        }
    }
}