using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class PermissionRole
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }

    // Navigation properties - made optional to avoid soft delete issues
    public virtual Role? Role { get; set; }
    public virtual Permission? Permission { get; set; }

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermissionRole>(entity =>
        {
            entity.ToTable("permission_role");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");

            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter to handle soft delete from related entities
            entity.HasQueryFilter(e => e.Role == null || e.Role.DeletedAt == null);
            entity.HasQueryFilter(e => e.Permission == null || e.Permission.DeletedAt == null);
        });
    }
}
