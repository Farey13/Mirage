using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT UserID, Username, PasswordHash, FullName, IsActive, CreatedAt FROM Users WHERE Username = @Username";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }


    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                       SELECT r.RoleName 
                       FROM Roles r
                       INNER JOIN UserRoles ur ON r.RoleID = ur.RoleID
                       WHERE ur.UserID = @UserId
                       """;
        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Users (Username, PasswordHash, FullName, IsActive)
                           OUTPUT INSERTED.UserID, INSERTED.Username, INSERTED.PasswordHash, INSERTED.FullName, INSERTED.IsActive, INSERTED.CreatedAt
                           VALUES (@Username, @PasswordHash, @FullName, @IsActive);
                           """;
        var newUser = await connection.QuerySingleAsync<User>(sql, user);
        return newUser;
    }
    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Users WHERE UserID = @UserId";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });
    }
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT UserID, Username, FullName, IsActive, CreatedAt FROM Users";
        return await connection.QueryAsync<User>(sql);
    }
}