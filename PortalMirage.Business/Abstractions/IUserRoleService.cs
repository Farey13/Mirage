using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IUserRoleService
{
    Task<bool> AssignRoleToUserAsync(string username, string roleName, int actorUserId); // Add actorUserId
    Task<bool> RemoveRoleFromUserAsync(string username, string roleName, int actorUserId); // Add actorUserId
    Task<IEnumerable<Role>> GetRolesForUserAsync(string username);
}