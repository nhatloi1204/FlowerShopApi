using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IProductCategoryService
{
    Task<BaseResponse<ProductCategoryOutputResource>> CreateAsync(ProductCategoryInputResource request);
    Task<BaseResponse<ProductCategoryOutputResource>> UpdateAsync(long id, ProductCategoryInputResource request);
    Task<BaseResponse<bool>> DeleteAsync(long id);
    Task<BaseResponse<ProductCategoryOutputResource>> GetByIdAsync(long id);
    Task<BaseResponse<List<ProductCategoryOutputResource>>> GetAllAsync();
}
