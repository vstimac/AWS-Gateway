using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AWSGateway.Models;
using System.Diagnostics;
using System.Net;

namespace AWSGateway.Services
{
    public class S3Service : IS3Service, IDisposable
    {
        private AWSProfile _profile;
        private AWSCredentials _credentials;

        // klijent u regiji profila - za ListBuckets i utvrđivanje regije bucketa
        private AmazonS3Client _client;

        // regija svakog bucketa (prazan string = nije utvrđena, koristi se regija profila) i po jedan klijent za svaku regiju
        // bucket se mora adresirati u svojoj regiji - posebno važno za presigned poveznice koje se potpisuju lokalno, bez mrežnog poziva
        private Dictionary<string, string> _bucketRegions = new Dictionary<string, string>();
        private Dictionary<string, AmazonS3Client> _regionClients = new Dictionary<string, AmazonS3Client>();
        private object _cacheLock = new object();

        public S3Service(AWSProfile profile)
        {
            _profile = profile;
            _credentials = AwsCredentialsFactory.Build(profile);

            _client = new AmazonS3Client(_credentials, RegionEndpoint.GetBySystemName(profile.Region));
            _regionClients[profile.Region] = _client;
        }

        // list buckets - traži dozvolu s3:ListAllMyBuckets koju uloge ograničene na jedan bucket često nemaju
        public async Task<List<string>> ListBucketsAsync()
        {
            List<string> names = new List<string>();

            try
            {
                ListBucketsResponse response = await _client.ListBucketsAsync();

                // bez bucketa SDK kolekciju može vratiti kao null
                if (response.Buckets != null)
                {
                    foreach (S3Bucket x in response.Buckets)
                    {
                        names.Add(x.BucketName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod dohvata bucketa: " + ex.Message, ex);
            }

            return names;
        }

        public async Task<string> GetBucketRegionAsync(string bucket)
        {
            lock (_cacheLock)
            {
                string cachedRegion;

                if (_bucketRegions.TryGetValue(bucket, out cachedRegion))
                {
                    return cachedRegion;
                }
            }

            string region = string.Empty;
            bool accessDenied = false;

            try
            {
                region = await RequestBucketLocationAsync(_client, bucket);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod utvrđivanja regije bucketa " + bucket + " (regija profila): " + ex.Message);
                accessDenied = IsAccessDenied(ex);

                // zahtjev iz regije profila može završiti preusmjeravanjem - drugi pokušaj ide preko globalnog us-east-1 endpointa
                if (accessDenied == false)
                {
                    try
                    {
                        using (AmazonS3Client globalClient = new AmazonS3Client(_credentials, RegionEndpoint.USEast1))
                        {
                            region = await RequestBucketLocationAsync(globalClient, bucket);
                        }
                    }
                    catch (Exception globalEx)
                    {
                        Debug.WriteLine("Greška kod utvrđivanja regije bucketa " + bucket + " (us-east-1): " + globalEx.Message);
                        accessDenied = IsAccessDenied(globalEx);
                    }
                }
            }

            // uspjeh ili nedostatak dozvole se pamte; ostale greške (mreža, istekla sesija) ne, da se kod sljedeće operacije pokuša ponovno
            if (string.IsNullOrEmpty(region) == false || accessDenied)
            {
                lock (_cacheLock)
                {
                    _bucketRegions[bucket] = region;
                }
            }

            return region;
        }

        private async Task<string> RequestBucketLocationAsync(AmazonS3Client client, string bucket)
        {
            GetBucketLocationRequest request = new GetBucketLocationRequest();
            request.BucketName = bucket;

            GetBucketLocationResponse response = await client.GetBucketLocationAsync(request);

            string location = string.Empty;

            if (response.Location != null)
            {
                location = response.Location.Value;
            }

            return NormalizeBucketLocation(location);
        }

        private bool IsAccessDenied(Exception ex)
        {
            AmazonS3Exception s3Exception = ex as AmazonS3Exception;

            if (s3Exception == null)
            {
                return false;
            }

            return s3Exception.StatusCode == HttpStatusCode.Forbidden;
        }

        // GetBucketLocation vraća prazno za us-east-1 i zastarjeli "EU" za eu-west-1
        private string NormalizeBucketLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return "us-east-1";
            }

            if (location == "EU")
            {
                return "eu-west-1";
            }

            return location;
        }

        // klijent u regiji bucketa; ako regija nije utvrđena, klijent u regiji profila
        private async Task<AmazonS3Client> GetClientForBucketAsync(string bucket)
        {
            string region = await GetBucketRegionAsync(bucket);

            if (string.IsNullOrEmpty(region))
            {
                region = _profile.Region;
            }

            lock (_cacheLock)
            {
                AmazonS3Client client;

                if (_regionClients.TryGetValue(region, out client) == false)
                {
                    client = new AmazonS3Client(_credentials, RegionEndpoint.GetBySystemName(region));
                    _regionClients[region] = client;
                }

                return client;
            }
        }

        public async Task<List<AWSResource>> ListObjectsAsync(string bucket)
        {
            List<AWSResource> resources = new List<AWSResource>();

            try
            {
                AmazonS3Client client = await GetClientForBucketAsync(bucket);
                string region = client.Config.RegionEndpoint.SystemName;
                string continuationToken = null;

                // paginacija - jedan poziv vraća max 1000 ključeva, ponavljamo dok ima još stranica
                do
                {
                    ListObjectsV2Request request = new ListObjectsV2Request();
                    request.BucketName = bucket;
                    request.ContinuationToken = continuationToken;

                    ListObjectsV2Response response = await client.ListObjectsV2Async(request);

                    // prazan bucket - SDK kolekciju može vratiti kao null
                    if (response.S3Objects != null)
                    {
                        foreach (S3Object x in response.S3Objects)
                        {
                            AWSResource resource = new AWSResource();
                            resource.ResourceId = x.Key;
                            resource.Type = AWSResource.ResourceType.S3Object;
                            resource.Name = x.Key;
                            resource.Region = region;
                            resource.Status = "available";
                            resource.Size = x.Size ?? 0;
                            resource.CreatedAt = x.LastModified ?? DateTime.MinValue;

                            // klasa pohrane dolazi uz popis objekata bez dodatnog poziva
                            if (x.StorageClass != null && string.IsNullOrEmpty(x.StorageClass.Value) == false)
                            {
                                resource.StorageClass = x.StorageClass.Value;
                            }
                            else
                            {
                                resource.StorageClass = "STANDARD";
                            }

                            resources.Add(resource);
                        }
                    }

                    if (response.IsTruncated == true)
                    {
                        continuationToken = response.NextContinuationToken;
                    }
                    else
                    {
                        continuationToken = null;
                    }
                } while (continuationToken != null);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod dohvata objekata: " + ex.Message, ex);
            }

            return resources;
        }

        // HEAD zahtjev - 404 znači da objekt ne postoji
        // NAPOMENA: bez dozvole s3:ListBucket AWS za nepostojeći objekt vraća 403 umjesto 404, pa se tada baca iznimka
        public async Task<bool> ObjectExistsAsync(string bucket, string key)
        {
            try
            {
                AmazonS3Client client = await GetClientForBucketAsync(bucket);

                GetObjectMetadataRequest request = new GetObjectMetadataRequest();
                request.BucketName = bucket;
                request.Key = key;

                await client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw new Exception("Greška kod provjere objekta: " + ex.Message, ex);
            }
        }

        // bez try/catch jer imamo CancellationToken - pozivatelj razlikuje otkazivanje (OperationCanceledException) od greške
        // partSizeMB - veličina dijela za multipart upload u MB; 0 = SDK sam odlučuje (zadano ponašanje)
        // concurrentServiceRequests - broj istovremenih zahtjeva za dijelove JEDNE datoteke unutar ovog uploada; 0 = SDK zadano (10)
        // TransferUtilityConfig se prima kroz konstruktor TransferUtility-ja, pa se za svaki poziv stvara nova instanca s tocno zadanim postavkama
        public async Task UploadFileAsync(string filePath, string bucket, string key, CancellationToken token, int partSizeMB = 0, int concurrentServiceRequests = 0, IProgress<TransferProgress> progress = null)
        {
            AmazonS3Client client = await GetClientForBucketAsync(bucket);

            TransferUtilityConfig config = new TransferUtilityConfig();

            if (concurrentServiceRequests > 0)
            {
                config.ConcurrentServiceRequests = concurrentServiceRequests;
            }

            // TransferUtility stvoren s postojećim klijentom ne zatvara taj klijent kod Dispose
            using (TransferUtility utility = new TransferUtility(client, config))
            {
                TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();
                request.BucketName = bucket;
                request.FilePath = filePath;
                request.Key = key;

                if (partSizeMB > 0)
                {
                    request.PartSize = partSizeMB * 1024L * 1024L;
                }

                if (progress != null)
                {
                    ProgressThrottle throttle = new ProgressThrottle(progress);
                    request.UploadProgressEvent += (sender, args) => throttle.Report(args.TransferredBytes, args.TotalBytes);
                }

                await utility.UploadAsync(request, token);
            }
        }

        // rucna provjera nakon greske/otkazivanja - trazi nedovrsene multipart uploade za dani kljuc
        // (TransferUtility bi trebao sam pozvati abort kod greske/otkazivanja - ovo je provjera da je stvarno tako)
        public async Task<List<string>> ListIncompleteMultipartUploadsAsync(string bucket, string key)
        {
            List<string> uploadIds = new List<string>();

            try
            {
                AmazonS3Client client = await GetClientForBucketAsync(bucket);

                ListMultipartUploadsRequest request = new ListMultipartUploadsRequest();
                request.BucketName = bucket;
                request.Prefix = key;

                ListMultipartUploadsResponse response = await client.ListMultipartUploadsAsync(request);

                if (response.MultipartUploads != null)
                {
                    foreach (MultipartUpload upload in response.MultipartUploads)
                    {
                        if (upload.Key == key)
                        {
                            uploadIds.Add(upload.UploadId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod provjere nedovršenih multipart uploada: " + ex.Message, ex);
            }

            return uploadIds;
        }

        // preuzimanje ide u privremenu datoteku koja se tek na kraju premješta na odredište -
        // postojeća datoteka ostaje netaknuta ako download padne ili se otkaže, a nepotpuna datoteka ne ostaje na disku
        public async Task DownloadFileAsync(string bucket, string key, string saveTo, CancellationToken token = default, IProgress<TransferProgress> progress = null)
        {
            string tempPath = saveTo + ".download";

            try
            {
                AmazonS3Client client = await GetClientForBucketAsync(bucket);

                using (TransferUtility utility = new TransferUtility(client))
                {
                    TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest();
                    request.BucketName = bucket;
                    request.Key = key;
                    request.FilePath = tempPath;

                    if (progress != null)
                    {
                        ProgressThrottle throttle = new ProgressThrottle(progress);
                        request.WriteObjectProgressEvent += (sender, args) => throttle.Report(args.TransferredBytes, args.TotalBytes);
                    }

                    await utility.DownloadAsync(request, token);
                }

                File.Move(tempPath, saveTo, true);
            }
            catch (OperationCanceledException)
            {
                DeleteTempFile(tempPath);
                throw;
            }
            catch (Exception ex)
            {
                DeleteTempFile(tempPath);
                throw new Exception("Greška kod downloada: " + ex.Message, ex);
            }
        }

        private void DeleteTempFile(string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod brisanja privremene datoteke: " + ex.Message);
            }
        }

        // generira vremenski ograničenu (presigned) poveznicu - potpisuje se lokalno klijentom u regiji bucketa
        public async Task<string> GetPresignedUrlAsync(string bucket, string key, TimeSpan duration)
        {
            try
            {
                AmazonS3Client client = await GetClientForBucketAsync(bucket);

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
                request.BucketName = bucket;
                request.Key = key;
                request.Expires = DateTime.UtcNow.Add(duration);

                return client.GetPreSignedURL(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod generiranja poveznice: " + ex.Message, ex);
            }
        }

        public async Task DeleteObjectAsync(string bucket, string key)
        {
            try
            {
                AmazonS3Client client = await GetClientForBucketAsync(bucket);

                DeleteObjectRequest request = new DeleteObjectRequest();
                request.BucketName = bucket;
                request.Key = key;

                await client.DeleteObjectAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod brisanja: " + ex.Message, ex);
            }
        }

        public void Dispose()
        {
            lock (_cacheLock)
            {
                // _client je također u rječniku, pa se zatvara u istoj petlji
                foreach (AmazonS3Client client in _regionClients.Values)
                {
                    client.Dispose();
                }

                _regionClients.Clear();
            }
        }

        // SDK javlja napredak vrlo često i s više dretvi (dijelovi idu paralelno) -
        // prosljeđuje se samo kad postotak naraste, da se UI ne zatrpa porukama i da napredak ne ide unatrag
        private class ProgressThrottle
        {
            private IProgress<TransferProgress> _progress;
            private object _lock = new object();
            private int _lastPercent = -1;

            public ProgressThrottle(IProgress<TransferProgress> progress)
            {
                _progress = progress;
            }

            public void Report(long transferredBytes, long totalBytes)
            {
                TransferProgress value = new TransferProgress(transferredBytes, totalBytes);
                int percent = value.GetPercent();

                lock (_lock)
                {
                    if (percent <= _lastPercent)
                    {
                        return;
                    }

                    _lastPercent = percent;
                }

                _progress.Report(value);
            }
        }
    }
}