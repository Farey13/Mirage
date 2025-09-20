using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business.Abstractions;

public interface IAuditLogService
{
    Task LogAsync(int? userId, string actionType, string moduleName, string? recordId = null, string? fieldName = null, string? oldValue = null, string? newValue = null);
}