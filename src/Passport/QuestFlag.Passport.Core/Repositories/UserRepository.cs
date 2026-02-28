using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Core.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _userManager.Users.Where(u => u.TenantId == tenantId).ToListAsync(ct);
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user, string password, string role, CancellationToken ct = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException($"Failed to add user to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

        return user;
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task AssignRoleAsync(ApplicationUser user, string roleName)
    {
        await _userManager.AddToRoleAsync(user, roleName);
    }
}
