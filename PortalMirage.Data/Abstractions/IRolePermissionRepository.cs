namespace PortalMirage.Data.Abstractions;

public interface IRolePermissionRepository
{
    Task AssignPermissionToRoleAsync(int roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
}