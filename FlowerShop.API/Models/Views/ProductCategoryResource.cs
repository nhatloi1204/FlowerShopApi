namespace FlowerShop.API.Models.Views;

public class ProductCategoryInputResource
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class ProductCategoryOutputResource
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Slug { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
