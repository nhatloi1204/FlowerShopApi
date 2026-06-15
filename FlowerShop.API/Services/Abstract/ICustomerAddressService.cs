using FlowerShop.API.Helpers;
using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface ICustomerAddressService
{
    Task<BaseResponse<CustomerAddressOutputResource>> CreateAddressAsync(long customerId, CustomerAddressInputResource input);
    Task<BaseResponse<CustomerAddressOutputResource>> UpdateAddressAsync(long addressId, long customerId, CustomerAddressInputResource input);
    Task<BaseResponse<bool>> DeleteAddressAsync(long addressId, long customerId);
}
