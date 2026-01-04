using Microsoft.AspNetCore.Mvc;
using MoferPOS.Backend.Api.Contracts;
using MoferPOS.Backend.Application.Orders;
using MoferPOS.Backend.Application.Orders.Queries;

namespace MoferPOS.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;
    private readonly IOrderQueryService _orderQueries;

    public OrdersController(IOrderService orders, IOrderQueryService orderQueries)
    {
        _orders = orders;
        _orderQueries = orderQueries;
    }

    // POST /api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _orders.CreateOrderAsync(request, ct);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/orders?organizationId=...&locationId=...&status=Completed&from=...&to=...&page=1&pageSize=25
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid organizationId,
        [FromQuery] Guid? locationId,
        [FromQuery] string? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _orderQueries.GetOrdersAsync(
                organizationId,
                locationId,
                status,
                from,
                to,
                page,
                pageSize,
                ct);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
