using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Domain.Contracts;

namespace QuestFlag.Passport.Application.Features.Tenants.Commands;

public record DeleteTenantCommand(Guid Id) : IRequest<Unit>;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Unit> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        await _tenantRepository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
