namespace FlowerShop.API.Models.DTOs.Admin.Role;

public class RoleResponse
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public List<PermissionResponse>? Permissions { get; set; } = new List<PermissionResponse>();
    public DateTime? CreatedAt { get; set; }
}
