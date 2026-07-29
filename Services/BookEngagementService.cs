using Elibrary.Api.Data;
using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Services;

public class BookEngagementService(AppDbContext db)
{
    public async Task<BookDetailDto?> GetDetailAsync(int bookId, int? userId, CancellationToken ct = default)
    {
        var book = await db.Books.AsNoTracking()
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive, ct);
        if (book is null) return null;

        var comments = await db.BookComments.AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.BookId == bookId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(100)
            .ToListAsync(ct);

        int? myRating = null;
        bool? myLiked = null;
        if (userId.HasValue)
        {
            myRating = await db.BookRatings.AsNoTracking()
                .Where(r => r.BookId == bookId && r.UserId == userId)
                .Select(r => (int?)r.Stars)
                .FirstOrDefaultAsync(ct);
            var reaction = await db.BookReactions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId, ct);
            myLiked = reaction is null ? null : reaction.IsLike;
        }

        return new BookDetailDto(
            EntityMappers.MapBook(book),
            comments.Select(EntityMappers.MapComment),
            myRating,
            myLiked);
    }

    public async Task<(DownloadResponse? Result, string? Error)> DownloadAsync(int bookId, int userId, CancellationToken ct = default)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive, ct);
        if (book is null) return (null, "Book not found.");
        if (string.IsNullOrWhiteSpace(book.DownloadUrl))
            return (null, "Download link is not available for this book.");

        book.DownloadCount += 1;
        book.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (new DownloadResponse(book.DownloadUrl, book.DownloadCount), null);
    }

    public async Task<(BookDto? Result, string? Error)> RateAsync(int bookId, int userId, int stars, CancellationToken ct = default)
    {
        if (stars is < 1 or > 5) return (null, "Rating must be between 1 and 5.");
        var book = await db.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive, ct);
        if (book is null) return (null, "Book not found.");

        var existing = await db.BookRatings.FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId, ct);
        if (existing is null)
        {
            db.BookRatings.Add(new BookRating
            {
                BookId = bookId,
                UserId = userId,
                Stars = stars
            });
        }
        else
        {
            existing.Stars = stars;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        await RefreshRatingStatsAsync(book, ct);
        return (EntityMappers.MapBook(book), null);
    }

    public async Task<(BookDto? Result, string? Error)> ReactAsync(int bookId, int userId, bool? like, CancellationToken ct = default)
    {
        var book = await db.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive, ct);
        if (book is null) return (null, "Book not found.");

        var existing = await db.BookReactions.FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId, ct);
        if (like is null)
        {
            if (existing is not null) db.BookReactions.Remove(existing);
        }
        else if (existing is null)
        {
            db.BookReactions.Add(new BookReaction { BookId = bookId, UserId = userId, IsLike = like.Value });
        }
        else
        {
            existing.IsLike = like.Value;
        }

        await db.SaveChangesAsync(ct);
        await RefreshReactionStatsAsync(book, ct);
        return (EntityMappers.MapBook(book), null);
    }

    public async Task<(BookCommentDto? Result, string? Error)> CommentAsync(int bookId, int userId, string body, CancellationToken ct = default)
    {
        body = body.Trim();
        if (body.Length < 2) return (null, "Comment is too short.");
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive, ct);
        if (book is null) return (null, "Book not found.");

        var comment = new BookComment
        {
            BookId = bookId,
            UserId = userId,
            Body = body
        };
        db.BookComments.Add(comment);
        book.CommentsCount += 1;
        book.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await db.Entry(comment).Reference(c => c.User).LoadAsync(ct);
        return (EntityMappers.MapComment(comment), null);
    }

    private async Task RefreshRatingStatsAsync(Book book, CancellationToken ct)
    {
        var stats = await db.BookRatings.Where(r => r.BookId == book.Id)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Avg = g.Average(x => x.Stars) })
            .FirstOrDefaultAsync(ct);

        book.RatingsCount = stats?.Count ?? 0;
        book.AverageRating = stats is null ? 0 : Math.Round(stats.Avg, 2);
        book.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private async Task RefreshReactionStatsAsync(Book book, CancellationToken ct)
    {
        book.LikesCount = await db.BookReactions.CountAsync(r => r.BookId == book.Id && r.IsLike, ct);
        book.DislikesCount = await db.BookReactions.CountAsync(r => r.BookId == book.Id && !r.IsLike, ct);
        book.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
