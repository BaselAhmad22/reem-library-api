using Elibrary.Api.Dtos;
using Elibrary.Api.Helpers;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Elibrary.Api.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly LibraryContentService _content;

    public PublicController(LibraryContentService content) => _content = content;

    [HttpGet("content")]
    public async Task<ActionResult<PublicContentDto>> Content(CancellationToken ct)
        => Ok(await _content.GetPublicContentAsync(ct));

    [HttpGet("books/{id:int}")]
    public async Task<ActionResult<BookDto>> Book(int id, CancellationToken ct)
    {
        var book = await _content.GetPublicBookAsync(id, ct);
        if (book is null) return NotFound();
        return Ok(book);
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "ok", time = DateTime.UtcNow });
}
