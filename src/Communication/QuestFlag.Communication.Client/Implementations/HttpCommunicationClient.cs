using System.Net.Http.Json;
using QuestFlag.Communication.Domain.DTOs;
using QuestFlag.Communication.Client.Contracts;

namespace QuestFlag.Communication.Client.Implementations;

public class HttpCommunicationClient : ICommunicationClient
{
    private readonly HttpClient _httpClient;

    public HttpCommunicationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendMessageAsync(SendMessageDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/comm/messages", dto);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SendMessageResult>();
        return result?.TransactionId ?? string.Empty;
    }

    public async Task<MessageStatusDto?> GetMessageStatusAsync(string transactionId)
    {
        return await _httpClient.GetFromJsonAsync<MessageStatusDto>($"api/comm/messages/{transactionId}/status");
    }

    private record SendMessageResult(string TransactionId);
}
