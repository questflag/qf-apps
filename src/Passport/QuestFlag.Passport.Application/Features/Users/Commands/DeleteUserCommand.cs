using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Domain.Contracts;

namespace QuestFlag.Passport.Application.Features.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<Unit>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user != null)
        {
            await _userRepository.DeleteAsync(user);
        }
        return Unit.Value;
    }
}
