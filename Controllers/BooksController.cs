using Elibrary.Api.Dtos;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elibrary.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly BookService _books;

    public BooksController(BookService books) => _books = books;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> List(CancellationToken ct)
        => Ok(await _books.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> Get(int id, CancellationToken ct)
    {
        var book = await _books.GetByIdAsync(id, ct);
        if (book is null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create([FromBody] BookRequest req, CancellationToken ct)
    {
        var (result, error) = await _books.CreateAsync(req, ct);
        if (error != null) return BadRequest(new { message = error });
        return CreatedAtAction(nameof(Get), new { id = result!.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookDto>> Update(int id, [FromBody] BookRequest req, CancellationToken ct)
    {
        var (result, error) = await _books.UpdateAsync(id, req, ct);
        if (error == "Not found") return NotFound();
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _books.DeleteAsync(id, ct);
        if (!ok) return error == "Not found" ? NotFound() : BadRequest(new { message = error });
        return NoContent();
    }
}
