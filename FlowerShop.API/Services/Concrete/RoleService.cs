using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public RoleService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BaseResponse<RoleOutputResource>> CreateRoleAsync(RoleInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BaseResponse<RoleOutputResource>.Fail("Role title is required");
        }

        // Check if role already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Title == request.Title);

        if (existingRole != null)
        {
            return BaseResponse<RoleOutputResource>.Fail("Role already exists");
        }

        var role = _mapper.Map<Role>(request);
        role.CreatedAt = DateTime.UtcNow;
        role.UpdatedAt = DateTime.UtcNow;

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<RoleOutputResource>(role);
        return BaseResponse<RoleOutputResource>.Ok(response, "Role created successfully");
    }

    public async Task<BaseResponse<RoleOutputResource>> UpdateRoleAsync(long roleId, RoleInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BaseResponse<RoleOutputResource>.Fail("Role title is required");
        }

        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return BaseResponse<RoleOutputResource>.Fail("Role not found");
        }

        _mapper.Map(request, role);

        _context.Roles.Update(role);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<RoleOutputResource>(role);
        return BaseResponse<RoleOutputResource>.Ok(response, "Role updated successfully");
    }

    public async Task<BaseResponse<List<RoleOutputResource>>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.Permissions)
            .ToListAsync();

        var response = _mapper.Map<List<RoleOutputResource>>(roles);
        return BaseResponse<List<RoleOutputResource>>.Ok(response, "Roles retrieved successfully");
    }

    public async Task<BaseResponse<RoleOutputResource>> GetRoleByIdAsync(long roleId)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return BaseResponse<RoleOutputResource>.Fail("Role not found");
        }

        var response = _mapper.Map<RoleOutputResource>(role);
        return BaseResponse<RoleOutputResource>.Ok(response, "Role retrieved successfully");
    }

    public async Task<BaseResponse<bool>> DeleteRoleAsync(long roleId)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return BaseResponse<bool>.Fail("Role not found");
        }

        // Check if any users have this role
        var usersWithRole = await _context.RoleUsers
            .Where(ru => ru.RoleId == roleId)
            .CountAsync();

        if (usersWithRole > 0)
        {
            return BaseResponse<bool>.Fail($"Cannot delete role. {usersWithRole} user(s) have this role.");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.Ok(true, "Role deleted successfully");
    }

    public async Task<BaseResponse<bool>> AssignPermissionsAsync(long roleId, AssignPermissionsInputResource request)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
        {
            return BaseResponse<bool>.Fail("Role not found");
        }

        // Get all permissions to assign
        var permissionsToAdd = await _context.Permissions
            .Where(p => request.PermissionIds.Contains(p.Id))
            .ToListAsync();

        if (permissionsToAdd.Count != request.PermissionIds.Count)
        {
            return BaseResponse<bool>.Fail("Some permissions not found");
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

        return BaseResponse<bool>.Ok(true, "Permissions assigned successfully");
    }
}
