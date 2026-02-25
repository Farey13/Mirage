using Dapper;
using PortalMirage.Data.Abstractions;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class RolePermissionRepository(IDbConnectionFactory connectionFactory) : IRolePermissionRepository
{
    public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_RolePermissions_AssignPermissionToRole",
            new { RoleId = roleId, PermissionId = permissionId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_RolePermissions_RemovePermissionFromRole",
            new { RoleId = roleId, PermissionId = permissionId },
            commandType: CommandType.StoredProcedure);
    }
}
