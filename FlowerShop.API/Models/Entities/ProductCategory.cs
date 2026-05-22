using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class ProductProductCategory
{
    public long ProductId { get; set; }
    public long CategoryId { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
    public virtual ProductCategory? Category { get; set; }

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductProductCategory>(entity =>
        {
            entity.ToTable("product_product_category");
            entity.HasKey(e => new { e.ProductId, e.CategoryId });

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            // Foreign Keys
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // --> Prevent from deleting category having active products
        });
    }
}
