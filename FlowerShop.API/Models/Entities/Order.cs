using FlowerShop.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime ReceiveDate { get; set; }
        public TimeSpan ReceiveTime { get; set; }
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.Scheduled;
        public string? Description { get; set; } // Message from customer
        public decimal TotalPrice { get; set; }
        public decimal AmountPaid { get; set; } // Total amount paid by customer (can be less than TotalPrice if they choose to pay on delivery)
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Foreign Keys
        public long? CustomerId { get; set; }
        public long? ReceiveAddressId { get; set; }
        public long? PaymentMethodId { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual CustomerAddress? Address { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);

                // Cấu hình Enum thành String trong DB
                entity.Property(e => e.DeliveryMode).HasConversion<string>().HasColumnType("varchar(20)");
                entity.Property(e => e.Status).HasConversion<string>().HasColumnType("varchar(50)");

                entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasColumnType("numeric(15,2)");
                entity.Property(e => e.AmountPaid).HasColumnName("amount_paid").HasColumnType("numeric(15,2)");
                entity.Property(e => e.ReceiveDate).HasColumnName("receive_date").HasColumnType("date");
                entity.Property(e => e.ReceiveTime).HasColumnName("receive_time").HasColumnType("time");

                entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address").HasColumnType("jsonb");

                // Mối quan hệ
                entity.HasOne(d => d.Customer).WithMany(p => p.Orders).HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(d => d.Address).WithMany().HasForeignKey(d => d.ReceiveAddressId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders).HasForeignKey(d => d.PaymentMethodId).OnDelete(DeleteBehavior.SetNull);

                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}