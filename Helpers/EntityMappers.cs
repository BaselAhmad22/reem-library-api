using Elibrary.Api.Dtos;
using Elibrary.Api.Models;

namespace Elibrary.Api.Helpers;

public static class EntityMappers
{
    public static SettingsDto MapSettings(LibrarySettings s) => new(
        s.NameAr, s.NameEn, s.TaglineAr, s.TaglineEn, s.AboutAr, s.AboutEn,
        s.Email, s.Phone, s.AddressAr, s.AddressEn, s.WhatsApp);

    public static BookDto MapBook(Book b) => new(
        b.Id, b.Title, b.Author, b.Description, b.CoverUrl, b.DownloadUrl, b.Isbn, b.PublishedYear,
        b.Language, b.IsFeatured, b.IsActive,
        b.DownloadCount, b.AverageRating, b.RatingsCount, b.LikesCount, b.DislikesCount, b.CommentsCount,
        b.CategoryId,
        b.Category?.NameAr ?? "", b.Category?.NameEn ?? "", b.Category?.Slug ?? "",
        b.CreatedAt, b.UpdatedAt);

    public static BookCommentDto MapComment(BookComment c) => new(
        c.Id, c.BookId, c.UserId, c.User?.FullName ?? "User", c.Body, c.CreatedAt);
}
