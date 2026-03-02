using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenIddict.Abstractions;

namespace QuestFlag.Passport.Application.Features.Agents.Commands;

public record DeleteAgentCommand(string ClientId) : IRequest;

public class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand>
{
    private readonly IOpenIddictApplicationManager _manager;

    public DeleteAgentCommandHandler(IOpenIddictApplicationManager manager)
    {
        _manager = manager;
    }

    public async Task Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        var app = await _manager.FindByClientIdAsync(request.ClientId, cancellationToken);
        if (app != null)
        {
            await _manager.DeleteAsync(app, cancellationToken);
        }
    }
}
