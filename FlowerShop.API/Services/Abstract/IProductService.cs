using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IProductService
{
    Task<BaseResponse<ProductOutputResource>> CreateAsync(ProductInputResource request);
    Task<BaseResponse<ProductOutputResource>> UpdateAsync(long id, ProductInputResource request);
    Task<BaseResponse<bool>> DeleteAsync(long id);
    Task<BaseResponse<ProductOutputResource>> GetByIdAsync(long id);
    Task<BaseResponse<ProductOutputResource>> GetBySlugAsync(string slug);
    Task<BaseResponse<List<ProductOutputResource>>> GetAllAsync();
    Task<BaseResponse<PagedList<ProductOutputResource>>> GetProductsAsync(ProductQueryResource query);
}
