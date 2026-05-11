using FlowerShop.API.Models.DTOs.Admin.Role;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Get all roles (SuperAdmin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _roleService.GetAllRolesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get role by ID (SuperAdmin only)
    /// </summary>
    [HttpGet("{roleId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetRoleById(long roleId)
    {
        var result = await _roleService.GetRoleByIdAsync(roleId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Create new role (SuperAdmin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateRoleAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetRoleById), new { roleId = result.Data.Id }, result);
    }

    /// <summary>
    /// Update role (SuperAdmin only)
    /// </summary>
    [HttpPut("{roleId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRole(long roleId, [FromBody] UpdateRoleRequest request)
    {
        var result = await _roleService.UpdateRoleAsync(roleId, request);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete role (SuperAdmin only)
    /// </summary>
    [HttpDelete("{roleId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteRole(long roleId)
    {
        var result = await _roleService.DeleteRoleAsync(roleId);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Assign permissions to role (SuperAdmin only)
    /// </summary>
    [HttpPost("{roleId}/permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AssignPermissions(long roleId, [FromBody] AssignPermissionsRequest request)
    {
        var result = await _roleService.AssignPermissionsAsync(roleId, request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
