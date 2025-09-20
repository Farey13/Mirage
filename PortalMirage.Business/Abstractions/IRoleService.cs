using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> CreateRoleAsync(string roleName);
}