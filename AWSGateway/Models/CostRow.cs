namespace AWSGateway.Models
{
    // dnevni trošak AWS usluge u regiji - agregat na razini usluge, NE trošak pojedinog resursa
    public class CostRow
    {
        // dan na koji se trošak odnosi, "yyyy-MM-dd" (UTC dan, kako ga vraća Cost Explorer)
        public string Date { get; set; }
        public string Service { get; set; }
        public string Region { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }

        public CostRow()
        {
            Date = string.Empty;
            Service = string.Empty;
            Region = string.Empty;
            Amount = 0;
            Unit = "USD";
        }
    }
}