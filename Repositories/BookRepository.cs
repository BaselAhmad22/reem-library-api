using Elibrary.Api.Data;
using Elibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _db;

    public BookRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Book>> GetAllWithCategoryAsync(CancellationToken ct = default)
        => await _db.Books.Include(b => b.Category).OrderByDescending(b => b.CreatedAt).ToListAsync(ct);

    public Task<Book?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default)
        => _db.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Book>> GetActiveWithCategoryAsync(CancellationToken ct = default)
        => await _db.Books.AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public Task AddAsync(Book book, CancellationToken ct = default)
        => _db.Books.AddAsync(book, ct).AsTask();

    public void Update(Book book) => _db.Books.Update(book);

    public void Remove(Book book) => _db.Books.Remove(book);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
