using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api")]
public class HealthCheckController : ControllerBase
{
    /// <summary>
    /// Health check endpoint - no auth required
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Test endpoint - shows current user claims
    /// </summary>
    [HttpGet("debug/claims")]
    public IActionResult GetCurrentUserClaims()
    {
        var claims = User.Claims
            .Select(c => new { c.Type, c.Value })
            .ToList();

        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            name = User.Identity?.Name,
            claims = claims
        });
    }
}
