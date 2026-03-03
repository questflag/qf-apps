using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Passport.UserClient.Contracts;

namespace QuestFlag.Passport.UserClient;

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

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            await _tokenProvider.HandleUnauthorizedAsync();
        }

        return response;
    }
}
