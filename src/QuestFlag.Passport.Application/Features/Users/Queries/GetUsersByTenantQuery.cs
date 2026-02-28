using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Application.Common.DTOs;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Queries;

public record GetUsersByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<UserSummaryDto>>;

public class GetUsersByTenantQueryHandler : IRequestHandler<GetUsersByTenantQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersByTenantQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> Handle(GetUsersByTenantQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        
        var dtos = new List<UserSummaryDto>();
        foreach (var u in users)
        {
            var roles = await _userRepository.GetRolesAsync(u);
            var mainRole = roles.Count > 0 ? roles[0] : "None";
            dtos.Add(new UserSummaryDto(u.Id, u.UserName ?? "", u.Email ?? "", u.DisplayName, u.IsActive, mainRole));
        }

        return dtos;
    }
}
