using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PortalMirage.Business;

public class KitValidationService(
    IKitValidationRepository kitValidationRepository,
    IAuditLogService auditLogService) : IKitValidationService
{
    public async Task<KitValidation> CreateAsync(KitValidation kitValidation)
    {
        // In the future, any business rules (like checking for duplicate lot numbers) would go here.
        var newValidation = await kitValidationRepository.CreateAsync(kitValidation);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newValidation.ValidatedByUserID,
            actionType: "Create",
            moduleName: "KitValidation",
            recordId: newValidation.ValidationID.ToString(),
            newValue: $"Kit: {newValidation.KitName}, Lot: {newValidation.KitLotNumber}, Status: {newValidation.ValidationStatus}"
        );

        return newValidation;
    }

    public async Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await kitValidationRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int validationId, int userId, string reason)
    {
        var success = await kitValidationRepository.DeactivateAsync(validationId, userId, reason);
        if (success)
        {
            await auditLogService.LogAsync(userId, "Deactivate", "KitValidation", validationId.ToString(), newValue: reason);
        }
        return success;
    }
}