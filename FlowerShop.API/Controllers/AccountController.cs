using FlowerShop.API.Helpers;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerAddressService _addressService;

    public AccountController(
        ICustomerService customerService,
        ICustomerAddressService addressService)
    {
        _customerService = customerService;
        _addressService = addressService;
    }

    #region Profile Management

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var response = await _customerService.GetCustomerByIdAsync(User.GetUserId());
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] CustomerInputResource input)
    {
        var response = await _customerService.UpdateCustomerAsync(User.GetUserId(), input);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    #endregion

    #region Address Management

    [HttpPost("addresses")]
    public async Task<IActionResult> CreateAddress([FromBody] CustomerAddressInputResource input)
    {
        var response = await _addressService.CreateAddressAsync(User.GetUserId(), input);
        return Ok(response);
    }

    [HttpPut("addresses/{id:long}")]
    public async Task<IActionResult> UpdateAddress(long id, [FromBody] CustomerAddressInputResource input)
    {
        var response = await _addressService.UpdateAddressAsync(id, User.GetUserId(), input);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete("addresses/{id:long}")]
    public async Task<IActionResult> DeleteAddress(long id)
    {
        var response = await _addressService.DeleteAddressAsync(id, User.GetUserId());
        return response.Success ? Ok(response) : BadRequest(response);
    }

    #endregion
}