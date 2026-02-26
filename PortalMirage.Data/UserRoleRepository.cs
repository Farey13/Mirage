using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class UserRoleRepository(IDbConnectionFactory connectionFactory) : IUserRoleRepository
{
    public async System.Threading.Tasks.Task AssignRoleToUserAsync(int userId, int roleId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_UserRoles_AssignRoleToUser",
            new { UserId = userId, RoleId = roleId },
            commandType: CommandType.StoredProcedure);
    }

    public async System.Threading.Tasks.Task RemoveRoleFromUserAsync(int userId, int roleId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_UserRoles_RemoveRoleFromUser",
            new { UserId = userId, RoleId = roleId },
            commandType: CommandType.StoredProcedure);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Role>> GetRolesForUserAsync(string username)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Role>(
            "usp_UserRoles_GetRolesForUser",
            new { Username = username },
            commandType: CommandType.StoredProcedure);
    }
}
