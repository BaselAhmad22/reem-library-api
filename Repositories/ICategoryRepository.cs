using Elibrary.Api.Models;

namespace Elibrary.Api.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllOrderedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetActiveOrderedAsync(CancellationToken ct = default);
    Task<Category?> GetByIdWithBooksAsync(int id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Update(Category category);
    void Remove(Category category);
    Task SaveChangesAsync(CancellationToken ct = default);
}
