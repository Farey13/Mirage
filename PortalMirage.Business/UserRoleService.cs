using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class UserRoleService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IAuditLogService auditLogService) : IUserRoleService
{
    public async Task<bool> AssignRoleToUserAsync(string username, string roleName, int actorUserId)
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
        await auditLogService.LogAsync(actorUserId, "Update", "UserManagement", user.UserID.ToString(), newValue: $"Assigned role '{role.RoleName}' to user '{username}'");
        return true;
    }

    public async Task<IEnumerable<Role>> GetRolesForUserAsync(string username)
    {
        return await userRoleRepository.GetRolesForUserAsync(username); // You will need to create this repository method
    }

    public async Task<bool> RemoveRoleFromUserAsync(string username, string roleName, int actorUserId)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        var roles = await roleRepository.GetAllAsync();
        var role = roles.FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));

        if (user is null || role is null)
        {
            return false;
        }

        await userRoleRepository.RemoveRoleFromUserAsync(user.UserID, role.RoleID);
        await auditLogService.LogAsync(actorUserId, "Update", "UserManagement", user.UserID.ToString(), newValue: $"Removed role '{role.RoleName}' from user '{username}'");
        return true;
    }
}