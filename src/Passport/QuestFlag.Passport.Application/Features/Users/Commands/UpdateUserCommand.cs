using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

public record UpdateUserCommand(Guid Id, string Username, string Email, string DisplayName, bool IsActive, List<string> Roles, List<string> AgentClientIds) : IRequest<Unit>;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id '{request.Id}' not found.");
        }

        // Check if username is changing and if it conflicts
        if (user.UserName != request.Username)
        {
            var existing = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException($"User with username '{request.Username}' already exists.");
            }
        }

        user.UserName = request.Username;
        user.Email = request.Email;
        user.DisplayName = request.DisplayName;
        user.IsActive = request.IsActive;

        await _userRepository.UpdateAsync(user, cancellationToken);
        
        // Update roles
        await _userRepository.SetRolesAsync(user, request.Roles);

        // Update agents
        if (request.AgentClientIds != null)
        {
            await _userRepository.SetAssignedAgentsAsync(user, request.AgentClientIds);
        }

        return Unit.Value;
    }
}
