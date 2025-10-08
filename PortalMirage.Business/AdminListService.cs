using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class AdminListService(
    IAdminListRepository adminListRepository,
    IAuditLogService auditLogService) : IAdminListService
{
    public async Task<IEnumerable<AdminListItem>> GetAllAsync()
    {
        return await adminListRepository.GetAllAsync();
    }

    public async Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType)
    {
        return await adminListRepository.GetByTypeAsync(listType);
    }

    public async Task<AdminListItem> CreateAsync(AdminListItem item, int actorUserId)
    {
        var createdItem = await adminListRepository.CreateAsync(item);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: actorUserId,
            actionType: "Create",
            moduleName: "AdminList",
            recordId: createdItem.ItemID.ToString(),
            newValue: $"List '{createdItem.ListType}' - Added new item '{createdItem.ItemValue}'"
        );

        return createdItem;
    }

    public async Task<AdminListItem> UpdateAsync(AdminListItem item, int actorUserId)
    {
        var updatedItem = await adminListRepository.UpdateAsync(item);

        // ADDED: Log the update event
        await auditLogService.LogAsync(
            userId: actorUserId,
            actionType: "Update",
            moduleName: "AdminList",
            recordId: updatedItem.ItemID.ToString(),
            newValue: $"List '{updatedItem.ListType}' - Updated item '{updatedItem.ItemValue}' (IsActive: {updatedItem.IsActive})"
        );

        return updatedItem;
    }
}