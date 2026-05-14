using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IAdminService
{
    // Role Management
    Task<BaseResponse<RoleOutputResource>> CreateRoleAsync(RoleInputResource request);
    Task<BaseResponse<RoleOutputResource>> UpdateRoleAsync(long roleId, RoleInputResource request);
    Task<BaseResponse<List<RoleOutputResource>>> GetAllRolesAsync();
    Task<BaseResponse<RoleOutputResource>> GetRoleByIdAsync(long roleId);
    Task<BaseResponse<bool>> DeleteRoleAsync(long roleId);

    // Permission Management for Role
    Task<BaseResponse<RoleOutputResource>> AssignPermissionsToRoleAsync(long roleId, AssignPermissionsInputResource request);
    Task<BaseResponse<List<PermissionOutputResource>>> GetAllPermissionsAsync();
    Task<BaseResponse<List<PermissionOutputResource>>> GetRolePermissionsAsync(long roleId);
    Task<BaseResponse<bool>> RemovePermissionFromRoleAsync(long roleId, long permissionId);
}
