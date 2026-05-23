using FlowerShop.API.Models.Enums;

namespace FlowerShop.API.Models.Views;

public class ShippingAddressSnapshot
{
    public string ReceiveCustomerName { get; set; } = string.Empty;
    public string ReceiveCustomerPhone { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string WardName { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
}

public class OrderItemInputResource
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
}

public class OrderCreateInputResource
{
    public string? Description { get; set; }
    public DateTime ReceiveDate { get; set; }
    public TimeSpan ReceiveTime { get; set; }
    public DeliveryMode DeliveryMode { get; set; }
    public long ReceiveAddressId { get; set; }
    public long PaymentMethodId { get; set; }
    public List<OrderItemInputResource> Items { get; set; } = new();
}

// (Advanced Filters) for Order List API - support FE filter + search + pagination in 1 request
public class OrderQueryResource
{
    public string? Search { get; set; }       // filter by customer name, phone, or product name in order items
    public OrderStatus? Status { get; set; }    // filter by order status tabs
    public DeliveryMode? DeliveryMode { get; set; }
    public long? PaymentMethodId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class OrderItemOutputResource
{
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class OrderOutputResource
{
    public long Id { get; set; }
    public DateTime ReceiveDate { get; set; }
    public TimeSpan ReceiveTime { get; set; }
    public string DeliveryMode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsRefundPending { get; set; } // Cancelled but refund not completed yet
    public DateTime CreatedAt { get; set; }

    public ShippingAddressSnapshot? ShippingAddress { get; set; }
    public List<OrderItemOutputResource> OrderItems { get; set; } = new();

    // Return list of allowed next statuses based on current status 
    // Support FE in showing action buttons
    // (e.g. Cancel, Mark as Delivered, Request Refund...)
    public List<string> AllowedNextStatuses { get; set; } = new();
}

public class OrderPagedListResource
{
    public List<OrderOutputResource> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    // Return count of orders in each status for current filter 
    // (e.g. for showing counts in status tabs on FE)
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}