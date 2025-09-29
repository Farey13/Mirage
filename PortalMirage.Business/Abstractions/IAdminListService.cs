using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IAdminListService
{
    Task<IEnumerable<AdminListItem>> GetAllAsync();
    Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType);
    Task<AdminListItem> CreateAsync(AdminListItem item);
    Task<AdminListItem> UpdateAsync(AdminListItem item);
}