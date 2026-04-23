namespace FlowerShop.API.Models.DTOs.Auth;

public class AuthResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
