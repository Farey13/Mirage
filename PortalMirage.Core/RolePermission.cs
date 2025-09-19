namespace PortalMirage.Core.Models;

public record RolePermission
{
    public int RoleID { get; set; }
    public int PermissionID { get; set; }
}