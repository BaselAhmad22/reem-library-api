using Elibrary.Api.Dtos;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elibrary.Api.Controllers;

[ApiController]
[Authorize(Roles = "admin,super_admin")]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categories;

    public CategoriesController(CategoryService categories) => _categories = categories;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> List(CancellationToken ct)
        => Ok(await _categories.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryRequest req, CancellationToken ct)
    {
        var (result, error) = await _categories.CreateAsync(req, ct);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryRequest req, CancellationToken ct)
    {
        var (result, error) = await _categories.UpdateAsync(id, req, ct);
        if (error == "Not found") return NotFound();
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var (ok, error) = await _categories.DeleteAsync(id, ct);
        if (!ok) return error == "Not found" ? NotFound() : BadRequest(new { message = error });
        return NoContent();
    }
}
