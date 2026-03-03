using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using QuestFlag.Passport.Domain.Contracts;

namespace QuestFlag.Passport.Application.Features.Tenants.Commands;

public record UpdateTenantCommand(Guid Id, string Name, string Slug, bool IsActive, string? CustomDomain = null, string? SubdomainSlug = null) : IRequest<Unit>;

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50).Matches("^[a-z0-9-]+$");
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, Unit>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Unit> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with id '{request.Id}' not found.");
        }

        // Check if slug is changing and if it conflicts
        if (tenant.Slug != request.Slug)
        {
            var existing = await _tenantRepository.GetBySlugAsync(request.Slug, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException($"Tenant with slug '{request.Slug}' already exists.");
            }
        }

        tenant.Name = request.Name;
        tenant.Slug = request.Slug;
        tenant.IsActive = request.IsActive;
        tenant.CustomDomain = request.CustomDomain;
        tenant.SubdomainSlug = request.SubdomainSlug;

        await _tenantRepository.UpdateAsync(tenant, cancellationToken);
        return Unit.Value;
    }
}
