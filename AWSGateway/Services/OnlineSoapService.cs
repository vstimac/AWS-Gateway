using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using AWSGateway.Helpers;

namespace AWSGateway.Services
{
    public class OnlineSoapService
    {
        // CountryInfoService
        private const string SoapURL = "http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso";

        private HttpClient _httpClient;

        public OnlineSoapService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        // Capital City
        public async Task<string> GetCapitalCityAsync(string countryCode)
        {
            try
            {
                string soapRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                        <soap:Body>
                            <CapitalCity xmlns=""http://www.oorsprong.org/websamples.countryinfo"">
                                <sCountryISOCode>" + countryCode + @"</sCountryISOCode>
                            </CapitalCity>
                        </soap:Body>
                    </soap:Envelope>";

                using (StringContent content = new StringContent(soapRequest, Encoding.UTF8, "text/xml"))
                {
                    // SOAPAction header govori servisu koju metod zovem
                    content.Headers.Add("SOAPAction", "http://www.oorsprong.org/websamples.countryinfo/CapitalCity");

                    using (HttpResponseMessage response = await _httpClient.PostAsync(SoapURL, content))
                    {
                        string responseXml = await response.Content.ReadAsStringAsync();
                        XDocument doc = XDocument.Parse(responseXml);
                        XNamespace ns = XNamespace.Get("http://www.oorsprong.org/websamples.countryinfo");

                        foreach (XElement element in doc.Descendants(ns + "CapitalCityResult"))
                        {
                            return element.Value;
                        }

                        return "Nepoznato";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod SOAP poziva CapitalCity: " + ex.Message);
            }
        }

        // SOAP metoda CountryName - vraca naziv drzave za ISO kod
        public async Task<string> GetCountryNameAsync(string countryCode)
        {
            try
            {
                string soapRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                        <soap:Body>
                            <CountryName xmlns=""http://www.oorsprong.org/websamples.countryinfo"">
                                <sCountryISOCode>" + countryCode + @"</sCountryISOCode>
                            </CountryName>
                        </soap:Body>
                    </soap:Envelope>";

                using (StringContent content = new StringContent(soapRequest, Encoding.UTF8, "text/xml"))
                {
                    content.Headers.Add("SOAPAction", "http://www.oorsprong.org/websamples.countryinfo/CountryName");

                    using (HttpResponseMessage response = await _httpClient.PostAsync(SoapURL, content))
                    {
                        string responseXml = await response.Content.ReadAsStringAsync();
                        XDocument doc = XDocument.Parse(responseXml);
                        XNamespace ns = XNamespace.Get("http://www.oorsprong.org/websamples.countryinfo");

                        foreach (XElement element in doc.Descendants(ns + "CountryNameResult"))
                        {
                            return element.Value;
                        }

                        return "Nepoznato";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod SOAP poziva CountryName: " + ex.Message);
            }
        }

        // Get AWS region info - država regije dolazi iz zajedničkog popisa regija (AwsRegions)
        public async Task<string> GetAwsRegionInfoAsync(string awsRegion)
        {
            try
            {
                string countryCode = AwsRegions.GetCountryCode(awsRegion);

                if (string.IsNullOrEmpty(countryCode))
                {
                    return "Regija: " + awsRegion;
                }

                string countryName = await GetCountryNameAsync(countryCode);
                string capital = await GetCapitalCityAsync(countryCode);

                return countryName + " > " + capital;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Greška kod dohvaćanja podataka o regiji: " + ex.Message);
                return "Regija: " + awsRegion;
            }
        }
    }
}