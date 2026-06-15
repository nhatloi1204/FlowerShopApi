namespace FlowerShop.API.Models.Views;

public class CustomerAddressOutputResource
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string ReceiveCustomerName { get; set; } = null!;
    public string ReceiveCustomerPhone { get; set; } = null!;
    public long? ProvinceId { get; set; }
    public long? WardId { get; set; }
}

public class CustomerInputResource
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Image { get; set; }
}

public class CustomerOutputResource
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Image { get; set; }
    public string? Provider { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CustomerAddressOutputResource> Addresses { get; set; } = new();
}
public class CustomerQueryResource
{
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}