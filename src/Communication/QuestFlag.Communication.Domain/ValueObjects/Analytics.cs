namespace QuestFlag.Communication.Domain.ValueObjects;

public record Analytics(
    string DominantSentiment,
    float SentimentPercentage,
    List<string> ExtractedFacts,
    List<float> ExtractedNumbers);
