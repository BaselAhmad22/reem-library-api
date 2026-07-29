using Elibrary.Api.Data;
using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Services;

public class LibraryContentService
{
    private readonly AppDbContext _db;
    private readonly IBookRepository _books;
    private readonly ICategoryRepository _categories;

    public LibraryContentService(AppDbContext db, IBookRepository books, ICategoryRepository categories)
    {
        _db = db;
        _books = books;
        _categories = categories;
    }

    public async Task<PublicContentDto> GetPublicContentAsync(CancellationToken ct = default)
    {
        var settings = await _db.LibrarySettings.AsNoTracking().FirstAsync(ct);
        var categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(
                c.Id, c.NameAr, c.NameEn, c.Slug, c.SortOrder, c.IsActive,
                c.Books.Count(b => b.IsActive)))
            .ToListAsync(ct);

        var books = await _books.GetActiveWithCategoryAsync(ct);
        var bookDtos = books.Select(EntityMappers.MapBook).ToList();
        var topRated = bookDtos
            .Where(b => b.RatingsCount > 0)
            .OrderByDescending(b => b.AverageRating)
            .ThenByDescending(b => b.RatingsCount)
            .ThenByDescending(b => b.DownloadCount)
            .Take(20)
            .ToList();

        // If no ratings yet, show featured / popular downloads as starter carousel
        if (topRated.Count == 0)
        {
            topRated = bookDtos
                .OrderByDescending(b => b.IsFeatured)
                .ThenByDescending(b => b.DownloadCount)
                .Take(12)
                .ToList();
        }

        return new PublicContentDto(
            EntityMappers.MapSettings(settings),
            categories,
            bookDtos.Where(b => b.IsFeatured),
            topRated,
            bookDtos,
            bookDtos.Count);
    }

    public async Task<BookDto?> GetPublicBookAsync(int id, CancellationToken ct = default)
    {
        var book = await _books.GetByIdWithCategoryAsync(id, ct);
        if (book is null || !book.IsActive) return null;
        return EntityMappers.MapBook(book);
    }

    public async Task<IReadOnlyList<BookDto>> GetTopRatedAsync(int take = 20, CancellationToken ct = default)
    {
        var books = await _db.Books.AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.AverageRating)
            .ThenByDescending(b => b.RatingsCount)
            .ThenByDescending(b => b.DownloadCount)
            .Take(take)
            .ToListAsync(ct);
        return books.Select(EntityMappers.MapBook).ToList();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var totalBooks = await _db.Books.CountAsync(ct);
        var activeBooks = await _db.Books.CountAsync(b => b.IsActive, ct);
        var categories = await _db.Categories.CountAsync(ct);
        var featured = await _db.Books.CountAsync(b => b.IsFeatured && b.IsActive, ct);
        return new DashboardStatsDto(totalBooks, activeBooks, categories, featured);
    }
}

public record DashboardStatsDto(int TotalBooks, int ActiveBooks, int Categories, int FeaturedBooks);
