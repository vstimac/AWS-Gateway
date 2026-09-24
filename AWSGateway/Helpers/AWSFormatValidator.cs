using System.Text.RegularExpressions;

namespace AWSGateway.Helpers
{
    // provjera formata vrijednosti koje korisnik upisuje - samo oblik, stvarnu ispravnost potvrđuje AWS kod prijave
    public static class AwsFormatValidator
    {
        // AKIA = dugotrajni ključ IAM korisnika, ASIA = privremeni (STS) ključ
        private static readonly Regex AccessKeyPattern = new Regex("^(AKIA|ASIA)[A-Z0-9]{16}$");

        // arn:aws:iam::123456789012:role/naziv (uloga smije imati putanju, npr. role/tim/naziv)
        private static readonly Regex RoleArnPattern = new Regex(@"^arn:aws[a-z\-]*:iam::\d{12}:role/[\w+=,.@/\-]+$");

        // virtualni MFA uređaj je ARN (arn:aws:iam::123456789012:mfa/naziv), hardverski ima serijski broj - AWS dopušta 9-256 znakova
        private static readonly Regex MfaSerialPattern = new Regex(@"^[\w+=/:,.@\-]{9,256}$");

        // 3-63 znaka, mala slova, brojke, točka i crtica, počinje i završava slovom ili brojkom
        private static readonly Regex BucketNamePattern = new Regex("^[a-z0-9][a-z0-9.\\-]{1,61}[a-z0-9]$");

        public const int MaxProfileNameLength = 50;

        public static bool IsValidAccessKeyId(string value)
        {
            return string.IsNullOrEmpty(value) == false && AccessKeyPattern.IsMatch(value);
        }

        public static bool IsValidRoleArn(string value)
        {
            return string.IsNullOrEmpty(value) == false && RoleArnPattern.IsMatch(value);
        }

        public static bool IsValidMfaSerial(string value)
        {
            return string.IsNullOrEmpty(value) == false && MfaSerialPattern.IsMatch(value);
        }

        public static bool IsValidBucketName(string value)
        {
            if (string.IsNullOrEmpty(value) || BucketNamePattern.IsMatch(value) == false)
            {
                return false;
            }

            // dvije točke zaredom nisu dopuštene
            return value.Contains("..") == false;
        }
    }
}