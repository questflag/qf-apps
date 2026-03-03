using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Communication.Core.Messaging.Kafka;

public class KafkaConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly string _bootstrapServers;

    public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger)
    {
        _logger = logger;
        _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        
        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "qf-communication-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureTopicExistsAsync(stoppingToken);

        _logger.LogInformation("Subscribing to topic: {Topic}", KafkaTopics.CommunicationTasks);
        _consumer.Subscribe(KafkaTopics.CommunicationTasks);

        await Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result != null)
                    {
                        _logger.LogInformation($"Consumed message: {result.Message.Value}");
                        // Logic to dispatch to Twilio/SendGrid would go here
                        
                        _consumer.Commit(result);
                    }
                }
                catch (ConsumeException ex)
                {
                    if (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning("Topic {Topic} not found. Waiting before retry...", KafkaTopics.CommunicationTasks);
                        await Task.Delay(5000, stoppingToken);
                    }
                    else
                    {
                        _logger.LogError(ex, "Kafka consumer error");
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }, stoppingToken);
    }

    private async Task EnsureTopicExistsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build();
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = KafkaTopics.CommunicationTasks,
                    ReplicationFactor = 1,
                    NumPartitions = 1
                }
            });
            _logger.LogInformation("Topic {Topic} created successfully.", KafkaTopics.CommunicationTasks);
        }
        catch (CreateTopicsException ex)
        {
            _logger.LogInformation("Topic creation skipped or already exists. Message: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure topic {Topic} exists. The consumer will continue to run and auto-retry.", KafkaTopics.CommunicationTasks);
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

