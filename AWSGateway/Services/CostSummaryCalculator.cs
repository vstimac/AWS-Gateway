using System.Globalization;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    // računa sažetak troškova iz dnevnih redaka - čista računska klasa bez AWS poziva i baze, može se unit-testirati
    public class CostSummaryCalculator
    {
        // iznosi manji od ovoga su praktički nula (Cost Explorer vraća i usluge s 0.0000000001 USD)
        public const double MinVisibleAmount = 0.0000005;

        // comparisonRows smije biti null (usporedni podaci nisu dostupni)
        public CostSummary Calculate(List<CostRow> rows, List<CostRow> comparisonRows, DateTime startUtc, DateTime endUtc)
        {
            CostSummary summary = new CostSummary();
            summary.Rows = rows;

            Dictionary<string, double> serviceAmounts = new Dictionary<string, double>();
            Dictionary<string, double> dailyAmounts = new Dictionary<string, double>();

            foreach (CostRow row in rows)
            {
                summary.Total += row.Amount;

                if (string.IsNullOrEmpty(row.Unit) == false)
                {
                    summary.Unit = row.Unit;
                }

                AddAmount(serviceAmounts, row.Service, row.Amount);
                AddAmount(dailyAmounts, row.Date, row.Amount);
            }

            foreach (KeyValuePair<string, double> pair in serviceAmounts)
            {
                if (Math.Abs(pair.Value) < MinVisibleAmount)
                {
                    continue;
                }

                ServiceCostTotal service = new ServiceCostTotal();
                service.Service = pair.Key;
                service.Amount = pair.Value;

                if (summary.Total > 0)
                {
                    service.SharePercent = pair.Value / summary.Total * 100.0;
                }

                summary.Services.Add(service);
            }

            summary.Services.Sort(CompareByAmountDescending);

            // svaki dan perioda dobiva zapis, i dani bez troška - graf tako ima kontinuiranu vremensku os
            for (DateTime day = startUtc.Date; day < endUtc.Date; day = day.AddDays(1))
            {
                DailyCostTotal dailyTotal = new DailyCostTotal();
                dailyTotal.Date = day;

                double amount;

                if (dailyAmounts.TryGetValue(day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), out amount))
                {
                    dailyTotal.Amount = amount;
                }

                summary.Days.Add(dailyTotal);
            }

            if (comparisonRows != null)
            {
                double comparisonTotal = 0;

                foreach (CostRow row in comparisonRows)
                {
                    comparisonTotal += row.Amount;
                }

                summary.ComparisonTotal = comparisonTotal;

                if (comparisonTotal >= MinVisibleAmount)
                {
                    summary.ChangePercent = (summary.Total - comparisonTotal) / comparisonTotal * 100.0;
                }
            }

            return summary;
        }

        // raspodjela jedne usluge po regijama, sortirano po iznosu
        public List<RegionCostTotal> GetRegionTotals(CostSummary summary, string service)
        {
            Dictionary<string, double> regionAmounts = new Dictionary<string, double>();

            foreach (CostRow row in summary.Rows)
            {
                if (row.Service == service)
                {
                    AddAmount(regionAmounts, row.Region, row.Amount);
                }
            }

            List<RegionCostTotal> regions = new List<RegionCostTotal>();

            foreach (KeyValuePair<string, double> pair in regionAmounts)
            {
                if (Math.Abs(pair.Value) < MinVisibleAmount)
                {
                    continue;
                }

                RegionCostTotal region = new RegionCostTotal();
                region.Region = pair.Key;
                region.Amount = pair.Value;
                regions.Add(region);
            }

            regions.Sort(CompareRegionsByAmountDescending);

            return regions;
        }

        private static void AddAmount(Dictionary<string, double> amounts, string key, double amount)
        {
            if (key == null)
            {
                key = string.Empty;
            }

            double current;

            if (amounts.TryGetValue(key, out current))
            {
                amounts[key] = current + amount;
            }
            else
            {
                amounts[key] = amount;
            }
        }

        private static int CompareByAmountDescending(ServiceCostTotal a, ServiceCostTotal b)
        {
            return b.Amount.CompareTo(a.Amount);
        }

        private static int CompareRegionsByAmountDescending(RegionCostTotal a, RegionCostTotal b)
        {
            return b.Amount.CompareTo(a.Amount);
        }
    }
}