namespace FlowerShop.API.Models.Views;

public class RegisterInputResource
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PasswordConfirm { get; set; }
}

public class LoginInputResource
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class ExternalAuthInputResource
{
    public string Provider { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
}

public class LoginOutputResource
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
