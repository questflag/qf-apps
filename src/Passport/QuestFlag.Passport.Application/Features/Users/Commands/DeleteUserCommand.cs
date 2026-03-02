using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Domain.Interfaces;

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
        await _userRepository.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
