using AWSGateway.Helpers;

namespace AWSGateway.Models
{
    // period troškova [Start, End) u UTC danima (Cost Explorer računa po UTC danima; End je isključiv)
    // uz odabrani period nosi i usporedni period iste duljine
    public class CostPeriod
    {
        public string Label { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }

        public string ComparisonLabel { get; set; }
        public DateTime ComparisonStartUtc { get; set; }
        public DateTime ComparisonEndUtc { get; set; }

        // tekući mjesec do danas uključivo; usporedba s istim brojem dana prošlog mjeseca
        public static CostPeriod ThisMonth(DateTime todayUtc)
        {
            DateTime today = todayUtc.Date;
            DateTime start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            CostPeriod period = new CostPeriod();
            period.Label = LanguageHelper.Get("cost_period_this_month_label");
            period.StartUtc = start;
            period.EndUtc = today.AddDays(1);

            int days = (period.EndUtc - start).Days;
            DateTime comparisonStart = start.AddMonths(-1);
            DateTime comparisonEnd = comparisonStart.AddDays(days);

            // prošli mjesec može biti kraći (npr. 31. u mjesecu nakon mjeseca od 30 dana)
            if (comparisonEnd > start)
            {
                comparisonEnd = start;
            }

            period.ComparisonLabel = LanguageHelper.Get("cost_period_this_month_comparison");
            period.ComparisonStartUtc = comparisonStart;
            period.ComparisonEndUtc = comparisonEnd;

            return period;
        }

        // cijeli prošli mjesec; usporedba s mjesecom prije njega
        public static CostPeriod LastMonth(DateTime todayUtc)
        {
            DateTime thisMonthStart = new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            CostPeriod period = new CostPeriod();
            period.Label = LanguageHelper.Get("cost_period_last_month_label");
            period.StartUtc = thisMonthStart.AddMonths(-1);
            period.EndUtc = thisMonthStart;
            period.ComparisonLabel = LanguageHelper.Get("cost_period_last_month_comparison");
            period.ComparisonStartUtc = thisMonthStart.AddMonths(-2);
            period.ComparisonEndUtc = thisMonthStart.AddMonths(-1);

            return period;
        }

        // zadnjih 30 dana uključujući danas; usporedba s 30 dana prije toga
        public static CostPeriod Last30Days(DateTime todayUtc)
        {
            DateTime end = todayUtc.Date.AddDays(1);

            CostPeriod period = new CostPeriod();
            period.Label = LanguageHelper.Get("cost_period_last_30_days_label");
            period.StartUtc = end.AddDays(-30);
            period.EndUtc = end;
            period.ComparisonLabel = LanguageHelper.Get("cost_period_last_30_days_comparison");
            period.ComparisonStartUtc = end.AddDays(-60);
            period.ComparisonEndUtc = end.AddDays(-30);

            return period;
        }
    }
}