using PortalMirage.Business.Abstractions;
using PortalMirage.Data.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class UserRoleService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository) : IUserRoleService
{
    public async Task<bool> AssignRoleToUserAsync(string username, string roleName)
    {
        // 1. Find the user and role by their names to get their IDs
        var user = await userRepository.GetByUsernameAsync(username);
        var roles = await roleRepository.GetAllAsync();
        var role = roles.FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        // 2. Business Rule: Ensure both user and role exist
        if (user is null || role is null)
        {
            return false; // Cannot assign if either doesn't exist
        }

        // 3. If they exist, create the link
        await userRoleRepository.AssignRoleToUserAsync(user.UserID, role.RoleID);
        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(string username, string roleName)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        var roles = await roleRepository.GetAllAsync();
        var role = roles.FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        if (user is null || role is null)
        {
            return false;
        }

        await userRoleRepository.RemoveRoleFromUserAsync(user.UserID, role.RoleID);
        return true;
    }
}