using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(
        IShiftRepository shiftRepository, 
        IAuditLogService auditLogService,
        ILogger<ShiftService> logger)
    {
        _shiftRepository = shiftRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all shifts");
        return await _shiftRepository.GetAllAsync();
    }

    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        return await _shiftRepository.GetByIdAsync(shiftId);
    }

    public async Task<Shift> CreateAsync(Shift shift, int actorUserId)
    {
        _logger.LogInformation("Creating shift: {ShiftName} by user {UserId}", shift.ShiftName, actorUserId);
        var createdShift = await _shiftRepository.CreateAsync(shift);
        await _auditLogService.LogAsync(actorUserId, "Create", "ShiftManagement", createdShift.ShiftID.ToString(), newValue: $"Created shift '{createdShift.ShiftName}'");
        _logger.LogInformation("Shift created with ID: {ShiftId}", createdShift.ShiftID);
        return createdShift;
    }

    public async Task<Shift> UpdateAsync(Shift shift, int actorUserId)
    {
        _logger.LogInformation("Updating shift {ShiftId} by user {UserId}", shift.ShiftID, actorUserId);
        var updatedShift = await _shiftRepository.UpdateAsync(shift);
        await _auditLogService.LogAsync(actorUserId, "Update", "ShiftManagement", updatedShift.ShiftID.ToString(), newValue: $"Updated shift '{updatedShift.ShiftName}' (IsActive: {updatedShift.IsActive})");
        _logger.LogInformation("Shift {ShiftId} updated successfully", shift.ShiftID);
        return updatedShift;
    }

    public async System.Threading.Tasks.Task DeactivateAsync(int shiftId, int actorUserId)
    {
        _logger.LogInformation("Deactivating shift {ShiftId} by user {UserId}", shiftId, actorUserId);
        var shift = await _shiftRepository.GetByIdAsync(shiftId);
        await _shiftRepository.DeactivateAsync(shiftId);
        await _auditLogService.LogAsync(actorUserId, "Deactivate", "ShiftManagement", shiftId.ToString(), newValue: $"Deactivated shift '{shift?.ShiftName}'");
        _logger.LogInformation("Shift {ShiftId} deactivated successfully", shiftId);
    }
}