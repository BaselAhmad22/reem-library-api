using Elibrary.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;

    public Repository(AppDbContext db)
    {
        Db = db;
        Set = db.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => Set.FindAsync([id], ct).AsTask();

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Set.AsNoTracking().ToListAsync(ct);

    public Task AddAsync(T entity, CancellationToken ct = default)
        => Set.AddAsync(entity, ct).AsTask();

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Db.SaveChangesAsync(ct);
}
