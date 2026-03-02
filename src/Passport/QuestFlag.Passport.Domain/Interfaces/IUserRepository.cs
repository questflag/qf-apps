using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<ApplicationUser>> GetByTenantIdAsync(System.Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationUser>> SearchAsync(System.Guid tenantId, string query, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationUser>> SearchAllAsync(string query, CancellationToken ct = default);
    Task<ApplicationUser?> GetByIdAsync(System.Guid id, CancellationToken ct = default);
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<ApplicationUser> AddAsync(ApplicationUser user, string password, IEnumerable<string> roles, CancellationToken ct = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken ct = default);
    Task DeleteAsync(System.Guid id, CancellationToken ct = default);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task SetRolesAsync(ApplicationUser user, IEnumerable<string> roles);
    Task SetAssignedAgentsAsync(ApplicationUser user, IEnumerable<string> agentClientIds);
    Task<List<string>> GetAssignedAgentIdsAsync(ApplicationUser user);
}
