using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class ProductStatus
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductStatus>(entity =>
        {
            entity.ToTable("product_statuses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(100)").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("varchar(255)");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");

            // One-to-Many: ProductStatus -> Product
            entity.HasMany(e => e.Products)
                .WithOne(p => p.ProductStatus)
                .HasForeignKey(p => p.ProductStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
