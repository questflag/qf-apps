using System.Net;
using System.Net.Http.Headers;
using QuestFlag.Infrastructure.Client.Contracts;

namespace QuestFlag.Infrastructure.Client;

public class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly IAccessTokenProvider _tokenProvider;

    public AuthenticatedHttpHandler(IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"[AuthenticatedHttpHandler] Attaching token (length: {token.Length}) to {request.Method} {request.RequestUri}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            Console.WriteLine($"[AuthenticatedHttpHandler] NO TOKEN found for {request.Method} {request.RequestUri}");
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine($"[AuthenticatedHttpHandler] 401 Unauthorized from {request.Method} {request.RequestUri}");
                await _tokenProvider.HandleUnauthorizedAsync();
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                Console.WriteLine($"[AuthenticatedHttpHandler] 403 Forbidden from {request.Method} {request.RequestUri}. Keeping current session.");
            }
            else if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[AuthenticatedHttpHandler] {(int)response.StatusCode} {response.StatusCode} from {request.Method} {request.RequestUri}");
            }

            return response;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("TypeError: NetworkError"))
        {
            // When navigating away (e.g. forced logout due to repeated 401), pending fetch
            // requests are aborted by the browser and surface as a NetworkError.
            throw new TaskCanceledException("Request was aborted due to navigation (NetworkError).", ex);
        }
    }
}
