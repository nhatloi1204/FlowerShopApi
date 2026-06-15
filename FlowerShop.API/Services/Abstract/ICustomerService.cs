using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface ICustomerService
{
    Task<BaseResponse<PagedList<CustomerOutputResource>>> GetCustomersAsync(CustomerQueryResource queryResource);
    Task<BaseResponse<CustomerOutputResource>> GetCustomerByIdAsync(long id);
    Task<BaseResponse<CustomerOutputResource>> UpdateCustomerAsync(long id, CustomerInputResource input);
    Task<BaseResponse<bool>> DeleteCustomerAsync(long id);
}