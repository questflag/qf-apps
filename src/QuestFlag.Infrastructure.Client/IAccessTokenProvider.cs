using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Infrastructure.Client;

/// <summary>
/// Abstraction used by the DelegatingHandler to retrieve the access token 
/// (from either local storage, a cookie, or Blazor's TokenProvider)
/// and handle 401s.
/// </summary>
public interface IAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task HandleUnauthorizedAsync();
}
