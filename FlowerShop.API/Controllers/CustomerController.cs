using FlowerShop.API.Helpers;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdmin")]
[Route("api/admin/customers")]
public class AdminCustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public AdminCustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CustomerQueryResource queryResource)
    {
        var response = await _customerService.GetCustomersAsync(queryResource);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var response = await _customerService.GetCustomerByIdAsync(id);
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] CustomerInputResource input)
    {
        var response = await _customerService.UpdateCustomerAsync(id, input);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var response = await _customerService.DeleteCustomerAsync(id);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}