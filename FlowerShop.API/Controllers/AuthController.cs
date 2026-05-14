using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<LoginOutputResource>>> Register([FromBody] RegisterInputResource request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<LoginOutputResource>>> Login([FromBody] LoginInputResource request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("external-login")]
    public async Task<ActionResult<BaseResponse<LoginOutputResource>>> ExternalLogin([FromBody] ExternalAuthInputResource request)
    {
        var result = await _authService.ExternalLoginAsync(request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
