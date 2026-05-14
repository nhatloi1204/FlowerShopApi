namespace FlowerShop.API.Models.Views;

public class MediaOutputResource
{
    public long Id { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public long ModelId { get; set; }
    public string CollectionName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long Size { get; set; }
    public string CustomProperties { get; set; } = "{}";
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
