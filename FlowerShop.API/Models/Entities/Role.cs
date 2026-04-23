using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class Role
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasColumnType("varchar(255)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Many-to-Many: Role -> Permission via PermissionRole
            entity.HasMany(e => e.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<PermissionRole>(
                    l => l.HasOne<Permission>().WithMany().HasForeignKey(pr => pr.PermissionId),
                    r => r.HasOne<Role>().WithMany().HasForeignKey(pr => pr.RoleId),
                    j => j.HasKey(pr => new { pr.RoleId, pr.PermissionId }));
        });
    }
}
