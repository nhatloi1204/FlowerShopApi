using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // =========================================================================
    // CLIENT ROUTES - MUST BE AUTHENTICATED AS CUSTOMER (ROLE IS NOT SUPERADMIN) TO ACCESS
    // =========================================================================

    [HttpPost("orders")]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateInputResource request)
    {
        var customerId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _orderService.CreateOrderAsync(customerId, request);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetCustomerOrderDetail), new { id = result.Data?.Id }, result);
    }

    [HttpGet("orders/history")]
    [Authorize]
    public async Task<IActionResult> GetCustomerHistory([FromQuery] OrderQueryResource query)
    {
        var customerId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _orderService.GetCustomerOrderHistoryAsync(customerId, query);
        return Ok(result);
    }

    [HttpGet("orders/{id}")]
    [Authorize]
    public async Task<IActionResult> GetCustomerOrderDetail(long id)
    {
        var customerId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _orderService.GetCustomerOrderDetailAsync(customerId, id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("orders/{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelOrder(long id)
    {
        var customerId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _orderService.CancelOrderAsync(customerId, id);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // =========================================================================
    // ADMIN ROUTES - ONLY ACCESSIBLE BY ROLE
    // =========================================================================

    [HttpGet("admin/orders")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAdminOrders([FromQuery] OrderQueryResource query)
    {
        var result = await _orderService.GetAdminOrdersAsync(query);
        return Ok(result);
    }

    [HttpGet("admin/orders/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAdminOrderDetail(long id)
    {
        var result = await _orderService.GetAdminOrderDetailAsync(id);

        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPut("admin/orders/{id}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] string newStatus)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}