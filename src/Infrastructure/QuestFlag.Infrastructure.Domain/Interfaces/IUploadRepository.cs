using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuestFlag.Infrastructure.Domain.Entities;

namespace QuestFlag.Infrastructure.Domain.Interfaces;

public interface IUploadRepository
{
    Task<UploadRecord> AddAsync(UploadRecord record, CancellationToken ct = default);
    Task UpdateAsync(UploadRecord record, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string deletedByUserId, CancellationToken ct = default);
    
    Task<UploadRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // List records respecting RBAC (role == tenant_admin sees all in tenant, role == user sees own)
    // Supports sorting, filtering by tenant, user, dates, etc., and returning total count for pagination
    Task<(IReadOnlyList<UploadRecord> Items, int TotalCount)> GetListAsync(
        Guid tenantId,
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        string? category,
        string? status,
        string role,
        string sortBy = "CreatedAtUtc",
        bool descending = true,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken ct = default);
}
