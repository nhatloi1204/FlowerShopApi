using FlowerShop.API.Models.DTOs.Admin.Role;
using FlowerShop.API.Models.DTOs.Auth;

namespace FlowerShop.API.Services.Abstract;

public interface IRoleService
{
    Task<AuthResponse<RoleResponse>> CreateRoleAsync(CreateRoleRequest request);
    Task<AuthResponse<RoleResponse>> UpdateRoleAsync(long roleId, UpdateRoleRequest request);
    Task<AuthResponse<bool>> DeleteRoleAsync(long roleId);
    Task<AuthResponse<RoleResponse>> GetRoleByIdAsync(long roleId);
    Task<AuthResponse<List<RoleResponse>>> GetAllRolesAsync();
    Task<AuthResponse<bool>> AssignPermissionsAsync(long roleId, AssignPermissionsRequest request);
}
