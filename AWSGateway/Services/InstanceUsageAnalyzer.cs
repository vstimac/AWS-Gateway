using AWSGateway.Helpers;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    // procjena opterećenja EC2 instance iz CloudWatch točaka - čista računska klasa bez AWS poziva,
    // pa se može unit-testirati sintetičkim podacima (npr. 288 točaka s CPU 2 % mora dati razinu Idle)
    public class InstanceUsageAnalyzer
    {
        // neaktivna instanca - pragovi se oslanjaju na kriterij AWS Trusted Advisora za slabo iskorištene instance (CPU do 10 %, mreža do 5 MB dnevno)
        public const double IdleCpuAverageMax = 10.0;
        public const double IdleCpuMaximumMax = 30.0;
        public const long IdleNetworkBytesPerDayMax = 5L * 1024 * 1024;

        // granice razina opterećenja prema prosječnom CPU-u
        public const double LowCpuAverageMax = 20.0;
        public const double ModerateCpuAverageMax = 70.0;

        // pragovi za upozorenja
        public const double SpikeCpuMaximumMin = 90.0;
        public const double SpikeCpuAverageMax = 40.0;
        public const double LowCreditBalanceMax = 20.0;
        public const double PartialCoveragePercentMax = 80.0;

        // ispod ovog vremena rada procjena nije pouzdana (kod koraka od 5 min to je 6 točaka)
        public const int MinimumCoveredSeconds = 30 * 60;

        private const double SecondsPerDay = 86400.0;

        public InstanceUsageReport Analyze(List<EC2MetricPoint> points, TimeSpan lookback, int periodSeconds)
        {
            if (periodSeconds <= 0)
            {
                throw new ArgumentException("Korak mjerenja mora biti veći od 0 sekundi.", nameof(periodSeconds));
            }

            InstanceUsageReport report = new InstanceUsageReport();
            report.Lookback = lookback;
            report.PeriodSeconds = periodSeconds;
            report.ExpectedIntervalCount = (int)Math.Round(lookback.TotalSeconds / periodSeconds);

            double cpuAverageSum = 0;
            double networkInSum = 0;
            double networkOutSum = 0;
            double creditMinimum = double.MaxValue;

            foreach (EC2MetricPoint point in points)
            {
                switch (point.MetricName)
                {
                    case EC2Service.MetricCpuUtilization:
                        report.CpuIntervalCount++;
                        cpuAverageSum += point.Average;
                        if (point.Maximum > report.CpuMaximum)
                        {
                            report.CpuMaximum = point.Maximum;
                        }
                        break;
                    case EC2Service.MetricNetworkIn:
                        networkInSum += point.Sum;
                        break;
                    case EC2Service.MetricNetworkOut:
                        networkOutSum += point.Sum;
                        break;
                    case EC2Service.MetricCpuCreditBalance:
                        report.HasCreditData = true;
                        if (point.Minimum < creditMinimum)
                        {
                            creditMinimum = point.Minimum;
                        }
                        break;
                    case EC2Service.MetricStatusCheckFailed:
                        if (point.Maximum > 0)
                        {
                            report.StatusCheckFailedCount++;
                        }
                        break;
                }
            }

            report.NetworkInBytes = (long)networkInSum;
            report.NetworkOutBytes = (long)networkOutSum;

            // prosjek intervala - svi intervali su jednako dugi, pa je prosjek prosjeka ispravan
            if (report.CpuIntervalCount > 0)
            {
                report.CpuAverage = cpuAverageSum / report.CpuIntervalCount;
            }

            if (report.HasCreditData)
            {
                report.CpuCreditMinimum = creditMinimum;
            }

            if (report.ExpectedIntervalCount > 0)
            {
                double coverage = report.CpuIntervalCount * 100.0 / report.ExpectedIntervalCount;

                // CloudWatch zna vratiti jednu točku više na rubu perioda
                if (coverage > 100)
                {
                    coverage = 100;
                }

                report.CoveragePercent = coverage;
            }

            // vrijeme stvarnog rada = broj intervala s CPU podacima × korak
            long coveredSeconds = (long)report.CpuIntervalCount * periodSeconds;

            if (coveredSeconds > 0)
            {
                report.NetworkBytesPerDay = (long)(report.GetTotalNetworkBytes() * SecondsPerDay / coveredSeconds);
            }

            report.Level = DetermineLevel(report, coveredSeconds);
            AddWarnings(report);

            return report;
        }

        // pravila se provjeravaju redom - prvo koje vrijedi određuje razinu
        private InstanceUsageReport.UsageLevel DetermineLevel(InstanceUsageReport report, long coveredSeconds)
        {
            if (coveredSeconds < MinimumCoveredSeconds)
            {
                return InstanceUsageReport.UsageLevel.InsufficientData;
            }

            if (report.CpuAverage <= IdleCpuAverageMax
                && report.CpuMaximum < IdleCpuMaximumMax
                && report.NetworkBytesPerDay <= IdleNetworkBytesPerDayMax)
            {
                return InstanceUsageReport.UsageLevel.Idle;
            }

            if (report.CpuAverage < LowCpuAverageMax)
            {
                return InstanceUsageReport.UsageLevel.Low;
            }

            if (report.CpuAverage <= ModerateCpuAverageMax)
            {
                return InstanceUsageReport.UsageLevel.Moderate;
            }

            return InstanceUsageReport.UsageLevel.High;
        }

        // upozorenja se računaju neovisno o razini - pokrivaju slučajeve koje prosjek sakrije
        private void AddWarnings(InstanceUsageReport report)
        {
            if (report.Level != InstanceUsageReport.UsageLevel.InsufficientData
                && report.CoveragePercent < PartialCoveragePercentMax)
            {
                report.Warnings.Add(LanguageHelper.Format("ec2_warning_partial_coverage", report.CoveragePercent.ToString("F0")));
            }

            if (report.CpuIntervalCount > 0
                && report.CpuMaximum >= SpikeCpuMaximumMin
                && report.CpuAverage < SpikeCpuAverageMax)
            {
                report.Warnings.Add(LanguageHelper.Format("ec2_warning_cpu_spike", report.CpuMaximum.ToString("F1"), report.CpuAverage.ToString("F1")));
            }

            if (report.HasCreditData && report.CpuCreditMinimum < LowCreditBalanceMax)
            {
                report.Warnings.Add(LanguageHelper.Format("ec2_warning_low_credits", report.CpuCreditMinimum.ToString("F1")));
            }

            if (report.StatusCheckFailedCount > 0)
            {
                report.Warnings.Add(LanguageHelper.Format("ec2_warning_status_check_failed", report.StatusCheckFailedCount));
            }
        }
    }
}