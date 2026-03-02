namespace QuestFlag.Communication.Domain.Enums;

public enum MessageStatus
{
    CREATED,
    VALIDATED,
    QUEUED,
    SENDING,
    SENT,
    DELIVERED,
    READ,
    RETRYING,
    FAILED,
    DEAD_LETTERED,
    CANCELLED
}
