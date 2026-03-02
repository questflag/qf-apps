using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Application.Common.DTOs;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Queries;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return null;

        return new UserProfileDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.TwoFactorEnabled
        );
    }
}
