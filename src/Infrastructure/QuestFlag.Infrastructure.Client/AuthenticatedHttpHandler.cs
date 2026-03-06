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
                await _tokenProvider.HandleUnauthorizedAsync();
            }

            return response;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("TypeError: NetworkError"))
        {
            // When navigating away (e.g. forced logout due to 401), pending fetch requests
            // are aborted by the browser, surfacing as a NetworkError rather than a graceful cancellation.
            // By translating this to a TaskCanceledException, we prevent Blazor from generating Unhandled Errors.
            throw new TaskCanceledException("Request was aborted due to navigation (NetworkError).", ex);
        }
    }
}
