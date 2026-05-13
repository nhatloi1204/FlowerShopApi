using FlowerShop.API.Data;
using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Models.DTOs.ProductCategory;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class ProductCategoryService : IProductCategoryService
{
    private readonly AppDbContext _context;
    private readonly ISlugService _slugService;

    public ProductCategoryService(AppDbContext context, ISlugService slugService)
    {
        _context = context;
        _slugService = slugService;
    }

    public async Task<AuthResponse<ProductCategoryResponse>> CreateAsync(CreateProductCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new AuthResponse<ProductCategoryResponse>
            {
                Success = false,
                Message = "Category name is required"
            };
        }

        var slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Categories);

        var category = new ProductCategory
        {
            Name = request.Name,
            Description = request.Description,
            Slug = slug,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new AuthResponse<ProductCategoryResponse>
        {
            Success = true,
            Message = "Category created successfully",
            Data = MapToResponse(category)
        };
    }

    public async Task<AuthResponse<ProductCategoryResponse>> UpdateAsync(long id, UpdateProductCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new AuthResponse<ProductCategoryResponse>
            {
                Success = false,
                Message = "Category name is required"
            };
        }

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return new AuthResponse<ProductCategoryResponse>
            {
                Success = false,
                Message = "Category not found"
            };
        }

        if (!string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            category.Slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Categories);
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return new AuthResponse<ProductCategoryResponse>
        {
            Success = true,
            Message = "Category updated successfully",
            Data = MapToResponse(category)
        };
    }

    public async Task<AuthResponse<bool>> DeleteAsync(long id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return new AuthResponse<bool>
            {
                Success = false,
                Message = "Category not found"
            };
        }

        category.DeletedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _context.Categories.Update(category);
        await _context.SaveChangesAsync();

        return new AuthResponse<bool>
        {
            Success = true,
            Message = "Category deleted successfully",
            Data = true
        };
    }

    public async Task<AuthResponse<ProductCategoryResponse>> GetByIdAsync(long id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return new AuthResponse<ProductCategoryResponse>
            {
                Success = false,
                Message = "Category not found"
            };
        }

        return new AuthResponse<ProductCategoryResponse>
        {
            Success = true,
            Message = "Category retrieved successfully",
            Data = MapToResponse(category)
        };
    }

    public async Task<AuthResponse<List<ProductCategoryResponse>>> GetAllAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return new AuthResponse<List<ProductCategoryResponse>>
        {
            Success = true,
            Message = "Categories retrieved successfully",
            Data = categories.Select(MapToResponse).ToList()
        };
    }

    private static ProductCategoryResponse MapToResponse(ProductCategory category)
    {
        return new ProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
