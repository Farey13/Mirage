using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class RoleService(IRoleRepository roleRepository) : IRoleService
{
    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await roleRepository.GetAllAsync();
    }

    public async Task<Role?> CreateRoleAsync(string roleName)
    {
        // 1. Business Rule: Check if a role with the same name already exists (ignoring case)
        var existingRoles = await roleRepository.GetAllAsync();
        if (existingRoles.Any(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
        {
            // Role name is already taken, creation fails.
            return null;
        }

        // 2. If the name is unique, create the new role object
        var roleToCreate = new Role { RoleName = roleName };

        // 3. Pass the new role to the data layer to be saved
        var createdRole = await roleRepository.CreateAsync(roleToCreate);

        return createdRole;
    }
}