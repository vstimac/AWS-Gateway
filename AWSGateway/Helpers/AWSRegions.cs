namespace AWSGateway.Helpers
{
    // jedna AWS regija - kod, grad i ISO kod države (za SOAP servis s podacima o državi)
    public class AwsRegionInfo
    {
        public string Code { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }

        public AwsRegionInfo(string code, string city, string countryCode)
        {
            Code = code;
            City = city;
            CountryCode = countryCode;
        }

        // ComboBox prikazuje ToString - "eu-central-1 (Frankfurt)"
        public override string ToString()
        {
            if (string.IsNullOrEmpty(City))
            {
                return Code;
            }

            return Code + " (" + City + ")";
        }
    }

    // jedini popis regija u aplikaciji - koriste ga prijava, upravljanje profilima i SOAP servis
    public static class AwsRegions
    {
        public const string DefaultRegion = "eu-north-1";

        // europske regije prve; eu-south-1, eu-south-2 i eu-central-2 moraju se ručno omogućiti na AWS računu (opt-in)
        private static readonly List<AwsRegionInfo> _regions = new List<AwsRegionInfo>
        {
            new AwsRegionInfo("eu-central-1", "Frankfurt", "DE"),
            new AwsRegionInfo("eu-central-2", "Zürich", "CH"),
            new AwsRegionInfo("eu-west-1", "Dublin", "IE"),
            new AwsRegionInfo("eu-west-2", "London", "GB"),
            new AwsRegionInfo("eu-west-3", "Pariz", "FR"),
            new AwsRegionInfo("eu-north-1", "Stockholm", "SE"),
            new AwsRegionInfo("eu-south-1", "Milano", "IT"),
            new AwsRegionInfo("eu-south-2", "Aragón", "ES"),
            new AwsRegionInfo("us-east-1", "Sjeverna Virginia", "US"),
            new AwsRegionInfo("us-east-2", "Ohio", "US"),
            new AwsRegionInfo("us-west-1", "Sjeverna Kalifornija", "US"),
            new AwsRegionInfo("us-west-2", "Oregon", "US"),
            new AwsRegionInfo("ca-central-1", "Kanada", "CA"),
            new AwsRegionInfo("sa-east-1", "São Paulo", "BR"),
            new AwsRegionInfo("ap-south-1", "Mumbai", "IN"),
            new AwsRegionInfo("ap-northeast-1", "Tokio", "JP"),
            new AwsRegionInfo("ap-northeast-2", "Seul", "KR"),
            new AwsRegionInfo("ap-northeast-3", "Osaka", "JP"),
            new AwsRegionInfo("ap-southeast-1", "Singapur", "SG"),
            new AwsRegionInfo("ap-southeast-2", "Sydney", "AU")
        };

        // null ako regija nije na popisu
        public static AwsRegionInfo Find(string code)
        {
            foreach (AwsRegionInfo region in _regions)
            {
                if (region.Code == code)
                {
                    return region;
                }
            }

            return null;
        }

        // prazan string za regiju koja nije na popisu
        public static string GetCountryCode(string code)
        {
            AwsRegionInfo region = Find(code);

            if (region == null)
            {
                return string.Empty;
            }

            return region.CountryCode;
        }

        // puni ComboBox svim regijama i odabire zadanu; regija koja nije na popisu (npr. iz starijeg profila) dodaje se da se ne izgubi
        public static void FillComboBox(ComboBox comboBox, string selectedCode)
        {
            comboBox.Items.Clear();

            foreach (AwsRegionInfo region in _regions)
            {
                comboBox.Items.Add(region);
            }

            SelectRegion(comboBox, selectedCode);
        }

        public static void SelectRegion(ComboBox comboBox, string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                code = DefaultRegion;
            }

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                AwsRegionInfo region = comboBox.Items[i] as AwsRegionInfo;

                if (region != null && region.Code == code)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }

            AwsRegionInfo unknownRegion = new AwsRegionInfo(code, string.Empty, string.Empty);
            comboBox.Items.Add(unknownRegion);
            comboBox.SelectedItem = unknownRegion;
        }

        public static string GetSelectedCode(ComboBox comboBox)
        {
            AwsRegionInfo region = comboBox.SelectedItem as AwsRegionInfo;

            if (region == null)
            {
                return DefaultRegion;
            }

            return region.Code;
        }
    }
}