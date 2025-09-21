namespace PortalMirage.Core.Models;

public record Role
{
    public int RoleID { get; init; }
    public required string RoleName { get; set; }
}