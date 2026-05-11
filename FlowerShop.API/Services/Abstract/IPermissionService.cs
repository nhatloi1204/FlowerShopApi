using FlowerShop.API.Models.DTOs.Admin.Role;
using FlowerShop.API.Models.DTOs.Auth;

namespace FlowerShop.API.Services.Abstract;

public interface IPermissionService
{
    Task<AuthResponse<List<PermissionResponse>>> GetAllPermissionsAsync();
    Task<AuthResponse<PermissionResponse>> GetPermissionByIdAsync(long permissionId);
}
