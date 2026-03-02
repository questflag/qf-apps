using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenIddict.Abstractions;
using QuestFlag.Passport.Application.Common.DTOs;

namespace QuestFlag.Passport.Application.Features.Agents.Queries;

public record GetAgentsQuery : IRequest<IReadOnlyList<AgentDto>>;

public class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, IReadOnlyList<AgentDto>>
{
    private readonly IOpenIddictApplicationManager _manager;

    public GetAgentsQueryHandler(IOpenIddictApplicationManager manager)
    {
        _manager = manager;
    }

    public async Task<IReadOnlyList<AgentDto>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var agents = new List<AgentDto>();
        await foreach (var app in _manager.ListAsync(null, null, cancellationToken))
        {
            var clientId = await _manager.GetClientIdAsync(app, cancellationToken);
            var displayName = await _manager.GetDisplayNameAsync(app, cancellationToken);
            var type = await _manager.GetClientTypeAsync(app, cancellationToken);
            var permissions = await _manager.GetPermissionsAsync(app, cancellationToken);
            var redirectUris = await _manager.GetRedirectUrisAsync(app, cancellationToken);
            var postLogoutRedirectUris = await _manager.GetPostLogoutRedirectUrisAsync(app, cancellationToken);

            agents.Add(new AgentDto(
                clientId ?? "",
                displayName ?? "",
                type ?? "",
                new HashSet<string>(permissions),
                new HashSet<Uri>(redirectUris.Select(u => new Uri(u))),
                new HashSet<Uri>(postLogoutRedirectUris.Select(u => new Uri(u)))
            ));
        }

        return agents;
    }
}
