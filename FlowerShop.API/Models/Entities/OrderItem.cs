using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class OrderItem
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Save price at the time of order to avoid issues with price changes later

        // Snapshot of product details at the time of order 
        // (optional, can be used for historical data or if product details change later)
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }

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

                entity.Property(e => e.ProductName).HasColumnName("product_name").HasColumnType("varchar(255)");
                entity.Property(e => e.ProductImage).HasColumnName("product_image").HasColumnType("varchar(500)");

                entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);
                entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId);
            });
        }
    }
}