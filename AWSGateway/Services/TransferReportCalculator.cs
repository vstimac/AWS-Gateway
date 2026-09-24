using AWSGateway.Models;

namespace AWSGateway.Services
{
    // računa sažetak iz istih redaka koji su prikazani u tablici - sažetak, PDF i CSV tako uvijek odgovaraju filtru
    // čista računska klasa bez baze, pa se može unit-testirati
    public class TransferReportCalculator
    {
        public const string StatusCompleted = "completed";
        public const string StatusFailed = "failed";
        public const string StatusCancelled = "cancelled";

        public TransferReportSummary Calculate(List<TransferModels> transfers)
        {
            TransferReportSummary summary = new TransferReportSummary();

            long speedBytes = 0;
            double speedMs = 0;
            Dictionary<string, int> errorCounts = new Dictionary<string, int>();

            foreach (TransferModels transfer in transfers)
            {
                summary.TotalCount++;

                switch (transfer.Status)
                {
                    case StatusCompleted:
                        summary.CompletedCount++;
                        AddCompletedVolume(summary, transfer);

                        // prosječna brzina je ukupno preneseno / ukupno trajanje, a ne prosjek pojedinačnih brzina -
                        // inače bi mala datoteka s velikom brzinom vukla prosjek jednako kao velika
                        if (transfer.DurationMs.HasValue && transfer.DurationMs.Value > 0 && transfer.BytesTransferred > 0)
                        {
                            speedBytes += transfer.BytesTransferred;
                            speedMs += transfer.DurationMs.Value;
                        }
                        break;
                    case StatusFailed:
                        summary.FailedCount++;
                        CountError(errorCounts, transfer.ErrorMessage);
                        break;
                    case StatusCancelled:
                        summary.CancelledCount++;
                        break;
                }
            }

            if (speedMs > 0)
            {
                double megabytes = speedBytes / 1024.0 / 1024.0;
                summary.AverageSpeedMBps = megabytes / (speedMs / 1000.0);
            }

            foreach (KeyValuePair<string, int> pair in errorCounts)
            {
                if (pair.Value > summary.MostCommonErrorCount)
                {
                    summary.MostCommonError = pair.Key;
                    summary.MostCommonErrorCount = pair.Value;
                }
            }

            return summary;
        }

        private void AddCompletedVolume(TransferReportSummary summary, TransferModels transfer)
        {
            if (transfer.Direction == "upload")
            {
                summary.UploadedBytes += transfer.BytesTransferred;
            }
            else if (transfer.Direction == "download")
            {
                summary.DownloadedBytes += transfer.BytesTransferred;
            }
        }

        // stari zapisi nemaju razlog greške - ne ulaze u "najčešću grešku"
        private void CountError(Dictionary<string, int> errorCounts, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return;
            }

            int count;

            if (errorCounts.TryGetValue(errorMessage, out count))
            {
                errorCounts[errorMessage] = count + 1;
            }
            else
            {
                errorCounts[errorMessage] = 1;
            }
        }
    }
}