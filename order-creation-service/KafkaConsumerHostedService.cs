using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Services;
using Confluent.Kafka;
using Kafka.Public;
using Kafka.Public.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using Confluent.SchemaRegistry.Serdes;
using Confluent.Kafka.SyncOverAsync;
using Common.Services;
using Confluent.SchemaRegistry;
using Confluent.Kafka.Admin;

namespace order_creation_service
{
   public class KafkaConsumerHostedService : IHostedService
   {
      private readonly IOrderService _orderService;
      private readonly ILogger<KafkaConsumerHostedService> _logger;
      private readonly ClusterClient _cluster;
      private readonly string _port;

      public KafkaConsumerHostedService(IConfiguration config, ILogger<KafkaConsumerHostedService> logger, IOrderService orderService)
      {
         _orderService = orderService;
         _logger = logger;
         _port = config.GetValue<string>(Constants.ConfigurationKeys.KafkaPortKey);
         _cluster = new ClusterClient(new Configuration
         {
            Seeds = _port
         }, new ConsoleLogger());
      }

      public async Task StartAsync(CancellationToken cancellationToken)
      {
         _logger.LogInformation("Starting ...");

         await CreateTopic();

         _cluster.ConsumeFromLatest(Constants.KafkaBulkOrderCreateTopicString);

         _cluster.MessageReceived += record =>
         {
            _logger.LogInformation("Received order creation message!");
            try
            {
               var createOrder = JsonConvert.DeserializeObject<CreateOrder>(System.Text.Encoding.UTF8.GetString(record.Value as byte[]));
               InsertOrderRecord(createOrder);
            }
            catch (Exception ex)
            {
               _logger.LogError($"Failed to process order ", ex);
            }
         };

         _logger.LogInformation("Started");
         
      }

      private async Task CreateTopic()
      {
         _logger.LogInformation("creating topic to listen to..");
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _port}).Build())
        {
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[] { 
                    new TopicSpecification { Name = Constants.KafkaBulkOrderCreateTopicString, ReplicationFactor = 1, NumPartitions = 1 } });
            }
            catch (CreateTopicsException e)
            {
                Console.WriteLine($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
            }
        }
            _logger.LogInformation("Topic created!");
      }

      private async void InsertOrderRecord(CreateOrder createOrder) 
      {
         try
         {
            await _orderService.CreateOrder(createOrder);
         }
         catch (System.Exception ex)
         {
            _logger.LogError("failed to create order: "+ex.Message, ex);
         }
      }
         
      public Task StopAsync(CancellationToken cancellationToken)
      {
         _cluster?.Dispose();
         return Task.CompletedTask;
      }
   }
}
