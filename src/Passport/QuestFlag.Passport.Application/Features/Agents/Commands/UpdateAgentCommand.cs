using MediatR;
using OpenIddict.Abstractions;
using QuestFlag.Passport.Application.Common.DTOs;
using System.Linq;

namespace QuestFlag.Passport.Application.Features.Agents.Commands;

public record UpdateAgentCommand(
    string ClientId,
    string DisplayName,
    string? ClientSecret,
    string Type,
    HashSet<string> Permissions,
    HashSet<Uri> RedirectUris,
    HashSet<Uri> PostLogoutRedirectUris) : IRequest;

public class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand>
{
    private readonly IOpenIddictApplicationManager _manager;

    public UpdateAgentCommandHandler(IOpenIddictApplicationManager manager)
    {
        _manager = manager;
    }

    public async Task Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        var app = await _manager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (app == null) throw new InvalidOperationException("Agent not found.");

        var descriptor = new OpenIddictApplicationDescriptor();
        await _manager.PopulateAsync(descriptor, app, cancellationToken);

        descriptor.DisplayName = request.DisplayName;
        descriptor.Type = request.Type;
        if (!string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            descriptor.ClientSecret = request.ClientSecret;
        }

        descriptor.Permissions.Clear();
        foreach (var p in request.Permissions) descriptor.Permissions.Add(p);

        descriptor.RedirectUris.Clear();
        foreach (var u in request.RedirectUris) descriptor.RedirectUris.Add(u);

        descriptor.PostLogoutRedirectUris.Clear();
        foreach (var u in request.PostLogoutRedirectUris) descriptor.PostLogoutRedirectUris.Add(u);

        await _manager.UpdateAsync(app, descriptor, cancellationToken);
    }
}
