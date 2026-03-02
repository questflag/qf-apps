using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Roles.Commands;

public record UpdateRoleCommand(Guid Id, string Name) : IRequest<Unit>;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Unit>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Unit> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            throw new InvalidOperationException($"Role with id '{request.Id}' not found.");
        }

        role.Name = request.Name;
        await _roleRepository.UpdateAsync(role, cancellationToken);
        return Unit.Value;
    }
}
