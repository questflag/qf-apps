using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Infrastructure.Domain.Contracts;

namespace QuestFlag.Infrastructure.Core.Implementations.Repositories;

public class Repository<TEntity, TContext> : IRepository<TEntity> 
    where TEntity : class
    where TContext : DbContext
{
    protected readonly TContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(TContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<TEntity?> GetByIdAsync(object[] id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(id, ct);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet.Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await DbSet.SingleOrDefaultAsync(predicate, ct);
    }

    public virtual IQueryable<TEntity> GetQueryable()
    {
        return DbSet.AsQueryable();
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        DbSet.Add(entity);
        await DbContext.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync(ct);
    }
}
