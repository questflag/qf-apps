using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Application.Common.DTOs;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Queries;

public record GetUsersByTenantQuery(Guid TenantId, string? SearchTerm = null) : IRequest<IReadOnlyList<UserSummaryDto>>;

public class GetUsersByTenantQueryHandler : IRequestHandler<GetUsersByTenantQuery, IReadOnlyList<UserSummaryDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersByTenantQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> Handle(GetUsersByTenantQuery request, CancellationToken cancellationToken)
    {
        var users = string.IsNullOrWhiteSpace(request.SearchTerm)
            ? await _userRepository.GetByTenantIdAsync(request.TenantId, cancellationToken)
            : await _userRepository.SearchAsync(request.TenantId, request.SearchTerm, cancellationToken);
        
        var dtos = new List<UserSummaryDto>();
        foreach (var u in users)
        {
            var roles = (await _userRepository.GetRolesAsync(u)).ToList();
            var agentIds = await _userRepository.GetAssignedAgentIdsAsync(u);
            
            dtos.Add(new UserSummaryDto(
                u.Id, 
                u.TenantId, 
                u.Tenant?.Name ?? "Unknown", 
                u.UserName ?? "", 
                u.Email ?? "", 
                u.DisplayName, 
                u.IsActive, 
                u.TwoFactorEnabled, 
                u.EmailConfirmed, 
                roles, 
                agentIds));
        }

        return dtos;
    }
}
