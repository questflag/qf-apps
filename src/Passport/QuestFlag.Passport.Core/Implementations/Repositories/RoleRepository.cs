using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Contracts;

namespace QuestFlag.Passport.Core.Implementations.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RoleRepository(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IdentityRole<Guid>?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _roleManager.FindByIdAsync(id.ToString());
    }

    public async Task<IdentityRole<Guid>?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _roleManager.FindByNameAsync(name);
    }

    public async Task<IReadOnlyList<IdentityRole<Guid>>> GetAllAsync(CancellationToken ct = default)
    {
        return await _roleManager.Roles.ToListAsync(ct);
    }

    public async Task AddAsync(IdentityRole<Guid> role, CancellationToken ct = default)
    {
        await _roleManager.CreateAsync(role);
    }

    public async Task UpdateAsync(IdentityRole<Guid> role, CancellationToken ct = default)
    {
        await _roleManager.UpdateAsync(role);
    }

    public async Task UpdateAsync(Guid id, string name, CancellationToken ct = default)
    {
        var role = await GetByIdAsync(id, ct);
        if (role != null)
        {
            role.Name = name;
            await _roleManager.UpdateAsync(role);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await GetByIdAsync(id, ct);
        if (role != null)
        {
            await _roleManager.DeleteAsync(role);
        }
    }
}
