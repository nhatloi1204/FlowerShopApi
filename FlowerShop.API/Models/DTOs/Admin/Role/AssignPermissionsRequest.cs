namespace FlowerShop.API.Models.DTOs.Admin.Role;

public class AssignPermissionsRequest
{
    public List<long> PermissionIds { get; set; } = new List<long>();
}
