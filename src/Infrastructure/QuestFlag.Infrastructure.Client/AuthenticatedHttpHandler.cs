using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Optionally, handle 401s here if we want to trigger a refresh 
        // For Blazor with cookie-based auth, a 401 might mean the cookie expired
        // and a full page reload or redirect to login is needed.
        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            await _tokenProvider.HandleUnauthorizedAsync();
        }

        return response;
    }
}
