namespace PortalMirage.Business.Abstractions;

public interface IRolePermissionService
{
    Task<bool> AssignPermissionToRoleAsync(string roleName, string permissionName);
    Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionName);
}