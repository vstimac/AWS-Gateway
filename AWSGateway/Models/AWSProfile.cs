using AWSGateway.Helpers;

namespace AWSGateway.Models
{
    public class AWSProfile
    {
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        public string DefaultBucket { get; set; }

        // AssumeRole prijava - ARN uloge i MFA serijski broj nisu tajni, mogu se spremiti (XmlProfileManager)
        public string RoleArn { get; set; }
        public string MfaSerialNumber { get; set; }

        // privremene (STS) vjerodajnice - NIKAD se ne spremaju na disk (vidi XmlProfileManager.Add/Update)
        public string TempAccessKeyId { get; set; }
        public string TempSecretAccessKey { get; set; }
        public string SessionToken { get; set; }
        public DateTime? SessionExpiresAtUtc { get; set; }

        // rezultat sts:GetCallerIdentity - samo za prikaz, ne za autorizaciju
        public string AwsAccountId { get; set; }
        public string AwsArn { get; set; }

        // korisnik aplikacije kojem profil pripada; prazno = stariji zapis bez vlasnika (vidljiv svima dok ga netko ne spremi)
        public string Owner { get; set; }

        // true ako spremljeni access key nije moguće dešifrirati (DPAPI je vezan uz Windows korisnika i računalo) - ne sprema se
        public bool AccessKeyUnreadable { get; set; }

        public AWSProfile()
        {
            ProfileId = 0;
            ProfileName = string.Empty;
            AccessKey = string.Empty;
            SecretKey = string.Empty;
            Region = "eu-north-1";
            DefaultBucket = string.Empty;
            RoleArn = string.Empty;
            MfaSerialNumber = string.Empty;
            TempAccessKeyId = string.Empty;
            TempSecretAccessKey = string.Empty;
            SessionToken = string.Empty;
            SessionExpiresAtUtc = null;
            AwsAccountId = string.Empty;
            AwsArn = string.Empty;
            AccessKeyUnreadable = false;
            Owner = string.Empty;
        }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(AccessKey) || AccessKey.Length != 20)
            {
                return false;
            }

            // AKIA = dugotrajni IAM korisnicki kljuc, ASIA = privremeni (STS) kljuc - oba su valjani format za bazne vjerodajnice
            if (!AccessKey.StartsWith("AKIA") && !AccessKey.StartsWith("ASIA"))
            {
                return false;
            }

            if (string.IsNullOrEmpty(SecretKey) || SecretKey.Length < 40)
            {
                return false;
            }

            if (string.IsNullOrEmpty(Region))
            {
                return false;
            }

            return true;
        }

        // true ako je prijava preuzimanjem uloge (AssumeRole) - koriste se privremene vjerodajnice s istekom
        public bool IsTemporarySession()
        {
            return string.IsNullOrEmpty(SessionToken) == false;
        }

        // spremljeni profil za prijavu ulogom (za razliku od IsTemporarySession koji govori o aktivnoj sesiji)
        public bool UsesAssumeRole()
        {
            return string.IsNullOrEmpty(RoleArn) == false;
        }

        // preostalo vrijeme sesije; TimeSpan.Zero ako nije privremena sesija ili je vec istekla
        public TimeSpan GetRemainingSessionTime()
        {
            if (IsTemporarySession() == false || SessionExpiresAtUtc.HasValue == false)
            {
                return TimeSpan.Zero;
            }

            TimeSpan remaining = SessionExpiresAtUtc.Value - DateTime.UtcNow;

            if (remaining < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            return remaining;
        }

        public bool HasOwner()
        {
            return string.IsNullOrEmpty(Owner) == false;
        }

        // vlasnik se uspoređuje točno, kao i korisničko ime u bazi
        public bool IsVisibleTo(string username)
        {
            if (HasOwner() == false)
            {
                return true;
            }

            return string.Equals(Owner, username, StringComparison.Ordinal);
        }

        public string GetLoginModeDisplay()
        {
            if (UsesAssumeRole())
            {
                return LanguageHelper.Get("login_mode_role");
            }

            return LanguageHelper.Get("data_files_mode_access_key");
        }

        // "AKIA…WXYZ" - dovoljno za prepoznavanje ključa bez prikaza cijele vrijednosti
        public string GetMaskedAccessKey()
        {
            if (AccessKeyUnreadable)
            {
                return "(nije čitljiv)";
            }

            if (string.IsNullOrEmpty(AccessKey))
            {
                return "-";
            }

            if (AccessKey.Length < 8)
            {
                return AccessKey;
            }

            return AccessKey.Substring(0, 4) + "…" + AccessKey.Substring(AccessKey.Length - 4);
        }

        // "naziv (regija, način prijave)"
        public string GetSummary()
        {
            return ProfileName + " (" + Region + ", " + GetLoginModeDisplay() + ")";
        }

        // ComboBox i ListBox prikazuju ToString
        public override string ToString()
        {
            return GetSummary();
        }
    }
}