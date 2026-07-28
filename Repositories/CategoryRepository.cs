using Elibrary.Api.Data;
using Elibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetAllOrderedAsync(CancellationToken ct = default)
        => await _db.Categories.Include(c => c.Books).OrderBy(c => c.SortOrder).ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetActiveOrderedAsync(CancellationToken ct = default)
        => await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

    public Task<Category?> GetByIdWithBooksAsync(int id, CancellationToken ct = default)
        => _db.Categories.Include(c => c.Books).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default)
    {
        var q = _db.Categories.Where(c => c.Slug == slug);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return q.AnyAsync(ct);
    }

    public Task AddAsync(Category category, CancellationToken ct = default)
        => _db.Categories.AddAsync(category, ct).AsTask();

    public void Update(Category category) => _db.Categories.Update(category);

    public void Remove(Category category) => _db.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
