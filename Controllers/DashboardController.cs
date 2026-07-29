using Elibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elibrary.Api.Controllers;

[ApiController]
[Authorize(Roles = "admin,super_admin")]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly LibraryContentService _content;

    public DashboardController(LibraryContentService content) => _content = content;

    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
        => Ok(await _content.GetDashboardStatsAsync(ct));
}
