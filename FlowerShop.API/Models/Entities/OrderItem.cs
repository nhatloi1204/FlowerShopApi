using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class OrderItem
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Save price at the time of order to avoid issues with price changes later

        public virtual Order Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(e => new { e.OrderId, e.ProductId });

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(15,2)");
                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);
                entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId);
            });
        }
    }
}