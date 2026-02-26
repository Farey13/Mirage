using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class AdminListRepository(IDbConnectionFactory connectionFactory) : IAdminListRepository
{
    public async Task<IEnumerable<AdminListItem>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<AdminListItem>(
            "usp_AdminListItems_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<AdminListItem>(
            "usp_AdminListItems_GetByType",
            new { ListType = listType },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<AdminListItem> CreateAsync(AdminListItem item)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<AdminListItem>(
            "usp_AdminListItems_Create",
            new { ListType = item.ListType, ItemValue = item.ItemValue, Description = item.Description, IsActive = item.IsActive },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<AdminListItem> UpdateAsync(AdminListItem item)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<AdminListItem>(
            "usp_AdminListItems_Update",
            new { ItemID = item.ItemID, ItemValue = item.ItemValue, Description = item.Description, IsActive = item.IsActive },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<AdminListItem?> GetItemAsync(string listType, string itemValue)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<AdminListItem>(
            "usp_AdminListItems_GetItem",
            new { ListType = listType, ItemValue = itemValue },
            commandType: CommandType.StoredProcedure);
    }
}
