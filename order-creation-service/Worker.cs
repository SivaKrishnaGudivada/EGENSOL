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

namespace order_creation_service
{

   //public class KafkaProducerHostedService : IHostedService
   //{
   //   private readonly ILogger<KafkaProducerHostedService> _logger;
   //   private readonly IProducer<Null, CreateOrder> _producer;


   //   public KafkaProducerHostedService(ILogger<KafkaProducerHostedService> logger)
   //   {

   //      _logger = logger;
   //      var config = new ProducerConfig()
   //      {
   //         BootstrapServers = "localhost:9092"
   //      };

   //      _producer = new ProducerBuilder<Null, CreateOrder>(config)
   //      .SetValueSerializer(new Common.Services.CustomCreateOrderAsyncSerializer())
   //      .Build();
   //   }

   //   public async Task StartAsync(CancellationToken cancellationToken)
   //   {
   //      for (var i = 0; i < 100; ++i)
   //      {
   //         var value = new CreateOrder();

   //         try
   //         {
   //            await _producer.ProduceAsync(Constants.KafkaBulkOrderCreateTopicString, new Message<Null, CreateOrder>()
   //            {
   //               Value = value
   //            }, cancellationToken);
   //         }
   //         catch (Exception ex)
   //         {

   //         }

   //         await Task.Delay(1000 * 10);
   //      }

   //      _producer.Flush(TimeSpan.FromSeconds(10));
   //   }

   //   public Task StopAsync(CancellationToken cancellationToken)
   //   {
   //      _producer?.Dispose();
   //      return Task.CompletedTask;
   //   }
   //}

   public class KafkaConsumerHostedService : IHostedService
   {
      private readonly IOrderService _orderService;
      private readonly ILogger<KafkaConsumerHostedService> _logger;
      private readonly ClusterClient _cluster;

      public KafkaConsumerHostedService(IConfiguration config, ILogger<KafkaConsumerHostedService> logger, IOrderService orderService)
      {
         _orderService = orderService;
         _logger = logger;
         var port = config.GetValue<string>(Constants.ConfigurationKeys.KafkaPortKey);
         _cluster = new ClusterClient(new Configuration
         {
            Seeds = port
         }, new ConsoleLogger());
      }

      public Task StartAsync(CancellationToken cancellationToken)
      {
         _logger.LogInformation("Starting ...");

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
         return Task.CompletedTask;
      }

      private void InsertOrderRecord(CreateOrder createOrder) =>
         _orderService.CreateOrder(createOrder);

      public Task StopAsync(CancellationToken cancellationToken)
      {
         _cluster?.Dispose();
         return Task.CompletedTask;
      }
   }
}
