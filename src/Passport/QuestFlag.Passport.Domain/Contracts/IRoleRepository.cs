using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QuestFlag.Passport.Domain.Entities;

namespace QuestFlag.Passport.Domain.Contracts;

public interface IRoleRepository
{
    Task<IdentityRole<Guid>?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IdentityRole<Guid>?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<IdentityRole<Guid>>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(IdentityRole<Guid> role, CancellationToken ct = default);
    Task UpdateAsync(IdentityRole<Guid> role, CancellationToken ct = default);
    Task UpdateAsync(Guid id, string name, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
