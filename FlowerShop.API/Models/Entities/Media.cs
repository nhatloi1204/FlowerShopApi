using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Models.Entities;

public class Media
{
    public long Id { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public long ModelId { get; set; }
    public Guid? Uuid { get; set; }
    public string CollectionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public string Disk { get; set; } = "cloudinary";
    public string? ConversionsDisk { get; set; }
    public long Size { get; set; }
    public string Manipulations { get; set; } = "{}";
    public string CustomProperties { get; set; } = "{}";
    public string GeneratedConversions { get; set; } = "{}";
    public string ResponsiveImages { get; set; } = "{}";
    public int? OrderColumn { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Media>(entity =>
        {
            entity.ToTable("media");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");

            // Polymorphic association: model_type and model_id
            entity.Property(e => e.ModelType).HasColumnName("model_type").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.ModelId).HasColumnName("model_id").IsRequired();

            entity.Property(e => e.Uuid).HasColumnName("uuid").HasColumnType("uuid");
            entity.HasIndex(e => e.Uuid).IsUnique();

            entity.Property(e => e.CollectionName).HasColumnName("collection_name").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.FileName).HasColumnName("file_name").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.MimeType).HasColumnName("mime_type").HasColumnType("varchar(255)");
            entity.Property(e => e.Disk).HasColumnName("disk").HasColumnType("varchar(255)").IsRequired();
            entity.Property(e => e.ConversionsDisk).HasColumnName("conversions_disk").HasColumnType("varchar(255)");
            entity.Property(e => e.Size).HasColumnName("size").IsRequired();

            // Jsonb with default value as empty JSON object
            entity.Property(e => e.Manipulations).HasColumnName("manipulations").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.CustomProperties).HasColumnName("custom_properties").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.GeneratedConversions).HasColumnName("generated_conversions").HasColumnType("jsonb").HasDefaultValue("{}");
            entity.Property(e => e.ResponsiveImages).HasColumnName("responsive_images").HasColumnType("jsonb").HasDefaultValue("{}");

            entity.Property(e => e.OrderColumn).HasColumnName("order_column");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp without time zone");

            entity.HasIndex(e => new { e.ModelType, e.ModelId }).HasDatabaseName("idx_media_model");
            entity.HasIndex(e => e.OrderColumn).HasDatabaseName("idx_media_order_column");
        });
    }
}