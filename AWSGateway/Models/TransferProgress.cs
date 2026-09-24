namespace AWSGateway.Models
{
    // napredak jednog prijenosa (upload ili download) - preneseni i ukupni bajtovi
    public class TransferProgress
    {
        public long TransferredBytes { get; set; }
        public long TotalBytes { get; set; }

        public TransferProgress(long transferredBytes, long totalBytes)
        {
            TransferredBytes = transferredBytes;
            TotalBytes = totalBytes;
        }

        public int GetPercent()
        {
            if (TotalBytes <= 0)
            {
                return 0;
            }

            long percent = TransferredBytes * 100 / TotalBytes;

            if (percent > 100)
            {
                return 100;
            }

            return (int)percent;
        }
    }
}