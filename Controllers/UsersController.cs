using System.Security.Claims;
using Elibrary.Api.Data;
using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Models;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Controllers;

[ApiController]
[Authorize(Roles = "super_admin")]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    private readonly RealtimeService _realtime;

    public UsersController(AppDbContext db, AuditService audit, RealtimeService realtime)
    {
        _db = db;
        _audit = audit;
        _realtime = realtime;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        var users = await _db.Users.AsNoTracking()
            .Where(u => u.Role == "admin" || u.Role == "super_admin")
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto(u.Id, u.Email, u.FullName, u.Role, u.IsActive))
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        var emailErr = Validators.Email(req.Email);
        if (emailErr != null) return BadRequest(new { message = emailErr });
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });
        if (string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new { message = "Full name is required." });

        var email = req.Email.Trim().ToLower();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return BadRequest(new { message = "Email already exists." });

        var role = req.Role is "super_admin" or "admin" ? req.Role : "admin";
        var user = new User
        {
            Email = email,
            FullName = req.FullName.Trim(),
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("create", "user", user.Id.ToString(), user.Email);
        var dto = new UserDto(user.Id, user.Email, user.FullName, user.Role, user.IsActive);
        await _realtime.PublishAsync("user", "create", dto);
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Id == me && !req.IsActive)
            return BadRequest(new { message = "You cannot deactivate your own account." });

        user.FullName = req.FullName.Trim();
        user.Role = req.Role is "super_admin" or "admin" ? req.Role : user.Role;
        user.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("update", "user", user.Id.ToString(), user.Email);
        var dto = new UserDto(user.Id, user.Email, user.FullName, user.Role, user.IsActive);
        await _realtime.PublishAsync("user", "update", dto);
        return Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var me = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == me) return BadRequest(new { message = "You cannot delete your own account." });
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        await _audit.LogAsync("delete", "user", id.ToString(), user.Email);
        await _realtime.PublishAsync("user", "delete", new { id });
        return NoContent();
    }
}
