using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Data;

public class AdminSeeder
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AdminSeeder(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task SeedAsync()
    {
        try
        {
            string email = _config["SuperAdmin:Email"];
            if (await _context.Users.AnyAsync(u => u.Email == email)) return;

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Title == "SuperAdmin");
            if (adminRole == null)
            {
                adminRole = new Role { Title = "SuperAdmin", CreatedAt = DateTime.UtcNow };
                _context.Roles.Add(adminRole);
            }

            await SeedPermissionsAndAssignToRoleAsync(adminRole);
            var user = new User
            {
                Name = _config["SuperAdmin:Username"] ?? "Super Admin",
                Email = email,
                Password = PasswordHelper.HashPassword(_config["SuperAdmin:Password"]),
                CreatedAt = DateTime.UtcNow,
                Verified = true
            };
            _context.Users.Add(user);

            _context.RoleUsers.Add(new RoleUser { User = user, Role = adminRole });
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seeding Error: {ex.Message}");
        }
    }

    private async Task SeedPermissionsAndAssignToRoleAsync(Role adminRole)
    {
        var permissionTitles = new List<string>
        {
            "product.create", "product.read", "product.update", "product.delete", "product.view_all",
            "category.create", "category.read", "category.update", "category.delete",
            "order.create", "order.read", "order.read_all", "order.update_status", "order.delete",
            "user.create", "user.read", "user.update", "user.delete", "user.view_all",
            "role.create", "role.read", "role.update", "role.delete", "role.view_all",
            "permission.read", "permission.assign"
        };

        var existingPermissionTitles = await _context.Permissions.Select(p => p.Title).ToListAsync();
        var newPermissions = permissionTitles
            .Where(t => !existingPermissionTitles.Contains(t))
            .Select(t => new Permission { Title = t, Description = $"Quyền {t}" })
            .ToList();

        if (newPermissions.Any())
        {
            await _context.Permissions.AddRangeAsync(newPermissions);
        }

        var existingPermissionIds = await _context.Permissions
            .Where(p => existingPermissionTitles.Contains(p.Title))
            .Select(p => p.Id)
            .ToListAsync();

        var existingMappingIds = await _context.PermissionRoles
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var mappingsToAdd = existingPermissionIds
            .Where(id => !existingMappingIds.Contains(id))
            .Select(id => new PermissionRole { Role = adminRole, PermissionId = id })
            .ToList();

        foreach (var newPerm in newPermissions)
        {
            mappingsToAdd.Add(new PermissionRole { Role = adminRole, Permission = newPerm });
        }

        if (mappingsToAdd.Any())
        {
            await _context.PermissionRoles.AddRangeAsync(mappingsToAdd);
        }
    }
}