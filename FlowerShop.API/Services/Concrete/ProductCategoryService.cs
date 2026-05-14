using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class ProductCategoryService : IProductCategoryService
{
    private readonly AppDbContext _context;
    private readonly ISlugService _slugService;
    private readonly IMapper _mapper;

    public ProductCategoryService(AppDbContext context, ISlugService slugService, IMapper mapper)
    {
        _context = context;
        _slugService = slugService;
        _mapper = mapper;
    }

    public async Task<BaseResponse<ProductCategoryOutputResource>> CreateAsync(ProductCategoryInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BaseResponse<ProductCategoryOutputResource>.Fail("Category name is required");
        }

        var slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Categories);

        var category = _mapper.Map<ProductCategory>(request);
        category.Slug = slug;
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ProductCategoryOutputResource>(category);
        return BaseResponse<ProductCategoryOutputResource>.Ok(response, "Category created successfully");
    }

    public async Task<BaseResponse<ProductCategoryOutputResource>> UpdateAsync(long id, ProductCategoryInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BaseResponse<ProductCategoryOutputResource>.Fail("Category name is required");
        }

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return BaseResponse<ProductCategoryOutputResource>.Fail("Category not found");
        }

        if (!string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            category.Slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Categories);
        }

        _mapper.Map(request, category);
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ProductCategoryOutputResource>(category);
        return BaseResponse<ProductCategoryOutputResource>.Ok(response, "Category updated successfully");
    }

    public async Task<BaseResponse<bool>> DeleteAsync(long id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return BaseResponse<bool>.Fail("Category not found");
        }

        category.DeletedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.Ok(true, "Category deleted successfully");
    }

    public async Task<BaseResponse<ProductCategoryOutputResource>> GetByIdAsync(long id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return BaseResponse<ProductCategoryOutputResource>.Fail("Category not found");
        }

        var response = _mapper.Map<ProductCategoryOutputResource>(category);
        return BaseResponse<ProductCategoryOutputResource>.Ok(response, "Category retrieved successfully");
    }

    public async Task<BaseResponse<List<ProductCategoryOutputResource>>> GetAllAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        var response = _mapper.Map<List<ProductCategoryOutputResource>>(categories);
        return BaseResponse<List<ProductCategoryOutputResource>>.Ok(response, "Categories retrieved successfully");
    }
}
