using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class AdminListService(IAdminListRepository adminListRepository) : IAdminListService
{
    public async Task<IEnumerable<AdminListItem>> GetAllAsync()
    {
        return await adminListRepository.GetAllAsync();
    }

    public async Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType)
    {
        return await adminListRepository.GetByTypeAsync(listType);
    }

    public async Task<AdminListItem> CreateAsync(AdminListItem item)
    {
        // We could add business logic here later, like preventing duplicate values.
        return await adminListRepository.CreateAsync(item);
    }

    public async Task<AdminListItem> UpdateAsync(AdminListItem item)
    {
        return await adminListRepository.UpdateAsync(item);
    }
}