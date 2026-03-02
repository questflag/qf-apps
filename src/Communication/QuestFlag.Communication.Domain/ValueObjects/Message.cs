namespace QuestFlag.Communication.Domain.ValueObjects;

public record Message(string Sender, string Content, DateTime Timestamp);
