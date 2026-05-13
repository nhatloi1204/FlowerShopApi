using Microsoft.AspNetCore.Http;

using FlowerShop.API.Models.Enums;

namespace FlowerShop.API.Models.DTOs.Product;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
    public string? Description { get; set; }
    public ProductStatus? Status { get; set; }
    public long? CompanyId { get; set; }
    public List<long>? CategoryIds { get; set; }
    public List<long>? TagIds { get; set; }
    public List<IFormFile>? Images { get; set; }
}
