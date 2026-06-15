using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomerService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BaseResponse<PagedList<CustomerOutputResource>>> GetCustomersAsync(CustomerQueryResource queryResource)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryResource.Search))
        {
            var searchPattern = $"%{queryResource.Search}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Name ?? "", searchPattern) ||
                EF.Functions.ILike(c.Email ?? "", searchPattern) ||
                EF.Functions.ILike(c.Phone ?? "", searchPattern)
            );
        }

        var totalItems = await query.CountAsync();

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((queryResource.Page - 1) * queryResource.PageSize)
            .Take(queryResource.PageSize)
            .ToListAsync();

        var mappedCustomers = _mapper.Map<List<CustomerOutputResource>>(customers);

        var result = new PagedList<CustomerOutputResource>
        {
            Items = mappedCustomers,
            TotalItems = totalItems,
            CurrentPage = queryResource.Page,
            TotalPages = (int)Math.Ceiling((double)totalItems / queryResource.PageSize)
        };

        return BaseResponse<PagedList<CustomerOutputResource>>.Ok(result, "Fetched customers successfully");
    }

    public async Task<BaseResponse<CustomerOutputResource>> GetCustomerByIdAsync(long id)
    {
        var customer = await _context.Customers
            .Include(c => c.Addresses)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return BaseResponse<CustomerOutputResource>.Fail("Customer not found");
        }

        var result = _mapper.Map<CustomerOutputResource>(customer);
        return BaseResponse<CustomerOutputResource>.Ok(result, "Fetched customer detail successfully");
    }

    public async Task<BaseResponse<CustomerOutputResource>> UpdateCustomerAsync(long id, CustomerInputResource input)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
        {
            return BaseResponse<CustomerOutputResource>.Fail("Customer not found");
        }

        _mapper.Map(input, customer);
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = _mapper.Map<CustomerOutputResource>(customer);
        return BaseResponse<CustomerOutputResource>.Ok(result, "Updated customer successfully");
    }

    public async Task<BaseResponse<bool>> DeleteCustomerAsync(long id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
        {
            return BaseResponse<bool>.Fail("Customer not found");
        }

        customer.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return BaseResponse<bool>.Ok(true, "Deleted customer successfully");
    }
}