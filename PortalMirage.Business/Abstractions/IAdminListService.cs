using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IAdminListService
{
    Task<IEnumerable<AdminListItem>> GetAllAsync();
    Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType);

    // Updated methods with actorUserId parameter for audit logging
    Task<AdminListItem> CreateAsync(AdminListItem item, int actorUserId);
    Task<AdminListItem> UpdateAsync(AdminListItem item, int actorUserId);

    Task<AdminListItem?> GetItemAsync(string listType, string itemValue);
}