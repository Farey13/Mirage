using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class MachineBreakdownService : IMachineBreakdownService
{
    private readonly IMachineBreakdownRepository _machineBreakdownRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<MachineBreakdownService> _logger;

    public MachineBreakdownService(
        IMachineBreakdownRepository machineBreakdownRepository,
        IAuditLogService auditLogService,
        ILogger<MachineBreakdownService> logger)
    {
        _machineBreakdownRepository = machineBreakdownRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown)
    {
        _logger.LogInformation("Creating machine breakdown for machine: {MachineName}", breakdown.MachineName);
        var newBreakdown = await _machineBreakdownRepository.CreateAsync(breakdown);

        await _auditLogService.LogAsync(
            userId: newBreakdown.ReportedByUserID,
            actionType: "Create",
            moduleName: "MachineBreakdown",
            recordId: newBreakdown.BreakdownID.ToString(),
            newValue: $"Machine: {newBreakdown.MachineName}, Reason: {newBreakdown.BreakdownReason}"
        );

        _logger.LogInformation("Machine breakdown created with ID: {BreakdownId}", newBreakdown.BreakdownID);
        return newBreakdown;
    }

    public async Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching pending machine breakdowns from {StartDate} to {EndDate}", startDate, endDate);
        return await _machineBreakdownRepository.GetPendingByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> MarkAsResolvedAsync(int breakdownId, int userId, string resolutionNotes)
    {
        _logger.LogInformation("Marking machine breakdown {BreakdownId} as resolved by user {UserId}", breakdownId, userId);
        
        var breakdown = await _machineBreakdownRepository.GetByIdAsync(breakdownId);
        if (breakdown is null)
        {
            _logger.LogWarning("Machine breakdown not found: {BreakdownId}", breakdownId);
            return false;
        }

        if (breakdown.IsResolved)
        {
            _logger.LogInformation("Machine breakdown already resolved: {BreakdownId}", breakdownId);
            return true;
        }

        var success = await _machineBreakdownRepository.MarkAsResolvedAsync(breakdownId, userId, resolutionNotes);
        if (success)
        {
            await _auditLogService.LogAsync(
                userId: userId,
                actionType: "Resolve",
                moduleName: "MachineBreakdown",
                recordId: breakdownId.ToString(),
                newValue: resolutionNotes
            );
            _logger.LogInformation("Machine breakdown {BreakdownId} marked as resolved", breakdownId);
        }
        return success;
    }

    public async Task<IEnumerable<MachineBreakdown>> GetResolvedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching resolved machine breakdowns from {StartDate} to {EndDate}", startDate, endDate);
        return await _machineBreakdownRepository.GetResolvedByDateRangeAsync(startDate, endDate);
    }

    public async Task<MachineBreakdown?> GetByIdAsync(int breakdownId)
    {
        return await _machineBreakdownRepository.GetByIdAsync(breakdownId);
    }

    public async Task<bool> DeactivateAsync(int breakdownId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating machine breakdown {BreakdownId} by user {UserId}", breakdownId, userId);
        var success = await _machineBreakdownRepository.DeactivateAsync(breakdownId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "MachineBreakdown", breakdownId.ToString(), newValue: reason);
            _logger.LogInformation("Machine breakdown {BreakdownId} deactivated successfully", breakdownId);
        }
        return success;
    }
}