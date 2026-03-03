using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Contracts;

public interface IUserRepository
{
    Task<IReadOnlyList<ApplicationUser>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationUser>> SearchAsync(Guid tenantId, string query, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationUser>> SearchAllAsync(string searchTerm, CancellationToken ct = default);
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default); // Kept from original, not explicitly removed
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default); // New
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password); // Replaces AddAsync
    Task<IdentityResult> UpdateAsync(ApplicationUser user); // Modified from original UpdateAsync
    Task<IdentityResult> DeleteAsync(ApplicationUser user); // Modified from original DeleteAsync
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task SetRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task SetAssignedAgentsAsync(ApplicationUser user, IEnumerable<string> agentClientIds);
    Task<HashSet<string>> GetAssignedAgentIdsAsync(ApplicationUser user); // Modified return type
}
