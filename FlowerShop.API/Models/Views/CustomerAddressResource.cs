namespace FlowerShop.API.Models.Views;

public class CustomerAddressInputResource
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string ReceiveCustomerName { get; set; } = null!;
    public string ReceiveCustomerPhone { get; set; } = null!;
    public long? ProvinceId { get; set; }
    public long? WardId { get; set; }
}
