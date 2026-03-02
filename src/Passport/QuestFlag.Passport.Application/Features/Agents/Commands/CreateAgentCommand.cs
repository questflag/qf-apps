using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenIddict.Abstractions;

namespace QuestFlag.Passport.Application.Features.Agents.Commands;

public record CreateAgentCommand(
    string ClientId,
    string DisplayName,
    string? ClientSecret,
    string Type,
    HashSet<string> Permissions,
    HashSet<Uri> RedirectUris,
    HashSet<Uri> PostLogoutRedirectUris) : IRequest<string>;

public class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, string>
{
    private readonly IOpenIddictApplicationManager _manager;

    public CreateAgentCommandHandler(IOpenIddictApplicationManager manager)
    {
        _manager = manager;
    }

    public async Task<string> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = request.ClientId,
            DisplayName = request.DisplayName,
            ClientSecret = request.ClientSecret,
            Type = request.Type
        };

        foreach (var p in request.Permissions) descriptor.Permissions.Add(p);
        foreach (var u in request.RedirectUris) descriptor.RedirectUris.Add(u);
        foreach (var u in request.PostLogoutRedirectUris) descriptor.PostLogoutRedirectUris.Add(u);

        await _manager.CreateAsync(descriptor, cancellationToken);
        return request.ClientId;
    }
}
