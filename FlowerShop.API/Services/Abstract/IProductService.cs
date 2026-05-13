using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Models.DTOs.Product;

namespace FlowerShop.API.Services.Abstract;

public interface IProductService
{
    Task<AuthResponse<ProductResponse>> CreateAsync(CreateProductRequest request);
    Task<AuthResponse<ProductResponse>> UpdateAsync(long id, UpdateProductRequest request);
    Task<AuthResponse<bool>> DeleteAsync(long id);
    Task<AuthResponse<ProductResponse>> GetByIdAsync(long id);
    Task<AuthResponse<ProductResponse>> GetBySlugAsync(string slug);
    Task<AuthResponse<List<ProductResponse>>> GetAllAsync();
}
