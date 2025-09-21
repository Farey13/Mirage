namespace PortalMirage.Core.Models;

public record Permission
{
    public int PermissionID { get; init; }
    public required string PermissionName { get; set; }
}