using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace QuestFlag.Passport.Domain.Interfaces;

public interface IRoleRepository
{
    Task<IReadOnlyList<IdentityRole<Guid>>> GetAllAsync(CancellationToken ct = default);
    Task<IdentityRole<Guid>?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IdentityRole<Guid>> AddAsync(string roleName, CancellationToken ct = default);
    Task UpdateAsync(Guid id, string newName, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
