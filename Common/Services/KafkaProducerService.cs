using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Confluent.Kafka.SyncOverAsync;

namespace Common.Services
{
   public class CreateOrderKafkaPublisher : ICreateOrderPublisher
   {
      private readonly IProducer<Null, CreateOrder> _producer;
      private readonly ILogger<CreateOrderKafkaPublisher> _logger;
      public CreateOrderKafkaPublisher(IConfiguration iconfig, ILogger<CreateOrderKafkaPublisher> logger)
      {
         _logger = logger;
         var port = iconfig.GetValue<string>("KafkaPort");
         _producer = new ProducerBuilder<Null, CreateOrder>(new ProducerConfig()
         {
            BootstrapServers = port
         })
         .SetValueSerializer(new SyncOverAsyncSerializer<CreateOrder>(new  CustomCreateOrderAsyncSerializer()))
         .Build();
      }

      public void Publish(CreateOrder data)
      {
         _logger.LogInformation("Publishing order creation data for order creation service");
         _producer.Produce(Constants.KafkaBulkOrderCreateTopicString, new Message<Null, CreateOrder>() { Value = data });
      }
   }

   public class CustomCreateOrderAsyncSerializer : IAsyncSerializer<CreateOrder>
   {
      public byte[] Serialize(CreateOrder data, SerializationContext context)
      {
         return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
      }

      public async Task<byte[]> SerializeAsync(CreateOrder data, SerializationContext context)
      {
         return await Task.Run(() => Serialize(data,context));
      }
   }
}