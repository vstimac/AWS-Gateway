using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using AWSGateway.Models;
using System.IO;
using System.Linq;

namespace AWSGateway.Services
{
    // jedan interval CloudWatch metrike - sve četiri statistike, analizator bira koju koristi za koju metriku
    public class EC2MetricPoint
    {
        public string MetricName { get; set; }
        public DateTime Timestamp { get; set; }
        public double Average { get; set; }
        public double Maximum { get; set; }
        public double Minimum { get; set; }
        public double Sum { get; set; }
        public string Unit { get; set; }

        public EC2MetricPoint()
        {
            MetricName = string.Empty;
            Unit = string.Empty;
        }
    }

    public class EC2Service : IEC2Service
    {
        // nazivi metrika - koristi ih i InstanceUsageAnalyzer
        public const string MetricCpuUtilization = "CPUUtilization";
        public const string MetricNetworkIn = "NetworkIn";
        public const string MetricNetworkOut = "NetworkOut";
        public const string MetricCpuCreditBalance = "CPUCreditBalance";
        public const string MetricStatusCheckFailed = "StatusCheckFailed";

        private static readonly string[] MonitoredMetrics =
        {
            MetricCpuUtilization, MetricNetworkIn, MetricNetworkOut, MetricCpuCreditBalance, MetricStatusCheckFailed
        };

        private AWSProfile _profile;
        private AmazonEC2Client _client;
        private AmazonCloudWatchClient _cloudWatchClient;

        public EC2Service(AWSProfile profile)
        {
            _profile = profile;

            AWSCredentials credentials = AwsCredentialsFactory.Build(_profile);
            RegionEndpoint region = RegionEndpoint.GetBySystemName(_profile.Region);

            _client = new AmazonEC2Client(credentials, region);
            _cloudWatchClient = new AmazonCloudWatchClient(credentials, region);
        }

        public async Task StartInstanceAsync(string instanceId)
        {
            try
            {
                StartInstancesRequest request = new StartInstancesRequest();
                request.InstanceIds = new List<string>();
                request.InstanceIds.Add(instanceId);

                await _client.StartInstancesAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod pokretanja instance: " + ex.Message, ex);
            }
        }

        // zaustavlja pokrenutu instancu
        public async Task StopInstanceAsync(string instanceId)
        {
            try
            {
                StopInstancesRequest request = new StopInstancesRequest();
                request.InstanceIds = new List<string>();
                request.InstanceIds.Add(instanceId);

                await _client.StopInstancesAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod zaustavljanja instance: " + ex.Message, ex);
            }
        }

        // restart instance bez gasenja
        public async Task RebootInstanceAsync(string instanceId)
        {
            try
            {
                RebootInstancesRequest request = new RebootInstancesRequest();
                request.InstanceIds = new List<string>();
                request.InstanceIds.Add(instanceId);

                await _client.RebootInstancesAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod restarta instance: " + ex.Message, ex);
            }
        }

        // dohvaca instance u regiji
        public async Task<List<AWSResource>> DescribeInstancesAsync()
        {
            List<AWSResource> instances = new List<AWSResource>();

            try
            {
                DescribeInstancesRequest request = new DescribeInstancesRequest();
                DescribeInstancesResponse response = await _client.DescribeInstancesAsync(request);

                foreach (Reservation reservation in response.Reservations)
                {
                    foreach (Instance instance in reservation.Instances)
                    {
                        string instanceName = instance.InstanceId;

                        // instanca bez tagova - SDK kolekciju može vratiti kao null
                        if (instance.Tags != null)
                        {
                            foreach (Amazon.EC2.Model.Tag tag in instance.Tags)
                            {
                                if (tag.Key == "Name")
                                {
                                    instanceName = tag.Value;
                                    break;
                                }
                            }
                        }

                        AWSResource resource = new AWSResource();
                        resource.ResourceId = instance.InstanceId;
                        resource.Type = AWSResource.ResourceType.EC2Instance;
                        resource.Name = instanceName;
                        resource.Region = _profile.Region;

                        if (instance.State != null && instance.State.Name != null)
                        {
                            resource.Status = instance.State.Name.Value;
                        }

                        if (instance.LaunchTime.HasValue)
                        {
                            resource.CreatedAt = instance.LaunchTime.Value;
                        }

                        instances.Add(resource);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod dohvaćanja instanci: " + ex.Message, ex);
            }

            return instances;
        }

        // dohvaća sve praćene metrike za instancu u zadanom periodu, s korakom periodSeconds (mora biti višekratnik od 60)
        // traže se sve četiri statistike - cijena CloudWatch API poziva je po zahtjevu, ne po statistici
        // ne izmišlja podatke - ako AWS ne vrati točke za metriku (npr. CPUCreditBalance za ne-t instance), taj dio liste ostaje prazan
        public async Task<List<EC2MetricPoint>> GetInstanceMetricsAsync(string instanceId, TimeSpan lookback, int periodSeconds)
        {
            List<EC2MetricPoint> points = new List<EC2MetricPoint>();

            try
            {
                DateTime end = DateTime.UtcNow;
                DateTime start = end.Subtract(lookback);

                foreach (string metricName in MonitoredMetrics)
                {
                    GetMetricStatisticsRequest request = new GetMetricStatisticsRequest();
                    request.Namespace = "AWS/EC2";
                    request.MetricName = metricName;
                    request.StartTime = start;
                    request.EndTime = end;
                    request.Period = periodSeconds;
                    request.Statistics = new List<string> { "Average", "Maximum", "Minimum", "Sum" };
                    request.Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "InstanceId", Value = instanceId }
                    };

                    GetMetricStatisticsResponse response = await _cloudWatchClient.GetMetricStatisticsAsync(request);

                    // bez točaka SDK kolekciju može vratiti kao null
                    if (response.Datapoints == null)
                    {
                        continue;
                    }

                    foreach (Datapoint dp in response.Datapoints.OrderBy(d => d.Timestamp))
                    {
                        // točka bez vremena se preskače - ne izmišljamo trenutak mjerenja
                        if (dp.Timestamp.HasValue == false)
                        {
                            continue;
                        }

                        EC2MetricPoint point = new EC2MetricPoint();
                        point.MetricName = metricName;
                        point.Timestamp = dp.Timestamp.Value;
                        point.Average = dp.Average ?? 0;
                        point.Maximum = dp.Maximum ?? 0;
                        point.Minimum = dp.Minimum ?? 0;
                        point.Sum = dp.Sum ?? 0;
                        point.Unit = dp.Unit;

                        points.Add(point);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Greška kod dohvaćanja CloudWatch metrika: " + ex.Message, ex);
            }

            return points;
        }
    }
}