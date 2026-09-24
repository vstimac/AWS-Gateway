using AWSGateway.Models;

namespace AWSGateway.Services
{
    // omogućuje testiranje logike koja ovisi o EC2/CloudWatch operacijama bez stvarnog AWS poziva
    public interface IEC2Service
    {
        Task StartInstanceAsync(string instanceId);
        Task StopInstanceAsync(string instanceId);
        Task RebootInstanceAsync(string instanceId);
        Task<List<AWSResource>> DescribeInstancesAsync();
        Task<List<EC2MetricPoint>> GetInstanceMetricsAsync(string instanceId, TimeSpan lookback, int periodSeconds);
    }
}