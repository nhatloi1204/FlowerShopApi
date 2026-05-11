using FlowerShop.API.Models.DTOs.Admin.Role;
using FlowerShop.API.Models.DTOs.Auth;

namespace FlowerShop.API.Services.Abstract;

public interface IAdminService
{
    // Role Management
    Task<AuthResponse<RoleResponse>> CreateRoleAsync(CreateRoleRequest request);
    Task<AuthResponse<RoleResponse>> UpdateRoleAsync(long roleId, UpdateRoleRequest request);
    Task<AuthResponse<List<RoleResponse>>> GetAllRolesAsync();
    Task<AuthResponse<RoleResponse>> GetRoleByIdAsync(long roleId);
    Task<AuthResponse<string>> DeleteRoleAsync(long roleId);

    // Permission Management for Role
    Task<AuthResponse<RoleResponse>> AssignPermissionsToRoleAsync(long roleId, AssignPermissionsRequest request);
    Task<AuthResponse<List<PermissionResponse>>> GetAllPermissionsAsync();
    Task<AuthResponse<List<PermissionResponse>>> GetRolePermissionsAsync(long roleId);
    Task<AuthResponse<string>> RemovePermissionFromRoleAsync(long roleId, long permissionId);
}
