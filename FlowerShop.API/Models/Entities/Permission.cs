using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class Permission
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title").HasColumnType("varchar(255)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
    }
}
