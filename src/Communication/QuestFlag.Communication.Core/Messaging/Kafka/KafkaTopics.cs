namespace QuestFlag.Communication.Core.Messaging.Kafka;

public static class KafkaTopics
{
    public const string CommunicationTasks = "qf-communication-tasks";
    public const string DeadLetterQueue = "qf-dlq";
    public const string ConversationEvents = "qf-conversation-events";
    public const string UploadCompleted = "upload-completed";
}
