using Amazon.Runtime;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    // jedno mjesto koje odlucuje koje vjerodajnice koristiti - izbjegava triplirani if/else u S3Service/EC2Service/CostExplorerService
    public static class AwsCredentialsFactory
    {
        public static AWSCredentials Build(AWSProfile profile)
        {
            if (profile.IsTemporarySession())
            {
                return new SessionAWSCredentials(profile.TempAccessKeyId, profile.TempSecretAccessKey, profile.SessionToken);
            }

            return new BasicAWSCredentials(profile.AccessKey, profile.SecretKey);
        }
    }
}
