using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    public class StsService
    {
        // sts:GetCallerIdentity radi bez posebnih S3/EC2 dozvola - koristi se za provjeru vjerodajnica kod prijave (oba nacina)
        public async Task<(string Account, string Arn)> GetCallerIdentityAsync(AWSProfile profile)
        {
            AWSCredentials credentials = AwsCredentialsFactory.Build(profile);
            RegionEndpoint region = RegionEndpoint.GetBySystemName(profile.Region);

            using (AmazonSecurityTokenServiceClient client = new AmazonSecurityTokenServiceClient(credentials, region))
            {
                GetCallerIdentityResponse response = await client.GetCallerIdentityAsync(new GetCallerIdentityRequest());
                return (response.Account, response.Arn);
            }
        }

        // preuzima ulogu koristeci BAZNE (dugotrajne) vjerodajnice; includeDuration=false se koristi za retry ako uloga ne dopusta trazeno trajanje
        public async Task<Credentials> AssumeRoleAsync(AWSProfile baseProfile, string roleArn, string mfaSerial, string mfaCode, string roleSessionName, bool includeDuration)
        {
            BasicAWSCredentials baseCredentials = new BasicAWSCredentials(baseProfile.AccessKey, baseProfile.SecretKey);
            RegionEndpoint region = RegionEndpoint.GetBySystemName(baseProfile.Region);

            using (AmazonSecurityTokenServiceClient client = new AmazonSecurityTokenServiceClient(baseCredentials, region))
            {
                AssumeRoleRequest request = new AssumeRoleRequest();
                request.RoleArn = roleArn;
                request.RoleSessionName = roleSessionName;

                if (includeDuration)
                {
                    request.DurationSeconds = 3600;
                }

                if (string.IsNullOrWhiteSpace(mfaSerial) == false)
                {
                    request.SerialNumber = mfaSerial;
                }

                if (string.IsNullOrWhiteSpace(mfaCode) == false)
                {
                    request.TokenCode = mfaCode;
                }

                AssumeRoleResponse response = await client.AssumeRoleAsync(request);
                return response.Credentials;
            }
        }

        // RoleSessionName mora sadrzavati samo slova, brojke i +=,.@_- , max 64 znaka (AWS ogranicenje)
        public static string BuildRoleSessionName(string username, DateTime utcNow)
        {
            string safeUsername = ReplaceInvalidChars(username);
            string timestamp = utcNow.ToString("yyyyMMddHHmmss");
            string sessionName = "AWSGateway-" + safeUsername + "-" + timestamp;

            if (sessionName.Length > 64)
            {
                sessionName = sessionName.Substring(0, 64);
            }

            return sessionName;
        }

        private static string ReplaceInvalidChars(string input)
        {
            string safeInput = input;

            if (safeInput == null)
            {
                safeInput = string.Empty;
            }

            StringBuilder result = new StringBuilder(safeInput.Length);

            foreach (char c in safeInput)
            {
                bool isValid = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ||
                               c == '+' || c == '=' || c == ',' || c == '.' || c == '@' || c == '_' || c == '-';

                if (isValid)
                {
                    result.Append(c);
                }
                else
                {
                    result.Append('-');
                }
            }

            return result.ToString();
        }
    }
}
