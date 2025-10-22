using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class AdminListRepository(IDbConnectionFactory connectionFactory) : IAdminListRepository
{
    public async Task<IEnumerable<AdminListItem>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM AdminListItems ORDER BY ListType, ItemValue";
        return await connection.QueryAsync<AdminListItem>(sql);
    }

    public async Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM AdminListItems WHERE ListType = @ListType ORDER BY ItemValue";
        return await connection.QueryAsync<AdminListItem>(sql, new { ListType = listType });
    }

    public async Task<AdminListItem> CreateAsync(AdminListItem item)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO AdminListItems (ListType, ItemValue, Description, IsActive)
                           OUTPUT INSERTED.*
                           VALUES (@ListType, @ItemValue, @Description, @IsActive);
                           """;
        return await connection.QuerySingleAsync<AdminListItem>(sql, item);
    }

    public async Task<AdminListItem> UpdateAsync(AdminListItem item)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE AdminListItems
                           SET ItemValue = @ItemValue,
                               Description = @Description,
                               IsActive = @IsActive
                           OUTPUT INSERTED.*
                           WHERE ItemID = @ItemID;
                           """;
        return await connection.QuerySingleAsync<AdminListItem>(sql, item);
    }

    public async Task<AdminListItem?> GetItemAsync(string listType, string itemValue)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM AdminListItems WHERE ListType = @ListType AND ItemValue = @ItemValue";
        return await connection.QuerySingleOrDefaultAsync<AdminListItem>(sql, new { ListType = listType, ItemValue = itemValue });
    }
}