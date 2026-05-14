using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Abstract;

public interface IAuthService
{
    Task<BaseResponse<LoginOutputResource>> RegisterAsync(RegisterInputResource request);
    Task<BaseResponse<LoginOutputResource>> LoginAsync(LoginInputResource request);
    Task<BaseResponse<LoginOutputResource>> ExternalLoginAsync(ExternalAuthInputResource request);
}
