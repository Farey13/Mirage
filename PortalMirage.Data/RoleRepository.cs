using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class RoleRepository(IDbConnectionFactory connectionFactory) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT RoleID, RoleName FROM Roles";
        return await connection.QueryAsync<Role>(sql);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Roles (RoleName)
                           OUTPUT INSERTED.RoleID, INSERTED.RoleName
                           VALUES (@RoleName);
                           """;
        var newRole = await connection.QuerySingleAsync<Role>(sql, role);
        return newRole;
    }
}