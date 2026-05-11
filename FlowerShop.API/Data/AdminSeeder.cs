using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Data;

public class AdminSeeder
{
    private readonly AppDbContext _context;

    public AdminSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        try
        {
            var adminRole = await SeedSuperAdminRoleAsync();
            await SeedPermissionsAndAssignToRoleAsync(adminRole);
            await SeedSuperAdminUserAsync(adminRole);

            Console.WriteLine("SuperAdmin Initialization Completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seeding Error: {ex.Message}");
        }
    }

    // 1. Tạo duy nhất Role SuperAdmin
    private async Task<Role> SeedSuperAdminRoleAsync()
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Title == "SuperAdmin");
        if (role == null)
        {
            role = new Role { Title = "SuperAdmin", CreatedAt = DateTime.UtcNow };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Role 'SuperAdmin' created.");
        }
        return role;
    }

    // 2. Tạo Permissions và gán hết cho Role vừa tìm được
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

        // Tạo những Permission chưa có trong DB
        var existingPermissionTitles = await _context.Permissions.Select(p => p.Title).ToListAsync();
        var newPermissions = permissionTitles
            .Where(t => !existingPermissionTitles.Contains(t))
            .Select(t => new Permission { Title = t, Description = $"Quyền {t}" })
            .ToList();

        if (newPermissions.Any())
        {
            await _context.Permissions.AddRangeAsync(newPermissions);
            await _context.SaveChangesAsync();
        }

        // Gán tất cả Permission (cũ + mới) cho duy nhất SuperAdmin
        var allPermissions = await _context.Permissions.ToListAsync();
        var existingMappingIds = await _context.PermissionRoles
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var mappingsToAdd = allPermissions
            .Where(p => !existingMappingIds.Contains(p.Id))
            .Select(p => new PermissionRole { RoleId = adminRole.Id, PermissionId = p.Id })
            .ToList();

        if (mappingsToAdd.Any())
        {
            await _context.PermissionRoles.AddRangeAsync(mappingsToAdd);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Assigned {mappingsToAdd.Count} permissions to SuperAdmin.");
        }
    }

    // 3. Tạo User và gắn vào Role SuperAdmin
    private async Task SeedSuperAdminUserAsync(Role adminRole)
    {
        string email = Environment.GetEnvironmentVariable("SUPER_ADMIN_EMAIL");
        if (string.IsNullOrEmpty(email) || await _context.Users.AnyAsync(u => u.Email == email)) return;

        var user = new User
        {
            Name = Environment.GetEnvironmentVariable("SUPER_ADMIN_USERNAME") ?? "Super Admin",
            Email = email,
            Password = PasswordHelper.HashPassword(Environment.GetEnvironmentVariable("SUPER_ADMIN_PASSWORD")),
            CreatedAt = DateTime.UtcNow,
            Verified = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Gán User vào Role
        _context.RoleUsers.Add(new RoleUser { UserId = user.Id, RoleId = adminRole.Id });
        await _context.SaveChangesAsync();
        Console.WriteLine($"User {email} is now a SuperAdmin.");
    }
}