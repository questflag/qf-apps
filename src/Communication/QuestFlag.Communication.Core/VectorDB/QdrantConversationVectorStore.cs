using Qdrant.Client;
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
        // Placeholder for Qdrant SDK call
        await Task.CompletedTask;
    }
}
