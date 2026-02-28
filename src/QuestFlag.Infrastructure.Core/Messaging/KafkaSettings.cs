namespace QuestFlag.Infrastructure.Core.Messaging;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicName { get; set; } = "upload-completed";
    public string GroupId { get; set; } = "qf-upload-processor-group";

    // Downstream API triggered after upload
    public string DownstreamWebhookUrl { get; set; } = string.Empty;
}
