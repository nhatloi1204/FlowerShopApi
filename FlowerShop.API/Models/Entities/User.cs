using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class User
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool Verified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationToken { get; set; }
    public string? RememberToken { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? CompanyId { get; set; }

    // Navigation properties
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)");
            entity.Property(e => e.Email).HasColumnName("email").HasColumnType("varchar(255)");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Password).HasColumnName("password").HasColumnType("varchar(255)");
            entity.Property(e => e.Verified).HasColumnName("verified").HasDefaultValue(false);
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.VerificationToken).HasColumnName("verification_token").HasColumnType("varchar(255)");
            entity.Property(e => e.RememberToken).HasColumnName("remember_token").HasColumnType("varchar(255)");
            entity.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Many-to-Many: User -> Role via RoleUser
            entity.HasMany(e => e.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<RoleUser>(
                    l => l.HasOne<Role>().WithMany().HasForeignKey(ru => ru.RoleId),
                    r => r.HasOne<User>().WithMany().HasForeignKey(ru => ru.UserId),
                    j => j.HasKey(ru => new { ru.UserId, ru.RoleId }));
        });
    }
}
