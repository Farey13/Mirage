namespace PortalMirage.Data.Abstractions;

public interface IUserRoleRepository
{
    Task AssignRoleToUserAsync(int userId, int roleId);
    Task RemoveRoleFromUserAsync(int userId, int roleId);
}