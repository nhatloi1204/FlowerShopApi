using FlowerShop.API.Data;
using FlowerShop.API.Models.DTOs.Admin.Role;
using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    #region Role Management

    public async Task<AuthResponse<RoleResponse>> CreateRoleAsync(CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Role title is required"
            };
        }

        // Check if role already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Title == request.Title);

        if (existingRole != null)
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Role already exists"
            };
        }

        var role = new Role
        {
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return new AuthResponse<RoleResponse>
        {
            Success = true,
            Message = "Role created successfully",
            Data = MapToRoleResponse(role)
        };
    }

    public async Task<AuthResponse<RoleResponse>> UpdateRoleAsync(long roleId, UpdateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Role title is required"
            };
        }

        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Role not found"
            };
        }

        role.Title = request.Title;
        role.UpdatedAt = DateTime.UtcNow;

        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        return new AuthResponse<RoleResponse>
        {
            Success = true,
            Message = "Role updated successfully",
            Data = MapToRoleResponse(role)
        };
    }

    public async Task<AuthResponse<List<RoleResponse>>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.Permissions)
            .ToListAsync();

        var roleResponses = roles.Select(MapToRoleResponse).ToList();

        return new AuthResponse<List<RoleResponse>>
        {
            Success = true,
            Message = "Roles retrieved successfully",
            Data = roleResponses
        };
    }

    public async Task<AuthResponse<RoleResponse>> GetRoleByIdAsync(long roleId)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Role not found"
            };
        }

        return new AuthResponse<RoleResponse>
        {
            Success = true,
            Message = "Role retrieved successfully",
            Data = MapToRoleResponse(role)
        };
    }

    public async Task<AuthResponse<string>> DeleteRoleAsync(long roleId)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return new AuthResponse<string>
            {
                Success = false,
                Message = "Role not found"
            };
        }

        // Check if any users have this role
        var usersWithRole = await _context.RoleUsers
            .Where(ru => ru.RoleId == roleId)
            .CountAsync();

        if (usersWithRole > 0)
        {
            return new AuthResponse<string>
            {
                Success = false,
                Message = $"Cannot delete role. {usersWithRole} user(s) have this role."
            };
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return new AuthResponse<string>
        {
            Success = true,
            Message = "Role deleted successfully"
        };
    }

    #endregion

    #region Permission Management

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

    public async Task<AuthResponse<RoleResponse>> AssignPermissionsToRoleAsync(long roleId, AssignPermissionsRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Role not found"
            };
        }

        // Get all permissions to assign
        var permissionsToAdd = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.Id))
            .ToListAsync();

        if (permissionsToAdd.Count != request.PermissionIds.Count)
        {
            return new AuthResponse<RoleResponse>
            {
                Success = false,
                Message = "Some permissions not found"
            };
        }

        // Remove existing permissions
        var existingPermissions = await _context.PermissionRoles
            .Where(pr => pr.RoleId == roleId)
            .ToListAsync();

        _context.PermissionRoles.RemoveRange(existingPermissions);

        // Add new permissions
        foreach (var permission in permissionsToAdd)
        {
            _context.PermissionRoles.Add(new PermissionRole
            {
                RoleId = roleId,
                PermissionId = permission.Id
            });
        }

        await _context.SaveChangesAsync();

        // Reload role with permissions
        role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        return new AuthResponse<RoleResponse>
        {
            Success = true,
            Message = "Permissions assigned to role successfully",
            Data = MapToRoleResponse(role)
        };
    }

    public async Task<AuthResponse<List<PermissionResponse>>> GetRolePermissionsAsync(long roleId)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return new AuthResponse<List<PermissionResponse>>
            {
                Success = false,
                Message = "Role not found"
            };
        }

        var permissionResponses = role.Permissions
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
            Message = "Role permissions retrieved successfully",
            Data = permissionResponses
        };
    }

    public async Task<AuthResponse<string>> RemovePermissionFromRoleAsync(long roleId, long permissionId)
    {
        var permissionRole = await _context.PermissionRoles
            .FirstOrDefaultAsync(pr => pr.RoleId == roleId && pr.PermissionId == permissionId);

        if (permissionRole == null)
        {
            return new AuthResponse<string>
            {
                Success = false,
                Message = "Permission not assigned to this role"
            };
        }

        _context.PermissionRoles.Remove(permissionRole);
        await _context.SaveChangesAsync();

        return new AuthResponse<string>
        {
            Success = true,
            Message = "Permission removed from role successfully"
        };
    }

    #endregion

    #region Helper Methods

    private RoleResponse MapToRoleResponse(Role role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Title = role.Title,
            CreatedAt = role.CreatedAt,
            Permissions = role.Permissions?
                .Select(p => new PermissionResponse
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description
                })
                .ToList() ?? new List<PermissionResponse>()
        };
    }

    #endregion
}
