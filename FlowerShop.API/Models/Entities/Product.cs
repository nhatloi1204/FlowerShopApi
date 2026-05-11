using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class Product
{
    public long Id { get; set; }
    public long ProductStatusId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; } = 0;
    public decimal? DiscountPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int Quantity { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int ViewCount { get; set; } = 0;
    public string? MainImage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ProductStatus? ProductStatus { get; set; }
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductStatusId).HasColumnName("product_status_id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(e => e.Slug).HasColumnName("slug").HasColumnType("varchar(255)");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.SKU).HasColumnName("sku").HasColumnType("varchar(50)");
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(12,2)");
            entity.Property(e => e.DiscountPrice).HasColumnName("discount_price").HasColumnType("numeric(12,2)");
            entity.Property(e => e.CostPrice).HasColumnName("cost_price").HasColumnType("numeric(12,2)");
            entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
            entity.Property(e => e.MainImage).HasColumnName("main_image").HasColumnType("varchar(255)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Foreign Key: Product -> ProductStatus
            entity.HasOne(e => e.ProductStatus)
                .WithMany(ps => ps.Products)
                .HasForeignKey(e => e.ProductStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-Many: Product -> Category via ProductCategory
            entity.HasMany(e => e.Categories)
                .WithMany(c => c.Products)
                .UsingEntity<ProductCategory>(
                    l => l.HasOne<Category>().WithMany().HasForeignKey(pc => pc.CategoryId),
                    r => r.HasOne<Product>().WithMany().HasForeignKey(pc => pc.ProductId),
                    j => j.HasKey(pc => new { pc.ProductId, pc.CategoryId }));

            // Many-to-Many: Product -> ProductTag via ProductProductTag
            entity.HasMany(e => e.ProductTags)
                .WithMany(pt => pt.Products)
                .UsingEntity<ProductProductTag>(
                    l => l.HasOne<ProductTag>().WithMany().HasForeignKey(ppt => ppt.TagId),
                    r => r.HasOne<Product>().WithMany().HasForeignKey(ppt => ppt.ProductId),
                    j => j.HasKey(ppt => new { ppt.ProductId, ppt.TagId }));
        });
    }
}
