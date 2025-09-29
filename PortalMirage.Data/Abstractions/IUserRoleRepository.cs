using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IUserRoleRepository
{
    System.Threading.Tasks.Task AssignRoleToUserAsync(int userId, int roleId);
    System.Threading.Tasks.Task RemoveRoleFromUserAsync(int userId, int roleId);

    System.Threading.Tasks.Task<IEnumerable<Role>> GetRolesForUserAsync(string username);
}