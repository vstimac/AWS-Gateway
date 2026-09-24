using System.Globalization;
using Amazon;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using Amazon.Runtime;
using AWSGateway.Models;

namespace AWSGateway.Services
{
    public class CostExplorerService : ICostExplorerService
    {
        private AmazonCostExplorerClient _client;

        // Cost Explorer je globalna usluga - dostupna samo preko us-east-1 endpointa, bez obzira na regiju profila
        // NAPOMENA: svaki GetCostAndUsage zahtjev (i svaka stranica rezultata) naplaćuje se 0,01 USD - zato postoji predmemorija
        public CostExplorerService(AWSProfile profile)
        {
            AWSCredentials credentials = AwsCredentialsFactory.Build(profile);
            _client = new AmazonCostExplorerClient(credentials, RegionEndpoint.USEast1);
        }

        // dnevni trošak po usluzi i regiji za period [start, end) - agregat na razini usluge, ne po pojedinom resursu
        // krediti i povrati se isključuju: inače račun koji troši AWS kredite vidi 0 ili negativan iznos umjesto stvarnog troška usluga
        public async Task<(List<CostRow> Rows, int PageCount)> GetCostByServiceAndRegionAsync(DateTime start, DateTime end)
        {
            List<CostRow> rows = new List<CostRow>();
            int pageCount = 0;

            try
            {
                string nextToken = null;

                do
                {
                    GetCostAndUsageRequest request = new GetCostAndUsageRequest();
                    request.TimePeriod = new DateInterval
                    {
                        Start = start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        End = end.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    };
                    request.Granularity = Granularity.DAILY;
                    request.Metrics = new List<string> { "UnblendedCost" };
                    request.GroupBy = new List<GroupDefinition>
                    {
                        new GroupDefinition { Type = "DIMENSION", Key = "SERVICE" },
                        new GroupDefinition { Type = "DIMENSION", Key = "REGION" }
                    };
                    request.Filter = new Expression
                    {
                        Not = new Expression
                        {
                            Dimensions = new DimensionValues
                            {
                                Key = "RECORD_TYPE",
                                Values = new List<string> { "Credit", "Refund" }
                            }
                        }
                    };
                    request.NextPageToken = nextToken;

                    GetCostAndUsageResponse response = await _client.GetCostAndUsageAsync(request);
                    pageCount++;

                    if (response.ResultsByTime != null)
                    {
                        foreach (ResultByTime result in response.ResultsByTime)
                        {
                            AddRows(rows, result);
                        }
                    }

                    nextToken = response.NextPageToken;
                } while (string.IsNullOrEmpty(nextToken) == false);
            }
            catch (DataUnavailableException ex)
            {
                throw new Exception("Cost Explorer još nema podataka za ovaj račun (nov račun ili Cost Explorer tek omogućen - podaci stižu do 24 h).", ex);
            }
            catch (AmazonCostExplorerException ex)
            {
                if (ex.ErrorCode == "AccessDeniedException")
                {
                    throw new Exception("Cost Explorer nije dostupan: račun nema omogućen Cost Explorer ili korisnik nema dozvolu ce:GetCostAndUsage.", ex);
                }

                throw new Exception("Greška kod dohvaćanja troškova: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod dohvaćanja troškova: " + ex.Message, ex);
            }

            return (rows, pageCount);
        }

        private void AddRows(List<CostRow> rows, ResultByTime result)
        {
            if (result.Groups == null)
            {
                return;
            }

            string date = string.Empty;

            if (result.TimePeriod != null && result.TimePeriod.Start != null)
            {
                date = result.TimePeriod.Start;
            }

            foreach (Group group in result.Groups)
            {
                CostRow row = new CostRow();
                row.Date = date;
                row.Service = "Nepoznato";
                row.Region = "Nepoznato";

                if (group.Keys != null && group.Keys.Count > 0)
                {
                    row.Service = group.Keys[0];
                }

                if (group.Keys != null && group.Keys.Count > 1)
                {
                    row.Region = group.Keys[1];
                }

                MetricValue metric;

                if (group.Metrics != null && group.Metrics.TryGetValue("UnblendedCost", out metric))
                {
                    // AWS vraća iznos s točkom kao decimalnim separatorom - bez invarijantne kulture
                    // hrvatske postavke čitaju točku kao separator tisućica ("0.0123" postaje 123)
                    double amount;

                    if (double.TryParse(metric.Amount, NumberStyles.Float, CultureInfo.InvariantCulture, out amount))
                    {
                        row.Amount = amount;
                    }

                    if (string.IsNullOrEmpty(metric.Unit) == false)
                    {
                        row.Unit = metric.Unit;
                    }
                }

                rows.Add(row);
            }
        }
    }
}