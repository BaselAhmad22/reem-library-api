using System.ComponentModel.DataAnnotations;

namespace Elibrary.Api.Dtos;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required, MinLength(2)] string FullName);

public record LoginResponse(string Token, UserDto User);

public record UserDto(int Id, string Email, string FullName, string Role, bool IsActive);

public record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string FullName,
    string Role = "admin");

public record UpdateUserRequest(
    [Required] string FullName,
    string Role,
    bool IsActive);

public record CategoryDto(int Id, string NameAr, string NameEn, string Slug, int SortOrder, bool IsActive, int BooksCount);

public record CategoryRequest(
    [Required] string NameAr,
    [Required] string NameEn,
    string? Slug,
    int SortOrder = 0,
    bool IsActive = true);

public record BookDto(
    int Id,
    string Title,
    string Author,
    string Description,
    string CoverUrl,
    string DownloadUrl,
    string Isbn,
    int? PublishedYear,
    string Language,
    bool IsFeatured,
    bool IsActive,
    int DownloadCount,
    double AverageRating,
    int RatingsCount,
    int LikesCount,
    int DislikesCount,
    int CommentsCount,
    int CategoryId,
    string CategoryNameAr,
    string CategoryNameEn,
    string CategorySlug,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record BookRequest(
    [Required] string Title,
    [Required] string Author,
    string Description = "",
    string CoverUrl = "",
    string DownloadUrl = "",
    string Isbn = "",
    int? PublishedYear = null,
    string Language = "ar",
    bool IsFeatured = false,
    bool IsActive = true,
    [Required] int CategoryId = 0);

public record BookCommentDto(
    int Id,
    int BookId,
    int UserId,
    string UserName,
    string Body,
    DateTime CreatedAt);

public record BookDetailDto(
    BookDto Book,
    IEnumerable<BookCommentDto> Comments,
    int? MyRating,
    bool? MyLiked);

public record RateBookRequest([Range(1, 5)] int Stars);

public record ReactBookRequest(bool? Like);

public record CommentRequest([Required, MinLength(2), MaxLength(2000)] string Body);

public record DownloadResponse(string DownloadUrl, int DownloadCount);

public record SettingsDto(
    string NameAr,
    string NameEn,
    string TaglineAr,
    string TaglineEn,
    string AboutAr,
    string AboutEn,
    string Email,
    string Phone,
    string AddressAr,
    string AddressEn,
    string WhatsApp);

public record PublicContentDto(
    SettingsDto Settings,
    IEnumerable<CategoryDto> Categories,
    IEnumerable<BookDto> FeaturedBooks,
    IEnumerable<BookDto> TopRatedBooks,
    IEnumerable<BookDto> Books,
    int TotalBooks);
