namespace FlowerShop.API.Models.DTOs.Auth;

public class ExternalAuthRequest
{
    public string IdToken { get; set; } = string.Empty; // Token Google/FB
    public string Provider { get; set; } = string.Empty; // Google/FB
}