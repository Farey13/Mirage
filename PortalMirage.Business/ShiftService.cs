using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ShiftService(IShiftRepository shiftRepository, IAuditLogService auditLogService) : IShiftService
{
    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        return await shiftRepository.GetAllAsync();
    }

    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        return await shiftRepository.GetByIdAsync(shiftId);
    }

    public async Task<Shift> CreateAsync(Shift shift, int actorUserId)
    {
        // In the future, we could add business logic here,
        // like preventing overlapping shift times.
        var createdShift = await shiftRepository.CreateAsync(shift);
        await auditLogService.LogAsync(actorUserId, "Create", "ShiftManagement", createdShift.ShiftID.ToString(), newValue: $"Created shift '{createdShift.ShiftName}'");
        return createdShift;
    }

    public async Task<Shift> UpdateAsync(Shift shift, int actorUserId)
    {
        var updatedShift = await shiftRepository.UpdateAsync(shift);
        await auditLogService.LogAsync(actorUserId, "Update", "ShiftManagement", updatedShift.ShiftID.ToString(), newValue: $"Updated shift '{updatedShift.ShiftName}' (IsActive: {updatedShift.IsActive})");
        return updatedShift;
    }

    public async System.Threading.Tasks.Task DeactivateAsync(int shiftId, int actorUserId)
    {
        var shift = await shiftRepository.GetByIdAsync(shiftId);
        await shiftRepository.DeactivateAsync(shiftId);
        await auditLogService.LogAsync(actorUserId, "Deactivate", "ShiftManagement", shiftId.ToString(), newValue: $"Deactivated shift '{shift?.ShiftName}'");
    }
}