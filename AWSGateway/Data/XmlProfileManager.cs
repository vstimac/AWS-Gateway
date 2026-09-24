using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AWSGateway.Helpers;
using AWSGateway.Models;

namespace AWSGateway.Data
{
    // aws_profiles.xml - postavke prijave; tajni ključ i privremene STS vjerodajnice se nikad ne spremaju
    // svaki profil ima vlasnika (korisnik aplikacije) - korisnik vidi i mijenja samo svoje profile i starije profile bez vlasnika
    public class XmlProfileManager
    {
        // veže enkripciju AccessKey-a na Windows korisnički račun (DPAPI), bez potrebe za upravljanjem ključevima
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("AWSGateway.AccessKey.v1");

        private string _xmlPath;

        public XmlProfileManager()
        {
            _xmlPath = AppPaths.Combine("aws_profiles.xml");
        }

        // ---------------------------------------------------------------
        // Enkripcija access keya

        // enkriptira AccessKey za pohranu na disk (DPAPI, CurrentUser)
        private static string EncryptAccessKey(string accessKey)
        {
            if (string.IsNullOrEmpty(accessKey))
            {
                return string.Empty;
            }

            byte[] plainBytes = Encoding.UTF8.GetBytes(accessKey);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }

        // stari nešifrirani zapis prepoznaje se po formatu access keya; DPAPI blob koji se ne može dešifrirati
        // (datoteka s drugog računala ili drugog Windows korisnika) vraća se kao prazan, uz oznaku unreadable
        private static string DecryptAccessKey(string storedValue, out bool unreadable)
        {
            unreadable = false;

            if (string.IsNullOrEmpty(storedValue))
            {
                return string.Empty;
            }

            if (AwsFormatValidator.IsValidAccessKeyId(storedValue))
            {
                return storedValue;
            }

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(storedValue);
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Greška kod dešifriranja access keya: " + ex.Message);
                unreadable = true;
                return string.Empty;
            }
        }

        // ---------------------------------------------------------------
        // Datoteka

        // InvalidDataException ako XML nije ispravan - pozivatelj može ponuditi sigurnosnu kopiju (BackupCorruptedFile)
        private XDocument LoadDocument()
        {
            if (File.Exists(_xmlPath) == false)
            {
                return new XDocument(new XElement("AWSProfiles"));
            }

            try
            {
                XDocument doc = XDocument.Load(_xmlPath);

                if (doc.Root == null || doc.Root.Name != "AWSProfiles")
                {
                    throw new InvalidDataException("Datoteka s profilima nema očekivani korijenski element AWSProfiles.");
                }

                return doc;
            }
            catch (XmlException ex)
            {
                throw new InvalidDataException("Datoteka s profilima je oštećena: " + ex.Message, ex);
            }
        }

        // zapis u privremenu datoteku pa zamjena - prekid usred spremanja ne ostavlja napola zapisan XML
        private void SaveDocument(XDocument doc)
        {
            string tempPath = _xmlPath + ".tmp";

            doc.Save(tempPath);
            File.Move(tempPath, _xmlPath, true);
        }

        // premješta oštećenu datoteku u sigurnosnu kopiju i vraća njezinu putanju; nakon toga popis profila je prazan
        public string BackupCorruptedFile()
        {
            string backupPath = _xmlPath + ".bak-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            File.Move(_xmlPath, backupPath);

            return backupPath;
        }

        // ---------------------------------------------------------------
        // CRUD - svaka operacija radi u ime korisnika aplikacije

        // READ - profili korisnika i stariji profili bez vlasnika
        public List<AWSProfile> GetForUser(string username)
        {
            List<AWSProfile> profiles = new List<AWSProfile>();

            XDocument doc = LoadDocument();

            foreach (XElement element in doc.Root.Elements("Profile"))
            {
                AWSProfile profile = ReadProfile(element);

                if (profile.IsVisibleTo(username))
                {
                    profiles.Add(profile);
                }
            }

            return profiles;
        }

        // null ako profil ne postoji ili ga korisnik ne smije vidjeti
        public AWSProfile GetById(int profileId, string username)
        {
            foreach (AWSProfile profile in GetForUser(username))
            {
                if (profile.ProfileId == profileId)
                {
                    return profile;
                }
            }

            return null;
        }

