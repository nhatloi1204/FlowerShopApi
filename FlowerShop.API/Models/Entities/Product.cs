using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Enums;

public class Product
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; } = 0;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    //public long? StatusId { get; set; }
    public long? CompanyId { get; set; }

    // Navigation properties
    //public virtual ProductStatus? Status { get; set; }
    public virtual ICollection<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Slug).HasColumnName("slug").HasColumnType("varchar(255)");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.PriceMin).HasColumnName("price_min").HasColumnType("numeric(15,2)");
            entity.Property(e => e.PriceMax).HasColumnName("price_max").HasColumnType("numeric(15,2)");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric(15,2)");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity").HasDefaultValue(0);
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp with time zone");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>().HasColumnType("varchar(50)");
            //entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");

            // Global Query Filter - Soft Delete
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Foreign Key: Product -> ProductStatus
            //entity.HasOne(e => e.Status)
            //    .WithMany(ps => ps.Products)
            //    .HasForeignKey(e => e.StatusId)
            //    .OnDelete(DeleteBehavior.SetNull);

            // Many-to-Many: Product -> ProductCategory via ProductProductCategory
            entity.HasMany(e => e.Categories)
                .WithMany(c => c.Products)
                .UsingEntity<ProductProductCategory>(
                    l => l.HasOne<ProductCategory>().WithMany().HasForeignKey(pc => pc.CategoryId),
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
