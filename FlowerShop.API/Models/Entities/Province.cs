using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities
{
    public class Province
    {
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        //Navigation properties
        public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("provinces");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)").IsRequired();

                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

                // Global Query Filter - Soft Delete
                entity.HasQueryFilter(e => e.DeletedAt == null);
            });
        }
    }
}