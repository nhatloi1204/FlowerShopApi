using Microsoft.EntityFrameworkCore;
using FlowerShop.API.Models.Entities;

namespace FlowerShop.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets - Auth & System
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RoleUser> RoleUsers { get; set; }
    public DbSet<PermissionRole> PermissionRoles { get; set; }

    // DbSets - Products
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<ProductStatus> ProductStatuses { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<ProductProductTag> ProductProductTags { get; set; }

    // DbSets - Customers
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<Ward> Wards { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Call OnModelCreating from each entity - Auth & System
        User.OnModelCreating(modelBuilder);
        Role.OnModelCreating(modelBuilder);
        Permission.OnModelCreating(modelBuilder);
        RoleUser.OnModelCreating(modelBuilder);
        PermissionRole.OnModelCreating(modelBuilder);

        // Call OnModelCreating from each entity - Products
        Category.OnModelCreating(modelBuilder);
        Product.OnModelCreating(modelBuilder);
        ProductTag.OnModelCreating(modelBuilder);
        ProductStatus.OnModelCreating(modelBuilder);
        ProductCategory.OnModelCreating(modelBuilder);
        ProductProductTag.OnModelCreating(modelBuilder);

        // Call OnModelCreating from each entity - Customers
        Customer.OnModelCreating(modelBuilder);
        CustomerAddress.OnModelCreating(modelBuilder);
        Province.OnModelCreating(modelBuilder);
        Ward.OnModelCreating(modelBuilder);
    }
}
