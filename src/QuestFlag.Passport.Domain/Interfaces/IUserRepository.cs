using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<ApplicationUser>> GetByTenantIdAsync(System.Guid tenantId, CancellationToken ct = default);
    Task<ApplicationUser?> GetByIdAsync(System.Guid id, CancellationToken ct = default);
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<ApplicationUser> AddAsync(ApplicationUser user, string password, string role, CancellationToken ct = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken ct = default);
    Task DeleteAsync(System.Guid id, CancellationToken ct = default);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task AssignRoleAsync(ApplicationUser user, string roleName);
}
