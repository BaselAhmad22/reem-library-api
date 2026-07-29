using System.Security.Claims;
using Elibrary.Api.Dtos;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elibrary.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly LibraryContentService _content;
    private readonly BookEngagementService _engagement;

    public PublicController(LibraryContentService content, BookEngagementService engagement)
    {
        _content = content;
        _engagement = engagement;
    }

    [HttpGet("content")]
    public async Task<ActionResult<PublicContentDto>> Content(CancellationToken ct)
        => Ok(await _content.GetPublicContentAsync(ct));

    [HttpGet("books/top-rated")]
    public async Task<ActionResult<IEnumerable<BookDto>>> TopRated([FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _content.GetTopRatedAsync(Math.Clamp(take, 1, 50), ct));

    [HttpGet("books/{id:int}")]
    public async Task<ActionResult<BookDetailDto>> Book(int id, CancellationToken ct)
    {
        var userId = TryGetUserId();
        var detail = await _engagement.GetDetailAsync(id, userId, ct);
        if (detail is null) return NotFound();
        return Ok(detail);
    }

    [Authorize]
    [HttpPost("books/{id:int}/download")]
    public async Task<ActionResult<DownloadResponse>> Download(int id, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (result, error) = await _engagement.DownloadAsync(id, userId, ct);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("books/{id:int}/rate")]
    public async Task<ActionResult<BookDto>> Rate(int id, [FromBody] RateBookRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (result, error) = await _engagement.RateAsync(id, userId, req.Stars, ct);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("books/{id:int}/reaction")]
    public async Task<ActionResult<BookDto>> React(int id, [FromBody] ReactBookRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (result, error) = await _engagement.ReactAsync(id, userId, req.Like, ct);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("books/{id:int}/comments")]
    public async Task<ActionResult<BookCommentDto>> Comment(int id, [FromBody] CommentRequest req, CancellationToken ct)
    {
        var userId = RequireUserId();
        var (result, error) = await _engagement.CommentAsync(id, userId, req.Body, ct);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", time = DateTime.UtcNow });

    private int? TryGetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, out var id) ? id : null;
    }

    private int RequireUserId()
    {
        var id = TryGetUserId();
        if (id is null) throw new UnauthorizedAccessException();
        return id.Value;
    }
}
