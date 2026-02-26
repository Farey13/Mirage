using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class RoleRepository(IDbConnectionFactory connectionFactory) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Role>(
            "usp_Roles_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var newRole = await connection.QuerySingleAsync<Role>(
            "usp_Roles_Create",
            new { RoleName = role.RoleName },
            commandType: CommandType.StoredProcedure);
        return newRole;
    }
}
