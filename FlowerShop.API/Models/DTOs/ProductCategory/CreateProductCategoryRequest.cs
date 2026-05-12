namespace FlowerShop.API.Models.DTOs.ProductCategory;

public class CreateProductCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
