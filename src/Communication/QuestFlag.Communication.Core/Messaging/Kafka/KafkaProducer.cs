using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace QuestFlag.Communication.Core.Messaging.Kafka;

public class KafkaProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string topic, string key, string value)
    {
        await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value });
    }
}
