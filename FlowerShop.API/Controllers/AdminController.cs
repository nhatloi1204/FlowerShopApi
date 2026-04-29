using FlowerShop.API.Models.DTOs.Admin.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Admin gateway endpoint - Check admin access
    /// </summary>
    [HttpGet("health")]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult Health()
    {
        return Ok(new { message = "Admin access granted" });
    }
}
