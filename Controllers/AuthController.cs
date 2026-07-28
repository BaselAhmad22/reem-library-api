using Elibrary.Api.Data;
using Elibrary.Api.Dtos;
using Elibrary.Api.Models;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Elibrary.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly AuditService _audit;

    public AuthController(AppDbContext db, JwtTokenService jwt, AuditService audit)
    {
        _db = db;
        _jwt = jwt;
        _audit = audit;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = _jwt.CreateToken(user);
        await _audit.LogAsync("login", "user", user.Id.ToString(), $"Login: {user.Email}");
        return Ok(new LoginResponse(token, ToDto(user)));
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(id);
        if (user is null || !user.IsActive) return Unauthorized();
        return Ok(ToDto(user));
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout()
    {
        await _audit.LogAsync("logout", "user", null, "Logout");
        return Ok(new { message = "Logged out." });
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Email, u.FullName, u.Role, u.IsActive);
}
