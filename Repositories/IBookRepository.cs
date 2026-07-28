using Elibrary.Api.Models;

namespace Elibrary.Api.Repositories;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAllWithCategoryAsync(CancellationToken ct = default);
    Task<Book?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Book>> GetActiveWithCategoryAsync(CancellationToken ct = default);
    Task AddAsync(Book book, CancellationToken ct = default);
    void Update(Book book);
    void Remove(Book book);
    Task SaveChangesAsync(CancellationToken ct = default);
}
