using Microsoft.AspNetCore.Mvc;
using MoferPOS.Backend.Application.Menu;

namespace MoferPOS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menu;

    public MenuController(IMenuService menu)
    {
        _menu = menu;
    }

    // GET /api/menu?organizationId=...&locationId=...
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid organizationId, [FromQuery] Guid locationId, CancellationToken ct)
    {
        try
        {
            var result = await _menu.GetMenuAsync(organizationId, locationId, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

