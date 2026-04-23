namespace FlowerShop.API.Models.DTOs.Auth;

public class LoginResponse
{
    public long UserId { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
