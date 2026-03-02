namespace QuestFlag.Communication.Domain.Enums;

public enum StreamStatus
{
    STREAM_INITIALIZED,
    STREAM_ACTIVE,
    SENTENCE_DETECTED,
    AI_PROCESSING,
    AI_RESPONDING,
    TTS_STREAMING,
    STREAM_TERMINATED
}
