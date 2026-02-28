using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Auth.Commands;

public record LoginCommand(string TenantSlug, string Username, string Password) : IRequest<bool>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.TenantSlug).NotEmpty();
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;

    public LoginCommandHandler(ITenantRepository tenantRepository, IUserRepository userRepository)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
        if (tenant == null || !tenant.IsActive)
            return false;

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null || user.TenantId != tenant.Id || !user.IsActive)
            return false;

        return await _userRepository.CheckPasswordAsync(user, request.Password);
    }
}
