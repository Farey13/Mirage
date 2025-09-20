using PortalMirage.Business.Abstractions;
using PortalMirage.Data.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class RolePermissionService(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository) : IRolePermissionService
{
    public async Task<bool> AssignPermissionToRoleAsync(string roleName, string permissionName)
    {
        var roles = await roleRepository.GetAllAsync();
        var role = roles.FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        var permissions = await permissionRepository.GetAllAsync();
        var permission = permissions.FirstOrDefault(p => p.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase));

        if (role is null || permission is null)
        {
            return false;
        }

        await rolePermissionRepository.AssignPermissionToRoleAsync(role.RoleID, permission.PermissionID);
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionName)
    {
        var roles = await roleRepository.GetAllAsync();
        var role = roles.FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        var permissions = await permissionRepository.GetAllAsync();
        var permission = permissions.FirstOrDefault(p => p.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase));

        if (role is null || permission is null)
        {
            return false;
        }

        await rolePermissionRepository.RemovePermissionFromRoleAsync(role.RoleID, permission.PermissionID);
        return true;
    }
}