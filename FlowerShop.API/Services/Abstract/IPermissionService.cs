using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IPermissionService
{
    Task<BaseResponse<List<PermissionOutputResource>>> GetAllPermissionsAsync();
    Task<BaseResponse<PermissionOutputResource>> GetPermissionByIdAsync(long permissionId);
}
