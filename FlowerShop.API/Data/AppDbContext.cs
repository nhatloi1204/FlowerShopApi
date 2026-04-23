using Microsoft.EntityFrameworkCore;
using FlowerShop.API.Models.Entities;

namespace FlowerShop.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RoleUser> RoleUsers { get; set; }
    public DbSet<PermissionRole> PermissionRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Call OnModelCreating from each entity
        User.OnModelCreating(modelBuilder);
        Role.OnModelCreating(modelBuilder);
        Permission.OnModelCreating(modelBuilder);
        RoleUser.OnModelCreating(modelBuilder);
        PermissionRole.OnModelCreating(modelBuilder);
    }
}
