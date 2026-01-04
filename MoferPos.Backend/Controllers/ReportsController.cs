using Microsoft.AspNetCore.Mvc;
using MoferPOS.Backend.Application.Reports;

namespace MoferPOS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports)
    {
        _reports = reports;
    }

    // GET /api/reports/sales-summary?organizationId=...&locationId=...&from=...&to=...
    [HttpGet("sales-summary")]
    public async Task<IActionResult> SalesSummary(
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? locationId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken ct)
    {
        try
        {
            var result = await _reports.GetSalesSummaryAsync(organizationId, locationId, from, to, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
