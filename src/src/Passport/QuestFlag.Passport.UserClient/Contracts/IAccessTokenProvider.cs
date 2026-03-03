using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Passport.UserClient.Contracts;

/// <summary>
/// Abstraction used by HTTP handlers to retrieve the access token
/// and handle 401s.
/// </summary>
public interface IAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task HandleUnauthorizedAsync();
}
