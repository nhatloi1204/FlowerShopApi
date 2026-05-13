using FlowerShop.API.Models.Enums;

namespace FlowerShop.API.Models.DTOs.Product;

public class ProductResponse
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public string? Description { get; set; }
    public ProductStatus Status { get; set; }
    public long? CompanyId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
