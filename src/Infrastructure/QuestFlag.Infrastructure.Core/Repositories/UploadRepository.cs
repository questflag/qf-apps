using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Infrastructure.Core.Data;
using QuestFlag.Infrastructure.Domain.Entities;
using QuestFlag.Infrastructure.Domain.Enums;
using QuestFlag.Infrastructure.Domain.Interfaces;

namespace QuestFlag.Infrastructure.Core.Repositories;

public class UploadRepository : IUploadRepository
{
    private readonly AppDbContext _dbContext;

    public UploadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UploadRecord> AddAsync(UploadRecord record, CancellationToken ct = default)
    {
        _dbContext.UploadRecords.Add(record);
        await _dbContext.SaveChangesAsync(ct);
        return record;
    }

    public async Task UpdateAsync(UploadRecord record, CancellationToken ct = default)
    {
        _dbContext.UploadRecords.Update(record);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, string deletedByUserId, CancellationToken ct = default)
    {
        var record = await GetByIdAsync(id, ct);
        if (record != null)
        {
            record.IsDeleted = true;
            record.DeletedAtUtc = DateTime.UtcNow;
            record.DeletedByUserId = deletedByUserId;
            
            _dbContext.UploadRecords.Update(record);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<UploadRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.UploadRecords.FindAsync(new object[] { id }, ct);
    }

    public async Task<(IReadOnlyList<UploadRecord> Items, int TotalCount)> GetListAsync(
        Guid tenantId,
        Guid? userIdFilter,
        DateTime? fromDate,
        DateTime? toDate,
        string? category,
        string? status,
        string role,
        string sortBy = "CreatedAtUtc",
        bool descending = true,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = _dbContext.UploadRecords.AsQueryable();

        // 1. Mandatory Filters (Tenant isolation & Role based constraints)
        query = query.Where(x => x.TenantId == tenantId);
        
        if (!string.Equals(role, UserRole.TenantAdmin, StringComparison.OrdinalIgnoreCase))
        {
            // Regular user only sees their own
            query = query.Where(x => x.UserId == userIdFilter); // Note: if user is not admin, the controller should force their own ID here
        }
        else if (userIdFilter.HasValue)
        {
            // Admin filtering heavily by specific user
            query = query.Where(x => x.UserId == userIdFilter.Value);
        }

        // 2. Optional Filters
        if (fromDate.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(x => x.CreatedAtUtc <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.Category == category);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UploadStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        // 3. Counting total records matching criteria
        var totalCount = await query.CountAsync(ct);

        // 4. Sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "filename" => descending ? query.OrderByDescending(x => x.OriginalFileName) : query.OrderBy(x => x.OriginalFileName),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "size" => descending ? query.OrderByDescending(x => x.SizeInBytes) : query.OrderBy(x => x.SizeInBytes),
            "taskname" => descending ? query.OrderByDescending(x => x.TaskName) : query.OrderBy(x => x.TaskName),
            _ => descending ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc) // Default
        };

        // 5. Pagination
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
