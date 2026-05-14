namespace FlowerShop.API.Models.Views;

public class ProductTagInputResource
{
    public string? Name { get; set; }
}

public class ProductTagOutputResource
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
