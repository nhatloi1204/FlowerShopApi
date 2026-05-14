namespace FlowerShop.API.Models.Views;

public class RoleInputResource
{
    public string? Title { get; set; }
}

public class RoleOutputResource
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<PermissionOutputResource> Permissions { get; set; } = new();
}

public class PermissionOutputResource
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class AssignPermissionsInputResource
{
    public List<long> PermissionIds { get; set; } = new();
}
