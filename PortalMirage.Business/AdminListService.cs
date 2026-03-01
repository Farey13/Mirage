using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class AdminListService : IAdminListService
{
    private readonly IAdminListRepository _adminListRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AdminListService> _logger;

    public AdminListService(
        IAdminListRepository adminListRepository,
        IAuditLogService auditLogService,
        ILogger<AdminListService> logger)
    {
        _adminListRepository = adminListRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<IEnumerable<AdminListItem>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all admin list items");
        return await _adminListRepository.GetAllAsync();
    }

    public async Task<IEnumerable<AdminListItem>> GetByTypeAsync(string listType)
    {
        _logger.LogDebug("Fetching admin list items for type: {ListType}", listType);
        return await _adminListRepository.GetByTypeAsync(listType);
    }

    public async Task<AdminListItem> CreateAsync(AdminListItem item, int actorUserId)
    {
        _logger.LogInformation("Creating admin list item: {ItemValue} of type {ListType} by user {UserId}", 
            item.ItemValue, item.ListType, actorUserId);
        
        var createdItem = await _adminListRepository.CreateAsync(item);

        await _auditLogService.LogAsync(
            userId: actorUserId,
            actionType: "Create",
            moduleName: "AdminList",
            recordId: createdItem.ItemID.ToString(),
            newValue: $"List '{createdItem.ListType}' - Added new item '{createdItem.ItemValue}'"
        );

        _logger.LogInformation("Admin list item created with ID: {ItemId}", createdItem.ItemID);
        return createdItem;
    }

    public async Task<AdminListItem?> GetItemAsync(string listType, string itemValue)
    {
        return await _adminListRepository.GetItemAsync(listType, itemValue);
    }

    public async Task<AdminListItem> UpdateAsync(AdminListItem item, int actorUserId)
    {
        _logger.LogInformation("Updating admin list item {ItemId} by user {UserId}", item.ItemID, actorUserId);
        
        var updatedItem = await _adminListRepository.UpdateAsync(item);

        await _auditLogService.LogAsync(
            userId: actorUserId,
            actionType: "Update",
            moduleName: "AdminList",
            recordId: updatedItem.ItemID.ToString(),
            newValue: $"List '{updatedItem.ListType}' - Updated item '{updatedItem.ItemValue}' (IsActive: {updatedItem.IsActive})"
        );

        _logger.LogInformation("Admin list item {ItemId} updated successfully", item.ItemID);
        return updatedItem;
    }
}