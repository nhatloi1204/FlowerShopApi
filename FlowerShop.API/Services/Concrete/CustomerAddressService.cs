using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Helpers;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class CustomerAddressService : ICustomerAddressService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomerAddressService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BaseResponse<CustomerAddressOutputResource>> CreateAddressAsync(long customerId, CustomerAddressInputResource input)
    {
        var address = _mapper.Map<CustomerAddress>(input);
        address.CustomerId = customerId;
        address.CreatedAt = DateTime.UtcNow;
        address.UpdatedAt = DateTime.UtcNow;

        await _context.CustomerAddresses.AddAsync(address);
        await _context.SaveChangesAsync();

        var result = _mapper.Map<CustomerAddressOutputResource>(address);
        return BaseResponse<CustomerAddressOutputResource>.Ok(result, "Add address successfully");
    }

    public async Task<BaseResponse<CustomerAddressOutputResource>> UpdateAddressAsync(long addressId, long customerId, CustomerAddressInputResource input)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

        if (address == null) return BaseResponse<CustomerAddressOutputResource>.Fail("Cannot find address.");

        _mapper.Map(input, address);
        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(); 

        var result = _mapper.Map<CustomerAddressOutputResource>(address);
        return BaseResponse<CustomerAddressOutputResource>.Ok(result, "Update address successfully");
    }

    public async Task<BaseResponse<bool>> DeleteAddressAsync(long addressId, long customerId)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

        if (address == null) return BaseResponse<bool>.Fail("Cannot find address.");

        address.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.Ok(true, "Delete address successfully");
    }
}