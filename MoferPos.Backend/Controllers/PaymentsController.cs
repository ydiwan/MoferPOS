using Microsoft.AspNetCore.Mvc;
using MoferPOS.Backend.Api.Contracts.Payments;
using MoferPOS.Backend.Application.Payments;

namespace MoferPOS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments)
    {
        _payments = payments;
    }

    // POST /api/payments/{paymentId}/terminal-result
    [HttpPost("{paymentId:guid}/terminal-result")]
    public async Task<IActionResult> ApplyTerminalResult(
        [FromRoute] Guid paymentId,
        [FromBody] TerminalResultRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _payments.ApplyTerminalResultAsync(paymentId, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

