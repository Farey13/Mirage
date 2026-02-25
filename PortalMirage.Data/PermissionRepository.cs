using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class PermissionRepository(IDbConnectionFactory connectionFactory) : IPermissionRepository
{
    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Permission>(
            "usp_Permissions_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Permission> CreateAsync(Permission permission)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var newPermission = await connection.QuerySingleAsync<Permission>(
            "usp_Permissions_Create",
            new { PermissionName = permission.PermissionName },
            commandType: CommandType.StoredProcedure);
        return newPermission;
    }
}
