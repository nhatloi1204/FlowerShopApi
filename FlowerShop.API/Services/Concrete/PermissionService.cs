using FlowerShop.API.Data;
using FlowerShop.API.Models.DTOs.Admin.Role;
using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponse<List<PermissionResponse>>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Title)
            .ToListAsync();

        var permissionResponses = permissions
            .Select(p => new PermissionResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description
            })
            .ToList();

        return new AuthResponse<List<PermissionResponse>>
        {
            Success = true,
            Message = "Permissions retrieved successfully",
            Data = permissionResponses
        };
    }

    public async Task<AuthResponse<PermissionResponse>> GetPermissionByIdAsync(long permissionId)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == permissionId);

        if (permission == null)
        {
            return new AuthResponse<PermissionResponse>
            {
                Success = false,
                Message = "Permission not found"
            };
        }

        var permissionResponse = new PermissionResponse
        {
            Id = permission.Id,
            Title = permission.Title,
            Description = permission.Description
        };

        return new AuthResponse<PermissionResponse>
        {
            Success = true,
            Message = "Permission retrieved successfully",
            Data = permissionResponse
        };
    }
}
