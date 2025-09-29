using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class UserRoleRepository(IDbConnectionFactory connectionFactory) : IUserRoleRepository
{
    public async System.Threading.Tasks.Task AssignRoleToUserAsync(int userId, int roleId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO UserRoles (UserID, RoleID) VALUES (@UserId, @RoleId);";
        await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }

    public async System.Threading.Tasks.Task RemoveRoleFromUserAsync(int userId, int roleId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "DELETE FROM UserRoles WHERE UserID = @UserId AND RoleID = @RoleId;";
        await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }

    public async System.Threading.Tasks.Task<IEnumerable<Role>> GetRolesForUserAsync(string username)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           SELECT r.*
                           FROM Roles r
                           INNER JOIN UserRoles ur ON r.RoleID = ur.RoleID
                           INNER JOIN Users u ON ur.UserID = u.UserID
                           WHERE u.Username = @Username
                           """;
        return await connection.QueryAsync<Role>(sql, new { Username = username });
    }
}