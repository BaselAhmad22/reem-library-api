using Elibrary.Api.Dtos;
using Elibrary.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Elibrary.Api.Controllers;

[ApiController]
[Authorize(Roles = "admin,super_admin")]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly SettingsService _settings;

    public SettingsController(SettingsService settings) => _settings = settings;

    [HttpGet]
    public async Task<ActionResult<SettingsDto>> Get(CancellationToken ct)
        => Ok(await _settings.GetAsync(ct));

    [HttpPut]
    public async Task<ActionResult<SettingsDto>> Update([FromBody] SettingsDto req, CancellationToken ct)
    {
        var (result, error) = await _settings.UpdateAsync(req, ct);
        if (error != null) return BadRequest(new { message = error });
        return Ok(result);
    }
}
