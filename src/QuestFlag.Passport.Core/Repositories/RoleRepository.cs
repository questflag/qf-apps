using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Core.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RoleRepository(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<IdentityRole<Guid>>> GetAllAsync(CancellationToken ct = default)
    {
        return await _roleManager.Roles.ToListAsync(ct);
    }

    public async Task<IdentityRole<Guid>?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _roleManager.FindByIdAsync(id.ToString());
    }

    public async Task<IdentityRole<Guid>> AddAsync(string roleName, CancellationToken ct = default)
    {
        var role = new IdentityRole<Guid>(roleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        
        return role;
    }

    public async Task UpdateAsync(Guid id, string newName, CancellationToken ct = default)
    {
        var role = await GetByIdAsync(id, ct);
        if (role == null) return;
        
        role.Name = newName;
        role.NormalizedName = _roleManager.KeyNormalizer?.NormalizeName(newName);
        
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await GetByIdAsync(id, ct);
        if (role != null)
        {
            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to delete role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
