using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Tenants.Commands;

public record CreateTenantCommand(string Name, string Slug) : IRequest<Guid>;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(50).Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.");
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTenantCommandHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await _tenantRepository.GetBySlugAsync(request.Slug, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Tenant with slug '{request.Slug}' already exists.");
        }

        var tenant = new Tenant
        {
            Name = request.Name,
            Slug = request.Slug,
            IsActive = true
        };

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        return tenant.Id;
    }
}
