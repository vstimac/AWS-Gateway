using AWSGateway.Models;

namespace AWSGateway.Services
{
    // omogućuje testiranje cache/orkestracijske logike bez stvarnog AWS Cost Explorer poziva
    public interface ICostExplorerService
    {
        // dnevni trošak po usluzi i regiji za period [start, end), bez AWS kredita i povrata; vraća i broj stranica (IP2)
        Task<(List<CostRow> Rows, int PageCount)> GetCostByServiceAndRegionAsync(DateTime start, DateTime end);
    }
}