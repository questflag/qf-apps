using Microsoft.EntityFrameworkCore;
using QuestFlag.Infrastructure.Application.Data;
using QuestFlag.Infrastructure.Domain.Entities;
using QuestFlag.Infrastructure.Domain.Enums;
using QuestFlag.Infrastructure.Domain.Contracts;

namespace QuestFlag.Infrastructure.Application.Implementations.Repositories;

public class UploadRepository : IUploadRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<UploadRecord> _dbSet;

    public UploadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = dbContext.UploadRecords;
    }

    public virtual async Task<UploadRecord> AddAsync(UploadRecord entity, CancellationToken ct = default)
    {
        _dbSet.Add(entity);
        await _dbContext.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(UploadRecord entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
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
            
            await UpdateAsync(record, ct);
        }
    }

    public async Task<UploadRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, ct);
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
        var query = _dbSet.AsQueryable();

        // 1. Mandatory Filters (Tenant isolation & Role based constraints)
        query = query.Where(x => x.TenantId == tenantId);
        
        if (!string.Equals(role, UserRole.TenantAdmin, StringComparison.OrdinalIgnoreCase))
        {
            // Regular user only sees their own
            query = query.Where(x => x.UserId == userIdFilter); 
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
