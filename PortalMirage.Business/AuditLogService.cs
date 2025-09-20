using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    public async Task LogAsync(int? userId, string actionType, string moduleName, string? recordId = null, string? fieldName = null, string? oldValue = null, string? newValue = null)
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
            // Timestamp is set by the database default
        };

        await auditLogRepository.CreateAsync(logEntry);
    }
}