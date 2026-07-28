using System.Security.Claims;
using Elibrary.Api.Data;
using Elibrary.Api.Models;

namespace Elibrary.Api.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(string action, string entityType, string? entityId = null, string details = "")
    {
        var user = _http.HttpContext?.User;
        var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = user?.FindFirstValue(ClaimTypes.Email) ?? "system";

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = int.TryParse(userId, out var id) ? id : null,
            UserEmail = email,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
