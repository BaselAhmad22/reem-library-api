namespace Elibrary.Api.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    /// <summary>English PDF path or absolute URL.</summary>
    public string DownloadUrl { get; set; } = string.Empty;
    /// <summary>Arabic PDF path or absolute URL.</summary>
    public string DownloadUrlAr { get; set; } = string.Empty;
    public int? GutenbergId { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int? PublishedYear { get; set; }
    public string Language { get; set; } = "ar";
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public int DownloadCount { get; set; }
    public double AverageRating { get; set; }
    public int RatingsCount { get; set; }
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookRating> Ratings { get; set; } = [];
    public ICollection<BookReaction> Reactions { get; set; } = [];
    public ICollection<BookComment> Comments { get; set; } = [];
}
