using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestFlag.Infrastructure.Domain.Events;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Core.Messaging;

public class KafkaUploadEventPublisher : IUploadEventPublisher, IDisposable
{
    private readonly KafkaSettings _settings;
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaUploadEventPublisher> _logger;

    public KafkaUploadEventPublisher(IOptions<KafkaSettings> options, ILogger<KafkaUploadEventPublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;

        var config = new ProducerConfig { BootstrapServers = _settings.BootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishUploadCompletedAsync(UploadCompletedEvent @event, CancellationToken ct = default)
    {
        try
        {
            var key = @event.TenantId.ToString();
            var value = JsonSerializer.Serialize(@event);

            var message = new Message<string, string> { Key = key, Value = value };

            var deliveryResult = await _producer.ProduceAsync(_settings.TopicName, message, ct);
            
            _logger.LogInformation("Delivered upload event to {Topic} partition {Partition} at offset {Offset}", 
                deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to deliver Kafka event for upload {UploadId}", @event.UploadId);
            throw;
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}
