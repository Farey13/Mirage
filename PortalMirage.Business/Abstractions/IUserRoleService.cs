namespace PortalMirage.Business.Abstractions;

public interface IUserRoleService
{
    Task<bool> AssignRoleToUserAsync(string username, string roleName);
    Task<bool> RemoveRoleFromUserAsync(string username, string roleName);
}