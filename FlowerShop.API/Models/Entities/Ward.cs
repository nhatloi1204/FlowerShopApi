using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class Ward
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public long? ProvinceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual Province? Province { get; set; }
        public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ward>(entity =>
            {
                entity.ToTable("wards");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)").IsRequired();
                entity.Property(e => e.ProvinceId).HasColumnName("province_id");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

                // Foreign Key
                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Wards)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Global Query Filter - Soft Delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}
