using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestFlag.Infrastructure.Domain.Events;

namespace QuestFlag.Infrastructure.Core.Messaging;

public class UploadCompletedConsumer : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly IConsumer<string, string> _consumer;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadCompletedConsumer> _logger;

    public UploadCompletedConsumer(
        IOptions<KafkaSettings> options,
        HttpClient httpClient,
        ILogger<UploadCompletedConsumer> logger)
    {
        _settings = options.Value;
        _httpClient = httpClient;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false // Manual commit after successful handling
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UploadCompletedConsumer started. Subscribing to topic: {Topic}", _settings.TopicName);
        _consumer.Subscribe(_settings.TopicName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Block for up to 1 second waiting for a message
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult != null && consumeResult.Message != null)
                    {
                        var @event = JsonSerializer.Deserialize<UploadCompletedEvent>(consumeResult.Message.Value);
                        if (@event != null)
                        {
                            await HandleEventAsync(@event, stoppingToken);
                        }

                        // Commit offset to avoid reprocessing
                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consumer error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event in consumer loop");
                    // Wait a bit before continuing to avoid tight error loops
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task HandleEventAsync(UploadCompletedEvent @event, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Processing UploadCompletedEvent for UploadId: {UploadId}", @event.UploadId);

        if (string.IsNullOrWhiteSpace(_settings.DownstreamWebhookUrl))
        {
            _logger.LogWarning("No downstream webhook URL configured. Event ignored.");
            return;
        }

        try
        {
            // Call the downstream API (could be an ML pipeline, OCR service, etc.)
            var response = await _httpClient.PostAsJsonAsync(_settings.DownstreamWebhookUrl, @event, stoppingToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully invoked downstream API for UploadId: {UploadId}", @event.UploadId);
            }
            else
            {
                _logger.LogError("Downstream API call failed with status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call downstream API.");
            throw; // This will cause the catch block in ExecuteAsync to trigger, skipping commit
        }
    }

    public override void Dispose()
    {
        _httpClient.Dispose();
        _consumer.Dispose();
        base.Dispose();
    }
}
