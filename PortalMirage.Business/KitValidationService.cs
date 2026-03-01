using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace PortalMirage.Business;

public class KitValidationService : IKitValidationService
{
    private readonly IKitValidationRepository _kitValidationRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<KitValidationService> _logger;

    public KitValidationService(
        IKitValidationRepository kitValidationRepository,
        IAuditLogService auditLogService,
        ILogger<KitValidationService> logger)
    {
        _kitValidationRepository = kitValidationRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<KitValidation> CreateAsync(KitValidation kitValidation)
    {
        _logger.LogInformation("Creating kit validation for kit: {KitName}", kitValidation.KitName);
        var newValidation = await _kitValidationRepository.CreateAsync(kitValidation);

        await _auditLogService.LogAsync(
            userId: newValidation.ValidatedByUserID,
            actionType: "Create",
            moduleName: "KitValidation",
            recordId: newValidation.ValidationID.ToString(),
            newValue: $"Kit: {newValidation.KitName}, Lot: {newValidation.KitLotNumber}, Status: {newValidation.ValidationStatus}"
        );

        _logger.LogInformation("Kit validation created with ID: {ValidationId}", newValidation.ValidationID);
        return newValidation;
    }

    public async Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching kit validations from {StartDate} to {EndDate}", startDate, endDate);
        return await _kitValidationRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int validationId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating kit validation {ValidationId} by user {UserId}", validationId, userId);
        var success = await _kitValidationRepository.DeactivateAsync(validationId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "KitValidation", validationId.ToString(), newValue: reason);
            _logger.LogInformation("Kit validation {ValidationId} deactivated successfully", validationId);
        }
        return success;
    }
}