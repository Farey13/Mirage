using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IAuditLogRepository
{
    // We specify the full name for the async Task here
    System.Threading.Tasks.Task CreateAsync(AuditLog logEntry);
}