using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.Configuration;

namespace QuestFlag.Communication.Core.VectorDB;

public class QdrantConversationVectorStore
{
    private readonly QdrantClient _client;

    public QdrantConversationVectorStore(IConfiguration configuration)
    {
        var url = configuration["Qdrant:Url"] ?? "http://localhost:6334";
        _client = new QdrantClient(new Uri(url));
    }

    public async Task StoreConversationSummaryAsync(
        string tenantId, 
        string agentId, 
        string conversationId, 
        float[] vector, 
        string text)
    {
        var collectionName = $"tenant_{tenantId.Replace("-", "")}";
        
        // Ensure collection exists (In a real scenario, this would be managed elsewhere)
        // await _client.CreateCollectionAsync(collectionName, new VectorParams { Size = 1536, Distance = Distance.Cosine });

        var point = new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = vector,
            Payload =
            {
                ["agentId"] = agentId,
                ["conversationId"] = conversationId,
                ["text"] = text
            }
        };

        await _client.UpsertAsync(collectionName, new[] { point });
    }
}
