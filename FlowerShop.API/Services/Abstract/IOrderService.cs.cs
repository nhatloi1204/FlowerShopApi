using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IOrderService
{
    // Client Flow (Customer)
    Task<BaseResponse<OrderOutputResource>> CreateOrderAsync(long customerId, OrderCreateInputResource request);
    Task<BaseResponse<PagedList<OrderOutputResource>>> GetCustomerOrderHistoryAsync(long customerId, OrderQueryResource query);
    Task<BaseResponse<OrderOutputResource>> GetCustomerOrderDetailAsync(long customerId, long orderId);
    Task<BaseResponse<OrderOutputResource>> CancelOrderAsync(long customerId, long orderId);

    // Admin Flow (SuperAdmin)
    Task<BaseResponse<OrderPagedListResource>> GetAdminOrdersAsync(OrderQueryResource query);
    Task<BaseResponse<OrderOutputResource>> GetAdminOrderDetailAsync(long orderId);
    Task<BaseResponse<OrderOutputResource>> UpdateOrderStatusAsync(long orderId, string newStatus);
}