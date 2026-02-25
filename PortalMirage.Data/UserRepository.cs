using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "usp_Users_GetByUsername",
            new { Username = username },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<int> userIds)
    {
        if (!userIds.Any()) return Enumerable.Empty<User>();
        using var connection = await connectionFactory.CreateConnectionAsync();
        var userIdsCsv = string.Join(",", userIds);
        return await connection.QueryAsync<User>(
            "usp_Users_GetByIds",
            new { UserIds = userIdsCsv },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<string>(
            "usp_Users_GetUserRoles",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var newUser = await connection.QuerySingleAsync<User>(
            "usp_Users_Create",
            new { Username = user.Username, PasswordHash = user.PasswordHash, FullName = user.FullName, IsActive = user.IsActive },
            commandType: CommandType.StoredProcedure);
        return newUser;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "usp_Users_GetById",
            new { UserId = userId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<User>(
            "usp_Users_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_Users_UpdatePasswordHash",
            new { UserId = userId, NewPasswordHash = newPasswordHash },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }
}
