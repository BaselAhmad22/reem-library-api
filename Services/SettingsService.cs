using Elibrary.Api.Data;
using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Services;

public class SettingsService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    private readonly RealtimeService _realtime;

    public SettingsService(AppDbContext db, AuditService audit, RealtimeService realtime)
    {
        _db = db;
        _audit = audit;
        _realtime = realtime;
    }

    public async Task<SettingsDto> GetAsync(CancellationToken ct = default)
    {
        var s = await _db.LibrarySettings.AsNoTracking().FirstAsync(ct);
        return EntityMappers.MapSettings(s);
    }

    public async Task<(SettingsDto? Result, string? Error)> UpdateAsync(SettingsDto req, CancellationToken ct = default)
    {
        var err = Validators.FirstError(
            Validators.Require(req.NameAr, "NameAr"),
            Validators.Require(req.NameEn, "NameEn"),
            string.IsNullOrWhiteSpace(req.Email) ? null : Validators.Email(req.Email),
            Validators.Phone(req.Phone),
            Validators.Phone(req.WhatsApp));
        if (!string.IsNullOrEmpty(err)) return (null, err);

        var s = await _db.LibrarySettings.FirstAsync(ct);
        s.NameAr = req.NameAr.Trim();
        s.NameEn = req.NameEn.Trim();
        s.TaglineAr = req.TaglineAr?.Trim() ?? "";
        s.TaglineEn = req.TaglineEn?.Trim() ?? "";
        s.AboutAr = req.AboutAr?.Trim() ?? "";
        s.AboutEn = req.AboutEn?.Trim() ?? "";
        s.Email = req.Email?.Trim() ?? "";
        s.Phone = req.Phone?.Trim() ?? "";
        s.AddressAr = req.AddressAr?.Trim() ?? "";
        s.AddressEn = req.AddressEn?.Trim() ?? "";
        s.WhatsApp = req.WhatsApp?.Trim() ?? "";

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("update", "settings", s.Id.ToString(), "Library settings updated");
        var dto = EntityMappers.MapSettings(s);
        await _realtime.PublishAsync("settings", "update", dto);
        return (dto, null);
    }
}
