using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Models.DTOs.ProductCategory;

namespace FlowerShop.API.Services.Abstract;

public interface IProductCategoryService
{
    Task<AuthResponse<ProductCategoryResponse>> CreateAsync(CreateProductCategoryRequest request);
    Task<AuthResponse<ProductCategoryResponse>> UpdateAsync(long id, UpdateProductCategoryRequest request);
    Task<AuthResponse<bool>> DeleteAsync(long id);
    Task<AuthResponse<ProductCategoryResponse>> GetByIdAsync(long id);
    Task<AuthResponse<List<ProductCategoryResponse>>> GetAllAsync();
}
