using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class Customer
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Image { get; set; }
        public string? Provider { get; set; }
        public string? ProviderAccountId { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public string? VerificationToken { get; set; }
        public string? RememberToken { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)");
                entity.Property(e => e.Email).HasColumnName("email").HasColumnType("varchar(255)");
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Password).HasColumnName("password").HasColumnType("varchar(255)");
                entity.Property(e => e.Phone).HasColumnName("phone").HasColumnType("varchar(255)");
                entity.Property(e => e.Image).HasColumnName("image").HasColumnType("varchar(500)");
                entity.Property(e => e.Provider).HasColumnName("provider").HasColumnType("varchar(50)");
                entity.Property(e => e.ProviderAccountId).HasColumnName("provider_account_id").HasColumnType("varchar(255)");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.VerificationToken).HasColumnName("verification_token").HasColumnType("varchar(255)");
                entity.Property(e => e.RememberToken).HasColumnName("remember_token").HasColumnType("varchar(255)");

                entity.HasMany(e => e.Orders)
                  .WithOne(o => o.Customer)
                  .HasForeignKey(o => o.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
