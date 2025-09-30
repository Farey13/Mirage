using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    // Your existing method
    public async System.Threading.Tasks.Task LogAsync(int? userId, string actionType, string moduleName, string? recordId = null, string? fieldName = null, string? oldValue = null, string? newValue = null)
    {
        var logEntry = new AuditLog
        {
            UserID = userId,
            ActionType = actionType,
            ModuleName = moduleName,
            RecordID = recordId,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue
        };
        await auditLogRepository.CreateAsync(logEntry);
    }

    // The new method we are adding
    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await auditLogRepository.GetByDateRangeAsync(startDate, endDate);
    }
}