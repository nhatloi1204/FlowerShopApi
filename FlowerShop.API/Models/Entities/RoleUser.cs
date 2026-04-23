using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class RoleUser
{
    public long UserId { get; set; }
    public long RoleId { get; set; }

    // Navigation properties - made optional to avoid soft delete issues
    public virtual User? User { get; set; }
    public virtual Role? Role { get; set; }

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleUser>(entity =>
        {
            entity.ToTable("role_user");
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Query filter to handle soft delete from related entities
            entity.HasQueryFilter(e => e.User == null || e.User.DeletedAt == null);
            entity.HasQueryFilter(e => e.Role == null || e.Role.DeletedAt == null);
        });
    }
}
