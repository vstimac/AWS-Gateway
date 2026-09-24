using AWSGateway.Helpers;

namespace AWSGateway.Models
{
    // rezultat analize CloudWatch metrika jedne EC2 instance - izračunate vrijednosti, procjena i upozorenja
    public class InstanceUsageReport
    {
        public enum UsageLevel
        {
            InsufficientData, Idle, Low, Moderate, High
        }

        public TimeSpan Lookback { get; set; }
        public int PeriodSeconds { get; set; }

        // pokrivenost - koliko intervala s CPU podacima postoji u odnosu na broj intervala u periodu
        public int ExpectedIntervalCount { get; set; }
        public int CpuIntervalCount { get; set; }
        public double CoveragePercent { get; set; }

        public double CpuAverage { get; set; }
        public double CpuMaximum { get; set; }

        public long NetworkInBytes { get; set; }
        public long NetworkOutBytes { get; set; }

        // mrežni promet preračunat na jedan dan STVARNOG rada instance (ne na cijeli period)
        public long NetworkBytesPerDay { get; set; }

        // CPUCreditBalance postoji samo za burstable (t) instance
        public bool HasCreditData { get; set; }
        public double CpuCreditMinimum { get; set; }

        // broj intervala u kojima provjera zdravlja instance nije uspjela
        public int StatusCheckFailedCount { get; set; }

        public UsageLevel Level { get; set; }
        public List<string> Warnings { get; set; }

        public InstanceUsageReport()
        {
            Lookback = TimeSpan.Zero;
            PeriodSeconds = 0;
            ExpectedIntervalCount = 0;
            CpuIntervalCount = 0;
            CoveragePercent = 0;
            CpuAverage = 0;
            CpuMaximum = 0;
            NetworkInBytes = 0;
            NetworkOutBytes = 0;
            NetworkBytesPerDay = 0;
            HasCreditData = false;
            CpuCreditMinimum = 0;
            StatusCheckFailedCount = 0;
            Level = UsageLevel.InsufficientData;
            Warnings = new List<string>();
        }

        public long GetTotalNetworkBytes()
        {
            return NetworkInBytes + NetworkOutBytes;
        }

        public string GetLevelTitle()
        {
            switch (Level)
            {
                case UsageLevel.Idle:
                    return LanguageHelper.Get("ec2_level_idle");
                case UsageLevel.Low:
                    return LanguageHelper.Get("ec2_level_low");
                case UsageLevel.Moderate:
                    return LanguageHelper.Get("ec2_level_moderate");
                case UsageLevel.High:
                    return LanguageHelper.Get("ec2_level_high");
                default:
                    return LanguageHelper.Get("ec2_level_insufficient");
            }
        }

        public string GetDescription()
        {
            switch (Level)
            {
                case UsageLevel.Idle:
                    return LanguageHelper.Get("ec2_desc_idle");
                case UsageLevel.Low:
                    return LanguageHelper.Get("ec2_desc_low");
                case UsageLevel.Moderate:
                    return LanguageHelper.Get("ec2_desc_moderate");
                case UsageLevel.High:
                    return LanguageHelper.Get("ec2_desc_high");
                default:
                    return LanguageHelper.Get("ec2_desc_insufficient");
            }
        }

        public string GetRecommendation()
        {
            switch (Level)
            {
                case UsageLevel.Idle:
                    return LanguageHelper.Get("ec2_rec_idle");
                case UsageLevel.Low:
                    return LanguageHelper.Get("ec2_rec_low");
                case UsageLevel.Moderate:
                    return LanguageHelper.Get("ec2_rec_moderate");
                case UsageLevel.High:
                    return LanguageHelper.Get("ec2_rec_high");
                default:
                    return LanguageHelper.Get("ec2_rec_insufficient");
            }
        }
    }
}