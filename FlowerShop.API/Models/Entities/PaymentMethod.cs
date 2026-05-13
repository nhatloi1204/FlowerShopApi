using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class PaymentMethod
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!; // "COD", "Transfer", "Momo", "ZaloPay", "VNPAY"
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("payment_methods");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(100)").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description").HasColumnType("varchar(255)");
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            });
        }
    }
}