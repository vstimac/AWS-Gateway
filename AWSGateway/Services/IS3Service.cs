using AWSGateway.Models;

namespace AWSGateway.Services
{
    // omogućuje testiranje logike koja ovisi o S3 operacijama bez stvarnog AWS poziva
    public interface IS3Service
    {
        Task<List<string>> ListBucketsAsync();

        // regija bucketa (s3:GetBucketLocation); prazan string ako se ne može utvrditi - tada se koristi regija profila
        Task<string> GetBucketRegionAsync(string bucket);

        Task<List<AWSResource>> ListObjectsAsync(string bucket);

        // true ako objekt s tim ključem postoji (HEAD zahtjev)
        Task<bool> ObjectExistsAsync(string bucket, string key);

        // concurrentServiceRequests - broj istovremenih zahtjeva ZA DIJELOVE JEDNE datoteke unutar jednog multipart uploada (TransferUtilityConfig.ConcurrentServiceRequests), 0 = SDK zadano (10)
        Task UploadFileAsync(string filePath, string bucket, string key, CancellationToken token, int partSizeMB = 0, int concurrentServiceRequests = 0, IProgress<TransferProgress> progress = null);

        Task DownloadFileAsync(string bucket, string key, string saveTo, CancellationToken token = default, IProgress<TransferProgress> progress = null);

        Task<string> GetPresignedUrlAsync(string bucket, string key, TimeSpan duration);

        Task DeleteObjectAsync(string bucket, string key);

        // rucna provjera nakon greske/otkazivanja - je li ostao nedovrseni multipart upload za taj kljuc (SDK bi ga trebao sam abortirati)
        Task<List<string>> ListIncompleteMultipartUploadsAsync(string bucket, string key);
    }
}