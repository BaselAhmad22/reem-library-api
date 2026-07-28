using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Models;
using Elibrary.Api.Repositories;
using Elibrary.Api.Services;

namespace Elibrary.Api.Services;

public class BookService
{
    private readonly IBookRepository _books;
    private readonly ICategoryRepository _categories;
    private readonly AuditService _audit;
    private readonly RealtimeService _realtime;

    public BookService(
        IBookRepository books,
        ICategoryRepository categories,
        AuditService audit,
        RealtimeService realtime)
    {
        _books = books;
        _categories = categories;
        _audit = audit;
        _realtime = realtime;
    }

    public async Task<IReadOnlyList<BookDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _books.GetAllWithCategoryAsync(ct);
        return list.Select(EntityMappers.MapBook).ToList();
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var book = await _books.GetByIdWithCategoryAsync(id, ct);
        return book is null ? null : EntityMappers.MapBook(book);
    }

    public async Task<(BookDto? Result, string? Error)> CreateAsync(BookRequest req, CancellationToken ct = default)
    {
        var err = Validate(req);
        if (err != null) return (null, err);
        if (await _categories.GetByIdWithBooksAsync(req.CategoryId, ct) is null)
            return (null, "Category not found.");

        var book = MapToEntity(req, new Book { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await _books.AddAsync(book, ct);
        await _books.SaveChangesAsync(ct);
        book = (await _books.GetByIdWithCategoryAsync(book.Id, ct))!;
        await _audit.LogAsync("create", "book", book.Id.ToString(), book.Title);
        var dto = EntityMappers.MapBook(book);
        await _realtime.PublishAsync("book", "create", dto);
        return (dto, null);
    }

    public async Task<(BookDto? Result, string? Error)> UpdateAsync(int id, BookRequest req, CancellationToken ct = default)
    {
        var err = Validate(req);
        if (err != null) return (null, err);
        var book = await _books.GetByIdWithCategoryAsync(id, ct);
        if (book is null) return (null, "Not found");
        if (await _categories.GetByIdWithBooksAsync(req.CategoryId, ct) is null)
            return (null, "Category not found.");

        MapToEntity(req, book);
        book.UpdatedAt = DateTime.UtcNow;
        _books.Update(book);
        await _books.SaveChangesAsync(ct);
        book = (await _books.GetByIdWithCategoryAsync(id, ct))!;
        await _audit.LogAsync("update", "book", book.Id.ToString(), book.Title);
        var dto = EntityMappers.MapBook(book);
        await _realtime.PublishAsync("book", "update", dto);
        return (dto, null);
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var book = await _books.GetByIdWithCategoryAsync(id, ct);
        if (book is null) return (false, "Not found");
        _books.Remove(book);
        await _books.SaveChangesAsync(ct);
        await _audit.LogAsync("delete", "book", id.ToString(), book.Title);
        await _realtime.PublishAsync("book", "delete", new { id });
        return (true, null);
    }

    private static string? Validate(BookRequest req) =>
        Validators.FirstError(
            Validators.Require(req.Title, "Title"),
            Validators.Require(req.Author, "Author"),
            Validators.Url(req.CoverUrl),
            req.CategoryId <= 0 ? "Category is required." : null,
            req.PublishedYear is < 1000 or > 2100 ? "Invalid published year." : null);

    private static Book MapToEntity(BookRequest req, Book book)
    {
        book.Title = req.Title.Trim();
        book.Author = req.Author.Trim();
        book.Description = req.Description?.Trim() ?? "";
        book.CoverUrl = req.CoverUrl?.Trim() ?? "";
        book.Isbn = req.Isbn?.Trim() ?? "";
        book.PublishedYear = req.PublishedYear;
        book.Language = string.IsNullOrWhiteSpace(req.Language) ? "ar" : req.Language.Trim();
        book.AvailableCopies = Math.Max(0, req.AvailableCopies);
        book.IsFeatured = req.IsFeatured;
        book.IsActive = req.IsActive;
        book.CategoryId = req.CategoryId;
        return book;
    }
}
