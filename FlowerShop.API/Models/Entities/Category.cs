using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class Category
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? Image { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(e => e.Slug).HasColumnName("slug").HasColumnType("varchar(255)");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Image).HasColumnName("image").HasColumnType("varchar(255)");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Many-to-Many: Category -> Product via ProductCategory
            entity.HasMany(e => e.Products)
                .WithMany(p => p.Categories)
                .UsingEntity<ProductCategory>(
                    l => l.HasOne<Product>().WithMany().HasForeignKey(pc => pc.ProductId),
                    r => r.HasOne<Category>().WithMany().HasForeignKey(pc => pc.CategoryId),
                    j => j.HasKey(pc => new { pc.ProductId, pc.CategoryId }));
        });
    }
}
