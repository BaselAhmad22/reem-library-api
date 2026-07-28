using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Models;
using Elibrary.Api.Repositories;

namespace Elibrary.Api.Services;

public class CategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly AuditService _audit;
    private readonly RealtimeService _realtime;

    public CategoryService(ICategoryRepository categories, AuditService audit, RealtimeService realtime)
    {
        _categories = categories;
        _audit = audit;
        _realtime = realtime;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _categories.GetAllOrderedAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<(CategoryDto? Result, string? Error)> CreateAsync(CategoryRequest req, CancellationToken ct = default)
    {
        var err = Validators.FirstError(Validators.Require(req.NameAr, "NameAr"), Validators.Require(req.NameEn, "NameEn"));
        if (!string.IsNullOrEmpty(err)) return (null, err);

        var slug = string.IsNullOrWhiteSpace(req.Slug) ? Validators.Slugify(req.NameEn) : Validators.Slugify(req.Slug);
        if (await _categories.SlugExistsAsync(slug, ct: ct))
            return (null, "Slug already exists.");

        var cat = new Category
        {
            NameAr = req.NameAr.Trim(),
            NameEn = req.NameEn.Trim(),
            Slug = slug,
            SortOrder = req.SortOrder,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        await _categories.AddAsync(cat, ct);
        await _categories.SaveChangesAsync(ct);
        await _audit.LogAsync("create", "category", cat.Id.ToString(), cat.NameEn);
        var dto = new CategoryDto(cat.Id, cat.NameAr, cat.NameEn, cat.Slug, cat.SortOrder, cat.IsActive, 0);
        await _realtime.PublishAsync("category", "create", dto);
        return (dto, null);
    }

    public async Task<(CategoryDto? Result, string? Error)> UpdateAsync(int id, CategoryRequest req, CancellationToken ct = default)
    {
        var err = Validators.FirstError(Validators.Require(req.NameAr, "NameAr"), Validators.Require(req.NameEn, "NameEn"));
        if (!string.IsNullOrEmpty(err)) return (null, err);

        var cat = await _categories.GetByIdWithBooksAsync(id, ct);
        if (cat is null) return (null, "Not found");

        var slug = string.IsNullOrWhiteSpace(req.Slug) ? Validators.Slugify(req.NameEn) : Validators.Slugify(req.Slug);
        if (await _categories.SlugExistsAsync(slug, id, ct))
            return (null, "Slug already exists.");

        cat.NameAr = req.NameAr.Trim();
        cat.NameEn = req.NameEn.Trim();
        cat.Slug = slug;
        cat.SortOrder = req.SortOrder;
        cat.IsActive = req.IsActive;
        _categories.Update(cat);
        await _categories.SaveChangesAsync(ct);
        await _audit.LogAsync("update", "category", cat.Id.ToString(), cat.NameEn);
        var dto = new CategoryDto(cat.Id, cat.NameAr, cat.NameEn, cat.Slug, cat.SortOrder, cat.IsActive, cat.Books.Count);
        await _realtime.PublishAsync("category", "update", dto);
        return (dto, null);
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var cat = await _categories.GetByIdWithBooksAsync(id, ct);
        if (cat is null) return (false, "Not found");
        if (cat.Books.Count > 0)
            return (false, "Cannot delete category with books. Move or delete books first.");
        _categories.Remove(cat);
        await _categories.SaveChangesAsync(ct);
        await _audit.LogAsync("delete", "category", id.ToString(), cat.NameEn);
        await _realtime.PublishAsync("category", "delete", new { id });
        return (true, null);
    }

    private static CategoryDto Map(Category c) =>
        new(c.Id, c.NameAr, c.NameEn, c.Slug, c.SortOrder, c.IsActive, c.Books.Count);
}
