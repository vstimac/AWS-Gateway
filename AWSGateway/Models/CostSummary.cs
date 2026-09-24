namespace AWSGateway.Models
{
    public class ServiceCostTotal
    {
        public string Service { get; set; }
        public double Amount { get; set; }
        public double SharePercent { get; set; }

        public ServiceCostTotal()
        {
            Service = string.Empty;
        }
    }

    public class RegionCostTotal
    {
        public string Region { get; set; }
        public double Amount { get; set; }

        public RegionCostTotal()
        {
            Region = string.Empty;
        }
    }

    public class DailyCostTotal
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
    }

    // sažetak troškova za period - izračunava ga CostSummaryCalculator
    public class CostSummary
    {
        public double Total { get; set; }
        public string Unit { get; set; }

        // null ako usporedni podaci nisu dostupni
        public double? ComparisonTotal { get; set; }

        // null ako usporedba nije moguća (nema podataka ili je usporedni trošak 0)
        public double? ChangePercent { get; set; }

        // usluge sortirane po iznosu, bez usluga s iznosom 0
        public List<ServiceCostTotal> Services { get; set; }

        // po jedan zapis za svaki dan perioda, i za dane bez troška
        public List<DailyCostTotal> Days { get; set; }

        // izvorni retci - za raspodjelu odabrane usluge po regijama
        public List<CostRow> Rows { get; set; }

        public CostSummary()
        {
            Unit = "USD";
            Services = new List<ServiceCostTotal>();
            Days = new List<DailyCostTotal>();
            Rows = new List<CostRow>();
        }

        // null ako nema troškova
        public ServiceCostTotal GetTopService()
        {
            if (Services.Count == 0)
            {
                return null;
            }

            return Services[0];
        }
    }
}