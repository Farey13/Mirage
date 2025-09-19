using Dapper;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class RolePermissionRepository(IDbConnectionFactory connectionFactory) : IRolePermissionRepository
{
    public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "INSERT INTO RolePermissions (RoleID, PermissionID) VALUES (@RoleId, @PermissionId);";
        await connection.ExecuteAsync(sql, new { RoleId = roleId, PermissionId = permissionId });
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "DELETE FROM RolePermissions WHERE RoleID = @RoleId AND PermissionID = @PermissionId;";
        await connection.ExecuteAsync(sql, new { RoleId = roleId, PermissionId = permissionId });
    }
}