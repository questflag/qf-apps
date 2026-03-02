using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;
using QuestFlag.Passport.Core.Data;

namespace QuestFlag.Passport.Core.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PassportDbContext _dbContext;

    public UserRepository(UserManager<ApplicationUser> userManager, PassportDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _userManager.Users
            .Include(u => u.Tenant)
            .Include(u => u.UserAgents)
            .Where(u => u.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ApplicationUser>> SearchAsync(Guid tenantId, string query, CancellationToken ct = default)
    {
        var normalizedQuery = query.ToUpperInvariant();
        return await _userManager.Users
            .Include(u => u.Tenant)
            .Where(u => u.TenantId == tenantId)
            .Where(u => u.NormalizedUserName!.Contains(normalizedQuery) 
                     || u.NormalizedEmail!.Contains(normalizedQuery)
                     || u.DisplayName.ToUpper().Contains(normalizedQuery))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(CancellationToken ct = default)
    {
        return await _userManager.Users.Include(u => u.Tenant).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ApplicationUser>> SearchAllAsync(string query, CancellationToken ct = default)
    {
        var normalizedQuery = query.ToUpperInvariant();
        return await _userManager.Users
            .Include(u => u.Tenant)
            .Where(u => u.NormalizedUserName!.Contains(normalizedQuery) 
                     || u.NormalizedEmail!.Contains(normalizedQuery)
                     || u.DisplayName.ToUpper().Contains(normalizedQuery))
            .ToListAsync(ct);
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user, string password, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        var roleResult = await _userManager.AddToRolesAsync(user, roles);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException($"Failed to add user to roles: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

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

    public async Task SetRolesAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        await _userManager.AddToRolesAsync(user, roles);
    }

    public async Task SetAssignedAgentsAsync(ApplicationUser user, IEnumerable<string> agentClientIds)
    {
        // 1. Remove existing
        var existing = await _dbContext.UserAgents.Where(x => x.UserId == user.Id).ToListAsync();
        _dbContext.UserAgents.RemoveRange(existing);

        // 2. Add new
        foreach (var clientId in agentClientIds)
        {
            _dbContext.UserAgents.Add(new UserAgent { UserId = user.Id, ClientId = clientId });
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<string>> GetAssignedAgentIdsAsync(ApplicationUser user)
    {
        return await _dbContext.UserAgents
            .Where(x => x.UserId == user.Id)
            .Select(x => x.ClientId)
            .ToListAsync();
    }
}
