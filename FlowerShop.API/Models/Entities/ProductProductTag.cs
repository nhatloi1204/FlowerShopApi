using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class ProductProductTag
{
    public long ProductId { get; set; }
    public long TagId { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
    public virtual ProductTag? Tag { get; set; }

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductProductTag>(entity =>
        {
            entity.ToTable("product_product_tag");
            entity.HasKey(e => new { e.ProductId, e.TagId });

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");

            // Foreign Keys
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany()
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
