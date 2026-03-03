namespace QuestFlag.Communication.Shared.Messaging;

public class KafkaSettings
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;

    // Downstream API triggered after upload
    public string DownstreamWebhookUrl { get; set; } = string.Empty;
}