        // null ako profil ne postoji; naziv se uspoređuje bez obzira na velika i mala slova
        public AWSProfile FindByName(string profileName, string username)
        {
            foreach (AWSProfile profile in GetForUser(username))
            {
                if (string.Equals(profile.ProfileName, profileName, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }

            return null;
        }

        // CREATE - ID dodjeljuje manager, vlasnik je korisnik koji sprema; vraća ID novog profila
        public int Add(AWSProfile profile, string username)
        {
            RequireUsername(username);

            XDocument doc = LoadDocument();

            EnsureUniqueName(doc, profile.ProfileName, 0, username);

            int newId = GetNextId(doc);

            XElement profileElement = new XElement("Profile",
                new XElement("ProfileId", newId),
                new XElement("Owner", username),
                new XElement("CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));

            WriteProfileFields(profileElement, profile);

            doc.Root.Add(profileElement);
            SaveDocument(doc);

            profile.ProfileId = newId;
            profile.Owner = username;

            return newId;
        }

        // UPDATE - mijenja sva spremljiva polja; profil bez vlasnika spremanjem postaje korisnikov
        public void Update(AWSProfile profile, string username)
        {
            RequireUsername(username);

            XDocument doc = LoadDocument();

            XElement profileElement = FindProfileElement(doc, profile.ProfileId);

            if (profileElement == null)
            {
                throw new InvalidOperationException("Profil više ne postoji.");
            }

            EnsureCanModify(profileElement, username);
            EnsureUniqueName(doc, profile.ProfileName, profile.ProfileId, username);

            WriteProfileFields(profileElement, profile);
            SetElementValue(profileElement, "Owner", username);
            RenameLegacyCreatedAt(profileElement);

            SaveDocument(doc);

            profile.Owner = username;
        }

        // DELETE - samo vlastiti profil ili profil bez vlasnika
        public void Delete(int profileId, string username)
        {
            RequireUsername(username);

            XDocument doc = LoadDocument();

            XElement profileElement = FindProfileElement(doc, profileId);

            if (profileElement == null)
            {
                return;
            }

            EnsureCanModify(profileElement, username);

            profileElement.Remove();
            SaveDocument(doc);
        }

        // kod brisanja korisničkog računa - inače bi profili ostali i pripali novom korisniku istog imena
        public int DeleteAllForOwner(string username)
        {
            RequireUsername(username);

            if (File.Exists(_xmlPath) == false)
            {
                return 0;
            }

            XDocument doc = LoadDocument();
            List<XElement> ownedElements = new List<XElement>();

            foreach (XElement element in doc.Root.Elements("Profile"))
            {
                if (string.Equals(GetElementValue(element, "Owner", ""), username, StringComparison.Ordinal))
                {
                    ownedElements.Add(element);
                }
            }

            foreach (XElement element in ownedElements)
            {
                element.Remove();
            }

            if (ownedElements.Count > 0)
            {
                SaveDocument(doc);
            }

            return ownedElements.Count;
        }

        // ---------------------------------------------------------------
        // Pomoćne metode

        private void RequireUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("Profili se mogu mijenjati samo nakon prijave.");
            }
        }

        // provjera i na razini managera, ne samo u formi - tuđi profil se ne može izmijeniti ni kad bi ga forma nekako ponudila
        private void EnsureCanModify(XElement profileElement, string username)
        {
            string owner = GetElementValue(profileElement, "Owner", "");

            if (string.IsNullOrEmpty(owner) == false && string.Equals(owner, username, StringComparison.Ordinal) == false)
            {
                throw new UnauthorizedAccessException("Profil pripada drugom korisniku.");
            }
        }

        private AWSProfile ReadProfile(XElement element)
        {
            AWSProfile profile = new AWSProfile();

            int profileId;
            int.TryParse(GetElementValue(element, "ProfileId", "0"), out profileId);

            bool unreadable;

            profile.ProfileId = profileId;
            profile.ProfileName = GetElementValue(element, "ProfileName", "");
            profile.AccessKey = DecryptAccessKey(GetElementValue(element, "AccessKey", ""), out unreadable);
            profile.AccessKeyUnreadable = unreadable;
            profile.Region = GetElementValue(element, "Region", AwsRegions.DefaultRegion);
            profile.DefaultBucket = GetElementValue(element, "DefaultBucket", "");
            profile.RoleArn = GetElementValue(element, "RoleArn", "");
            profile.MfaSerialNumber = GetElementValue(element, "MfaSerialNumber", "");
            profile.Owner = GetElementValue(element, "Owner", "");

            // secret key i privremene vjerodajnice se ne spremaju - ostaju na zadanim (praznim) vrijednostima iz AWSProfile()
            return profile;
        }

        // SecretKey se ne sprema. RoleArn/MfaSerialNumber nisu tajni (ARN je javan identifikator, serijski broj nije tajna).
        // TempAccessKeyId/TempSecretAccessKey/SessionToken/SessionExpiresAtUtc se NIKAD ne spremaju - privremeni su i beskorisni nakon isteka.
        private void WriteProfileFields(XElement profileElement, AWSProfile profile)
        {
            SetElementValue(profileElement, "ProfileName", profile.ProfileName);
            SetElementValue(profileElement, "AccessKey", EncryptAccessKey(profile.AccessKey));
            SetElementValue(profileElement, "Region", profile.Region);
            SetElementValue(profileElement, "DefaultBucket", profile.DefaultBucket);
            SetElementValue(profileElement, "RoleArn", profile.RoleArn);
            SetElementValue(profileElement, "MfaSerialNumber", profile.MfaSerialNumber);
        }

        // naziv je jedinstven među profilima koje korisnik vidi (vlastiti i bez vlasnika) - dva korisnika smiju imati profil istog naziva
        private void EnsureUniqueName(XDocument doc, string profileName, int ownProfileId, string username)
        {
            foreach (XElement element in doc.Root.Elements("Profile"))
            {
                int profileId;
                int.TryParse(GetElementValue(element, "ProfileId", "0"), out profileId);

                if (profileId == ownProfileId)
                {
                    continue;
                }

                string owner = GetElementValue(element, "Owner", "");
                bool visibleToUser = string.IsNullOrEmpty(owner) || string.Equals(owner, username, StringComparison.Ordinal);

                if (visibleToUser == false)
                {
                    continue;
                }

                if (string.Equals(GetElementValue(element, "ProfileName", ""), profileName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Profil s nazivom \"" + profileName + "\" već postoji.");
                }
            }
        }

        private int GetNextId(XDocument doc)
        {
            int maxId = 0;

            foreach (XElement element in doc.Root.Elements("Profile"))
            {
                int profileId;

                if (int.TryParse(GetElementValue(element, "ProfileId", "0"), out profileId) && profileId > maxId)
                {
                    maxId = profileId;
                }
            }

            return maxId + 1;
        }

        // stariji zapisi imaju ExportedAt, koji je zapravo značio vrijeme stvaranja profila
        private void RenameLegacyCreatedAt(XElement profileElement)
        {
            XElement legacy = profileElement.Element("ExportedAt");

            if (legacy == null)
            {
                return;
            }

            if (profileElement.Element("CreatedAt") == null)
            {
                profileElement.Add(new XElement("CreatedAt", legacy.Value));
            }

            legacy.Remove();
        }

        // trazi element profila po ProfileId, vraca null ako ga nema
        private XElement FindProfileElement(XDocument doc, int profileId)
        {
            foreach (XElement element in doc.Root.Elements("Profile"))
            {
                XElement idElement = element.Element("ProfileId");

                if (idElement != null && idElement.Value == profileId.ToString())
                {
                    return element;
                }
            }

            return null;
        }

        private string GetElementValue(XElement profileElement, string elementName, string defaultValue)
        {
            XElement element = profileElement.Element(elementName);

            if (element != null)
            {
                return element.Value;
            }

            return defaultValue;
        }

        // vrijednost elementa - ako element ne postoji (stariji XML zapis prije dodavanja ovog polja), dodaje ga
        private void SetElementValue(XElement profileElement, string elementName, string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            XElement element = profileElement.Element(elementName);

            if (element != null)
            {
                element.Value = value;
            }
            else
            {
                profileElement.Add(new XElement(elementName, value));
            }
        }
    }
}