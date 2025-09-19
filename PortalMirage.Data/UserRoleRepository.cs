using Dapper;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class UserRoleRepository(IDbConnectionFactory connectionFactory) : IUserRoleRepository
{
    public async Task AssignRoleToUserAsync(int userId, int roleId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO UserRoles (UserID, RoleID) VALUES (@UserId, @RoleId);";
        await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }

    public async Task RemoveRoleFromUserAsync(int userId, int roleId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "DELETE FROM UserRoles WHERE UserID = @UserId AND RoleID = @RoleId;";
        await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }
}