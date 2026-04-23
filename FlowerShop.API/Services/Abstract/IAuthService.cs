using FlowerShop.API.Models.DTOs.Auth;

namespace FlowerShop.API.Services.Abstract;

public interface IAuthService
{
    Task<AuthResponse<LoginResponse>> RegisterAsync(RegisterRequest request);
    Task<AuthResponse<LoginResponse>> LoginAsync(LoginRequest request);
}
