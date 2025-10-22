using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data.Abstractions;

public interface IAdminListRepository
{
    Task<IEnumerable<AdminListItem>> GetAllAsync();
    Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType);
    Task<AdminListItem> CreateAsync(AdminListItem item);
    Task<AdminListItem> UpdateAsync(AdminListItem item);

    Task<AdminListItem?> GetItemAsync(string listType, string itemValue);
}