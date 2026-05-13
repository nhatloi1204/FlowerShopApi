using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/admin/permissions")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all permissions (SuperAdmin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get permission by ID (SuperAdmin only)
    /// </summary>
    [HttpGet("{permissionId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetPermissionById(long permissionId)
    {
        var result = await _permissionService.GetPermissionByIdAsync(permissionId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}
