using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IProductTagService
{
    Task<BaseResponse<IEnumerable<ProductTagOutputResource>>> GetAllAsync();
    Task<BaseResponse<ProductTagOutputResource>> GetByIdAsync(long id);
    Task<BaseResponse<ProductTagOutputResource>> CreateAsync(ProductTagInputResource request);
    Task<BaseResponse<ProductTagOutputResource>> UpdateAsync(long id, ProductTagInputResource request);
    Task<BaseResponse<bool>> DeleteAsync(long id);
}