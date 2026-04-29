using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class CustomerAddress
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string ReceiveCustomerName { get; set; } = null!;
        public string ReceiveCustomerPhone { get; set; } = null!;
        public long? CustomerId { get; set; }
        public long? ProvinceId { get; set; }
        public long? WardId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Province? Province { get; set; }
        public virtual Ward? Ward { get; set; }
        // public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.ToTable("customer_addresses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)").IsRequired();
                entity.Property(e => e.Address).HasColumnName("address").HasColumnType("varchar(500)").IsRequired();
                entity.Property(e => e.ReceiveCustomerName).HasColumnName("receive_customer_name").HasColumnType("varchar(255)").IsRequired();
                entity.Property(e => e.ReceiveCustomerPhone).HasColumnName("receive_customer_phone").HasColumnType("varchar(20)").IsRequired();

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.ProvinceId).HasColumnName("province_id");
                entity.Property(e => e.WardId).HasColumnName("ward_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
                // Foreign Keys
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Addresses)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Province)
                    .WithMany()
                    .HasForeignKey(e => e.ProvinceId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Ward)
                    .WithMany()
                    .HasForeignKey(e => e.WardId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Global Query Filter - Soft Delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
