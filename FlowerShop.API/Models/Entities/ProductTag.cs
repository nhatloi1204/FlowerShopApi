using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class ProductTag
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.ToTable("product_tags");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)");
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Many-to-Many: ProductTag -> Product via ProductProductTag
            entity.HasMany(e => e.Products)
                .WithMany(p => p.ProductTags)
                .UsingEntity<ProductProductTag>();
        });
    }
}
