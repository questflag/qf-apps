using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

public record CreateUserCommand(Guid TenantId, string Username, string Email, string Password, string DisplayName, string RoleName) : IRequest<Guid>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoleName).NotEmpty();
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ITenantRepository _tenantRepository;

    public CreateUserCommandHandler(IUserRepository userRepository, ITenantRepository tenantRepository)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException($"User with username '{request.Username}' already exists.");

        var user = new ApplicationUser
        {
            TenantId = request.TenantId,
            UserName = request.Username,
            Email = request.Email,
            DisplayName = request.DisplayName,
            IsActive = true
        };

        var createdUser = await _userRepository.AddAsync(user, request.Password, request.RoleName, cancellationToken);
        return createdUser.Id;
    }
}
