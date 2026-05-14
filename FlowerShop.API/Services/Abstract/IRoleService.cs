using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IRoleService
{
    Task<BaseResponse<RoleOutputResource>> CreateRoleAsync(RoleInputResource request);
    Task<BaseResponse<RoleOutputResource>> UpdateRoleAsync(long roleId, RoleInputResource request);
    Task<BaseResponse<bool>> DeleteRoleAsync(long roleId);
    Task<BaseResponse<RoleOutputResource>> GetRoleByIdAsync(long roleId);
    Task<BaseResponse<List<RoleOutputResource>>> GetAllRolesAsync();
    Task<BaseResponse<bool>> AssignPermissionsAsync(long roleId, AssignPermissionsInputResource request);
}
