using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class PermissionRepository(IDbConnectionFactory connectionFactory) : IPermissionRepository
{
    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT PermissionID, PermissionName FROM Permissions";
        return await connection.QueryAsync<Permission>(sql);
    }

    public async Task<Permission> CreateAsync(Permission permission)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Permissions (PermissionName)
                           OUTPUT INSERTED.PermissionID, INSERTED.PermissionName
                           VALUES (@PermissionName);
                           """;
        var newPermission = await connection.QuerySingleAsync<Permission>(sql, permission);
        return newPermission;
    }
}