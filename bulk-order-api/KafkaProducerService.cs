using Confluent.Kafka;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

public class KafkaPublisher : IKafkaPublisher
{
    private readonly IProducer<Null, object> _producer;
    private readonly ILogger<KafkaPublisher> _logger;
    public KafkaPublisher(IConfiguration iconfig, ILogger<KafkaPublisher> logger)
    {
        _logger = logger;
        var port = iconfig.GetValue<string>("KafkaPort");
        _producer = new ProducerBuilder<Null, object>(new ProducerConfig()
        {
            BootstrapServers = port
        }).Build();
    }

    public async void Publish(string topic, object data)
    {
        await _producer.ProduceAsync(
            topic,
            new Message<Null, object>()
            {
                Value = data
            }, default(CancellationToken));

        _producer.Flush(TimeSpan.FromSeconds(1000));
    }
}