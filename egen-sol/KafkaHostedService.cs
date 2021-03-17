using System.Threading;
using System.Threading.Tasks;
using Common.Services;
using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HostedServices
{
    public class KafkaConsumerHostedService : IHostedService
    {
        private readonly ClusterClient _client;
        private readonly ILogger<KafkaConsumerHostedService> _logger;

        public KafkaConsumerHostedService(IConfiguration config, ILogger<KafkaConsumerHostedService> logger)
        {
            _logger = logger;
            var port = config.GetValue<string>(Constants.ConfigurationKeys.KafkaPortKey);
            _client = new ClusterClient(new Configuration
            {
                Seeds = port
            }, new ConsoleLogger());
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.ConsumeFromLatest(Constants.KafkaBulkOrderCreateTopicString);
            _client.MessageReceived += rec =>
           {
               // TODO: update shared cache between services.
           };
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Dispose();
            return Task.CompletedTask;
        }
    }
}
